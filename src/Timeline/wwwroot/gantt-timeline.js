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
    observeStickyLabels: function (scrollContainer, edgeGap) {
        var cells = scrollContainer.querySelectorAll('[data-sticky-cell]');
        if (cells.length === 0) return;
        if (edgeGap === undefined) edgeGap = 6;

        var update = function () {
            var containerRect = scrollContainer.getBoundingClientRect();
            var viewportCenter = containerRect.left + scrollContainer.clientWidth / 2;

            cells.forEach(function (cell) {
                var label = cell.querySelector('[data-sticky-label]');
                if (!label) return;

                var cellRect = cell.getBoundingClientRect();
                var cellCenter = cellRect.left + cellRect.width / 2;

                // Сдвиг, чтобы центр label оказался в центре viewport
                var desiredShift = viewportCenter - cellCenter;

                // Максимальный сдвиг — label не выходит за границы ячейки
                var maxShift = Math.max(0, cellRect.width / 2 - label.offsetWidth / 2 - edgeGap);
                var shift = Math.max(-maxShift, Math.min(maxShift, desiredShift));

                label.style.setProperty('--sticky-shift', shift + 'px');
            });
        };

        scrollContainer.addEventListener('scroll', update, { passive: true });
        window.addEventListener('resize', update);
        update();
    }
};

window.FiduciaGantt = {
    _syncPairs: [],

    bindVerticalSync: function (leftEl, rightEl) {
        if (!leftEl || !rightEl) return;
        var syncing = false;

        var onLeftScroll = function () {
            if (syncing) return;
            syncing = true;
            rightEl.scrollTop = leftEl.scrollTop;
            requestAnimationFrame(function () { syncing = false; });
        };

        var onRightScroll = function () {
            if (syncing) return;
            syncing = true;
            leftEl.scrollTop = rightEl.scrollTop;
            requestAnimationFrame(function () { syncing = false; });
        };

        leftEl.addEventListener('scroll', onLeftScroll, { passive: true });
        rightEl.addEventListener('scroll', onRightScroll, { passive: true });

        this._syncPairs.push({ leftEl: leftEl, rightEl: rightEl, leftHandler: onLeftScroll, rightHandler: onRightScroll });
    },

    unbindVerticalSync: function (leftEl, rightEl) {
        for (var i = this._syncPairs.length - 1; i >= 0; i--) {
            var pair = this._syncPairs[i];
            if (pair.leftEl === leftEl && pair.rightEl === rightEl) {
                leftEl.removeEventListener('scroll', pair.leftHandler);
                rightEl.removeEventListener('scroll', pair.rightHandler);
                this._syncPairs.splice(i, 1);
            }
        }
    },

    scrollTo: function (element, left) {
        if (element && element.scrollTo) {
            element.scrollTo({ left: left, behavior: 'smooth' });
        }
    },

    observeHeaderHeight: function (headerEl, dotNetRef) {
        if (!headerEl || !dotNetRef) return;
        var observer = new ResizeObserver(function () {
            dotNetRef.invokeMethodAsync('OnHeaderHeightChanged', headerEl.offsetHeight);
        });
        observer.observe(headerEl);
        headerEl._fiduciaHeaderObserver = observer;
    },

    disconnectHeaderObserver: function (headerEl) {
        if (headerEl && headerEl._fiduciaHeaderObserver) {
            headerEl._fiduciaHeaderObserver.disconnect();
            delete headerEl._fiduciaHeaderObserver;
        }
    },

    bindHorizontalSync: function (headerEl, scrollEl) {
        if (!headerEl || !scrollEl) return;
        // Находим скролл-контейнер внутри GanttTimeline
        var timelineScroll = headerEl.querySelector('div[style*="overflow-x:auto"]');
        if (!timelineScroll) return;

        var syncing = false;

        var onHeaderScroll = function () {
            if (syncing) return;
            syncing = true;
            scrollEl.scrollLeft = timelineScroll.scrollLeft;
            requestAnimationFrame(function () { syncing = false; });
        };

        var onBarScroll = function () {
            if (syncing) return;
            syncing = true;
            timelineScroll.scrollLeft = scrollEl.scrollLeft;
            requestAnimationFrame(function () { syncing = false; });
        };

        timelineScroll.addEventListener('scroll', onHeaderScroll, { passive: true });
        scrollEl.addEventListener('scroll', onBarScroll, { passive: true });

        this._syncPairs.push({
            leftEl: timelineScroll, rightEl: scrollEl,
            leftHandler: onHeaderScroll, rightHandler: onBarScroll
        });
    }
};