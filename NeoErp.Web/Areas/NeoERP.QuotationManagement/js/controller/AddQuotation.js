QMModule.controller('AddQuotation', function ($scope, $rootScope, $http, $filter, $routeParams, $window,$q) {
    $scope.save = "Save";
    var dt = $q.defer();
    if ($routeParams.id != undefined) {
        $scope.OrderNo = $routeParams.id.split(new RegExp('_', 'i')).join('/');
    }
    else {$scope.OrderNo = "undefined"; }
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

    var formUrl = "/api/QuotationApi/GetCategoryList";
    $http.get(formUrl).then(function (response) {
        if (response.data.length != 0) {
            $scope.categoryList = response.data;
        }
    });
    $http.get('/api/QuotationApi/GetFormDetailSetup')
        .then(function (response) {
            $scope.formDetail = response.data;
        if ($scope.formDetail.length > 0) {
            debugger;
            $scope.DocumentName = $scope.formDetail[0].TABLE_NAME;
            $scope.companycode = $scope.formDetail[0].COMPANY_CODE;
            $scope.freeze_master_ref_flag = $scope.formDetail[0].FREEZE_MASTER_REF_FLAG;
        }
        var values = $scope.formDetail;
        //collection of Master elements
        angular.forEach(values,
            function (value, key) {
                if (value.MASTER_CHILD_FLAG == 'M' && value.DISPLAY_FLAG == 'Y') {
                    $scope.masterModels[value.COLUMN_NAME] = value.DEFA_VALUE == null ? "" : value.DEFA_VALUE;
                    $scope.masterModelTemplate[value.COLUMN_NAME] = value.DEFA_VALUE == null ? "" : value.DEFA_VALUE;
                }
                if (value.MASTER_CHILD_FLAG == 'M' && value.DISPLAY_FLAG == 'Y') {

                    this.push(value);
                    if (value['COLUMN_NAME'].indexOf('DATE') > -1) {
                        $scope.masterModelTemplate[value['COLUMN_NAME']] = $filter('date')(new Date(), 'dd-MMM-yyyy');
                    } else {
                        $scope.masterModelTemplate[value['COLUMN_NAME']] = "";
                    }
                    $scope.masterModels[value.COLUMN_NAME] = value.DEFA_VALUE;
                }
            },
            $scope.MasterFormElement);

        //collection of child elements.
        angular.forEach(values,
            function (value, key) {
                if (value.MASTER_CHILD_FLAG == 'C' && value.DISPLAY_FLAG == 'Y') {
                    this.push(value);
                    if (value['COLUMN_NAME'] === "ITEM_CODE") {
                        $rootScope.ITEM_CODE_DEFAULTVAL = value.DEFA_VALUE == null ? "" : value.DEFA_VALUE;
                    }
                }
            },
            $scope.ChildFormElement[0].element);

        angular.forEach(values,
            function (value, key) {
                if (value.MASTER_CHILD_FLAG == 'C' && value.DISPLAY_FLAG == 'Y') {
                    this.push(value);
                    $scope.childModelTemplate[value['COLUMN_NAME']] = "";
                }
            },
            $scope.aditionalChildFormElement);

            //More specification 

            angular.forEach(values,
                function (value, key) {
                    if (value.MASTER_CHILD_FLAG == 'C' && value.DISPLAY_FLAG == 'N') {
                        this.push(value);
                        $scope.specificationModel[value['COLUMN_NAME']] = "";
                    }
                },
                $scope.aditionalElement);

            var tempFn = function (response) {
                var rows = response.data;
                if (rows.length > 0) {
                    $scope.masterModels = {};
                    var masterModel = angular.copy($scope.masterModelTemplate);
                    var savedData = $scope.getObjWithKeysFromOtherObj(masterModel, response.data[0]);
                    $scope.newgenorderno = savedData.QUOTE_NO;
                    $scope.masterModels = savedData;
                    //to solve problem in suppliercode binding for update purpose
                    suppliercodeforupdate = $scope.masterModels.SUPPLIER_CODE;
                    //

                    if ($scope.OrderNo != undefined && $scope.OrderNo != "undefined") {
                        $scope.ChildFormElement = [];
                        $scope.childModels = [];
                        $scope.newgenorderno = "";
                        $scope.save = "Update";
                    }
                    for (var i = 0; i < rows.length; i++) {
                        setDataOnModal(rows, i);
                    }
                }
                else {
                }
            };

        // check master transaction type .. if cr/dr disable child element
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
                };
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
        $http.get('/api/QuotationApi/GetQuotationDetailByOrderno?voucherNo=' + $scope.OrderNo)
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
            else if(keys[i] === "CATEGORY")
            {
                var selectedCategory = $scope.categoryList.find(function (category) {
                    return category.CATEGORY_EDESC === objKeyswithData[keys[i]];
                });
                if (selectedCategory) {
                    result[keys[i]] = selectedCategory.CATEGORY;
                }
            }else {
                    result[keys[i]] = objKeyswithData[keys[i]];
                }
        }
        return result;
    };

    $scope.monthSelectorOptionsSingle = {
        open: function () {
            var calendar = this.dateView.calendar;
            calendar.wrapper.width(this.wrapper.width() + 100);
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
                    }                }

            });
        } catch (e) {
            return;
        }

    };
    $scope.someFn = function () {

        angular.forEach($scope.MasterFormElement, function (value, key) {
            if (value.MASTER_CHILD_FLAG == 'M' && value.DISPLAY_FLAG == 'Y') {
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
                        var currentDt=$filter('date')(new Date(), 'dd-MMM-yyyy')
                        if (value['COLUMN_HEADER'].indexOf('Delivered') > -1) {

                            if ($scope.masterModels[value.COLUMN_NAME] == null) {
                                var englishdate = $filter('date')(new Date(), 'yyyy-MM-dd');
                                var nepalidate = AD2BS(englishdate);
                                $("#nepaliDate5").val(nepalidate);                            }
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
    $scope.remove_child_element = function (index, id) {
        if (id === "" || id === null || id === undefined) {
            if ($scope.ChildFormElement.length > 1) {
                $scope.ChildFormElement.splice(index, 1);
                $scope.childModels.splice(index, 1);
            }
        } else {
            $http.post('/api/QuotationApi/updateItemsById?id=' + id )
                .then(function (response) {
                    var message = response.data.MESSAGE;
                    displayPopupNotification(message, "success");
                }).catch(function (error) {
                    var message = 'Error in displaying project!!';
                    displayPopupNotification(message, "error");
                });
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
                    if (item[value.COLUMN_NAME] === "NaN" || item[value.COLUMN_NAME] === undefined || item[value.COLUMN_NAME] === null) {
                        item[value.COLUMN_NAME] = "";
                    }
                }
            }); 
            if ('ID' in item) {
                if (item['ID'] === "" || item['ID'] === null || item['ID'] === undefined)
                {
                    item['ID'] = 0;
                }
               
            }
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
                Order_No: orderno
            };
            console.log(model);
            var staturl = window.location.protocol + "//" + window.location.host + "/api/QuotationApi/SaveItemData";
            var response = $http({
                method: "POST",
                url: staturl,
                data: model,
                headers: {
                    'Content-Type': 'application/json'
                }
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

            if ($scope.OrderNo != undefined && $scope.OrderNo != "undefined") {
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
    $scope.generatedUrl = '';

    $scope.generateLink = function () {
        $http.get('/api/QuotationApi/getTenderId?tenderNo=' + $scope.OrderNo)
            .then(function (response) {
                $scope.ID = response.data[0].ID;
                var linkeUrl = window.location.protocol + "//" + window.location.host + "/Quotation/Index?qo=" + $scope.ID;
                $scope.generatedUrl = linkeUrl;
            })
            .catch(function (error) {
                displayPopupNotification("Error fetching ID", "error");
            });
    };

});

