// Barcode scanning with the camera. Uses the native BarcodeDetector when available, otherwise a
// WebAssembly ponyfill (also works on iPhone). Product names are looked up in Open Food Facts.
let stream = null, timer = null, detector = null;

async function getDetector() {
    if (detector) return detector;
    const formats = ["ean_13", "ean_8", "upc_a", "upc_e", "code_128"];
    if ("BarcodeDetector" in window) {
        try {
            const supported = await window.BarcodeDetector.getSupportedFormats();
            if (supported.includes("ean_13")) return detector = new window.BarcodeDetector({ formats });
        } catch { }
    }
    const mod = await import("https://esm.sh/barcode-detector@2/pure");
    return detector = new mod.BarcodeDetector({ formats });
}

export async function start(video, ref) {
    stop();
    const det = await getDetector();
    stream = await navigator.mediaDevices.getUserMedia({ video: { facingMode: "environment" }, audio: false });
    video.srcObject = stream;
    video.setAttribute("playsinline", "");
    await video.play();
    let last = "";
    timer = setInterval(async () => {
        if (video.readyState < 2) return;
        try {
            const codes = await det.detect(video);
            const code = codes[0]?.rawValue;
            if (code && code !== last) {
                last = code;
                navigator.vibrate?.(80);
                await ref.invokeMethodAsync("OnScan", code);
                setTimeout(() => { if (last === code) last = ""; }, 2500);
            }
        } catch { }
    }, 300);
}

export function stop() {
    if (timer) clearInterval(timer);
    timer = null;
    stream?.getTracks().forEach(t => t.stop());
    stream = null;
}

export async function lookup(code) {
    try {
        const r = await fetch(`https://world.openfoodfacts.org/api/v2/product/${encodeURIComponent(code)}.json?fields=product_name_nl,product_name,brands`);
        if (!r.ok) return "";
        const p = (await r.json()).product;
        return (p?.product_name_nl || p?.product_name || "").trim();
    } catch { return ""; }
}
