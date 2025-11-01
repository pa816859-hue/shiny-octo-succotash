const LIGHTGALLERY_PLUGINS = [
    window.lgThumbnail,
    window.lgZoom,
    window.lgFullscreen,
    window.lgAutoplay
].filter(Boolean);

const galleryInstances = new WeakMap();
const plyrPlayers = new WeakMap();
const carouselRegistry = new WeakMap();

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

function initCarousel(carousel) {
    if (!carousel) {
        return;
    }

    if (carouselRegistry.has(carousel)) {
        const refresh = carouselRegistry.get(carousel);
        if (typeof refresh === 'function') {
            requestAnimationFrame(refresh);
        }
        return;
    }

    const viewport = carousel.querySelector('[data-carousel-viewport]');
    const prev = carousel.querySelector('[data-carousel-prev]');
    const next = carousel.querySelector('[data-carousel-next]');

    if (!viewport) {
        return;
    }

    const scrollByAmount = (direction) => {
        const amount = Math.max(1, Math.floor(viewport.clientWidth * 0.85));
        viewport.scrollBy({ left: amount * direction, behavior: 'smooth' });
    };

    const updateControls = () => {
        const maxScrollLeft = viewport.scrollWidth - viewport.clientWidth;
        const tolerance = 4;
        const atStart = viewport.scrollLeft <= tolerance;
        const atEnd = viewport.scrollLeft >= maxScrollLeft - tolerance;

        if (prev) {
            prev.disabled = atStart;
        }

        if (next) {
            next.disabled = atEnd;
        }
    };

    if (prev) {
        prev.addEventListener('click', () => scrollByAmount(-1));
    }

    if (next) {
        next.addEventListener('click', () => scrollByAmount(1));
    }

    viewport.addEventListener('scroll', updateControls, { passive: true });
    window.addEventListener('resize', updateControls, { passive: true });

    viewport.addEventListener('keydown', (event) => {
        if (event.key === 'ArrowRight') {
            event.preventDefault();
            scrollByAmount(1);
        } else if (event.key === 'ArrowLeft') {
            event.preventDefault();
            scrollByAmount(-1);
        }
    });

    requestAnimationFrame(updateControls);
    carouselRegistry.set(carousel, updateControls);
}

function initUserProfile() {
    const container = document.querySelector('.user-profile');
    if (!container) {
        return;
    }

    const galleryHosts = Array.from(container.querySelectorAll('[data-gallery]'));
    galleryHosts.forEach((host) => {
        ensureGallery(host);
    });

    const carousels = Array.from(container.querySelectorAll('[data-carousel]'));
    carousels.forEach(initCarousel);

    container.querySelectorAll('video').forEach(initPlyr);

    container.addEventListener('keydown', (event) => {
        if (!['ArrowRight', 'ArrowLeft', 'Home', 'End'].includes(event.key)) {
            return;
        }

        const cards = Array.from(container.querySelectorAll('.media-card'));
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

document.addEventListener('DOMContentLoaded', () => {
    initUserProfile();
});
