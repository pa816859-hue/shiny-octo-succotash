const LIGHTGALLERY_PLUGINS = [
    window.lgThumbnail,
    window.lgZoom,
    window.lgFullscreen,
    window.lgAutoplay
].filter(Boolean);

const galleryInstances = new WeakMap();
const plyrPlayers = new WeakMap();

function combineMediaUrl(root, filePath) {
    if (!filePath) {
        return null;
    }

    const normalizedRoot = (root || '').replace(/\/+$/, '');
    const normalizedPath = filePath.replace(/^\/+/, '').replace(/\\/g, '/');
    return `${normalizedRoot}/${normalizedPath}`;
}

function ensureGallery(element) {
    if (!element || !window.lightGallery) {
        return null;
    }

    const existing = galleryInstances.get(element);
    if (existing) {
        existing.refresh();
        return existing;
    }

    const instance = window.lightGallery(element, {
        selector: '.media-card [data-lg-item] > a',
        speed: 300,
        download: false,
        plugins: LIGHTGALLERY_PLUGINS
    });

    galleryInstances.set(element, instance);
    return instance;
}

function destroyGallery(element) {
    const existing = galleryInstances.get(element);
    if (existing && typeof existing.destroy === 'function') {
        existing.destroy(true);
        galleryInstances.delete(element);
    }
}

function initPlyr(video) {
    if (!window.Plyr || !video) {
        return null;
    }

    if (plyrPlayers.has(video)) {
        return plyrPlayers.get(video);
    }

    const player = new window.Plyr(video, {
        controls: ['play', 'progress', 'current-time', 'mute', 'volume', 'captions', 'fullscreen'],
        autopause: true
    });

    plyrPlayers.set(video, player);
    return player;
}

function destroyPlayers(root) {
    root.querySelectorAll('video').forEach((video) => {
        const player = plyrPlayers.get(video);
        if (player && typeof player.destroy === 'function') {
            player.destroy();
            plyrPlayers.delete(video);
        }
    });
}

function normalizeTag(tag) {
    return (tag || '').trim().toLowerCase();
}

function buildQueryLabel(includes, excludes) {
    const includeText = (includes || [])
        .filter((tag) => tag)
        .map((tag) => `+${tag}`)
        .join(', ');
    const excludeText = (excludes || [])
        .filter((tag) => tag)
        .map((tag) => `-${tag}`)
        .join(', ');

    if (includeText && excludeText) {
        return `${includeText} ${excludeText}`;
    }

    if (includeText) {
        return includeText;
    }

    if (excludeText) {
        return excludeText;
    }

    return 'Preview';
}

function renderChipList(container, tags, type) {
    if (!container) {
        return;
    }

    container.innerHTML = '';
    (tags || []).forEach((tag) => {
        if (!tag) {
            return;
        }

        const chip = document.createElement('button');
        chip.type = 'button';
        chip.className = `tag-chip tag-chip--${type}`;
        chip.dataset.tag = tag;
        chip.dataset.action = `remove-${type}`;
        const symbol = document.createElement('span');
        symbol.className = 'tag-chip__symbol';
        symbol.textContent = type === 'include' ? '+' : '−';
        const label = document.createElement('span');
        label.className = 'tag-chip__label';
        label.textContent = tag;
        chip.appendChild(symbol);
        chip.appendChild(label);
        container.appendChild(chip);
    });
}

function createPreviewCard(item, mediaRoot) {
    const card = document.createElement('article');
    card.className = 'media-card';
    card.tabIndex = 0;
    card.dataset.hasMedia = 'true';

    const primaryTag = Array.isArray(item.tags) && item.tags.length ? item.tags[0].tag : null;

    const header = document.createElement('header');
    header.className = 'media-card__header';
    header.innerHTML = `<p class="media-card__title">${item.displayName || 'Unknown user'}</p>`;
    if (item.addedOn) {
        const meta = document.createElement('span');
        meta.className = 'media-card__meta';
        meta.textContent = new Date(item.addedOn).toLocaleString();
        header.appendChild(meta);
    }
    card.appendChild(header);

    const body = document.createElement('div');
    body.className = 'media-card__body';

    if (item.filePath) {
        const mediaWrapper = document.createElement('div');
        mediaWrapper.className = 'media-card__media';
        mediaWrapper.dataset.lgItem = '';
        const anchor = document.createElement('a');
        const src = combineMediaUrl(mediaRoot, item.filePath);
        anchor.href = src;
        anchor.dataset.subHtml = `<p>${item.messageText || ''}</p>`;
        const image = document.createElement('img');
        image.src = src;
        image.alt = `Preview for ${primaryTag || 'tag'}`;
        anchor.appendChild(image);
        mediaWrapper.appendChild(anchor);
        body.appendChild(mediaWrapper);
    }

    if (item.messageText) {
        const text = document.createElement('p');
        text.className = 'media-card__text';
        text.textContent = item.messageText;
        body.appendChild(text);
    }

    if (Array.isArray(item.tags) && item.tags.length) {
        const tags = document.createElement('div');
        tags.className = 'media-card__tags';
        item.tags.forEach((tag) => {
            const chip = document.createElement('span');
            chip.textContent = tag.tag ? `${tag.tag} · ${(tag.score * 100).toFixed(1)}%` : tag;
            tags.appendChild(chip);
        });
        body.appendChild(tags);
    }

    card.appendChild(body);
    return card;
}

async function fetchQuery(endpoint, includeTags, excludeTags, pageSize) {
    if (!endpoint) {
        return null;
    }

    const url = new URL(endpoint, window.location.origin);
    (includeTags || []).forEach((tag) => {
        if (tag) {
            url.searchParams.append('include', tag);
        }
    });

    (excludeTags || []).forEach((tag) => {
        if (tag) {
            url.searchParams.append('exclude', tag);
        }
    });

    if (pageSize) {
        url.searchParams.set('pageSize', String(pageSize));
    }

    const response = await fetch(url.toString(), { headers: { 'Accept': 'application/json' } });
    if (!response.ok) {
        throw new Error(`Request failed with status ${response.status}`);
    }

    return response.json();
}

function initTagIndex() {
    const container = document.querySelector('.tag-browser[data-tag-view="index"]');
    if (!container) {
        return;
    }

    const previewGallery = container.querySelector('[data-preview-gallery]');
    const previewTitle = container.querySelector('[data-preview-title]');
    const statusElement = container.querySelector('[data-scroll-status]');
    const includeList = container.querySelector('[data-include-list]');
    const excludeList = container.querySelector('[data-exclude-list]');
    const searchInput = container.querySelector('[data-tag-search]');
    const tagCount = container.querySelector('[data-tag-count]');
    const mediaRoot = container.dataset.mediaRoot || document.body.dataset.mediaRoot || '';
    const queryEndpoint = container.dataset.queryEndpoint;
    const pageSize = Number(container.dataset.previewPageSize || '6');

    if (!previewGallery) {
        return;
    }

    function setStatus(message) {
        if (statusElement) {
            statusElement.textContent = message;
        }
    }

    const queryState = {
        include: [],
        exclude: []
    };

    let currentPreview = {
        include: [],
        exclude: []
    };

    function getCards() {
        return Array.from(container.querySelectorAll('.tag-card'));
    }

    function updateCardIndicators() {
        const includeSet = new Set(queryState.include.map((tag) => normalizeTag(tag)));
        const excludeSet = new Set(queryState.exclude.map((tag) => normalizeTag(tag)));
        const activeIncludeSet = new Set((currentPreview.include || []).map((tag) => normalizeTag(tag)));
        const activeExcludeSet = new Set((currentPreview.exclude || []).map((tag) => normalizeTag(tag)));

        getCards().forEach((card) => {
            const tagValue = normalizeTag(card.dataset.tag);
            card.classList.toggle('is-included', includeSet.has(tagValue));
            card.classList.toggle('is-excluded', excludeSet.has(tagValue));
            const isActive = activeIncludeSet.has(tagValue) || activeExcludeSet.has(tagValue);
            card.classList.toggle('is-active', isActive);
        });
    }

    function updateTagCountDisplay() {
        if (!tagCount) {
            return;
        }

        const visibleCount = getCards().filter((card) => !card.hidden).length;
        tagCount.textContent = `${visibleCount} tag${visibleCount === 1 ? '' : 's'}`;
    }

    function applySearchFilter(term) {
        const normalizedTerm = (term || '').trim().toLowerCase();
        getCards().forEach((card) => {
            const matches = !normalizedTerm || normalizeTag(card.dataset.tag).includes(normalizedTerm);
            card.hidden = !matches;
        });

        updateTagCountDisplay();
    }

    async function runQuery(options = {}) {
        const { include, exclude, label } = options;
        const includeTags = Array.isArray(include) ? include : queryState.include;
        const excludeTags = Array.isArray(exclude) ? exclude : queryState.exclude;

        currentPreview = {
            include: [...includeTags],
            exclude: [...excludeTags]
        };

        updateCardIndicators();

        if (!includeTags.length && !excludeTags.length) {
            destroyPlayers(previewGallery);
            destroyGallery(previewGallery);
            previewGallery.innerHTML = '';
            if (previewTitle) {
                previewTitle.textContent = 'Preview';
            }
            setStatus('Add tags to the query or preview a tag to see media here.');
            return;
        }

        if (!queryEndpoint) {
            setStatus('Query endpoint is not available.');
            return;
        }

        setStatus('Loading preview…');
        destroyPlayers(previewGallery);
        destroyGallery(previewGallery);
        previewGallery.innerHTML = '';

        try {
            const payload = await fetchQuery(queryEndpoint, includeTags, excludeTags, pageSize);
            const items = payload?.photos?.items || [];
            const responseIncludes = payload?.includes || includeTags;
            const responseExcludes = payload?.excludes || excludeTags;

            if (previewTitle) {
                const titleLabel = label || buildQueryLabel(responseIncludes, responseExcludes);
                previewTitle.textContent = titleLabel;
            }

            if (!items.length) {
                setStatus('No media found for this query.');
                updateCardIndicators();
                return;
            }

            const fragment = document.createDocumentFragment();
            items.forEach((item) => {
                const card = createPreviewCard(item, mediaRoot);
                fragment.appendChild(card);
            });
            previewGallery.appendChild(fragment);
            ensureGallery(previewGallery);
            previewGallery.querySelectorAll('video').forEach(initPlyr);
            setStatus(`${items.length} preview item${items.length === 1 ? '' : 's'} loaded.`);
        } catch (error) {
            console.error('Unable to load tag preview', error);
            setStatus('Unable to load preview for this query.');
        }
    }

    function renderQueryState() {
        renderChipList(includeList, queryState.include, 'include');
        renderChipList(excludeList, queryState.exclude, 'exclude');
        updateCardIndicators();
    }

    function addTagToQuery(type, tag) {
        if (!tag) {
            return;
        }

        const normalized = normalizeTag(tag);
        if (!normalized) {
            return;
        }

        const target = queryState[type];
        if (target.some((value) => normalizeTag(value) === normalized)) {
            return;
        }

        const oppositeType = type === 'include' ? 'exclude' : 'include';
        const opposite = queryState[oppositeType].filter((value) => normalizeTag(value) !== normalized);

        queryState[type] = [...target, tag];
        queryState[oppositeType] = opposite;
        renderQueryState();
        runQuery();
    }

    function removeTagFromQuery(type, tag) {
        if (!tag) {
            return;
        }

        const normalized = normalizeTag(tag);
        const updated = queryState[type].filter((value) => normalizeTag(value) !== normalized);
        if (updated.length === queryState[type].length) {
            return;
        }

        queryState[type] = updated;
        renderQueryState();
        runQuery();
    }

    function resetQuery() {
        currentPreview = { include: [], exclude: [] };
        queryState.include = [];
        queryState.exclude = [];
        renderQueryState();
        destroyPlayers(previewGallery);
        destroyGallery(previewGallery);
        previewGallery.innerHTML = '';
        if (previewTitle) {
            previewTitle.textContent = 'Preview';
        }
        setStatus('Add tags to the query or preview a tag to see media here.');
    }

    container.addEventListener('click', (event) => {
        const target = event.target instanceof HTMLElement ? event.target.closest('[data-action]') : null;
        if (!target) {
            return;
        }

        const tag = target.dataset.tag;
        switch (target.dataset.action) {
            case 'preview-tag':
                runQuery({ include: tag ? [tag] : [], exclude: [], label: tag ? `Preview · ${tag}` : undefined });
                break;
            case 'include-tag':
                addTagToQuery('include', tag);
                break;
            case 'exclude-tag':
                addTagToQuery('exclude', tag);
                break;
            case 'run-query':
                runQuery();
                break;
            case 'reset-query':
                resetQuery();
                break;
            default:
                break;
        }
    });

    if (includeList) {
        includeList.addEventListener('click', (event) => {
            const target = event.target instanceof HTMLElement ? event.target.closest('[data-action]') : null;
            if (!target) {
                return;
            }

            if (target.dataset.action === 'remove-include') {
                removeTagFromQuery('include', target.dataset.tag);
            }
        });
    }

    if (excludeList) {
        excludeList.addEventListener('click', (event) => {
            const target = event.target instanceof HTMLElement ? event.target.closest('[data-action]') : null;
            if (!target) {
                return;
            }

            if (target.dataset.action === 'remove-exclude') {
                removeTagFromQuery('exclude', target.dataset.tag);
            }
        });
    }

    if (searchInput) {
        const handleSearch = (event) => {
            applySearchFilter(event.target.value);
        };

        searchInput.addEventListener('input', handleSearch);
        searchInput.addEventListener('search', handleSearch);
    }

    renderQueryState();
    applySearchFilter(searchInput ? searchInput.value : '');
}

function initTagDetail() {
    const container = document.querySelector('.tag-detail[data-tag-view="detail"]');
    if (!container) {
        return;
    }

    const gallery = container.querySelector('[data-gallery]');
    if (!gallery) {
        return;
    }

    ensureGallery(gallery);
}

document.addEventListener('DOMContentLoaded', () => {
    initTagIndex();
    initTagDetail();
});
