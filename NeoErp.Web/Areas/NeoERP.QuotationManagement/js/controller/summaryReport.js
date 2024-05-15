QMModule.controller('summaryReport', function ($scope, $rootScope, $http, $filter, $timeout, $location, $httpParamSerializer) {
    // Initialize date pickers
    $("#englishdatedocument").kendoDatePicker();
    $("#validDt").kendoDatePicker();
    $("#deliveryDt").kendoDatePicker();

    // Initialize scope variables
    $scope.productFormList = [];
    $scope.counterProduct = 1;
    $scope.quotationDetails = false;
    $scope.showSpecificationDetail = false;
    $scope.tableData = true;
    $scope.tenderItemDetails = false;
    var uniqueVendors = [];

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
        pageSize: 10 // Optionally, set page size
    });

    // Initialize main grid
    $("#kGrid").kendoGrid({
        dataSource: $scope.dataSource,
        height: 400,
        sortable: true,
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

    // Initialize data source for view grid
    $scope.viewGridDataSource = new kendo.data.DataSource({
        data: [], // Initially empty
        pageSize: 10 // Optionally, set page size
    });

    // Handle click event on view button
    $("#kGrid").on("click", ".view-btn", function () {
        var id = $(this).data("id");
        $scope.tableData = false;
        $scope.tenderItemDetails = true;

        // Fetch item details by tender ID
        $http.get('/api/QuotationApi/ItemDetailsByTender?tenderNo=' + id)
            .then(function (response) {
                var quotation = response.data[0];
                var itemDetails = quotation.Items;
                var partyDetails = quotation.PartDetails;
                var quotationNo = quotation.QUOTATION_NO; // Extract quotation_no from quotation object

                $scope.TENDER_NO = quotation.TENDER_NO;
                $scope.ISSUE_DATE = formatDate(quotation.ISSUE_DATE);
                $scope.VALID_DATE = formatDate(quotation.VALID_DATE);
                $scope.NEPALI_DATE = quotation.NEPALI_DATE;
                $scope.TXT_REMARKS = quotation.REMARKS;
                var id = 1;

                if (itemDetails && itemDetails.length > 0) {
                    if (partyDetails && partyDetails.length > 0) {
                        var ratesByItemCode = {};

                        // Process party details
                        partyDetails.forEach(function (party) {
                            if (!ratesByItemCode[party.ITEM_CODE]) {
                                ratesByItemCode[party.ITEM_CODE] = {};
                            }
                            ratesByItemCode[party.ITEM_CODE][party.PARTY_NAME] = {
                                rate: party.RATE,
                                status: party.STATUS,
                                quotationNo: party.QUOTATION_NO
                            };
                            // Add vendor names to uniqueVendors array
                            //if (uniqueVendors.indexOf(party.PARTY_NAME) === -1) {
                            //    uniqueVendors.push(party.PARTY_NAME);
                            //    uniqueVendors.push(party.QUOTATION_NO);
                            //}
                            var vendorExists = uniqueVendors.some(function (item) {
                                return item.vendor === party.PARTY_NAME && item.quotationNo === party.QUOTATION_NO;
                            });

                            // If the vendor and quotation number do not exist, add them to uniqueVendors
                            if (!vendorExists) {
                                uniqueVendors.push({ vendor: party.PARTY_NAME, quotationNo: party.QUOTATION_NO });
                            }
                        });

                        itemDetails.forEach(function (item) {
                            uniqueVendors.forEach(function (vendor) {
                                item[vendor] = ratesByItemCode[item.ITEM_CODE][vendor] || '';
                            });
                        });


                        // Generate columns for view grid
                        var dynamicColumns = generateColumns(uniqueVendors); // Pass quotationNo to generateColumns
                        var staticColumns = [
                            { field: "ID", title: "S/No", width: 50, type: "number" },
                            { field: "ITEM_DESC", title: "Product Name", width: 300, type: "string" },
                            { field: "SPECIFICATION", title: "Specification", width: 150, type: "string" },
                            { field: "UNIT", title: "Unit", width: 90, type: "string" },
                            { field: "QUANTITY", title: "Quantity", width: 90, type: "number" }
                        ];
                        itemDetails.forEach(function (item) {
                            item.ID = id++; // Assign the current ID and increment for the next item
                        });
                        var combinedColumns = staticColumns.concat(dynamicColumns);

                        // Create the viewGrid with the combined columns
                        $("#viewGrid").kendoGrid({
                            dataSource: $scope.viewGridDataSource,
                            height: 400,
                            sortable: true,
                            pageable: {
                                refresh: true,
                                pageSizes: true
                            },
                            resizable: true, // Enable column resizing
                            columns: combinedColumns
                        });

                        $scope.viewGridDataSource.data(itemDetails); // Populate viewGrid with the combined data
                    } else {
                        console.log("No party details found.");
                    }
                } else {
                    console.log("No tenders found.");
                }
            })
            .catch(function (error) {
                var message = 'Error in displaying quotation!!';
                displayPopupNotification(message, "error");
            });
    });



    function generateColumns(uniqueVendors) {
        var vendorColumns = [];
        // Generate vendor columns
        uniqueVendors.forEach(function (vendor) {
            vendorColumns.push({
                field: vendor.vendor,
                title: "<span>" + vendor.vendor + " - " + vendor.quotationNo + "</span>", // Corrected here
                width: 200,
                template: function (dataItem) {
                    console.log(dataItem);
                    var rateInfo = dataItem[vendor.vendor]; // Accessing rate info using vendor name
                    console.log(rateInfo);
                    var backgroundColor = rateInfo.status === 'AP' ? '#9afa84' : 'transparent';
                    return `
            <div 
                style="background-color: ${backgroundColor}; height: 100%; display: flex; align-items: center; cursor: pointer;" 
                onclick="navigateToNextPage('${vendor.quotationNo}', '${vendor.vendor}')"
            >
                ${rateInfo.rate}
            </div>`;
                }
            });
        });

        return vendorColumns;
    }


    // Function to handle navigation
    function navigateToNextPage(quotationNo, vendor) {
        window.location.href = `/nextPage?quotationNo=${quotationNo}&vendor=${vendor}`;
    }
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
