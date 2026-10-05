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
- Er hoort precies één snackbakker te zijn. Zonder online snackbakker kan niemand bestellen ("Wachten op de snackbakker…").

## Regels
- Eén bestelling per persoon (naam, hoofdletterongevoelig); opnieuw bestellen vervangt de vorige.
- Voorraad wordt bij bestellen meteen gereserveerd (afgetrokken) en bij annuleren teruggegeven. Meer bestellen dan voorraad kan niet (UI begrenst, engine weigert).
- Bestellen kan alleen als `Open` aan staat (schakelaar op het bak-scherm).
- **Nieuwe ronde** wist bestellingen, mandjes en weigeringen; voorraad blijft zoals hij is.
- Elke snack heeft **stuks per pak** (`PackSize`, standaard 1). Op het voorraadscherm telt de knop **+ pak (N)** in één keer een heel pak bij de voorraad op.

## Samen bakken
- Elke snack heeft een baktijd (minuten) en een **frituurgroep**. Snacks met dezelfde groep (hoofdletterongevoelig) gaan samen in één mandje; lege groep = altijd los.
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
- **Voorraad per gezinscode:** state opgeslagen per gezinscode op elk apparaat met `Version`; de bakker neemt een nieuwere versie van andere apparaten over. Fix: afgebroken `joinRoom`-regel in `patat.js` hersteld.
