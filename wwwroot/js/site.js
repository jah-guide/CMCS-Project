// CMCS shared UI hooks (dashboards keep page-specific scripts in @section Scripts)
(function () {
  document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(function (el) {
    if (window.bootstrap && bootstrap.Tooltip) {
      new bootstrap.Tooltip(el);
    }
  });
})();
