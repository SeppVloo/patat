// Vibration for the fry timer. Android: navigator.vibrate. iOS (18+) has no vibrate API, but clicking a
// <label> around an <input type=checkbox switch> gives a haptic tick. iOS only allows that synchronously
// inside a real tap, so it is also triggered from a native click listener (Blazor async interop is too late).
(function () {
    function tick() {
        const label = document.createElement('label');
        label.setAttribute('aria-hidden', 'true');
        label.style.display = 'none';
        const input = document.createElement('input');
        input.type = 'checkbox';
        input.setAttribute('switch', '');
        label.appendChild(input);
        document.body.appendChild(label);
        label.click();
        label.remove();
    }

    window.patatBuzz = function (n) {
        n = Math.max(1, n || 1);
        if (navigator.vibrate) {
            const pattern = [];
            for (let i = 0; i < n; i++) pattern.push(250, 150);
            navigator.vibrate(pattern);
            return;
        }
        tick();
        for (let i = 1; i < n; i++) setTimeout(tick, i * 200);
    };

    // Elements with data-buzz tick on tap; any tap while the alarm flashes ticks too.
    document.addEventListener('click', e => {
        const el = e.target.closest ? e.target.closest('[data-buzz]') : null;
        if (el) window.patatBuzz(+el.dataset.buzz || 1);
        else if (document.body.classList.contains('pt-alarm')) window.patatBuzz(1);
    }, true);
})();
