QMModule.controller('summaryReport', function ($scope, $rootScope, $http, $filter, $timeout, $window) {

    // Initialize scope variables
    $scope.productFormList = [];
    $scope.termList = [];
    $scope.counterProduct = 1;
    $scope.quotationDetails = false;
    $scope.showSpecificationDetail = false;
    $scope.tableData = true;
    $scope.tenderItemDetails = false;

    $scope.toggleDetails = function () {
        $scope.showSpecificationDetail = !$scope.showSpecificationDetail;
    };
    $scope.ItemSelect = {
        dataTextField: "ItemDescription",
        dataValueField: "ItemCode",
        height: 600,
        valuePrimitive: true,
        maxSelectedItems: 1,
        headerTemplate: '<div class="col-md-offset-3"><strong>Group...</strong></div>',
        placeholder: "Select Item...",
        autoClose: true,
        dataSource: {
            transport: {
                read: {
                    url: window.location.protocol + "//" + window.location.host + "/api/QuotationApi/ItemDetails",
                    dataType: "json"
                }
            }
        }
    };

    // Fetch tender details
    $http.post('/api/QuotationApi/TendersItemWise')
        .then(function (response) {
            var tenderDetails = response.data;
            if (tenderDetails && tenderDetails.length > 0) {
                $scope.dataSource.data(tenderDetails); // Set the data to the dataSource
            } else {
                console.log("No tenders found.");
            }
        });

    // Initialize data source for main grid
    $scope.dataSource = new kendo.data.DataSource({
        data: [], // Initially empty
    });

    // Initialize main grid
    $("#kGrid").kendoGrid({
        dataSource: $scope.dataSource,
        height: 400,
        sortable: true, // Enable sorting
        pageable: {
            refresh: true,
            pageSizes: true
        },
        toolbar: ["excel"/*, "pdf"*/],
        excel: {
            fileName: "Tender Details.xlsx",
            allPages: true
        },
        resizable: true, // Enable column resizing
        columns: [
            { field: "TENDER_NO", title: "Tender No", width: 120, type: "string" },
            {
                field: "CREATED_DATE", title: "Tender Date", width: 100, type: "string",
                template: "#=kendo.toString(kendo.parseDate(CREATED_DATE),'dd MMM yyyy') == null?'':kendo.toString(kendo.parseDate(CREATED_DATE),'dd MMM yyyy') #",
            },
            {
                field: "VALID_DATE", title: "To be Delivery Date", width: 100, type: "string",
                template: "#=kendo.toString(kendo.parseDate(VALID_DATE),'dd MMM yyyy') == null?'':kendo.toString(kendo.parseDate(VALID_DATE),'dd MMM yyyy') #",
            },
            { field: "ITEM_DESC", title: "Product Name", width: 400, type: "string" },
            { field: "STATUS", title: "Status", width: 90, type: "string" },
            {
                title: "Actions",
                width: 120,
                template: "<a class='btn btn-sm btn-info view-btn' data-id='#= TENDER_NO #'><i class='fa fa-eye'></i></a>"
            }
        ]
    });
    $scope.openImage = function (imageUrl) {
        window.open(imageUrl, '_blank');
    };
    // Initialize data source for view grid
    $scope.viewGridDataSource = new kendo.data.DataSource({
        data: [], // Initially empty
        pageSize: 10 // Optionally, set page size
    });

    // Handle click event on view button
    $("#kGrid").on("click", ".view-btn", function () {
        var quoteNo = $(this).data("id");
        var id = quoteNo.split(new RegExp('/', 'i')).join('_');
        window.location.href="/QuotationManagement/Home/Index#!QM/QuotationDetail/" +id;
    });



    $("#itemtxtSearchString").keyup(function () {
        var val = $(this).val().toLowerCase(); // Get the search input value
        var filters = [];
        var columns = $("#kGrid").data("kendoGrid").columns;
        for (var i = 0; i < columns.length; i++) {
            var column = columns[i];
            var field = column.field;
            if (column.type === "string") {
                filters.push({
                    field: field,
                    operator: "contains",
                    value: val
                });
            } else if (column.type === "number") {
                filters.push({
                    field: field,
                    operator: "eq",
                    value: parseFloat(val) || null
                });
            } else if (column.type === "date") {
                // Assuming you have a parsedDate variable defined somewhere in your code
                if (parsedDate) {
                    filters.push({
                        field: field,
                        operator: "eq",
                        value: new Date(val) || null
                    });
                }
            }
        }
        $scope.dataSource.filter({
            logic: "or",
            filters: filters
        });
    });

    function formatDate(dateString) {
        var date = new Date(dateString);
        return $filter('date')(date, 'dd-MMM-yyyy');
    }

});
