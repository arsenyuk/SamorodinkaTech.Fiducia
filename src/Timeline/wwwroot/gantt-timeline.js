window.FiduciaTimeline = {
    getScrollInfo: function (element) {
        return {
            scrollLeft: element.scrollLeft,
            clientWidth: element.clientWidth,
            scrollWidth: element.scrollWidth
        };
    },
    scrollTo: function (element, left) {
        element.scrollLeft = left;
    },
    scrollToToday: function (element, todayOffset) {
        var attempts = 0;
        var tryScroll = function () {
            if (element.scrollWidth > 0 && element.clientWidth > 0) {
                element.scrollLeft = todayOffset;
            } else if (attempts < 20) {
                attempts++;
                setTimeout(tryScroll, 100);
            }
        };
        setTimeout(tryScroll, 200);
    },
    observeStickyLabels: function (scrollContainer) {
        var overlays = scrollContainer.querySelectorAll('[data-sticky-overlay]');
        overlays.forEach(function (overlay) {
            // Найти следующую за оверлеем строку с data-sticky-row
            var row = overlay.nextElementSibling;
            if (!row || !row.hasAttribute('data-sticky-row')) return;

            var cells = row.querySelectorAll('[data-sticky-cell]');
            if (cells.length === 0) return;

            var update = function () {
                var viewCenter = scrollContainer.scrollLeft + scrollContainer.clientWidth / 2;
                var bestLabel = '';
                var bestDist = Infinity;

                cells.forEach(function (cell) {
                    var rect = cell.getBoundingClientRect();
                    var containerRect = scrollContainer.getBoundingClientRect();
                    var left = rect.left - containerRect.left;
                    var right = rect.right - containerRect.left;
                    var center = (left + right) / 2;
                    var dist = Math.abs(center - viewCenter);
                    if (dist < bestDist) {
                        bestDist = dist;
                        bestLabel = cell.getAttribute('data-sticky-cell') || '';
                    }
                });

                overlay.textContent = bestLabel;
            };

            scrollContainer.addEventListener('scroll', update, { passive: true });
            update();
        });
    }
};