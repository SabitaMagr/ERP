﻿DTModule.controller('divisionSetupCtrl', function ($scope, $rootScope, $http, $routeParams, $window, $filter) {
    $scope.saveupdatebtn = "Save";
    $scope.savegroup = false;
    $scope.editFlag = "N";
    $scope.treenodeselected = "N";
    $scope.phoneNumbr = /^\+?(\d+(\.(\d+)?)?|\.\d+)$/;
    $scope.treeSelectedDivisionCode = "";
    $scope.groupschilddisabled = true;
    $scope.masterdivisiondisabled = false;
    //To open the context menu and bind data use divisiondataFillDefered
    var divisiondataFillDefered = $.Deferred();


    $scope.divisionsetup =
        {
            DIVISION_CODE: '',
            DIVISION_EDESC: '',
          
            GROUP_SKU_FLAG: '',
            PRE_DIVISION_CODE: '',
            REMARKS: '',
            ADDRESS: '',
            TELEPHONE_NO: '',
            EMAIL: '',
            ABBR_CODE: '',
            MASTER_DIVISION_CODE: '',
            PARENT_DIVISION_CODE: ''

        }
    $scope.divisionArr = $scope.divisionsetup;


    //Grid for Root tree View

    var getDivisionCodeByUrl = window.location.protocol + "//" + window.location.host + "/api/TemplateApi/GetDivision";
    debugger;
    $scope.divisiontreeData = new kendo.data.HierarchicalDataSource({
        
        transport: {
            read: {
                url: getDivisionCodeByUrl,
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
                id: "DIVISION_CODE",
                parentId: "PRE_DIVISION_CODE",
                children: "Items",
                fields: {
                    DIVISION_CODE: { field: "DIVISION_CODE", type: "string" },
                    RESOURCE_EDESC: { field: "DIVISION_EDESC", type: "string" },
                    parentId: { field: "PRE_DIVISION_CODE", type: "string", defaultValue: "00" },
                }
            }
        }
    });


    //treeview on select
    $scope.divisionOptions = {
        loadOnDemand: false,
        select: function (e) {
            debugger;
            var currentItem = e.sender.dataItem(e.node);
            $('#divisionGrid').removeClass("show-displaygrid");
            $("#divisionGrid").html("");
            $($(this._current).parents('ul')[$(this._current).parents('ul').length - 1]).find('span').removeClass('hasTreeCustomMenu');
            $(this._current.context).find('span').addClass('hasTreeCustomMenu');
            $scope.divisionsetup.DIVISION_CODE = currentItem.divisionId;
            $scope.divisionArr.DIVISION_CODE = currentItem.divisionId;
            $scope.treeSelectedDivisionCode = currentItem.divisionId;
            $scope.divisionsetup.GROUP_SKU_FLAG = currentItem.groupSkuFlag;
            $scope.divisionArr.GROUP_SKU_FLAG = currentItem.groupSkuFlag;
            $scope.divisionsetup.DIVISION_EDESC = currentItem.divisionName;
            $scope.groupschilddisabled = true;
            $scope.divisionsetup.MASTER_DIVISION_CODE = currentItem.DIVISION_CODE;
            $scope.treenodeselected = "Y";
            $scope.newrootinsertFlag = "N";
            $scope.$apply();
            //$scope.movescrollbar();
        },

    };


    //data Bound
    $scope.divisionDataBound = function () {
        $('#divisionRoottree').data("kendoTreeView").expand('.k-item');
    }


    //kendo Grid View
    $scope.BindGrid = function (groupId) {
        $(".topsearch").show();
        var url = null;
        if (groupId == "All") {
            if ($('#divtxtSearchString').val() == null || $('#divtxtSearchString').val() == '' || $('#divtxtSearchString').val() == undefined || $('#divtxtSearchString').val() == 'undefined') {
                alert('Input is empty or undefined.');
                return;
            }
            url = "/api/SetupApi/GetAllLocationList?searchtext=" + $('#divtxtSearchString').val();
        }
        else {
            $("#divtxtSearchString").val('');
            url = "/api/SetupApi/GetChildOfDivisionByGroup?groupId=" + groupId;
        }
        $scope.divisionChildGridOptions = {
            dataSource: {
                type: "json",
                transport: {
                    read: url,
                },
                pageSize: 50,
                serverSorting: true
            },
            scrollable: true,
            height: 450,
            sortable: true,
            pageable: true,
            dataBound: function (e) {
                debugger;
                $("#kGrid tbody tr").css("cursor", "pointer");
                DisplayNoResultsFound($('#kGrid'));
                $("#kGrid tbody tr").on('dblclick', function () {
                    var resourceCode = $(this).find('td span').html()
                    $scope.edit(resourceCode);
                    var tree = $("#divisionRoottree").data("kendoTreeView");
                    tree.dataSource.read();
                })
            },
            columns: [
                //{
                    
                //    title: "Division ID",
                //    field: "DIVISION_CODE",
                //    width: "80px"

                //},
                {
                    field: "DIVISION_EDESC",
                    title: "Division Name",
                    width: "120px"
                },
                {
                    field: "ADDRESS",
                    title: "Address",
                    width: "120px"
                },
                {
                    field: "EMAIL",
                    title: "Email",
                    width: "120px"
                },
                
                {
                    field: "REMARKS",
                    title: "Remarks",
                    width: "120px"
                },
               
               
                {
                    //command
                    title: "Action ",
                    template: '<a class="fa fa-pencil-square-o editAction" title="Edit" ng-click="edit(dataItem.DIVISION_CODE)"><span class="sr-only"></span> </a><a class="fa fa-trash deleteAction" title="Delete" ng-click="delete(dataItem.DIVISION_CODE,dataItem.GROUP_SKU_FLAG)"><span class="sr-only"></span> </a>',
                    width: "60px"
                }
            ],


        };


        $scope.onContextSelect = function (event) {


            if ($scope.divisionsetup.DIVISION_CODE == "")
                return displayPopupNotification("Select division.", "error");;
                $scope.saveupdatebtn = "Save";
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

                            var delUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/DeleteDivisionSetupByDivisionCode?divisionCode=" + $scope.divisionsetup.DIVISION_CODE;
                            $http({
                                method: 'POST',
                                url: delUrl
                            }).then(function successCallback(response) {



                                if (response.data.MESSAGE == "DELETED") {
                                    

                                    $("#divisionSetupModal").modal("hide");
                                    $scope.treenodeselected = "N";
                                    $scope.refresh();
                                    bootbox.hideAll();
                                    displayPopupNotification("Data succesfully deleted ", "success");
                                }
                                if (response.data.MESSAGE == "HAS_CHILD") {

                                    
                                    displayPopupNotification("Please delete the respective child first", "warning");
                                }
                                
                            }, function errorCallback(response) {
                                $scope.refresh();
                                displayPopupNotification(response.data.STATUS_CODE, "error");
                                
                            });

                        }
                        else if (result == false) {

                            $scope.refresh();
                            $("#divisionSetupModal").modal("hide");
                            bootbox.hideAll();
                        }

                    }
                });
            }
            else if (event.item.innerText.trim() == "Update") {
                debugger; 
                divisiondataFillDefered = $.Deferred();
                $scope.saveupdatebtn = "Update";
                $scope.fillDivisionSetupForms($scope.divisionsetup.DIVISION_CODE);
                $.when(divisiondataFillDefered).then(function () {


                    if ($scope.divisionsetup.PARENT_DIVISION_CODE == null || $scope.divisionsetup.PARENT_DIVISION_CODE == undefined) {
                        var popUpDropdown = $("#masterdivisioncode").data("kendoDropDownList");
                        popUpDropdown.value('');
                    }
                    else {
                        var popUpDropdown = $("#masterdivisioncode").data("kendoDropDownList");
                        popUpDropdown.value($scope.divisionsetup.PARENT_DIVISION_CODE);
                    }
                    $("#divisionSetupModal").modal();
                });

            }
            else if (event.item.innerText.trim() == "Add") {

                divisiondataFillDefered = $.Deferred();
                $scope.savegroup = true;
                $scope.fillDivisionSetupForms($scope.divisionsetup.DIVISION_CODE);
               
                $.when(divisiondataFillDefered).then(function () {


                    $scope.clearFields();
                    var tree = $("#masterdivisioncode").data("kendoDropDownList");
                    tree.value($scope.divisionArr.DIVISION_CODE);


                    $scope.divisionsetup.GROUP_SKU_FLAG = "G";
                    $scope.divisionArr.GROUP_SKU_FLAG = "G";
                    $scope.divisionArr.DIVISION_EDESC = "";
                    $('#divisionname').val("");
                    $scope.clearFields();
                    $("#divisionSetupModal").modal();
                });

            }
        }
    }
  
    //for Under Group 
    //$scope.divisionGroupDataSource = [
    //    { text: "<PRIMARY>", value: "" }
    //];
   
    var divisionCodeUrl = window.location.protocol + "//" + window.location.host + "/api/TemplateApi/getDivisionCodeWithChild";

    $scope.divisionGroupDataSource = new kendo.data.DataSource({
        transport: {
            read: {
                url: divisionCodeUrl,
            }
        }
    });

    $scope.divisionGroupOptions = {
        dataSource: $scope.divisionGroupDataSource,
        optionLabel: "<PRIMARY>",
        dataTextField: "DIVISION_EDESC",
        dataValueField: "DIVISION_CODE",
       
        change: function (e) {

            var currentItem = e.sender.dataItem(e.node);
            
            if (currentItem.DIVISION_CODE != "") {
                $scope.fillDivisionSetupForms(currentItem.DIVISION_CODE);
            } else {
                
            }
            var natureTree = $("#masterdivisioncode").data("kendoDropDownList");
            
        },
        dataBound: function () {


            $scope.divisionGroupDataSource;
        }
    }


    $scope.movescrollbar = function () {
        var element = $(".k-in");
        for (var i = 0; i < element.length; i++) {
            var selectnode = $(element[i]).hasClass("k-state-focused");
            if (selectnode) {
                $("#divisionRoottree").animate({
                    scrollTop: (parseInt(i))
                });
                break;
            }
        }
    }

    //
    $scope.fillDivisionSetupForms = function (divisionId) {
        var getResourcedetaisByUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/getDivisionDetailsByDivisionCode?divisionCode=" + divisionId;
        $http({
            method: 'GET',
            url: getResourcedetaisByUrl,

        }).then(function successCallback(response) {

            
            $scope.divisionsetup = response.data.DATA;
            $scope.divisionArr = response.data.DATA;
            $scope.divisionArr.DIVISION_CODE = $scope.divisionsetup.DIVISION_CODE;
            $scope.divisionArr.MASTER_DIVISION_CODE = $scope.divisionsetup.DIVISION_CODE;
            //To open the context menu and bind data in modal
            divisiondataFillDefered.resolve(response);


        }, function errorCallback(response) {

        });
    }
    //Add for Tree Root
    $scope.addnewaccount = function () {
        $scope.editFlag = "N";
        $scope.saveupdatebtn = "Save"
        $scope.masterdivisiondisabled = false;
        $scope.clearFields();
        $scope.groupschilddisabled = true;
        $scope.groupDivisionTypeFlag = "Y";
        $scope.divisionsetup.GROUP_SKU_FLAG = "G";
        $scope.divisionArr.GROUP_SKU_FLAG = "G";
        //var tree = $("#masterdivisioncode").data("kendoDropDownList");
       
        //tree.value("");
        $('#divisionRoottree').data("kendoTreeView").dataSource.read();
        $("#divisionSetupModal").modal("toggle");
    }
   

    $scope.showModalForNew = function (event) {

        debugger;
        $scope.saveupdatebtn = "Save"
        $scope.editFlag = "N";

        var returnMaxCustomerUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/getDivisionDetailsByDivisionCode?divisionCode=" + $scope.treeSelectedDivisionCode;
        $http({
            method: 'GET',
            url: returnMaxCustomerUrl,
        }).then(function successCallback(response) {
            $scope.groupschilddisabled = true;
            $scope.divisionsetup = response.data.DATA;
            $scope.divisionArr = response.data.DATA;
            $scope.divisionArr.DIVISION_CODE = $scope.divisionsetup.DIVISION_CODE;
            $scope.divisionArr.MASTER_DIVISION_CODE = $scope.divisionsetup.DIVISION_CODE;
            $scope.divisionArr.DIVISION_EDESC = "";
            var tree = $("#masterdivisioncode").data("kendoDropDownList");
            tree.value($scope.divisionArr.DIVISION_CODE);
            $scope.divisionsetup.GROUP_SKU_FLAG = "I";
            $scope.divisionArr.GROUP_SKU_FLAG = "I";
            $scope.clearFields();
            $("#divisionSetupModal").modal();


        }, function errorCallback(response) {
            displayPopupNotification(response.data.STATUS_CODE, "error");

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

   
    //Clear Fields
    $scope.clearFields = function () {

        $scope.divisionArr.DIVISION_EDESC = "";
        $scope.divisionArr.REMARKS = "";
        $scope.divisionArr.ADDRESS = "";
        $scope.divisionArr.TELEPHONE_NO = " ";
        $scope.divisionArr.EMAIL = " ";

       
    }

    //Edit Function
    $scope.edit = function (divisionCode) {

        $scope.editFlag = "Y";
        $scope.saveupdatebtn = "Update";
        divisiondataFillDefered = $.Deferred();
        $scope.fillDivisionSetupForms(divisionCode);
        $.when(divisiondataFillDefered).done(function () {
            $scope.groupDivisionTypeFlag = "N";
            $("#divisionSetupModal").modal();

        });


    }



    //Delete function for child
    $scope.delete = function (code) {
        debugger
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

                    $scope.divisionsetup.GROUP_SKU_FLAG = "I";

                    var delUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/DeleteDivisionSetupByDivisionCode?divisionCode=" + code;
                    $http({
                        method: 'POST',
                        url: delUrl
                    }).then(function successCallback(response) {



                        if (response.data.MESSAGE == "DELETED") {
                            debugger;
                            // $scope.budgetcenterArr = [];
                            if ($scope.divisionsetup.GROUP_SKU_FLAG !== "I") {
                                var tree = $("#divisionRoottree").data("kendoTreeView");
                                if (tree != undefined) {
                                    tree.dataSource.read();
                                }
                            }
                            $("#divisionSetupModal").modal("hide");
                            
                            var grid = $('#kGrid').data("kendoGrid");
                            grid.dataSource.read();
                            bootbox.hideAll();
                             $scope.treenodeselected = "N";
                            $scope.refresh();
                            $("#divisionRoottree").data("kendoTreeView").dataSource.read();

                            $("#kGrid").data("kendoGrid").dataSource.read();
                            displayPopupNotification("Data succesfully deleted ", "success");
                        }
                        if (response.data.MESSAGE == "HAS_CHILD") {

                            
                            displayPopupNotification("Please delete the respective child first", "warning");
                        }
                       
                    }, function errorCallback(response) {
                        $scope.refresh();
                        displayPopupNotification(response.data.STATUS_CODE, "error");
                        
                    });

                }
                else if (result == false) {

                    $scope.refresh();
                    $("#divisionSetupModal").modal("hide");
                    bootbox.hideAll();
                }

            }
        });
    }


    //Save and Update Function
    $scope.saveNewDivision = function (isValid) {
        debugger;
        if (!isValid) {
            displayPopupNotification("Input fields are not valid. Please review and try again", "warning");
            return;
        }
        debugger;
      
        if ($scope.saveupdatebtn == "Save") {
        
            var predivisioncode = $("#masterdivisioncode").data("kendoDropDownList").value();

            var createurl = window.location.protocol + "//" + window.location.host + "/api/setupapi/createNewDivisionHead";
            var model = {
                DIVISION_CODE: $scope.divisionArr.DIVISION_CODE,
                DIVISION_EDESC: $scope.divisionArr.DIVISION_EDESC,
                ADDRESS: $scope.divisionArr.ADDRESS,
                TELEPHONE_NO: $scope.divisionArr.TELEPHONE_NO,
                EMAIL: $scope.divisionArr.EMAIL,
                MASTER_DIVISION_CODE: $scope.divisionArr.MASTER_DIVISION_CODE,
                GROUP_SKU_FLAG: $scope.divisionArr.GROUP_SKU_FLAG,
                PRE_DIVISION_CODE: $scope.divisionArr.DIVISION_CODE,
                REMARKS: $scope.divisionArr.REMARKS,


            }
            $http({
                method: 'post',
                url: createurl,
                data: model
            }).then(function successcallback(response) {

                if (response.data.MESSAGE == "INSERTED") {

                    $scope.divisionArr = [];
                    $scope.divisionArr.DIVISION_EDESC = "";
                    if ($scope.divisionsetup.GROUP_SKU_FLAG !== "I") {

                        $("#divisionRoottree").data("kendoTreeView").dataSource.read();
                       
                    }
                    if ($scope.savegroup == false) { $("#divisionSetupModal").modal("toggle"); }
                    else { $("#divisionSetupModal").modal("toggle"); }

                    var grid = $("#kGrid").data("kendoGrid");
                    if (grid != undefined) {

                        grid.dataSource.read();
                    }
                    
                    $("#masterdivisioncode").data("kendoDropDownList").dataSource.read();
                   
                    displayPopupNotification("data succesfully saved ", "success");
                    debugger;
                    $scope.treenodeselected = "N";
                    $scope.refresh();
                }
                else {
                    displayPopupNotification("Something went wrong.Please try again later.", "error");
                }
               
            }, function errorcallback(response) {
                displayPopupNotification("Something went wrong.Please try again later.", "error");
               
            });
        }
        else {

            var updateurl = window.location.protocol + "//" + window.location.host + "/api/setupapi/updateDivisionCode";
            
            $scope.saveupdatebtn = "Update";
            var model = {
                DIVISION_CODE: $scope.divisionArr.DIVISION_CODE,
                DIVISION_EDESC: $scope.divisionArr.DIVISION_EDESC,
                ADDRESS: $scope.divisionArr.ADDRESS,
                TELEPHONE_NO: $scope.divisionArr.TELEPHONE_NO,
                EMAIL: $scope.divisionArr.EMAIL,
                MASTER_DIVISION_CODE: $scope.divisionArr.MASTER_DIVISION_CODE,
                GROUP_SKU_FLAG: $scope.divisionArr.GROUP_SKU_FLAG,
                PRE_DIVISION_CODE: $scope.divisionArr.DIVISION_CODE,
                REMARKS: $scope.divisionArr.REMARKS,


            }
            $http({
                method: 'post',
                url: updateurl,
                data: model
            }).then(function successcallback(response) {


                if (response.data.MESSAGE == "UPDATED") {

                    $scope.divisionArr = [];
                    if ($scope.divisionsetup.GROUP_SKU_FLAG !== "I") {
                        $("#divisionRoottree").data("kendoTreeView").dataSource.read();
                       

                    }


                    

                    var grid = $("#kGrid").data("kendoGrid");

                    if (grid != undefined) {
                        grid.dataSource.read();
                    }
                    $("#masterdivisioncode").data("kendoDropDownList").dataSource.read();
                    $("#divisionSetupModal").modal("toggle");
                    $scope.refresh();
                    displayPopupNotification("data succesfully updated ", "success");
                    $scope.treenodeselected = "N";
                }
                if (response.data.MESSAGE == "error") {
                    displayPopupNotification("Something went wrong.Please try again later.", "error");
                }
               
            }, function errorcallback(response) {

                displayPopupNotification("Something went wrong.Please try again later.", "error");
               
            });




        }
        $scope.divisiontreeData();
        $scope.refresh();
    }


    $scope.SubLedger_Cancel = function () {

        $scope.refresh();
    }

  

    $scope.refresh = function () {

        $("#divisionRoottree").data("kendoTreeView").dataSource.read();
     
      
        
    }
});