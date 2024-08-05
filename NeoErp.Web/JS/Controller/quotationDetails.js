TMModule.controller('quotationDetails', function ($scope, $rootScope, $http, $routeParams, $http, $filter, $q, $timeout) {
    $("#englishdatedocument").kendoDatePicker();
    var dt = $q.defer();
    $scope.save = "Save";
    var dt = $q.defer();
    if ($routeParams.id != undefined) {
        $scope.OrderNo = $routeParams.id.split(new RegExp('_', 'i')).join('/');
    }
    else { $scope.OrderNo = "undefined"; }
    $scope.isReadOnly = true;
    $scope.masterModels = {};
    $scope.masterChildData = null;
    $scope.masterModelTemplate = {};
    $scope.MasterFormElement = [];
    $scope.ChildFormElement = [{ element: [] }];
    $scope.aditionalChildFormElement = [];
    $scope.childModelTemplate = {};
    $scope.specificationModel = {};
    $scope.childModels = [];
    $scope.freeze_master_ref_flag = 'N';
    $scope.newgenorderno = "";
    $scope.aditionalElement = [];
    $scope.categoryList = "";
    $scope.data = [];
    $scope.termList = [];
    $scope.counterProduct = 1;
    $scope.counterTerm = 1;
    $scope.quotationNo = "";
    $scope.tenderNo = "";
    $scope.companyCode = "";
    $scope.currencyData = [];
    $scope.selectedCurrency = "";
    $scope.company = {};
    // Fetch currency data and set default

    $http.get("https://gist.githubusercontent.com/aaronhayes/5fef481815ac75f771d37b16d16d35c9/raw/edbec8eea5cc9ace57a79409cc390b7b9bcf24f6/currencies.json")
        .then(function (response) {
            $scope.currencyData = response.data;

            // Set the default currency and rate here
            var nepaliRupee = $scope.currencyData.find(currency => currency.code === 'NPR');
            if (nepaliRupee) {
                $scope.masterModels['CURRENCY_CODE'] = nepaliRupee.code; // Set the currency code to NPR
                $scope.masterModels['CURRENCY_RATE'] = 1; // Set rate for NPR to 1
            }

            // Now set the currency options
            $scope.currencyOptions = {
                dataSource: $scope.currencyData,
                dataTextField: "name",
                dataValueField: "code",
                optionLabel: "Select Currency",
                filter: "contains",
                autoBind: true,
                change: function (e) {
                    var selectedCurrency = this.value();
                    if (selectedCurrency === 'NPR') {
                        $scope.$applyAsync(function () {
                            $scope.masterModels['CURRENCY_RATE'] = 1; // Set rate for NPR to 1
                        });
                    } else {
                        $scope.$applyAsync(function () {
                            $scope.masterModels['CURRENCY_RATE'] = ""; // Clear rate for other currencies
                        });
                    }
                },
                dataBound: function (e) {
                    // Automatically select Nepali Rupee (NPR) on load
                    var dropdown = this;
                    if (nepaliRupee) {
                        dropdown.value(nepaliRupee.code); // Set dropdown value to NPR code
                        dropdown.trigger("change"); // Trigger change event
                    }
                }
            };
        })
        .catch(function (error) {
            console.error("Error fetching currency data:", error);
        });

    $scope.currencyOptions = {
        dataSource: $scope.currencyData,
        dataTextField: "name",
        dataValueField: "code",
        optionLabel: "Select Currency",
        filter: "contains",
        autoBind: true,
        change: function (e) {
            var selectedCurrency = this.value();
            if (selectedCurrency === 'NPR') {
                $scope.$applyAsync(function () {
                    $scope.masterModels['CURRENCY_RATE'] = 1; // Set rate for NPR to 1
                });
            } else {
                $scope.$applyAsync(function () {
                    $scope.masterModels['CURRENCY_RATE'] = ""; // Clear rate for other currencies
                });
            }
        },
        dataBound: function (e) {
            // Automatically select Nepali Rupee (NPR) on load
            var dropdown = this;
            var nepaliRupee = $scope.currencyData.find(currency => currency.code === 'NPR');
            if (nepaliRupee) {
                dropdown.value(nepaliRupee.code); // Set dropdown value to NPR code
                dropdown.trigger("change"); // Trigger change event
            }
        }
    };
    var url = new URL(window.location.href);
    $scope.quotationNo = url.searchParams.get("qo");
    $http.get("/api/ApiQuotation/GetCompanyDetails?id=" + $scope.quotationNo)
        .then(function (response) {
            var company = response.data[0];
            company.LOGO_FILE_NAME = window.location.protocol + "//" + window.location.host + "/Pictures/Login/" + company.LOGO_FILE_NAME;
            $scope.company = company;

            var companyCode = company.COMPANY_CODE;
            $scope.companyCode = companyCode

            return $http.get('/api/ApiQuotation/GetFormDetailSetup', { params: { companyCode: companyCode } });
        })
        .then(function (response) {
            $scope.formDetail = response.data;
            if ($scope.formDetail.length > 0) {
                $scope.DocumentName = $scope.formDetail[0].TABLE_NAME;
                $scope.companycode = $scope.formDetail[0].COMPANY_CODE;
                $scope.freeze_master_ref_flag = $scope.formDetail[0].FREEZE_MASTER_REF_FLAG;
            }
            var values = $scope.formDetail;

            // Collection of Master elements
            angular.forEach(values, function (value, key) {
                if (value.MASTER_CHILD_FLAG == 'M') {
                    $scope.masterModels[value.COLUMN_NAME] = value.DEFA_VALUE == null ? "" : value.DEFA_VALUE;
                    $scope.masterModelTemplate[value.COLUMN_NAME] = value.DEFA_VALUE == null ? "" : value.DEFA_VALUE;
                }
                if (value.MASTER_CHILD_FLAG == 'M') {
                    this.push(value);
                    if (value['COLUMN_NAME'].indexOf('DATE') > -1) {
                        $scope.masterModelTemplate[value['COLUMN_NAME']] = $filter('date')(new Date(), 'dd-MMM-yyyy');
                    } else {
                        $scope.masterModelTemplate[value['COLUMN_NAME']] = "";
                    }
                    $scope.masterModels[value.COLUMN_NAME] = value.DEFA_VALUE;
                }
            }, $scope.MasterFormElement);

            // Collection of child elements
            angular.forEach(values, function (value, key) {
                if (value.MASTER_CHILD_FLAG == 'C' && value.DISPLAY_FLAG == 'Y') {
                    this.push(value);
                    if (value['COLUMN_NAME'] === "ITEM_CODE") {
                        $rootScope.ITEM_CODE_DEFAULTVAL = value.DEFA_VALUE == null ? "" : value.DEFA_VALUE;
                    }
                }
            }, $scope.ChildFormElement[0].element);

            angular.forEach(values, function (value, key) {
                if (value.MASTER_CHILD_FLAG == 'C' && value.DISPLAY_FLAG == 'Y') {
                    this.push(value);
                    $scope.childModelTemplate[value['COLUMN_NAME']] = "";
                }
            }, $scope.aditionalChildFormElement);

            // More specification
            angular.forEach(values, function (value, key) {
                if (value.MASTER_CHILD_FLAG == 'C' && value.DISPLAY_FLAG == 'N') {
                    this.push(value);
                    $scope.specificationModel[value['COLUMN_NAME']] = "";
                }
            }, $scope.aditionalElement);

            var tempFn = function (response) {
                var rows = response.data;
                if (rows.length > 0) {
                    $scope.masterModels = {};
                    var masterModel = angular.copy($scope.masterModelTemplate);
                    var savedData = $scope.getObjWithKeysFromOtherObj(masterModel, response.data[0]);
                    $scope.newgenorderno = savedData.QUOTE_NO;
                    $scope.masterModels = savedData;

                    // Solve problem in suppliercode binding for update purpose
                    suppliercodeforupdate = $scope.masterModels.SUPPLIER_CODE;

                    if ($scope.quotationNo != undefined && $scope.quotationNo != "undefined") {
                        $scope.ChildFormElement = [];
                        $scope.childModels = [];
                        $scope.newgenorderno = "";
                        $scope.save = "Update";
                    }
                    for (var i = 0; i < rows.length; i++) {
                        setDataOnModal(rows, i);
                    }
                } else {
                    // Handle no rows case
                }
            };

            // Check master transaction type .. if cr/dr disable child element
            if ($scope.masterChildData === null) {
                $scope.masterModelDataFn(tempFn);
            } else {
                tempFn($scope.masterChildData);
            }
            $scope.check = function () {
                var MasterFormlen = $scope.MasterFormElement.length;
                $.each($scope.MasterFormElement, function (key, value) {
                    var mastercolname = $scope.MasterFormElement[key].COLUMN_NAME;
                    if (mastercolname == "MASTER_TRANSACTION_TYPE") {
                        if ($scope.MasterFormElement[key].DEFA_VALUE != null);
                        $scope.checktransaction = true;
                        return;
                    }
                });
            }

            $scope.check();

            var date = new Date();
            $scope.todaydate = $filter('date')(new Date(), 'dd-MMM-yyyy');
            $scope.someFn();
            elements = $scope.MasterFormElement;

            $scope.masterModels;
            $scope.childModels;
            dt.resolve($scope.MasterFormElement);
        })
        .catch(function (error) {
            console.error('Error fetching company details or form detail setup:', error);
        });

    $scope.openImage = function (imageUrl) {
        window.open(imageUrl, '_blank');
    };
    $scope.fetchEmployeeDetails = function () {
        $http.get('/api/ApiQuotation/getSupplierDetails', { params: { panNo: $scope.masterModels['PAN_NO'] } })
            .then(function (response) {
                if (response.data.length > 0) {
                    var employeeDetails = response.data[0];
                    $scope.masterModels['SUPPLIER_CODE'] = employeeDetails.SUPPLIER_EDESC;
                    $scope.masterModels['ADDRESS'] = employeeDetails.ADDRESS;
                    $scope.masterModels['CONTACT_NO'] = employeeDetails.CONTACT_NO;
                    $scope.masterModels['EMAIL'] = employeeDetails.EMAIL;
                } else {
                    $scope.masterModels['SUPPLIER_CODE'] = "";
                    $scope.masterModels['ADDRESS'] = "";
                    $scope.masterModels['CONTACT_NO'] = "";
                    $scope.masterModels['EMAIL'] = "";
                }
            })
            .catch(function (error) {
                console.error('Error fetching Employee details:', error);
            });
    };

    $scope.ChildSumOperations = function (keys) {

        $scope.Childcolqtyratevalidation(keys);
        var child_rate = $scope.childModels[keys].UNIT_PRICE;
        var quantity = $scope.childModels[keys].QUANTITY;
        if (child_rate != null && child_rate != "" && child_rate !== undefined) {
            $scope.childModels[keys].UNIT_PRICE = parseFloat(child_rate.toFixed(2));
        }



        if (quantity !== undefined || quantity !== null || quantity !== "") {
            $scope.childModels[keys].CALC_QUANTITY = quantity;
        }
        if (child_rate !== undefined && child_rate !== null && child_rate !== "") {
            $scope.childModels[keys].CALC_UNIT_PRICE = child_rate;
        }
        if (child_rate === undefined || quantity === undefined) {

        }
        if (child_rate === 0 || quantity === 0) {

            $scope.childModels[keys].TOTAL_PRICE = 0;
        }
        if (child_rate === "" || quantity === "") {


        }
        if (child_rate === null || quantity === null) {


        }
        else {
            if (child_rate != null && child_rate !== "" && child_rate !== undefined) {
                if (quantity != null && quantity != "" && quantity !== undefined) {
                    var total_price = child_rate * quantity;

                    $scope.childModels[keys].TOTAL_PRICE = parseFloat(total_price.toFixed(2));
                    $scope.childModels[keys].CALC_TOTAL_PRICE = $scope.childModels[keys].TOTAL_PRICE;
                }
            }




        }


        $scope.GrandtotalCalution();
    }
    $scope.Childcolqtyratevalidation = function (keys) {

        var child_rate = $scope.childModels[keys].UNIT_PRICE;
        var quantity = $scope.childModels[keys].QUANTITY;
        //rate validation start
        if (child_rate === undefined) {
            $(".UNIT_PRICE_" + keys).parent().css({ "border": "solid 1px red" });
        }
        else {
            $(".UNIT_PRICE_" + keys).parent().css({ "border": "none" });
        }
        //validation end

        //quantity validation start
        if (quantity === undefined) {
            $(".QUANTITY_" + keys).parent().css({ "border": "solid 1px red" });
        }
        else {
            $(".QUANTITY_" + keys).parent().css({ "border": "none" });
        }

        //if (child_rate === undefined && quantity === undefined) {
        //    $scope.childModels[keys].CALC_UNIT_PRICE = "";
        //    $scope.childModels[keys].CALC_QUANTITY = "";
        //    return;
        //}
        //validation end
    };
    $scope.GrandtotalCalution = function () {

        var sum = 0;
        angular.forEach($scope.childModels, function (value, key) {

            if ($scope.childModels[key].hasOwnProperty("TOTAL_PRICE")) {
                if (typeof value.TOTAL_PRICE !== 'undefined' && value.TOTAL_PRICE !== null && value.TOTAL_PRICE !== "") {
                    sum = parseFloat(sum) + (parseFloat(value.TOTAL_PRICE));
                }
            }
            //else {
            //    if ($scope.childModels[key].hasOwnProperty("CALC_TOTAL_PRICE")) {
            //        if (typeof value.CALC_TOTAL_PRICE !== 'undefined' && value.CALC_TOTAL_PRICE !== null && value.CALC_TOTAL_PRICE !== "") {
            //            sum = parseFloat(sum) + (parseFloat(value.CALC_TOTAL_PRICE));
            //        }
            //    }
            //}
        });

        $scope.summary.grandTotal = (parseFloat(sum)).toFixed(2);
        if ($scope.data.length == 0 || $scope.data.length == "undefined") {
            $scope.adtotal = $scope.summary.grandTotal;
        }
        $scope.setTotal();
    }
    $scope.setTotal = function () {

        $scope.units = [];
        $scope.totalQty = 0;
        var totalQty = 0;
        $scope.childModels.forEach(function (item) {
            var qtySum = 0;
            $scope.childModels.forEach(function (it) {
                if (item.MU_CODE == it.MU_CODE) {
                    if (it.QUANTITY !== undefined) {
                        qtySum += it.QUANTITY;
                    }
                }
            });

            $scope.units.push({ mu_name: item.MU_CODE, mu_code_value: (parseFloat(qtySum)).toFixed(4) });
            if (item.QUANTITY !== undefined) {
                totalQty += item.QUANTITY;
                $scope.totalQty = (parseFloat(totalQty)).toFixed(4);
            }
        });
        $scope.units = _.uniq($scope.units, JSON.stringify);
    }
    //$scope.TOTAL_QUANTITY = 0;
    // Fetch currency data from API

    //$scope.addProduct = function () {
    //    $http.get("/api/QuotationApi/ItemDetails")
    //        .then(function (response) {
    //            // Assuming response.data is an array of objects with 'ItemDescription' and 'ItemCode' properties
    //            $scope.ItemSelect.dataSource.data = response.data;
    //            $scope.productFormList.push({
    //                ID: $scope.counterProduct,
    //                ItemDescription: "",
    //                UNIT: "",
    //                IMAGE: "",
    //                QUANTITY: "",
    //                UNIT_PRICE: "",
    //                AMOUNT: "",
    //                REMARKS: ""
    //            });
    //        })
    //        .catch(function (error) {
    //            console.error('Error fetching item details:', error);
    //        });
    //};

    ////$scope.addProduct();
    //$scope.updateUnit = function (product) {
    //    if (product && product.ItemDescription) {
    //        // Find the item with the matching ItemCode
    //        var selectedItem = $scope.ItemSelect.dataSource.data.find(function (item) {
    //            return item.ItemCode === product.ItemDescription;
    //        });

    //        // If a matching item is found, set the UNIT to the ItemUnit of the selected item
    //        if (selectedItem) {
    //            product.UNIT = selectedItem.ItemUnit;
    //            product.SPECIFICATION = selectedItem.SPECIFICATION;
    //        }
    //    } else {
    //        product.UNIT = ""; 
    //    }
    //};


    //$scope.updateDiscountAmt = function (product) {
    //    product.DISCOUNT_AMOUNT = 0;
    //    if (product.DISCOUNT) {
    //        if ($scope.selectedDiscountType === 'Percentage') {
    //            product.DISCOUNT_AMOUNT = (product.DISCOUNT / 100) * product.AMOUNT;
    //        } else if ($scope.selectedDiscountType === 'Flat') {
    //            product.DISCOUNT_AMOUNT = product.DISCOUNT;
    //        } else if ($scope.selectedDiscountType === 'Quantity') {
    //            product.DISCOUNT_AMOUNT = (product.DISCOUNT * product.QUANTITY);
    //        }
    //    } else {
    //        product.DISCOUNT_AMOUNT = null;
    //    }
    //};

    //$scope.updateAmount = function (product) {
    //    if (product.RATE) {
    //        product.AMOUNT = (product.RATE ? product.RATE : 0) * (product.QUANTITY ? product.QUANTITY : 0);
    //    } else {
    //        product.AMOUNT = null;
    //    }
    //};
    //$scope.updateTaxableAmt = function (product) {
    //    if (product.EXCISE || product.DISCOUNT || product.AMOUNT) {
    //        var taxableAMt = (product.AMOUNT ? product.AMOUNT : 0) - (product.DISCOUNT_AMOUNT ? product.DISCOUNT_AMOUNT : 0) + (product.EXCISE ? product.EXCISE : 0);
    //        product.TAXABLE_AMOUNT = parseFloat(taxableAMt.toFixed(2));
    //        var vatAMt = (13 / 100) * product.TAXABLE_AMOUNT;
    //        product.VAT_AMOUNT = parseFloat(vatAMt.toFixed(2));
    //        var netAmt = vatAMt + taxableAMt;
    //        product.NET_AMOUNT = parseFloat(netAmt.toFixed(2));
    //    } else {
    //        product.TAXABLE_AMOUNT = null;
    //    }
    //};
    //$scope.addRow = function () {
    //    var maxId = Math.max(...$scope.termList.map(term => term.ID));
    //    $scope.counterTerm = maxId !== -Infinity ? maxId + 1 : 1;
    //    if ($scope.counterTerm <= 30) {
    //        $scope.termList.push({
    //            ID: $scope.counterTerm,
    //            TERM_CONDITION: "",
    //        });
    //    } else {
    //        displayPopupNotification("Terms and Condition cannot be more than 30!!","error")
    //    }

    //};

    //$scope.updateQuantity = function () {
    //    var totalQty = 0;
    //    angular.forEach($scope.productFormList, function (item) {
    //        totalQty += item.QUANTITY ?? 0;
    //    });
    //    $scope.totalQty = totalQty ?? totalQty.toFixed(2);
    //};
    //$scope.updateQuantity();

  

    ////To show sum in summary table 
    //$scope.TOTAL_AMOUNT = 0;
    //$scope.TOTAL_DISCOUNT = 0;
    //$scope.TOTAL_EXCISE = 0;
    //$scope.TOTAL_TAXABLE_AMOUNT = 0;
    //$scope.TOTAL_VAT = 0;
    //$scope.TOTAL_NET_AMOUNT = 0;

    //$scope.$watch('productFormList', function (newVal, oldVal) {
    //    // Reset sums
    //    $scope.TOTAL_AMOUNT = 0;
    //    $scope.TOTAL_DISCOUNT = 0;
    //    $scope.TOTAL_EXCISE = 0;
    //    $scope.TOTAL_TAXABLE_AMOUNT = 0;
    //    $scope.TOTAL_VAT = 0;
    //    $scope.TOTAL_NET_AMOUNT = 0;

    //    // Update sums for each product
    //    angular.forEach(newVal, function (product) {
    //        $scope.TOTAL_AMOUNT += (product.AMOUNT || 0.00);
    //        $scope.TOTAL_DISCOUNT += (product.DISCOUNT_AMOUNT || 0.00);
    //        $scope.TOTAL_EXCISE += (product.EXCISE || 0.00);
    //        $scope.TOTAL_TAXABLE_AMOUNT += (product.TAXABLE_AMOUNT || 0.00);
    //    });

    //    // Convert sums to have two decimal places
    //    $scope.TOTAL_AMOUNT = ($scope.TOTAL_AMOUNT).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    //    $scope.TOTAL_DISCOUNT = ($scope.TOTAL_DISCOUNT).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    //    $scope.TOTAL_EXCISE = ($scope.TOTAL_EXCISE).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    //    var totalTaxableAmt = $scope.TOTAL_TAXABLE_AMOUNT;
    //    $scope.TOTAL_TAXABLE_AMOUNT = ($scope.TOTAL_TAXABLE_AMOUNT).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    //    var totalVatAmt = (totalTaxableAmt || 0.00) * 0.13;
    //    $scope.TOTAL_VAT = (totalVatAmt).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });

    //    // Calculate total net amount
    //    $scope.TOTAL_NET_AMOUNT = (totalVatAmt + totalTaxableAmt).toLocaleString('en-IN', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    //}, true);
    $scope.saveData = function () {
        if ($scope.PAN_NO == "") {
            displayPopupNotification("PAN No is required", "warning");
        }
        else if ($scope.PARTY_NAME == "") {
            displayPopupNotification("Party Name is required", "warning");
        }
        else if ($scope.ADDRESS == "") {
            displayPopupNotification("Address is required", "warning");
        }
        else if ($scope.CONTACT_NO == "") {
            displayPopupNotification("Contact No is required", "warning");
        }
        else if ($scope.CURRENCY_RATE == "") {
            displayPopupNotification("Currency Rate is required", "warning");
        }
        else if ($scope.EMAIL == "") {
            displayPopupNotification("Email is required", "warning");
        } else if ($scope.isValidEmail($scope.EMAIL) === 'invalid') {
            displayPopupNotification("Please enter a valid email address", "warning");
        }
        else if ($scope.selectedCurrency == "") {
            displayPopupNotification("Currency is required", "warning");
        }
        else if ($('#deliveryDt').val() == "") {
            displayPopupNotification("Delivery Date is required", "warning");
        }
        else {
            $http.get('/api/ApiQuotation/checkPAN?panNo=' + $scope.PAN_NO + '&tenderNo=' + $scope.TENDER_NO)
                .then(function (response) {
                    if (response.data > 0) {
                        var revise = "Revised " + response.data;
                    } else {
                        var revise = "";
                    }
            var formData = {
                PAN_NO: $scope.PAN_NO,
                TENDER_NO: $scope.TENDER_NO,
                PARTY_NAME: $scope.PARTY_NAME,
                ADDRESS: $scope.ADDRESS,
                CONTACT_NO: $scope.CONTACT_NO,
                CUSTOMER_CODE: $scope.customerCode,
                EMAIL: $scope.EMAIL,
                CURRENCY: $scope.selectedCurrency,
                CURRENCY_RATE: $scope.CURRENCY_RATE,
                DELIVERY_DATE: $('#englishdatedocument').val(),
                TOTAL_AMOUNT: $scope.TOTAL_AMOUNT,
                TOTAL_DISCOUNT: $scope.TOTAL_DISCOUNT,
                TOTAL_EXCISE: $scope.TOTAL_EXCISE,
                TOTAL_TAXABLE_AMOUNT: $scope.TOTAL_TAXABLE_AMOUNT,
                TOTAL_VAT: $scope.TOTAL_VAT,
                REVISE: revise,
                TOTAL_NET_AMOUNT: $scope.TOTAL_NET_AMOUNT,
                DISCOUNT_TYPE: $scope.selectedDiscountType,
                COMPANY_CODE: $scope.companyCode,
                BRANCH_CODE: $scope.branchCode,
                CREATED_BY: $scope.createdBy,
                Item_Detail: [],
                TermsCondition: []
            };
            var rateEmpty = false; // Flag to check if any rate is empty

            angular.forEach($scope.productFormList, function (itemDetails) {
                if (itemDetails.RATE == "" || typeof itemDetails.RATE === "undefined") {
                    displayPopupNotification("Rate is required", "warning");
                    rateEmpty = true;
                } else {
                    var itemDetail = {
                        ITEM_CODE: itemDetails.ITEM,
                        RATE: itemDetails.RATE,
                        AMOUNT: itemDetails.AMOUNT,
                        DISCOUNT: itemDetails.DISCOUNT,
                        DISCOUNT_AMOUNT: itemDetails.DISCOUNT_AMOUNT,
                        EXCISE: itemDetails.EXCISE,
                        TAXABLE_AMOUNT: itemDetails.TAXABLE_AMOUNT,
                        VAT_AMOUNT: itemDetails.VAT_AMOUNT,
                        NET_AMOUNT: itemDetails.NET_AMOUNT
                    };
                    formData.Item_Detail.push(itemDetail);
                }
            });
            if (!rateEmpty) { // Proceed with HTTP POST only if all rates are filled
                angular.forEach($scope.termList, function (termCondition) {
                    // Check if the term and condition is not empty
                    if (termCondition.TERM_CONDITION.trim() !== "") {
                        var termConditions = {
                            TERM_CONDITION: termCondition.TERM_CONDITION
                        };
                        formData.TermsCondition.push(termConditions);
                    }
                });
                
                $http.post('/api/ApiQuotation/SaveFormData', formData)
                    .then(function (response) {
                        $scope.Cancel();
                        $scope.formSubmitted = true;
                        $scope.tenderNo = response.data.data.tenderNo; // Access the TENDER_NO from the response
                        $scope.quotationNo = response.data.data.quotationNo;
                        //myInventoryDropzone.processQueue();
                        window.location.reload();
                        var message = response.data.MESSAGE;
                        window.location.href = '/Quotation/Message';
                    })
                    .catch(function (error) {
                        var message = error;
                        displayPopupNotification(message, "error");
                    });
                    }
             })
        }
    };
    $scope.addTerm = function () {
        $scope.termList.push({
            ID: $scope.counterTerm,
            TERM_CONDITION: ""
                })
    };
    $scope.addTerm();
    $scope.deleteRow = function (index) {
        var deletedTerm = $scope.termList.splice(index, 1)[0];
        delete deletedTerm.ID;
        $scope.termList.splice(index, 1); // Remove the row at the specified index
            if (index === 0) {
                $scope.addTerm();
        }
    };
    $scope.Cancel = function () {
        $("#englishdatedocument").data("kendoDatePicker").value(null);
        $scope.PAN_NO = "";
        $scope.TENDER_NO = "";
        $scope.PARTY_NAME = "";
        $scope.ADDRESS = "";
        $scope.CONTACT_NO = "";
        $scope.EMAIL = "";
        $scope.ISSUE_DATE = "",
        $scope.VALID_DATE="",
        $scope.selectedCurrency = "";
        $scope.CURRENCY_RATE = "";
        $scope.TOTAL_AMOUNT = "";
        $scope.TOTAL_DISCOUNT = "";
        $scope.TOTAL_EXCISE = "";
        $scope.TOTAL_TAXABLE_AMOUNT = "";
        $scope.TOTAL_VAT = "";
        $scope.TOTAL_NET_AMOUNT = "";
        $scope.TOTAL_QUANTITY = "";
        $scope.productFormList.forEach(function (product) {
            product.ItemDescription = '';
            product.SPECIFICATION = '';
            product.CATEGORY = '';
            product.BRAND_NAME = '';
            product.INTERFACE = '';
            product.TYPE = '';
            product.LAMINATION = '';
            product.ITEM_SIZE = '';
            product.THICKNESS = '';
            product.COLOR = '';
            product.GRADE = '';
            product.SIZE_LENGHT = '';
            product.SIZE_WIDTH = '';
            product.IMAGE = ''; // Clear the image field
            product.UNIT = '';
            product.QUANTITY = '';
            product.AMOUNT='',
            product.DISCOUNT = '',
            product.DISCOUNT_AMOUNT = '',
            product.EXCISE = '',
            product.TAXABLE_AMOUNT = '',
            product.VAT_AMOUNT = '',
            product.NET_AMOUNT = ''
        });
        $scope.termList.forEach(function (term) {
            term.ID = '';
            term.TERM_CONDITION = '';
        });
        var fileInputs = document.querySelectorAll('input[type="file"]');
        fileInputs.forEach(function (input) {
            input.value = '';
        })
    };
    $scope.isValidEmail = function (email) {
        var emailPattern = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
        return emailPattern.test(email) ? 'valid' : 'invalid';
    };


    var formUrl = "/api/QuotationApi/GetCategoryList";
    $http.get(formUrl).then(function (response) {
        if (response.data.length != 0) {
            $scope.categoryList = response.data;
        }
    });

    function setDataOnModal(rows, i) {
        var tempCopy = angular.copy($scope.childModelTemplate);
        $scope.ChildFormElement.push({ element: $scope.aditionalChildFormElement });
        $scope.childModels.push($scope.getObjWithKeysFromOtherObj(tempCopy, rows[i]));
        setTimeout(function () {
            //console.log("setDataModal=================>>>" + JSON.stringify(rows));
            if ($scope.childModels[0].hasOwnProperty("ITEM_CODE")) {
                $("#products_" + i).data('kendoComboBox').dataSource.data([{ ItemCode: rows[i].ITEM_CODE, ItemDescription: rows[i].ITEM_EDESC, Type: "code" }]);
            }
            if ($scope.masterModels.hasOwnProperty("CUSTOMER_CODE")) {
                $("#customers").data('kendoComboBox').dataSource.data([{ CustomerCode: rows[0].CUSTOMER_CODE, CustomerName: rows[0].CUSTOMER_EDESC, Type: "code", REGD_OFFICE_EADDRESS: rows[0].REGD_OFFICE_EADDRESS, TPIN_VAT_NO: rows[0].TPIN_VAT_NO, TEL_MOBILE_NO1: rows[0].TEL_MOBILE_NO1, CUSTOMER_NDESC: rows[0].CUSTOMER_NDESC }]);
            }
        }, 0);

        var rowsObj = rows[i];

        var config = {
            async: false
        };
    }

    $scope.masterModelDataFn = function (fn) {
        $http.get('/api/ApiQuotation/GetQuotationDetailByOrderno?id=' + $scope.quotationNo + '&companyCode=' + $scope.companyCode)
            .then(function (result) {
                fn(result);
                $scope.data = result;
            });

    };
    $scope.ConvertEngToNep = function () {

        var englishdateId = $('.englishdate').attr('id');
        var englishdate = $("#" + englishdateId).val();
        var FormattedEngDate = moment(englishdate).format('YYYY-MM-DD');
        var nepalidate = AD2BS(FormattedEngDate);
        var splitteddate = englishdateId.split(/_(.+)/)[1];
        $("#nepaliDate5_" + splitteddate).val(nepalidate);

    };

    $scope.ConvertNepToEng = function ($event) {
        var date = BS2AD($("#nepaliDate5").val());
        $("#englishdatedocument").val($filter('date')(date, "dd-MMM-yyyy"));
        $('#nepaliDate5').trigger('change')
    };

    $scope.ConvertEngToNepang = function (data) {
        $("#nepaliDate5").val(AD2BS(data));
    };
    $scope.ConverEngToNep = function (data) {
        $("#nepaliDate1").val(AD2BS(data));
    };
    $scope.ConvertEnToNep = function (data) {
        $("#nepaliDate2").val(AD2BS(data));
    };
    $scope.monthSelectorOptions = {
        open: function () {
            var calendar = this.dateView.calendar;

            calendar.wrapper.width(this.wrapper.width() + 100);
        },
        change: function () {
            var date = new Date();
            date.setDate(date.getDate() - $scope.formBackDays);
            var minDate = dateSet(date);
            var maxDate = dateSet(new Date());
            var selecteddate = dateSet(this.value());
            if ((selecteddate > maxDate) || (selecteddate < minDate)) {
                alert("Selected date not available");
                $("#englishdatedocument").focus();
                var months = ["jan", "feb", "mar", "apr", "may", "jun", "jul", "aug", "sep", "oct", "nov", "dec"];
                var curDate = new Date();
                curDate = curDate.getDate() + "-" + months[curDate.getMonth()] + "-" + curDate.getFullYear();
                $("#englishdatedocument").val(curDate);
            }
            $scope.ConvertEngToNepang(kendo.toString(this.value(), 'yyyy-MM-dd'))
        },
        format: "dd-MMM-yyyy",

        // specifies that DateInput is used for masking the input element
        dateInput: true
    };
     $scope.monthOptions = {
    open: function () {
        var calendar = this.dateView.calendar;
        calendar.wrapper.width(this.wrapper.width() + 100);
    },
    change: function () {
        var selected = dateSet(this.value());
        var currentDate = moment(); 
        var selectedDate = moment(selected); 

        if (selectedDate.isBefore(currentDate)) {
            displayPopupNotification("Delivery Date cannot be less than the current Date", "error");
            $("#englishdatedocument2").focus();

            // Reset Nepali Date
            var curDate = new Date();
            var nepDate = AD2BS(curDate.getFullYear() + '-' + (curDate.getMonth() + 1) + '-' + curDate.getDate());
            $("#nepaliDate2").val(nepDate);

            // Reset English Date
            var months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
            var curEngDate = curDate.getDate() + '-' + months[curDate.getMonth()] + '-' + curDate.getFullYear();
            $("#englishdatedocument2").val(curEngDate);
        } else {
            $scope.ConvertEnToNep(kendo.toString(this.value(), 'yyyy-MM-dd'));
        }
    },
    format: "dd-MMM-yyyy",
    dateInput: true // specifies that DateInput is used for masking the input element
};
    $scope.openImage = function (imageUrl) {
        window.open(imageUrl, '_blank');
    };
    $scope.getObjWithKeysFromOtherObj = function (objKeys, objKeyswithData, itemList) {
        var keys = Object.keys(objKeys);
        var result = {};
        for (var i = 0; i < keys.length; i++) {
            if (keys[i] === "IMAGE") {
                var imageUrl = window.location.protocol + "//" + window.location.host + "/Areas/NeoERP.QuotationManagement/Image/Items/" + objKeyswithData[keys[i]];
                result[keys[i]] = imageUrl;
            }
            else if (keys[i] === "CATEGORY") {
                var selectedCategory = $scope.categoryList.find(function (category) {
                    return category.CATEGORY_EDESC === objKeyswithData[keys[i]];
                });
                if (selectedCategory) {
                    result[keys[i]] = selectedCategory.CATEGORY;
                }
            } else {
                result[keys[i]] = objKeyswithData[keys[i]];
            }
        }
        return result;
    };

    $scope.monthSelectorOptionsSingle = {
        open: function () {
            var calendar = this.dateView.calendar;
            calendar.wrapper.width(this.wrapper.width() - 6);
        },
        change: function () {
            var issueDt = moment($("#englishdatedocument").val(), 'DD-MMM-YYYY');
            var validUntilDt = moment($("#englishdatedocument1").val(), 'DD-MMM-YYYY');

            if (issueDt.isAfter(validUntilDt)) {
                displayPopupNotification("To be Deliver Date cannot be less than Date/Miti date.", "error");
                $("#englishdatedocument1").focus();

                // Reset Nepali Date
                var curDate = new Date();
                var nepDate = AD2BS(curDate.getFullYear() + '-' + (curDate.getMonth() + 1) + '-' + curDate.getDate());
                $("#nepaliDate1").val(nepDate);

                // Reset English Date
                var months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
                var curEngDate = curDate.getDate() + '-' + months[curDate.getMonth()] + '-' + curDate.getFullYear();
                $("#englishdatedocument1").val(curEngDate);
            }
            $scope.ConverEngToNep(kendo.toString(this.value(), 'yyyy-MM-dd'));
        },
        format: "dd-MMM-yyyy",
        dateInput: true // specifies that DateInput is used for masking the input element
    };
    $scope.TodayDate = AD2BS(moment($("#englishdatedocument").val()).format('YYYY-MM-DD'));

    $scope.getNepaliDate = function (date) {
        return AD2BS(moment(date).format('YYYY-MM-DD'));
    }

    $scope.getmucode = function (index, productId) {

        try {
            var pId = $.isNumeric(parseInt(productId));
            if (pId === false) {
                throw "";
            }
            var response = $http({
                method: "GET",
                url: "/api/QuotationApi/GetMUCodeByProductId",
                params: { productId: productId },
                contentType: "application/json",
                dataType: "json"
            });
            return response.then(function (data) {
                //
                if (!data == "") {
                    $scope.childModels[index].MU_CODE = data.data[0].ItemUnit;
                    $scope.childModels[index].INTERFACE = data.data[0].INTERFACE;
                    $scope.childModels[index].LAMINATION = data.data[0].LAMINATION;
                    $scope.childModels[index].THICKNESS = data.data[0].THICKNESS;
                    $scope.childModels[index].GRADE = data.data[0].GRADE;
                    $scope.childModels[index].SIZE_LENGTH = data.data[0].SIZE_LENGTH;
                    $scope.childModels[index].BRAND_NAME = data.data[0].BRAND_NAME;
                    $scope.childModels[index].TYPE = data.data[0].TYPE;
                    $scope.childModels[index].ITEM_SIZE = data.data[0].ITEM_SIZE;
                    $scope.childModels[index].COLOR = data.data[0].COLOR;
                    $scope.childModels[index].SIZE_WIDTH = data.data[0].SIZE_WIDTH;
                    var selectedCategory = $scope.categoryList.find(function (category) {
                        return category.CATEGORY_EDESC === data.data[0].Category;
                    });
                    if (selectedCategory) {
                        $scope.childModels[index].CATEGORY = selectedCategory.CATEGORY;
                    }
                }

            });
        } catch (e) {
            return;
        }

    };
    $scope.someFn = function () {

        angular.forEach($scope.MasterFormElement, function (value, key) {
            if (value.MASTER_CHILD_FLAG == 'M') {
                $scope.masterModels[value.COLUMN_NAME] = value.DEFA_VALUE == null ? "" : value.DEFA_VALUE;
                $scope.masterModels[value.COLUMN_NAME] = value.DEFA_VALUE == null ? "" : value.DEFA_VALUE;
                if (value['COLUMN_NAME'].indexOf('DATE') > -1) {
                    $scope.masterModels[value['COLUMN_NAME']] = $filter('date')(new Date(), 'dd-MMM-yyyy');
                }
            }
        });
        var cml = $scope.childModels.length;
        var sl = parseFloat(cml) - 1;
        $scope.ChildFormElement.splice(0, sl);
        $scope.childModels.splice(0, sl);
        angular.forEach($scope.ChildFormElement[0].element, function (value, key) {

            if ($scope.childModels.length == 0) {
                $scope.childModels.push({});
            }
            if (value.MASTER_CHILD_FLAG == 'C' && value.DISPLAY_FLAG == 'Y') {

                $scope.childModels[0][value.COLUMN_NAME] = value.DEFA_VALUE == null ? "" : value.DEFA_VALUE;
            }
        });
        var response = $http.get('/api/QuotationApi/getTenderNo');
        response.then(function (res) {
            if (res.data != "0") {
                if ($scope.save !== "Update") {
                    $scope.newgenorderno = res.data[0]['TENDER_NO'];
                }
                //$scope.newgenorderno = res.data[0]['TENDER_NO'];
                var primarycolumn = PrimaryColumnForTable($scope.DocumentName);
                //$scope.masterModels[primarycolumn] = res.data;
                $.each($scope.MasterFormElement, function (key, value) {

                    if (value['COLUMN_NAME'].indexOf('DATE') > -1) {

                        //$scope.masterModels[value.COLUMN_NAME] = $filter('date')(new Date(), 'dd-MMM-yyyy');
                        if (value['COLUMN_HEADER'].indexOf('Miti') > -1) {

                            if ($scope.masterModels[value.COLUMN_NAME] == null) {
                                //if (value.DEFA_VALUE == null) {

                                var englishdate = $filter('date')(new Date(), 'yyyy-MM-dd');
                                var nepalidate = AD2BS(englishdate);
                                $("#nepaliDate5").val(nepalidate);
                            }
                            else {
                                var englishdate = $filter('date')(new Date($scope.masterModels[value.COLUMN_NAME]), 'yyyy-MM-dd');
                                var nepalidate = AD2BS(englishdate);
                                $("#nepaliDate5").val(nepalidate);
                            }
                        }
                        var currentDt = $filter('date')(new Date(), 'dd-MMM-yyyy')
                        if (value['COLUMN_HEADER'].indexOf('Delivered') > -1) {

                            if ($scope.masterModels[value.COLUMN_NAME] == currentDt) {
                                $("#nepaliDate1").val('');
                            }
                            else {
                                var englishdate = $filter('date')(new Date($scope.masterModels[value.COLUMN_NAME]), 'yyyy-MM-dd');
                                var nepalidate = AD2BS(englishdate);
                                $("#nepaliDate1").val(nepalidate);
                            }
                        }
                    }
                });
            }
        });
    }
    function PrimaryColumnForTable(tablename) {

        var primarycolumn = "";
        if (tablename == "IP_QUOTATION_INQUIRY") {
            primarycolumn = "QUOTE_NO";
        }
        return primarycolumn;
    }

    // remove child row.
    $scope.remove_child_element = function (index) {

        if ($scope.ChildFormElement.length > 1) {
            $scope.ChildFormElement.splice(index, 1);
            $scope.childModels.splice(index, 1);
        }
    }
    $scope.loadingBtnReset = function () {
        $("#savedocumentformdata").button('reset');
        $(".portlet-title .btn").attr("disabled", false);
    }
    $scope.SaveDocumentFormData = function (param) {
        debugger;
        if ($scope.masterModels.hasOwnProperty("CUSTOMER_CODE")) {
            var master_customer_code = $scope.masterModels.CUSTOMER_CODE;
            if ($(".mcustomer").hasClass("borderRed") || master_customer_code == null || master_customer_code == "" || master_customer_code == undefined) {
                var dataForm = _.findWhere($scope.formDetail, { COLUMN_NAME: "CUSTOMER_CODE" })
                if (dataForm.DEFA_VALUE === "") {
                    displayPopupNotification("Customer Code is required", "warning");
                    return $scope.loadingBtnReset();
                } else {
                    $scope.masterModels.CUSTOMER_CODE = dataForm.DEFA_VALUE;
                }

            }
        };
        if ($scope.masterModels.hasOwnProperty("ISSUE_TYPE_CODE")) {
            var master_issue_type_code = $scope.masterModels.ISSUE_TYPE_CODE;
            if ($(".missuetype").hasClass("borderRed") || master_issue_type_code == null || master_issue_type_code == "" || master_issue_type_code == undefined) {
                displayPopupNotification("Issue Type Code is required", "warning");
                return $scope.loadingBtnReset();
            }
        };
        //check master From Location validation

        if ($scope.masterModels.hasOwnProperty("FROM_LOCATION_CODE")) {
            var master_From_location_code = $scope.masterModels.FROM_LOCATION_CODE;
            if ($(".mlocation").hasClass("borderRed") || master_From_location_code == null || master_From_location_code == "" || master_From_location_code == undefined) {
                displayPopupNotification("Location Code is required", "warning");
                return $scope.loadingBtnReset();
            }
        };


        //check master To Location validation

        if ($scope.masterModels.hasOwnProperty("TO_LOCATION_CODE")) {
            var master_to_location_code = $scope.masterModels.TO_LOCATION_CODE;
            if ($(".mtolocation").hasClass("borderRed") || master_to_location_code == null || master_to_location_code == "" || master_to_location_code == undefined) {
                displayPopupNotification("Location Code is required", "warning");
                return $scope.loadingBtnReset();
            }
        };

        //check master Currency validation

        if ($scope.masterModels.hasOwnProperty("CURRENCY_CODE")) {
            var master_currency_code = $scope.masterModels.CURRENCY_CODE;
            if (master_currency_code == null || master_currency_code == "" || master_currency_code == undefined) {
                var result = _.find($scope.formDetail, function (item) {
                    return item.COLUMN_NAME == "CURRENCY_CODE";
                });
                if (result != null) {
                    master_currency_code = result.DEFA_VALUE;
                }

                var resultExcange = _.find($scope.formDetail, function (item) {
                    return item.COLUMN_NAME == "EXCHANGE_RATE";
                });
                if (resultExcange != null) {
                    $scope.masterModels["EXCHANGE_RATE"] = resultExcange.DEFA_VALUE;
                }
            }

            if ($(".mcurrency").hasClass("borderRed") || master_currency_code == null || master_currency_code == "" || master_currency_code == undefined) {
                displayPopupNotification("Currency Code is required", "warning");
                return $scope.loadingBtnReset();
            }
        };

        //check master supplier code validation
        if ($scope.masterModels.hasOwnProperty("SUPPLIER_CODE")) {

            //   //to solve problem in mastercode binding for update
            if ($scope.orderno !== "undefined") {
                if ($scope.masterModels.SUPPLIER_CODE === '' || $scope.masterModels.SUPPLIER_CODE === null || $scope.masterModels.SUPPLIER_CODE === undefined) {
                    if ($rootScope.suppliervalidation === "") {
                        $scope.masterModels.SUPPLIER_CODE = suppliercodeforupdate;
                    }

                }
            }
            var master_supplier_code = $scope.masterModels.SUPPLIER_CODE;
            if ($(".msupplier").hasClass("borderRed") || master_supplier_code == null || master_supplier_code == "" || master_supplier_code == undefined) {
                displayPopupNotification("Supplier Code is required", "warning");
                return $scope.loadingBtnReset();
            }
        };


        //check master priority code validation
        if ($scope.masterModels.hasOwnProperty("PRIORITY_CODE")) {
            var master_priotrity_code = $scope.masterModels.PRIORITY_CODE;
            if ($(".mprority").hasClass("borderRed") || master_priotrity_code == null || master_priotrity_code == "" || master_priotrity_code == undefined) {
                displayPopupNotification("Priority Code is required", "warning");
                return $scope.loadingBtnReset();
            }
        };


        //check master division code validation
        if ($scope.masterModels.hasOwnProperty("DIVISION_CODE")) {
            var master_division_code = $scope.masterModels.DIVISION_CODE;
            if ($(".mdivision").hasClass("borderRed") || master_division_code == null || master_division_code == "" || master_division_code == undefined) {
                var dataForm = _.findWhere($scope.formDetail, { COLUMN_NAME: "DIVISION_CODE" })
                if (dataForm.DISPLAY_FLAG == "Y") {
                    displayPopupNotification("Division Code is required", "warning");
                    return $scope.loadingBtnReset();
                }
                else {
                    $scope.masterModels.DIVISION_CODE = dataForm.DEFA_VALUE;
                }
            }
        };

        //check master employee code / marketing person validation
        if ($scope.masterModels.hasOwnProperty("EMPLOYEE_CODE")) {
            var master_employee_code = $scope.masterModels.EMPLOYEE_CODE;
            if ($(".memployee").hasClass("borderRed") || master_employee_code == null || master_employee_code == "" || master_employee_code == undefined) {

                var dataForm = _.findWhere($scope.formDetail, { COLUMN_NAME: "EMPLOYEE_CODE" })
                if (dataForm.DISPLAY_FLAG == "Y") {
                    displayPopupNotification("Employee Code is required", "warning");
                    return $scope.loadingBtnReset();
                }
                else {
                    $scope.masterModels.EMPLOYEE_CODE = dataForm.DEFA_VALUE;
                }
            }
        };

        //check master sales type validation
        if ($scope.masterModels.hasOwnProperty("SALES_TYPE_CODE")) {
            var master_salestype = $scope.masterModels.SALES_TYPE_CODE;
            if ($(".msalestype").hasClass("borderRed") || master_salestype == null || master_salestype == "" || master_salestype == undefined) {
                var dataForm = _.findWhere($scope.formDetail, { COLUMN_NAME: "SALES_TYPE_CODE" })
                if (dataForm.DISPLAY_FLAG == "Y") {
                    displayPopupNotification("Sales Type Code is required", "warning");
                    return $scope.loadingBtnReset();
                }
                else {
                    $scope.masterModels.SALES_TYPE_CODE = dataForm.DEFA_VALUE;
                }

            }
        };
        if ($scope.masterModels.hasOwnProperty("TO_DELIVERED_DATE")) {
            var master_salestype = $scope.masterModels.TO_DELIVERED_DATE;
            if ($(".msalestype").hasClass("borderRed") || master_salestype == null || master_salestype == "" || master_salestype == undefined) {
                var dataForm = _.findWhere($scope.formDetail, { COLUMN_NAME: "TO_DELIVERED_DATE" })
                if (dataForm.DISPLAY_FLAG == "Y") {
                    displayPopupNotification("To be Delivered Date is required", "warning");
                    return $scope.loadingBtnReset();
                }
                else {
                    $scope.masterModels.SALES_TYPE_CODE = dataForm.DEFA_VALUE;
                }

            }
        };
        //check master terms days

        if ($scope.masterModels.hasOwnProperty("TERMS_DAY")) {
            var TERMS_DAY_VAL = $scope.masterModels.TERMS_DAY;
            if ($(".termsday").hasClass("borderRed")) {
                displayPopupNotification("Terms of day must be less than 100", "warning");
                return $scope.loadingBtnReset();
            }
        };

        //check child validation
        var childlen = $scope.childModels.length;
        for (var i = 0; i < childlen; i++) {


            //check child  to location validation
            if ($scope.childModels[0].hasOwnProperty("TO_LOCATION_CODE")) {

                var child_from_location = $scope.childModels[i].TO_LOCATION_CODE;
                if ($(".clocation").parent().parent().hasClass("borderRed") || child_from_location == null || child_from_location == "" || child_from_location == undefined) {
                    displayPopupNotification("Location Code is required", "warning");
                    return $scope.loadingBtnReset();
                }
            };
            //check child  from location validation
            if ($scope.childModels[0].hasOwnProperty("FROM_LOCATION_CODE")) {

                var child_from_location = $scope.childModels[i].FROM_LOCATION_CODE;
                if ($(".clocation").parent().parent().hasClass("borderRed") || child_from_location == null || child_from_location == "" || child_from_location == undefined) {
                    displayPopupNotification("Location Code is required", "warning");
                    return $scope.loadingBtnReset();
                }
            };
            //check child product validation
            if ($scope.childModels[0].hasOwnProperty("ITEM_CODE")) {
                ;
                var child_item = $scope.childModels[i].ITEM_CODE;
                if ($(".cproducts").parent().parent().hasClass("borderRed") || child_item == null || child_item == "" || child_item == undefined) {
                    displayPopupNotification("Product Code is required", "warning");
                    return $scope.loadingBtnReset();
                }
            }
            //check child quantity validation
            if ($scope.childModels[0].hasOwnProperty("QUANTITY")) {
                var child_quanity = $scope.childModels[i].QUANTITY;
                if (child_quanity == null || child_quanity == "" || child_quanity == undefined) {
                    displayPopupNotification("Quantity is required", "warning");
                    return $scope.loadingBtnReset();
                };
            }
            //check child cal quantity validation
            if ($scope.childModels[0].hasOwnProperty("CALC_QUANTITY")) {
                var calc_quanity = $scope.childModels[i].CALC_QUANTITY;
                if (calc_quanity === null || calc_quanity === "") {
                    displayPopupNotification("Calculated Quantity is required", "warning");
                    return $scope.loadingBtnReset();
                }
                if (calc_quanity === undefined) {
                    displayPopupNotification("Enter Calculated Quantity Correctly", "error");
                    return $scope.loadingBtnReset();
                }
            }
            //check rate/unit validation
            if ($scope.childModels[0].hasOwnProperty("UNIT_PRICE")) {
                var child_rate = $scope.childModels[i].UNIT_PRICE;
                //if (child_rate == null)
                //{
                //    $scope.childModels[i].UNIT_PRICE = 0;
                //    child_rate = 0;
                //}
                if (child_rate == null || child_rate === "") {
                    displayPopupNotification("Rate/Unit Price is required", "warning");
                    return $scope.loadingBtnReset();
                }
                if (child_rate === undefined) {
                    displayPopupNotification("Enter Amount Correctly", "error");
                    return $scope.loadingBtnReset();
                }
            };

            //check cal_rate/unit validation
            if ($scope.childModels[0].hasOwnProperty("CALC_UNIT_PRICE")) {
                var calc_rate = $scope.childModels[i].CALC_UNIT_PRICE;

                //if (calc_rate == null) {
                //    $scope.childModels[i].CALC_UNIT_PRICE = 0;
                //    calc_rate = 0;
                //}
                if (calc_rate == null || calc_rate === "") {
                    displayPopupNotification("Calcutaled Rate/Unit Price is required", "warning");
                    return $scope.loadingBtnReset();
                }
                if (calc_rate === undefined) {
                    displayPopupNotification("Enter Calcutaled Amount Correctly", "error");
                    return $scope.loadingBtnReset();
                }
            };

            if ($scope.childModels[0].hasOwnProperty("COMPLETED_QUANTITY")) {

                var completed_quantity = $scope.childModels[i].COMPLETED_QUANTITY;
                if (completed_quantity === null || completed_quantity === "") {
                    displayPopupNotification("Calcutaled Completed Quantity  is required", "warning");
                    return $scope.loadingBtnReset();
                }
                if (completed_quantity === undefined) {
                    displayPopupNotification("Enter Completed Quantity Correctly", "error");
                    return $scope.loadingBtnReset();
                }
            };

        };

        //showloader();
        var saveflag = param;
        var tablename = $scope.DocumentName;
        var orderno = $scope.OrderNo;
        var masterelementvalues = $scope.masterModels;
        //var grandtotal = $scope.summary.grandTotal;
        var grandtotal = $scope.adtotal;
        var masterElementJson = {};

        $.each($scope.MasterFormElement, function (key, value) {
            if (value['COLUMN_NAME'].indexOf('DATE') > -1) {

                var date = $scope.masterModels[value.COLUMN_NAME];
                $scope.masterModels[value.COLUMN_NAME] = moment(date).format('DD-MMM-YYYY');
            }
            if (value['COLUMN_NAME'].indexOf('DELIVERED') > -1) {

                var date = $scope.masterModels[value.COLUMN_NAME];
                $scope.masterModels[value.COLUMN_NAME] = moment(date).format('DD-MMM-YYYY');
            }
            if (value['COLUMN_NAME'].indexOf('QUOTE_NO') !== -1) {
                $scope.masterModels[value.COLUMN_NAME] = $scope.newgenorderno;
            }
        });
        var childcolumnkeys = "";
        for (key in $scope.childModels[0]) {


            childcolumnkeys += key + ",";

        }
        $.each($scope.MasterFormElement, function (key, value) {
            if ($scope.masterModels[value.COLUMN_NAME] == undefined || $scope.masterModels[value.COLUMN_NAME] == null) {
                $scope.masterModels[value.COLUMN_NAME] = "";
            }
        });
        var count = 0;
        var totalItems = $scope.childModels.length;
        $.each($scope.childModels, function (i, item) {
            $.each($scope.ChildFormElement, function (key, value) {
                if (value.COLUMN_NAME in item) {
                    if (value.COLUMN_NAME === "ID") {
                        item[value.COLUMN_NAME] = count;
                    } else if (item[value.COLUMN_NAME] === "NaN" || item[value.COLUMN_NAME] === undefined || item[value.COLUMN_NAME] === null) {
                        item[value.COLUMN_NAME] = "";
                    }
                }
            });

            if ('IMAGE' in item) {
                var fileInput = document.getElementById('image_' + i);
                var file = fileInput ? fileInput.files[0] : null;

                if (file) {
                    var reader = new FileReader();
                    reader.onload = (function (childItem, index) {
                        return function (e) {
                            $scope.$apply(function () {
                                childItem['IMAGE'] = e.target.result.split(',')[1]; // Store base64 image data
                                count++;

                                if (count === totalItems) {
                                    sendFormDataToServer();
                                }
                            });
                        };
                    })(item, i);
                    reader.onerror = function (error) {
                        console.error("Error reading file: ", error);
                        displayPopupNotification("Error reading file!!", "error");
                    };
                    reader.readAsDataURL(file);
                } else {
                    item['IMAGE'] = null; // Handle case when there's no image
                    count++;

                    if (count === totalItems) {
                        sendFormDataToServer();
                    }
                }
            } else {
                count++;

                if (count === totalItems) {
                    sendFormDataToServer();
                }
            }
        });

        if (totalItems === 0) {
            sendFormDataToServer();
        }

        function sendFormDataToServer() {
            var model = {
                Table_Name: tablename,
                Master_COLUMN_VALUE: JSON.stringify($scope.masterModels),
                Child_COLUMNS: childcolumnkeys,
                Child_COLUMN_VALUE: JSON.stringify($scope.childModels),
                Order_No: orderno,
                CHARGES: JSON.stringify($scope.data),
            };
            console.log(model);
            var staturl = window.location.protocol + "//" + window.location.host + "/api/QuotationApi/SaveItemData";
            var response = $http({
                method: "POST",
                data: model,
                url: staturl,
                contentType: "application/json",
                dataType: "json"
            });
            return response.then(function (data) {
                if (data.data.MESSAGE == "INSERTED") {
                    DisplayBarNotificationMessage("Voucher Saved Successfully! </br> Voucher no: " + data.data.VoucherNo);
                    $window.location.href = window.location.protocol + "//" + window.location.host + "/QuotationManagement/Home/Index#!QM/QuotationSetup";
                    hideloader();
                }
                else if (data.data.MESSAGE == "UPDATED") {
                    DisplayBarNotificationMessage("Data succesfully updated.", "success");
                    var landingUrl = window.location.protocol + "//" + window.location.host + "/QuotationManagement/Home/Index#!QM/QuotationSetup";
                    $window.location.href = landingUrl;
                    hideloader();
                }
                else {
                    hideloader();
                    displayPopupNotification("Something went wrong!.Please try again later.", "error");
                    //   console.log(response.data.MESSAGE);
                }
            },
                function errorCallback(response) {
                    hideloader();
                    $scope.loadingBtnReset();
                    displayPopupNotification("Something went wrong!.Please try again later.", "error");
                    // console.log(response.data.MESSAGE);

                });
        }
    };
    $scope.saveSelection = function () {
        angular.forEach($scope.aditionalElement, function (element) {
            if (element.selected) {
                element.DISPLAY_FLAG = 'Y';
                angular.forEach($scope.ChildFormElement, function (row) {
                    let existingIndex = row.element.findIndex(col => col.COLUMN_NAME === element.COLUMN_NAME);
                    if (existingIndex === -1) {
                        row.element.push(angular.copy(element));
                    } else {
                        row.element[existingIndex] = angular.copy(element);
                    }
                });
                let aditionalElementIndex = $scope.aditionalChildFormElement.findIndex(col => col.COLUMN_NAME === element.COLUMN_NAME);
                if (aditionalElementIndex === -1) {
                    $scope.aditionalChildFormElement.push(angular.copy(element));
                } else {
                    $scope.aditionalChildFormElement[aditionalElementIndex] = angular.copy(element);
                }
            } else {
                element.DISPLAY_FLAG = 'N';
                angular.forEach($scope.ChildFormElement, function (row) {
                    row.element = row.element.filter(col => col.COLUMN_NAME !== element.COLUMN_NAME);
                });
                $scope.aditionalChildFormElement = $scope.aditionalChildFormElement.filter(col => col.COLUMN_NAME !== element.COLUMN_NAME);

            }
        });

        // Update the childModelTemplate with the selected elements
        $scope.childModelTemplate = {};
        angular.forEach($scope.ChildFormElement[0].element, function (item) {
            $scope.childModelTemplate[item.COLUMN_NAME] = "";
        });

        // Sort all rows by SERIAL_NO or any other sorting criteria
        angular.forEach($scope.ChildFormElement, function (row) {
            row.element = $filter('orderBy')(row.element, 'SERIAL_NO');
        });

        // Update each childModels to reflect the changes in childModelTemplate
        angular.forEach($scope.childModels, function (model) {
            angular.forEach($scope.childModelTemplate, function (value, key) {
                if (!model.hasOwnProperty(key)) {
                    model[key] = "";
                }
            });
        });

        // Call your data retrieval function to update $scope.data
        var rows = $scope.data.data;
        if (rows.length > 0) {
            $scope.masterModels = {};
            var masterModel = angular.copy($scope.masterModelTemplate);
            var savedData = $scope.getObjWithKeysFromOtherObj(masterModel, $scope.data.data[0]);
            //savedData.TO_DELIVERED_DATE = formatDate(savedData.TO_DELIVERED_DATE);
            $scope.newgenorderno = savedData.QUOTE_NO;
            $scope.masterModels = savedData;
            //console.log($scope.masterModels);
            //to solve problem in suppliercode binding for update purpose
            suppliercodeforupdate = $scope.masterModels.SUPPLIER_CODE;
            //

            if ($scope.quotationNo != undefined && $scope.quotationNo != "undefined") {
                $scope.ChildFormElement = [];
                $scope.childModels = [];
                $scope.newgenorderno = "";
                $scope.save = "Update";
            }
            for (var i = 0; i < rows.length; i++) {
                setDataOnModal(rows, i);
            }
        }

        $('#myModal').modal('hide');
    };

    $scope.add_child_element = function () {
        let newRowTemplate = angular.copy($scope.ChildFormElement[0].element);

        $scope.ChildFormElement.push({ element: newRowTemplate });
        let newChildModel = {};
        angular.forEach(newRowTemplate, function (childelementvalue) {
            newChildModel[childelementvalue.COLUMN_NAME] = childelementvalue.DEFA_VALUE == null ? "" : childelementvalue.DEFA_VALUE;
        });
        $scope.childModels.push(newChildModel);
    };
    $scope.originalSelections = [];
    $scope.cancelSelection = function () {
        angular.forEach($scope.aditionalElement, function (element, index) {
            element.selected = $scope.originalSelections[index];
        });
        $('#myModal').modal('hide');
    };
    $scope.CancelForm = function () {
        $scope.masterModels = {};
        $scope.masterChildData = null;
        $scope.masterModelTemplate = {};
        $scope.MasterFormElement = [];
        $scope.ChildFormElement = [{ element: [] }];
        $scope.aditionalChildFormElement = [];
        $scope.childModelTemplate = {};
        $scope.specificationModel = {};
        $scope.childModels = [];
        $scope.freeze_master_ref_flag = 'N';
        $scope.newgenorderno = "";
        $scope.aditionalElement = [];
        $scope.categoryList = "";
        $scope.data = [];
        $window.location.href = "/QuotationManagement/Home/Index#!QM/QuotationSetup";
    };
});


