// Carousel accessibility enhancements
document.addEventListener('DOMContentLoaded', function () {
    // Preview-mode popup: collapsed state persists across navigation via localStorage
    // so editors who collapse it once don't have to dismiss it on every page.
    var previewBanner = document.querySelector('[data-preview-banner]');
    if (previewBanner) {
        var previewToggle = previewBanner.querySelector('[data-preview-toggle]');
        var storageKey = 'ficto_preview_banner_collapsed';
        var applyCollapsed = function (collapsed) {
            previewBanner.setAttribute('data-collapsed', collapsed ? 'true' : 'false');
            if (previewToggle) {
                previewToggle.setAttribute('aria-expanded', collapsed ? 'false' : 'true');
            }
        };
        applyCollapsed(localStorage.getItem(storageKey) === 'true');
        if (previewToggle) {
            previewToggle.addEventListener('click', function () {
                var next = previewBanner.getAttribute('data-collapsed') !== 'true';
                applyCollapsed(next);
                localStorage.setItem(storageKey, next ? 'true' : 'false');
            });
        }
    }


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

    // Filter form: on checkbox change, cascade parent → descendants, then AJAX-swap
    // the product grid without a full page reload. The URL is updated via pushState
    // so the filtered view stays shareable and the back/forward buttons keep working.
    document.querySelectorAll('[data-filter-form]').forEach(function (form) {
        var results = document.getElementById('product-results');
        if (!results) return;

        form.addEventListener('change', async function (e) {
            var changed = e.target.closest('input[type="checkbox"][name="category"]');
            if (changed) {
                var item = changed.closest('.product-filter__item');
                var childList = item ? item.querySelector('.product-filter__children') : null;
                if (childList) {
                    childList.querySelectorAll('input[type="checkbox"]').forEach(function (c) {
                        c.checked = changed.checked;
                    });
                }
            }

            var params = new URLSearchParams(new FormData(form));
            var query = params.toString();
            var url = form.action + (query ? '?' + query : '');

            var response = await fetch(url, {
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });
            results.innerHTML = await response.text();
            history.pushState({}, '', url);
        });
    });
});
