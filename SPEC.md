# Patat – Specificatie

> Houd dit document bij bij elke nieuwe of gewijzigde spec (incl. changelog), in dezelfde commit als de code.

## Overzicht
- Gezinsapp om snacks bij de patat te bestellen. Iedereen bestelt op zijn **eigen apparaat**; de **snackbakker** ziet een overzicht en beheert de voorraad.
- Route `/` (bestellen), `/bakker` (bak-overzicht), `/voorraad` (voorraadbeheer).
- Nederlandstalig, werkt op laptop, iPad en iPhone (installeerbaar als PWA). Gehost op GitHub Pages: https://patat.vloo.nl (geen eigen server).

## Architectuur
- Eigen repo/solution (`Patat.slnx`), standalone **Blazor WebAssembly**-app `Patat/Patat.csproj` (.NET 11). `App.razor`, `MainLayout.razor`, `Program.cs`, `wwwroot/index.html` (laadt `patat.css`). Paden in `Patat/` hieronder.
- `Model/PatatEngine.cs`: pure logica, geen UI. `PatatState` = snacks, bestellingen, mandjes (batches), `Open`, `Rejected`.
- `Pages/PatatPage.razor`: instellen, bestellen, tabs, JS-interop en synchronisatie.
- `Components/BakkerView.razor`, `Components/VoorraadView.razor`, `Components/Help.razor` (❓ Hoe werkt het?).
- `wwwroot/patat.js`: Trystero (MQTT-strategie, net als Pong) + localStorage.
- `wwwroot/patat.css`: alle stijlen met prefix `pt-`.

## Synchronisatie
- Iedereen met dezelfde **gezinscode** komt in Trystero-room `fam-<code>` (appId `sepp-patat-v1`). Werkt via wifi én mobiel internet.
- Het apparaat van de **snackbakker** is de bron van waarheid en stuurt na elke wijziging de hele state (JSON) naar iedereen. Nieuwe peers krijgen direct de state.
- De state hoort bij de **gezinscode**, niet bij een apparaat: elk apparaat bewaart de laatst ontvangen state in `patat.state.<code>`. `PatatState.Version` wordt bij elke wijziging van de bakker opgehoogd. Een niet-bakker stuurt bij `hello` zijn bewaarde state mee; de bakker neemt die over als hij een hogere `Version` heeft. Zo kan de bakker op een ander apparaat verder.
- Andere apparaten sturen alleen `order` (JSON `Order`) of `cancel` (naam). Een verzonden bestelling blijft *pending* (`patat.pending`) en wordt opnieuw verstuurd bij elke ontvangen state, tot de state die bestelling (zelfde `Stamp`) bevat of in `Rejected` staat.
- **Database (optioneel, Firebase Realtime Database):** als `wwwroot/firebase-config.js` is ingevuld, logt elk apparaat anoniem in en gebruikt `families/<sha256("patat:"+code)>/state` (state-JSON) en `/inbox` (`{type:"order",json}` of `{type:"cancel",person}`). De bakker verwerkt de inbox, schrijft de nieuwe state en verwijdert het inbox-item. Iedereen leest de state live, ook zonder online bakker-peer. Een nieuwere database-state (`Version`) wordt door de bakker overgenomen; is de database leeg, dan schrijft de bakker zijn lokale state. Zonder config (standaard `null`) werkt alles zoals hiervoor via Trystero. Zie `FIREBASE.md`.
- Er hoort precies één snackbakker te zijn. Zonder online snackbakker kan niemand bestellen ("Wachten op de snackbakker…"), behalve in databasemodus: dan komen bestellingen in de inbox en verwerkt de bakker ze zodra hij de app opent.

## Regels
- Eén bestelling per persoon (naam, hoofdletterongevoelig); opnieuw bestellen vervangt de vorige.
- Voorraad wordt bij bestellen meteen gereserveerd (afgetrokken) en bij annuleren teruggegeven. Meer bestellen dan voorraad kan niet (UI begrenst, engine weigert).
- Bestellen kan alleen als `Open` aan staat (schakelaar op het bak-scherm).
- **Nieuwe ronde** wist bestellingen, mandjes en weigeringen; voorraad blijft zoals hij is.
- Elke snack heeft **stuks per pak** (`PackSize`, standaard 1). Op het voorraadscherm telt de knop **+ pak (N)** in één keer een heel pak bij de voorraad op.
- Elke snack heeft een **icoon** (`Icon`): sleutel uit de eigen SVG-catalogus `SnackIcons` (patat, frikandel, frikandel speciaal, kroket, kipcorn, kaassoufflé, bitterbal, berenklauw, loempia, mexicano, viandel, bamischijf, nasibal, kipnuggets, burger, saus, snack), `emoji` (eigen emoji) of leeg = automatisch raden op naam. Kiezen via de icoonknop op het voorraadscherm.

## Samen bakken
- Elke snack heeft een baktijd (minuten) en een **frituurgroep**. Alleen snacks met dezelfde groep (hoofdletterongevoelig) én dezelfde baktijd gaan samen in één mandje (`GroupKey` = groep@minuten); lege groep = altijd los.
- Is de timer van een mandje verlopen, dan klinkt een piepsignaal (Web Audio, `window.patatBeep`) en trilt de telefoon; dit herhaalt elke 15 s tot *Klaar*. Het geluid wordt bij *Start* ontgrendeld (nodig op mobiel).
- **Geschiedenis:** *Nieuwe ronde* bewaart de bestellingen als `Round` (tijd, bestellingen, snacknamen) in `PatatState.History`; rondes ouder dan een jaar worden verwijderd. Zichtbaar onder 📜 Geschiedenis op het bak-scherm.
- Mandje-tijd = langste baktijd in de groep. Mandjes staan op volgorde van langste eerst.
- Per mandje: *Start* (timer, knippert als hij klaar is), *Klaar*, *Opnieuw*. Komt er een bestelling bij voor een mandje dat al klaar was, dan gaat dat mandje terug naar open.

## Opslag (localStorage)
- Trystero wordt pas in `start()` dynamisch geladen, zodat de voorraad uit localStorage altijd gelezen kan worden, ook als de CDN faalt (bijv. vlak na een redeploy).
- Onleesbare opgeslagen state wordt nooit overschreven met de standaardlijst: er wordt eerst een reservekopie `patat.state.backup-<tijd>` gemaakt en een melding getoond.
- `patat.name`, `patat.code`, `patat.bakker` ("1"), `patat.state.<code>` (alle apparaten; oude `patat.state` wordt eenmalig als terugval gelezen), `patat.pending` (alleen niet-bakker).

## Changelog
- **Basis:** bestellen per apparaat, bakker-overzicht met mandjes per frituurgroep en timers, voorraadbeheer, synchronisatie via Trystero, in-app hulp.
- **Eigen repo:** losgetrokken uit SeppsGameCenter (geschiedenis behouden) naar standalone Blazor WebAssembly-app met eigen solution, GitHub Pages-workflow en PWA-manifest. Routes nu `/`, `/bakker`, `/voorraad`. JS-interop afgeschermd met try/catch (`JsVoid`/`JsGet`).
- **Deploy:** GitHub Pages-workflow zet bij geen CNAME de `<base href>` op `/<repo-naam>/`.
- **Pakken + nieuwe stijl:** snacks hebben stuks per pak met een *+ pak*-knop om de voorraad snel op te hogen. Professionelere, rustige vormgeving (neutrale kleuren, subtiele schaduwen, segment-tabs).
- **Voorraad blijft na redeploy:** Trystero lazy geladen (opslag werkt los van de CDN); onleesbare state krijgt een reservekopie i.p.v. een stille reset.
- **Eigen domein:** `wwwroot/CNAME` = `patat.vloo.nl`; site draait daardoor vanaf `/` (base href blijft `/`).
in `patat.js` hersteld.
- **Iconen + database:** eigen SVG-iconen per snack met kiezer en automatisch raden voor nieuwe snacks; optionele Firebase Realtime Database als gedeelde opslag per gezinscode (inbox voor bestellingen), met Trystero als terugval.
- **Geschiedenis, piep, mandjes:** bestelgeschiedenis tot een jaar; geluidssignaal als de baktimer verloopt; alleen gelijke baktijden samen in een mandje.
