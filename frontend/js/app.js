// Logique de l'application : navigation entre vues + rendu.
const el = (id) => document.getElementById(id);
const show = (id) => {
  document.querySelectorAll(".view").forEach((v) => v.classList.add("hidden"));
  el(id).classList.remove("hidden");
};
const escape = (s) => String(s ?? "").replace(/[&<>"]/g, (c) =>
  ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;" }[c]));

function toast(msg, type = "info") {
  const t = el("toast");
  t.textContent = msg;
  t.className = "toast " + type;
  t.classList.remove("hidden");
  setTimeout(() => t.classList.add("hidden"), 3500);
}

// ---------- Barre de navigation ----------
function refreshNav() {
  const user = API.getUser();
  el("nav-user").textContent = user ? `${user.email} (${user.role})` : "";
  el("btn-logout").classList.toggle("hidden", !user);
  el("nav-admin").classList.toggle("hidden", user?.role !== "Admin");
}

// ---------- Authentification ----------
function initAuth() {
  const tabs = document.querySelectorAll(".auth-tab");
  tabs.forEach((tab) => tab.addEventListener("click", () => {
    tabs.forEach((t) => t.classList.remove("active"));
    tab.classList.add("active");
    el("form-login").classList.toggle("hidden", tab.dataset.tab !== "login");
    el("form-register").classList.toggle("hidden", tab.dataset.tab !== "register");
  }));

  el("form-login").addEventListener("submit", async (e) => {
    e.preventDefault();
    try {
      const data = await API.login({
        email: el("login-email").value,
        motDePasse: el("login-password").value,
      });
      API.setSession(data);
      toast("Connecté avec succès", "success");
      afterLogin();
    } catch (err) { toast(err.message, "error"); }
  });

  el("form-register").addEventListener("submit", async (e) => {
    e.preventDefault();
    try {
      const data = await API.register({
        nom: el("reg-nom").value,
        prenom: el("reg-prenom").value,
        email: el("reg-email").value,
        motDePasse: el("reg-password").value,
        role: parseInt(el("reg-role").value, 10),
      });
      API.setSession(data);
      toast("Compte créé", "success");
      afterLogin();
    } catch (err) { toast(err.message, "error"); }
  });
}

function afterLogin() {
  refreshNav();
  loadCatalogue();
  show("view-catalogue");
}

// ---------- Catalogue ----------
async function loadCatalogue() {
  const grid = el("catalogue-grid");
  grid.innerHTML = "<p>Chargement…</p>";
  try {
    const formations = await API.formations();
    if (!formations.length) { grid.innerHTML = "<p>Aucune formation publiée.</p>"; return; }
    grid.innerHTML = formations.map((f) => `
      <article class="card formation-card" data-id="${f.id}">
        <span class="badge">${escape(f.categorieNom || "Sans catégorie")}</span>
        <h3>${escape(f.titre)}</h3>
        <p>${escape(f.description)}</p>
        <div class="card-meta">
          <span>${f.dureeHeures} h</span>
          <span>${f.prix} MAD</span>
          <span>${escape(f.formateurNom)}</span>
        </div>
        <button class="btn btn-primary btn-detail" data-id="${f.id}">Voir la formation</button>
      </article>`).join("");
    grid.querySelectorAll(".btn-detail").forEach((b) =>
      b.addEventListener("click", () => openFormation(parseInt(b.dataset.id, 10))));
  } catch (err) { grid.innerHTML = `<p class="error">${escape(err.message)}</p>`; }
}

// ---------- Détail formation ----------
async function openFormation(id) {
  show("view-detail");
  const box = el("detail-content");
  box.innerHTML = "<p>Chargement…</p>";
  try {
    const [f, modules, sessions] = await Promise.all([
      API.formation(id), API.modules(id), API.sessions(id),
    ]);
    window.__currentFormationId = id;

    const sessionsHtml = sessions.length ? sessions.map((s) => `
      <li>
        <strong>${escape(s.titre)}</strong> — ${new Date(s.dateDebut).toLocaleDateString()}
        (${s.placesMax} places)
        <button class="btn btn-small" data-session="${s.id}">S'inscrire</button>
      </li>`).join("") : "<li>Aucune session planifiée.</li>";

    const modulesHtml = modules.length ? modules.map((m) => `
      <div class="module">
        <h4>${escape(m.titre)}</h4>
        <p>${escape(m.description)}</p>
        <ul class="contenus">
          ${m.contenus.map((c) => `<li><span class="tag tag-${c.modalite.toLowerCase()}">${c.modalite}</span> ${escape(c.titre)}</li>`).join("")}
        </ul>
      </div>`).join("") : "<p>Programme à venir.</p>";

    box.innerHTML = `
      <button class="btn btn-small" id="btn-back">← Retour au catalogue</button>
      <span class="badge">${escape(f.categorieNom || "Sans catégorie")}</span>
      <h2>${escape(f.titre)}</h2>
      <p class="lead">${escape(f.description)}</p>
      <div class="detail-grid">
        <section>
          <h3>Sessions</h3>
          <ul class="sessions">${sessionsHtml}</ul>
          <h3>Programme</h3>
          ${modulesHtml}
        </section>
        <aside>
          <h3>🎓 Formateur IA</h3>
          <p>Discutez en direct avec l'avatar formateur de cette formation.</p>
          <video id="ia-video" class="ia-video" autoplay playsinline></video>
          <audio id="ia-audio" autoplay></audio>
          <button class="btn btn-primary" id="btn-ia-start">Démarrer le formateur IA</button>
          <div id="ia-chat" class="hidden">
            <input id="ia-msg" placeholder="Posez une question…" />
            <button class="btn btn-small" id="btn-ia-send">Envoyer</button>
          </div>
        </aside>
      </div>`;

    el("btn-back").addEventListener("click", () => show("view-catalogue"));
    box.querySelectorAll("[data-session]").forEach((b) =>
      b.addEventListener("click", () => inscrire(parseInt(b.dataset.session, 10))));
    el("btn-ia-start").addEventListener("click", () => demarrerIA(id));
  } catch (err) { box.innerHTML = `<p class="error">${escape(err.message)}</p>`; }
}

async function inscrire(sessionId) {
  if (!API.isLogged()) return toast("Connectez-vous pour vous inscrire.", "error");
  try {
    await API.sinscrire(sessionId);
    toast("Inscription confirmée !", "success");
  } catch (err) { toast(err.message, "error"); }
}

// ---------- Formateur IA temps réel ----------
async function demarrerIA(formationId) {
  if (!API.isLogged()) return toast("Connectez-vous pour utiliser le formateur IA.", "error");
  const btn = el("btn-ia-start");
  btn.disabled = true; btn.textContent = "Connexion…";
  try {
    await FormateurIA.demarrerSessionTempsReel(formationId, "ia-video", "ia-audio");
    btn.classList.add("hidden");
    el("ia-chat").classList.remove("hidden");
    el("btn-ia-send").addEventListener("click", async () => {
      const input = el("ia-msg");
      if (input.value.trim()) { await FormateurIA.envoyerMessage(input.value.trim()); input.value = ""; }
    });
    toast("Formateur IA connecté", "success");
  } catch (err) {
    toast("IA indisponible : " + err.message, "error");
    btn.disabled = false; btn.textContent = "Démarrer le formateur IA";
  }
}

// ---------- Admin ----------
async function loadAdmin() {
  show("view-admin");
  const box = el("admin-content");
  box.innerHTML = "<p>Chargement…</p>";
  try {
    const s = await API.adminStats();
    box.innerHTML = Object.entries(s).map(([k, v]) =>
      `<div class="stat"><span class="stat-value">${v}</span><span class="stat-label">${escape(k)}</span></div>`).join("");
  } catch (err) { box.innerHTML = `<p class="error">${escape(err.message)}</p>`; }
}

// ---------- Init ----------
document.addEventListener("DOMContentLoaded", () => {
  initAuth();
  refreshNav();

  el("btn-logout").addEventListener("click", () => {
    API.clear(); refreshNav(); show("view-auth");
    toast("Déconnecté", "info");
  });
  el("nav-catalogue").addEventListener("click", () => { loadCatalogue(); show("view-catalogue"); });
  el("nav-admin").addEventListener("click", loadAdmin);

  if (API.isLogged()) { afterLogin(); }
  else { show("view-auth"); }
  // Le catalogue est public : on peut le charger même sans connexion
  loadCatalogue();
});
