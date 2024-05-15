QMModule.controller('summaryReport', function ($scope, $rootScope, $http, $filter, $timeout) {

    // Initialize scope variables
    $scope.productFormList = [];
    $scope.termList = [];
    $scope.counterProduct = 1;
    $scope.quotationDetails = false;
    $scope.showSpecificationDetail = false;
    $scope.tableData = true;
    $scope.tenderItemDetails = false;
    var uniqueVendors = [];

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
        pageSize: 10 // Optionally, set page size
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
                        var uniqueVendors = [];

                        partyDetails.forEach(function (party) {
                            if (!ratesByItemCode[party.ITEM_CODE]) {
                                ratesByItemCode[party.ITEM_CODE] = {};
                            }
                            ratesByItemCode[party.ITEM_CODE][party.PARTY_NAME] = {
                                rate: party.RATE,
                                status: party.STATUS
                            };
                            var vendorExists = uniqueVendors.some(function (item) {
                                return item.name === party.PARTY_NAME && item.quotationNo === party.QUOTATION_NO && item.status === party.STATUS;
                            });

                            // If the vendor and quotation number do not exist, add them to uniqueVendors
                            if (!vendorExists) {
                                uniqueVendors.push({
                                    name: party.PARTY_NAME, quotationNo: party.QUOTATION_NO, status: party.STATUS
                                });
                            }
                        });

                        itemDetails.forEach(function (item) {
                            uniqueVendors.forEach(function (vendor) {
                                item[vendor.name] = ratesByItemCode[item.ITEM_CODE][vendor.name] || '';
                            });
                            item.QUOTATION_NO = $scope.TENDER_NO; // Add quotation number to each item
                        });

                        var dynamicColumns = generateColumns(uniqueVendors);
                        var staticColumns = [
                            { field: "ID", title: "S/No", width: 50, type: "number" },
                            { field: "ITEM_DESC", title: "Product Name", width: 250, type: "string" },
                            { field: "SPECIFICATION", title: "Specification", width: 150, type: "string" },
                            { field: "UNIT", title: "Unit", width: 90, type: "string" },
                            { field: "QUANTITY", title: "Quantity", width: 90, type: "number" },
                            { field: "LAST_VENDOR", title: "Last Vendor", width: 90, type: "string" },
                            { field: "LAST_PRICE", title: "Last Price", width: 90, type: "number", style: "text-align:right" }

                        ];

                        var id = 1; // Initialize ID counter
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
                            resizable: true,
                            columns: combinedColumns
                        });

                        $scope.viewGridDataSource.data(itemDetails); // Populate viewGrid with the combined data

                        // Attach event listeners to the parent element for event delegation
                        $("#viewGrid").on("click", ".vendor-title", function () {
                            var quotationNo = $(this).attr('data-quotation-no');
                            handleTitleClick(quotationNo);
                        });

                        $("#viewGrid").on("click", ".vendor-search", function () {
                            var quotationNo = $(this).siblings('.vendor-title').attr('data-quotation-no');
                            handleTitleClick(quotationNo);
                        });
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
            var backgroundColor = vendor.status === 'AP' ? '#9afa84' : 'transparent';

            var headerTemplate = `
        <div style="display: flex; align-items: center;">
            <span class="k-link vendor-title" data-quotation-no="${vendor.quotationNo}">${vendor.name}</span>
            <i class="fa fa-search vendor-search" style="margin-left: 1rem; cursor: pointer;"></i>
        </div>`;

            var column = {
                field: vendor.name,
                headerTemplate: headerTemplate,
                width: 180,
                template: function (dataItem) {
                    var rateInfo = dataItem[vendor.name];
                    return `
                <div style="display: flex; justify-content: right; cursor: pointer;">
                    ${rateInfo.rate}
                </div>`;
                },
                headerAttributes: { style: `background-color: ${backgroundColor}` }
            };

            vendorColumns.push(column);
        });

        return vendorColumns;
    }

    function handleTitleClick(quotationNo) {
        $http.get('/api/QuotationApi/QuotationDetailsById?quotationNo=' + quotationNo)
            .then(function (response) {
                var quotation = response.data[0];
                console.log(quotation.CONTACT_NO);
                $scope.PAN_NO = quotation.PAN_NO;
                $scope.PARTY_NAME = quotation.PARTY_NAME;
                $scope.ADDRESS = quotation.ADDRESS;
                $scope.CONTACT_NO = quotation.CONTACT_NO;
                $scope.EMAIL = quotation.EMAIL;
                $scope.CURRENCY_RATE = quotation.CURRENCY_RATE;
                $scope.CURRENCY = quotation.CURRENCY;
                $scope.DELIVERY_DATE = formatDate(quotation.DELIVERY_DATE);
                $scope.TENDER_NO = quotation.TENDER_NO;
                $scope.ISSUE_DATE = formatDate(quotation.ISSUE_DATE);
                $scope.VALID_DATE = formatDate(quotation.VALID_DATE);
                $scope.NEPALI_DATE = quotation.NEPALI_DATE;
                $scope.TXT_REMARKS = quotation.REMARKS;
                $scope.STATUS = quotation.STATUS;

                $scope.TOTAL_AMOUNT = quotation.TOTAL_AMOUNT;
                $scope.TOTAL_DISCOUNT = quotation.TOTAL_DISCOUNT;
                $scope.TOTAL_EXCISE = quotation.TOTAL_EXCISE;
                $scope.TOTAL_TAXABLE_AMOUNT = quotation.TOTAL_TAXABLE_AMOUNT;
                $scope.TOTAL_VAT =quotation.TOTAL_VAT;
                $scope.TOTAL_NET_AMOUNT = quotation.TOTAL_NET_AMOUNT;

                var id = 1;
                var idTerm = 1;
                var quantity = 0;

                $scope.productFormList = [];
                $scope.termList = [];
                for (var i = 0; i < quotation.Item_Detail.length; i++) {
                    var itemList = quotation.Item_Detail[i];
                    var imageUrl = window.location.protocol + "//" + window.location.host + "/Areas/NeoERP.QuotationManagement/Image/Items/" + itemList.IMAGE;
                    $scope.productFormList.push({
                        TID: itemList.ID,
                        ID: id,
                        ItemDescription: itemList.ITEM_CODE,
                        SPECIFICATION: itemList.SPECIFICATION,
                        IMAGE: itemList.IMAGE,
                        IMAGE_LINK: imageUrl,
                        UNIT: itemList.UNIT,
                        QUANTITY: itemList.QUANTITY,
                        CATEGORY: itemList.CATEGORY,
                        BRAND_NAME: itemList.BRAND_NAME,
                        INTERFACE: itemList.INTERFACE,
                        TYPE: itemList.TYPE,
                        LAMINATION: itemList.LAMINATION,
                        ITEM_SIZE: itemList.ITEM_SIZE,
                        THICKNESS: itemList.THICKNESS,
                        COLOR: itemList.COLOR,
                        GRADE: itemList.GRADE,
                        SIZE_LENGTH: itemList.SIZE_LENGTH,
                        SIZE_WIDTH: itemList.SIZE_WIDTH,
                        RATE: itemList.RATE,
                        AMOUNT: itemList.AMOUNT,
                        DISCOUNT: itemList.DISCOUNT,
                        DISCOUNT_AMOUNT: itemList.DISCOUNT_AMOUNT,
                        EXCISE: itemList.EXCISE,
                        TAXABLE_AMOUNT: itemList.TAXABLE_AMOUNT,
                        VAT_AMOUNT: itemList.VAT_AMOUNT,
                        NET_AMOUNT: itemList.NET_AMOUNT,
                    });
                    quantity += itemList.QUANTITY;
                    id++;
                }
                for (var i = 0; i < quotation.TermsCondition.length; i++) {
                    var termList = quotation.TermsCondition[i];
                    $scope.termList.push({
                        ID: idTerm,
                        TERM_CONDITION: termList.TERM_CONDITION,
                    });
                    idTerm++;
                }
                $scope.TOTAL_QUANTITY = quantity;

                $scope.tenderItemDetails = false;
                $scope.quotationDetails = true;

                // After populating data, trigger select events
                setTimeout(function () {
                    for (let i = 0; i < quotation.Item_Detail.length; i++) {
                        var currentItem = quotation.Item_Detail[i];

                        var currentItemCode = currentItem.ITEM_CODE;
                        // Check if the element exists before attempting to trigger the select event
                        var dropdownElement = $("#item_" + id).data("kendoDropDownList");
                        if (dropdownElement) {
                            dropdownElement.value(currentItemCode);
                        }
                        id++;
                    }
                }, 200);
            })
            .catch(function (error) {
                var message = 'Error in displaying quotation!!';
                displayPopupNotification(message, "error");
            });
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
