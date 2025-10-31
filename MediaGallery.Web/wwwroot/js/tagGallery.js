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

function parseJson(id) {
    const script = document.getElementById(id);
    if (!script) {
        return null;
    }

    try {
        return JSON.parse(script.textContent);
    } catch (error) {
        console.error(`Failed to parse JSON for ${id}`, error);
        return null;
    }
}

function formatScore(score) {
    if (typeof score !== 'number') {
        return null;
    }
    return `Score ${(score * 100).toFixed(1)}%`;
}

function createMediaCard(item, mediaRoot, options = {}) {
    const card = document.createElement('article');
    card.className = 'media-card';
    card.tabIndex = 0;
    card.dataset.type = item.type;

    const header = document.createElement('header');
    header.className = 'media-card__header';
    header.innerHTML = `<strong>${options.title ?? item.title ?? 'Media item'}</strong>`;
    card.appendChild(header);

    const figure = document.createElement('figure');
    figure.className = 'media-card__figure';

    if (item.type === 'photo') {
        const src = combineMediaUrl(mediaRoot, item.filePath);
        if (src) {
            const wrapper = document.createElement('div');
            wrapper.dataset.lgItem = '';
            const anchor = document.createElement('a');
            anchor.href = src;
            anchor.dataset.subHtml = `<p>${options.title ?? item.title ?? ''}</p>`;
            if (item.width && item.height) {
                anchor.dataset.lgSize = `${item.width}-${item.height}`;
            }

            const img = document.createElement('img');
            img.src = src;
            img.alt = options.title ?? item.title ?? 'Gallery item';
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
    const meta = Array.isArray(options.meta) ? options.meta.filter(Boolean) : [];
    if (meta.length) {
        caption.innerHTML = meta.join(' \u2022 ');
    }
    figure.appendChild(caption);

    card.appendChild(figure);
    return card;
}

function attachKeyboardNavigation(container, target) {
    if (!container || !target) {
        return;
    }

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

function initTagIndex() {
    const container = document.querySelector('.tag-browser[data-tag-view="index"]');
    if (!container) {
        return false;
    }

    const mediaRoot = getMediaRoot(container);
    const previewGallery = container.querySelector('[data-preview-gallery]');
    const previewTitle = container.querySelector('[data-preview-title]');
    const statusElement = container.querySelector('[data-scroll-status]');
    const data = parseJson('tag-index-data');
    if (!previewGallery || !Array.isArray(data)) {
        return false;
    }

    const tagMap = new Map();
    data.forEach((tag) => tagMap.set(tag.tag, tag));
    const cards = Array.from(container.querySelectorAll('.tag-card'));

    function updateStatus(message) {
        if (statusElement) {
            statusElement.textContent = message;
        }
    }

    function renderPreview(tagName) {
        const info = tagMap.get(tagName);
        destroyPlayers(previewGallery);
        destroyGallery(previewGallery);
        previewGallery.innerHTML = '';

        cards.forEach((card) => card.classList.toggle('is-active', card.dataset.tag === tagName));

        if (!info) {
            updateStatus('Select a tag to preview media.');
            if (previewTitle) {
                previewTitle.textContent = 'Preview';
            }
            return;
        }

        if (previewTitle) {
            previewTitle.textContent = `Preview: ${info.tag}`;
        }

        if (!info.highlight || info.highlight.isAccessible === false) {
            updateStatus('No accessible preview media for this tag.');
            return;
        }

        const meta = [formatScore(info.topScore), `${info.total} items available`];
        const card = createMediaCard(
            { ...info.highlight, title: info.tag },
            mediaRoot,
            { title: info.tag, meta }
        );
        previewGallery.appendChild(card);
        ensureGallery(previewGallery);
        previewGallery.querySelectorAll('video').forEach(initPlyr);
        updateStatus('Preview ready. Use keyboard arrows to focus media.');
    }

    container.querySelectorAll('[data-action="preview-tag"]').forEach((button) => {
        button.addEventListener('click', () => {
            const tag = button.dataset.tag;
            renderPreview(tag);
        });
    });

    if (data.length) {
        renderPreview(data[0].tag);
    }

    attachKeyboardNavigation(container, previewGallery);
    return true;
}

function initTagDetail() {
    const container = document.querySelector('.tag-detail[data-tag-view="detail"]');
    if (!container) {
        return false;
    }

    const galleryElement = container.querySelector('[data-gallery]');
    const statusElement = container.querySelector('[data-scroll-status]');
    const mediaRoot = getMediaRoot(container);
    const detail = parseJson('tag-detail-data');
    if (!galleryElement || !detail) {
        return false;
    }

    const accessibleMedia = (detail.media || []).filter((item) => item.isAccessible !== false);
    accessibleMedia.sort((a, b) => (b.score ?? 0) - (a.score ?? 0));

    destroyPlayers(galleryElement);
    destroyGallery(galleryElement);
    galleryElement.innerHTML = '';

    if (!accessibleMedia.length) {
        if (statusElement) {
            statusElement.textContent = 'No media available for this tag.';
        }
        return true;
    }

    const fragment = document.createDocumentFragment();
    accessibleMedia.forEach((item) => {
        const meta = [formatScore(item.score)];
        fragment.appendChild(createMediaCard(item, mediaRoot, { meta }));
    });
    galleryElement.appendChild(fragment);
    ensureGallery(galleryElement);
    galleryElement.querySelectorAll('video').forEach(initPlyr);
    if (statusElement) {
        statusElement.textContent = `${accessibleMedia.length} media item${accessibleMedia.length === 1 ? '' : 's'} loaded.`;
    }

    attachKeyboardNavigation(container, galleryElement);
    return true;
}

document.addEventListener('DOMContentLoaded', () => {
    const initializedIndex = initTagIndex();
    const initializedDetail = initTagDetail();
    if (!initializedIndex && !initializedDetail) {
        // Nothing to initialize on this page.
    }
});
