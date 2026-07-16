(function () {
  'use strict';

  const BloodCenter = {
    init() {
      this.initTheme();
      this.initSidebar();
      this.initDropdowns();
      this.initSearch();
      this.initNotifications();
      this.initTooltips();
      this.initMobileNav();
    },

    initTheme() {
      const saved = localStorage.getItem('bc-theme') || 'light';
      this.setTheme(saved);
      document.querySelectorAll('[data-bc-theme-toggle]').forEach(btn => {
        btn.addEventListener('click', () => {
          const current = document.documentElement.getAttribute('data-theme') || 'light';
          const next = current === 'dark' ? 'light' : 'dark';
          this.setTheme(next);
        });
      });
    },

    setTheme(theme) {
      document.documentElement.setAttribute('data-theme', theme);
      localStorage.setItem('bc-theme', theme);
      document.querySelectorAll('[data-bc-theme-icon]').forEach(el => {
        el.className = theme === 'dark' ? 'bi bi-sun-fill' : 'bi bi-moon-fill';
      });
    },

    initSidebar() {
      this.sidebar = document.getElementById('sidebar');
      this.sidebarOverlay = document.getElementById('sidebar-overlay');

      document.querySelectorAll('[data-bc-sidebar-toggle]').forEach(btn => {
        btn.addEventListener('click', (e) => {
          e.stopPropagation();
          const isMobile = window.innerWidth <= 768;
          if (isMobile) {
            this.sidebar.classList.toggle('mobile-open');
            if (this.sidebarOverlay) this.sidebarOverlay.classList.toggle('show');
          } else {
            this.sidebar.classList.toggle('collapsed');
          }
        });
      });

      if (this.sidebarOverlay) {
        this.sidebarOverlay.addEventListener('click', () => {
          this.sidebar.classList.remove('mobile-open');
          this.sidebarOverlay.classList.remove('show');
        });
      }

      document.querySelectorAll('.sidebar-nav .nav-link[data-bc-toggle="collapse"]').forEach(link => {
        link.addEventListener('click', (e) => {
          e.preventDefault();
          const target = document.getElementById(link.getAttribute('data-bc-target'));
          if (!target) return;
          const expanded = link.getAttribute('aria-expanded') === 'true';
          link.setAttribute('aria-expanded', !expanded);
          target.style.maxHeight = expanded ? '0' : target.scrollHeight + 'px';
        });
      });
    },

    initDropdowns() {
      document.addEventListener('click', (e) => {
        const toggle = e.target.closest('[data-bc-dropdown-toggle]');
        if (toggle) {
          e.preventDefault();
          e.stopPropagation();
          const menu = document.getElementById(toggle.getAttribute('data-bc-target'));
          if (!menu) return;
          const isOpen = menu.classList.contains('show');
          document.querySelectorAll('.dropdown-menu.show').forEach(m => m.classList.remove('show'));
          if (!isOpen) menu.classList.add('show');
        } else if (!e.target.closest('.dropdown-menu')) {
          document.querySelectorAll('.dropdown-menu.show').forEach(m => m.classList.remove('show'));
        }
      });
    },

    initSearch() {
      const searchInput = document.getElementById('header-search');
      if (!searchInput) return;
      searchInput.addEventListener('input', function () {
        const query = this.value.trim().toLowerCase();
        if (query.length < 2) return;
        const results = [];
        document.querySelectorAll('.sidebar-nav .nav-link .nav-text').forEach(el => {
          if (el.textContent.toLowerCase().includes(query)) {
            results.push(el.textContent.trim());
          }
        });
        console.log('[BloodCenter] Search:', query, results);
      });
    },

    initNotifications() {
      document.querySelectorAll('[data-bc-notifications]').forEach(el => {
        el.addEventListener('click', () => {
          const badge = el.querySelector('.badge-dot');
          if (badge) badge.style.display = 'none';
        });
      });
    },

    initTooltips() {
      document.querySelectorAll('[data-bc-tooltip]').forEach(el => {
        el.addEventListener('mouseenter', function () {
          const text = this.getAttribute('data-bc-tooltip');
          if (!text) return;
          const tip = document.createElement('div');
          tip.className = 'bc-tooltip';
          tip.textContent = text;
          document.body.appendChild(tip);
          const rect = this.getBoundingClientRect();
          tip.style.top = (rect.top - tip.offsetHeight - 6) + 'px';
          tip.style.left = (rect.left + rect.width / 2 - tip.offsetWidth / 2) + 'px';
          this._bcTip = tip;
        });
        el.addEventListener('mouseleave', function () {
          if (this._bcTip) { this._bcTip.remove(); this._bcTip = null; }
        });
      });
    },

    initMobileNav() {
      document.querySelectorAll('.m-nav-item').forEach(item => {
        item.addEventListener('click', function () {
          document.querySelectorAll('.m-nav-item').forEach(i => i.classList.remove('active'));
          this.classList.add('active');
        });
      });
    }
  };

  document.addEventListener('DOMContentLoaded', () => BloodCenter.init());
  window.BloodCenter = BloodCenter;
})();
