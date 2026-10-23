const i18n = {
  en: {
    "nav.what": "What it does",
    "nav.presets": "Presets",
    "nav.install": "Install",
    kicker: "BONELAB · Fusion · MelonLoader",
    lede: "A small open-source mod for when Fusion lobbies fill up with people and props and your FPS falls apart. Settings live in BoneMenu and stick between sessions.",
    "cta.download": "Download",
    "cta.source": "Source on GitHub",
    "photo.caption": "Not a render farm. Just a desk that has seen too many 20-player lobbies.",
    "what.title": "What it does",
    "what.1": "Culls distant prop renderers on your client so the GPU stops drawing junk across the map.",
    "what.2": "Puts far rigidbodies to sleep when they are not moving.",
    "what.3": "Cuts particles and shadows at range, and can adapt quality to your FPS.",
    "what.4": "If you are the host: optional DespawnAll cleanup on a timer or prop threshold.",
    "what.5": "Everything is toggled from BoneMenu and saved via MelonPreferences.",
    "presets.title": "Presets",
    "presets.sub": "Start with Crowded if the lobby is already a mess.",
    "presets.near": "Near detail",
    "presets.far": "Far cull",
    "presets.budget": "Prop budget",
    "install.title": "Install",
    "install.1": "MelonLoader for BONELAB",
    "install.2": "BoneLib + LabFusion",
    "install.3": "Drop FusOpt.dll into BONELAB/Mods",
    "install.4": "In-game: BoneMenu → FusOpt → Crowded",
    "note.title": "Quick notes",
    "note.body": "Client culling only changes what you render. Host cleanup wipes networked props for everyone — keep Auto Clean off unless you mean it. MIT license."
  },
  ru: {
    "nav.what": "Что делает",
    "nav.presets": "Пресеты",
    "nav.install": "Установка",
    kicker: "BONELAB · Fusion · MelonLoader",
    lede: "Небольшой open-source мод на случай, когда в Fusion-лобби набивается народ и пропы, а FPS разваливается. Настройки в BoneMenu и сохраняются между сессиями.",
    "cta.download": "Скачать",
    "cta.source": "Исходники на GitHub",
    "photo.caption": "Не рендер-ферма. Просто стол, который видел слишком много лобби на 20 человек.",
    "what.title": "Что делает",
    "what.1": "Локально выключает рендереры дальних пропов, чтобы GPU не рисовал мусор через всю карту.",
    "what.2": "Усыпляет дальние rigidbody, если они не двигаются.",
    "what.3": "Режет партиклы и тени на дистанции и может подстраивать качество под FPS.",
    "what.4": "Если вы хост: опциональная очистка DespawnAll по таймеру или порогу пропов.",
    "what.5": "Всё переключается из BoneMenu и сохраняется через MelonPreferences.",
    "presets.title": "Пресеты",
    "presets.sub": "Если лобби уже каша — начинайте с Crowded.",
    "presets.near": "Ближняя детализация",
    "presets.far": "Дальний калл",
    "presets.budget": "Бюджет пропов",
    "install.title": "Установка",
    "install.1": "MelonLoader для BONELAB",
    "install.2": "BoneLib + LabFusion",
    "install.3": "Положите FusOpt.dll в BONELAB/Mods",
    "install.4": "В игре: BoneMenu → FusOpt → Crowded",
    "note.title": "Коротко",
    "note.body": "Клиентский калл меняет только то, что видите вы. Очистка хоста сносит сетевые пропы у всех — Auto Clean лучше не включать просто так. Лицензия MIT."
  }
};

const presets = {
  balanced: {
    near: 24, far: 70, budget: 120,
    note: {
      en: "Everyday play. Softer distances, more props kept nearby.",
      ru: "Обычная игра. Мягче дистанции, больше пропов рядом."
    }
  },
  performance: {
    near: 20, far: 60, budget: 90,
    note: {
      en: "More aggressive, adaptive quality on.",
      ru: "Агрессивнее, с адаптивным качеством."
    }
  },
  crowded: {
    near: 16, far: 48, budget: 70,
    note: {
      en: "Good default for 15–30 player chaos.",
      ru: "Нормальный дефолт для хаоса на 15–30 игроков."
    }
  },
  extreme: {
    near: 12, far: 35, budget: 40,
    note: {
      en: "When the lobby is already on fire.",
      ru: "Когда лобби уже горит."
    }
  }
};

let lang = localStorage.getItem("fusopt-lang") || "en";

function applyLang() {
  document.documentElement.lang = lang;
  document.querySelectorAll("[data-i18n]").forEach((el) => {
    const key = el.getAttribute("data-i18n");
    if (i18n[lang][key]) el.textContent = i18n[lang][key];
  });
  document.querySelectorAll(".lang button").forEach((btn) => {
    btn.classList.toggle("active", btn.dataset.lang === lang);
  });
  const active = document.querySelector(".preset.active");
  if (active) showPreset(active.dataset.preset);
}

function showPreset(name) {
  const p = presets[name];
  if (!p) return;
  document.getElementById("vNear").textContent = `${p.near} m`;
  document.getElementById("vFar").textContent = `${p.far} m`;
  document.getElementById("vBudget").textContent = String(p.budget);
  document.getElementById("presetNote").textContent = p.note[lang];
}

document.querySelectorAll(".lang button").forEach((btn) => {
  btn.addEventListener("click", () => {
    lang = btn.dataset.lang;
    localStorage.setItem("fusopt-lang", lang);
    applyLang();
  });
});

document.querySelectorAll(".preset").forEach((btn) => {
  btn.addEventListener("click", () => {
    document.querySelectorAll(".preset").forEach((b) => b.classList.remove("active"));
    btn.classList.add("active");
    showPreset(btn.dataset.preset);
  });
});

applyLang();
showPreset("crowded");
