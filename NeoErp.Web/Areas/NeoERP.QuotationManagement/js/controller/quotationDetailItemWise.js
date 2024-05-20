QMModule.controller('quotationDetailItemWise', function ($scope, $rootScope, $http, $filter, $timeout, $window) {

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

    $scope.openImage = function (imageUrl) {
        window.open(imageUrl, '_blank');
    };
    // Initialize data source for view grid
    $scope.viewGridDataSource = new kendo.data.DataSource({
        data: [], // Initially empty
        pageSize: 10 // Optionally, set page size
    });


    var url = new URL(window.location.href);
    var quotationNo = url.searchParams.get("quotation");
    var tenderNo = url.searchParams.get("tender");

        $http.get('/api/QuotationApi/QuotationDetailsById?quotationNo='+ quotationNo + '&tenderNo='+ tenderNo)
         .then(function (response) {
             var quotation = response.data[0];
             $scope.QUOTATION_NO = quotation.QUOTATION_NO;
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

             $scope.TOTAL_AMOUNT = (quotation.TOTAL_AMOUNT).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
             $scope.TOTAL_DISCOUNT = (quotation.TOTAL_DISCOUNT).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
             $scope.TOTAL_EXCISE = (quotation.TOTAL_EXCISE).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
             $scope.TOTAL_TAXABLE_AMOUNT = (quotation.TOTAL_TAXABLE_AMOUNT).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
             $scope.TOTAL_VAT = (quotation.TOTAL_VAT).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
             $scope.TOTAL_NET_AMOUNT = (quotation.TOTAL_NET_AMOUNT).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

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
                     RATE: (itemList.RATE).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 }),
                     AMOUNT: (itemList.AMOUNT).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 }),
                     DISCOUNT: (itemList.DISCOUNT).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 }),
                     DISCOUNT_AMOUNT: (itemList.DISCOUNT_AMOUNT).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 }),
                     EXCISE: (itemList.EXCISE).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 }),
                     TAXABLE_AMOUNT: (itemList.TAXABLE_AMOUNT).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 }),
                     VAT_AMOUNT: (itemList.VAT_AMOUNT).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 }),
                     NET_AMOUNT: (itemList.NET_AMOUNT).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 }),
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

    function formatDate(dateString) {
        var date = new Date(dateString);
        return $filter('date')(date, 'dd-MMM-yyyy');
    }

    $scope.AcceptEvent = function () {
        var quotationNo = $scope.QUOTATION_NO;
        var status = 'AP';
        $http.post('/api/QuotationApi/updateQuotationStatus?quotationNo=' + quotationNo + '&status=' + status)
            .then(function (response) {
                displayPopupNotification("Quotation Accepted!!", "success");
                var landingUrl = window.location.protocol + "//" + window.location.host + "/QuotationManagement/Home/Index#!QM/SummaryReport";
                $window.location.href = landingUrl;
                setTimeout(function () {
                    $window.location.href = landingUrl;
                }, 1000);
            }).catch(function (error) {
                var message = 'Failed to accept Quotation!!'; // Extract message from response
                displayPopupNotification(message, "error");
                setTimeout(function () {
                    window.location.reload();
                }, 2000);
            });
    }
    $scope.RejectEvent = function () {
        var quotationNo = $scope.QUOTATION_NO;
        var status = 'R';
        $http.post('/api/QuotationApi/updateQuotationStatus?quotationNo=' + quotationNo + '&status=' + status)
            .then(function (response) {
                displayPopupNotification("Quotation Rejected!!", "success");
                var landingUrl = window.location.protocol + "//" + window.location.host + "/QuotationManagement/Home/Index#!QM/SummaryReport";
                $window.location.href = landingUrl;
                setTimeout(function () {
                    $window.location.href = landingUrl;
                }, 1000);
            }).catch(function (error) {
                var message = 'Failed to reject Quotation!!';
                displayPopupNotification(message, "error");
                setTimeout(function () {
                    window.location.reload();
                }, 2000);
            });
    }
});
