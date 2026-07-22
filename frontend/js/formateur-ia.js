// Intégration du formateur IA côté navigateur.
//  - anam.ai : avatar conversationnel temps réel (audio/vidéo via WebRTC)
//  - HeyGen  : lecture des vidéos avatar déjà générées (URL renvoyée par l'API)
//
// Le backend ne renvoie qu'un session token éphémère anam.ai ; la clé API reste serveur.
const FormateurIA = (() => {
  let anamClient = null;

  // Démarre une session temps réel : le backend crée le session token,
  // puis le SDK anam.ai diffuse l'avatar dans les éléments <video>/<audio>.
  async function demarrerSessionTempsReel(formationId, videoElementId, audioElementId) {
    // 1) Récupérer un session token auprès de notre API (persona configurée côté serveur)
    const { sessionToken } = await API.sessionTempsReel(formationId, "Formateur IA");

    // 2) Charger le SDK anam.ai (module ESM depuis un CDN)
    //    Doc : https://docs.anam.ai
    const mod = await import("https://esm.sh/@anam-ai/js-sdk");
    const createClient = mod.createClient || mod.default?.createClient;
    if (!createClient) throw new Error("SDK anam.ai introuvable (vérifier l'URL/version).");

    // 3) Créer le client avec le token éphémère
    anamClient = createClient(sessionToken);

    // 4) Diffuser vers les éléments du DOM.
    //    La méthode recommandée diffuse vidéo + audio ; fallback vidéo seule.
    if (typeof anamClient.streamToVideoAndAudioElements === "function") {
      await anamClient.streamToVideoAndAudioElements(videoElementId, audioElementId);
    } else if (typeof anamClient.streamToVideoElement === "function") {
      await anamClient.streamToVideoElement(videoElementId);
    } else {
      throw new Error("Aucune méthode de streaming trouvée sur le client anam.ai.");
    }
    return anamClient;
  }

  // Forcer le formateur IA à dire un texte (répond à l'oral)
  async function envoyerMessage(texte) {
    if (!anamClient) throw new Error("Aucune session active.");
    if (typeof anamClient.talk === "function") return anamClient.talk(texte);
    console.warn("Méthode talk introuvable sur le client anam.ai.");
  }

  async function arreter() {
    if (anamClient && typeof anamClient.stopStreaming === "function") {
      await anamClient.stopStreaming();
    }
    anamClient = null;
  }

  return { demarrerSessionTempsReel, envoyerMessage, arreter };
})();
