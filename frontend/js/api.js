// Petite couche d'accès à l'API : gestion du token JWT + wrapper fetch.
const API = (() => {
  const base = window.APP_CONFIG.API_BASE_URL;
  const TOKEN_KEY = "formationai_token";
  const USER_KEY = "formationai_user";

  const getToken = () => localStorage.getItem(TOKEN_KEY);
  const getUser = () => {
    const raw = localStorage.getItem(USER_KEY);
    return raw ? JSON.parse(raw) : null;
  };
  const setSession = (data) => {
    localStorage.setItem(TOKEN_KEY, data.token);
    localStorage.setItem(USER_KEY, JSON.stringify({
      id: data.userId, email: data.email, role: data.role,
    }));
  };
  const clear = () => {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  };

  async function request(path, { method = "GET", body, auth = false } = {}) {
    const headers = { "Content-Type": "application/json" };
    if (auth && getToken()) headers["Authorization"] = "Bearer " + getToken();

    const res = await fetch(base + path, {
      method,
      headers,
      body: body ? JSON.stringify(body) : undefined,
    });

    if (res.status === 401) { clear(); throw new Error("Session expirée, reconnectez-vous."); }

    const text = await res.text();
    const data = text ? JSON.parse(text) : null;
    if (!res.ok) throw new Error(data?.message || `Erreur ${res.status}`);
    return data;
  }

  return {
    getToken, getUser, setSession, clear,
    isLogged: () => !!getToken(),

    // Auth
    register: (dto) => request("/api/auth/register", { method: "POST", body: dto }),
    login: (dto) => request("/api/auth/login", { method: "POST", body: dto }),

    // Formations & catalogue
    formations: () => request("/api/formations"),
    formation: (id) => request(`/api/formations/${id}`),
    categories: () => request("/api/categories"),
    modules: (formationId) => request(`/api/modules/formation/${formationId}`),
    sessions: (formationId) => request(`/api/sessions/formation/${formationId}`),

    // Apprenant
    sinscrire: (sessionFormationId) =>
      request("/api/inscriptions", { method: "POST", auth: true, body: { sessionFormationId } }),
    mesInscriptions: () => request("/api/inscriptions/mes-inscriptions", { auth: true }),
    mesCertificats: () => request("/api/certificats/mes-certificats", { auth: true }),

    // Quiz
    quiz: (id) => request(`/api/quiz/${id}`, { auth: true }),
    soumettreQuiz: (quizId, reponses) =>
      request("/api/quiz/soumettre", { method: "POST", auth: true, body: { quizId, reponses } }),

    // Formateur IA
    genererVideo: (dto) =>
      request("/api/formateur-ia/videos/generer", { method: "POST", auth: true, body: dto }),
    videosFormation: (formationId) =>
      request(`/api/formateur-ia/videos/formation/${formationId}`),
    sessionTempsReel: (formationId, persona) =>
      request("/api/formateur-ia/session-temps-reel", {
        method: "POST", auth: true, body: { formationId, persona },
      }),

    // Admin
    adminStats: () => request("/api/admin/statistiques", { auth: true }),
  };
})();
