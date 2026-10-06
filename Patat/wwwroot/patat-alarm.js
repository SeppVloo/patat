// Keep the screen on while a fry timer runs, and flash the screen when one expires.
(function () {
    let lock = null, want = false;
    async function acquire() {
        if (!want || lock || !navigator.wakeLock || document.visibilityState !== 'visible') return;
        try {
            lock = await navigator.wakeLock.request('screen');
            lock.addEventListener('release', () => { lock = null; });
        } catch { lock = null; }
    }
    document.addEventListener('visibilitychange', acquire);
    window.patatWake = function (on) {
        want = !!on;
        if (want) acquire();
        else if (lock) { lock.release(); lock = null; }
    };
    window.patatFlash = function (on) {
        document.body.classList.toggle('pt-alarm', !!on);
    };
})();
