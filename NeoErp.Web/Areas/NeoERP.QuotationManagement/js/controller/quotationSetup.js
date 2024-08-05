QMModule.controller('quotationSetup', function ($scope, $rootScope, $http, $filter, $timeout, $route, $window) {
    $scope.BackFromMenu = function () {
        $window.location.href = '/QuotationManagement/Home/Dashboard';
    };
    $scope.grid = {
        dataSource: {
            type: "json",
            transport: {
                read: {
                    url: window.location.protocol + "//" + window.location.host + "/api/QuotationApi/ListAllTenders",
                    dataType: "json",
                    type: "GET",
                    contentType: "application/json; charset=utf-8"
                },
                parameterMap: function (options, type) {
                    var paramMap = JSON.stringify($.extend(options));
                    return paramMap;
                }
            },
            error: function (e) {
                displayPopupNotification("Sorry, an error occurred while processing data", "error");
            },
            schema: {
                model: {
                    fields: {
                        TENDER_NO: { type: "string" },
                        ISSUE_DATE: { type: "date" },
                        CREATED_DATE: { type: "date" },
                        VALID_DATE: { type: "date" },
                        APPROVED_STATUS: { type: "string" }
                    }
                }
            },
            pageSize: 50
        },
        toolbar: kendo.template($("#toolbar-template").html()),
        excel: {
            fileName: "Quotation Setup",
            allPages: true
        },
        pdf: {
            fileName: "Received Schedule",
            allPages: true,
            avoidLinks: true,
            pageSize: "auto",
            margin: {
                top: "2cm",
                right: "1cm",
                left: "1cm",
                bottom: "1cm"
            },
            landscape: true,
            repeatHeaders: true,
            scale: 0.8
        },
        height: window.innerHeight * 0.7,  // 60% of the window height
        sortable: true,
        reorderable: true,
        groupable: true,
        resizable: true,
        filterable: {
            extra: false,
            operators: {
                number: {
                    eq: "Is equal to",
                    neq: "Is not equal to",
                    gte: "is greater than or equal to",
                    gt: "is greater than",
                    lte: "is less than or equal to",
                    lt: "is less than"
                },
                string: {
                    eq: "Is equal to",
                    neq: "Is not equal to",
                    startswith: "Starts with",
                    contains: "Contains",
                    doesnotcontain: "Does not contain",
                    endswith: "Ends with"
                },
                date: {
                    eq: "Is equal to",
                    neq: "Is not equal to",
                    gte: "Is after or equal to",
                    gt: "Is after",
                    lte: "Is before or equal to",
                    lt: "Is before"
                }
            }
        },
        columnMenu: true,
        columnMenuInit: function (e) {
            wordwrapmenu(e);
            checkboxItem = $(e.container).find('input[type="checkbox"]');
        },
        columnShow: function (e) {
            if ($(".k-widget.k-reset.k-header.k-menu.k-menu-vertical").is(":visible") && checkboxItem !== "") {
                SaveReportSetting('AreaSetup', 'grid');
            }
        },
        columnHide: function (e) {
            if ($(".k-widget.k-reset.k-header.k-menu.k-menu-vertical").is(":visible") && checkboxItem !== "") {
                SaveReportSetting('AreaSetup', 'grid');
            }
        },
        pageable: {
            refresh: true,
            buttonCount: 5
        },
        scrollable: {
            virtual: true
        },
        dataBound: function (e) {
            var grid = e.sender;
            if (grid.dataSource.total() === 0) {
                var colCount = grid.columns.length;
                $(grid.wrapper).find('tbody').append('<tr class="kendo-data-row" style="font-size:12px;"><td colspan="' + colCount + '" class="alert alert-danger">Sorry, no data :(</td></tr>');
                displayPopupNotification("No Data Found.", "info");
            } else {
                for (var i = 0; i < grid.columns.length; i++) {
                    grid.showColumn(i);
                }
                $("div.k-group-indicator").each(function (i, v) {
                    grid.hideColumn($(v).data("field"));
                });
            }

            //UpdateReportUsingSetting("AreaSetup", "grid");
            $('div').removeClass('.k-header k-grid-toolbar'); // Check if this line is necessary
        },
        columns: [
            { field: "TENDER_NO", title: "Tender No.", filterable: true },
            { field: "ISSUE_DATE", title: "Date", template: "#= kendo.toString(kendo.parseDate(ISSUE_DATE),'dd MMM yyyy') #" },
            { field: "CREATED_DATE", title: "Prepared Date & Time", template: "#= kendo.toString(kendo.parseDate(CREATED_DATE),'dd MMM yyyy') #" },
            { field: "VALID_DATE", title: "Checked Date", template: "#= kendo.toString(kendo.parseDate(VALID_DATE),'dd MMM yyyy') #" },
            { field: "APPROVED_STATUS", title: "Status" },
            {
                title: "Action",
                template: "<a class='fa fa-eye viewAction' ng-click='ViewQuot($event)' title='View'></a>&nbsp;&nbsp;<a class='fa fa-edit editAction' ng-click='EditQuot($event)' title='Edit'></a>&nbsp;&nbsp;<a class='fa fa-trash-o deleteAction' ng-click='deleteQuot($event)' title='Delete'></a>"
            }
        ]
    };
    $scope.deleteQuot = function (event) {
        var grid = $("#grid").data("kendoGrid");
        var row = event.target.closest("tr");
        var data = grid.dataItem(row);

        if (data.APPROVED_STATUS === "Approved") {
            displayPopupNotification("Deletion not allowed. The item is approved.", "warning");
            return;
        }

        bootbox.confirm({
            title: "Delete",
            message: "Are you sure?",
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-success'
                },
                cancel: {
                    label: 'No',
                    className: 'btn-danger'
                }
            },
            callback: function (result) {
                if (result) {
                    var response = $http({
                        method: 'POST',
                        url: "/api/QuotationApi/DeleteQuotVoucher?voucherno=" + data.TENDER_NO,
                        dataType: "JSON"
                    });
                    response.then(function (data) {
                        if (data.data.MESSAGE === "DELETED") {
                            displayPopupNotification("Data successfully deleted", "success");
                            $route.reload();
                        //    $window.location.href = window.location.protocol + "//" + window.location.host + "/QuotationManagement/Home/Index#!QM/QuotationSetup";
                        } else if (data.data.MESSAGE === "REFERENCED") {
                            displayPopupNotification("Voucher is in reference. Please delete the referenced voucher: " + data.data.VoucherNo, "warning");
                        } else {
                            displayPopupNotification("Something went wrong! Please try again.", "error");
                        }
                    }, function errorCallback(response) {
                        displayPopupNotification("Something went wrong! Please try again.", "error");
                    });
                }
            }
        });
    };
    $scope.ViewQuot = function (event) {
        var grid = $("#grid").data("kendoGrid");
        var row = event.target.closest("tr");
        var data = grid.dataItem(row);
        var id = data.TENDER_NO;
        id = id.split(new RegExp('/', 'i')).join('_');
        $window.location.href = "/QuotationManagement/Home/Index#!QM/ViewQuotation/" + id;
    };
    $scope.EditQuot = function (event) {
        var grid = $("#grid").data("kendoGrid");
        var row = event.target.closest("tr");
        var data = grid.dataItem(row);
        var id = data.TENDER_NO;
        id = id.split(new RegExp('/', 'i')).join('_');
        $window.location.href = "/QuotationManagement/Home/Index#!QM/EditQuotation/" + id;
    };
 });