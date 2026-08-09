/* =====================================================================
   Autohaus — vanilla JS. One function per feature. Guard clauses.
   ===================================================================== */
(function () {
  "use strict";

  var prefersReduced = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

  /* ---------- Sticky navbar ---------- */
  function initStickyNav() {
    var nav = document.querySelector(".ah-nav");
    if (!nav) return;
    var onScroll = function () {
      nav.classList.toggle("scrolled", window.scrollY > 24);
    };
    onScroll();
    window.addEventListener("scroll", onScroll, { passive: true });
  }

  /* ---------- Mobile navigation ---------- */
  function initMobileNav() {
    var burger = document.querySelector(".ah-burger");
    var menu = document.querySelector(".ah-mobile-menu");
    var overlay = document.querySelector(".ah-overlay");
    if (!burger || !menu) return;

    var open = function () {
      menu.classList.add("open");
      if (overlay) overlay.classList.add("open");
      burger.classList.add("open");
      burger.setAttribute("aria-expanded", "true");
      document.body.classList.add("ah-lock");
    };
    var close = function () {
      menu.classList.remove("open");
      if (overlay) overlay.classList.remove("open");
      burger.classList.remove("open");
      burger.setAttribute("aria-expanded", "false");
      document.body.classList.remove("ah-lock");
    };
    var toggle = function () {
      menu.classList.contains("open") ? close() : open();
    };

    burger.addEventListener("click", toggle);
    if (overlay) overlay.addEventListener("click", close);
    menu.querySelectorAll("a").forEach(function (a) {
      a.addEventListener("click", close);
    });
    document.addEventListener("keydown", function (e) {
      if (e.key === "Escape" && menu.classList.contains("open")) close();
    });
  }

  /* ---------- Scroll reveal ---------- */
  function initReveal() {
    var els = document.querySelectorAll(".reveal");
    if (!els.length) return;
    if (prefersReduced || !("IntersectionObserver" in window)) {
      els.forEach(function (el) { el.classList.add("in"); });
      return;
    }
    var io = new IntersectionObserver(function (entries) {
      entries.forEach(function (entry) {
        if (entry.isIntersecting) {
          entry.target.classList.add("in");
          io.unobserve(entry.target);
        }
      });
    }, { threshold: 0.14, rootMargin: "0px 0px -40px 0px" });
    els.forEach(function (el) { io.observe(el); });
  }

  /* ---------- Count-up stats ---------- */
  function initCountUp() {
    var nums = document.querySelectorAll("[data-count]");
    if (!nums.length) return;
    if (prefersReduced || !("IntersectionObserver" in window)) {
      nums.forEach(function (n) { n.textContent = formatCount(n); });
      return;
    }
    var run = function (el) {
      var target = parseFloat(el.getAttribute("data-count"));
      var decimals = (el.getAttribute("data-count").split(".")[1] || "").length;
      var dur = 1400, start = null;
      var step = function (ts) {
        if (!start) start = ts;
        var p = Math.min((ts - start) / dur, 1);
        var eased = 1 - Math.pow(1 - p, 3);
        el.textContent = (target * eased).toLocaleString("en-US", {
          minimumFractionDigits: decimals, maximumFractionDigits: decimals
        });
        if (p < 1) requestAnimationFrame(step);
        else el.textContent = target.toLocaleString("en-US", {
          minimumFractionDigits: decimals, maximumFractionDigits: decimals
        });
      };
      requestAnimationFrame(step);
    };
    var formatCount = function (el) {
      return parseFloat(el.getAttribute("data-count")).toLocaleString("en-US");
    };
    var io = new IntersectionObserver(function (entries) {
      entries.forEach(function (entry) {
        if (entry.isIntersecting) { run(entry.target); io.unobserve(entry.target); }
      });
    }, { threshold: 0.5 });
    nums.forEach(function (n) { io.observe(n); });
  }

  /* ---------- Inventory filter ---------- */
  function initInventoryFilter() {
    var group = document.querySelector("[data-filter-group]");
    if (!group) return;
    var buttons = group.querySelectorAll(".ah-filter-btn");
    var grid = document.querySelector("[data-inv-grid]");
    if (!grid) return;
    var cards = grid.querySelectorAll(".ah-card");
    var empty = grid.querySelector(".ah-empty");
    var countEl = document.querySelector("[data-inv-count]");

    var apply = function (filter) {
      var shown = 0;
      cards.forEach(function (card) {
        var cats = (card.getAttribute("data-category") || "").split(" ");
        var match = filter === "all" || cats.indexOf(filter) !== -1;
        card.classList.toggle("is-hidden", !match);
        if (match) shown++;
      });
      if (empty) empty.style.display = shown === 0 ? "" : "none";
      if (countEl) countEl.textContent = shown;
    };

    buttons.forEach(function (btn) {
      btn.addEventListener("click", function () {
        buttons.forEach(function (b) { b.setAttribute("aria-pressed", "false"); });
        btn.setAttribute("aria-pressed", "true");
        apply(btn.getAttribute("data-filter"));
      });
    });
  }

  /* ---------- Inventory sort (inventory page) ---------- */
  function initInventorySort() {
    var select = document.querySelector("[data-sort]");
    var grid = document.querySelector("[data-inv-grid]");
    if (!select || !grid) return;
    var cards = Array.prototype.slice.call(grid.querySelectorAll(".ah-card"));

    select.addEventListener("change", function () {
      var mode = select.value;
      var sorted = cards.slice().sort(function (a, b) {
        var an, bn;
        if (mode === "price-asc" || mode === "price-desc") {
          an = +a.getAttribute("data-price"); bn = +b.getAttribute("data-price");
        } else if (mode === "year-desc") {
          an = +b.getAttribute("data-year"); bn = +a.getAttribute("data-year");
        } else if (mode === "mileage-asc") {
          an = +a.getAttribute("data-mileage"); bn = +b.getAttribute("data-mileage");
        } else {
          return 0;
        }
        if (mode === "price-desc") return bn - an;
        return an - bn;
      });
      sorted.forEach(function (c) { grid.appendChild(c); });
    });
  }

  /* ---------- Hero search -> redirect to inventory ---------- */
  function initSearchForm() {
    var form = document.querySelector("[data-search-form]");
    if (!form) return;
    form.addEventListener("submit", function (e) {
      e.preventDefault();
      var data = new FormData(form);
      var params = new URLSearchParams();
      ["make", "model", "maxprice", "condition"].forEach(function (k) {
        var v = data.get(k);
        if (v) params.set(k, v);
      });
      window.location.href = "inventory.html" + (params.toString() ? "?" + params.toString() : "");
    });
  }

  /* Pre-select filter on inventory page from URL condition param */
  function initFilterFromQuery() {
    var group = document.querySelector("[data-filter-group]");
    if (!group) return;
    var params = new URLSearchParams(window.location.search);
    var cond = params.get("condition");
    if (!cond) return;
    var map = { new: "New", used: "Used", electric: "Electric", suv: "SUV", sedan: "Sedan" };
    var target = map[cond.toLowerCase()] || cond;
    var btn = group.querySelector('[data-filter="' + target + '"]');
    if (btn) btn.click();
  }

  /* ---------- Finance calculator ---------- */
  function initFinanceCalc() {
    var calc = document.querySelector("[data-calc]");
    if (!calc) return;
    var price = calc.querySelector("[data-calc-price]");
    var down = calc.querySelector("[data-calc-down]");
    var term = calc.querySelector("[data-calc-term]");
    var apr = calc.querySelector("[data-calc-apr]");
    var monthlyOut = calc.querySelector("[data-calc-monthly]");
    var financedOut = calc.querySelector("[data-calc-financed]");
    var interestOut = calc.querySelector("[data-calc-interest]");
    var totalOut = calc.querySelector("[data-calc-total]");

    var fmt = function (n) {
      return "$" + Math.round(n).toLocaleString("en-US");
    };
    var setLabel = function (input) {
      var lbl = calc.querySelector('[data-val="' + input.getAttribute("id") + '"]');
      if (!lbl) return;
      var v = +input.value;
      if (input.getAttribute("data-fmt") === "money") lbl.textContent = fmt(v);
      else if (input.getAttribute("data-fmt") === "months") lbl.textContent = v + " mo";
      else if (input.getAttribute("data-fmt") === "pct") lbl.textContent = v.toFixed(1) + "%";
      else lbl.textContent = v;
    };

    var compute = function () {
      var P = +price.value - +down.value;
      if (P < 0) P = 0;
      var n = +term.value;
      var r = (+apr.value) / 100 / 12;
      var m;
      if (r === 0) m = P / n;
      else m = (P * r) / (1 - Math.pow(1 + r, -n));
      var total = m * n;
      var interest = total - P;

      if (monthlyOut) monthlyOut.textContent = Math.round(m).toLocaleString("en-US");
      if (financedOut) financedOut.textContent = fmt(P);
      if (interestOut) interestOut.textContent = fmt(interest);
      if (totalOut) totalOut.textContent = fmt(total + +down.value);
    };

    [price, down, term, apr].forEach(function (input) {
      if (!input) return;
      setLabel(input);
      input.addEventListener("input", function () { setLabel(input); compute(); });
    });
    compute();
  }

  /* ---------- Lightbox ---------- */
  function initLightbox() {
    var triggers = document.querySelectorAll("[data-lightbox]");
    if (!triggers.length) return;
    var lb = document.querySelector(".ah-lightbox");
    if (!lb) return;
    var img = lb.querySelector("img");
    var cap = lb.querySelector(".ah-lightbox-caption");
    var closeBtn = lb.querySelector(".ah-lightbox-close");
    var lastFocused = null;

    var open = function (src, alt) {
      lastFocused = document.activeElement;
      img.src = src;
      img.alt = alt || "";
      if (cap) cap.textContent = alt || "";
      lb.classList.add("open");
      document.body.classList.add("ah-lock");
      if (closeBtn) closeBtn.focus();
    };
    var close = function () {
      lb.classList.remove("open");
      document.body.classList.remove("ah-lock");
      img.src = "";
      if (lastFocused) lastFocused.focus();
    };

    triggers.forEach(function (t) {
      t.addEventListener("click", function () {
        var src = t.getAttribute("data-lightbox");
        var alt = t.getAttribute("data-lightbox-alt") || t.querySelector("img") && t.querySelector("img").alt;
        open(src, alt);
      });
      t.addEventListener("keydown", function (e) {
        if (e.key === "Enter" || e.key === " ") { e.preventDefault(); t.click(); }
      });
    });
    if (closeBtn) closeBtn.addEventListener("click", close);
    lb.addEventListener("click", function (e) { if (e.target === lb) close(); });
    document.addEventListener("keydown", function (e) {
      if (e.key === "Escape" && lb.classList.contains("open")) close();
    });
  }

  /* ---------- Booking form validation ---------- */
  function initBookingForm() {
    var form = document.querySelector("[data-booking-form]");
    if (!form) return;
    var success = form.querySelector(".ah-form-success");

    var showError = function (field, on) {
      field.classList.toggle("invalid", on);
      field.setAttribute("aria-invalid", on ? "true" : "false");
      var err = field.parentElement.querySelector(".ah-error");
      if (err) err.classList.toggle("show", on);
    };

    var validateField = function (field) {
      var val = field.value.trim();
      var ok = true;
      if (field.hasAttribute("required") && !val) ok = false;
      if (field.type === "email" && val && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(val)) ok = false;
      if (field.type === "tel" && val && val.replace(/\D/g, "").length < 7) ok = false;
      showError(field, !ok);
      return ok;
    };

    var fields = form.querySelectorAll("input[required], select[required], textarea[required], input[type=email], input[type=tel]");
    fields.forEach(function (f) {
      f.addEventListener("blur", function () { validateField(f); });
      f.addEventListener("input", function () { if (f.classList.contains("invalid")) validateField(f); });
    });

    form.addEventListener("submit", function (e) {
      e.preventDefault();
      var allOk = true;
      fields.forEach(function (f) { if (!validateField(f)) allOk = false; });
      if (!allOk) {
        var firstBad = form.querySelector(".invalid");
        if (firstBad) firstBad.focus();
        return;
      }
      if (success) {
        success.classList.add("show");
        success.setAttribute("role", "status");
      }
      form.reset();
      setTimeout(function () { if (success) success.classList.remove("show"); }, 6000);
    });
  }

  /* ---------- Footer year ---------- */
  function initYear() {
    var el = document.querySelector("[data-year]");
    if (el) el.textContent = new Date().getFullYear();
  }

  /* ---------- Boot ---------- */
  function boot() {
    initStickyNav();
    initMobileNav();
    initReveal();
    initCountUp();
    initInventoryFilter();
    initInventorySort();
    initSearchForm();
    initFilterFromQuery();
    initFinanceCalc();
    initLightbox();
    initBookingForm();
    initYear();
  }

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", boot);
  } else {
    boot();
  }
})();
