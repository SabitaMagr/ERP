QMModule.controller('quotationDetailItemWise', function ($scope, $rootScope, $http, $filter, $timeout, $window) {

    // Initialize scope variables
    $scope.productFormList = [];
    $scope.productList = [];
    $scope.currencyData = []; // Declare currencyData on $scope
    $scope.currencyOptions = {};
    $scope.termList = [];
    $scope.counterProduct = 1;
    $scope.quotationDetails = false;
    $scope.showSpecificationDetail = false;
    var uniqueVendors = [];
    $scope.amountinword = "";
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
             //$scope.CURRENCY = quotation.CURRENCY;
             $scope.DELIVERY_DATE = formatDate(quotation.DELIVERY_DATE);
             $scope.TENDER_NO = quotation.TENDER_NO;
             $scope.ISSUE_DATE = formatDate(quotation.ISSUE_DATE);
             $scope.VALID_DATE = formatDate(quotation.VALID_DATE);
             $scope.NEPALI_DATE = quotation.NEPALI_DATE;
             $scope.TXT_REMARKS = quotation.REMARKS;
             $scope.STATUS = quotation.STATUS;
             $scope.DISCOUNT_TYPE = quotation.DISCOUNT_TYPE;
             $http.get("https://gist.githubusercontent.com/aaronhayes/5fef481815ac75f771d37b16d16d35c9/raw/edbec8eea5cc9ace57a79409cc390b7b9bcf24f6/currencies.json")
                 .then(function (response) {

                     $scope.currencyData = response.data;
                     var currency = $scope.currencyData.find(function (item) {
                         return item.code === $scope.CURRENCY;
                     });

                     if (currency) {
                         $scope.CURRENCY = currency.name;
                     } 
                 })
             $scope.TOTAL_AMOUNT = (quotation.TOTAL_AMOUNT).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
             $scope.TOTAL_DISCOUNT = (quotation.TOTAL_DISCOUNT).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
             $scope.TOTAL_EXCISE = (quotation.TOTAL_EXCISE).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
             $scope.TOTAL_TAXABLE_AMOUNT = (quotation.TOTAL_TAXABLE_AMOUNT).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
             $scope.TOTAL_VAT = (quotation.TOTAL_VAT).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
             $scope.TOTAL_NET_AMOUNT = (quotation.TOTAL_NET_AMOUNT).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
             $scope.amountinword = convertNumberToWords(quotation.TOTAL_NET_AMOUNT);

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
                     DISCOUNT: itemList.DISCOUNT,
                     DISCOUNT_AMOUNT: (itemList.DISCOUNT_AMOUNT).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 }),
                     EXCISE: (itemList.EXCISE).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 }),
                     TAXABLE_AMOUNT: (itemList.TAXABLE_AMOUNT).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 }),
                     ACTUAL_PRICE: ((itemList.TAXABLE_AMOUNT - itemList.EXCISE) / itemList.QUANTITY).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 }),
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

             var imageurl = [];
             var imageslistcount = "";
             if (quotation.IMAGES_LIST != null || quotation.IMAGES_LIST != undefined) {
                 imageslistcount = quotation.IMAGES_LIST.length;

                 $.each(quotation.IMAGES_LIST, function (key, value) {
                     var filepath = value.DOCUMENT_FILE_NAME;
                     var path = filepath.replace(/[/]/g, '_');
                     imageurl.push(path);
                 });
                 if (imageurl.length > 0) {
                     for (var i = 0; i < imageurl.length; i++) {
                         var mockFile = {
                             name: quotation.IMAGES_LIST[i].DOCUMENT_NAME,
                             size: 12345,
                             type: 'image/jpeg',
                             url: imageurl[i],
                             accepted: true,
                         };
                         if (i == 0) {
                             myInventoryDropzone.on("addedfile", function (file) {
                                 caption = file.caption == undefined ? "" : file.caption;
                                 file._captionLabel = Dropzone.createElement("<a class='fa fa-download dropzone-download' href='" + imageurl[i] + "' name='Download' class='dropzone_caption' download></a>");
                                 file.previewElement.appendChild(file._captionLabel);
                             });
                         }
                         myInventoryDropzone.emit("addedfile", mockFile);
                         myInventoryDropzone.emit("thumbnail", mockFile, imageurl[i]);
                         myInventoryDropzone.emit('complete', mockFile);
                         myInventoryDropzone.files.push(mockFile);
                         $('.dz-details').find('img').addClass('sr-only');
                         $('.dz-remove').css("display", "none");
                     }
                 }
             }
         })
        .catch(function (error) {
            var message = 'Error in displaying quotation!!';
            displayPopupNotification(message, "error");
        });
    $http.get('/api/QuotationApi/QuotationDetailsId?quotationNo=' + quotationNo + '&tenderNo=' + tenderNo)
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
            $scope.DISCOUNT_TYPE = quotation.DISCOUNT_TYPE;

            $scope.TOTAL_AMOUNT = (quotation.TOTAL_AMOUNT).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
            $scope.TOTAL_DISCOUNT = (quotation.TOTAL_DISCOUNT).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
            $scope.TOTAL_EXCISE = (quotation.TOTAL_EXCISE).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
            $scope.TOTAL_TAXABLE_AMOUNT = (quotation.TOTAL_TAXABLE_AMOUNT).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
            $scope.TOTAL_VAT = (quotation.TOTAL_VAT).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
            $scope.TOTAL_NET_AMOUNT = (quotation.TOTAL_NET_AMOUNT).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
            $scope.amountinword = convertNumberToWords(quotation.TOTAL_NET_AMOUNT);

            var id = 1;
            var idTerm = 1;
            var quantity = 0;

            $scope.productList = [];
            $scope.termList = [];
            for (var i = 0; i < quotation.Item_Detail.length; i++) {
                var itemList = quotation.Item_Detail[i];
                var imageUrl = window.location.protocol + "//" + window.location.host + "/Areas/NeoERP.QuotationManagement/Image/Items/" + itemList.IMAGE;
                $scope.productList.push({
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
                    DISCOUNT: itemList.DISCOUNT,
                    DISCOUNT_AMOUNT: (itemList.DISCOUNT_AMOUNT).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 }),
                    EXCISE: (itemList.EXCISE).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 }),
                    TAXABLE_AMOUNT: (itemList.TAXABLE_AMOUNT).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 }),
                    ACTUAL_PRICE: ((itemList.TAXABLE_AMOUNT - itemList.EXCISE) / itemList.QUANTITY).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 }),
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
                //$window.location.href = landingUrl;
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
                //$window.location.href = landingUrl;
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
    $scope.printPage = function () {
        setTimeout(function () {
            $("#saveAndPrintQuotationModal").modal("toggle");
        }, 50);
    }
    $scope.printTodayDateTime = new Date();

    $scope.cnlPrint = function () {
    };
    $scope.printcounttext = "Copy of Original";
    function convertNumberToWords(amount) {

        var finalWord1 = test_value(amount);
        var finalWord2 = "";

        var val = amount.toString();
        var actual_val = amount;
        amount = val;

        if (val.indexOf('.') != -1) {
            val = val.substring(val.indexOf('.') + 1, val.length);
            if (val.length == 0 || val == '00') {
                finalWord2 = "paisa zero";
            }
            else {
                amount = val;
                finalWord2 = "paisa" + test_value(amount);
            }
            return finalWord1 + " Rupees and " + finalWord2;
        }
        else {
            //finalWord2 =  " Zero paisa only";
            return finalWord1 + " Rupees";
        }
        amount = actual_val;
    }
    function test_value(amount) {

        var junkVal = amount;
        junkVal = Math.floor(junkVal);
        var obStr = new String(junkVal);
        numReversed = obStr.split("");
        actnumber = numReversed.reverse();


        if (Number(junkVal) == 0) {
            return obStr + '' + 'Rupees Zero';

        }


        var iWords = ["Zero", " One", " Two", " Three", " Four", " Five", " Six", " Seven", " Eight", " Nine"];
        var ePlace = ['Ten', ' Eleven', ' Twelve', ' Thirteen', ' Fourteen', ' Fifteen', ' Sixteen', ' Seventeen', ' Eighteen', 'Nineteen'];
        var tensPlace = ['dummy', ' Ten', ' Twenty', ' Thirty', ' Forty', ' Fifty', ' Sixty', ' Seventy', ' Eighty', ' Ninety'];

        var iWordsLength = numReversed.length;
        var totalWords = "";
        var inWords = new Array();
        var finalWord = "";
        j = 0;
        for (i = 0; i < iWordsLength; i++) {
            switch (i) {
                case 0:
                    if (actnumber[i] == 0 || actnumber[i + 1] == 1) {
                        inWords[j] = '';
                    }
                    else {
                        inWords[j] = iWords[actnumber[i]];
                    }
                    inWords[j] = inWords[j];
                    break;
                case 1:
                    tens_complication();
                    break;
                case 2:
                    if (actnumber[i] == 0) {
                        inWords[j] = '';
                    }
                    else if (actnumber[i - 1] != 0 && actnumber[i - 2] != 0) {
                        inWords[j] = iWords[actnumber[i]] + ' Hundred and';
                    }
                    else {
                        inWords[j] = iWords[actnumber[i]] + ' Hundred';
                    }
                    break;
                case 3:
                    if (actnumber[i] == 0 || actnumber[i + 1] == 1) {
                        inWords[j] = '';
                    }
                    else {
                        inWords[j] = iWords[actnumber[i]];
                    }
                    if (actnumber[i + 1] != 0 || actnumber[i] > 0) { //here
                        inWords[j] = inWords[j] + " Thousand";
                    }
                    break;
                case 4:
                    tens_complication();
                    break;
                case 5:
                    if (actnumber[i] == "0" || actnumber[i + 1] == 1) {
                        inWords[j] = '';
                    }
                    else {
                        inWords[j] = iWords[actnumber[i]];
                    }
                    if (actnumber[i + 1] != 0 || actnumber[i] > 0) {   //here 
                        inWords[j] = inWords[j] + " Lakh";
                    }

                    break;
                case 6:
                    tens_complication();
                    break;
                case 7:
                    if (actnumber[i] == "0" || actnumber[i + 1] == 1) {
                        inWords[j] = '';
                    }
                    else {
                        inWords[j] = iWords[actnumber[i]];
                    }
                    if (actnumber[i + 1] != 0 || actnumber[i] > 0) { // changed here
                        inWords[j] = inWords[j] + " Crore";
                    }
                    break;
                case 8:
                    tens_complication();
                    break;
                default:
                    break;
            }
            j++;
        }
        function tens_complication() {
            if (actnumber[i] == 0) {
                inWords[j] = '';
            }
            else if (actnumber[i] == 1) {
                inWords[j] = ePlace[actnumber[i - 1]];
            }
            else {
                inWords[j] = tensPlace[actnumber[i]];
            }
        }
        inWords.reverse();
        for (i = 0; i < inWords.length; i++) {
            finalWord += inWords[i];
        }
        return finalWord;
    }
   $scope.printDiv1 = function() {
    var printContents = document.getElementById('saveAndPrintQuotationModalFor').innerHTML;
    var popupWin = window.open('', '_blank', 'width=800,height=1000');
    popupWin.document.open();
    popupWin.document.write(`
        <html>
        <head>
            <title>Quotation Details</title>
            <style>
                @media print {
                    body, html {
                        margin: 0;
                        padding: 0;
                        width: 100%;
                        height: 100%;
                        overflow: hidden;
                    }
                    .modal-body {
                        padding: 20px;
                    }
                    .form-horizontal {
                        display: flex;
                        flex-direction: column;
                    }
                    .container {
                        width: 100%;
                        margin: 0;
                    }
                }
                @page {
                    size: A4;
                    margin: 20mm;
                }
            </style>
        </head>
        <body onload="window.print();window.close()">
            <div class="container">
                ${printContents}
            </div>
        </body>
        </html>`
    );
    popupWin.document.close();
};

});
