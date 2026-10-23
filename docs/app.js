const i18n = {
  en: {
    brand: "FusOpt",
    "nav.features": "Features",
    "nav.presets": "Presets",
    "nav.install": "Install",
    "hero.eyebrow": "Open source · BONELAB · Fusion",
    "hero.title": "FusOpt",
    "hero.lede": "Keep crowded Fusion lobbies playable. Cull distant props, sleep physics, adapt quality — all from BoneMenu with settings that stick.",
    "hero.ctaPrimary": "Get the mod",
    "hero.ctaSecondary": "View source",
    "metrics.players": "player lobbies targeted",
    "metrics.presets": "ready presets",
    "metrics.open": "open source MIT",
    "features.title": "Built for chaos lobbies",
    "features.sub": "Client relief first. Host cleanup when you run the server.",
    "features.prop.title": "Prop culling",
    "features.prop.body": "Distant clutter renderers turn off locally so your GPU stops drawing junk across the map.",
    "features.phys.title": "Physics sleep",
    "features.phys.body": "Far rigidbodies sleep instead of waking every frame when 30 people yeet crates.",
    "features.adapt.title": "Adaptive quality",
    "features.adapt.body": "Shadow distance and pixel lights scale with your FPS target automatically.",
    "features.host.title": "Host cleanup",
    "features.host.body": "Optional DespawnAll on interval or prop threshold. Memory clean after sweep.",
    "features.menu.title": "BoneMenu + save",
    "features.menu.body": "Every toggle lives in BoneMenu and persists through MelonPreferences.",
    "features.open.title": "Truly open",
    "features.open.body": "MIT licensed. Fork it, patch it, ship improvements back to the community.",
    "presets.title": "Pick a pressure level",
    "presets.sub": "Click a preset to preview distances and budgets.",
    "presets.balanced": "Balanced",
    "presets.performance": "Performance",
    "presets.crowded": "Crowded",
    "presets.extreme": "Extreme",
    "presets.near": "Near detail",
    "presets.far": "Far cull",
    "presets.budget": "Prop budget",
    "install.title": "Install in minutes",
    "install.sub": "MelonLoader stack. No wizard required.",
    "install.s1": "Install MelonLoader for BONELAB.",
    "install.s2": "Install BoneLib and LabFusion.",
    "install.s3": "Drop FusOpt.dll into BONELAB/Mods (or install via Thunderstore).",
    "install.s4": "Launch → BoneMenu → FusOpt → Crowded preset.",
    "band.title": "Ship frames. Keep the lobby.",
    "band.sub": "FusOpt is free, open source, and built for the servers that break first.",
    "band.cta": "Star on GitHub"
  },
  ru: {
    brand: "FusOpt",
    "nav.features": "Возможности",
    "nav.presets": "Пресеты",
    "nav.install": "Установка",
    "hero.eyebrow": "Open source · BONELAB · Fusion",
    "hero.title": "FusOpt",
    "hero.lede": "Держите переполненные Fusion-лобби играбельными. Калл дальних пропов, сон физики, адаптивное качество — всё из BoneMenu с сохранением настроек.",
    "hero.ctaPrimary": "Скачать мод",
    "hero.ctaSecondary": "Исходники",
    "metrics.players": "игроков в целевых лобби",
    "metrics.presets": "готовых пресета",
    "metrics.open": "открытый код MIT",
    "features.title": "Сделано для хаос-лобби",
    "features.sub": "Сначала облегчение клиенту. Очистка хоста — когда сервер ваш.",
    "features.prop.title": "Калл пропов",
    "features.prop.body": "Далёкий мусор локально отключает рендереры — GPU перестаёт рисовать хлам через всю карту.",
    "features.phys.title": "Сон физики",
    "features.phys.body": "Далёкие rigidbody засыпают вместо пробуждения каждый кадр, когда 30 человек швыряют ящики.",
    "features.adapt.title": "Адаптивное качество",
    "features.adapt.body": "Дистанция теней и число пиксельных ламп подстраиваются под ваш целевой FPS.",
    "features.host.title": "Очистка хоста",
    "features.host.body": "Опциональный DespawnAll по таймеру или порогу пропов. После — чистка памяти.",
    "features.menu.title": "BoneMenu + сейв",
    "features.menu.body": "Каждый тумблер в BoneMenu и сохраняется через MelonPreferences.",
    "features.open.title": "По-настоящему open source",
    "features.open.body": "Лицензия MIT. Форкайте, патчьте, возвращайте улучшения сообществу.",
    "presets.title": "Выберите уровень давления",
    "presets.sub": "Нажмите пресет, чтобы увидеть дистанции и лимиты.",
    "presets.balanced": "Balanced",
    "presets.performance": "Performance",
    "presets.crowded": "Crowded",
    "presets.extreme": "Extreme",
    "presets.near": "Ближняя детализация",
    "presets.far": "Дальний калл",
    "presets.budget": "Бюджет пропов",
    "install.title": "Установка за минуты",
    "install.sub": "Стек MelonLoader. Без мастеров установки.",
    "install.s1": "Установите MelonLoader для BONELAB.",
    "install.s2": "Установите BoneLib и LabFusion.",
    "install.s3": "Положите FusOpt.dll в BONELAB/Mods (или через Thunderstore).",
    "install.s4": "Запуск → BoneMenu → FusOpt → пресет Crowded.",
    "band.title": "Верните кадры. Оставьте лобби.",
    "band.sub": "FusOpt бесплатен, открыт и заточен под серверы, которые ломаются первыми.",
    "band.cta": "Звезда на GitHub"
  }
};

const presetData = {
  balanced: {
    near: 24, far: 70, budget: 120,
    note: {
      en: "Everyday play — softer cull distances, higher prop budget.",
      ru: "Повседневная игра — мягче дистанции калла, выше бюджет пропов."
    }
  },
  performance: {
    near: 20, far: 60, budget: 90,
    note: {
      en: "FPS-first with adaptive quality enabled.",
      ru: "Упор на FPS с включённым адаптивным качеством."
    }
  },
  crowded: {
    near: 16, far: 48, budget: 70,
    note: {
      en: "Default for 15–30 player Fusion chaos.",
      ru: "По умолчанию для хаоса Fusion на 15–30 игроков."
    }
  },
  extreme: {
    near: 12, far: 35, budget: 40,
    note: {
      en: "Survival mode for potato PCs and prop apocalypse lobbies.",
      ru: "Режим выживания для слабых ПК и апокалипсиса пропов."
    }
  }
};

let lang = localStorage.getItem("fusopt-lang") || "en";

function applyLang() {
  document.documentElement.lang = lang;
  document.querySelectorAll("[data-i18n]").forEach((el) => {
    const key = el.getAttribute("data-i18n");
    const value = i18n[lang][key];
    if (value) el.textContent = value;
  });
  document.querySelectorAll(".lang button").forEach((btn) => {
    btn.classList.toggle("active", btn.dataset.lang === lang);
  });
  const active = document.querySelector(".preset-btn.active");
  if (active) showPreset(active.dataset.preset);
}

function showPreset(name) {
  const p = presetData[name];
  if (!p) return;
  document.getElementById("barNear").style.width = `${(p.near / 70) * 100}%`;
  document.getElementById("barFar").style.width = `${(p.far / 120) * 100}%`;
  document.getElementById("barBudget").style.width = `${(p.budget / 120) * 100}%`;
  document.getElementById("presetNote").textContent = p.note[lang];
}

function animateCounts() {
  document.querySelectorAll(".count").forEach((el) => {
    const target = Number(el.dataset.count || 0);
    const start = performance.now();
    const dur = 1100;
    const tick = (now) => {
      const t = Math.min(1, (now - start) / dur);
      const eased = 1 - Math.pow(1 - t, 3);
      el.textContent = String(Math.round(target * eased));
      if (t < 1) requestAnimationFrame(tick);
    };
    requestAnimationFrame(tick);
  });
}

function observeFeatures() {
  const io = new IntersectionObserver(
    (entries) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting) entry.target.classList.add("visible");
      });
    },
    { threshold: 0.2 }
  );
  document.querySelectorAll(".feature").forEach((el, i) => {
    el.style.transitionDelay = `${i * 60}ms`;
    io.observe(el);
  });
}

document.querySelectorAll(".lang button").forEach((btn) => {
  btn.addEventListener("click", () => {
    lang = btn.dataset.lang;
    localStorage.setItem("fusopt-lang", lang);
    applyLang();
  });
});

document.querySelectorAll(".preset-btn").forEach((btn) => {
  btn.addEventListener("click", () => {
    document.querySelectorAll(".preset-btn").forEach((b) => b.classList.remove("active"));
    btn.classList.add("active");
    showPreset(btn.dataset.preset);
  });
});

applyLang();
showPreset("crowded");
animateCounts();
observeFeatures();
