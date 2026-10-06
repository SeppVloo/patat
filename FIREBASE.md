# Database instellen (Firebase Realtime Database)

Zonder database werkt Patat via Trystero (de snackbakker moet dan online zijn). Met database wordt de voorraad online bewaard per gezinscode.

1. Ga naar https://console.firebase.google.com en maak een project (bijv. `patat`).
2. **Build → Authentication → Sign-in method:** zet **Anonymous** aan.
3. **Authentication → Settings → Authorized domains:** voeg `patat.vloo.nl` toe.
4. **Build → Realtime Database → Create database** (regio Europe-west1), start in *locked mode*.
5. **Rules:** plak dit en publiceer:

```json
{
  "rules": {
	".read": false,
	".write": false,
	"families": {
	  "$family": {
		".read": "auth != null",
		".write": "auth != null",
		"state": { ".validate": "newData.isString() && newData.val().length < 200000" },
		"inbox": { "$item": { ".validate": "newData.hasChild('type')" } },
		"presence": { "$item": { ".validate": "!newData.exists() || newData.hasChild('name')" } }
	  }
	},
	"lan": {
	  "$net": {
		".read": "auth != null",
		".write": "auth != null",
		".validate": "!newData.exists() || (newData.child('code').isString() && newData.child('code').val().length <= 30)"
	  }
	}
  }
}
```

6. **Project settings → Your apps → Web app (</>)**: registreer een app en kopieer de config.
7. Vul `Patat/wwwroot/firebase-config.js` in (`export default { apiKey, authDomain, databaseURL, projectId, appId };`), commit en push.

De familiepaden zijn een SHA-256-hash van de gezinscode; wie de code kent kan de voorraad lezen/wijzigen. Kies dus een niet te raden gezinscode.
