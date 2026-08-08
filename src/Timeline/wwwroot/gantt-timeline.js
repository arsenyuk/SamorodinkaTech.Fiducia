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
    }
};