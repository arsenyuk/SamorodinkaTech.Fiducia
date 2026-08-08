export function getScrollInfo(element) {
    return {
        scrollLeft: element.scrollLeft,
        clientWidth: element.clientWidth,
        scrollWidth: element.scrollWidth
    };
}

export function scrollTo(element, left, animate) {
    if (animate) {
        element.scrollTo({ left: left, behavior: 'smooth' });
    } else {
        element.scrollLeft = left;
    }
}