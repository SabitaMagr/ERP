

DTModule.controller('regionalSetupTreeCtrl', function ($scope, $rootScope, $http, $routeParams, $window, $filter) {
    $scope.saveupdatebtn = "Save";
    $scope.regionalsArr;
    $scope.savegroup = false;
    $scope.groupschilddisabled = true;
    $scope.editFlag = "N";
    $scope.newrootinsertFlag = "Y";
    $scope.treenodeselected = "N";
    $scope.regionalsetup =
        {
            REGION_CODE: "",
            REGION_EDESC: "",
            REGION_NDESC: "",
            REMARKS: "",
            PRE_REGION_CODE: "",
            GROUP_SKU_FLAG: "G",
            PARENT_REGION_CODE: "",
        }
    $scope.treeselectedRegionCode = "";
    $scope.regionalsArr = $scope.regionalsetup;
    $scope.masterRegionalCodeDataSource = [
        { text: "<PRIMARY>", value: "" }
    ];
    $scope.treeselectedregionalCode = "";
    var regionaldataFillDefered = $.Deferred();

    var regionalCodeUrl = window.location.protocol + "//" + window.location.host + "/api/TemplateApi/GetRegional";

    $scope.regionalGroupDataSource = new kendo.data.DataSource({
        transport: {
            read: {
                url: regionalCodeUrl,
            }
        }
    });
    $scope.MACDS = [];

    $scope.regionalGroupOptions = {
        dataSource: $scope.regionalGroupDataSource,
        optionLabel: "<PRIMARY>",
        dataTextField: "REGION_EDESC",
        dataValueField: "REGION_CODE",
        change: function (e) {

            var currentItem = e.sender.dataItem(e.node);

        },
        dataBound: function (e) {

        }
    }

  
    var gettoregionalsByUrl = window.location.protocol + "//" + window.location.host + "/api/TemplateApi/GetTreeRegional";
    $scope.regionaltreeData = new kendo.data.HierarchicalDataSource({
        transport: {
            read: {
                url: gettoregionalsByUrl,
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
                id: "REGION_CODE",
                parentId: "PRE_REGION_CODE",
                children: "Items",
                fields: {
                    REGION_CODE: { field: "REGION_CODE", type: "string" },
                    REGION_EDESC: { field: "REGION_EDESC", type: "string" },
                    parentId: { field: "PRE_REGION_CODE", type: "string", defaultValue: "00" },
                }
            }
        }
    });

    //Grid Binding main Part

    $scope.regionalOnDataBound = function (e) {

        $('#regionaltree').data("kendoTreeView").expand('.k-item');
    }
    $scope.getDetailByRegionalCode = function (regionalId) {
        var getRegionaldetaisByUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/getRegionalDetailsByRegionalCode?regionalCode=" + regionalId;
        $http({
            method: 'GET',
            url: getRegionaldetaisByUrl,

        }).then(function successCallback(response) {
            var regionalNature = response.data.DATA.REGION_NATURE;
            $scope.bindNatureByRegionalNature(regionalNature);

        }, function errorCallback(response) {

        });
    }

    $scope.fillRegionalSetupForms = function (regionalId) {

        var getRegionaldetaisByUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/getRegionalDetailsByRegionalCode?regionCode=" + regionalId;
        $http({
            method: 'GET',
            url: getRegionaldetaisByUrl,

        }).then(function successCallback(response) {

            if (response.data.DATA != null) {
                if (response.data.DATA.GROUP_SKU_FLAG == "I") {
                    var dropdownlist = $("#masterregionalcode").data("kendoDropDownList");
                    dropdownlist.value($scope.treeselectedRegionCode);
                }

                $scope.regionalsetup = response.data.DATA;
                $scope.regionalsArr = response.data.DATA;



                regionaldataFillDefered.resolve(response);

            }


            // this callback will be called asynchronously
            // when the response is available
        }, function errorCallback(response) {

            // called asynchronously if an error occurs
            // or server returns response with an error status.
        });
    }
    //treeview on select
    $scope.regionaloptions = {
        loadOnDemand: false,
        select: function (e) {
            var currentItem = e.sender.dataItem(e.node);
            $('#regionalGrid').removeClass("show-displaygrid");
            $("#regionalGrid").html("");
            $($(this._current).parents('ul')[$(this._current).parents('ul').length - 1]).find('span').removeClass('hasTreeCustomMenu');
            $(this._current.context).find('span').addClass('hasTreeCustomMenu');
            $scope.regionalsetup.REGION_CODE = currentItem.RegionId;
            $scope.regionalsArr.REGION_CODE = currentItem.RegionId;
            $scope.regionalsetup.REGION_EDESC = currentItem.RegionName;
            $scope.treenodeselected = "Y";
            $scope.treeselectedRegionCode = currentItem.RegionId;
            $scope.newrootinsertFlag = "N";
            $scope.groupschilddisabled = true;
            $scope.$apply();
            //$scope.movescrollbar();

        },

    };

    $scope.movescrollbar = function () {
        var element = $(".k-in");
        for (var i = 0; i < element.length; i++) {
            var selectnode = $(element[i]).hasClass("k-state-focused");
            if (selectnode) {
                $("#regionaltree").animate({
                    scrollTop: (parseInt(i * 2))
                });
                break;
            }
        }
    }
    $scope.onContextSelect = function (event) {
        if ($scope.regionalsetup.REGION_CODE == "")
            return displayPopupNotification("Select regional.", "error");;
        $scope.saveupdatebtn = "Save";
        if (event.item.innerText.trim() == "Delete") {
            $scope.delete($scope.regionalsArr.REGION_CODE,"");

        }
        else if (event.item.innerText.trim() == "Update") {
            regionaldataFillDefered = $.Deferred();
            $scope.saveupdatebtn = "Update";
            //var dropdownlist = $("#masterregionalcode").data("kendoDropDownList");
            //dropdownlist.value($scope.treeselectedRegionCode);
            $scope.fillRegionalSetupForms($scope.regionalsetup.REGION_CODE);
            $.when(regionaldataFillDefered).then(function () {

                if ($scope.regionalsetup.PARENT_REGION_CODE == null || $scope.regionalsetup.PARENT_REGION_CODE == undefined) {
                    var popUpDropdown = $("#masterregionalcode").data("kendoDropDownList");
                    popUpDropdown.value('');
                }
                else {
                    var popUpDropdown = $("#masterregionalcode").data("kendoDropDownList");
                    popUpDropdown.value($scope.regionalsetup.PARENT_REGION_CODE);
                }
                $("#regionalModal").modal();
            })
        }
        else if (event.item.innerText.trim() == "Add") {
            regionaldataFillDefered = $.Deferred();
            $scope.savegroup = true;
            $scope.regionalsArr = [];
            $scope.fillRegionalSetupForms($scope.regionalsetup.REGION_CODE);
            $.when(regionaldataFillDefered).then(function () {
                $scope.Cleardata();
                var dropdownlist = $("#masterregionalcode").data("kendoDropDownList");
                dropdownlist.value($scope.treeselectedRegionCode);
                $scope.regionalsetup.GROUP_SKU_FLAG = "G";
                $scope.regionalsArr.GROUP_SKU_FLAG = "G";
                //$scope.regionalsArr.REGION_CODE = $scope.regionalsetup.REGION_CODE;
                //$scope.regionalsArr.REGION_TYPE_FLAG = "N";
                $("#regionalModal").modal();

            });


        }
    }
    $scope.saveNewRegional = function (isValid) {
        debugger;
        if (isValid == false) {
            displayPopupNotification("Please Select Reginal English Name", "warning");
            return;
        }


        if ($scope.saveupdatebtn == "Save") {

            var createUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/createNewRegionalHead";
            var model = {
                //model: $scope.regionalsetup
                REGION_CODE: $scope.regionalsArr.REGION_CODE,
                REGION_EDESC: $scope.regionalsArr.REGION_EDESC,
                REGION_NDESC: $scope.regionalsArr.REGION_NDESC,
                REMARKS: $scope.regionalsArr.REMARKS,
                GROUP_SKU_FLAG: $scope.regionalsArr.GROUP_SKU_FLAG,

            }
            $http({
                method: 'POST',
                url: createUrl,
                data: model
            }).then(function successCallback(response) {
                debugger;
                if (response.data.MESSAGE == "INSERTED") {
                    $scope.regionalsArr = [];

                    if ($scope.regionalsetup.GROUP_SKU_FLAG !== "I") {
                        var tree = $("#regionaltree").data("kendoTreeView");
                        tree.dataSource.read();
                    }

                    var grid = $('#kGrid').data("kendoGrid");
                    if (grid != undefined) {
                        grid.dataSource.read();
                    }
                    //$scope.Cleardata();
                    if ($scope.savegroup == true) { $("#regionalModal").modal("toggle"); }
                    else { $("#regionalModal").modal("toggle"); }
                    $("#masterregionalcode").data("kendoDropDownList").dataSource.read();
                    $scope.Cleardata();
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
            $scope.Cleardata();


        }
        else {
            var updateUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/updateRegionalByRegionalCode";
            var model = {
                REGION_CODE: $scope.regionalsArr.REGION_CODE,
                REGION_EDESC: $scope.regionalsArr.REGION_EDESC,
                REGION_NDESC: $scope.regionalsArr.REGION_NDESC,
                PRE_REGION_CODE: $scope.regionalsArr.PRE_REGION_CODE,
                REGION_TYPE_FLAG: $scope.regionalsArr.REGION_TYPE_FLAG,
                REMARKS: $scope.regionalsArr.REMARKS,
                GROUP_SKU_FLAG: $scope.regionalsArr.GROUP_SKU_FLAG
            }
            $scope.saveupdatebtn = "Update";
            $http({
                method: 'POST',
                url: updateUrl,
                data: model
            }).then(function successCallback(response) {

                if (response.data.MESSAGE == "UPDATED") {
                    $scope.regionalsArr = [];
                    if ($scope.regionalsetup.GROUP_SKU_FLAG !== "I") {
                        var tree = $("#regionaltree").data("kendoTreeView");
                        tree.dataSource.read();
                    }

                    $scope.Cleardata();
                    var dropdownlist = $("#masterregionalcode").data("kendoDropDownList");
                    dropdownlist.dataSource.read();
                    $("#kGrid").data("kendoGrid").dataSource.read();
                    $("#regionalModal").modal("toggle");
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


    $scope.MACDSOptions = {
        dataSource: $scope.MACDS,
        dataTextField: "text",
        dataValueField: "value",

    };

    $scope.masterRegionalCodeOptions = {
        dataSource: $scope.masterRegionalCodeDataSource,
        dataTextField: "text",
        dataValueField: "value",
    };


    $scope.BindGrid = function (groupId) {
        $(".topsearch").show();
        var url = null;
        if (groupId == "All") {
            if ($('#regtxtSearchString').val() == null || $('#regtxtSearchString').val() == '' || $('#regtxtSearchString').val() == undefined || $('#regtxtSearchString').val() == 'undefined') {
                alert('Input is empty or undefined.');
                return;
            }
            url = "/api/SetupApi/GetAllRegionalList?searchtext=" + $('#regtxtSearchString').val();
        }
        else {
            $("#regtxtSearchString").val('');
            url = "/api/SetupApi/GetChildOfRegionalByGroup?groupId=" + groupId;
        }
        $scope.regionalChildGridOptions = {

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
            dataBound: function (e) {
                $("#kGrid tbody tr").css("cursor", "pointer");
                DisplayNoResultsFound($('#kGrid'));
                $("#kGrid tbody tr").on('dblclick', function () {
                    var regionalCode = $(this).find('td span').html()
                    $scope.edit(regionalCode);

                })
            },
            columns: [
                {
                    //hidden: true,
                    title: "Code",
                    field: "REGION_CODE",
                    width: "80px"

                },
                {
                    field: "REGION_EDESC",
                    title: "Regional Name",
                    width: "120px"
                },
                {
                    field: "REMARKS",
                    title: "Remarks",
                    width: "120px"
                },
             
                {
                    field: "CREATED_BY",
                    title: "Created By",
                    width: "120px"
                },
              
                {

                    title: "Action",
                    template: '<a class="fa fa-pencil-square-o editAction" title="Edit" ng-click="edit(dataItem.REGION_CODE)"><span class="sr-only"></span> </a><a class="fa fa-trash deleteAction" title="Delete" ng-click="delete(dataItem.REGION_CODE,dataItem.GROUP_SKU_FLAG)"><span class="sr-only"></span> </a>',
                    width: "70px"
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
        //if ($scope.regionalsetup.REGION_CODE != null && $scope.regionalsetup.REGION_CODE != undefined && $scope.regionalsetup.REGION_CODE != "" && $scope.regionalsetup.GROUP_SKU_FLAG != "I") {
        regionaldataFillDefered = $.Deferred();
        $scope.editFlag = "N";
        $scope.saveupdatebtn = "Save";
        $scope.fillRegionalSetupForms($scope.treeselectedRegionCode);
        $.when(regionaldataFillDefered).then(function () {
            $scope.Cleardata();
            var dropdownlist = $("#masterregionalcode").data("kendoDropDownList");
            dropdownlist.value($scope.treeselectedRegionCode);
            $scope.regionalsetup.GROUP_SKU_FLAG = "I";
            $scope.regionalsArr.GROUP_SKU_FLAG = "I";
            $("#regionalModal").modal("toggle");
            $scope.regionalsArr.REGION_CODE = $scope.regionalsetup.REGION_CODE;

            //var tree = $("#masterregionalcode").data("kendoDropDownList");
            //if (tree != null && tree != undefined) {
            //    tree.value($scope.regionalsArr.REGION_CODE);
            //}
        }, function errorCallback(response) {
            displayPopupNotification(response.data.REGION_CODE, "error");

        });
        //}
        //else {
        //    displayPopupNotification("Please select the regional head first", "warning");
        //}
    }
    $scope.edit = function (regionalCode) {
        $scope.editFlag = "Y";
        $scope.saveupdatebtn = "Update";
        $scope.fillRegionalSetupForms(regionalCode);
        var dropdownlist = $("#masterregionalcode").data("kendoDropDownList");
        dropdownlist.value($scope.treeselectedRegionCode);

        $("#regionalModal").modal();
    }
    $scope.delete = function (code, groupskuFlag) {
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
                    debugger;
                    if (groupskuFlag === "I")
                        $scope.regionalsetup.GROUP_SKU_FLAG = "I";
                    else
                        $scope.regionalsetup.GROUP_SKU_FLAG = "G"
                    var delUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/DeleteRegionalSetupByRegionalCode?regionalCode=" + code;
                    $http({
                        method: 'POST',
                        url: delUrl
                    }).then(function successCallback(response) {

                        if (response.data.MESSAGE == "DELETED") {

                            debugger;
                            //if ($scope.regionalsetup.GROUP_SKU_FLAG !== "I") {
                                var tree = $("#regionaltree").data("kendoTreeView");
                                tree.dataSource.read();
                            //}
                                $scope.treenodeselected = "N";
                            var grid = $('#kGrid').data("kendoGrid");
                            grid.dataSource.read();
                            $scope.Cleardata();
                            displayPopupNotification("Data succesfully deleted ", "success");
                        }
                        else if (response.data.MESSAGE == "HAS_CHILD") {
                            displayPopupNotification("You can not delete. It has child.", "warning");
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
    $scope.addNewRegional = function () {

        $scope.editFlag = "N";
        $scope.savegroup = true;

        $scope.Cleardata();
        $scope.regionalsArr = [];
        $scope.MACDS = [];
        $scope.saveupdatebtn = "Save"
        $scope.groupRegionalTypeFlag = "Y";
        $scope.regionalsetup.REGION_TYPE_FLAG = "N";

        $scope.MACDS.push({ text: "<PRIMARY>", value: "" });
        $scope.regionalsArr.GROUP_SKU_FLAG = 'G';
        $scope.regionalsetup.GROUP_SKU_FLAG = 'G';
        $('#masterregionalcode').data('kendoDropDownList').value("");

        //$scope.regionalsArr.REGION_EDESC = "";
        //$scope.regionalsArr.REGION_NDESC = "";
        //$scope.regionalsArr.REMARKS = "";
        //var tree = $("#childmasterregionalcode").data("kendoDropDownList");
        //tree.setDataSource($scope.MACDS);
        $scope.regionalsArr.REGION_TYPE_FLAG = "N";

        //$("#childmasterregionalcode").data("kendoDropDownList").value("");
        $("#regionalModal").modal("toggle");
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

    $scope.Cleardata = function () {
        $scope.regionalsArr.REGION_EDESC = "";
        $scope.regionalsArr.REGION_NDESC = "";
        $scope.regionalsArr.REMARKS = "";


    }

});

