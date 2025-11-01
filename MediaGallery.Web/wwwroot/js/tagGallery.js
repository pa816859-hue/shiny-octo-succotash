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

async function fetchPreview(endpoint, tag, pageSize) {
    if (!endpoint || !tag) {
        return null;
    }

    const url = new URL(endpoint, window.location.origin);
    url.searchParams.set('tag', tag);
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
    const mediaRoot = container.dataset.mediaRoot || document.body.dataset.mediaRoot || '';
    const endpoint = container.dataset.detailEndpoint;
    const pageSize = Number(container.dataset.previewPageSize || '6');
    const initialTag = container.dataset.initialTag;

    if (!previewGallery) {
        return;
    }

    function setStatus(message) {
        if (statusElement) {
            statusElement.textContent = message;
        }
    }

    async function renderPreview(tag) {
        if (!tag) {
            setStatus('Select a tag to load preview media.');
            return;
        }

        setStatus('Loading preview…');
        destroyPlayers(previewGallery);
        destroyGallery(previewGallery);
        previewGallery.innerHTML = '';

        container.querySelectorAll('.tag-card').forEach((card) => {
            card.classList.toggle('is-active', card.dataset.tag === tag);
        });

        try {
            const payload = await fetchPreview(endpoint, tag, pageSize);
            const items = payload?.photos?.items || [];
            if (!items.length) {
                setStatus('No preview media available for this tag.');
                if (previewTitle) {
                    previewTitle.textContent = `Preview · ${tag}`;
                }
                return;
            }

            if (previewTitle) {
                previewTitle.textContent = `Preview · ${tag}`;
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
            setStatus('Unable to load preview for this tag.');
        }
    }

    container.querySelectorAll('[data-action="preview-tag"]').forEach((button) => {
        button.addEventListener('click', () => {
            const tag = button.dataset.tag;
            renderPreview(tag);
        });
    });

    if (initialTag) {
        renderPreview(initialTag);
    }
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
