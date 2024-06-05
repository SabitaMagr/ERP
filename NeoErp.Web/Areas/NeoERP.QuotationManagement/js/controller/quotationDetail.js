QMModule.controller('quotationDetail', function ($scope, $rootScope, $http, $filter, $timeout, $window) {

    // Initialize scope variables
    $scope.productFormList = [];
    $scope.termList = [];
    $scope.counterProduct = 1;
    $scope.quotationDetails = false;
    $scope.showSpecificationDetail = false;
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


    $scope.openImage = function (imageUrl) {
        window.open(imageUrl, '_blank');
    };
    // Initialize data source for view grid
    $scope.viewGridDataSource = new kendo.data.DataSource({
        data: [], // Initially empty
        pageSize: 10 // Optionally, set page size
    });


    var url = new URL(window.location.href);
    var id = url.searchParams.get("id");

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
                            ratesByItemCode[party.ITEM_CODE][party.QUOTATION_NO] = {
                                rate: party.ACTUAL_PRICE,
                                status: party.STATUS
                            };

                            var vendorExists = uniqueVendors.some(function (item) {
                                return item.quotationNo === party.QUOTATION_NO;
                            });

                            // If the vendor and quotation number do not exist, add them to uniqueVendors
                            if (!vendorExists) {
                                uniqueVendors.push({
                                    name: party.PARTY_NAME, quotationNo: party.QUOTATION_NO, status: party.STATUS,revise:party.REVISE
                                });
                            }
                        });

                        itemDetails.forEach(function (item) {
                            uniqueVendors.forEach(function (vendor) {
                                item[vendor.quotationNo] = ratesByItemCode[item.ITEM_CODE][vendor.quotationNo] || '';
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
                        displayPopupNotification("Party details not found.", "error");
                    }
                } else {
                    console.log("No tenders found.");
                }

            })
            .catch(function (error) {
                var message = 'Error in displaying quotation!!';
                displayPopupNotification(message, "error");
            });

    function generateColumns(uniqueVendors) {
        var vendorColumns = [];
        // Generate vendor columns
        uniqueVendors.forEach(function (vendor) {
            var backgroundColor = vendor.status === 'AP' ? '#9afa84' : 'transparent';
            console.log(vendor);
            var displayRevise = vendor.revise ? ` (${vendor.revise})` : '';

            var headerTemplate = `
            <div style="display: flex; align-items: center;">
                <span class="k-link vendor-title" data-quotation-no="${vendor.quotationNo}">${vendor.name}${displayRevise}</span>
                <i class="fa fa-search vendor-search" style="margin-left: 1rem; cursor: pointer;"></i>
            </div>`;

            var column = {
                field: vendor.quotationNo, // Use quotationNo as the field
                headerTemplate: headerTemplate,
                width: 180,
                template: function (dataItem) {
                    var rateInfo = dataItem[vendor.quotationNo]; // Access rate info using quotationNo
                    return rateInfo ? `
                    <div style="display: flex; justify-content: right; cursor: pointer;">
                        ${rateInfo.rate}
                    </div>` : '';
                },
                headerAttributes: { style: `background-color: ${backgroundColor}` }
            };

            vendorColumns.push(column);
        });

        return vendorColumns;
    }

    function handleTitleClick(quotationNo) {
        var tenderNo = $scope.TENDER_NO;
        var landingUrl = window.location.protocol + "//" + window.location.host + "/QuotationManagement/Home/QuotationDetailItemwise?quotation=" + quotationNo + "&tender=" + tenderNo;
        setTimeout(function () {
            $window.location.href = landingUrl;
        }, 1000);
    }


    function formatDate(dateString) {
        var date = new Date(dateString);
        return $filter('date')(date, 'dd-MMM-yyyy');
    }

 
});
