﻿DTModule.controller('BranchSetupCtrl', function ($scope, $rootScope, $http, $routeParams, $window, $filter) {
    $scope.saveupdatebtn = "Save";
    $scope.branchsetup = [];
    $scope.savegroup = false;
    $scope.groupschilddisabled = true;
    $scope.editFlag = "N";
    $scope.treenodeselected = "N";
    $scope.treeSelectedBranchCenterCode = "";
    $scope.phoneNumbr = /^[0-9]{9,10}$/;
    var branchdataFillDefered = $.Deferred();

    $scope.branchsetup =
        {
             BRANCH_CODE: '',
             BRANCH_NDESC: '',
             BRANCH_EDESC: '',
             GROUP_SKU_FLAG: '',
             PRE_BRANCH_CODE: '',
             REMARKS: '',
             ADDRESS: '',
             TELEPHONE_NO: '',
             EMAIL: '',
             ABBR_CODE: '',
             MASTER_BRANCH_CODE: '',
             PARENT_BRANCH_CODE:''

        }
    $scope.branchgArr = $scope.branchsetup;

    //Clear Fields
    $scope.clearFields = function () {
        $scope.branchgArr = {
            BRANCH_NDESC: "",
            BRANCH_EDESC: "",
            REMARKS: "",
            ADDRESS: "",
            TELEPHONE_NO: "",
            EMAIL: "",
        };
      
    }

    //Add new Branch in Root tree
    $scope.addnewaccount = function () {
        $scope.editFlag = "N";
        $scope.saveupdatebtn = "save"
        $scope.brancgArr = [];
        $scope.clearFields();
        //edit function me ba
        
        $scope.groupbranchTypeFlag = "Y";
        $scope.branchsetup.GROUP_SKU_FLAG = "G";
        $scope.branchgArr.GROUP_SKU_FLAG = "G";
        var tree = $("#masterbranchcode").data("kendoDropDownList");
        tree.value("");
        var tree = $("#branchParenttree").data("kendoTreeView");
        tree.dataSource.read();
        var grid = $("#kGrid").data("kendo-grid");
        if (grid != undefined) {
            grid.dataSource.data([]);
        }
        $("#branchSetupModal").modal("toggle");

    }



    //Add new Branch in Child Grid


    $scope.showModalForNew = function (event) { 
        debugger;
        $scope.saveupdatebtn = "save"
        $scope.editFlag = "N";
       
        var returnMaxCustomerUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/getBranchCenterDetailBybranchCode?branchCode=" + $scope.treeSelectedBranchCenterCode;
        $http({
            method: 'GET',
            url: returnMaxCustomerUrl,
        }).then(function successCallback(response) {
            debugger;
            $scope.branchsetup = response.data.DATA;
            $scope.branchgArr = response.data.DATA;
            $scope.branchgArr.BRANCH_CODE = $scope.branchsetup.BRANCH_CODE;
            $scope.branchgArr.MASTER_BRANCH_CODE = $scope.branchsetup.BRANCH_CODE;
            $scope.branchgArr.BRANCH_EDESC = "";
            var tree = $("#masterbranchcode").data("kendoDropDownList");
            tree.value($scope.branchgArr.BRANCH_CODE);
            $scope.branchsetup.GROUP_SKU_FLAG = "I";
            $scope.branchgArr.GROUP_SKU_FLAG = "I";
            $scope.clearFields();
            $("#branchSetupModal").modal();

           
        }, function errorCallback(response) {
            displayPopupNotification(response.data.STATUS_CODE, "error");

        });
    }
    //Refresh Grid

    $scope.refresh = function () {

        var tree = $("#branchParenttree").data("kendoTreeView");
        tree.dataSource.read();
        var grid = $("#kGrid").data("kendo-grid");
        if (grid != undefined) {
            grid.dataSource.read();
        }
    }

    //undergroup dropdown 

    $scope.branchGroupDataSource = [
        { text: "<PRIMARY>", value: "" }
    ];



    var branchCodeUrl = window.location.protocol + "//" + window.location.host + "/api/TemplateApi/getBranchCodeWithChild";

    $scope.branchGroupDataSource = new kendo.data.DataSource({
        transport: {
            read: {
                url: branchCodeUrl,
            }
        }
    });
   

    $scope.branchGroupOptions = {
        dataSource: $scope.branchGroupDataSource,
        optionLabel: "<PRIMARY>",
        dataTextField: "BRANCH_EDESC",
        dataValueField: "BRANCH_CODE",
        change: function (e) {

            var currentItem = e.sender.dataItem(e.node);

        },
        dataBound: function (e) {
            $scope.branchGroupDataSource;

        }
    }





    //parent treeView
    var getParentBranchByUrl = window.location.protocol + "//" + window.location.host + "/api/TemplateApi/GetBranch";
  
    $scope.branchtreeData = new kendo.data.HierarchicalDataSource({
        
        transport: {
            read: {
                url: getParentBranchByUrl,
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
                id: "BRANCH_CODE",
                parentId: "preBranchCode",
                children: "Items",
                fields: {
                    BRANCH_CODE: { field: "BRANCH_CODE", type: "string" },
                    BRANCH_EDESC: { field: "BRANCH_EDESC", type: "string" },
                    parentId: { field: "preBranchCode", type: "string", defaultValue: "00" },
                }
            }
        }
    });


    $scope.bcoptions = {
        loadOnDemand: false,
        select: function (e) {
            debugger;
            var currentItem = e.sender.dataItem(e.node);
            $('#branchGrid').removeClass("show-displaygrid");
            $("#branchGrid").html("");
            $($(this._current).parents('ul')[$(this._current).parents('ul').length - 1]).find('span').removeClass('hasTreeCustomMenu');
            $(this._current.context).find('span').addClass('hasTreeCustomMenu');
            $scope.branchsetup.BRANCH_CODE = currentItem.BranchId;
            $scope.branchgArr.BRANCH_CODE = currentItem.BranchId;
            $scope.branchsetup.BRANCH_EDESC = currentItem.BRANCH_EDESC;
          
           // $scope.branchsetup.MASTER_BRANCH_CODE = currentItem.masterBranchCenterCode;
            $scope.branchsetup.MASTER_BRANCH_CODE = currentItem.BranchId;
            $scope.groupschilddisabled = true;
            $scope.branchsetup.GROUP_SKU_FLAG = currentItem.GROUP_SKU_FLAG;
            $scope.branchgArr.GROUP_SKU_FLAG = currentItem.GROUP_SKU_FLAG;
            $scope.treeSelectedBranchCenterCode = currentItem.BranchId;
            $scope.treenodeselected = "Y";
            $scope.newrootinsertFlag = "N";
            //masterbranchcode=under group dropdown wala
            var tree = $("#masterbranchcode").data("kendoDropDownList");
            tree.value($scope.branchgArr.BRANCH_CODE);
            $scope.movescrollbar();
        },

    };


    $scope.bconDataBound = function () {
        $('#branchParenttree').data("kendoTreeView").expand('.k-item');
    }



    $scope.movescrollbar = function () {
        var element = $(".k-in");
        for (var i = 0; i < element.length; i++) {
            var selectnode = $(element[i]).hasClass("k-state-focused");
            if (selectnode) {
                $("#branchParenttree").animate({
                    scrollTop: (parseInt(i))
                });
                break;
            }
        }
    }


    $scope.fillbranchCenterSetupForms = function (branchId) {
      
        var getBranchdetaisByUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/getBranchCenterDetailBybranchCode?branchCode=" + branchId;
        $http({
            method: 'GET',
            url: getBranchdetaisByUrl,

        }).then(function successCallback(response) {
            $scope.branchsetup = response.data.DATA;
            $scope.branchgArr = response.data.DATA;
            $scope.branchgArr.BRANCH_CODE = $scope.branchsetup.BRANCH_CODE;
            $scope.branchgArr.MASTER_BRANCH_CODE = $scope.branchsetup.BRANCH_CODE;
            branchdataFillDefered.resolve(response);
        }, function errorCallback(response) {
        });
    }




    //Main grid bind


    $scope.BindGrid = function (groupId) {
        $(".topsearch").show();
        var url = null;
        if (groupId == "All") {
            if ($('#bchtxtSearchString').val() == null || $('#bchtxtSearchString').val() == '' || $('#bchtxtSearchString').val() == undefined || $('#bchtxtSearchString').val() == 'undefined') {
                alert('Input is empty or undefined.');
                return;
            }
            url = "/api/SetupApi/GetAllBranchCenterList?searchtext=" + $('#bchtxtSearchString').val();
        }
        else {
            $("#bchtxtSearchString").val('');
            url = "/api/SetupApi/GetChildOfBranchCenterByGroup?groupId=" + groupId;
        }
        $scope.branchCenterChildGridOptions = {
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
                //    field: "BRANCH_CODE",
                //    title: "Code",
                //    width: "80px"

                //},
                {
                    field: "BRANCH_EDESC",
                    title: "Branch Name",
                    width: "100px"
                },
                {
                    field: "ADDRESS",
                    title: "Address",
                    width: "100px"
                },
                {
                    field: "EMAIL",
                    title: "Email",
                    width: "80px"
                }, {
                    field: "REMARKS",
                    title: "Remarks",
                    width: "80px"
                   
                },
               
                {
                    
                    title: "Action ",
                    template: '<a class="fa fa-pencil-square-o editAction" title="Edit" ng-click="edit(dataItem.BRANCH_CODE)"><span class="sr-only"></span> </a><a class="fa fa-trash deleteAction" title="delete" ng-click="delete(dataItem.BRANCH_CODE)"><span class="sr-only"></span> </a>',
                    width: "40px"
                }
            ],


        };

        //Context Menu here

        $scope.onContextSelect = function (event) {


            if ($scope.branchsetup.BRANCH_CODE == "")
                return displayPopupNotification("Select branch center.", "error");;
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

                            var delUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/DeleteBranchCenterSetupByBranchCode?branchCode=" + $scope.branchsetup.BRANCH_CODE;
                            $http({
                                method: 'POST',
                                url: delUrl
                            }).then(function successCallback(response) {



                                if (response.data.MESSAGE == "DELETED") {
                              
                                    $scope.treenodeselected = "N";
                                    $("#branchSetupModal").modal("hide");
                                    $scope.refresh();
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
                            $("#branchSetupModal").modal("hide");
                            bootbox.hideAll();
                        }

                    }
                });
            }
            else if (event.item.innerText.trim() == "Update") {
                
                $scope.saveupdatebtn = "Update";
                $scope.fillbranchCenterSetupForms($scope.branchsetup.BRANCH_CODE);
                $.when(branchdataFillDefered).then(function () {

                   
                    if ($scope.branchsetup.PARENT_BRANCH_CODE == null || $scope.branchsetup.PARENT_BRANCH_CODE == undefined) {
                        var popUpDropdown = $("#masterbranchcode").data("kendoDropDownList");
                        popUpDropdown.value('');
                    }
                    else {
                        var popUpDropdown = $("#masterbranchcode").data("kendoDropDownList");
                        popUpDropdown.value($scope.branchsetup.PARENT_BRANCH_CODE);
                    }
                    $("#branchSetupModal").modal();
                });

            }
            else if (event.item.innerText.trim() == "Add") {
                debugger;
               
                $scope.savegroup = true;
                $scope.fillbranchCenterSetupForms($scope.branchsetup.BRANCH_CODE);
               
                $.when(branchdataFillDefered).then(function () {
               
                    var tree = $("#masterbranchcode").data("kendoDropDownList");
                    tree.value($scope.branchgArr.BRANCH_CODE);
                    $scope.branchsetup.GROUP_SKU_FLAG = "G";
                    $scope.branchgArr.GROUP_SKU_FLAG = "G";
                    $scope.branchsetup.BRANCH_EDESC = "";
                    $scope.branchsetup.REMARKS = "";
                    $scope.branchsetup.ADDRESS = "";
                    $scope.branchsetup.TELEPHONE_NO = "";
                    $scope.branchsetup.EMAIL = "";
                    $('#branchengname').val("");
                    $scope.clearFields();
                    $("#branchSetupModal").modal();
                });

            }
        }

    }

    //Delete function for child
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

                    $scope.branchsetup.GROUP_SKU_FLAG = "I";

                    var delUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/DeleteBranchCenterSetupByBranchCode?branchCode=" + code;
                    $http({
                        method: 'POST',
                        url: delUrl
                    }).then(function successCallback(response) {



                        if (response.data.MESSAGE == "DELETED") {
                            debugger;
                           // $scope.budgetcenterArr = [];
                            if ($scope.branchsetup.GROUP_SKU_FLAG !== "I") {
                                var tree = $("#branchParenttree").data("kendoTreeView");
                                if (tree != undefined) {
                                    tree.dataSource.read();
                                }
                            }
                            $("#branchSetupModal").modal("hide");
                            //$scope.refresh();
                            var grid = $('#kGrid').data("kendoGrid");
                            grid.dataSource.read();
                            bootbox.hideAll();
                            $scope.treenodeselected = "N";
                            $scope.refresh();
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
                    $("#branchSetupModal").modal("hide");
                    bootbox.hideAll();
                }

            }
        });
    }


    //Edit Function
    $scope.edit = function (branchCode) {

        $scope.editFlag = "Y";
        $scope.saveupdatebtn = "Update";

        $scope.fillbranchCenterSetupForms(branchCode);
        $.when(branchdataFillDefered).done(function () {
            $scope.groupbranchTypeFlag = "N";
            $("#branchSetupModal").modal();
            
        });


    }

    //Save and Update Function
    $scope.saveNewBranchCenter = function () {
        debugger;
        if (!$scope.branchArrform.$valid) {
            displayPopupNotification("Invalid input fileds", "warning");
            return;
        }
       
        if ($scope.saveupdatebtn == "save") {
            var prebranchcode = $("#masterbranchcode").data("kendoDropDownList").value();

            var createurl = window.location.protocol + "//" + window.location.host + "/api/setupapi/createNewBranchHead";
            var model = {
                BRANCH_CODE: $scope.branchgArr.BRANCH_CODE === undefined ? $scope.branchsetup.BRANCH_CODE : $scope.branchgArr.BRANCH_CODE,
                //BRANCH_CODE: $scope.branchgArr.BRANCH_CODE,
                BRANCH_EDESC: $scope.branchgArr.BRANCH_EDESC,
                BRANCH_NDESC: $scope.branchgArr.BRANCH_NDESC,
                ADDRESS: $scope.branchgArr.ADDRESS,
                TELEPHONE_NO: $scope.branchgArr.TELEPHONE_NO,
                EMAIL: $scope.branchgArr.EMAIL,
                MASTER_BRANCH_CODE: $scope.branchgArr.MASTER_BRANCH_CODE,
                //GROUP_SKU_FLAG: $scope.branchgArr.GROUP_SKU_FLAG,
                GROUP_SKU_FLAG: $scope.branchgArr.GROUP_SKU_FLAG === undefined ? $scope.branchsetup.GROUP_SKU_FLAG : $scope.branchgArr.GROUP_SKU_FLAG,
                PRE_BRANCH_CODE: prebranchcode,
                REMARKS: $scope.branchgArr.REMARKS,
                

            }
            $http({
                method: 'post',
                url: createurl,
                data: model
            }).then(function successcallback(response) {
                debugger;
                if (response.data.MESSAGE == "INSERTED") {

                    $scope.branchgArr = [];
                    $scope.branchgArr.BRANCH_EDESC = "";
                    if ($scope.branchsetup.GROUP_SKU_FLAG !== "I") {

                        $("#branchParenttree").data("kendoTreeView").dataSource.read();
                       
                    }
                    if ($scope.savegroup == false) { $("#branchSetupModal").modal("toggle"); }
                    else { $("#branchSetupModal").modal("toggle"); }

                    var grid = $("#kGrid").data("kendoGrid");
                    if (grid != undefined) {

                        grid.dataSource.read();
                    }
                    
                    $("#masterbranchcode").data("kendoDropDownList").dataSource.read();
                    $scope.refresh();
                    $scope.treenodeselected = "N";
                    displayPopupNotification("data succesfully saved ", "success");
                }
                else {
                    displayPopupNotification("Something went wrong.Please try again later.", "error");
                }
                // this callback will be called asynchronously
                // when the response is available
            }, function errorcallback(response) {
                    debugger;
                displayPopupNotification("Something went wrong.Please try again later.", "error");
                // called asynchronously if an error occurs
                // or server returns response with an error status.
            });
        }
        else {

            var updateurl = window.location.protocol + "//" + window.location.host + "/api/setupapi/updateBranchByBranchCode";
            var model = {
                BRANCH_CODE: $scope.branchgArr.BRANCH_CODE,
                BRANCH_EDESC: $scope.branchgArr.BRANCH_EDESC,
                BRANCH_NDESC: $scope.branchgArr.BRANCH_NDESC,
                ADDRESS: $scope.branchgArr.ADDRESS,
                TELEPHONE_NO: $scope.branchgArr.TELEPHONE_NO,
                EMAIL: $scope.branchgArr.EMAIL,
                MASTER_BRANCH_CODE: $scope.branchgArr.MASTER_BRANCH_CODE,
                GROUP_SKU_FLAG: $scope.branchgArr.GROUP_SKU_FLAG,
                PRE_BRANCH_CODE: $scope.branchgArr.prebranchcode,
                REMARKS: $scope.branchgArr.REMARKS,

            }
            $scope.saveupdatebtn = "Update";
            $http({
                method: 'post',
                url: updateurl,
                data: model
            }).then(function successcallback(response) {


                if (response.data.MESSAGE == "UPDATED") {

                    $scope.branchgArr = [];
                    if ($scope.branchsetup.GROUP_SKU_FLAG !== "I") {
                        $("#branchParenttree").data("kendoTreeView").dataSource.read();
                        
                        
                    }


                   

                    var grid = $("#kGrid").data("kendoGrid");

                    if (grid != undefined) {
                        grid.dataSource.read();
                    }
                    $("#masterbranchcode").data("kendoDropDownList").dataSource.read();
                    $("#branchSetupModal").modal("toggle");
                    $scope.refresh();
                    $scope.treenodeselected = "N";
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


    $scope.SubLedger_Cancel = function () {

        debugger;
        $scope.refresh();
    }


});