# Front web — FormationAI

Front statique (HTML / CSS / JavaScript, sans build) qui consomme l'API FormationAI.

```
frontend/
├── index.html            # page unique (auth, catalogue, détail, admin)
├── css/styles.css
└── js/
    ├── config.js         # URL de l'API (à adapter)
    ├── api.js            # appels API + gestion du JWT
    ├── formateur-ia.js   # anam.ai (temps réel) + vidéos HeyGen
    └── app.js            # navigation et rendu
```

## Lancer

1. Démarrer l'API sur `http://localhost:8080`.
2. Vérifier `js/config.js` → `API_BASE_URL`.
3. Servir le dossier (le protocole `file://` limite certains appels) :

```bash
npx serve frontend
# ou
python -m http.server 5500 --directory frontend
```

Dans VS Code : clic droit sur `index.html` → **Open with Live Server**.

Comptes de démo : `admin@formationai.dev` / `Admin@123`.

## Formateur IA

Le front passe toujours par l'API (les clés restent serveur). Pour anam.ai, l'API renvoie un **session token** éphémère ; le SDK `@anam-ai/js-sdk` (CDN dans `formateur-ia.js`) établit la connexion temps réel — vérifier la version et les méthodes (`streamToVideoElement`, `talk`) dans la doc anam.ai.
