﻿

DTModule.controller('budgetCenterSetupTreeCtrl', function ($scope, $rootScope, $http, $routeParams, $window, $filter) {
    $scope.saveupdatebtn = "Save";
    $scope.budgetcenterArr;
    $scope.savegroup = false;
    $scope.editFlag = "N";
    $scope.treenodeselected = "N";
    $scope.treeSelectedBudgetCenterCode = "";
    var budgetdataFillDefered = $.Deferred();


    var accTypeUrl123 = window.location.protocol + "//" + window.location.host + "/api/SetupApi/getAccountCodPrtyType";
    $scope.custaccountOptions = {
        dataTextField: "ACC_EDESC",
        dataValueField: "ACC_CODE",
        maxSelectedItems: 1,
        valuePrimitive: true,
        autoClose: true,
        headerTemplate: '<div class="col-md-offset-3"><strong>Select Customer acc...</strong></div>',
        placeholder: "Select Customer acc...",

        dataBound: function (e) {
            $(".k-list.k-reset").slimScroll({ 'height': '179px', 'scroll': 'scroll' });
        },
        dataSource: {
            transport: {
                read: {
                    url: accTypeUrl123,
                    dataType: "json"
                }
            }
        }
    };



    $scope.budgetcentersetup =
        {
            BUDGET_CODE: '',
            MASTER_BUDGET_CODE: '',
            BUDGET_EDESC: '',
            GROUP_SKU_FLAG: '',
            PRE_BUDGET_CODE: '',
            REMARKS: '',
            FREEZE_FLAG: '',
            BUDGET_NDESC: '',
            ASSIGN_TO: '',
            ACC_CODE: '',
            PARENT_BUDGET_CODE: '',
        }
    $scope.budgetcenterArr = $scope.budgetcentersetup;
    $scope.monthSelectorOptions = {
        open: function () {
            var calendar = this.dateView.calendar;

            calendar.wrapper.width(this.wrapper.width() + 100);
        },
        change: function () {

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

        },
        format: "dd-MMM-yyyy",


        dateInput: true
    };

    $scope.masterbudgetCodeDataSource = [
        { text: "/Root", value: "" }
    ];
    var budgetCodeUrl = window.location.protocol + "//" + window.location.host + "/api/TemplateApi/getBudgetCenterCodeWithChild";

    $scope.budgetGroupDataSource = {
        transport: {
            read: {
                url: budgetCodeUrl,
            }
        }
    };
    //UNDER GROUP DROP DOWN
    $scope.MACDBS = [];
    debugger;
    $scope.budgetGroupOptions = {
        dataSource: $scope.budgetGroupDataSource,
        optionLabel: "<PRIMARY>",
        dataTextField: "BUDGET_EDESC",
        dataValueField: "BUDGET_CODE",
        //filter: "contains",
    }
    //Clear Fields
    $scope.clearFields = function () {

        $scope.budgetcenterArr.REMARKS = "";
        $scope.budgetcenterArr.FREEZE_FLAG = "";
        $scope.budgetcenterArr.ASSIGN_TO = "";
        $scope.budgetcenterArr.BUDGET_NDESC = "";
        $scope.budgetcenterArr.EXCHANGE_RATE = " ";
        $scope.budgetcenterArr.ACC_CODE = " ";


    }

    var budgetCenterCodeUrl = window.location.protocol + "//" + window.location.host + "/api/TemplateApi/getBudgetCenterCodeWithChild";

    $scope.budgetCenterGroupDataSource = {
        transport: {
            read: {
                url: budgetCenterCodeUrl,
            }
        }
    };
    var getBudgetCenterCodeByUrl = window.location.protocol + "//" + window.location.host + "/api/TemplateApi/getbudgetCenterCode";
    $scope.budgetcentertreeData = new kendo.data.HierarchicalDataSource({

        transport: {
            read: {
                url: getBudgetCenterCodeByUrl,
                type: 'GET',
                data: function (data, evt) {
                }
            },

        },
        schema: {
            parse: function (data) {
                return data;
            },
            model: {
                id: "BudgetCenterId",
                parentId: "preBudgetCenterCode",
                children: "Items",
                fields: {
                    BUDGET_CODE: { field: "BudgetCenterId", type: "string" },
                    BUDGET_EDESC: { field: "BudgetCenterName", type: "string" },
                    parentId: { field: "preBudgetCenterCode", type: "string", defaultValue: "00" },
                }
            }
        }
    });
    $scope.bcoptions = {

        loadOnDemand: false,
        select: function (e) {
            debugger;
            var currentItem = e.sender.dataItem(e.node);
            //$('#accountGrid').removeClass("show-displaygrid");
            //$("#accountGrid").html("");
            $($(this._current).parents('ul')[$(this._current).parents('ul').length - 1]).find('span').removeClass('hasTreeCustomMenu');
            $(this._current.context).find('span').addClass('hasTreeCustomMenu');
            $scope.budgetcentersetup.BUDGET_CODE = currentItem.BUDGET_CODE;
            $scope.budgetcenterArr.BUDGET_CODE = $scope.budgetcentersetup.BUDGET_CODE;
            //$scope.budgetcentersetup.BUDGET_EDESC = currentItem.BUDGET_EDESC;
            $scope.budgetcenterArr.BUDGET_EDESC = currentItem.BUDGET_EDESC;
            $scope.budgetcentersetup.MASTER_BUDGET_CODE = currentItem.masterBudgetCenterCode;
            $scope.budgetcentersetup.GROUP_SKU_FLAG = currentItem.budgettypeflag;
            $scope.budgetcenterArr.GROUP_SKU_FLAG = currentItem.budgettypeflag;
            $scope.treeSelectedBudgetCenterCode = currentItem.BUDGET_CODE;
            $scope.treenodeselected = "Y";
            $scope.newrootinsertFlag = "N";
            var tree = $("#masterbudgetcode").data("kendoDropDownList");
            tree.value($scope.budgetcentersetup.BUDGET_CODE);
            $scope.movescrollbar();
        },
    };

    $scope.movescrollbar = function () {
        var element = $(".k-in");
        for (var i = 0; i < element.length; i++) {
            var selectnode = $(element[i]).hasClass("k-state-focused");
            if (selectnode) {
                $("#budgetcentertree").animate({
                    scrollTop: (parseInt(i))
                });
                break;
            }
        }
    }
    $scope.bconDataBound = function () {

        //$('#budgetcentertree').data("kendoTreeView").expand('.k-item');
    }
    $scope.fillbudgetCenterSetupForms = function (budgetId) {
        debugger;
        var getBudgetdetaisByUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/getBudgetCenterDetailBybudgetCode?budgetCode=" + budgetId;
        $http({
            method: 'GET',
            url: getBudgetdetaisByUrl,

        }).then(function successCallback(response) {
            $scope.budgetcentersetup = response.data.DATA;
            $scope.budgetcenterArr = response.data.DATA;

            $scope.budgetcenterArr.ACC_CODE = $scope.budgetcentersetup.ACC_CODE;
            $scope.budgetcenterArr.BUDGET_CODE = $scope.budgetcentersetup.BUDGET_CODE;

            $scope.budgetcenterArr.MASTER_BUDGET_CODE = $scope.budgetcentersetup.BUDGET_CODE;


            budgetdataFillDefered.resolve(response);
            // this callback will be called asynchronously
            // when the response is available
        }, function errorCallback(response) {

            // called asynchronously if an error occurs
            // or server returns response with an error status.
        });
    }

    $scope.BindGrid = function (groupId) {
        $(".topsearch").show();
        var url = null;
        if (groupId == "All") {
            if ($('#bgttxtSearchString').val() == null || $('#bgttxtSearchString').val() == '' || $('#bgttxtSearchString').val() == undefined || $('#bgttxtSearchString').val() == 'undefined') {
                alert('Input is empty or undefined.');
                return;
            }
            url = "/api/SetupApi/GetAllBudgetCenterList?searchtext=" + $('#bgttxtSearchString').val();
        }
        else {
            $("#bgttxtSearchString").val('');
            url = "/api/SetupApi/GetChildOfBudgetCenterByGroup?groupId=" + groupId;
        }
        $scope.budgetCenterChildGridOptions = {

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
            //resizable: true,
            dataBound: function (e) {
                $("#kGrid tbody tr").css("cursor", "pointer");
                DisplayNoResultsFound($('#kGrid'));
                $("#kGrid tbody tr").on('dblclick', function () {
                    var bcCode = $(this).find('td span').html()
                    $scope.edit(bcCode);
                   
                })
            },
            columns: [
                //{
                //    //hidden: true,
                //    field: "BUDGET_CODE",
                //    title: "Code",
                //    width: "120px"

                //},
                {
                    field: "BUDGET_EDESC",
                    title: "Budget Name",
                    width: "120px"
                },
                {
                    field: "FREEZE_FLAG",
                    title: "Freeze Flag",
                    width: "120px"
                },
                {
                    field: "CREATED_BY",
                    title: "Created By",
                    width: "120px"
                }, {
                    field: "CREATED_DATE",
                    title: "Created Date",
                    template: "#= kendo.toString(kendo.parseDate(CREATED_DATE),'dd MMM yyyy') #",
                    width: "120px"
                },

                {
                    title: "Action ",
                    template: '<a class="fa fa-pencil-square-o editAction" title="Edit" ng-click="edit(dataItem.BUDGET_CODE)"><span class="sr-only"></span> </a><a class="fa fa-trash deleteAction" title="delete" ng-click="delete(dataItem.BUDGET_CODE)"><span class="sr-only"></span> </a>',


                    width: "40px"
                }
            ],


        };


        $scope.onContextSelect = function (event) {


            if ($scope.budgetcentersetup.BUDGET_CODE == "")
                return displayPopupNotification("Select budget center.", "error");;
            $scope.saveupdatebtn = "save";
            if (event.item.innerText.trim() == "Delete") {

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

                            var delUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/DeleteBudgetCenterSetupByBudgetCode?budgetCode=" + $scope.budgetcentersetup.BUDGET_CODE;
                            $http({
                                method: 'POST',
                                url: delUrl
                            }).then(function successCallback(response) {



                                if (response.data.MESSAGE == "DELETED") {
                                    $scope.budgetcenterArr = [];

                                    $("#groupBudgetCenterModal").modal("hide");
                                    $scope.refresh();
                                    bootbox.hideAll();
                                    $scope.treenodeselected = "N";
                                    displayPopupNotification("Data succesfully deleted ", "success");
                                }
                                if (response.data.MESSAGE == "HAS_CHILD") {


                                    displayPopupNotification("Please delete the respective child first", "warning");
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
                            $("#groupBudgetCenterModal").modal("hide");
                            bootbox.hideAll();
                        }

                    }
                });
            }
            else if (event.item.innerText.trim() == "Update") {

                $scope.saveupdatebtn = "Update";
                $scope.fillbudgetCenterSetupForms($scope.budgetcentersetup.BUDGET_CODE);
                $.when(budgetdataFillDefered).then(function () {


                    if ($scope.budgetcentersetup.PARENT_BUDGET_CODE == null || $scope.budgetcentersetup.PARENT_BUDGET_CODE == undefined) {
                        var popUpDropdown = $("#masterbudgetcode").data("kendoDropDownList");
                        popUpDropdown.value('');
                    }
                    else {
                        var popUpDropdown = $("#masterbudgetcode").data("kendoDropDownList");
                        popUpDropdown.value($scope.budgetcentersetup.PARENT_BUDGET_CODE);
                    }
                    $("#groupBudgetCenterModal").modal();
                });

            }
            else if (event.item.innerText.trim() == "Add") {


                $scope.savegroup = true;
                $scope.fillbudgetCenterSetupForms($scope.budgetcentersetup.BUDGET_CODE);
                //$scope.budgetcenterArr = [];
                $.when(budgetdataFillDefered).then(function () {



                    var tree = $("#masterbudgetcode").data("kendoDropDownList");
                    tree.value($scope.budgetcenterArr.BUDGET_CODE);

                    $scope.budgetcentersetup.GROUP_SKU_FLAG = "G";
                    $scope.budgetcenterArr.GROUP_SKU_FLAG = "G";
                    $scope.budgetcenterArr.BUDGET_EDESC = "";
                    $('#budgetname').val("");
                    $scope.clearFields();
                    $("#groupBudgetCenterModal").modal();
                });

            }
        }

    }

    $scope.showModalForNew = function (event) {
        $scope.saveupdatebtn = "save"
        $scope.editFlag = "N";
        var returnMaxCustomerUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/getBudgetCenterDetailBybudgetCode?budgetCode=" + $scope.treeSelectedBudgetCenterCode;
        $http({
            method: 'GET',
            url: returnMaxCustomerUrl,
        }).then(function successCallback(response) {

            $scope.budgetcentersetup = response.data.DATA;
            $scope.budgetcenterArr = response.data.DATA;
            $scope.budgetcenterArr.BUDGET_CODE = $scope.budgetcentersetup.BUDGET_CODE;
            $scope.budgetcenterArr.MASTER_BUDGET_CODE = $scope.budgetcentersetup.BUDGET_CODE;
            $scope.budgetcenterArr.BUDGET_EDESC = "";
            var tree = $("#masterbudgetcode").data("kendoDropDownList");
            tree.value($scope.budgetcenterArr.BUDGET_CODE);
            $scope.budgetcentersetup.GROUP_SKU_FLAG = "I";
            $scope.budgetcenterArr.GROUP_SKU_FLAG = "I";
            $scope.clearFields();
            $("#groupBudgetCenterModal").modal();

        }, function errorCallback(response) {
            displayPopupNotification(response.data.STATUS_CODE, "error");

        });

    }
    $scope.MACDBCOptions = {
        dataSource: $scope.MACDBS,
        dataTextField: "text",
        dataValueField: "value",

    };
    $scope.masterbudgetCodeOptions = {
        dataSource: $scope.masterbudgetCodeDataSource,
        dataTextField: "text",
        dataValueField: "value",
    };
    $scope.edit = function (budgetCode) {
        debugger;
        $scope.editFlag = "Y";
        $scope.saveupdatebtn = "Update";

        $scope.fillbudgetCenterSetupForms(budgetCode);
        $.when(budgetdataFillDefered).done(function () {
            $scope.groupbudgetTypeFlag = "N";
            $("#groupBudgetCenterModal").modal();
           
        });


    }
    $scope.saveNewBudgetCenter = function (isValid) {
        debugger;
        if (!isValid) {
            displayPopupNotification("Input fields are not valid. Please review and try again", "warning");
            return;
        }
        // return;
        if ($scope.saveupdatebtn == "save") {

            var createurl = window.location.protocol + "//" + window.location.host + "/api/setupapi/createNewBudgetHead";
            var model = {
                BUDGET_CODE: $scope.budgetcenterArr.BUDGET_CODE,
                BUDGET_EDESC: $scope.budgetcenterArr.BUDGET_EDESC,
                MASTER_BUDGET_CODE: $scope.budgetcenterArr.MASTER_BUDGET_CODE,
                GROUP_SKU_FLAG: $scope.budgetcenterArr.GROUP_SKU_FLAG,
                PRE_BUDGET_CODE: $scope.budgetcenterArr.PRE_BUDGET_CODE,
                REMARKS: $scope.budgetcenterArr.REMARKS,
                FREEZE_FLAG: $scope.budgetcenterArr.FREEZE_FLAG,
                BUDGET_NDESC: $scope.budgetcenterArr.BUDGET_NDESC,
                ASSIGN_TO: $scope.budgetcenterArr.ASSIGN_TO,
                ACC_CODE: $scope.budgetcenterArr.ACC_CODE

            }
            $http({
                method: 'post',
                url: createurl,
                data: model
            }).then(function successcallback(response) {

                if (response.data.MESSAGE == "INSERTED") {

                    $scope.budgetcenterArr = [];
                    $scope.budgetcenterArr.BUDGET_EDESC = "";
                    if ($scope.budgetcentersetup.GROUP_SKU_FLAG !== "I") {

                        var tree = $("#budgetcentertree").data("kendoTreeView");
                        if (tree != undefined) {
                            tree.dataSource.read();
                        }
                    }

                    var grid = $("#kGrid").data("kendoGrid");
                    if (grid != undefined) {

                        grid.dataSource.read();
                    }
                    var ddl = $("#masterbudgetcode").data("kendoDropDownList");
                    if (ddl != undefined)
                        ddl.dataSource.read();
                    $scope.clearFields();
                    $("#groupBudgetCenterModal").modal("toggle");
                    displayPopupNotification("data succesfully saved ", "success");
                }
                else {
                    displayPopupNotification("Something went wrong.Please try again later.", "error");
                }
                // this callback will be called asynchronously
                // when the response is available
            }, function errorcallback(response) {
                displayPopupNotification("Something went wrong.Please try again later.", "error");
                // called asynchronously if an error occurs
                // or server returns response with an error status.
            });
        }
        else {
            debugger;
            var updateurl = window.location.protocol + "//" + window.location.host + "/api/setupapi/updateBudgetByAccCode";
            var model = {
                BUDGET_CODE: $scope.budgetcenterArr.BUDGET_CODE,
                BUDGET_EDESC: $scope.budgetcenterArr.BUDGET_EDESC,
                MASTER_BUDGET_CODE: $scope.budgetcenterArr.MASTER_BUDGET_CODE,
                GROUP_SKU_FLAG: $scope.budgetcenterArr.GROUP_SKU_FLAG,
                PRE_BUDGET_CODE: $scope.budgetcenterArr.PRE_BUDGET_CODE,
                REMARKS: $scope.budgetcenterArr.REMARKS,
                FREEZE_FLAG: $scope.budgetcenterArr.FREEZE_FLAG,
                BUDGET_NDESC: $scope.budgetcenterArr.BUDGET_NDESC,
                ASSIGN_TO: $scope.budgetcenterArr.ASSIGN_TO,
                ACC_CODE: $scope.budgetcenterArr.ACC_CODE
            }
            //model.ACC_CODE = $scope.budgetcenterArr.ACC_CODE[0];
            $scope.saveupdatebtn = "Update";
            $http({
                method: 'post',
                url: updateurl,
                data: model
            }).then(function successcallback(response) {


                if (response.data.MESSAGE == "UPDATED") {

                    $scope.budgetcenterArr = [];
                    if ($scope.budgetcentersetup.GROUP_SKU_FLAG !== "I") {
                        var tree = $("#budgetcentertree").data("kendoTreeView");
                        if (tree != undefined) {
                            tree.dataSource.read();
                        }
                    }


                    var ddl = $("#masterbudgetcode").data("kendoDropDownList");
                    if (ddl != undefined)
                        ddl.dataSource.read();

                    var grid = $("#kGrid").data("kendoGrid");

                    if (grid != undefined) {
                        grid.dataSource.read();
                    }
                    $scope.clearFields();
                    $("#groupBudgetCenterModal").modal("toggle");
                    displayPopupNotification("data succesfully updated ", "success");
                }
                if (response.data.MESSAGE == "error") {
                    displayPopupNotification("Something went wrong.Please try again later.", "error");
                }
                // this callback will be called asynchronously
                // when the response is available
            }, function errorcallback(response) {

                displayPopupNotification("Something went wrong.Please try again later.", "error");
                // called asynchronously if an error occurs
                // or server returns response with an error status.
            });
        }
    }

    $scope.addnewaccount = function () {

        $scope.editFlag = "N";
        $scope.saveupdatebtn = "save"


        $scope.groupbudgetTypeFlag = "Y";
        $scope.budgetcentersetup.GROUP_SKU_FLAG = "G";
        $scope.budgetcenterArr.GROUP_SKU_FLAG = "G";
        var tree = $("#masterbudgetcode").data("kendoDropDownList");
        tree.value("");
        var tree = $("#budgetcentertree").data("kendoTreeView");
        tree.dataSource.read();
        var grid = $("#kGrid").data("kendo-grid");
        if (grid != undefined) {
            grid.dataSource.data([]);
        }
        $scope.clearFields();
        $("#groupBudgetCenterModal").modal("toggle");

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

                    $scope.budgetcentersetup.GROUP_SKU_FLAG = "I";

                    var delUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/DeleteBudgetCenterSetupByBudgetCode?budgetCode=" + code;
                    $http({
                        method: 'POST',
                        url: delUrl
                    }).then(function successCallback(response) {



                        if (response.data.MESSAGE == "DELETED") {

                            $scope.budgetcenterArr = [];
                            if ($scope.budgetcentersetup.GROUP_SKU_FLAG !== "I") {
                                var tree = $("#budgetcentertree").data("kendoTreeView");
                                if (tree != undefined) {
                                    tree.dataSource.read();
                                }
                            }
                            $("#groupBudgetCenterModal").modal("hide");
                            //$scope.refresh();
                            var grid = $('#kGrid').data("kendoGrid");
                            grid.dataSource.read();
                            bootbox.hideAll();
                            displayPopupNotification("Data succesfully deleted ", "success");
                        }
                        if (response.data.MESSAGE == "HAS_CHILD") {

                           
                            displayPopupNotification("Please delete the respective child first", "warning");
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
                    $("#groupBudgetCenterModal").modal("hide");
                    bootbox.hideAll();
                }

            }
        });
    }
    $scope.refresh = function () {

        var tree = $("#budgetcentertree").data("kendoTreeView");
        tree.dataSource.read();
        var grid = $("#kGrid").data("kendo-grid");
        if (grid != undefined) {
            grid.dataSource.read();
        }
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


});