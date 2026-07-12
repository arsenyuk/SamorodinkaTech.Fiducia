// УПД.15: Принудительное закрытие сессии при бездействии
window.idleTimer = {
    _timeout: null,
    _minutes: 5,

    start: function (minutes) {
        this._minutes = minutes || 5;
        this.reset();
        var events = ['mousemove', 'keydown', 'click', 'scroll', 'touchstart'];
        var self = this;
        events.forEach(function (e) {
            document.addEventListener(e, function () { self.reset(); }, { passive: true });
        });
    },

    reset: function () {
        clearTimeout(this._timeout);
        var self = this;
        this._timeout = setTimeout(function () { self.logout(); }, this._minutes * 60 * 1000);
    },

    logout: function () {
        // Вызов endpoint выхода (всегда возвращает 200 OK)
        fetch('/api/session/logout', { method: 'POST' })
            .finally(function () {
                // Очистка localStorage
                localStorage.removeItem('currentUserId');
                localStorage.removeItem('currentUserName');
                localStorage.removeItem('currentUserRole');

                // Очистка cookie
                document.cookie.split(';').forEach(function (c) {
                    var name = c.trim().split('=')[0];
                    document.cookie = name + '=;expires=Thu, 01 Jan 1970 00:00:00 GMT;path=/';
                });

                window.location = '/login';
            });
    }
};
