

DTModule.controller('resourceSetupTreeCtrl', function ($scope, $rootScope, $http, $routeParams, $window, $filter) {
    $scope.saveupdatebtn = "Save";
    $scope.resourcesArr;
    $scope.savegroup = false;
    $scope.editFlag = "N";
    $scope.newrootinsertFlag = "Y";
    $scope.treenodeselected = "N";
    $scope.groupchilddisabled = true;
    $scope.masterresourcedisabled = false;

    $scope.treeselectedresourceCode = "";
    //$scope.groupResourceTypeFlag = "Y";
    $scope.resourcesetup =
        {
            RESOURCE_TYPE: "",
            RESOURCE_CODE: "",
            RESOURCE_EDESC: "",
            RESOURCE_NDESC: "",
            RESOURCE_FLAG: "G",
            GROUP_SKU_FLAG: "",
            PRE_RESOURCE_CODE: "",
            REMARKS: "",
            PARENT_RESOURCE_CODE: "",
        }


    $scope.resourcesArr = $scope.resourcesetup;
    var resourcedataFillDefered = $.Deferred();
    var resourceCodeUrl = window.location.protocol + "//" + window.location.host + "/api/TemplateApi/getResourceCodeWithChild";

    $scope.resourceGroupDataSource = new kendo.data.DataSource({
        transport: {
            read: {
                url: resourceCodeUrl,
            }
        }
    });

    $scope.resourceGroupOptions = {
        dataSource: $scope.resourceGroupDataSource,
        optionLabel: "<PRIMARY>",
        dataTextField: "RESOURCE_EDESC",
        dataValueField: "RESOURCE_CODE",
        change: function (e) {

            var currentItem = e.sender.dataItem(e.node);
        },
        dataBound: function () {

        }
    }

    var getResourceCodeByUrl = window.location.protocol + "//" + window.location.host + "/api/TemplateApi/GetResource";
    $scope.resourcetreeData = new kendo.data.HierarchicalDataSource({

        transport: {
            read: {
                url: getResourceCodeByUrl,
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
                id: "RESOURCE_CODE",
                parentId: "PRE_RESOURCE_CODE",
                children: "Items",
                fields: {
                    RESOURCE_CODE: { field: "RESOURCE_CODE", type: "string" },
                    RESOURCE_EDESC: { field: "RESOURCE_EDESC", type: "string" },
                    parentId: { field: "PRE_RESOURCE_CODE", type: "string", defaultValue: "00" },
                }
            }
        }
    });

    //Grid Binding main Part

    $scope.resourceOnDataBound = function () {
        $('#resourcetree').data("kendoTreeView").expand('.k-item');
    }

    $scope.fillResourceSetupForms = function (resourceId) {
        var getResourcedetaisByUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/getResourceDetailsByResourceCode?resourceCode=" + resourceId;
        $http({
            method: 'GET',
            url: getResourcedetaisByUrl,

        }).then(function successCallback(response) {

            if (response.data.DATA != null) {
                $scope.resourcesetup = response.data.DATA;
                $scope.resourcesArr = response.data.DATA;
                resourcedataFillDefered.resolve(response);
            }

        }, function errorCallback(response) {

        });
    }
    //treeview on select
    $scope.resourceOptions = {
        loadOnDemand: false,
        select: function (e) {

            var currentItem = e.sender.dataItem(e.node);
            $('#resourceGrid').removeClass("show-displaygrid");
            $("#resourceGrid").html("");
            $($(this._current).parents('ul')[$(this._current).parents('ul').length - 1]).find('span').removeClass('hasTreeCustomMenu');
            $(this._current.context).find('span').addClass('hasTreeCustomMenu');
            $scope.resourcesetup.RESOURCE_CODE = currentItem.ResourceId;
            $scope.resourcesArr.RESOURCE_CODE = currentItem.ResourceId;
            $scope.treeselectedresourceCode = currentItem.ResourceId;
            $scope.resourcesetup.GROUP_SKU_FLAG = currentItem.groupSkuFlag;
            $scope.resourcesArr.GROUP_SKU_FLAG = currentItem.groupSkuFlag;
            //$scope.resourcesetup.RESOURCE_EDESC = currentItem.ResourceName;
            $scope.resourcesetup.MASTER_RESOURCE_CODE = currentItem.masterResourceCode;
            $scope.treenodeselected = "Y";
            $scope.newrootinsertFlag = "N";
            $scope.$apply();
            //$scope.movescrollbar();
        },

    };

    $scope.movescrollbar = function () {
        var element = $(".k-in");
        for (var i = 0; i < element.length; i++) {
            var selectnode = $(element[i]).hasClass("k-state-focused");
            if (selectnode) {
                $("#resourcetree").animate({
                    scrollTop: (parseInt(i * 2))
                });
                break;
            }
        }
    }
    $scope.onContextSelect = function (event) {

        if ($scope.resourcesetup.RESOURCE_CODE == "")
            return displayPopupNotification("Select resource.", "error");;
        $scope.saveupdatebtn = "Save";
        if (event.item.innerText.trim() == "Delete") {

            $scope.delete($scope.resourcesArr.RESOURCE_CODE, "");
        }
        else if (event.item.innerText.trim() == "Update") {

            resourcedataFillDefered = $.Deferred();
            $scope.saveupdatebtn = "Update";
            $scope.fillResourceSetupForms($scope.resourcesetup.RESOURCE_CODE);
            $.when(resourcedataFillDefered).then(function () {

                $scope.masterresourcedisabled = true;
                if ($scope.resourcesetup.PARENT_RESOURCE_CODE == null || $scope.resourcesetup.PARENT_RESOURCE_CODE == undefined) {
                    var popUpDropdown = $("#masterresourcecode").data("kendoDropDownList");
                    popUpDropdown.value('');
                }
                else {
                    var popUpDropdown = $("#masterresourcecode").data("kendoDropDownList");
                    popUpDropdown.value($scope.resourcesetup.PARENT_RESOURCE_CODE);
                }
                $("#resourceModal").modal();
            });

            //var tree = $("#masterresourcecode").data("kendoDropDownList");
            //tree.value($scope.treeselectedresourceCode);


        }
        else if (event.item.innerText.trim() == "Add") {
            resourcedataFillDefered = $.Deferred();
            $scope.savegroup = true;
            $scope.resourcesArr = [];
            $scope.fillResourceSetupForms($scope.resourcesetup.RESOURCE_CODE);
            $.when(resourcedataFillDefered).then(function () {

                var tree = $("#masterresourcecode").data("kendoDropDownList");
                tree.value($scope.treeselectedresourceCode);
                $scope.resourcesArr.RESOURCE_CODE = $scope.resourcesetup.RESOURCE_CODE;
                $scope.resourcesArr.GROUP_SKU_FLAG = "G";
                $scope.ClearFields();
                $("#resourceModal").modal();

            });

        }
    }
    $scope.saveNewResource = function (isValid) {
        debugger;
        var preresourcecode = $("#masterresourcecode").data("kendoDropDownList").value();
        var model = {
            RESOURCE_CODE: $scope.resourcesArr.RESOURCE_CODE,
            RESOURCE_EDESC: $scope.resourcesArr.RESOURCE_EDESC,
            RESOURCE_NDESC: $scope.resourcesArr.RESOURCE_NDESC,
            PRE_RESOURCE_CODE: preresourcecode,
            GROUP_SKU_FLAG: $scope.resourcesArr.GROUP_SKU_FLAG,
            RESOURCE_TYPE: $scope.resourcesArr.RESOURCE_TYPE,
            REMARKS: $scope.resourcesArr.REMARKS,
        }
        if (!isValid) {
            displayPopupNotification("Input fields are not valid. Please review and try again", "warning");
            return;
        }
        if (model.GROUP_SKU_FLAG == "I" && preresourcecode == "") {
            return displayPopupNotification("Plese Select", "error");
        }
        if ($scope.saveupdatebtn == "Save") {
            var createUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/createNewResourceHead";
            $http({
                method: 'POST',
                url: createUrl,
                data: model
            }).then(function successCallback(response) {

                if (response.data.MESSAGE == "INSERTED") {
                    $scope.resourcesArr = [];
                    if ($scope.resourcesetup.GROUP_SKU_FLAG !== "I") {
                        var tree = $("#resourcetree").data("kendoTreeView");
                        tree.dataSource.read();
                    }
                    if ($scope.savegroup == false) { $("#resourceModal").modal("toggle"); }
                    else { $("#resourceModal").modal("toggle"); }
                    //$("#kGrid").data("kendoGrid").dataSource.read();
                    var grid = $("#kGrid").data("kendo-grid");
                    if (grid != undefined) {
                        grid.dataSource.read();
                    }
                    $("#masterresourcecode").data("kendoDropDownList").dataSource.read();
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
            var updateUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/updateResourceByResourceCode";

            $scope.saveupdatebtn = "Update";
            $http({
                method: 'POST',
                url: updateUrl,
                data: model
            }).then(function successCallback(response) {

                if (response.data.MESSAGE == "UPDATED") {
                    $scope.resourcesArr = [];
                    if ($scope.resourcesetup.GROUP_SKU_FLAG !== "I") {
                        var tree = $("#resourcetree").data("kendoTreeView");
                        tree.dataSource.read();
                    }
                    //$("#kGrid").data("kendoGrid").dataSource.read();
                    var grid = $("#kGrid").data("kendo-grid");
                    if (grid != undefined) {
                        grid.dataSource.read();
                    }
                    $("#masterresourcecode").data("kendoDropDownList").dataSource.read();
                    $("#resourceModal").modal("toggle");
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
        var url = null;
        if (groupId == "All") {
            if ($('#restxtSearchString').val() == null || $('#restxtSearchString').val() == '' || $('#restxtSearchString').val() == undefined || $('#restxtSearchString').val() == 'undefined') {
                alert('Input is empty or undefined.');
                return;
            }
            url = "/api/SetupApi/GetAllResourceList?searchtext=" + $('#restxtSearchString').val();
        }
        else {
            $("#restxtSearchString").val('');
            url = "/api/SetupApi/GetChildOfResourceByGroup?groupId=" + groupId;
        }
        $scope.resourceChildGridOptions = {
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
               
                $("#kGrid tbody tr").css("cursor", "pointer");
                DisplayNoResultsFound($('#kGrid'));
                $("#kGrid tbody tr").on('dblclick', function () {
                    var resourceCode = $(this).find('td span').html()
                    $scope.edit(resourceCode);
                    //var tree = $("#resourcetree").data("kendoTreeView");
                    //tree.dataSource.read();
                })
            },
            columns: [
                //{
                //    //hidden: true,
                //    title: "Resource ID",
                //    field: "RESOURCE_CODE",
                //    width: "80px"

                //},
                {
                    field: "RESOURCE_EDESC",
                    title: "Resource Name",
                    width: "120px"
                },
                
                {
                    field: "REMARKS",
                    title: "Remarks",
                    width: "120px"
                },
                {
                    field: "CREATED_BY",
                    title: "CREATED BY",
                    width: "120px"
                }, {
                    field: "CREATED_DATE",
                    title: "CREATED DATE",
                    template: "#= kendo.toString(kendo.parseDate(CREATED_DATE),'dd MMM yyyy') #",
                    width: "120px"
                },
              
                {
                    title: "Action ",
                    template: '<a class="fa fa-pencil-square-o editAction" title="Edit" ng-click="edit(dataItem.RESOURCE_CODE)"><span class="sr-only"></span> </a><a class="fa fa-trash deleteAction" title="Delete" ng-click="delete(dataItem.RESOURCE_CODE,dataItem.GROUP_SKU_FLAG)"><span class="sr-only"></span> </a>',
                    width: "60px"
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

    $scope.showModalForNew = function (event) {

        resourcedataFillDefered = $.Deferred();
        $scope.editFlag = "N";
        $scope.fillResourceSetupForms($scope.treeselectedresourceCode);
        $.when(resourcedataFillDefered).then(function () {
            $scope.ClearFields();
            $scope.groupResourceTypeFlag = "N";
            $scope.saveupdatebtn = "Save"
            $scope.masterresourcedisabled = false;
            $scope.resourcesetup.GROUP_SKU_FLAG = "I";
            $scope.resourcesArr.GROUP_SKU_FLAG = "I";
            var tree = $("#masterresourcecode").data("kendoDropDownList");
            if (tree != null && tree != undefined) {
                tree.value($scope.treeselectedresourceCode);
            }
            $("#resourceModal").modal("toggle");
        }, function errorCallback(response) {
            displayPopupNotification(response.data.RESOURCE_CODE, "error");

        });

    }
    $scope.edit = function (resourceCode) {
        $scope.editFlag = "Y";
        $scope.saveupdatebtn = "Update"
        $scope.masterresourcedisabled = true;
        $scope.fillResourceSetupForms(resourceCode);
        $scope.groupResourceTypeFlag = "N";
        $scope.resourcesetup.GROUP_SKU_FLAG = "I";
        $scope.resourcesArr.GROUP_SKU_FLAG = "I";
        var tree = $("#masterresourcecode").data("kendoDropDownList");
        tree.value($scope.treeselectedresourceCode);
        $("#resourceModal").modal();
    }
    $scope.delete = function (code, skf) {
  
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
                  
                    if (skf === "I")
                        $scope.resourcesetup.GROUP_SKU_FLAG = "I";
                    else
                        $scope.resourcesetup.GROUP_SKU_FLAG = "G";

                    var delUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/DeleteResourceSetupByResourceCode?resourceCode=" + code;
                    $http({
                        method: 'POST',
                        url: delUrl
                    }).then(function successCallback(response) {
                    
                        if (response.data.MESSAGE == "DELETED") {
                            if ($scope.resourcesetup.GROUP_SKU_FLAG !== "I") {
                                var tree = $("#resourcetree").data("kendoTreeView");
                                tree.dataSource.read();
                            }
                            $("#kGrid").data("kendoGrid").dataSource.read();
                            displayPopupNotification("Data succesfully deleted ", "success");
                        }
                        else if (response.data.MESSAGE == "HAS_CHILD") {
                            displayPopupNotification("You can not delete. It has child.", "warning");
                        }
                    }, function errorCallback(response) {
                        displayPopupNotification(response.data.STATUS_CODE, "error");
                    });

                }

            }
        });
    }
    $scope.addNewResource = function () {
        $scope.editFlag = "N";
        $scope.saveupdatebtn = "Save"
        $scope.masterresourcedisabled = false;
        $scope.groupResourceTypeFlag = "Y";
        $scope.resourcesetup.GROUP_SKU_FLAG = "G";
        $scope.resourcesArr.GROUP_SKU_FLAG = "G";
        var tree = $("#masterresourcecode").data("kendoDropDownList");
        $scope.ClearFields();
        tree.value("");
        $('#resourcetree').data("kendoTreeView").dataSource.read();
        $("#resourceModal").modal("toggle");
    }
    $scope.Cancel = function () {
        $scope.saveupdatebtn = "Save";
        $scope.resourcesArr = [];
        //$("#kGrid").data("kendoGrid").clearSelection();
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

    $scope.ClearFields = function () {
        $scope.resourcesArr.RESOURCE_EDESC = "";
        $scope.resourcesArr.RESOURCE_NDESC = "";
        $scope.resourcesArr.RESOURCE_TYPE = "";
        $scope.resourcesArr.REMARKS = "";

    }

});

