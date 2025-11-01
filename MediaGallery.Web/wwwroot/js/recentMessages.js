const LIGHTGALLERY_PLUGINS = [
    window.lgThumbnail,
    window.lgZoom,
    window.lgFullscreen,
    window.lgAutoplay
].filter(Boolean);

const galleryInstances = new WeakMap();
const plyrPlayers = new WeakMap();

const MIME_FALLBACKS = {
    photo: 'image/jpeg',
    video: 'video/mp4'
};

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

function guessMimeType(path, type) {
    if (!path) {
        return type === 'video' ? MIME_FALLBACKS.video : MIME_FALLBACKS.photo;
    }

    const extension = path.split('.').pop()?.toLowerCase();
    if (!extension) {
        return type === 'video' ? MIME_FALLBACKS.video : MIME_FALLBACKS.photo;
    }

    const map = {
        jpg: 'image/jpeg',
        jpeg: 'image/jpeg',
        png: 'image/png',
        webp: 'image/webp',
        mp4: 'video/mp4',
        webm: 'video/webm',
        mov: 'video/quicktime'
    };

    return map[extension] || (type === 'video' ? MIME_FALLBACKS.video : MIME_FALLBACKS.photo);
}

function normalizeMessage(raw) {
    if (!raw) {
        return null;
    }

    const hasPhoto = Boolean(raw.photoPath);
    const hasVideo = Boolean(raw.videoPath);
    const mediaType = hasPhoto ? 'photo' : hasVideo ? 'video' : null;
    const filePath = hasPhoto ? raw.photoPath : hasVideo ? raw.videoPath : null;

    return {
        messageId: raw.messageId,
        channelId: raw.channelId,
        userId: raw.userId,
        user: raw.displayName ?? raw.username ?? `User ${raw.userId}`,
        text: raw.messageText ?? '',
        sentDate: raw.sentDate,
        mediaType,
        filePath,
        mimeType: guessMimeType(filePath, mediaType),
        photoId: raw.photoId,
        videoId: raw.videoId,
        isAccessible: Boolean(mediaType && filePath)
    };
}

function renderCard(item, mediaRoot) {
    const card = document.createElement('article');
    card.className = 'media-card';
    card.tabIndex = 0;
    card.dataset.messageId = item.messageId;

    const header = document.createElement('header');
    header.className = 'media-card__header';
    header.innerHTML = `<strong>${item.user ?? 'Unknown user'}</strong>`;
    card.appendChild(header);

    const figure = document.createElement('figure');
    figure.className = 'media-card__figure';

    const caption = document.createElement('figcaption');
    caption.className = 'media-card__caption';
    caption.textContent = item.text ?? '';

    if (item.mediaType === 'photo') {
        const href = combineMediaUrl(mediaRoot, item.filePath);
        if (href) {
            const wrapper = document.createElement('div');
            wrapper.dataset.lgItem = '';
            const anchor = document.createElement('a');
            anchor.href = href;
            anchor.dataset.subHtml = `<p>${item.text ?? ''}</p>`;
            if (item.width && item.height) {
                anchor.dataset.lgSize = `${item.width}-${item.height}`;
            }

            const img = document.createElement('img');
            img.src = href;
            img.alt = item.text ?? `Message ${item.messageId}`;
            img.loading = 'lazy';

            anchor.appendChild(img);
            wrapper.appendChild(anchor);
            figure.appendChild(wrapper);
        }
    } else if (item.mediaType === 'video') {
        const src = combineMediaUrl(mediaRoot, item.filePath);
        if (src) {
            const wrapper = document.createElement('div');
            wrapper.className = 'video-wrapper';
            wrapper.dataset.lgItem = '';

            const video = document.createElement('video');
            video.className = 'media-card__video js-plyr';
            video.controls = true;
            video.setAttribute('playsinline', '');

            const source = document.createElement('source');
            source.src = src;
            source.type = item.mimeType ?? MIME_FALLBACKS.video;
            video.appendChild(source);
            wrapper.appendChild(video);
            figure.appendChild(wrapper);
        }
    }

    figure.appendChild(caption);
    card.appendChild(figure);

    return card;
}

function parseInitialData() {
    const script = document.getElementById('recent-messages-data');
    if (!script) {
        return [];
    }

    try {
        return JSON.parse(script.textContent) || [];
    } catch (error) {
        console.error('Failed to parse initial recent message data', error);
        return [];
    }
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

function initRecentMessages() {
    const container = document.querySelector('.recent-messages');
    if (!container) {
        return;
    }

    const galleryElement = container.querySelector('[data-gallery]');
    const statusElement = container.querySelector('[data-scroll-status]');
    const sentinel = container.querySelector('[data-scroll-sentinel]');
    const filterOnlyMedia = container.querySelector('[data-filter="onlyMedia"]');
    const layoutToggle = container.querySelector('[data-action="toggle-layout"]');
    const slideshowBtn = container.querySelector('[data-action="start-slideshow"]');
    const mediaRoot = getMediaRoot(container);
    const seenIds = new Set();

    const feedEndpoint = container.dataset.feedEndpoint;
    const allItems = [];

    function applyFilters(items) {
        return items.filter((item) => {
            if (item.isAccessible === false) {
                return false;
            }
            if (filterOnlyMedia?.checked) {
                return item.mediaType === 'photo' || item.mediaType === 'video';
            }
            return true;
        });
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

        const items = applyFilters(allItems);
        if (!items.length) {
            updateStatus('No messages match the selected filters.');
            return;
        }

        const fragment = document.createDocumentFragment();
        items.forEach((item) => {
            fragment.appendChild(renderCard(item, mediaRoot));
        });
        galleryElement.appendChild(fragment);
        ensureGallery(galleryElement);
        galleryElement.querySelectorAll('video').forEach(initPlyr);
        updateStatus(`${items.length} item${items.length === 1 ? '' : 's'} loaded.`);
    }

function ingestItems(items) {
    if (!items?.length) {
        return;
    }
    items.forEach((raw) => {
        const item = normalizeMessage(raw);
        if (!item || item.isAccessible === false) {
            return;
        }
        if (seenIds.has(item.messageId)) {
            return;
        }
        seenIds.add(item.messageId);
        allItems.push(item);
    });
    render();
}

    const initialItems = parseInitialData();
    ingestItems(initialItems);

    let loading = false;
    let page = Number(container.dataset.initialPage || '1');
    if (Number.isNaN(page) || page < 1) {
        page = 1;
    }
    let hasMore = Boolean(feedEndpoint) && container.dataset.hasMore !== 'false';

    async function fetchPage(nextPage) {
        if (!feedEndpoint) {
            return [];
        }

        try {
            const url = new URL(feedEndpoint, window.location.origin);
            url.searchParams.set('page', String(nextPage));
            const response = await fetch(url.toString());
            if (!response.ok) {
                throw new Error(`Request failed with ${response.status}`);
            }
            const payload = await response.json();
            if (Array.isArray(payload.items)) {
                return payload.items;
            }

            if (payload.messages && Array.isArray(payload.messages.items)) {
                return payload.messages.items;
            }

            return Array.isArray(payload) ? payload : [];
        } catch (error) {
            console.error('Unable to load more messages', error);
            updateStatus('Unable to load additional messages.');
            hasMore = false;
            return [];
        }
    }

    async function loadMore() {
        if (loading || !hasMore) {
            return;
        }
        loading = true;
        updateStatus('Loading more messages…');
        const items = await fetchPage(page + 1);
        if (!items.length) {
            hasMore = false;
            updateStatus('You have reached the end of the results.');
        } else {
            page += 1;
            ingestItems(items);
        }
        loading = false;
    }

    if (filterOnlyMedia) {
        filterOnlyMedia.addEventListener('change', () => {
            render();
        });
    }

    if (layoutToggle) {
        layoutToggle.addEventListener('click', () => {
            container.classList.toggle('is-list-view');
            layoutToggle.classList.toggle('is-active');
        });
    }

    if (slideshowBtn) {
        slideshowBtn.addEventListener('click', () => {
            const instance = ensureGallery(galleryElement);
            if (instance && typeof instance.openGallery === 'function') {
                instance.openGallery(0);
                if (typeof instance.play === 'function') {
                    instance.play();
                }
            }
        });
    }

    if (sentinel && 'IntersectionObserver' in window) {
        const observer = new IntersectionObserver((entries) => {
            entries.forEach((entry) => {
                if (entry.isIntersecting) {
                    loadMore();
                }
            });
        }, {
            rootMargin: '200px'
        });
        observer.observe(sentinel);
    }

    attachKeyboardNavigation(container, galleryElement);
}

document.addEventListener('DOMContentLoaded', initRecentMessages);
