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
    const normalizedPath = filePath.replace(/^\/+/, '');
    return `${normalizedRoot}/${normalizedPath}`;
}

function getMediaRoot(element) {
    return element?.dataset.mediaRoot || document.body.dataset.mediaRoot || '';
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

function parseProfileData() {
    const script = document.getElementById('user-profile-data');
    if (!script) {
        return null;
    }

    try {
        return JSON.parse(script.textContent);
    } catch (error) {
        console.error('Failed to parse user profile data', error);
        return null;
    }
}

function createCard(item, mediaRoot) {
    const card = document.createElement('article');
    card.className = 'media-card';
    card.tabIndex = 0;
    card.dataset.type = item.type;

    const figure = document.createElement('figure');
    figure.className = 'media-card__figure';

    const header = document.createElement('header');
    header.className = 'media-card__header';
    header.innerHTML = `<strong>${item.title ?? 'Untitled item'}</strong>`;

    if (item.type === 'photo') {
        const src = combineMediaUrl(mediaRoot, item.filePath);
        if (src) {
            const wrapper = document.createElement('div');
            wrapper.dataset.lgItem = '';
            const anchor = document.createElement('a');
            anchor.href = src;
            anchor.dataset.subHtml = `<p>${item.title ?? ''}</p>`;
            if (item.width && item.height) {
                anchor.dataset.lgSize = `${item.width}-${item.height}`;
            }

            const img = document.createElement('img');
            img.src = src;
            img.alt = item.title ?? 'Gallery photo';
            img.loading = 'lazy';

            anchor.appendChild(img);
            wrapper.appendChild(anchor);
            figure.appendChild(wrapper);
        }
    } else if (item.type === 'video') {
        const src = combineMediaUrl(mediaRoot, item.filePath);
        if (src) {
            const wrapper = document.createElement('div');
            wrapper.className = 'video-wrapper';

            const video = document.createElement('video');
            video.className = 'media-card__video js-plyr';
            video.controls = true;
            video.setAttribute('playsinline', '');

            const source = document.createElement('source');
            source.src = src;
            source.type = item.mimeType ?? 'video/mp4';
            video.appendChild(source);
            wrapper.appendChild(video);
            figure.appendChild(wrapper);
        }
    }

    const caption = document.createElement('figcaption');
    caption.className = 'media-card__caption';

    const addedOn = item.addedOn ? new Date(item.addedOn) : null;
    const tags = Array.isArray(item.tags) ? item.tags.join(', ') : '';
    caption.innerHTML = [
        addedOn ? `<span class="meta">Added ${addedOn.toLocaleDateString()}</span>` : '',
        typeof item.views === 'number' ? `<span class="meta">${item.views.toLocaleString()} views</span>` : '',
        tags ? `<span class="meta">Tags: ${tags}</span>` : ''
    ].filter(Boolean).join(' \u2022 ');

    figure.appendChild(caption);
    card.appendChild(header);
    card.appendChild(figure);

    return card;
}

function attachKeyboardNavigation(container, target) {
    container.addEventListener('keydown', (event) => {
        if (!['ArrowRight', 'ArrowLeft', 'Home', 'End'].includes(event.key)) {
            return;
        }

        const cards = Array.from(target.querySelectorAll('.media-card'));
        if (!cards.length) {
            return;
        }

        const active = document.activeElement;
        let index = cards.indexOf(active);

        switch (event.key) {
            case 'ArrowRight':
                index = Math.min(cards.length - 1, index + 1);
                break;
            case 'ArrowLeft':
                index = Math.max(0, index - 1);
                break;
            case 'Home':
                index = 0;
                break;
            case 'End':
                index = cards.length - 1;
                break;
            default:
                break;
        }

        if (index >= 0) {
            event.preventDefault();
            cards[index].focus();
        }
    });
}

function initUserProfile() {
    const container = document.querySelector('.user-profile');
    if (!container) {
        return;
    }

    const mediaRoot = getMediaRoot(container);
    const galleryElement = container.querySelector('[data-gallery]');
    const statusElement = container.querySelector('[data-scroll-status]');
    const sortButtons = container.querySelectorAll('[data-sort]');
    const tagButtons = container.querySelectorAll('[data-filter-tag]');

    const profile = parseProfileData();
    if (!profile) {
        return;
    }

    const state = {
        sort: 'recent',
        activeTags: new Set()
    };

    function applyFilters(media) {
        let filtered = media.filter((item) => item.isAccessible !== false);
        if (state.activeTags.size) {
            filtered = filtered.filter((item) => {
                const tags = Array.isArray(item.tags) ? item.tags : [];
                return tags.some((tag) => state.activeTags.has(tag));
            });
        }
        return filtered;
    }

    function applySort(media) {
        const items = [...media];
        if (state.sort === 'popular') {
            items.sort((a, b) => (b.views ?? 0) - (a.views ?? 0));
        } else {
            items.sort((a, b) => {
                const left = a.addedOn ? new Date(a.addedOn).getTime() : 0;
                const right = b.addedOn ? new Date(b.addedOn).getTime() : 0;
                return right - left;
            });
        }
        return items;
    }

    function updateStatus(message) {
        if (statusElement) {
            statusElement.textContent = message;
        }
    }

    function render() {
        if (!galleryElement) {
            return;
        }

        destroyPlayers(galleryElement);
        destroyGallery(galleryElement);
        galleryElement.innerHTML = '';

        const filtered = applySort(applyFilters(profile.media || []));
        if (!filtered.length) {
            updateStatus('No media matches the current filters.');
            return;
        }

        const fragment = document.createDocumentFragment();
        filtered.forEach((item) => {
            fragment.appendChild(createCard(item, mediaRoot));
        });
        galleryElement.appendChild(fragment);
        ensureGallery(galleryElement);
        galleryElement.querySelectorAll('video').forEach(initPlyr);
        updateStatus(`${filtered.length} media item${filtered.length === 1 ? '' : 's'} available.`);
    }

    render();

    sortButtons.forEach((button) => {
        button.addEventListener('click', () => {
            const sort = button.dataset.sort;
            if (!sort) {
                return;
            }
            state.sort = sort;
            sortButtons.forEach((btn) => btn.classList.toggle('is-active', btn === button));
            render();
        });
    });

    tagButtons.forEach((button) => {
        button.addEventListener('click', () => {
            const tag = button.dataset.filterTag;
            if (!tag) {
                return;
            }
            if (state.activeTags.has(tag)) {
                state.activeTags.delete(tag);
                button.classList.remove('is-active');
                button.setAttribute('aria-pressed', 'false');
            } else {
                state.activeTags.add(tag);
                button.classList.add('is-active');
                button.setAttribute('aria-pressed', 'true');
            }
            render();
        });
    });

    attachKeyboardNavigation(container, galleryElement);
}

document.addEventListener('DOMContentLoaded', initUserProfile);
