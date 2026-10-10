// Show start-up errors on screen (e.g. older iPads); otherwise the loading circle just hangs.
(function () {
    function show(msg) {
        if (document.querySelector(".pt")) return;
        var app = document.getElementById("app") || document.body;
        var p = document.getElementById("pt-boot-error");
        if (!p) { p = document.createElement("pre"); p.id = "pt-boot-error"; app.appendChild(p); }
        p.style.cssText = "white-space:pre-wrap;font-size:12px;padding:12px;margin:12px;border:1px solid #c33;border-radius:8px;color:#c33";
        p.textContent += "Opstartfout: " + msg + "\n";
        if (p.textContent.indexOf("Browser:") < 0) p.textContent += "Browser: " + navigator.userAgent + "\n";
    }
    window.addEventListener("error", function (e) { show((e.message || "?") + (e.filename ? " @ " + e.filename.split("/").pop() + ":" + e.lineno : "")); });
    window.addEventListener("unhandledrejection", function (e) { var r = e.reason; show(String(r && (r.stack || r.message) || r)); });
    var origError = console.error;
    console.error = function () { try { show(Array.prototype.map.call(arguments, String).join(" ").slice(0, 600)); } catch (x) { } return origError.apply(console, arguments); };
    setTimeout(function () { if (!document.querySelector(".pt")) show("app is na 30 seconden nog niet gestart"); }, 30000);
})();
