// Barcode scanning with the camera. Uses the native BarcodeDetector when it supports EAN, otherwise a
// WebAssembly (ZXing) ponyfill that also works on iPhone. Frames are copied to a canvas first, which is
// far more reliable than detecting on the <video> element directly. Product names come from Open Food Facts.
let stream = null, timer = null, detector = null, busy = false;
const canvas = document.createElement("canvas");
const formats = ["ean_13", "ean_8", "upc_a", "upc_e", "code_128", "code_39", "itf"];

async function getDetector() {
    if (detector) return detector;
    if ("BarcodeDetector" in window) {
        try {
            const supported = await window.BarcodeDetector.getSupportedFormats();
            if (supported.includes("ean_13")) return detector = new window.BarcodeDetector({ formats: formats.filter(f => supported.includes(f)) });
        } catch { }
    }
    const mod = await import("https://esm.sh/barcode-detector@2.3.1/pure");
    return detector = new mod.BarcodeDetector({ formats });
}

export async function start(video, ref) {
    stop();
    const det = await getDetector();
    stream = await navigator.mediaDevices.getUserMedia({
        audio: false,
        video: { facingMode: { ideal: "environment" }, width: { ideal: 1920 }, height: { ideal: 1080 } }
    });
    const track = stream.getVideoTracks()[0];
    try { await track.applyConstraints({ advanced: [{ focusMode: "continuous" }] }); } catch { }
    video.setAttribute("playsinline", "");
    video.muted = true;
    video.srcObject = stream;
    await video.play();
    const ctx = canvas.getContext("2d", { willReadFrequently: true });
    timer = setInterval(async () => {
        if (busy || !stream || video.readyState < 2 || !video.videoWidth) return;
        busy = true;
        try {
            canvas.width = video.videoWidth;
            canvas.height = video.videoHeight;
            ctx.drawImage(video, 0, 0);
            const codes = await det.detect(canvas);
            const code = codes.find(c => c.rawValue)?.rawValue;
            if (code && stream) {
                navigator.vibrate?.(80);
                stop();
                await ref.invokeMethodAsync("OnScan", code);
            }
        } catch (e) {
            console.warn("Scannen mislukt", e);
        } finally { busy = false; }
    }, 250);
}

export function stop() {
    if (timer) clearInterval(timer);
    timer = null;
    stream?.getTracks().forEach(t => t.stop());
    stream = null;
}

// Returns JSON {label, text}: label = brand + product name, text = everything useful for matching a snack
// (names, generic name, categories, quantity). Empty object when the product is unknown.
export async function lookup(code) {
    try {
        const fields = "product_name_nl,product_name,generic_name_nl,generic_name,brands,categories,quantity,product_quantity,serving_size,serving_quantity,number_of_units,nutriments,nutriscore_grade,image_front_small_url,image_small_url";
        const r = await fetch(`https://world.openfoodfacts.org/api/v2/product/${encodeURIComponent(code)}.json?fields=${fields}`);
        if (!r.ok) return "{}";
        const p = (await r.json()).product;
        if (!p) return "{}";
        const name = (p.product_name_nl || p.product_name || "").trim();
        const brand = (p.brands || "").split(",")[0].trim();
        const label = brand && name && !name.toLowerCase().includes(brand.toLowerCase()) ? `${brand} ${name}` : name;
        const text = [p.product_name_nl, p.product_name, p.generic_name_nl, p.generic_name, p.categories, p.quantity, p.serving_size].filter(x => x).join(" | ");
        // Pieces per pack: explicit unit count, "10 x 70 g", "8 stuks", or total weight / portion weight.
        const total = parseFloat(p.product_quantity) || 0;
        const serving = parseFloat(p.serving_quantity) || 0;
        let pieces = parseInt(p.number_of_units) || 0;
        const m = /(\d{1,3})\s*[x×]\s*\d/i.exec(p.quantity || "") || /(\d{1,3})\s*(?:stuks|stuk|st\b|pcs|pieces)/i.exec(`${p.quantity || ""} ${name}`);
        if (!pieces && m) pieces = parseInt(m[1]);
        if (!pieces && total > 0 && serving > 0 && total >= serving) pieces = Math.round(total / serving);
        if (pieces < 1 || pieces > 200) pieces = 0;
        const n = p.nutriments || {};
        let kcal = n["energy-kcal_serving"] || 0;
        if (!kcal && n["energy-kcal_100g"]) {
            const per = serving || (pieces && total ? total / pieces : 0);
            if (per) kcal = n["energy-kcal_100g"] * per / 100;
        }
        return JSON.stringify({
            label, text, pieces,
            image: p.image_front_small_url || p.image_small_url || "",
            kcal: Math.round(kcal) || 0,
            nutri: /^[a-e]$/i.test(p.nutriscore_grade || "") ? p.nutriscore_grade.toUpperCase() : "",
            qty: p.quantity || ""
        });
    } catch { return "{}"; }
}
