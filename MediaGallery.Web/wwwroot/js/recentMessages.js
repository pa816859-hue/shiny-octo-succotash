const LIGHTGALLERY_PLUGINS = [
    window.lgThumbnail,
    window.lgZoom,
    window.lgFullscreen,
    window.lgAutoplay
].filter(Boolean);

const galleryInstances = new WeakMap();
const plyrPlayers = new WeakMap();

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

function updateStatus(statusElement, cards) {
    if (!statusElement) {
        return;
    }

    const visible = cards.filter((card) => !card.classList.contains('is-hidden')).length;
    statusElement.textContent = `${visible} item${visible === 1 ? '' : 's'} visible.`;
}

function initRecentMessages() {
    const container = document.querySelector('.recent-messages');
    if (!container) {
        return;
    }

    const galleryElement = container.querySelector('[data-gallery]');
    if (!galleryElement) {
        return;
    }

    const statusElement = container.querySelector('[data-scroll-status]');
    const filterOnlyMedia = container.querySelector('[data-filter="onlyMedia"]');
    const layoutToggle = container.querySelector('[data-action="toggle-layout"]');
    const slideshowBtn = container.querySelector('[data-action="start-slideshow"]');
    const cards = Array.from(galleryElement.querySelectorAll('.media-card'));

    const gallery = ensureGallery(galleryElement);
    galleryElement.querySelectorAll('video').forEach(initPlyr);
    updateStatus(statusElement, cards);

    function applyFilters() {
        const mediaOnly = Boolean(filterOnlyMedia?.checked);
        cards.forEach((card) => {
            const hasMedia = card.dataset.hasMedia === 'true';
            const shouldHide = mediaOnly && !hasMedia;
            card.classList.toggle('is-hidden', shouldHide);
        });

        if (gallery && typeof gallery.refresh === 'function') {
            gallery.refresh();
        }

        updateStatus(statusElement, cards);
    }

    if (filterOnlyMedia) {
        filterOnlyMedia.addEventListener('change', applyFilters);
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

    container.addEventListener('keydown', (event) => {
        if (!['ArrowRight', 'ArrowLeft', 'Home', 'End'].includes(event.key)) {
            return;
        }

        const visibleCards = cards.filter((card) => !card.classList.contains('is-hidden'));
        if (!visibleCards.length) {
            return;
        }

        const active = document.activeElement;
        let index = visibleCards.indexOf(active);

        switch (event.key) {
            case 'ArrowRight':
                index = Math.min(visibleCards.length - 1, index + 1);
                break;
            case 'ArrowLeft':
                index = Math.max(0, index - 1);
                break;
            case 'Home':
                index = 0;
                break;
            case 'End':
                index = visibleCards.length - 1;
                break;
            default:
                break;
        }

        if (index >= 0) {
            event.preventDefault();
            visibleCards[index].focus();
        }
    });
}

document.addEventListener('DOMContentLoaded', () => {
    initRecentMessages();
});
