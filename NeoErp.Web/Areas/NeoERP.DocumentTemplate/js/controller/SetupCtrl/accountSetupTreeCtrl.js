DTModule.controller('accountSetupTreeCtrl', function ($scope, $rootScope, $http, $routeParams, $window, $filter, hotkeys) {
    $scope.result = false;
    $scope.saveupdatebtn = "Save";
    $scope.accountsArr;
    $scope.savegroup = false;
    $scope.editFlag = "N";
    $scope.newrootinsertFlag = "Y";
    $scope.treenodeselected = "N";
    $scope.treeSelectedAccountCode = "";
    var dataFillDefered = $.Deferred();
    $scope.accountsetup =
        {
            TRANSACTION_TYPE: "",
            PREFIX_TEXT: "",
            ACC_CODE: "",
            ACC_EDESC: "",
            ACC_NDESC: "",
            TPB_FLAG: "",
            ACC_TYPE_FLAG: "T",
            MASTER_ACC_CODE: "",
            PRE_ACC_CODE: "",
            COMPANY_CODE: "",
            CREATED_BY: "",
            CREATED_DATE: "",
            DELETED_FLAG: "",
            SYN_ROWID: "",
            DELTA_FLAG: "",
            FREEZE_FLAG: "N",
            CURRENT_BALANCE: "",
            LIMIT: "",
            MODIFY_DATE: "",
            ACC_NATURE: "",
            BRANCH_CODE: "",
            SHARE_VALUE: "",
            MODIFY_BY: "",
            IND_VAT_FLAG: "N",
            BANK_ACCOUNT_NO: "",
            PRINTING_FLAG: "N",
            ACC_SNAME: "",
            TEL_NO: "",
            IND_TDS_FLAG: "N",
            ACC_ID: "",
            MOBILE_NO: "",
            EMAIL_ID: "",
            LINK_ID: "",
            CONTACT_PERSON: "",
            GROUP_START_CODE: "",
            GROUP_END_CODE: "",
            DEALER_FLAG: "",
            PARENT_ACC_CODE: "",
        }

    $scope.bankaccountdetail = {
        CONTACT_PERSON: "",
        DEPARTMENT: "",
        DESIGNATION: "",
        ADDRESS: "",
        MOBILE_NO: "",
        PHONE_NO: "",
        EMAIL_ID: ""
    }
    $scope.bankdetailArr = [];
    $scope.bankdetailArr.push($scope.bankaccountdetail);
    $scope.accountsArr = $scope.accountsetup;
    var accountCodeUrl = window.location.protocol + "//" + window.location.host + "/api/TemplateApi/getAccountCodeWithChild";
    $scope.accountGroupDataSource = new kendo.data.DataSource({
        transport: {
            read: {
                url: accountCodeUrl,
            }
        }
    });
    $scope.accountGroupOptions = {
        dataSource: $scope.accountGroupDataSource,
        optionLabel: "<PRIMARY>",
        dataTextField: "ACC_EDESC",
        dataValueField: "ACC_CODE",
        maxSelectedItems: 1,
        valuePrimitive: true,
        autoClose: true,
        dataBound: function () {
            $scope.accountGroupDataSource;
        }
    }
    //show  and hide text box  
    $scope.NatureAccCodeOnChange = function (kendoEvent) {
        if ($scope.accountsArr.ACC_NATURE == "LC" || $scope.accountsArr.ACC_NATURE == "AC")
            $scope.result = true;
        else
            $scope.result = false;
        if ($scope.accountsArr.ACC_NATURE == "LA")
            $scope.share = true;
        else
            $scope.share = false;
    }
    var getAccountCodeByUrl = window.location.protocol + "//" + window.location.host + "/api/TemplateApi/getAccountCode";
    $scope.acctreeData = new kendo.data.HierarchicalDataSource({
        transport: {
            read: {
                url: getAccountCodeByUrl,
                type: 'GET',
                data: function (data, evt) {
                }
            },
        },
        schema: {
            parse: function (data) {
                return data;
            },
            //select: onSelect,
            model: {
                id: "MASTER_ACC_CODE",
                parentId: "PRE_ACC_CODE",
                children: "Items",
                fields: {
                    ACC_CODE: { field: "ACC_CODE", type: "string" },
                    ACC_EDESC: { field: "ACC_EDESC", type: "string" },
                    parentId: { field: "PRE_ACC_CODE", type: "string", defaultValue: "00" },
                }
            }
        }
    });
    //Share Capital (balancesheet and credit)
    var SCNature = [
        { key: "LA", text: "Capital Account" },
        { key: "LB", text: "Current Liabilities" },
        { key: "LC", text: "Loan (Liability)" },
        { key: "LD", text: "Sundry Creditors" },
        { key: "PL", text: "Profit & Loss" },
        { key: "PV", text: "Provision" }
    ];
    //Assets(debit abd balancesheet)
    var FANature = [
        { key: "AA", text: "Fixed Assets" },
        { key: "AA", text: "Capital W.I.P" }
    ];
    //Current Assets(debit and balancesheet)
    var CANature = [
        { key: "AB", text: "Cash In Hand" },
        { key: "AC", text: "Bank Account" },
        { key: "AD", text: "Current Assets" },
        { key: "AE", text: "Sundry Debtors" },
        { key: "AF", text: "Advance" }
        //{ key: "AF", text: " Staff Advance" }
    ];
    //Salaes and Revenue(Credit and Profit & loss)
    var SRNature = [
        { key: "IA", text: "Sales Accounts" },
        { key: "IB", text: "Direct Income" },
        { key: "IC", text: "Indirect Income" }
    ];
    //Manufacturing Expences(debit and profit & loss)
    var MENature = [
        { key: "EA", text: "Manufacturing Ex" },
        { key: "EB", text: "Direct Expenses" },
        { key: "EC", text: "InDirect Expense" },
        { key: "ED", text: "Depreciation" }
    ];
    //Stock and Procurement(debit and profit & loss)
    SPNature = [
        { key: "SE", text: "Inventory" },
        { key: "SA", text: "Opening Stock" },
        { key: "SB", text: "Purchase Accoun" },
        { key: "SC", text: "Stock Transfer" },
        { key: "SD", text: "Closing Stock" }
    ];
    //Reserve & surplus
    var RStureArr = [
        { key: "AA", text: "Fixed Assets" },
        { key: "AB", text: "Cash In Hand" },
        { key: "AC", text: "Bank Account" },
        { key: "AD", text: "Current Assets" },
        { key: "AE", text: "Sundry Debtors" },
        { key: "AF", text: "Advance" },
        { key: "EA", text: "Manufacturing Ex" },
        { key: "EB", text: "Direct Expenses" },
        { key: "EC", text: "InDirect Expense" },
        { key: "ED", text: "Depreciation" },
        { key: "SA", text: "Opening Stock" },
        { key: "SB", text: "Purchase Accoun" },
        { key: "SC", text: "Stock Transfer" },
        { key: "SD", text: "Closing Stock" },
        { key: "SE", text: "Inventory" },
        { key: "LA", text: "Capital Account" },
        { key: "LB", text: "Current Liabilities" },
        { key: "LC", text: "Loan (Liability)" },
        { key: "LD", text: "Sundry Creditors" },
        { key: "PL", text: "Profit & Loss" },
        { key: "PV", text: "Provision" },
        { key: "IA", text: "Sales Accounts" },
        { key: "IB", text: "Direct Income" },
        { key: "IC", text: "Indirect Income" }
    ];
    $scope.acconDataBound = function () {
        $('#accounttree').data("kendoTreeView").expand('.k-item');
    }
    $scope.getDetailByAccCode = function (accId) {
        var getAccountdetaisByUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/getAccountDetailsByAccCode?accCode=" + accId;
        $http({
            method: 'GET',
            url: getAccountdetaisByUrl
        }).then(function successCallback(response) {
            var accNature = response.data.DATA.ACC_NATURE;
            if (accNature == "LA" || accNature == "LB" || accNature == "LC" || accNature == "AC" || accNature == "AB" || accNature == "AC" || accNature == "AE" || accNature == "AD") {
                $scope.accountsArr.TRANSACTION_TYPE = $("#cr").val();
                $scope.accountsArr.TPB_FLAG = $("#balancesheet").val();
                $scope.natureaccountOptions = {
                    dataSource: CANature,
                    dataTextField: "text",
                    dataValueField: "key",
                }
            }
            else if (accNature == "PL") {
                $scope.accountsArr.TRANSACTION_TYPE = $("#cr").val();
                $scope.accountsArr.TPB_FLAG = $("#balancesheet").val();
                $scope.natureaccountOptions = {
                    dataSource: RStureArr,
                    dataTextField: "text",
                    dataValueField: "key"
                }
            }
            else if (accNature == "AA") {
                $scope.accountsArr.TRANSACTION_TYPE = $("#dr").val();
                $scope.accountsArr.TPB_FLAG = $("#balancesheet").val();
                $scope.natureaccountOptions = {
                    dataSource: FANature,
                    dataTextField: "text",
                    dataValueField: "key"
                }
            }
            else if (accNature == "AD") {
                $scope.accountsArr.TRANSACTION_TYPE = $("#dr").val();
                $scope.accountsArr.TPB_FLAG = $("#balancesheet").val();
                $scope.natureaccountOptions = {
                    dataSource: CANature,
                    dataTextField: "text",
                    dataValueField: "key"
                }
            }
            else if (accNature == "IA" || accNature == "IC") {
                $scope.accountsArr.TRANSACTION_TYPE = $("#cr").val();
                $scope.accountsArr.TPB_FLAG = $("#profloss").val();
                $scope.natureaccountOptions = {
                    dataSource: SRNature,
                    dataTextField: "text",
                    dataValueField: "key"
                }
            }
            else if (accNature == "EA" || accNature == "EC" || accNature == "EB") {
                $scope.accountsArr.TRANSACTION_TYPE = $("#dr").val();
                $scope.accountsArr.TPB_FLAG = $("#profloss").val();
                $scope.natureaccountOptions = {
                    dataSource: MENature,
                    dataTextField: "text",
                    dataValueField: "key"
                }
            }
            else if (accNature == "SE" || accNature == "SA" || accNature == "SB") {
                $scope.accountsArr.TRANSACTION_TYPE = $("#dr").val();
                $scope.accountsArr.TPB_FLAG = $("#profloss").val();
                $scope.natureaccountOptions = {
                    dataSource: SPNature,
                    dataTextField: "text",
                    dataValueField: "key"
                }
            }
        }, function errorCallback(response) {
        });
    }
    $scope.fillAccSetupForms = function (accId) {
        var getAccountdetaisByUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/getAccountDetailsByAccCode?accCode=" + accId;
        $http({
            method: 'GET',
            url: getAccountdetaisByUrl
        }).then(function successCallback(response) {
            $scope.accountsetup = response.data.DATA;
            $scope.accountsArr = $scope.accountsetup;
            $scope.accountsArr.ACC_NATURE = $scope.accountsArr.ACC_NATURE;
            $scope.accountsArr.ACC_NATURE = $scope.accountsetup.ACC_NATURE;
            $scope.bankdetailArr.CONTACT_PERSON = $scope.accountsArr.CONTACT_PERSON;
            $scope.bankdetailArr.PHONE_NO = $scope.accountsArr.TEL_NO;
            $scope.bankdetailArr.MOBILE_NO = $scope.accountsArr.MOBILE_NO;
            $scope.bankdetailArr.EMAIL_ID = $scope.accountsArr.EMAIL_ID;
            $scope.bankdetailArr.LINK_ID = $scope.accountsArr.LINK_ID;
            $scope.bankdetailArr.ADDRESS = $scope.accountsArr.ADDRESS;
            $scope.bankdetailArr.DESIGNATION = $scope.accountsArr.DESIGNATION;
            $scope.bankdetailArr.DEPARTMENT = $scope.accountsArr.DEPARTMENT;
            $scope.accountsArr.MASTER_ACC_CODE = $scope.accountsetup.MASTER_ACC_CODE;
            $scope.accountsArr.ACC_TYPE_FLAG = "N";
            $scope.accountsArr.ACC_CODE = $scope.accountsetup.ACC_CODE;
            if ($scope.editFlag == "Y") {
                $scope.groupAccTypeFlag = "N";
                $scope.accountsetup.ACC_TYPE_FLAG = "T";
                $scope.accountsArr.ACC_TYPE_FLAG = "T";
            }
            dataFillDefered.resolve();
            // this callback will be called asynchronously
            // when the response is available
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
        });
    }
    //treeview on select
    $scope.accoptions = {
        loadOnDemand: false,
        select: function (e) {
            var currentItem = e.sender.dataItem(e.node);
            $('#accountGrid').removeClass("show-displaygrid");
            $("#accountGrid").html("");
            $($(this._current).parents('ul')[$(this._current).parents('ul').length - 1]).find('span').removeClass('hasTreeCustomMenu');
            $(this._current.context).find('span').addClass('hasTreeCustomMenu');
            $scope.accountsetup.ACC_CODE = currentItem.AccountId;
            $scope.accountsetup.ACC_EDESC = currentItem.AccountName;
            $scope.accountsetup.MASTER_ACC_CODE = currentItem.masterAccountCode;
            $scope.accountsetup.PRE_ACC_CODE = currentItem.PRE_ACC_CODE;
            $scope.accountsetup.ACC_TYPE_FLAG = currentItem.accounttypeflag;
            $scope.treeSelectedAccountCode = currentItem.AccountId;
            $scope.treenodeselected = "Y";
            $scope.newrootinsertFlag = "N";
        },
    };
    $scope.movescrollbar = function () {
        var element = $(".k-in");
        for (var i = 0; i < element.length; i++) {
            var selectnode = $(element[i]).hasClass("k-state-focused");
            if (selectnode) {
                $("#accounttree").animate({
                    scrollTop: (parseInt(i * 12))
                });
                break;
            }
        }
    }
    $scope.onContextSelect = function (event) {
        if ($scope.accountsetup.ACC_CODE == "")
            return displayPopupNotification("Select account.", "error");;
        $scope.saveupdatebtn = "Save";
        if (event.item.innerText.trim() == "Delete") {
            $scope.accountsArr.ACC_CODE = $scope.accountsetup.ACC_CODE;
            $scope.delete($scope.accountsArr.ACC_CODE, "");
        }
        else if (event.item.innerText.trim() == "Update") {
            dataFillDefered = $.Deferred();
            $scope.editFlag = "N"
            $scope.saveupdatebtn = "Update";
            $scope.clearFields();
            $scope.fillAccSetupForms($scope.accountsetup.ACC_CODE);
            $.when(dataFillDefered).then(function () {
                setAccNature();
                if ($scope.accountsetup.PARENT_ACC_CODE == "00" || $scope.accountsetup.PARENT_ACC_CODE == null) {
                    var tree = $("#masteraccountcode").data("kendoDropDownList");
                    tree.value('')
                }
                else {
                    var tree = $("#masteraccountcode").data("kendoDropDownList");
                    tree.value($scope.accountsetup.PARENT_ACC_CODE);
                }
                var tree1 = $("#masteraccountcode1").data("kendoDropDownList");
                tree1.value($scope.accountsetup.PARENT_ACC_CODE);
                $("#accountModal").modal();
            })
        }
        else if (event.item.innerText.trim() == "Add") {
            $scope.dekh = true;
            dataFillDefered = $.Deferred();
            $scope.savegroup = true;
            if ($scope.accountsetup.ACC_TYPE_FLAG == "N") {
                $scope.fillAccSetupForms($scope.accountsetup.ACC_CODE);
                $scope.getDetailByAccCode($scope.accountsArr.ACC_CODE);
                $.when(dataFillDefered).then(function () {
                    $scope.clearFields();
                    var tree = $("#masteraccountcode").data("kendoDropDownList");
                    tree.value($scope.accountsetup.ACC_CODE)
                    $("#accountModal").modal();
                })
            }
            else {
                var tree = $("#accounttree").data("kendoTreeView");
                tree.dataSource.read();
                displayPopupNotification("Please select the account head first", "warning");
            }
        }
    }
    $scope.saveNewAccount = function (isValid) {
        if ($scope.saveupdatebtn == "Save") {
            if ($scope.accountsArr.TRANSACTION_TYPE == null || $scope.accountsArr.TRANSACTION_TYPE == '' || $scope.accountsArr.TRANSACTION_TYPE == undefined || $scope.accountsArr.TRANSACTION_TYPE == 'undefined') {
                $scope.accountsArr.TRANSACTION_TYPE = $("#dr").val();
            }
            if ($scope.accountsArr.TPB_FLAG == null || $scope.accountsArr.TPB_FLAG == '' || $scope.accountsArr.TPB_FLAG == undefined || $scope.accountsArr.TPB_FLAG == 'undefined') {
                $scope.accountsArr.TPB_FLAG = $("#balancesheet").val();
            }
            var createUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/createNewAccountHead";
            var model = {
                ACC_CODE: $scope.accountsArr.ACC_CODE,
                ACC_EDESC: $scope.accountsArr.ACC_EDESC,
                ACC_NDESC: $scope.accountsArr.ACC_NDESC,
                TRANSACTION_TYPE: $scope.accountsArr.TRANSACTION_TYPE,
                TPB_FLAG: $scope.accountsArr.TPB_FLAG,
                MASTER_ACC_CODE: $scope.accountsArr.MASTER_ACC_CODE,
                PRE_ACC_CODE: $scope.accountsArr.PRE_ACC_CODE,
                ACC_TYPE_FLAG: $scope.accountsArr.ACC_TYPE_FLAG,
                CURRENT_BALANCE: $scope.accountsArr.CURRENT_BALANCE,
                ACC_SNAME: $scope.accountsArr.ACC_SNAME,
                LIMIT: $scope.accountsArr.LIMIT == null ? "" : $scope.accountsArr.LIMIT,
                ACC_NATURE: $scope.accountsArr.ACC_NATURE,
                //ACC_NATURE:$scope.userselected,
                //ACC_NATURE: $("#natureaccount").data("kendoDropDownList").dataItem().key,
                BANK_ACCOUNT_NO: $scope.accountsArr.BANK_ACCOUNT_NO == null ? "" : $scope.accountsArr.BANK_ACCOUNT_NO,
                FREEZE_FLAG: $scope.accountsArr.FREEZE_FLAG == null ? "" : $scope.accountsArr.FREEZE_FLAG,
                PRINTING_FLAG: $scope.accountsArr.PRINTING_FLAG == null ? "" : $scope.accountsArr.PRINTING_FLAG,
                IND_TDS_FLAG: $scope.accountsArr.IND_TDS_FLAG == null ? "" : $scope.accountsArr.IND_TDS_FLAG,
                IND_VAT_FLAG: $scope.accountsArr.IND_VAT_FLAG == null ? "" : $scope.accountsArr.IND_VAT_FLAG,
                GROUP_START_CODE: $scope.accountsArr.GROUP_START_CODE,
                GROUP_END_CODE: $scope.accountsArr.GROUP_END_CODE,
                PREFIX_TEXT: $scope.accountsArr.PREFIX_TEXT,
                ACC_ID: $scope.accountsArr.ACC_ID,
                CONTACT_PERSON: $scope.bankdetailArr.CONTACT_PERSON,
                TEL_NO: $scope.bankdetailArr.PHONE_NO,
                MOBILE_NO: $scope.bankdetailArr.MOBILE_NO,
                EMAIL_ID: $scope.bankdetailArr.EMAIL_ID,
                LINK_ID: $scope.accountsArr.LINK_ID,
                SHARE_VALUE: $scope.accountsArr.SHARE_VALUE,
                IND_MDF_FLAG: $scope.accountsArr.IND_MDF_FLAG
            }
            if (!isValid) {
                displayPopupNotification("Input fields are not valid. Please review and try again", "warning");
                return;
            }
            $http({
                method: 'POST',
                url: createUrl,
                data: model
            }).then(function successCallback(response) {
                if (response.data.MESSAGE == "INSERTED") {
                    $scope.accountsArr = [];
                    if ($scope.accountsetup.ACC_TYPE_FLAG !== "T") {
                        var tree = $("#accounttree").data("kendoTreeView");
                        tree.dataSource.read();
                    }
                    var grid = $("#kGrid").data("kendo-grid");
                    if (grid != undefined) {
                        grid.dataSource.read();
                    }
                    var ddl = $("#masteraccountcode").data("kendoDropDownList");
                    if (ddl != undefined)
                        ddl.dataSource.read();

                    if ($scope.savegroup == false) { $("#accountModal").modal("toggle"); }
                    else { $("#accountModal").modal("toggle"); }
                    //$scope.treenodeselected = "N";
                    displayPopupNotification("Data succesfully saved ", "success");
                }
                else {
                    displayPopupNotification("Something went wrong.Please try again later.", "error");
                }
                // this callback will be called asynchronously
                // when the response is available
            }, function errorCallback(response) {
                displayPopupNotification("Something went wrong.Please try again later.", "error");
                // called asynchronously if an error occurs
                // or server returns response with an error status.
            });
        }
        else {
            var updateUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/updateAccountByAccCode";
            var model = {
                ACC_CODE: $scope.accountsArr.ACC_CODE,
                ACC_EDESC: $scope.accountsArr.ACC_EDESC,
                ACC_NDESC: $scope.accountsArr.ACC_NDESC,
                TRANSACTION_TYPE: $scope.accountsArr.TRANSACTION_TYPE,
                TPB_FLAG: $scope.accountsArr.TPB_FLAG,
                MASTER_ACC_CODE: $scope.accountsArr.MASTER_ACC_CODE,
                PRE_ACC_CODE: $scope.accountsArr.PRE_ACC_CODE,
                ACC_TYPE_FLAG: $scope.accountsArr.ACC_TYPE_FLAG,
                CURRENT_BALANCE: $scope.accountsArr.CURRENT_BALANCE,
                ACC_SNAME: $scope.accountsArr.ACC_SNAME,
                LIMIT: $scope.accountsArr.LIMIT,
                ACC_NATURE: $scope.accountsArr.ACC_NATURE,
                //ACC_NATURE: $scope.userselected,
                //ACC_NATURE: $("#natureaccount").data("kendoDropDownList").dataItem().key,
                BANK_ACCOUNT_NO: $scope.accountsArr.BANK_ACCOUNT_NO == null ? "" : $scope.accountsArr.BANK_ACCOUNT_NO,
                FREEZE_FLAG: $scope.accountsArr.FREEZE_FLAG == null ? "" : $scope.accountsArr.FREEZE_FLAG,
                PRINTING_FLAG: $scope.accountsArr.PRINTING_FLAG == null ? "" : $scope.accountsArr.PRINTING_FLAG,
                IND_TDS_FLAG: $scope.accountsArr.IND_TDS_FLAG == null ? "" : $scope.accountsArr.IND_TDS_FLAG,
                IND_VAT_FLAG: $scope.accountsArr.IND_VAT_FLAG == null ? "" : $scope.accountsArr.IND_VAT_FLAG,
                GROUP_START_CODE: $scope.accountsArr.GROUP_START_CODE,
                GROUP_END_CODE: $scope.accountsArr.GROUP_END_CODE,
                PREFIX_TEXT: $scope.accountsArr.PREFIX_TEXT,
                ACC_ID: $scope.accountsArr.ACC_ID,
                CONTACT_PERSON: $scope.bankdetailArr.CONTACT_PERSON,
                TEL_NO: $scope.bankdetailArr.PHONE_NO,
                MOBILE_NO: $scope.bankdetailArr.MOBILE_NO,
                EMAIL_ID: $scope.bankdetailArr.EMAIL_ID,
                LINK_ID: $scope.accountsArr.LINK_ID,
                SHARE_VALUE: $scope.accountsArr.SHARE_VALUE,
                IND_MDF_FLAG: $scope.accountsArr.IND_MDF_FLAG
            }
            $scope.saveupdatebtn = "Update";
            $http({
                method: 'POST',
                url: updateUrl,
                data: model
            }).then(function successCallback(response) {
                if (response.data.MESSAGE == "UPDATED") {
                    $scope.accountsArr = [];
                    if ($scope.accountsetup.ACC_TYPE_FLAG !== "T") {
                        var tree = $("#accounttree").data("kendoTreeView");
                        tree.dataSource.read();
                    }
                    var ddl = $("#masteraccountcode").data("kendoDropDownList");
                    if (ddl != undefined)
                        ddl.dataSource.read();
                    var grid = $("#kGrid").data("kendo-grid");
                    if (grid != undefined) {
                        grid.dataSource.read();
                    }
                    $("#accountModal").modal("toggle");
                    //$scope.treenodeselected = "N";
                    displayPopupNotification("Data succesfully updated ", "success");
                }
                if (response.data.MESSAGE == "ERROR") {
                    displayPopupNotification("Something went wrong.Please try again later.", "error");
                }
                // this callback will be called asynchronously
                // when the response is available
            }, function errorCallback(response) {
                displayPopupNotification("Something went wrong.Please try again later.", "error");
                // called asynchronously if an error occurs
                // or server returns response with an error status.
            });
        }
    }
    $scope.BindGrid = function (groupId) {
        $(".topsearch").show();
        $scope.treenodeselected = "Y";
        var url = null;
        if (groupId == "All") {
            if ($('#acctxtSearchString').val() == null || $('#acctxtSearchString').val() == '' || $('#acctxtSearchString').val() == undefined || $('#acctxtSearchString').val() == 'undefined') {
                alert('Input is empty or undefined.');
                return;
            }
            url = "/api/SetupApi/GetAccountList?searchtext=" + $('#acctxtSearchString').val();
        }
        else {
            $("#acctxtSearchString").val('');
            url = "/api/SetupApi/GetChildOfAccountByGroup?groupId=" + groupId;
        }
        $scope.accoutnChildGridOptions = {
            dataSource: {
                type: "json",
                transport: {
                    read: url,
                },
                pageSize: 50,
                //serverPaging: true,
                serverSorting: true
            },
            scrollable: true,
            height: 450,
            sortable: true,
            pageable: true,
            groupable: true,
            resizable: true,
            filterable: {
                extra: false,
                operators: {
                    number: {
                        eq: "Is equal to",
                        neq: "Is not equal to",
                        gte: "is greater than or equal to	",
                        gt: "is greater than",
                        lte: "is less than or equal",
                        lt: "is less than",
                    },
                    string: {
                        eq: "Is equal to",
                        neq: "Is not equal to",
                        startswith: "Starts with	",
                        contains: "Contains",
                        doesnotcontain: "Does not contain",
                        endswith: "Ends with",
                    },
                    date: {
                        eq: "Is equal to",
                        neq: "Is not equal to",
                        gte: "Is after or equal to",
                        gt: "Is after",
                        lte: "Is before or equal to",
                        lt: "Is before",
                    }
                }
            },
            columnMenu: true,
            columnMenuInit: function (e) {
                wordwrapmenu(e);
                checkboxItem = $(e.container).find('input[type="checkbox"]');
            },
            columnShow: function (e) {
                if ($(".k-widget.k-reset.k-header.k-menu.k-menu-vertical").is(":visible") && checkboxItem != "")
                    SaveReportSetting('_accountSetupPartial', 'grid');
            },
            columnHide: function (e) {
                if ($(".k-widget.k-reset.k-header.k-menu.k-menu-vertical").is(":visible") && checkboxItem != "")
                    SaveReportSetting('_accountSetupPartial', 'grid');
            },
            dataBound: function (e) {
                $("#kGrid tbody tr").css("cursor", "pointer");
                DisplayNoResultsFound($('#kGrid'));
                $("#kGrid tbody tr").on('dblclick', function () {
                    var accCode = $(this).find('td span').html();
                    $scope.edit(accCode);
                    //var tree = $("#accounttree").data("kendoTreeView");
                    //tree.dataSource.read();
                })

            },
            columns: [
                {
                    //hidden: true,
                    title: "Code",
                    field: "ACC_CODE",
                    width: "60px"

                },
                {
                    field: "ACC_EDESC",
                    title: "Account Name",
                    width: "100px"
                },
                {
                    field: "EMAIL_ID",
                    title: "Email Id.",
                    width: "100px"
                },
                {
                    title: "Action ",
                    template: '<a class="fa fa-pencil-square-o editAction" title="Edit" ng-click="edit(dataItem.ACC_CODE)"><span class="sr-only"></span> </a><a class="fa fa-trash deleteAction" title="delete" ng-click="delete(dataItem.ACC_CODE,dataItem.ACC_TYPE_FLAG)"><span class="sr-only"></span> </a>',
                    width: "50px"
                }
            ],
        };
    }
    $scope.showModalForNew = function (event) {
        debugger;
        $scope.accountsArr = [];
        $scope.dekh = false;
        $scope.editFlag = "N";
        $scope.saveupdatebtn = "Save"
        var getAccountdetaisByUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/getAccountDetailsByAccCode?accCode=" + $scope.treeSelectedAccountCode;
        $http({
            method: 'GET',
            url: getAccountdetaisByUrl
        }).then(function successCallback(response) {
            debugger;
            $scope.accountsetup = response.data.DATA;
            $scope.accountsArr = $scope.accountsetup;
            $scope.accountsArr.MASTER_ACC_CODE = $scope.accountsetup.MASTER_ACC_CODE;
            $scope.accountsArr.ACC_CODE = $scope.accountsetup.ACC_CODE;
            $scope.getDetailByAccCode($scope.accountsArr.ACC_CODE);
            $scope.groupAccTypeFlag = "N";
            $scope.accountsArr.ACC_TYPE_FLAG = "T";
            var tree = $("#masteraccountcode").data("kendoDropDownList");
            tree.value($scope.treeSelectedAccountCode);
            $scope.clearFieldsForNew();
            $("#accountModal").modal("toggle");
            setAccNature();
            var getAccountCodeUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/GetNewAccountCode";
            $http({
                method: 'GET',
                url: getAccountCodeUrl
            }).then(function successCallback(result) {
                $scope.accountsArr.ACC_CODE = result.data;
            });
        }, function errorCallback(response) {
        });
    }
    $scope.edit = function (accCode) {
        $scope.clearFields();
        dataFillDefered = $.Deferred();
        $scope.saveupdatebtn = "Update";
        $scope.editFlag = "Y";
        $scope.fillAccSetupForms(accCode);
        $.when(dataFillDefered).then(function () {
            var tree = $("#masteraccountcode").data("kendoDropDownList");
            tree.value($scope.accountsetup.PARENT_ACC_CODE);
            setAccNature();
            var tree1 = $("#masteraccountcode1").data("kendoDropDownList");
            tree1.value($scope.accountsetup.PARENT_ACC_CODE);
            $("#accountModal").modal();
        })
    }
    $scope.delete = function (code, accountTypeFlag) {
        $("#acctxtSearchString").val('');
        bootbox.confirm({
            title: "Delete",
            message: "Are you sure?",
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-success',
                    label: '<i class="fa fa-check"></i> Yes',
                },
                cancel: {
                    label: 'No',
                    className: 'btn-danger',
                    label: '<i class="fa fa-times"></i> No',
                }
            },
            callback: function (result) {
                if (result == true) {
                    if (accountTypeFlag === "T")
                        $scope.accountsetup.ACC_TYPE_FLAG = "T";
                    else
                        $scope.accountsetup.ACC_TYPE_FLAG = "N";
                    var delUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/DeleteAccountSetupByAccCode?accCode=" + code;
                    $http({
                        method: 'POST',
                        url: delUrl
                    }).then(function successCallback(response) {
                        if (response.data.MESSAGE == "DELETED") {
                            if ($scope.accountsetup.ACC_TYPE_FLAG !== "T") {
                                var tree = $("#accounttree").data("kendoTreeView");
                                tree.dataSource.read();
                            }
                            $scope.treenodeselected = "N";
                            var grid = $("#kGrid").data("kendo-grid");
                            grid.dataSource.read();
                            displayPopupNotification("Data succesfully deleted ", "success");
                        }
                        if (response.data.MESSAGE == "HAS_CHILD") {
                            displayPopupNotification("Please delete the respective child first", "warning");
                        }
                        // this callback will be called asynchronously
                        // when the response is available
                    }, function errorCallback(response) {
                        displayPopupNotification(response.data.STATUS_CODE, "error");
                        // called asynchronously if an error occurs
                        // or server returns response with an error status.
                    });
                }
            }
        });
    }
    function DisplayNoResultsFound(grid) {
        // Get the number of Columns in the grid
        //var grid = $("#kGrid").data("kendo-grid");
        var dataSource = grid.data("kendoGrid").dataSource;
        var colCount = grid.find('.k-grid-header colgroup > col').length;
        // If there are no results place an indicator row
        if (dataSource._view.length == 0) {
            grid.find('.k-grid-content tbody')
                .append('<tr class="kendo-data-row"><td colspan="' + colCount + '" style="text-align:center"><b>No Results Found!</b></td></tr>');
        }
        // Get visible row count
        var rowCount = grid.find('.k-grid-content tbody tr').length;
        // If the row count is less that the page size add in the number of missing rows
        if (rowCount < dataSource._take) {
            var addRows = dataSource._take - rowCount;
            for (var i = 0; i < addRows; i++) {
                grid.find('.k-grid-content tbody').append('<tr class="kendo-data-row"><td>&nbsp;</td></tr>');
            }
        }
    }
    $scope.addNewAccount = function () {
        $scope.clearFields();
        $scope.editFlag = "N";
        $scope.dekh = true;
        $scope.saveupdatebtn = "Save"
        $scope.accountsArr = [];
        $scope.groupAccTypeFlag = "Y";
        $scope.accountsetup.ACC_TYPE_FLAG = "N";
        $scope.natureaccountOptions = {
            dataSource: RStureArr,
            dataTextField: "text",
            dataValueField: "key",
            value: 'AA'
        }
        var tree = $("#masteraccountcode").data("kendoDropDownList");
        tree.value("");
        $scope.accountsArr.ACC_TYPE_FLAG = "N";
        var tree = $("#accounttree").data("kendoTreeView");
        tree.dataSource.read();
        var grid = $("#kGrid").data("kendo-grid");
        if (grid != undefined) {
            grid.dataSource.data([]);
        }
        $("#accountModal").modal("toggle");
      //  $("#cr").attr('disabled', 'disabled');
      //  $("#dr").attr('disabled', 'disabled');
      //  $("#profloss").attr('disabled', 'disabled');
     //   $("#balancesheet").attr('disabled', 'disabled'); 

    }
    $scope.clearFields = function () {
        $scope.accountsArr.ACC_EDESC = "";
        $scope.accountsArr.ACC_NDESC = "";
        $scope.accountsArr.BANK_ACCOUNT_NO = "";
        $scope.accountsArr.BANK_ACCOUNT_NO = "";
        $scope.accountsArr.GROUP_START_CODE = "";
        $scope.accountsArr.GROUP_END_CODE = "";
        $scope.accountsArr.TRANSACTION_TYPE = "";
        $scope.accountsArr.TPB_FLAG = "";
        $scope.accountsArr.FREEZE_FLAG = "";
        $scope.accountsArr.PRINTING_FLAG = "";
        $scope.accountsArr.IND_VAT_FLAG = "";
        $scope.accountsArr.IND_TDS_FLAG = "";
        $scope.accountsArr.DEALER_FLAG = "";
        $scope.accountsArr.ACC_ID = "";
        $scope.accountsArr.PREFIX_TEXT = "";
        $scope.accountsArr.CURRENT_BALANCE = "";
        $scope.accountsArr.ACC_SNAME = "";
        $scope.accountsArr.TOTAL_DAYS = "";
        $scope.accountsArr.SAVING_INTEREST = "";
        $scope.accountsArr.LOAN_INTEREST = "";
        $scope.accountsArr.MATURITY_DAYS = "";
        $scope.accountsArr.LINK_ID = "";
        $scope.bankdetailArr.CONTACT_PERSON = "";
        $scope.bankdetailArr.ADDRESS = "";
        $scope.bankdetailArr.DESIGNATION = "";
        $scope.bankdetailArr.DEPARTMENT = "";
        $scope.bankdetailArr.MOBILE_NO = "";
        $scope.bankdetailArr.PHONE_NO = "";
        $scope.bankdetailArr.EMAIL_ID = "";
        $("#acctxtSearchString").val('');
        $scope.result = false;
        $scope.share = false;
    }
    $scope.clearFieldsForNew = function () {
        $scope.accountsArr.ACC_EDESC = "";
        $scope.accountsArr.ACC_NDESC = "";
        $scope.accountsArr.BANK_ACCOUNT_NO = "";
        $scope.accountsArr.BANK_ACCOUNT_NO = "";
        //$scope.accountsArr.GROUP_START_CODE = "";
        //$scope.accountsArr.GROUP_END_CODE = "";
        //$scope.accountsArr.TRANSACTION_TYPE = "";
        //$scope.accountsArr.TPB_FLAG = "";
        $scope.accountsArr.FREEZE_FLAG = "";
        $scope.accountsArr.PRINTING_FLAG = "";
        $scope.accountsArr.IND_VAT_FLAG = "";
        $scope.accountsArr.IND_TDS_FLAG = "";
        $scope.accountsArr.DEALER_FLAG = "";
        //$scope.accountsArr.ACC_ID = "";
        //$scope.accountsArr.PREFIX_TEXT = "";
        $scope.accountsArr.CURRENT_BALANCE = "";
        $scope.accountsArr.ACC_SNAME = "";
        $scope.accountsArr.TOTAL_DAYS = "";
        $scope.accountsArr.SAVING_INTEREST = "";
        $scope.accountsArr.LOAN_INTEREST = "";
        $scope.accountsArr.MATURITY_DAYS = "";
        $scope.accountsArr.LINK_ID = "";
        $scope.bankdetailArr.CONTACT_PERSON = "";
        $scope.bankdetailArr.ADDRESS = "";
        $scope.bankdetailArr.DESIGNATION = "";
        $scope.bankdetailArr.DEPARTMENT = "";
        $scope.bankdetailArr.MOBILE_NO = "";
        $scope.bankdetailArr.PHONE_NO = "";
        $scope.bankdetailArr.EMAIL_ID = "";
        $("#acctxtSearchString").val('');
        $scope.result = false;
        $scope.share = false;
    }
    $scope.reset = function () {
        $scope.accountsetup =
            {
                TRANSACTION_TYPE: "",
                PREFIX_TEXT: "",
                ACC_CODE: "",
                ACC_EDESC: "",
                ACC_NDESC: "",
                TPB_FLAG: "",
                ACC_TYPE_FLAG: "T",
                MASTER_ACC_CODE: "",
                PRE_ACC_CODE: "",
                COMPANY_CODE: "",
                CREATED_BY: "",
                CREATED_DATE: "",
                DELETED_FLAG: "",
                SYN_ROWID: "",
                DELTA_FLAG: "",
                FREEZE_FLAG: "N",
                CURRENT_BALANCE: "",
                LIMIT: "",
                MODIFY_DATE: "",
                ACC_NATURE: "",
                BRANCH_CODE: "",
                SHARE_VALUE: "",
                MODIFY_BY: "",
                IND_VAT_FLAG: "N",
                BANK_ACCOUNT_NO: "",
                PRINTING_FLAG: "N",
                ACC_SNAME: "",
                TEL_NO: "",
                IND_TDS_FLAG: "N",
                ACC_ID: "",
                MOBILE_NO: "",
                EMAIL_ID: "",
                LINK_ID: "",
                CONTACT_PERSON: "",
                GROUP_START_CODE: "",
                GROUP_END_CODE: "",
                DEALER_FLAG: "",
                PARENT_ACC_CODE: "",
                IND_MDF_FLAG: ""
            }
        $scope.newValue = function () {
            debugger;
            console.log(value);
        }
        $scope.bankaccountdetail = {
            CONTACT_PERSON: "",
            DEPARTMENT: "",
            DESIGNATION: "",
            ADDRESS: "",
            MOBILE_NO: "",
            PHONE_NO: "",
            EMAIL_ID: ""
        }
        $scope.bankdetailArr = [];
        $scope.bankdetailArr.push($scope.bankaccountdetail);
        $scope.accountsArr = $scope.accountsetup;
    }
    function setAccNature(){
        var accNature = $scope.accountsArr.ACC_NATURE;
        if (accNature == "AC" || accNature == "AB" || accNature == "AC" || accNature == "AE" || accNature == "AD" || accNature == "AF") {
            $("#natureaccount").kendoDropDownList({
                dataTextField: "text",
                dataValueField: "key",
                dataSource: CANature,
                value: accNature
            });
        }
        else if (accNature == "LA" || accNature == "LB" || accNature == "LC" || accNature == "LD" || accNature == "PV" || accNature == "PL" ) {
            $("#natureaccount").kendoDropDownList({
                dataTextField: "text",
                dataValueField: "key",
                dataSource: SCNature,
                value: accNature
            });
        }
        else if (accNature == "AA") {
            $("#natureaccount").kendoDropDownList({
                dataTextField: "text",
                dataValueField: "key",
                dataSource: RStureArr,
                value: accNature
            });
        }
        else if (accNature == "IA" || accNature == "IC") {
            $("#natureaccount").kendoDropDownList({
                dataSource: SRNature,
                dataTextField: "text",
                dataValueField: "key",
                value: accNature
            });
        }
        else if (accNature == "EA" || accNature == "EC" || accNature == "EB") {
            $("#natureaccount").kendoDropDownList({
                dataSource: MENature,
                dataTextField: "text",
                dataValueField: "key",
                value: accNature
            });
        }
        else if (accNature == "SE" || accNature == "SA" || accNature == "SB" || accNature == "SC" || accNature == "SD") {
            $("#natureaccount").kendoDropDownList({
                dataSource: SPNature,
                dataTextField: "text",
                dataValueField: "key",
                value: accNature
            });
        }
        if ($scope.accountsArr.ACC_NATURE == "LC" || $scope.accountsArr.ACC_NATURE == "AC")
            $scope.result = true;
        else
            $scope.result = false;

        if ($scope.accountsArr.ACC_NATURE == "LA")
            $scope.share = true;
        else
            $scope.share = false;
    }
});