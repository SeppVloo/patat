// Shared stock database (Firebase Realtime Database). The state of a family lives at
// families/<sha256(family code)>/state (JSON string) and the inbox of orders/cancels at .../inbox.
// The baker consumes the inbox, applies the rules in PatatEngine and writes the new state.
// The Firebase SDK is loaded lazily from gstatic, so the app still works when it is unreachable.
import config from "./firebase-config.js";

const SDK = "https://www.gstatic.com/firebasejs/11.0.2/";
let db = null, fb = null, stateRef = null, inboxRef = null, unsubs = [], dotnet = null;

function call(name, ...args) { dotnet?.invokeMethodAsync(name, ...args).catch(() => { }); }

async function familyId(code) {
    const bytes = new TextEncoder().encode("patat:" + code.trim().toLowerCase());
    const hash = await crypto.subtle.digest("SHA-256", bytes);
    return [...new Uint8Array(hash)].map(b => b.toString(16).padStart(2, "0")).join("");
}

export function enabled() { return !!(config && config.databaseURL); }

async function ensure() {
    if (db) return;
    const [app, auth, database] = await Promise.all([
        import(SDK + "firebase-app.js"), import(SDK + "firebase-auth.js"), import(SDK + "firebase-database.js")]);
    const fbApp = app.initializeApp(config);
    await auth.signInAnonymously(auth.getAuth(fbApp));
    db = database.getDatabase(fbApp);
    fb = database;
}

// Same wifi = same public IP address. The baker announces the family code under lan/<sha256(ip)>;
// a new device on that network can then offer to join. Entries older than 12 hours are ignored.
async function lanRef() {
    const r = await fetch("https://api.ipify.org?format=json");
    const ip = (await r.json()).ip;
    return fb.ref(db, `lan/${await familyId("lan:" + ip)}`);
}

export async function lanAnnounce(code, on) {
    if (!enabled()) return;
    try {
        await ensure();
        const ref = await lanRef();
        if (on) await fb.set(ref, { code, at: Date.now() }); else await fb.remove(ref);
    } catch (e) { console.warn("Wifi-gezin niet bijgewerkt", e); }
}

export async function lanFind() {
    if (!enabled()) return "";
    try {
        await ensure();
        const v = (await fb.get(await lanRef())).val();
        return v && v.code && Date.now() - (v.at || 0) < 12 * 3600 * 1000 ? v.code : "";
    } catch { return ""; }
}

export async function start(ref, code, host, name) {
    stop();
    if (!enabled()) return false;
    dotnet = ref;
    try {
        await ensure();
        const id = await familyId(code);
        stateRef = fb.ref(db, `families/${id}/state`);
        inboxRef = fb.ref(db, `families/${id}/inbox`);
        unsubs.push(fb.onValue(stateRef, snap => call("OnDbState", snap.val() ?? "")));
        if (host) unsubs.push(fb.onChildAdded(inboxRef, snap => call("OnDbInbox", snap.key, JSON.stringify(snap.val()))));
        // Presence: each open app registers itself; Firebase removes the entry when the connection drops.
        const presenceRoot = fb.ref(db, `families/${id}/presence`);
        const me = fb.push(presenceRoot);
        const info = { name: (name || "?").slice(0, 30), baker: !!host, at: Date.now() };
        unsubs.push(fb.onValue(fb.ref(db, ".info/connected"), async snap => {
            if (snap.val() !== true) return;
            await fb.onDisconnect(me).remove();
            await fb.set(me, info);
        }));
        unsubs.push(fb.onValue(presenceRoot, snap => call("OnDbPresence", JSON.stringify(Object.values(snap.val() ?? {})))));
        unsubs.push(() => fb.remove(me));
        return true;
    } catch (e) {
        console.warn("Database niet bereikbaar", e);
        return false;
    }
}

export function stop() {
    unsubs.forEach(u => { try { u(); } catch { } });
    unsubs = []; stateRef = inboxRef = null;
}

export async function save(json) { if (stateRef) await fb.set(stateRef, json); }
export async function order(json) { if (inboxRef) await fb.push(inboxRef, { type: "order", json }); }
export async function cancel(person) { if (inboxRef) await fb.push(inboxRef, { type: "cancel", person }); }
export async function done(key) { if (inboxRef) await fb.remove(fb.child(inboxRef, key)); }
