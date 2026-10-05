// Serverless sync for the snack orders (same approach as Pong's net.js).
// Everybody with the same family code joins one Trystero room (signalling via public MQTT brokers).
// The device of whoever is snackbakker broadcasts the state. Every device keeps a copy of the latest
// state per family code, so a baker on another device picks up the newest version (highest Version wins).
// Trystero is loaded lazily: if the CDN fails, localStorage (the stock!) must still be readable.
const TRYSTERO = "https://esm.sh/trystero@0.21.8/mqtt";

const APP_ID = "sepp-patat-v1";
let room = null, dotnet = null, isHost = false, stateKey = null;
let sendState, sendOrder, sendCancel, sendHello;

function peers() { return room ? Object.keys(room.getPeers()).length : 0; }
function call(name, ...args) { dotnet?.invokeMethodAsync(name, ...args).catch(() => { }); }
function cached() { return (stateKey && localStorage.getItem(stateKey)) || ""; }

export async function start(ref, code, host, key) {
    leave();
    dotnet = ref; isHost = host; stateKey = key;
    let joinRoom;
    try { ({ joinRoom } = await import(TRYSTERO)); } catch (e) { console.warn("Trystero laden mislukt", e); return; }
    room = joinRoom({ appId: APP_ID }, "fam-" + code.trim().toLowerCase());
    let onState, onOrder, onCancel, onHello;
    [sendState, onState] = room.makeAction("state");
    [sendOrder, onOrder] = room.makeAction("order");
    [sendCancel, onCancel] = room.makeAction("cancel");
    [sendHello, onHello] = room.makeAction("hello");

    onState(json => { if (!isHost) call("OnState", json); });
    onOrder(json => { if (isHost) call("OnOrder", json); });
    onCancel(person => { if (isHost) call("OnCancel", person); });
    // A hello carries the sender's cached state, so the baker can adopt it if it is newer.
    onHello((data, peer) => { if (isHost) call("OnHello", peer, typeof data === "string" ? data : ""); });

    room.onPeerJoin(peer => {
        if (isHost) call("OnHello", peer, ""); else sendHello(cached(), peer);
        call("OnPeers", peers());
    });
    room.onPeerLeave(() => call("OnPeers", peers()));
}

export function setHost(host) {
    isHost = host;
    if (!host) sendHello?.(cached());
}

export function broadcast(json, peer) { sendState?.(json, peer ?? null); }
export function order(json) { sendOrder?.(json); }
export function cancel(person) { sendCancel?.(person); }

export function leave() {
    try { room?.leave(); } catch { }
    room = null; sendState = sendOrder = sendCancel = sendHello = null;
}

export function load(key) { return localStorage.getItem(key); }
export function save(key, value) {
    if (value === null || value === undefined) localStorage.removeItem(key); else localStorage.setItem(key, value);
}
