// Carousel accessibility enhancements
document.addEventListener('DOMContentLoaded', function () {
    // Pause carousel on focus for accessibility
    document.querySelectorAll('.carousel').forEach(function (carousel) {
        var bsCarousel = bootstrap.Carousel.getOrCreateInstance(carousel);

        // Pause when any focusable element inside receives focus
        carousel.querySelectorAll('a, button, [tabindex]').forEach(function (el) {
            el.addEventListener('focus', function () {
                bsCarousel.pause();
            });
        });

        // Resume on mouse leave (only for auto-cycling carousels)
        carousel.addEventListener('mouseleave', function () {
            if (carousel.getAttribute('data-bs-ride') === 'carousel') {
                bsCarousel.cycle();
            }
        });
    });

    // Stack carousel: ensure no auto-cycle
    document.querySelectorAll('.stack-carousel').forEach(function (carousel) {
        var bsCarousel = bootstrap.Carousel.getOrCreateInstance(carousel, {
            ride: false,
            interval: false
        });
    });
});
