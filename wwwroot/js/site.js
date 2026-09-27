// CMCS shared UI hooks (dashboards keep page-specific scripts in @section Scripts)

(function () {

  document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(function (el) {

    if (window.bootstrap && bootstrap.Tooltip) {

      new bootstrap.Tooltip(el);

    }

  });



  document.querySelectorAll('[data-cmcs-autodismiss="true"]').forEach(function (el) {

    window.setTimeout(function () {

      if (window.bootstrap && bootstrap.Alert) {

        bootstrap.Alert.getOrCreateInstance(el).close();

      }

    }, 6000);

  });



  initClaimStatusFilter();

  initQueueFilter();

  initDatatableSearchInputs();



  function initClaimStatusFilter() {

    var table = document.getElementById('lecturerClaimsTable');

    var toolbar = document.getElementById('claimStatusFilter');

    if (!table || !toolbar) {

      return;

    }



    var rows = table.querySelectorAll('tbody tr[data-claim-status]');

    var emptyRow = document.getElementById('claimFilterEmpty');



    toolbar.querySelectorAll('[data-filter-status]').forEach(function (btn) {

      btn.addEventListener('click', function () {

        var status = btn.getAttribute('data-filter-status');

        toolbar.querySelectorAll('[data-filter-status]').forEach(function (b) {

          b.classList.toggle('active', b === btn);

          b.setAttribute('aria-pressed', b === btn ? 'true' : 'false');

        });



        var visible = 0;

        rows.forEach(function (row) {

          var match = status === 'all' || row.getAttribute('data-claim-status') === status;

          row.classList.toggle('d-none', !match);

          if (match) {

            visible++;

          }

        });



        if (emptyRow) {

          emptyRow.classList.toggle('d-none', visible > 0);

        }

      });

    });

  }



  function initQueueFilter() {

    var table = document.getElementById('claimsTable');

    var toolbar = document.getElementById('coordinatorPriorityFilter');

    if (!table || !toolbar) {

      return;

    }



    var rows = table.querySelectorAll('tbody tr[data-priority]');



    toolbar.querySelectorAll('[data-filter-priority]').forEach(function (btn) {

      btn.addEventListener('click', function () {

        var priority = btn.getAttribute('data-filter-priority');

        toolbar.querySelectorAll('[data-filter-priority]').forEach(function (b) {

          b.classList.toggle('active', b === btn);

        });



        rows.forEach(function (row) {

          var match = priority === 'all' || row.getAttribute('data-priority') === priority;

          row.classList.toggle('d-none', !match);

        });

      });

    });

  }

  function initDatatableSearchInputs() {
    document.querySelectorAll('[data-cmcs-datatable]').forEach(function (input) {
      var tableId = input.getAttribute('data-cmcs-datatable');
      input.addEventListener('keyup', function () {
        if (!window.jQuery || !$.fn.dataTable) {
          return;
        }
        var selector = '#' + tableId;
        if ($.fn.dataTable.isDataTable(selector)) {
          $(selector).DataTable().search(input.value).draw();
        }
      });
    });
  }

})();

