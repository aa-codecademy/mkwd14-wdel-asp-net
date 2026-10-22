// JavaScript for the CMS (every page that uses the Administration _Layout)

// Settings shared by EVERY DataTable in the CMS. Each Index view only adds its own ajax URL and columns.
// DataTables 3 needs no jQuery: dataTables.min.js adds a global "DataTable". Manual: https://datatables.net/manual/index
Object.assign(DataTable.defaults, {
    // Server-side processing: the SERVER pages, searches and sorts (see DataTableRequestViewModel),
    // so the browser never downloads the whole table
    serverSide: true,

    // Which parts to show, and where: "Showing 1 to 10 of 57 rows" above the table, the rows per page and the pager below it.
    // topEnd is null: no DataTables search box, we use our own (#inputSearch) above the table.
    layout: {
        topStart: 'info',
        topEnd: null,
        bottomStart: 'pageLength',
        bottomEnd: 'paging'
    }
});

// Only our own texts: the others (the screen reader labels, "entries", ...) keep their defaults
Object.assign(DataTable.defaults.language, {
    emptyTable: 'No data available',
    info: 'Showing _START_ to _END_ of _TOTAL_ rows',
    infoEmpty: 'Showing 0 to 0 of 0 rows',
    infoFiltered: '(filtered from _MAX_ rows)',
    lengthMenu: 'Show _MENU_ rows',
    zeroRecords: 'No matching rows found',
    paginate: {
        first: 'First',
        last: 'Last',
        next: 'Next',
        previous: 'Previous'
    }
});

// A click on a column title switches between ascending and descending.
// (The default has a third click that removes the sort: the server then sorts by its default, the Id.)
DataTable.defaults.column.orderSequence = ['asc', 'desc'];

// Reloads the table (page 1, new search) when the user presses Enter in #inputSearch or clicks #btnSearch.
// The table's ajax "data" function reads #inputSearch and sends it as search[value].
function bindTableSearch(table) {
    $('#inputSearch').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            table.ajax.reload();
        }
    });

    $('#btnSearch').on('click', function () {
        table.ajax.reload();
    });
}

// The HTML of one icon link in the actions column, e.g. actionLink('/Administration/Products/Edit/5', 'edit', 'text-warning')
function actionLink(url, icon, cssClass) {
    return '<a class="action-button ' + cssClass + '" href="' + url + '"><i class="material-icons">' + icon + '</i></a>';
}
