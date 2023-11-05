DTModule.controller('supplierSetupTreeCtrl', function ($scope, $rootScope, $http, $routeParams, $window, $filter) {

    $scope.saveupdatebtn = "Save";
    $scope.supplierArr;
    $scope.savegroup = false;
    $scope.editFlag = "N";
    $scope.treenodeselected = "N";
    $scope.newrootinsertFlag = "Y";
    $scope.phoneNumbr = /^\+?(\d+(\.(\d+)?)?|\.\d+)$/;
    $scope.accountcode = "";
    $scope.editcode = "";
    $scope.edesc = "";
    $scope.updateSuppliercode = "";
    $scope.AE = true;
    $scope.image = "";
    $scope.treeSelectedSupplierCode = "";
    $scope.treeSelectedMasterSupplierCode = "";
    $scope.imageurledit = "";
    $scope.suppliersetup =
        {
            SHORTCUT: "",
            SUPPLIER_CODE: "",
            PARENT_SUPPLIER_CODE: "",
            SUPPLIER_EDESC: "",
            SUPPLIER_NDESC: "",
            REGD_OFFICE_EADDRESS: "",
            REGD_OFFICE_NADDRESS: "",
            TEL_MOBILE_NO1: "",
            TEL_MOBILE_NO2: "",
            FAX_NO: "",
            EMAIL: "",
            PARTY_TYPE_CODE: "",
            LINK_SUB_CODE: "",
            REMARKS: "",
            ACTIVE_FLAG: "",
            GROUP_SKU_FLAG: "",
            MASTER_SUPPLIER_CODE: "",
            PRE_SUPPLIER_CODE: "",
            COMPANY_CODE: "",
            CREATED_BY: "",
            CREATED_DATE: "",
            DELETED_FLAG: "",
            CREDIT_DAYS: "",
            CURRENT_BALANCE: "",
            CREDIT_ACTION_FLAG: "",
            ACC_CODE: "",
            PR_CODE: "",
            TPIN_VAT_NO: "",
            SYN_ROWID: "",
            DELTA_FLAG: "",
            CREDIT_LIMIT: "",
            MODIFY_DATE: "",
            BRANCH_CODE: "",
            MODIFY_BY: "",
            OPENING_DATE: "",
            M_DAYS: "",
            APPROVED_FLAG: "",
            SUBSTITUTE_NAME: "",
            MATURITY_DATE: "",
            IMAGE_FILE_NAME: "",
            INTEREST_RATE: "",
            CASH_SUPPLIER_FLAG: "",
            SUPPLIER_ID: "",
            GROUP_START_NO: "",
            PREFIX_TEXT: "",
            EXCISE_NO: "",
            PREFIX: "",
            STARTID: "",
            TIN: "",

        }
    $scope.supplierArr = $scope.suppliersetup;
    $scope.supplierpersonalinfo =
        [{
            OWNER_NAME: '',
            DESIGNATION: '',
            CONTACT_PERSON: '',
            ADDRESS: '',
            TEL_MOBILE_NO: '',
        }]
    $scope.supplierpinfo = $scope.supplierpersonalinfo;
    $scope.suppliersisterconcerns =
        [{
            SISTER_CONCERN_EDESC: '',
            REMARKS: '',
        }]
    $scope.suppliersconcerns = $scope.suppliersisterconcerns;
    $scope.supplierbudgetcenter =
        [{
            BUDGET_CODE: '',
            REMARKS: '',
        }]
    $scope.supplierbcenter = $scope.supplierbudgetcenter;
    $scope.suppliersupplyterms =
        [{
            ITEM_NAME: '',
            MAX_LEAD_TIME: '',
            MIN_LEAD_TIME: '',
            IDEAL_LEAD_TIME: '',
            MIN_ORDER_QTY: '',
            MAX_ORDER_QTY: '',
        }]
    $scope.suppliersterms = $scope.suppliersupplyterms;
    $scope.suppliersupplyotherinfo =
        [{
            CUSTOM_FIELD: '',
            VALUE: '',
            SUPPLY_REMARKS: '',

        }]
    $scope.suppliersupplyoinfo = $scope.suppliersupplyotherinfo;
    $scope.suppliersupplyalternativelocinfo =
        [{
            SUPPLY_OFFICE_NAME: '',
            SUPPLY_CONTACT_PERSON: '',
            SUPPLY_ADDRESS: '',
            SUPPLY_TELEPHONE_NO: '',
            SUPPLY_EMAIL: '',
        }]
    $scope.suppliersupplyalocinfo = $scope.suppliersupplyalternativelocinfo;
    $scope.supplieraccount =
        {
            CREDIT_LIMIT: '',
            CREDIT_DAYS: '',
            CURRENT_BALANCE: '',
            IS_ACTIVE_SUPPLIER: '',

        }
    $scope.supplieracc = $scope.supplieraccount;
    $scope.supplierinvoices =
        [{
            REFERENCE_NO: '',
            INVOICE_DATE: '',
            DUE_DATE: '',
            TRANSACTION_TYPE: '',
            BALANCE: '',
            IVOICE_REMARKS: '',

        }]
    $scope.supplieri = $scope.supplierinvoices;
    $scope.suppliertermsandconditions =
        [{
            COMMENTS: '',
            TERMS_AND_CONDITIONS: '',
            CONDITIONS_VALUE: '',
            CONDITIONS_REMARKS: '',

        }]
    $scope.suppliertandc = $scope.suppliertermsandconditions
    $scope.supplierdistributionstatus =
        [{
            ITEM_NAME: '',
            AS_ON: '',
            QUANTITY: '',
            REMARKS: '',

        }]
    $scope.supplierdstatus = $scope.supplierdistributionstatus;
    $scope.budgetcentercount = '';
    var dataFillDefered = "";


    //$scope.Binding = function () {

    //    $('.k-list').slimScroll({
    //        height: '250px'
    //    });
    //};



    $scope.add_Personal_info = function (personalDetails) {

        for (var i = 0; i <= $scope.supplierpinfo.length - 1; i++) {

            if ($(".OWNER_" + i).parent().hasClass("borderRed")) {
                displayPopupNotification("Same Name.", "warning");
                e.preventDefault();
                e.stopPropagation();
                return false;
            }
        }
        $scope.supplierpinfo.push({
            'OWNER_NAME': personalDetails.OWNER_NAME,
            'DESIGNATION': personalDetails.DESIGNATION,
            'CONTACT_PERSON': personalDetails.CONTACT_PERSON,
            'ADDRESS': personalDetails.ADDRESS,
            'TEL_MOBILE_NO': personalDetails.TEL_MOBILE_NO,
        });
        //$scope.PD = {};
    };
    $scope.remove_Personal_info = function (index) {


        if ($scope.supplierpinfo.length > 1) {

            $scope.supplierpinfo.splice(index, 1);

        }
    }

    $scope.add_sister_concern = function (sisterConcernDetails) {
        for (var i = 0; i <= $scope.suppliersconcerns.length - 1; i++) {

            if ($(".sconcernname_" + i).parent().hasClass("borderRed")) {
                displayPopupNotification("Same  Name.", "warning");
                e.preventDefault();
                e.stopPropagation();
                return false;
            }
        }

        $scope.suppliersconcerns.push({
            'SISTER_CONCERN_EDESC': sisterConcernDetails.SISTER_CONCERN_EDESC,
            'REMARKS': sisterConcernDetails.REMARKS,

        });
    };
    $scope.remove_sister_concern = function (index) {


        if ($scope.suppliersconcerns.length > 1) {

            $scope.suppliersconcerns.splice(index, 1);

        }
    }

    $scope.add_budget_center = function (supplierbcDetails) {

        if ($scope.budgetcentercount === true) {
            displayPopupNotification("Same Code Or Budget Center cannot be selected", "warning");
            e.preventDefault();
            e.stopPropagation();
            return false;
        }
        $scope.supplierbcenter.push({
            'BUDGET_CODE': supplierbcDetails.BUDGET_CODE,
            'REMARKS': supplierbcDetails.REMARKS,

        });
    };
    $scope.remove_budget_center = function (index) {


        if ($scope.supplierbcenter.length > 1) {

            $scope.supplierbcenter.splice(index, 1);
            $scope.subledgercount = false;
        }
    }
    $scope.checkdupowner = function (key, index) {


        for (var i = 0; i <= $scope.supplierpinfo.length - 1; i++) {
            $(".OWNER_" + i).parent().removeClass("borderRed");
            $("#savedocumentformdata").prop("disabled", false);
        }

        for (var a = 0; a <= $scope.supplierpinfo.length - 1; a++) {
            for (var b = 0; b <= $scope.supplierpinfo.length - 1; b++) {
                if ($scope.supplierpinfo[a] != $scope.supplierpinfo[b]) {
                    if ($scope.supplierpinfo[a].OWNER_NAME === $scope.supplierpinfo[b].OWNER_NAME) {

                        $(".OWNER_" + b).parent().addClass("borderRed");
                        $("#savedocumentformdata").prop("disabled", true);
                    }
                }
            }

        }

    }
    $scope.checkdupsconcern = function (key, index) {


        for (var i = 0; i <= $scope.suppliersconcerns.length - 1; i++) {
            $(".sconcernname_" + i).parent().removeClass("borderRed");
            $("#savedocumentformdata").prop("disabled", false);
        }

        for (var a = 0; a <= $scope.suppliersconcerns.length - 1; a++) {
            for (var b = 0; b <= $scope.suppliersconcerns.length - 1; b++) {
                if ($scope.suppliersconcerns[a] != $scope.suppliersconcerns[b]) {
                    if ($scope.suppliersconcerns[a].SISTER_CONCERN_EDESC === $scope.suppliersconcerns[b].SISTER_CONCERN_EDESC) {

                        $(".sconcernname_" + b).parent().addClass("borderRed");
                        $("#savedocumentformdata").prop("disabled", true);
                    }
                }
            }

        }

    }
    $scope.add_supplier_supply_terms = function (supplyterm) {

        $scope.suppliersupplyterms.push({
            'ITEM_NAME': supplyterm.ITEM_NAME,
            'MAX_LEAD_TIME': supplyterm.MAX_LEAD_TIME,
            'MIN_LEAD_TIME': supplyterm.MIN_LEAD_TIME,
            'IDEAL_LEAD_TIME': supplyterm.IDEAL_LEAD_TIME,
            'MAX_ORDER_QTY': supplyterm.MAX_ORDER_QTY,
            'MIN_ORDER_QTY': supplyterm.MIN_ORDER_QTY,

        });
    };
    $scope.remove_supplier_supply_terms = function (index) {


        if ($scope.suppliersupplyterms.length > 1) {

            $scope.suppliersupplyterms.splice(index, 1);

        }
    }
    $scope.add_supplier_supply_terms = function (supplyterm) {

        $scope.suppliersupplyterms.push({
            'ITEM_NAME': supplyterm.ITEM_NAME,
            'MAX_LEAD_TIME': supplyterm.MAX_LEAD_TIME,
            'MIN_LEAD_TIME': supplyterm.MIN_LEAD_TIME,
            'IDEAL_LEAD_TIME': supplyterm.IDEAL_LEAD_TIME,
            'MAX_ORDER_QTY': supplyterm.MAX_ORDER_QTY,
            'MIN_ORDER_QTY': supplyterm.MIN_ORDER_QTY,

        });
    };
    $scope.remove_supplier_supply_terms = function (index) {


        if ($scope.suppliersupplyterms.length > 1) {

            $scope.suppliersupplyterms.splice(index, 1);

        }
    }
    $scope.add_supllier_supply_other_info = function (otherinfo) {

        $scope.suppliersupplyotherinfo.push({
            'CUSTOM_FIELD': otherinfo.CUSTOM_FIELD,
            'VALUE': otherinfo.VALUE,
            'SUPPLY_REMARKS': otherinfo.SUPPLY_REMARKS,


        });
    };
    $scope.remove_supllier_supply_other_info = function (index) {


        if ($scope.suppliersupplyotherinfo.length > 1) {

            $scope.suppliersupplyotherinfo.splice(index, 1);

        }
    }
    $scope.add_alternative_location = function (alternativelocn) {

        $scope.suppliersupplyalternativelocinfo.push({
            'SUPPLY_OFFICE_NAME': alternativelocn.SUPPLY_OFFICE_NAME,
            'SUPPLY_ADDRESS': alternativelocn.SUPPLY_ADDRESS,
            'SUPPLY_TELEPHONE_NO': alternativelocn.SUPPLY_TELEPHONE_NO,
            'SUPPLY_EMAIL': alternativelocn.SUPPLY_EMAIL,




        });
    };
    $scope.remove_alternative_location = function (index) {


        if ($scope.suppliersupplyalternativelocinfo.length > 1) {

            $scope.suppliersupplyalternativelocinfo.splice(index, 1);

        }
    }
    $scope.add_supplier_invoice = function (invoice) {

        $scope.supplieri.push({
            'REFERENCE_NO': invoice.REFERENCE_NO,
            'INVOICE_DATE': invoice.INVOICE_DATE,
            'DUE_DATE': invoice.DUE_DATE,
            'TRANSACTION_TYPE': invoice.TRANSACTION_TYPE,
            'BALANCE_AMOUNT': invoice.BALANCE_AMOUNT,
            'REMARKS': invoice.REMARKS,




        });
    };
    $scope.remove_supplier_invoice = function (index) {


        if ($scope.supplieri.length > 1) {

            $scope.supplieri.splice(index, 1);

        }
    }
    $scope.add_terms = function (term) {

        $scope.suppliertermsandconditions.push({
            'TERMS_AND_CONDITIONS': term.TERMS_AND_CONDITIONS,
            'CONDITIONS_VALUE': term.CONDITIONS_VALUE,
            'CONDITIONS_REMARKS': term.CONDITIONS_REMARKS,
        });
    };
    $scope.remove_terms = function (index) {


        if ($scope.suppliertermsandconditions.length > 1) {

            $scope.suppliertermsandconditions.splice(index, 1);

        }
    }
    $scope.add_distribution_status = function (status) {

        $scope.supplierdistributionstatus.push({
            'ITEM_NAME': status.ITEM_NAME,
            'AS_ON': status.AS_ON,
            'QUANTITY': status.QUANTITY,
            'REMARKS': status.REMARKS,
        });
    };
    $scope.remove_distribution_status = function (index) {


        if ($scope.supplierdistributionstatus.length > 1) {

            $scope.supplierdistributionstatus.splice(index, 1);

        }
    }
    $scope.mastersupplierCodeDataSource = [
        { text: "/Root", value: "" }
    ];




    var supplierCodeUrl = window.location.protocol + "//" + window.location.host + "/api/TemplateApi/getsupplierCodeWithChild";

    $scope.supplierGroupDataSource = {
        transport: {
            read: {
                url: supplierCodeUrl,
            }
        }
    };
    var partyTypeCodeUrl = window.location.protocol + "//" + window.location.host + "/api/TemplateApi/GetPartyType";

    $scope.partyTypeDataSource = {
        transport: {
            read: {
                url: partyTypeCodeUrl,
            }
        }
    };
    var partyRatingCodeUrl = window.location.protocol + "//" + window.location.host + "/api/TemplateApi/GetPartyRating";

    $scope.partyRatingDataSource = {
        transport: {
            read: {
                url: partyRatingCodeUrl,
            }
        }
    };

    var budgetCodeUrl = window.location.protocol + "//" + window.location.host + "/api/TemplateApi/getBudgetCenterCodeWithChild";

    $scope.budgetGroupDataSource = {
        transport: {
            read: {
                url: budgetCodeUrl,
            }
        }
    };
    $scope.budgetGroupOptions = {
        dataSource: $scope.budgetGroupDataSource,
        optionLabel: "-Select Budget Code-",
        dataTextField: "BUDGET_EDESC",
        dataValueField: "BUDGET_CODE",
        select: function (e) {

            var Code = e.dataItem.BUDGET_CODE;
            var key = this.element[0].attributes['budgetcenter-key'].value;
            var index = this.element[0].attributes['budgetcenter-index'].value;

            $scope.supplierbcenter[index].BUDGET_CODE = Code;


            var sublen = $scope.supplierbcenter.length;

            for (var j = 0; j < sublen; j++) {
                var subcode = $scope.supplierbcenter[j].BUDGET_CODE;
                if (index != j) {
                    if (subcode === Code) {

                        $($(".budgetcentercode_" + index)[0]).addClass("borderRed");
                        $("#savedocumentformdata").prop("disabled", true);
                        $scope.budgetcentercount = true;
                        return;

                    }
                    else {

                        $scope.budgetcentercount = false;
                        $("#savedocumentformdata").prop("disabled", false);
                    };

                }


            }
             //filter: "contains",
        },
        dataBound: function () {

            //$scope.Binding();
        }
    }

  

    $scope.partytypeOptions = {
        dataSource: $scope.partyTypeDataSource,
        optionLabel: "-Select Party Type-",
        dataTextField: "PARTY_TYPE_EDESC",
        dataValueField: "PARTY_TYPE_CODE",
        change: function (e) {
            $scope.partyTypeCodeOnChange(e);
        },
        //filter: "contains",
    }
    $scope.partyTypeCodeOnChange = function (kendoEvent) {
        debugger;
        var currentItem = kendoEvent.sender.dataItem(kendoEvent.node);

        var tree = $("#accountmap").data("kendoDropDownList");
        tree.value(currentItem.ACC_CODE);

        // $scope.supplierArr.ACC_CODE = currentItem.ACC_CODE;
        $scope.suppliersetup.PARTY_TYPE_CODE = currentItem.PARTY_TYPE_CODE;
        //$("#accountcode").data("kendo").value(currentItem.ACC_CODE);


        //var accountCodeUrl = window.location.protocol + "//" + window.location.host + "/api/TemplateApi/GetAccCodeByCode?acccode=" + currentItem.ACC_CODE;
        //$scope.accDSource = {
        //    transport: {
        //        read: {
        //            url: accountCodeUrl,
        //        }
        //    }
        //};


        //$scope.accountGOptions = {
        //    dataSource: $scope.accDSource,
        //    optionLabel: "-Select Account-",
        //    dataTextField: "ACC_EDESC",
        //    dataValueField: "ACC_CODE",
        //    change: function (e) {

        //    },
        //    dataBound: function () {

        //    }
        //}
    }
    var accountCodeUrl = window.location.protocol + "//" + window.location.host + "/api/TemplateApi/GetAllAccountCodesupp";
    $scope.accDSource = {
        transport: {
            read: {
                url: accountCodeUrl,
            }
        }
    };


    $scope.accountGOptions = {
        dataSource: $scope.accDSource,
        optionLabel: "-Select Account-",
        dataTextField: "ACC_EDESC",
        dataValueField: "ACC_CODE",
        change: function (e) {

        },
        dataBound: function () {

        }
    }
    $scope.PartyRatingOptions = {
        dataSource: $scope.partyRatingDataSource,
        optionLabel: "-select  Rating-",
        dataTextField: "PR_EDESC",
        dataValueField: "PR_CODE",
        //filter: "contains",
    }
    $scope.MACDS = [];


    $scope.refresh = function () {
        debugger;
        //var tree = $("#suppliertree").data("kendoTreeView");
        //tree.dataSource.read();
        //if ($scope.suppliersetup.GROUP_SKU_FLAG !== "I") {
        //    var tree = $("#suppliertree").data("kendoTreeView");
        //    tree.dataSource.read();
        //}
        var grid = $("#kGrid").data("kendo-grid");
        if (grid != undefined) {
            grid.dataSource.read();
        }
        var ddl = $("#groupmastersuppliercode").data("kendoDropDownList");
        if (ddl != undefined)
            ddl.dataSource.read();
        if ($scope.groupsupplierTypeFlag == "Y") {
            $("#groupsupplierModal").modal("hide");
        }
        else {
            $("#supplierModal").modal("hide");
        }
        $scope.treenodeselected = 'Y';
        $scope.clearData();
        $scope.reset();
        $('#blah').attr('src', '');
        $($("#myTab").find("li a")[0]).trigger("click");

    }

    $scope.MACDSOptions = {
        dataSource: $scope.supplierGroupDataSource,
        optionLabel: "--\ ROOT",
        dataTextField: "supplier_EDESC",
        dataValueField: "MASTER_SUPPLIER_CODE",
        filter: "contains",
    }

    $scope.Bind = function () {

        $('#groupsuppliercode_listbox').slimScroll({
            height: '250px'
        });
    };

    var getSupplierByUrl = window.location.protocol + "//" + window.location.host + "/api/TemplateApi/GetSuppliers";
    $scope.treeData = new kendo.data.HierarchicalDataSource({
        transport: {
            read: {
                url: getSupplierByUrl,
                type: 'GET',
                data: function (data, evt) {
                }
            },

        },
        schema: {
            model: {
                id: "masterSupplierCode",
                parentId: "preSupplierCode",
                children: "Items",
                fields: {
                    SUPPLIER_CODE: { field: "supplierId", type: "string" },
                    SUPPLIER_EDESC: { field: "supplierName", type: "string" },
                    parentId: { field: "preSupplierCode", type: "string", defaultValue: "00" },
                }
            }
        }
    });

    //treeview expand on startup
    $scope.supplieronDataBound = function () {
        // $('#suppliertree').data("kendoTreeView").expand('.k-supplier');
    }

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
        optionLabel: "--select Account Type--",
        dataTextField: "ACC_EDESC",
        dataValueField: "ACC_CODE",
        change: function (e) {

            var currentItem = e.sender.dataItem(e.node);
            var acccode = currentItem.ACC_CODE;
            $scope.supplierArr.ACC_CODE = acccode;
            $scope.suppliersetup.ACC_CODE = acccode;
        },
        dataBound: function () {

            $scope.accountGroupDataSource;
        }
    }
    $scope.fillSupllierSetupForms = function (SupplierCode) {
        debugger;
        var getsupplierdetaisByUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/getsupplierDetailsBysupplierCode?SupplierCode=" + SupplierCode;
        $http({
            method: 'GET',
            url: getsupplierdetaisByUrl,

        }).then(function successCallback(response) {
            debugger;
            //setTimeout(function () {
            $scope.suppliersetup = response.data.DATA;
            $scope.supplierArr = $scope.suppliersetup;
            $scope.supplierpinfo = response.data.DATA.suplierContactmodelList <= 0 ? $scope.supplierpinfo : response.data.DATA.suplierContactmodelList;
            $scope.suppliersconcerns = response.data.DATA.supplierSisterConcernmodelList <= 0 ? $scope.suppliersconcerns : response.data.DATA.supplierSisterConcernmodelList;
            $scope.supplierbcenter = response.data.DATA.supplierBudgetCenterInfoList <= 0 ? $scope.supplierbcenter : response.data.DATA.supplierBudgetCenterInfoList;
            $scope.supplieri = response.data.DATA.supplierOpeningBalanceModelList <= 0 ? $scope.supplieri : response.data.DATA.supplierOpeningBalanceModelList;
            $scope.supplierArr.MASTER_SUPPLIER_CODE = $scope.suppliersetup.MASTER_SUPPLIER_CODE;
            $scope.supplierArr.GROUP_SKU_FLAG = "G";
            $scope.supplierArr.SUPPLIER_CODE = $scope.suppliersetup.SUPPLIER_CODE;
            $scope.imageurledit = window.location.protocol + "//" + window.location.host + "/Areas/NeoERP.DocumentTemplate/images/supplier/" + $scope.suppliersetup.SUPPLIER_CODE+ "/" + response.data.DATA.IMAGE_FILE_NAME;

            dataFillDefered.resolve(response);
            if ($scope.editFlag == "N") {
                $scope.newSupplierId();
            }
            else {
                var childItemParent = $("#mastersuppliercode").data("kendoDropDownList");
                childItemParent.value($scope.suppliersetup.PARENT_SUPPLIER_CODE);
                $scope.supplierArr.SHORTCUT = $scope.supplierArr.SUPPLIER_CODE;
            }


            //}, 100);






            //$scope.mastersupplierCodeDataSource = [];
            //$scope.masterItemCodeDataSource = [];
            //if ($scope.editFlag = "Y") {
            //    $scope.AE = false;
            //    $("#groupsuppliercode").data("kendoComboBox").value($scope.edesc);
            //    $scope.Bind();

            //}




            //if ($scope.editFlag == "Y") {
            //    $scope.mastersupplierCodeDataSource.push({  text: $scope.suppliersetup.SUPPLIER_EDESC, value: $scope.suppliersetup.MASTER_SUPPLIER_CODE});
            //}
            //else {
            //   $scope.mastersupplierCodeDataSource.push({ text: "/Root", value: "" });
            //}
            //var tree = $("#childmastersuppliercode").data("kendoDropDownList");
            //tree.setDataSource($scope.mastersupplierCodeDataSource);
            // this callback will be called asynchronously
            // when the response is available
        }, function errorCallback(response) {

            // called asynchronously if an error occurs
            // or server returns response with an error status.
        });
    }

    $scope.supplieroptions = {
        loadOnDemand: false,
        select: function (e) {
            debugger;
            var currentsupplier = e.sender.dataItem(e.node);
            $('#supplierGrid').removeClass("show-displaygrid");
            $("#supplierGrid").html("");
            $($(this._current).parents('ul')[$(this._current).parents('ul').length - 1]).find('span').removeClass('hasTreeCustomMenu');
            $(this._current.context).find('span').addClass('hasTreeCustomMenu');
            $scope.suppliersetup.SUPPLIER_CODE = currentsupplier.SUPPLIER_CODE;
            //$scope.suppliersetup.SUPPLIER_EDESC = currentsupplier.SUPPLIER_EDESC;
            $scope.suppliersetup.MASTER_SUPPLIER_CODE = currentsupplier.masterSupplierCode;
            $scope.supplierArr.MASTER_SUPPLIER_CODE = $scope.suppliersetup.MASTER_SUPPLIER_CODE;
            $scope.editcode = $scope.suppliersetup.MASTER_SUPPLIER_CODE;
            //$scope.edesc = $scope.suppliersetup.SUPPLIER_EDESC;
            $scope.treenodeselected = "Y";
            $scope.newrootinsertFlag = "N";
            $scope.treeSelectedSupplierCode = currentsupplier.SUPPLIER_CODE;
            $scope.treeSelectedMasterSupplierCode = $scope.suppliersetup.MASTER_SUPPLIER_CODE;
            //$scope.MACDS = [];
            //$scope.MACDS.push({ text: $scope.suppliersetup.SUPPLIER_EDESC, value: $scope.suppliersetup.MASTER_SUPPLIER_CODE })
            //var tree = $("#childmastersuppliercode").data("kendoDropDownList");
            //tree.setDataSource($scope.MACDS);
            //$scope.movescrollbar();
        },

    };

  
    $scope.movescrollbar = function () {
        var element = $(".k-in");
        for (var i = 0; i < element.length; i++) {
            var selectnode = $(element[i]).hasClass("k-state-focused");
            if (selectnode) {
                $("#suppliertree").animate({
                    scrollTop: (parseInt(i))
                });
                break;
            }
        }
    }


    $scope.onContextSelect = function (event) {
        debugger;
        if ($scope.suppliersetup.supplier_CODE == "")
            return displayPopupNotification("Select supplier.", "error");;
        $scope.saveupdatebtn = "Save";
        if (event.item.innerText.trim() == "Delete") {
            debugger;
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

                        var delUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/DeletesuppliersetupBysuppliercode?suppliercode=" + $scope.suppliersetup.SUPPLIER_CODE;
                        $http({
                            method: 'POST',
                            url: delUrl
                        }).then(function successCallback(response) {
                            debugger;
                            if (response.data.MESSAGE == "DELETED") {
                                $scope.supplierArr = [];
                                $("#supplierModal").modal("hide");
                                $scope.refresh();
                                var tree = $("#suppliertree").data("kendoTreeView");
                                tree.dataSource.read();
                                bootbox.hideAll();
                                displayPopupNotification("Data succesfully deleted ", "success");
                            }
                            if (response.data.MESSAGE == "HAS_CHILD") {

                                $scope.supplierArr = [];
                                //$scope.suppliersetup.MASTER_SUPPLIER_CODE = "";
                                $scope.editcode = "";
                                $scope.edesc = "";
                                $("#supplierModal").modal("hide");
                                bootbox.hideAll();

                                $scope.refresh();
                                displayPopupNotification("Cannot Delete", "warning");
                            }
                            // this callback will be called asynchronously
                            // when the response is available
                        }, function errorCallback(response) {
                            $scope.refresh();
                            displayPopupNotification(response.data.STATUS_CODE, "error");
                            // called asynchronously if an error occurs
                            // or server returns response with an error status.
                        });

                    }
                    else if (result == false) {

                        $scope.refresh();
                        $("#supplierModal").modal("hide");
                        bootbox.hideAll();
                    }

                }
            });
        }
        else if (event.item.innerText.trim() == "Update") {
            debugger;
            $scope.saveupdatebtn = "Update";
            $scope.editFlag = "Y";
            $scope.groupsupplierTypeFlag = "Y";
            dataFillDefered = $.Deferred();
            $scope.fillSupllierSetupForms($scope.treeSelectedSupplierCode);
           
            $.when(dataFillDefered).done(function () {
                debugger;
                var tree = $("#groupmastersuppliercode").data("kendoDropDownList");
                tree.value($scope.suppliersetup.PARENT_SUPPLIER_CODE);
               

                $("#groupsupplierModal").modal();
            });


        }
        else if (event.item.innerText.trim() == "Add") {
            debugger;

            $scope.editFlag = "N";
            $scope.groupsupplierTypeFlag = "Y"
            dataFillDefered = $.Deferred();
            $scope.fillSupllierSetupForms($scope.treeSelectedSupplierCode);



            $scope.savegroup = true;

            $.when(dataFillDefered).done(function () {
                debugger;
                $scope.supplierArr.GROUP_SKU_FLAG = "G";
                $scope.suppliersetup.GROUP_SKU_FLAG = "G";
                $scope.suppliersetup.SUPPLIER_EDESC = "";
                $scope.supplierArr.SUPPLIER_EDESC = "";
                $scope.supplierArr.SUPPLIER_NDESC = "";
                $scope.suppliersetup.SUPPLIER_NDESC = "";
                $scope.suppliersetup.PREFIX_TEXT = "";
                $scope.supplierArr.PREFIX_TEXT = "";
                $scope.suppliersetup.GROUP_START_NO = "";
                $scope.supplierArr.REMARKS = "";
              

                var tree = $("#groupmastersuppliercode").data("kendoDropDownList");
                //tree.value($scope.suppliersetup.SUPPLIER_CODE);
                tree.value($scope.treeSelectedSupplierCode);

                $("#groupsupplierModal").modal();
            });



        }

    }
    $scope.saveNewsupplier = function (isValid) {
        debugger;
        //if (!isValid) {
        //    displayPopupNotification("Input fields are not valid. Please review and try again", "warning");
        //    return;
        //}
        if ($scope.suppliersetup.GROUP_SKU_FLAG == 'I' || $scope.supplierArr.GROUP_SKU_FLAG == 'I') {
            var validation = [
               { supplierengname: $scope.suppliersetupform.supplierengname.$invalid },
               { regofficeengname: $scope.suppliersetupform.regofficeengname.$invalid },
               { vatno: $scope.suppliersetupform.vatno.$invalid },
              

        ]

           if (validation[0].supplierengname == true) {

            displayPopupNotification("Enter English Nmae", "warning");
            return
        }
           if (validation[1].regofficeengname == true) {

            displayPopupNotification(" Enter Permanent Address", "warning");

            return
        }

           if (validation[2].vatno == true) {

            displayPopupNotification("PAN Number Should be 9 Letter only", "warning");

            return
           }
        }
        else {
            var validation = [
             { groupsupplierengname: $scope.groupsuppliersetupform.groupsupplierengname.$invalid },
            ]

            if (validation[0].groupsupplierengname == true) {

                displayPopupNotification("Enter English Nmae", "warning");
                return
            }

        }
      





        var mastersuppliervalue = $scope.suppliersetup.MASTER_SUPPLIER_CODE;
      
        //return;
        if ($scope.saveupdatebtn == "Save") {
            debugger;
            //return;
            var createUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/createNewsupplier";

            for (var a = 0; a <= $scope.supplierpinfo.length - 1; a++) {
                for (var b = 0; b <= $scope.supplierpinfo.length - 1; b++) {
                    if ($scope.supplierpinfo[a] != $scope.supplierpinfo[b]) {
                        if ($scope.supplierpinfo[a].OWNER_NAME === $scope.supplierpinfo[b].OWNER_NAME) {
                            $($("#myTab").find("li a")[1]).trigger("click");
                            displayPopupNotification("Validation Issue On Owner Name", "warning");
                            e.preventDefault();
                            e.stopPropagation();

                            //$('#myTab a[href="#tab-1"]').tab('show');
                            return false;

                        }
                    }
                }

            }
            for (var a = 0; a <= $scope.suppliersconcerns.length - 1; a++) {
                for (var b = 0; b <= $scope.suppliersconcerns.length - 1; b++) {
                    if ($scope.suppliersconcerns[a] != $scope.suppliersconcerns[b]) {
                        if ($scope.suppliersconcerns[a].SISTER_CONCERN_EDESC === $scope.suppliersconcerns[b].SISTER_CONCERN_EDESC) {
                            $($("#myTab").find("li a")[1]).trigger("click");
                            displayPopupNotification("Validation Issue On Sister Concern", "warning");
                            e.preventDefault();
                            e.stopPropagation();

                            //$('#myTab a[href="#tab-1"]').tab('show');
                            return false;

                        }
                    }
                }

            }
            for (var a = 0; a <= $scope.supplierbcenter.length - 1; a++) {
                for (var b = 0; b <= $scope.supplierbcenter.length - 1; b++) {
                    if ($scope.supplierbcenter[a] != $scope.supplierbcenter[b]) {
                        if ($scope.supplierbcenter[a].BUDGET_CODE === $scope.supplierbcenter[b].BUDGET_CODE) {
                            $($("#myTab").find("li a")[1]).trigger("click");
                            displayPopupNotification("Validation Issue On Budget Center Name", "warning");
                            e.preventDefault();
                            e.stopPropagation();

                            //$('#myTab a[href="#tab-1"]').tab('show');
                            return false;

                        }
                    }
                }

            }
            if ($scope.groupsupplierTypeFlag == "N") {

                document.uploadFile();
                if ($('#txtFile')[0].files[0] !== undefined) { $scope.supplierArr.IMAGE_FILE_NAME = $('#txtFile')[0].files[0].name; }

            }
            $scope.supplierArr.PARTY_TYPE_CODE = $("#partytypecode").data("kendoDropDownList").value();
            $scope.supplierArr.ACC_CODE = $("#accountmap").data("kendoDropDownList").value();
            $scope.supplierArr.suplierContactmodelList = $scope.supplierpinfo;
            $scope.supplierArr.supplierSisterConcernmodelList = $scope.suppliersconcerns;
            $scope.supplierArr.supplierBudgetCenterInfoList = $scope.supplierbcenter;
            $scope.supplierArr.supplierOpeningBalanceModelList = $scope.supplieri;

            var suplierSetupModalSet = { suplierSetupModel: $scope.supplierArr };




            $http({
                method: 'POST',
                url: createUrl,
                data: suplierSetupModalSet

            }).then(function successCallback(response) {
                debugger;
                if (response.data.MESSAGE == "INSERTED") {
                    //uploadFile();

                    $scope.supplierArr = [];
                    if ($scope.suppliersetup.GROUP_SKU_FLAG !== "I") {
                        var tree = $("#suppliertree").data("kendoTreeView");
                        tree.dataSource.read();
                    }
               
                    var grid = $("#kGrid").data("kendo-grid");
                    if (grid != undefined) {
                        grid.dataSource.read();
                    }
                    //var ddl = $("#groupmastersuppliercode").data("kendoDropDownList");
                    var ddl = $("#mastersuppliercode").data("kendoDropDownList");
                    if (ddl != undefined)
                        ddl.dataSource.read();

                    var ddl1 = $("#partytypecode").data("kendoDropDownList");
                    if (ddl1 != undefined)
                        ddl1.dataSource.read();
       
                    //$scope.supplierpinfo = [];
                    //$scope.suppliersconcerns = [];
                    //$scope.supplierbcenter = [];
                    //$scope.supplieri = [];
                    //$scope.suppliersetup.MASTER_SUPPLIER_CODE = "";
                    $scope.refresh();

                    //$("#groupsupplierModal").modal("toggle");
                    displayPopupNotification("Data succesfully saved ", "success");
                }
                else {

                    $scope.refresh();
                    displayPopupNotification("Something went wrong.Please try again later.", "error");
                }
                // this callback will be called asynchronously
                // when the response is available
            }, function errorCallback(response) {
                $scope.refresh();
                displayPopupNotification("Something went wrong.Please try again later.", "error");
                // called asynchronously if an error occurs
                // or server returns response with an error status.
            });



        }
        else {
            var updateUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/updatesupplierbysupplierCode";


            $scope.supplierArr.suplierContactmodelList = $scope.supplierpinfo;
            $scope.supplierArr.supplierSisterConcernmodelList = $scope.suppliersconcerns;
            $scope.supplierArr.supplierBudgetCenterInfoList = $scope.supplierbcenter;
            $scope.supplierArr.supplierOpeningBalanceModelList = $scope.supplieri;
            $scope.supplierArr.ACC_CODE = $("#accountmap").data("kendoDropDownList").value();

            var suplierSetupModalSet = { suplierSetupModel: $scope.supplierArr };
            $scope.saveupdatebtn = "Update";
            if ($scope.groupsupplierTypeFlag == "N") {

                document.uploadFile();
                if ($('#txtFile')[0].files[0] !== undefined) { $scope.supplierArr.IMAGE_FILE_NAME = $('#txtFile')[0].files[0].name; }
                else { $scope.supplierArr.IMAGE_FILE_NAME = $scope.supplierArr.IMAGE_FILE_NAME; }

            }
            $http({
                method: 'POST',
                url: updateUrl,
                data: suplierSetupModalSet
            }).then(function successCallback(response) {
                debugger;

                if (response.data.MESSAGE == "UPDATED") {

                    $scope.supplierArr = [];
                    //$scope.supplierpinfo = [];
                    //$scope.suppliersconcerns = [];
                    //$scope.supplierbcenter = [];
                    //$scope.supplieri = [];
                    if ($scope.suppliersetup.GROUP_SKU_FLAG !== "I") {
                        var tree = $("#suppliertree").data("kendoTreeView");
                        tree.dataSource.read();
                        $scope.suppliersetup.MASTER_SUPPLIER_CODE = "";
                    }
                  
                    $scope.refresh();

                    displayPopupNotification("Data succesfully updated ", "success");
                }
                if (response.data.MESSAGE == "ERROR") {
                    $scope.refresh();
                    displayPopupNotification("Something went wrong.Please try again later.", "error");
                }
                // this callback will be called asynchronously
                // when the response is available
            }, function errorCallback(response) {

                $scope.refresh();
                displayPopupNotification("Something went wrong.Please try again later.", "error");
                // called asynchronously if an error occurs
                // or server returns response with an error status.
            });
        }
    }


    $scope.MACDSOptions = {
        dataSource: $scope.MACDS,
        dataTextField: "text",
        dataValueField: "value",
    };

    $scope.supplierGroupOptions = {
        optionLabel: "<PRIMARY>",
        dataSource: $scope.supplierGroupDataSource,
        dataTextField: "SUPPLIER_EDESC",
        dataValueField: "SUPPLIER_CODE",
        select: function (e) {

            $rootScope.quickmastersuppliercode = e.dataItem.MASTER_SUPPLIER_CODE;
        },

        dataBound: function () {

            $scope.supplierGroupDataSource;
        }

    };
    $scope.clearData = function () {

        $scope.supplierArr =
            {
                SHORTCUT: "",
                SUPPLIER_CODE: "",
                SUPPLIER_EDESC: "",
                SUPPLIER_NDESC: "",
                REGD_OFFICE_EADDRESS: "",
                REGD_OFFICE_NADDRESS: "",
                TEL_MOBILE_NO1: "",
                TEL_MOBILE_NO2: "",
                FAX_NO: "",
                EMAIL: "",
                PARTY_TYPE_CODE: "",
                LINK_SUB_CODE: "",
                REMARKS: "",
                ACTIVE_FLAG: "",
                GROUP_SKU_FLAG: "",
                MASTER_SUPPLIER_CODE: "",
                PRE_SUPPLIER_CODE: "",
                COMPANY_CODE: "",
                CREATED_BY: "",
                CREATED_DATE: "",
                DELETED_FLAG: "",
                CREDIT_DAYS: "",
                CURRENT_BALANCE: "",
                CREDIT_ACTION_FLAG: "",
                //ACC_CODE: "",
                PR_CODE: "",
                TPIN_VAT_NO: "",
                SYN_ROWID: "",
                DELTA_FLAG: "",
                CREDIT_LIMIT: "",
                MODIFY_DATE: "",
                BRANCH_CODE: "",
                MODIFY_BY: "",
                OPENING_DATE: "",
                M_DAYS: "",
                APPROVED_FLAG: "",
                SUBSTITUTE_NAME: "",
                MATURITY_DATE: "",
                IMAGE_FILE_NAME: "",
                INTEREST_RATE: "",
                CASH_SUPPLIER_FLAG: "",
                SUPPLIER_ID: "",
                GROUP_START_NO: "",
                PREFIX_TEXT: "",
                EXCISE_NO: "",
                PREFIX: "",
                STARTID: "",
                TIN:"",


               
              
            }
        $('#masteraccountcode').val("");
        $('#nepaliDate5').val("");
        $('#nepaliDate6').val("");
    }

    $scope.BindGrid = function (groupId) {
        debugger;
        $(".topsearch").show();
        var url = null;
        if (groupId == "All") {
            if ($('#suptxtSearchString').val() == null || $('#suptxtSearchString').val() == '' || $('#suptxtSearchString').val() == undefined || $('#suptxtSearchString').val() == 'undefined') {
                alert('Input is empty or undefined.');
                return;
            }
            url = "/api/SetupApi/GetAllSupplyList?searchtext=" + $('#suptxtSearchString').val();
        }
        else {
            $("#suptxtSearchString").val('');
            url = "/api/SetupApi/GetChildOfsupplierByGroup?groupId=" + groupId;
        }
        $scope.supplierChildGridOptions = {
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
            resizable: true,
            pageable: true,
            //resizable: true,
            dataBound: function (e) {
                $("#kGrid tbody tr").css("cursor", "pointer");
                DisplayNoResultsFound($('#kGrid'));
                $("#kGrid tbody tr").on('dblclick', function () {
                    var accCode = $(this).find('td span').html()
                    $scope.edit(accCode);
                })

            },
            columns: [
                {
                    //hidden: true,

                    field: "SUPPLIER_CODE",
                    title: "CODE",
                    width: "80px"

                },
                {
                    field: "SUPPLIER_EDESC",
                    title: "NAME",
                    width: "120px"
                },
                {
                    field: "REGD_OFFICE_EADDRESS",
                    title: "OFFICE",
                    width: "120px"
                },
               

                {
                    field: "TEL_MOBILE_NO1",
                    title: "TEL1",
                    width: "80px"
                },
              

                {
                    field: "EMAIL",
                    title: "EMAIL",
                    width: "120px"
                },
                {
                    field: "TPIN_VAT_NO",
                    title: "TPIN VAT/",
                    width: "60px"
                },
               
                {
                    title: "Action ",
                    template: '<a class="fa fa-pencil-square-o editAction" title="Edit" ng-click="edit(dataItem.SUPPLIER_CODE)"><span class="sr-only"></span> </a><a class="fa fa-trash deleteAction" title="delete" ng-click="delete(dataItem.SUPPLIER_CODE)"><span class="sr-only"></span> </a>',
                  
                    width: "50px"
                }
            ],


        };

        $scope.onsiteSearch = function ($this) {

            var q = $("#txtSearchString").val();
            var grid = $("#kGrid").data("kendo-grid");
            grid.dataSource.query({
                page: 1,
                pageSize: 50,
                filter: {
                    logic: "or",
                    filters: [
                        { field: "ORDER_NO", operator: "contains", value: q },
                        { field: "ORDER_DATE", operator: "contains", value: q },
                        { field: "CREATED_BY", operator: "contains", value: q }
                    ]
                }
            });
        }
    }

    $scope.delete = function (code) {
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

                    var delUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/DeletesuppliersetupBysuppliercode?suppliercode=" + code;
                    $http({
                        method: 'POST',
                        url: delUrl
                    }).then(function successCallback(response) {
                        debugger;
                        if (response.data.MESSAGE == "DELETED") {
                            //$scope.supplierArr = [];
                            //$scope.supplierpinfo = [];
                            //$scope.suppliersconcerns = [];
                            //$scope.supplierbcenter = [];
                            //$scope.supplieri = [];
                            $scope.refresh();
                            var grid = $("#kGrid").data("kendo-grid");
                            if (grid != undefined) {
                                grid.dataSource.read();
                            }
                            bootbox.hideAll();
                            displayPopupNotification("Data succesfully deleted ", "success");
                        }
                        if (response.data.MESSAGE == "HAS_CHILD") {

                            //$scope.supplierArr = [];

                            $scope.suppliersetup.MASTER_SUPPLIER_CODE = "";
                            $scope.editcode = "";
                            $scope.edesc = "";
                            $("#supplierModal").modal("hide");
                            bootbox.hideAll();

                            $scope.refresh();
                            displayPopupNotification("Cannot Delete", "warning");
                        }
                        // this callback will be called asynchronously
                        // when the response is available
                    }, function errorCallback(response) {
                        $scope.refresh();
                        displayPopupNotification(response.data.STATUS_CODE, "error");
                        // called asynchronously if an error occurs
                        // or server returns response with an error status.
                    });

                }
                else if (result == false) {

                    $scope.refresh();
                    $("#supplierModal").modal("hide");
                    bootbox.hideAll();
                }

            }
        });
    }
    $scope.reset = function () {


        $scope.supplierpinfo =
            [{
                OWNER_NAME: '',
                DESIGNATION: '',
                CONTACT_PERSON: '',
                ADDRESS: '',
                TEL_MOBILE_NO: '',
            }]

        $scope.suppliersconcerns =
            [{
                SISTER_CONCERN_EDESC: '',
                REMARKS: '',
            }]

        $scope.supplierbcenter =
            [{
                BUDGET_CODE: '',
                REMARKS: '',
            }]

        $scope.suppliersterms =
            [{
                ITEM_NAME: '',
                MAX_LEAD_TIME: '',
                MIN_LEAD_TIME: '',
                IDEAL_LEAD_TIME: '',
                MIN_ORDER_QTY: '',
                MAX_ORDER_QTY: '',
            }]

        $scope.suppliersupplyoinfo =
            [{
                CUSTOM_FIELD: '',
                VALUE: '',
                SUPPLY_REMARKS: '',

            }]

        $scope.suppliersupplyalocinfo =
            [{
                SUPPLY_OFFICE_NAME: '',
                SUPPLY_CONTACT_PERSON: '',
                SUPPLY_ADDRESS: '',
                SUPPLY_TELEPHONE_NO: '',
                SUPPLY_EMAIL: '',
            }]

        $scope.supplieracc =
            {
                CREDIT_LIMIT: '',
                CREDIT_DAYS: '',
                CURRENT_BALANCE: '',
                IS_ACTIVE_SUPPLIER: '',

            }

        $scope.supplieri =
            [{
                REF_NO: '',
                INVOICE_DATE: '',
                DUE_DATE: '',
                TRANSACTION_TYPE: '',
                BALANCE_AMOUNT: '',
                REMARKS: '',

            }]

        $scope.suppliertandc =
            [{
                COMMENTS: '',
                TERMS_AND_CONDITIONS: '',
                CONDITIONS_VALUE: '',
                CONDITIONS_REMARKS: '',

            }]

        $scope.supplierdstatus =
            [{
                ITEM_NAME: '',
                AS_ON: '',
                QUANTITY: '',
                REMARKS: '',

            }]
    }

    $scope.newSupplierId = function () {
        var delUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/getNewSupplierId";
        $http({
            method: 'GET',
            url: delUrl
        }).then(function successCallback(response) {

            if (response.data.MESSAGE == "OK") {

                $scope.suppliersetup.SHORTCUT = response.data.DATA;
                $scope.supplierArr.SHORTCUT = response.data.DATA;

            }

            // this callback will be called asynchronously
            // when the response is available
        }, function errorCallback(response) {
            displayPopupNotification(response.data.STATUS_CODE, "error");
            // called asynchronously if an error occurs
            // or server returns response with an error status.
        });
    }

    $scope.showModalForNew = function (event) {
        debugger;
        $scope.editFlag = "N";
        $scope.AE = true;
        //$scope.fillSupllierSetupForms(Suppliercode);
        $scope.saveupdatebtn = "Save"
        $scope.groupsupplierTypeFlag = "N";
        $scope.suppliersetup.GROUP_SKU_FLAG = "I";
        $scope.supplierArr.GROUP_SKU_FLAG = "I";
        $scope.supplierArr.SUPPLIER_CODE = $scope.suppliersetup.SUPPLIER_CODE;
        //$scope.supplierArr.MASTER_SUPPLIER_CODE = $scope.suppliersetup.MASTER_SUPPLIER_CODE;
        var dropdown = $("#mastersuppliercode").data("kendoDropDownList");
        dropdown.dataSource.read();
        $scope.supplierArr.MASTER_SUPPLIER_CODE = $scope.treeSelectedMasterSupplierCode;
        //var tree = $("#mastersuppliercode").data("kendoDropDownList");
        //tree.value($scope.suppliersetup.SUPPLIER_CODE);
        var tree = $("#mastersuppliercode").data("kendoDropDownList");
        tree.value($scope.treeSelectedSupplierCode);
        
        $scope.newSupplierId();
        $($("#myTab").find("li a")[0]).trigger("click");
        $scope.supplierArr.CREATED_DATE = $filter('date')(new Date(), 'dd-MMM-yyyy');
        //$scope.supplierArr.MATURITY_DATE = $filter('date')(new Date(), 'dd-MMM-yyyy');
        $scope.ConvertEngToNepang(moment($scope.supplierArr.CREATED_DATE).format('YYYY-MM-DD'), "englishdatedocument5");
        $scope.imageurledit = "";
        //$scope.ConvertEngToNepang(moment($scope.supplierArr.MATURITY_DATE).format('YYYY-MM-DD'), "englishdatedocument6");
        $("#supplierModal").modal("toggle");


    }
    $scope.edit = function (Suppliercode) {
        debugger;
        dataFillDefered = $.Deferred();
        $scope.editFlag = "Y";
        $scope.AE = false;
        $scope.saveupdatebtn = "Update"
        $scope.fillSupllierSetupForms(Suppliercode);
        $.when(dataFillDefered).done(function () {

            debugger;
            var accd = $("#accountmap").data("kendoDropDownList");
            accd.value($scope.suppliersetup.ACC_CODE);
            var partytypeid = $("#partytypecode").data("kendoDropDownList");
            partytypeid.value($scope.suppliersetup.PARTY_TYPE_CODE);
            $scope.groupsupplierTypeFlag = "N";

            $scope.suppliersetup.GROUP_SKU_FLAG = "I";
            $scope.supplierArr.GROUP_SKU_FLAG = "I";
            $scope.savegroup = false;
            $scope.ConvertEngToNepang(moment($scope.supplierArr.CREATED_DATE).format('YYYY-MM-DD'), "englishdatedocument5");
            if ($scope.supplierArr.MATURITY_DATE != null) { $scope.ConvertEngToNepang(moment($scope.supplierArr.MATURITY_DATE).format('YYYY-MM-DD'), "englishdatedocument6"); }

        


          
            //$scope.ConvertEngToNepang(moment($scope.supplierArr.CREATED_DATE).format('MM-DD-YYYY'), "englishdatedocument5")
            //$scope.ConvertEngToNepang($scope.supplierArr.MATURITY_DATE.toString('yyyy-MM-dd'), "englishdatedocument6")
            $("#supplierModal").modal();
            setTimeout(function () {
                //$scope.loadimage($scope.supplierArr.IMAGE_FILE_NAME);
            }, 500);


        });





    }
    $scope.addNewsupplier = function () {
        debugger;
        $scope.clearData();
        $scope.editFlag = "N";
        $scope.AE = true;
        $scope.saveupdatebtn = "Save"
        $scope.groupsupplierTypeFlag = "Y";
        $scope.suppliersetup.GROUP_SKU_FLAG = "G";
        $scope.supplierArr.GROUP_SKU_FLAG = "G";
        var tree = $("#groupmastersuppliercode").data("kendoDropDownList");
        tree.value("");

        var tree = $("#suppliertree").data("kendoTreeView");
        tree.dataSource.read();
        $scope.newSupplierId();
        $("#groupsupplierModal").modal("toggle");
        $scope.savegroup = true;
    }

    $scope.ConvertNepToEng = function ($event) {
        debugger;
        //$event.stopPropagation();
        console.log($(this));
        var date = BS2AD($("#nepaliDate5").val());
        $("#englishdatedocument").val($filter('date')(date, "dd-MMM-yyyy"));
        $('#nepaliDate5').trigger('change');

        
        // var date1 = moment(maturityDate).format("DD-MM-YYYY")
        
       
    };
    $scope.ConvertEngToNepang = function (data, id) {

        var lastChar = id[id.length - 1];
        var ids = "#nepaliDate" + lastChar + "";
        $(ids).val(AD2BS(data));
    };

    $scope.monthSelectorOptions = {
        open: function () {

            var calendar = this.dateView.calendar;

            calendar.wrapper.width(this.wrapper.width() + 100);
        },
        change: function () {
            debugger;
            var id = this.element.attr('id');
            $scope.ConvertEngToNepang(kendo.toString(this.value(), 'yyyy-MM-dd'), id)
        },
        format: "dd-MMM-yyyy",

        // specifies that DateInput is used for masking the input element
        dateInput: true
    };
    $scope.monthSelectorOptionsmaturity = {
        open: function () {

            var calendar = this.dateView.calendar;

            calendar.wrapper.width(this.wrapper.width() + 100);
        },
        change: function () {

            var id = this.element.attr('id');
            $scope.ConvertEngToNepang(kendo.toString(this.value(), 'yyyy-MM-dd'), id)
        },
        format: "dd-MMM-yyyy",

        // specifies that DateInput is used for masking the input element
        dateInput: true
    };

    $scope.monthSelectorOptionsSingle = {
        open: function () {
            var calendar = this.dateView.calendar;

            calendar.wrapper.width(this.wrapper.width() - 6);
        },
        change: function () {
            $scope.ConvertEngToNepang(kendo.toString(this.value(), 'yyyy-MM-dd'))
        },
        format: "dd-MMM-yyyy",

        // specifies that DateInput is used for masking the input element
        dateInput: true
    };

    $scope.loadimage = function (img) {

        var imgfullpath = window.location.protocol + "//" + window.location.host + "/Areas/NeoERP.DocumentTemplate/images/" + img;

        $('#blah')
            .attr('src', imgfullpath)
            .width(140)
            .height(180);

        $('#txtFile')[0].files[0].name = img;
    };
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
    };
    var openingDate = $('#englishdatedocument5').val();
    $scope.OnOpeningDateChange = function () {
        var openingDate = $('#englishdatedocument5').val();
        var maturityDate = $('#englishdatedocument6').val();
        if (maturityDate != undefined && maturityDate != null && maturityDate != "undefined" && maturityDate != "") {
            var startDay = new Date(openingDate);
            var endDay = new Date(maturityDate);
            var millisBetween = startDay.getTime() - endDay.getTime();
            var days = millisBetween / (1000 * 3600 * 24);
            var finatdaysdiff = Math.round(Math.abs(days));
            $('#totaldays').val(finatdaysdiff);
        }
        var oDate = moment(openingDate).format("MM-DD-YYYY");
        var mDate = moment(maturityDate).format("MM-DD-YYYY");
        if (maturityDate != "") {
            if (mDate < oDate) {
                $('#englishdatedocument5').val("");
                $("#savedocumentformdata").prop("disabled", true);
                displayPopupNotification("Opening date must be less than Maturity Date!", "warning");
                return;
            }
            else {
                $("#savedocumentformdata").prop("disabled", false);
            }

        }

    };
    //OnNepaliOpeningDateChange
    $scope.OnMaturityDateChange = function () {
        debugger;
        var openingDate = $('#englishdatedocument5').val();
        var maturityDate = $('#englishdatedocument6').val();
       // var date1 = moment(maturityDate).format("DD-MM-YYYY")
        if (maturityDate != undefined && maturityDate != null && maturityDate != "undefined" && maturityDate != "") {
            var startDay = new Date(openingDate);
            var endDay = new Date(maturityDate);
            var millisBetween = startDay.getTime() - endDay.getTime();
            var days = millisBetween / (1000 * 3600 * 24);
            var finatdaysdiff = Math.round(Math.abs(days));
            $('#totaldays').val(finatdaysdiff);
        }
        

        var oDate = moment(openingDate).format("MM-DD-YYYY");
        var mDate = moment(maturityDate).format("MM-DD-YYYY");
        if (mDate < oDate) {
            $('#englishdatedocument6').val("");
            $("#savedocumentformdata").prop("disabled", true);
            displayPopupNotification("Maturity date must be greater than Opening Date!", "warning");
            return;
        }
        else {
            $("#savedocumentformdata").prop("disabled", false);
        }
    };




});

 
   $(document).ready(function () {
    $(document).off("keydown.bs.dropdown.data-api")
        .on("keydown.bs.dropdown.data-api", "[data-toggle='dropdown'], [role='menu']",
        $.fn.dropdown.Constructor.prototype.keydown);

});



  
  