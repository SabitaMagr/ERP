﻿DTModule.controller('processSetupBomCtrl', function ($scope, $http, processSetupBomService) {
    console.log("controller hit");

    $scope.FormName = "Process Setup (Bill of Materials)";

    $scope.txtSearchString = null;
    $scope.selectedProcessType = null;
    $scope.selectedLocation = null;

    $scope.IsCategoryProcessRoutineEdit = false;

    $scope.selectedProcessForDDL = null;

    $scope.selectedProcessFlag = "CATEGORY";

    $scope.isRoutineDetail = false;

    $scope.processCategoryRoutineObj = {
        "ROOT_UNDER": "",
        "IN_ENGLISH": "",
        "IN_NEPALI": "",
        "PRIORITY_NUMBER": "",
        "PROCESS_CODE": "",
        "PRIMARY_PROCESS_NAME": "",
        "REMARK": ""
    };

    $scope.routineDetailsSetupObj = {
        "ROUTINE_NAME": "",
        "ROUTINE_CODE": "",
        "BELONGS": "",
        "BELONGS_CODE": "",
        "INPUT_INDEX_ITEM": "",
        "OUTPUT_INDEX_ITEM": "",
        "INPUT_CAPACITY": "",
        "INPUT_UNIT": "",
        "INPUT_UNIT_CODE": "",
        "INPUT_IN_PERIOD": "",
        "OUTPUT_CAPACITY": "",
        "OUTPUT_UNIT": "",
        "OUTPUT_UNIT_CODE": "",
        "OUTPUT_IN_PERIOD": ""
    };
    $scope.gridData = [];

    $scope.inputDialogAction = "Finish";
    $scope.outPutDialogAction = "Finish";

    $scope.inputMaterialSetupObj = {
        "ID": 1,
        "PROCESS": "",
        "PROCESS_CODE": "",
        "ITEM_NAME": "",
        "ITEM_CODE": "",
        "ITEM_EDESC": "",
        "QUANTITY": "",
        "UNIT": "",
        "UNIT_CODE": "",
        "MU_CODE": "",
        "REMARK": "",
        "MODAL_INFO": "INPUT"
    };

    $scope.outPutMaterialSetupObj = {
        "PROCESS": "",
        "PROCESS_CODE": "",
        "ITEM_NAME": "",
        "ITEM_CODE": "",
        "ITEM_EDESC": "",
        "VALUATION_FLAG": "",
        "QUANTITY": "",
        "OUTPUT": "",
        "OUT_PUT": "",
        "UNIT": "",
        "UNIT_CODE": "",
        "MU_CODE": "",
        "REMARK": "",
        "MODAL_INFO": "OUTPUT"
    };
    $("#selectedLocation").prop("disabled", false);  
    $("#cprradio *").prop('disabled', false);
    /* Process Category Main Page Start */

    // All available process type for process setup
    $scope.getProcessType = function () {
        var processType = processSetupBomService.getProcessType();
        processType.then(function (processRes) {
            $scope.ProcessType = processRes.data;
        });
    };

    // all mu code (unit code) for selected item
    $scope.getProcessMuCode = function () {
        var allMuUnit = processSetupBomService.getProcessMuCode();
        allMuUnit.then(function (muRes) {
            $scope.AvailabelMuUnit = muRes.data;
        });
    };

    // item list for input output
    $scope.getAllItemForInputOutput = function () {
        var allItem = processSetupBomService.GetAllItemForInputOutput();
        allItem.then(function (itemRes) {
            $scope.AllItemForInputOutput = itemRes.data;
        });
    };

    //get all location for process
    $scope.getAllLocation = function () {
        var allLocation = processSetupBomService.GetAllLocation();
        allLocation.then(function (locRes) {
            $scope.AllAvailableLocation = locRes.data;
        });
    };

    $scope.BrowseInputUnitTreeList = function (tag) {
        $('#inputUnitModal').toggle();
        $scope.CalledFrom = tag;
    };

    $scope.BrowseOutputUnitTreeList = function (tag) {
        $('#outputUnitModal').modal('show');
        $scope.CalledFrom = tag;
    };

    // open input index item dialog
    $scope.BrowseInputIndexItem = function () {
        $('#inputIndexItemModal').toggle();
    };

    //configure and open input index grid
    $scope.inputIndexItemGridOptions = {
        dataSource: {
            data: [{}]
        },
        selectable: "single",
        scrollable: true,
        pageable: true,
        groupable: false,
        dataBound: function (e) {
            $("#inputIndexItemGrid tbody tr").css("cursor", "pointer");
            $("#inputIndexItemGrid tbody tr").on('dblclick', function () {
                var grid = $("#inputIndexItemGrid").data("kendoGrid");
                var dataItem = grid.dataItem(this);
                $scope.routineDetailsSetupObj.INPUT_INDEX_ITEM = dataItem.ITEM_CODE;
                $("#input_index_item").data("kendoComboBox").value(dataItem.ITEM_CODE);
                $("#inputIndexItemModal").toggle();
                $scope.$apply();
            });
        },
        columns: [
            {
                field: "ITEM_CODE",
                title: "Id"
            },
            {
                field: "ITEM_NAME",
                title: "Item Description"
            }
        ]
    };

    //configure production period grid
    $scope.productionInPeriodGridOptions = {
        dataSource: {
            transport: {
                read: {
                    url: window.location.protocol + "//" + window.location.host + "/api/ProcessSetupBomApi/GetProcessPeriod",
                    dataType: "json"
                }
            }
        },
        selectable: "single",
        scrollable: true,
        pageable: true,
        groupable: false,
        dataBound: function (e) {
            $("#productionInPeriodGrid tbody tr").css("cursor", "pointer");
            $("#productionInPeriodGrid tbody tr").on('dblclick', function () {
                var grid = $("#productionInPeriodGrid").data("kendoGrid");
                var dataItem = grid.dataItem(this);

                $scope.routineDetailsSetupObj.INPUT_IN_PERIOD = dataItem.PERIOD_CODE;
                $("#product_in_period").data("kendoComboBox").value(dataItem.PERIOD_CODE);
                $("#productionInPeriodModal").toggle();
                $scope.$apply();
            });
        },
        columns: [
            {
                field: "PERIOD_CODE",
                title: "Id"
            },
            {
                field: "PERIOD_EDESC",
                title: "Period Description"
            }
        ]
    };

    //configure time take period grid
    $scope.timeTakenInPeriodGridOptions = {
        dataSource: {
            transport: {
                read: {
                    url: window.location.protocol + "//" + window.location.host + "/api/ProcessSetupBomApi/GetProcessPeriod",
                    dataType: "json"
                }
            }
        },
        selectable: "single",
        scrollable: true,
        pageable: true,
        groupable: false,
        dataBound: function (e) {
            $("#timeTakenInPeriodGrid tbody tr").css("cursor", "pointer");
            $("#timeTakenInPeriodGrid tbody tr").on('dblclick', function () {
                var grid = $("#timeTakenInPeriodGrid").data("kendoGrid");
                var dataItem = grid.dataItem(this);

                $scope.routineDetailsSetupObj.OUTPUT_IN_PERIOD = dataItem.PERIOD_CODE;
                $("#time_taken_in_period").data("kendoComboBox").value(dataItem.PERIOD_CODE);
                $("#timeTakenInPeriodModal").toggle();
                $scope.$apply();
            });
        },
        columns: [
            {
                field: "PERIOD_CODE",
                title: "Id"
            },
            {
                field: "PERIOD_EDESC",
                title: "Period Description"
            }
        ]
    };

    //production unit grid
    $scope.productionUnitGridOptions = {
        dataSource: {
            data: [{}]
        },
        selectable: "single",
        scrollable: true,
        pageable: true,
        groupable: false,
        dataBound: function (e) {
            $("#productionUnitGrid tbody tr").css("cursor", "pointer");
            $("#productionUnitGrid tbody tr").on('dblclick', function () {
                var grid = $("#productionUnitGrid").data("kendoGrid");
                var dataItem = grid.dataItem(this);
                $scope.routineDetailsSetupObj.INPUT_UNIT = dataItem.UNIT;
                $("#production_unit").data("kendoComboBox").value(dataItem.UNIT);
                $("#productionUnitModal").toggle();
                $scope.$apply();
            });

        },
        columns: [
            {
                field: "UNIT",
                title: "UNIT"
            }
        ]
    };

    $scope.timeTakenUnitGridOptions = {
        dataSource: {
            data: [{}]
        },
        selectable: "single",
        scrollable: true,
        pageable: true,
        groupable: false,
        dataBound: function (e) {
            $("#timeTakenUnitGrid tbody tr").css("cursor", "pointer");
            $("#timeTakenUnitGrid tbody tr").on('dblclick', function () {
                var grid = $("#timeTakenUnitGrid").data("kendoGrid");
                var dataItem = grid.dataItem(this);
                $scope.routineDetailsSetupObj.OUTPUT_UNIT = dataItem.UNIT;
                $("#time_taken_unit").data("kendoComboBox").value(dataItem.UNIT);
                $("#timeTakenUnitModal").toggle();
                $scope.$apply();
            });

        },
        columns: [
            {
                field: "UNIT",
                title: "UNIT"
            }
        ]
    };

    // close input index item dialog
    $scope.closeInputIndexItemModal = function () {
        $('#inputIndexItemModal').toggle();
    };

    // open output index item dialog
    $scope.BrowseOutputIndexItem = function () {
        $('#outPutIndexItemModal').toggle();
    };

    $scope.outPutIndexItemGridOptions = {
        dataSource: {
            data: [{}]
        },
        selectable: "single",
        scrollable: true,
        pageable: true,
        groupable: false,
        dataBound: function (e) {
            $("#outPutIndexItemGrid tbody tr").css("cursor", "pointer");
            $("#outPutIndexItemGrid tbody tr").on('dblclick', function () {
                var grid = $("#outPutIndexItemGrid").data("kendoGrid");
                var dataItem = grid.dataItem(this);
                $scope.routineDetailsSetupObj.OUTPUT_INDEX_ITEM = dataItem.ITEM_NAME;
                $("#output_index_item").data("kendoComboBox").value(dataItem.ITEM_CODE);
                $("#outPutIndexItemModal").toggle();
                $scope.$apply();
            });

        },
        columns: [
            {
                field: "ITEM_CODE",
                title: "Id"
            },
            {
                field: "ITEM_NAME",
                title: "Item Description"
            }
        ]
    };

    // close out put index item dialog
    $scope.closeOutPutIndexItemModal = function () {
        $('#outPutIndexItemModal').toggle();
    };

    $scope.BrowseProductionUnit = function () {
        $("#productionUnitModal").toggle();
    };

    $scope.closeProductionUnitModal = function () {
        $("#productionUnitModal").toggle();
    };

    $scope.BrowseProductionInPeriod = function () {
        //var prodProidGrid = $("#productionInPeriodGrid").data("kendoGrid");
        //prodProidGrid.dataSource.transport.options.read.url = window.location.protocol + "//" + window.location.host + "/ProcessSetupBomApi/GetProcessPeriod";
        //prodProidGrid.dataSource.read();
        $("#productionInPeriodModal").toggle();
    };

    $scope.closeProductionInPeriodModal = function () {
        $("#productionInPeriodModal").toggle();
    };

    $scope.BrowseTimeTakenUnit = function () {
        $("#timeTakenUnitModal").toggle();
    };

    $scope.closeTimeTakenUnitModal = function () {
        $("#timeTakenUnitModal").toggle();
    };

    $scope.BrowseTimeTakenInPeriod = function () {

        //var timeTakePeriodGrid = $("#timeTakenInPeriodGrid").data("kendoGrid");
        //timeTakePeriodGrid.dataSource.transport.options.read.url = window.location.protocol + "//" + window.location.host + "/ProcessSetupBomApi/GetProcessPeriod";
        //timeTakePeriodGrid.dataSource.read();
        $("#timeTakenInPeriodModal").toggle();
    };

    $scope.closeTimeTakenInPeriodModal = function () {
        $("#timeTakenInPeriodModal").toggle();
    };


    //--------------------------------END----------------------------------------

    var processTypeUrl = window.location.protocol + "//" + window.location.host + "/api/ProcessSetupBomApi/GetAllProcessTypeCode";
    $scope.processTypeDataSource = new kendo.data.DataSource({
        transport: {
            read: {
                url: processTypeUrl
            }
        }
    });

    $scope.processTypeOptions = {
        dataSource: $scope.processTypeDataSource,
        filter: "contains",
        optionLabel: "--Select Process Type--",
        dataTextField: "PROCESS_TYPE_EDESC",
        dataValueField: "PROCESS_TYPE_CODE"

    };

    var itemUrl = window.location.protocol + "//" + window.location.host + "/api/ProcessSetupBomApi/GetAllItemForInputOutput";
    $scope.itemDataSource = new kendo.data.DataSource({
        transport: {
            read: {
                url: itemUrl
            }
        }
    });

    $scope.InputItemUnitList = [];
    $("#inputMaterialsItem").kendoAutoComplete({
        minLength: 1,
        dataTextField: "ITEM_EDESC",
        dataSource: $scope.itemDataSource,
        filter: "contains",
        placeholder: "Select Item...",
        select: function (e) {

            $("#inputMaterialsUnit").data("kendoComboBox").value("");
            var dataItem = this.dataItem(e.item.index());

            $scope.inputMaterialSetupObj.ITEM_NAME = dataItem.ITEM_EDESC;
            $scope.inputMaterialSetupObj.ITEM_CODE = dataItem.ITEM_CODE;

            $scope.inputMaterialSetupObj.UNIT = dataItem.MU_EDESC;
            $scope.inputMaterialSetupObj.UNIT_CODE = dataItem.MU_CODE;
            //set unit as per item

            $scope.InputItemUnitList.push({
                "MU_CODE": dataItem.MU_CODE,
                "MU_EDESC": dataItem.MU_EDESC
            });

            var inputUnitPerItem = $("#inputMaterialsUnit").data("kendoComboBox");
            inputUnitPerItem.dataSource.data($scope.InputItemUnitList);
            inputUnitPerItem.dataSource.query();
            inputUnitPerItem.select(function (dataItems) {
                return dataItems.MU_EDESC = dataItem.MU_EDESC;
            });
        }
        //separator: ","
    });

    $scope.OutputItemUnitList = [];
    $("#outPutMaterialsItem").kendoAutoComplete({
        minLength: 1,
        dataTextField: "ITEM_EDESC",
        dataSource: $scope.itemDataSource,
        filter: "contains",
        placeholder: "Select Item...",
        select: function (e) {

            $("#outputMaterialsUnit").data("kendoComboBox").value("");
            var dataItem = this.dataItem(e.item.index());
            $scope.outPutMaterialSetupObj.ITEM_NAME = dataItem.ITEM_EDESC;
            $scope.outPutMaterialSetupObj.ITEM_CODE = dataItem.ITEM_CODE;

            $scope.outPutMaterialSetupObj.UNIT = dataItem.MU_EDESC;
            $scope.outPutMaterialSetupObj.UNIT_CODE = dataItem.MU_CODE;

            $scope.OutputItemUnitList.push({
                "MU_CODE": dataItem.MU_CODE,
                "MU_EDESC": dataItem.MU_EDESC
            });
            var outputUnitPerItem = $("#outputMaterialsUnit").data("kendoComboBox");
            outputUnitPerItem.dataSource.data($scope.OutputItemUnitList);
            outputUnitPerItem.dataSource.query();
            //outputUnitPerItem.select(0);
            outputUnitPerItem.select(function (dataItems) {
                return dataItems.MU_EDESC = dataItem.MU_EDESC;
            });

        }
        //separator: ","
    });

    var locationUrl = window.location.protocol + "//" + window.location.host + "/api/ProcessSetupBomApi/GetAllLocation";
    $scope.locationDataSource = new kendo.data.DataSource({
        transport: {
            read: {
                url: locationUrl
            }
        }
    });

    $scope.locationOptions = {
        dataSource: $scope.locationDataSource,
        filter: "contains",
        optionLabel: "--Select Location--",
        dataTextField: "LOCATION_EDESC",
        dataValueField: "LOCATION_CODE"
    };

    $scope.inputMuGridOptions = {
        dataSource: {
            type: "json",
            transport: {
                read: "/api/ProcessSetupBomApi/GetProcessMuCodeList",
                parameterMap: function (options, type) {
                    //if (type === 'read') {
                    //    return {
                    //        from: "bom"
                    //    };
                    //}
                }
            },
            pageSize: 20,
            serverSorting: true
        },
        selectable: "single",
        scrollable: true,
        pageable: true,
        groupable: false,
        dataBound: function (e) {
            $("#inputMuGrid tbody tr").css("cursor", "pointer");
            $("#inputMuGrid tbody tr").on('dblclick', function () {
                var grid = $("#inputMuGrid").data("kendoGrid");
                var dataItem = grid.dataItem(this);
                $scope.routineDetailsSetupObj.INPUT_UNIT = dataItem.MU_EDESC;
                if ($scope.CalledFrom === 'input') $scope.inputMaterialSetupObj.UNIT = dataItem.MU_EDESC; $scope.inputMaterialSetupObj.UNIT_CODE = dataItem.MU_CODE;
                if ($scope.CalledFrom === 'output') $scope.outPutMaterialSetupObj.UNIT = dataItem.MU_EDESC; $scope.outPutMaterialSetupObj.UNIT_CODE = dataItem.MU_CODE;
                //$scope.routineDetailsSetupObj.INPUT_UNIT_CODE = dataItem.MU_CODE;
                $("#inputUnitModal").toggle();
                $scope.$apply();
            });

        },
        columns: [
            {
                field: "MU_CODE",
                title: "Codes"
            },
            {
                field: "MU_EDESC",
                title: "Description"
            }
        ]
    };

    $scope.outPutMuGridOptions = {
        dataSource: {
            type: "json",
            transport: {
                read: "/api/ProcessSetupBomApi/GetProcessMuCodeList",
                parameterMap: function (options, type) {
                    //if (type === 'read') {
                    //    return {
                    //        from: "bom"
                    //    };
                    //}
                }
            },
            pageSize: 20,
            serverSorting: true
        },
        selectable: "single",
        scrollable: true,
        pageable: true,
        groupable: false,
        dataBound: function (e) {
            $("#outPutMuGrid tbody tr").css("cursor", "pointer");
            $("#outPutMuGrid tbody tr").on('dblclick', function () {
                var grid = $("#outPutMuGrid").data("kendoGrid");
                var dataItem = grid.dataItem(this);
                $scope.routineDetailsSetupObj.OUTPUT_UNIT = dataItem.MU_EDESC;
                if ($scope.CalledFrom === 'output') $scope.outPutMaterialSetupObj.UNIT = dataItem.MU_EDESC; $scope.outPutMaterialSetupObj.UNIT_CODE = dataItem.MU_CODE;
                if ($scope.CalledFrom === 'input') $scope.inputMaterialSetupObj.UNIT = dataItem.MU_EDESC; $scope.inputMaterialSetupObj.UNIT_CODE = dataItem.MU_CODE;
                //$scope.routineDetailsSetupObj.INPUT_UNIT_CODE = dataItem.MU_CODE;
                $("#outputUnitModal").toggle();
                $scope.$apply();
            });

        },
        columns: [
            {
                field: "MU_CODE",
                title: "Codes"
            },
            {
                field: "MU_EDESC",
                title: "Description"
            }
        ]
    };


    var processTypeDDLUrl = window.location.protocol + "//" + window.location.host + "/api/ProcessSetupBomApi/GetAllProcessForDDL";
    $scope.processTypeDDLDataSource = new kendo.data.DataSource({
        transport: {
            read: {
                url: processTypeDDLUrl
            }
        }
    });

    $scope.processTypeDDLOptions = {

        dataSource: $scope.processTypeDDLDataSource,
        filter: "contains",
        optionLabel: "--Select Root Type--",
        dataTextField: "PROCESS_EDESC",
        dataValueField: "PROCESS_CODE"
        //onChange: onDDLChange,

    };

    $scope.fillPrimaryProcessName = false;
    $scope.onDDLChange = function (e) {

        var selectedText = $("#processType").data("kendoDropDownList").text();

        if (selectedText === "Primary") {

            $scope.processCategoryRoutineObj.PROCESS_CODE = $scope.selectedProcessForDDL;
            $scope.fillPrimaryProcessName = true;


        } else {
            $scope.fillPrimaryProcessName = false;
            var childProcessCode = processSetupBomService.GetChildProcessCode($scope.selectedProcessForDDL);
            childProcessCode.then(function (cpRes) {
                $scope.processCategoryRoutineObj.PROCESS_CODE = cpRes.data;
            });
        }
    };


    $scope.getAllCategoryProcessRoutineTree = function () {
        var allCategoryProcessRoutine = processSetupBomService.getAllCategoryProcessRoutineTree();
        allCategoryProcessRoutine.then(function (catRes) {
            $scope.AllCategoryProcessRoutine = catRes.data;
        });
    };

    $scope.removeNodeSelection = function () {
        var treeview = $("#categoryTreeGrid").data("kendoTreeView");
        treeview.select($());
        $scope.BindRoutineResultGrid();
    };

    var getCategoryTree = window.location.protocol + "//" + window.location.host + "/api/ProcessSetupBomApi/GetAllProcessCategoryRoutine";
    var categoryTreeDataSource = new kendo.data.HierarchicalDataSource({
        transport: {
            read: {
                url: getCategoryTree,
                dataType: "json"
            }
        },
        schema: {
            model: {
                id: "PROCESS_CODE",
                children: "ITEMS",
                hasChildren: "HAS_BRANCH"
            },
            parse: function (response) {
                return _.each(_.filter(response, function (x) {
                    return x.PRE_PROCESS_CODE === '00';
                }), function (y) {
                    y.ITEMS = _.filter(response, function (z) {
                        return z.PRE_PROCESS_CODE === y.PROCESS_CODE;
                    });
                    y.HAS_BRANCH = y.ITEMS.length === 0 ? false : true;
                });
            }
        }
    });

    $("#dropdowntree").kendoDropDownList({
        dataTextField: "PROCESS_EDESC",
        dataValueField: "PROCESS_CODE",
        dataSource: categoryTreeDataSource,
        //index: 0,
        change: onChange
    });


    $scope.processCategoryRoutineObj.PROCESS_CODE = $("#dropdowntree").val();

    function onChange(e) {

    }


    $("#categoryTreeGrid").kendoTreeView({
        loadOnDemand: false,
       // autoScroll: true,
       // autoBind: true,
        dataSource: categoryTreeDataSource,
        dataTextField: "PROCESS_EDESC",
       // height: 400,
        select: onCategorySelect,
        //scrollable: {
        //    virtual: true
        //},
        //dataBound: function () {

        //}

    });


    //Search on TREEE

    $("#processSearchString").on("input", function () {
        var query = this.value.toLowerCase();
        var dataSource = $("#categoryTreeGrid").data("kendoTreeView").dataSource;

        filter(dataSource, query);
    });

    function filter(dataSource, query) {
        var hasVisibleChildren = false;
        var data = dataSource instanceof kendo.data.HierarchicalDataSource && dataSource.data();

        for (var i = 0; i < data.length; i++) {
            var item = data[i];
            var text = item.PROCESS_EDESC.toLowerCase();
            var itemVisible =
                query === true // parent already matches
                || query === "" // query is empty
                || text.indexOf(query) >= 0; // item text matches query

            var anyVisibleChildren = filter(item.children, itemVisible || query); // pass true if parent matches

            hasVisibleChildren = hasVisibleChildren || anyVisibleChildren || itemVisible;

            item.hidden = !itemVisible && !anyVisibleChildren;
        }

        if (data) {
            // Re-apply the filter on the children.
            dataSource.filter({ field: "hidden", operator: "neq", value: true });
        }

        return hasVisibleChildren;
    }


    var selectedItemList = [];

    var selectedItemObj = {
        "PROCESS_CODE": "",
        "PROCESS_EDESC": ""
    };
    $scope.selectedParentProcessCode;

    $scope.isSelectedCatIsProcess = false;
    function onCategorySelect(e) {
        debugger;
        var treeview = $("#categoryTreeGrid").data("kendoTreeView");
        var item = treeview.dataItem(e.node);
        console.log("selected category : " + JSON.stringify(item));

        if (item) {

            if (item.PROCESS_FLAG === "P") $scope.isSelectedCatIsProcess = true;

            selectedItemObj = {
                "PROCESS_CODE": item.PROCESS_CODE,
                "PROCESS_EDESC": item.PROCESS_EDESC
            };
            $scope.selectedParentProcessCode = item.PROCESS_CODE;
            $scope.selectedParentProcessDesc = item.PROCESS_EDESC;


            $scope.selectedProcessForDDL = item.PROCESS_CODE;
            $scope.routineDetailsSetupObj.BELONGS = selectedItemObj.PROCESS_EDESC;
            var childProcess = processSetupBomService.GetChildProcessCode($scope.selectedProcessForDDL);
            childProcess.then(function (cpRes) {
                $scope.processCategoryRoutineObj.PROCESS_CODE = cpRes.data;
                $scope.routineDetailsSetupObj.BELONGS = cpRes.data;
            });

            var routineResultGrid = $("#routineResultGrid").data("kendoGrid");
            routineResultGrid.dataSource.transport.options.read.url = window.location.protocol + "//" + window.location.host + "/api/ProcessSetupBomApi/GetRoutineBasedOnProcessCode?processCode=" + selectedItemObj.PROCESS_CODE;
            routineResultGrid.dataSource.read();

        } else {
            selectedItem.pop();
        }

    }

    $scope.BindRoutineResultGrid = function () {

        $scope.routineResultOptions = {
            dataSource: {
                type: "json",
                transport: {
                    read: "/api/ProcessSetupBomApi/GetRoutineBasedOnProcessCode",
                    parameterMap: function (options, type) {
                        if (type === 'read') {
                            return {
                                processCode: selectedItemObj.PROCESS_CODE
                            };
                        }
                    }
                },
                pageSize: 20,
                serverSorting: true
            },
            selectable: "single",
            resizable: true,
            scrollable: true,
            pageable: true,
            groupable: false,
            change: routineRowSelected,
            dataBound: function (e) {

                $("#routineResultGrid tbody tr").css("cursor", "pointer");

            },
            columns: [
                {
                    field: "SHORT_CUT",
                    title: "Short Cut"
                },
                {
                    field: "PROCESS_DESCRIPTION",
                    title: "Process Description"
                },
                {
                    field: "ITEM_DESCRIPTION",
                    title: "Item Description"
                },
                {
                    field: "CAPACITY",
                    title: "Capacity"
                },
                {
                    field: "MU_CODE",
                    title: "MU"
                },
                {
                    field: "LOCATION_EDESC",
                    title: "Location"
                }
            ],
        };
    };

    function routineRowSelected(e) {
        debugger;
        var grid = $("#routineResultGrid").data("kendoGrid"),
            selectedItem = grid.dataItem(grid.select());

        if (selectedItem) {
            $('#editRoutine').attr("disabled", false);
            $('#newProcessSetupBtn').attr("disabled", true);

            selectedItemObj = {
                "PROCESS_CODE": selectedItem.SHORT_CUT,
                "PROCESS_EDESC": selectedItem.PROCESS_DESCRIPTION
            };

            // $scope.selectedProcessForDDL = selectedItem.SHORT_CUT;
            $scope.selectedProcessForDDL = $scope.selectedParentProcessCode;
            var childProcess = processSetupBomService.GetChildProcessDetail(selectedItemObj.PROCESS_CODE);
            childProcess.then(function (cpRes) {
                debugger;
                console.log("CP RES Detail: " + JSON.stringify(cpRes));
                $scope.processCategoryRoutineObj.PROCESS_CODE = cpRes.data.PROCESS_CODE;
                $scope.routineDetailsSetupObj.ROUTINE_NAME = cpRes.data.PROCESS_EDESC;
                $scope.processCategoryRoutineObj.IN_ENGLISH = cpRes.data.PROCESS_EDESC;
                $scope.processCategoryRoutineObj.IN_NEPALI = cpRes.data.PROCESS_EDESC;
                $scope.selectedProcessType = cpRes.data.PROCESS_TYPE_CODE;
                $scope.selectedLocation = cpRes.data.PRE_LOCATION_CODE;
                $scope.processCategoryRoutineObj.PRIORITY_NUMBER = cpRes.data.PRIORITY_ORDER_NO;
                $scope.processCategoryRoutineObj.REMARK = cpRes.data.REMARKS;
            });



            //childProcess.then(function (cpRes) {
            //    $scope.processCategoryRoutineObj.PROCESS_CODE = cpRes.data.PROCESS_CODE;
            //    $scope.routineDetailsSetupObj.BELONGS = cpRes.data;
            //});

            //var routineResultGrid = $("#routineResultGrid").data("kendoGrid");
            //routineResultGrid.dataSource.transport.options.read.url = window.location.protocol + "//" + window.location.host + "/api/ProcessSetupBomApi/GetRoutineBasedOnProcessCode?processCode=" + selectedItemObj.PROCESS_CODE;
            //routineResultGrid.dataSource.read();

        } else {
            selectedItem.pop();
        }

    }

    $scope.BindRoutineResultGrid();
    $scope.onContextAdd = function (event) {
        debugger;
        setTimeout(function () {
            debugger;
        if (event.PROCESS_FLAG == "C") {
            $scope.selectedProcessFlag = "PROCESS";
            $("input:radio[value='PROCESS']").prop('checked', true);
            $('#addNewProcessModel').toggle();
           

        }
        if (event.PROCESS_FLAG == "P") {
            $scope.selectedProcessFlag = "ROUTINE";
            $("input:radio[value='ROUTINE']").prop('checked', true);
            //$('#routineRadio').attr('checked', true);
            $('#addNewProcessModel').toggle();
        }
            //$('#newProcessSetupBtn').trigger('click');
        }, 0);
    }
    $scope.onContextUpdate = function (event) {
        debugger;
        var childProcess = processSetupBomService.GetChildProcessDetail(event.PROCESS_CODE);
        childProcess.then(function (cpRes) {
            debugger;
            console.log("CP RES Detail: " + JSON.stringify(cpRes));
            $scope.processCategoryRoutineObj.PROCESS_CODE = cpRes.data.PROCESS_CODE;
            $scope.routineDetailsSetupObj.ROUTINE_NAME = cpRes.data.PROCESS_EDESC;
            $scope.processCategoryRoutineObj.IN_ENGLISH = cpRes.data.PROCESS_EDESC;
            $scope.processCategoryRoutineObj.IN_NEPALI = cpRes.data.PROCESS_EDESC;
            $scope.selectedProcessType = cpRes.data.PROCESS_TYPE_CODE;
            $scope.selectedLocation = cpRes.data.PRE_LOCATION_CODE;
            $scope.processCategoryRoutineObj.PRIORITY_NUMBER = cpRes.data.PRIORITY_ORDER_NO;
            $scope.processCategoryRoutineObj.REMARK = cpRes.data.REMARKS;
            $('#processType').data("kendoDropDownList").value(event.PROCESS_TYPE_CODE);
            $('#selectedLocation').data("kendoDropDownList").value(event.LOCATION_CODE);
            $scope.IsCategoryProcessRoutineEdit = true;
            if (event.PROCESS_FLAG == "R") {
                $scope.selectedProcessFlag = "ROUTINE";
                $('#addNewProcessModel').toggle();
            } else if (event.PROCESS_FLAG == "P") {
                $scope.selectedProcessFlag = "PROCESS";
                $("input:radio[value='PROCESS']").prop('checked', true);
                $('#addNewProcessModel').toggle();
            } else {
                $scope.selectedProcessFlag = "CATEGORY";
                $('#catRadio').attr('checked', true);
                $('#addNewProcessCategoryModel').toggle();
            }

        });
        //$scope.processCategoryRoutineObj.PROCESS_CODE = event.PROCESS_CODE;
        //$scope.processCategoryRoutineObj.IN_ENGLISH = event.PROCESS_EDESC;
        //$scope.selectedProcessType = event.PROCESS_TYPE_CODE;       
        //$scope.selectedLocation = event.LOCATION_CODE;
        //$scope.processCategoryRoutineObj.PRIORITY_NUMBER = event.PRIORITY_ORDER_NO;             
    }
    $scope.onContextDelete = function (event) {
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

                    var delUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/DeleteProcessSetupByProcessCode?processCode=" + event.PROCESS_CODE;
                    $http({
                        method: 'POST',
                        url: delUrl
                    }).then(function successCallback(response) {

                        if (response.data.MESSAGE == "DELETED") {
                            var tree = $("#customertree").data("kendoTreeView");
                            tree.dataSource.read();
                            $scope.treenodeselected = "N";
                            displayPopupNotification("Data succesfully deleted ", "success");
                        }
                        if (response.data.MESSAGE == "HAS_CHILD") {
                            displayPopupNotification("Please delete child first", "warning");
                        }
                    }, function errorCallback(response) {
                        displayPopupNotification(response.data.STATUS_CODE, "error");
                        // called asynchronously if an error occurs
                        // or server returns response with an error status.
                    });

                }

                else {
                    bootbox.hideAll();
                    //displayPopupNotification(response.data.message, "error");
                }
            }
        });
    }

    $scope.SaveProcessCategorySetup = function () {
        debugger;
        var saveModel = {
            "IN_ENGLISH": $scope.processCategoryRoutineObj.IN_ENGLISH,
            "IN_NEPALI": $scope.processCategoryRoutineObj.IN_NEPALI,
            "PROCESS_CODE": $scope.processCategoryRoutineObj.PROCESS_CODE,
            "REMARK": $scope.processCategoryRoutineObj.REMARK,
            "PROCESS_FLAG": $scope.selectedProcessFlag,
            "PRIORITY_NUMBER": $scope.processCategoryRoutineObj.PRIORITY_NUMBER,
            "PROCESS_TYPE": $scope.selectedProcessType,
            "ROOT_UNDER": $scope.selectedProcessForDDL,
            "LOCATION": $scope.selectedLocation,
            "IS_EDIT": $scope.IsCategoryProcessRoutineEdit

        };
       
        var saveResult = processSetupBomService.SaveProcessCategorySetup(saveModel);
        saveResult.then(function (res) {
            debugger;
            if (res.data.MESSAGE === "Successful") {
                if (res.data.PROCESS_FLAG === "ROUTINE" || res.data.PROCESS_FLAG === 'R') {
                    debugger;
                    $scope.IsCategoryProcessRoutineEdit = false;
                    $scope.isRoutineDetail = true;
                    DisplayBarNotificationMessage("Process created successfully");
                    $('#addNewProcessCategoryModel').toggle();
                    $scope.routineDetailsSetupObj.ROUTINE_NAME = $scope.processCategoryRoutineObj.IN_ENGLISH;
                    $scope.routineDetailsSetupObj.ROUTINE_CODE = $scope.processCategoryRoutineObj.PROCESS_CODE;
                    $scope.routineDetailsSetupObj.BELONGS = $scope.selectedParentProcessDesc;
                    $scope.routineDetailsSetupObj.BELONGS_CODE = $scope.processCategoryRoutineObj.ROOT_UNDER;


                    if (res.data.SAVED_MODAL.IS_EDIT) {
                        debugger;
                        var getInputItemOfGivenRoutine = processSetupBomService.GetInputRoutineDetail(JSON.stringify($scope.processCategoryRoutineObj.PROCESS_CODE));
                        getInputItemOfGivenRoutine.then(function (iRes) {
                            if (iRes.data.length > 0) {
                                //reinitialize the  input material setup grid

                                $("#btnSaveRoutineDetail").show();

                                var billOfMatGrid = $("#billOfMatGrid").data("kendoGrid");
                                billOfMatGrid.dataSource.data(iRes.data);
                                billOfMatGrid.dataSource.query();


                                for (var i = 0; i < iRes.data.length; i++) {
                                    $scope.routineDetailsSetupObj.INPUT_INDEX_ITEM = iRes.data[i].ITEM_CODE;
                                    $scope.routineDetailsSetupObj.INPUT_UNIT = iRes.data[i].MU_CODE;

                                    $scope.InputIndexItemList.push({
                                        "ITEM_CODE": iRes.data[i].ITEM_CODE,
                                        "ITEM_NAME": iRes.data[i].ITEM_EDESC
                                    });

                                    //reinitialize the input index item comboBox
                                    var idex = $("#input_index_item").data('kendoComboBox');
                                    idex.dataSource.data($scope.InputIndexItemList);
                                    idex.dataSource.query();
                                    idex.select(0);


                                    //reinitialize the input index item grid
                                    var inputGrid = $("#inputIndexItemGrid").data("kendoGrid");
                                    inputGrid.dataSource.data($scope.InputIndexItemList);
                                    inputGrid.dataSource.query();



                                    $scope.ProductionUnitList.push({
                                        "UNIT": iRes.data[i].MU_CODE
                                    });


                                    //reinitialize the input unit comboBox
                                    var prodUnit = $("#production_unit").data("kendoComboBox");
                                    prodUnit.dataSource.data($scope.ProductionUnitList);
                                    prodUnit.dataSource.query();
                                    prodUnit.select(0);
                                    //$scope.inputMaterialSetupObj.UNIT =iRes.data[i].UNIT;
                                    //$scope.inputMaterialSetupObj.UNIT_CODE = iRes.data[i].UNIT_CODE;

                                    //reinitialize the input unit grid
                                    var prodUnitGrid = $("#productionUnitGrid").data("kendoGrid");
                                    prodUnitGrid.dataSource.data($scope.ProductionUnitList);
                                    prodUnitGrid.dataSource.query();

                                    $scope.InputPeriodList.push({
                                        "PERIOD_CODE": iRes.data[i].PERIOD_CODE,
                                        "PERIOD_EDESC": iRes.data[i].PERIOD_EDESC
                                    });

                                    var inputPeriodCBox = $("#product_in_period").data("kendoComboBox");
                                    inputPeriodCBox.dataSource.data($scope.InputPeriodList);
                                    inputPeriodCBox.dataSource.query();
                                    inputPeriodCBox.select(0);
                                    $scope.routineDetailsSetupObj.INPUT_CAPACITY = iRes.data[i].INDEX_CAPACITY;
                                    $scope.routineDetailsSetupObj.INPUT_IN_PERIOD = iRes.data[i].PERIOD_CODE;
                                    console.log("res.data[]======================>>> " + JSON.stringify(iRes.data));
                                    //reinitialize the input unit grid
                                    //var inputPeriodGrid = $("#productionUnitGrid").data("kendoGrid");
                                    //inputPeriodGrid.dataSource.data($scope.ProductionUnitList);
                                    //inputPeriodGrid.dataSource.query();


                                }

                            } else {
                                alert("no data");
                            }
                        });

                        var getOutputItemOfGivenRoutine = processSetupBomService.GetOutputRoutineDetail(JSON.stringify($scope.processCategoryRoutineObj.PROCESS_CODE));
                        getOutputItemOfGivenRoutine.then(function (oRes) {
                            if (oRes.data.length > 0) {
                                debugger;
                                console.log("o.Res.data===============>>" + JSON.stringify(oRes.data));
                                //reinitialize the output material setup grid
                                var outputMatGrid = $("#outputMatGrid").data("kendoGrid");
                                outputMatGrid.dataSource.data(oRes.data);
                                outputMatGrid.dataSource.query();

                                for (var o = 0; o < oRes.data.length; o++) {
                                    $scope.routineDetailsSetupObj.OUTPUT_INDEX_ITEM = oRes.data[o].ITEM_CODE;
                                    $scope.routineDetailsSetupObj.OUTPUT_UNIT = oRes.data[o].MU_CODE;
                                    //if (!$scope.OutputIndexItemList[o].ITEM_CODE === oRes.data[o].ITEM_CODE) {
                                    $scope.OutputIndexItemList.push({
                                        "ITEM_CODE": oRes.data[o].ITEM_CODE,
                                        "ITEM_NAME": oRes.data[o].ITEM_EDESC
                                    });

                                    //reinitialize the out put index item kendocombox
                                    var odex = $("#output_index_item").data('kendoComboBox');
                                    odex.dataSource.data($scope.OutputIndexItemList);
                                    odex.dataSource.query();
                                    odex.select(0);
                                    //$scope.routineDetailsSetupObj.OUTPUT_INDEX_ITEM = oRes.data.ITEM_EDESC;
                                    //$("#output_index_item").data("kendoComboBox").value(oRes.data.ITEM_CODE);

                                    //reinitialize the out put index grid
                                    var outputGrid = $("#outPutIndexItemGrid").data("kendoGrid");
                                    outputGrid.dataSource.data($scope.OutputIndexItemList);
                                    outputGrid.dataSource.query();

                                    //}


                                    $scope.TimeTakenUnitList.push({
                                        "UNIT": oRes.data[o].MU_CODE
                                    });


                                    //reinitialize the out put unit box
                                    var timeTakenUnit = $("#time_taken_unit").data("kendoComboBox");
                                    timeTakenUnit.dataSource.data($scope.TimeTakenUnitList);
                                    timeTakenUnit.dataSource.query();
                                    timeTakenUnit.select(0);
                                    //$scope.outPutMaterialSetupObj.UNIT = oRes.data[o].UNIT;
                                    //$scope.outPutMaterialSetupObj.UNIT_CODE = oRes.data[o].UNIT_CODE;

                                    var timeTakenUnitGrid = $("#timeTakenUnitGrid").data("kendoGrid");
                                    timeTakenUnitGrid.dataSource.data($scope.TimeTakenUnitList);
                                    timeTakenUnitGrid.dataSource.query();

                                    $scope.OutputPeriodList.push({
                                        "PERIOD_CODE": oRes.data[o].PERIOD_CODE,
                                        "PERIOD_EDESC": oRes.data[o].PERIOD_EDESC
                                    });


                                    //reinitialize the out put unit box
                                    var timeTakenPeriodcBox = $("#time_taken_in_period").data("kendoComboBox");
                                    timeTakenPeriodcBox.dataSource.data($scope.OutputPeriodList);
                                    timeTakenPeriodcBox.dataSource.query();
                                    timeTakenPeriodcBox.select(0);
                                    $scope.routineDetailsSetupObj.OUTPUT_CAPACITY = oRes.data[o].INDEX_TIME_REQUIRED;
                                    $scope.routineDetailsSetupObj.OUTPUT_IN_PERIOD = oRes.data[o].PERIOD_CODE;


                                    

                                }

                            } else {
                                alert("no data");
                            }


                        });

                    }
                    $('#routineDetailSetup').toggle();
                } else {
                    DisplayBarNotificationMessage("Process created successfully");
                    $scope.IsCategoryProcessRoutineEdit = false;
                    setTimeout(function () {
                        location.reload(true);
                    }, 2000);
                }
            }
        });

    };

    $scope.SaveProcessSetup = function () {

        var saveModel = {
            "IN_ENGLISH": $scope.processCategoryRoutineObj.IN_ENGLISH,
            "IN_NEPALI": $scope.processCategoryRoutineObj.IN_NEPALI,
            "PROCESS_CODE": $scope.processCategoryRoutineObj.PROCESS_CODE,
            "REMARK": $scope.processCategoryRoutineObj.REMARK,
            "PROCESS_FLAG": $scope.selectedProcessFlag,
            "PRIORITY_NUMBER": $scope.processCategoryRoutineObj.PRIORITY_NUMBER,
            "LOCATION": $scope.selectedLocation,
            "PROCESS_TYPE": $scope.selectedProcessType,
            "ROOT_UNDER": $scope.selectedProcessForDDL

        };

        var saveResult = processSetupBomService.SaveProcessSetup(saveModel);
        saveResult.then(function (res) {

            if (res.data.MESSAGE === "Successful") {

                if (res.data.PROCESS_FLAG === "ROUTINE") {
                    $scope.isRoutineDetail = true;
                    $scope.routineDetailsSetupObj.BELONGS = $scope.selectedParentProcessDesc;
                    $scope.routineDetailsSetupObj.BELONGS_CODE = $scope.processCategoryRoutineObj.ROOT_UNDER;
                    $scope.routineDetailsSetupObj.ROUTINE_NAME = $scope.processCategoryRoutineObj.IN_ENGLISH;
                    $scope.routineDetailsSetupObj.ROUTINE_CODE = $scope.processCategoryRoutineObj.PROCESS_CODE;
                    DisplayBarNotificationMessage("Process created successfully");

                    var billOfMatGrid = $("#billOfMatGrid").data('kendoGrid');
                    billOfMatGrid.dataSource.transport.options.read.url = window.location.protocol + "//" + window.location.host + "/ProcessSetupBomApi/GetBillOfMaterialList?processCode=" + $scope.processCategoryRoutineObj.PROCESS_CODE;
                    billOfMatGrid.dataSource.read();

                    var billOfOutMatGrid = $("#outputMatGrid").data('kendoGrid');
                    billOfOutMatGrid.dataSource.transport.options.read.url = window.location.protocol + "//" + window.location.host + "/ProcessSetupBomApi/GetOutputMaterialList?processCode=" + $scope.processCategoryRoutineObj.PROCESS_CODE;
                    billOfOutMatGrid.dataSource.read();

                    $('#addNewProcessCategoryModel').toggle();
                    $('#routineDetailSetup').toggle();
                } else {
                    DisplayBarNotificationMessage("Process created successfully");
                    setTimeout(function () {
                        location.reload(true);
                    }, 2000);
                }
            }
        });

    };

    /* Process Category Main Page End */

    //------------------------------------------------------------------------------------------------------------------------------------------------------

    /* Routine Detail Setup Page Start */

    $scope.newProcessSetup = function () {
        debugger;
        $scope.IsCategoryProcessRoutineEdit = false;
        if ($scope.isSelectedCatIsProcess) {
            $scope.selectedProcessFlag = "ROUTINE";
            $('#addNewProcessModel').toggle();

        } else if (selectedItemObj.PROCESS_CODE) {
            $scope.selectedProcessFlag = "PROCESS";
            $('#addNewProcessModel').toggle();
        } else {
            $scope.selectedProcessFlag = "CATEGORY";
            $('#addNewProcessCategoryModel').toggle();
            $('#catRadio').attr('checked', true);

            // $('#routineDetailSetup').toggle();
        }

        //$('#routineDetailSetup').toggle();
    };

    $scope.editProcessRoutineSetup = function () {
        debugger;
        if (selectedItemObj.PROCESS_CODE) {
            $scope.selectedProcessFlag = "ROUTINE";
            $scope.IsCategoryProcessRoutineEdit = true;
            var tree = $("#selectedLocation").data("kendoDropDownList");
            tree.value($scope.selectedLocation);
            $("#cprradio *").prop('disabled', true);
            $("#selectedLocation").prop("disabled", true);  

            $('#addNewProcessModel').toggle();
        }
    };

    $scope.close = function () {
        $('#addNewProcessCategoryModel').toggle();
        //$('#routineDetailSetup').toggle();
    };

    $scope.closeProcessModel = function () {

        $('#addNewProcessModel').toggle();

        setTimeout(function () {
            location.reload(true);
        }, 2000);
    };

    $scope.newRoutineDetailSetup = function () {

        $('#routineDetailSetup').toggle();
    };

    $scope.closeRoutineDetailSetup = function () {
        $('#routineDetailSetup').toggle();
    };

    $scope.closeProcessMuUnit = function () {
        $('#inputUnitModal').toggle();
    };
    $scope.ChangeFlag = function (value) {
        $scope.selectedProcessFlag = value;
    };

    //Input masterial grid Options
    $scope.billOfMatGridOptions = {
        dataSource: {
            type: "json",
            transport: {
                read: "/api/ProcessSetupBomApi/GetBillOfMaterialList?processCode=" + 0
            },
            pageSize: 20
        },
        selectable: "single",
        resizable: true,
        scrollable: true,
        pageable: true,
        groupable: false,
        change: inputMatSelected,
        editable: "popup",
        dataBound: function (e) {
            $("#billOfMatGrid tbody tr").css("cursor", "pointer");

        },
        columns: [
            {
                field: "MODEL_INFO", hidden: true
            },
            {
                field: "PROCESS_EDESC",
                title: "SKU ITEMS"
            },
            {
                field: "QUANTITY",
                title: "Quantity"
            },
            {
                field: "MU_CODE",
                title: "Unit"
            },
            {
                field: "ITEM_EDESC",
                title: "Item Code"
            },
            {
                field: "REMARK",
                title: "Remark"
            }
        ]
    };

    $scope.outputMatGridOptions = {
        dataSource: {
            type: "json",
            transport: {
                read: "/api/ProcessSetupBomApi/GetOutputMaterialList?processCode=" + 0
            },
            pageSize: 20
        },
        selectable: "single",
        resizable: true,
        scrollable: true,
        pageable: true,
        groupable: false,
        change: outputMatSelected,
        dataBound: function (e) {
            //var insertedRow = this.dataSource.view();
            //for (var or = 0; or < insertedRow.length; or++) {
            //    $scope.outPutMaterialSetupObj.UNIT = insertedRow[or].UNIT_EDESC;
            //    $scope.outPutMaterialSetupObj.UNIT_CODE = insertedRow[or].UNIT_CODE;
            //}
            $("#outputMatGrid tbody tr").css("cursor", "pointer");

        },
        columns: [
            {
                field: "MODEL_INFO", hidden: true
            },
            //{
            //    field: "PROCESS_CODE", hidden: true
            //},
            {
                // field: "SKU_FLAG",
                field: "PROCESS_EDESC",
                title: "SKU ITEMS"
            },
            {
                field: "QUANTITY",
                title: "Quantity"
            },
            {
                //field: "MU_CODE",
                field: "MU_CODE",
                title: "Unit"
            },
            {
                field: "OUT_PUT",
                title: "Output"
            },
            {
                field: "VALUATION_FLAG",
                title: "Valuation Flag"
            },
            {
                // field: "ITEM_CODE",
                field: "ITEM_EDESC",
                title: "Item Code"
            },
            {
                // field: "REMARKS",
                field: "REMARK",
                title: "Remark"
            }
        ]
    };

    function outputMatSelected() {
        $('#outPutMatEdit').attr("disabled", false);
        $('#outputMatDelete').attr("disabled", false);
    }

    $("#input_index_item").kendoComboBox({
        dataTextField: "ITEM_NAME",
        dataValueField: "ITEM_CODE",
        dataSource: $scope.inputEntry,
        filter: "contains",
        suggest: true,
        index: 2,
        select: function (e) {
            // console.log("input item index: " + JSON.stringify(e.dataItem));
            $scope.routineDetailsSetupObj.INPUT_INDEX_ITEM = e.dataItem.ITEM_CODE;
        }

    });

    $("#output_index_item").kendoComboBox({
        dataTextField: "ITEM_NAME",
        dataValueField: "ITEM_CODE",
        dataSource: $scope.inputEntry,
        filter: "contains",
        suggest: true,
        index: 2,
        select: function (e) {
            // console.log("Output item index: " + JSON.stringify(e.dataItem));
            $scope.routineDetailsSetupObj.OUTPUT_INDEX_ITEM = e.dataItem.ITEM_CODE;
        }
    });

    $("#production_unit").kendoComboBox({
        dataTextField: "UNIT",
        dataValueField: "UNIT",
        dataSource: $scope.inputEntry,
        filter: "contains",
        suggest: true,
        index: 2,
        select: function (e) {
            // console.log("Output item index: " + JSON.stringify(e.dataItem));
            $scope.routineDetailsSetupObj.INPUT_UNIT_CODE = e.dataItem.UNIT;
        }
    });

    $("#product_in_period").kendoComboBox({
        dataTextField: "PERIOD_EDESC",
        dataValueField: "PERIOD_CODE",
        dataSource: $scope.inputEntry,
        filter: "contains",
        suggest: true,
        index: 2,
        select: function (e) {
            // console.log("Output item index: " + JSON.stringify(e.dataItem));
            $scope.routineDetailsSetupObj.INPUT_IN_PERIOD = e.dataItem.PERIOD_CODE;
        }
    });

    $("#time_taken_unit").kendoComboBox({
        dataTextField: "UNIT",
        dataValueField: "UNIT",
        dataSource: $scope.inputEntry,
        filter: "contains",
        suggest: true,
        index: 2,
        select: function (e) {
            // console.log("Output item index: " + JSON.stringify(e.dataItem));
            $scope.routineDetailsSetupObj.OUTPUT_UNIT_CODE = e.dataItem.UNIT;
        }
    });

    $("#time_taken_in_period").kendoComboBox({
        dataTextField: "PERIOD_EDESC",
        dataValueField: "PERIOD_CODE",
        dataSource: $scope.inputEntry,
        filter: "contains",
        suggest: true,
        index: 2,
        select: function (e) {
            // console.log("Output item index: " + JSON.stringify(e.dataItem));
            $scope.routineDetailsSetupObj.OUTPUT_IN_PERIOD = e.dataItem.PERIOD_CODE;
        }
    });

    $("#inputMaterialsUnit").kendoComboBox({
        dataTextField: "MU_EDESC",
        dataValueField: "MU_CODE",
        dataSource: $scope.inputEntry,
        filter: "contains",
        suggest: true,
        index: 2
    });

    $("#outputMaterialsUnit").kendoComboBox({
        dataTextField: "MU_EDESC",
        dataValueField: "MU_CODE",
        dataSource: $scope.inputEntry,
        filter: "contains",
        suggest: true,
        index: 2
    });



    /* Routine Detail Setup Page End */

    //-------------------------------------------------------------------------------------------------------------------------------------------------------

    /* Input Material Setup Page Start */
    $scope.openNewInputMaterialSetup = function () {
        $scope.inputDialogAction = "Finish";
        $scope.inputMaterialSetupObj.PROCESS = $scope.processCategoryRoutineObj.IN_ENGLISH;
        $scope.inputMaterialSetupObj.PROCESS_CODE = $scope.processCategoryRoutineObj.PROCESS_CODE;
        $('#inputMaterialSetupModel').toggle();
    };

    $scope.inputEntry = {};
    function inputMatSelected(e) {
        $('#inputMatEdit').attr("disabled", false);
        $('#inputMatDelete').attr("disabled", false);
        //var rows = e.sender.select();
        //rows.each(function (e) {
        //    var grid = $("#billOfMatGrid").data("kendoGrid");
        //    var dataItem = grid.dataItem(this);
        //    $scope.inputEntry = dataItem;
        //    var idex = $("#input_index_item").data('kendoComboBox');
        //    idex.dataSource.data([$scope.inputEntry]);
        //    idex.dataSource.query();
        //});

        //console.log("$scope.inputEntry: " + JSON.stringify($scope.inputEntry));
    }

    $scope.deleteInputMaterialSetup = function () {
        var grid = $("#billOfMatGrid").data("kendoGrid");

        // finding all the selected rows
        var rows = grid.select();
        rows.each(function (index, row) {
            // reading each selected item
            var selectedItem = grid.dataItem(row);

            console.log("selected item to delete: " + JSON.stringify(selectedItem));
            for (var i = 0; i < $scope.InputIndexItemList.length; i++) {
                if ($scope.InputIndexItemList[i].ITEM_CODE === selectedItem.ITEM_CODE) {
                    $scope.InputIndexItemList.pop($scope.InputIndexItemList[i]);
                }
            }

            for (var j = 0; j < $scope.ProductionUnitList.length; j++) {
                if ($scope.ProductionUnitList[j].UNIT === selectedItem.UNIT) {
                    $scope.ProductionUnitList.pop($scope.ProductionUnitList[j]);
                }
            }

            // finally removing selected item from grid data source
            grid.dataSource.remove(selectedItem);

            //remove item from item grid
            var inputGrid = $("#inputIndexItemGrid").data("kendoGrid");
            inputGrid.dataSource.data($scope.InputIndexItemList);
            inputGrid.dataSource.query();


            var idex = $("#input_index_item").data('kendoComboBox');
            //idex.dataSource.data([$scope.inputMaterialSetupObj]);
            idex.dataSource.data($scope.InputIndexItemList);
            idex.dataSource.query();

            var prodUnit = $("#production_unit").data("kendoComboBox");
            prodUnit.dataSource.data($scope.ProductionUnitList);
            prodUnit.dataSource.query();

            var prodUnitGrid = $("#productionUnitGrid").data("kendoGrid");
            prodUnitGrid.dataSource.data($scope.ProductionUnitList);
            prodUnitGrid.dataSource.query();

        });
    };

    $scope.deleteOutputMaterialSetup = function () {
        var grid = $("#outputMatGrid").data("kendoGrid");
        var rows = grid.select();
        rows.each(function (index, row) {
            var selectedItem = grid.dataItem(row);

            for (var i = 0; i < $scope.OutputIndexItemList.length; i++) {
                if ($scope.OutputIndexItemList[i].ITEM_CODE === selectedItem.ITEM_CODE) {
                    $scope.OutputIndexItemList.pop($scope.OutputIndexItemList[i]);
                }
            }

            for (var j = 0; j < $scope.TimeTakenUnitList.length; j++) {
                if ($scope.TimeTakenUnitList[j].UNIT === selectedItem.UNIT) {
                    $scope.TimeTakenUnitList.pop($scope.TimeTakenUnitList[j]);
                }
            }

            grid.dataSource.remove(selectedItem);

            var odex = $("#output_index_item").data('kendoComboBox');
            odex.dataSource.data($scope.OutputIndexItemList);
            odex.dataSource.query();

            var inputGrid = $("#outPutIndexItemGrid").data("kendoGrid");
            inputGrid.dataSource.data($scope.OutputIndexItemList);
            inputGrid.dataSource.query();


            var timeTakenUnit = $("#time_taken_unit").data("kendoComboBox");
            timeTakenUnit.dataSource.data($scope.TimeTakenUnitList);
            timeTakenUnit.dataSource.query();

            var timeTakenUnitGrid = $("#timeTakenUnitGrid").data("kendoGrid");
            timeTakenUnitGrid.dataSource.data($scope.TimeTakenUnitList);
            timeTakenUnitGrid.dataSource.query();
        });
    };

    $scope.editInputMaterialSetup = function () {
        var inputMatGrid = $("#billOfMatGrid").data("kendoGrid");
        var dataRows = inputMatGrid.items();
        var selected = inputMatGrid.select();
        $scope.InputRowIndex = dataRows.index(selected);
        if (selected.length === 0) {
            alert('No record selected');
        } else {

            for (var i = 0; i < selected.length; i++) {
                console.log("Selected Inut Grid: ================>>" + JSON.stringify(inputMatGrid.dataItem(selected[i])));
                $scope.inputMaterialSetupObj.PROCESS = inputMatGrid.dataItem(selected[i]).PROCESS_EDESC;
                $scope.inputMaterialSetupObj.PROCESS_CODE = inputMatGrid.dataItem(selected[i]).PROCESS_CODE;
                $scope.inputMaterialSetupObj.ITEM_NAME = inputMatGrid.dataItem(selected[i]).ITEM_NAME;
                $scope.inputMaterialSetupObj.ITEM_CODE = inputMatGrid.dataItem(selected[i]).ITEM_CODE;
                $scope.inputMaterialSetupObj.ITEM_EDESC = inputMatGrid.dataItem(selected[i]).ITEM_EDESC;
                $scope.inputMaterialSetupObj.QUANTITY = inputMatGrid.dataItem(selected[i]).QUANTITY;
                $scope.inputMaterialSetupObj.UNIT = inputMatGrid.dataItem(selected[i]).UNIT;
                $scope.inputMaterialSetupObj.UNIT_CODE = inputMatGrid.dataItem(selected[i]).UNIT_CODE;
                $scope.inputMaterialSetupObj.MU_CODE = inputMatGrid.dataItem(selected[i]).MU_CODE;
                $scope.inputMaterialSetupObj.REMARK = inputMatGrid.dataItem(selected[i]).REMARK;


                //$scope.InputIndexItemList.push({
                //    "ITEM_CODE": inputMatGrid.dataItem(selected[i]).ITEM_CODE,
                //    "ITEM_EDESC": inputMatGrid.dataItem(selected[i]).ITEM_EDESC
                //});

                var inputMatItemAutoComplete = $("#inputMaterialsItem").data("kendoAutoComplete");
                inputMatItemAutoComplete.value(inputMatGrid.dataItem(selected[i]).ITEM_EDESC);
                inputMatItemAutoComplete.trigger("change");

                $scope.InputItemUnitList.push({
                    "MU_CODE": inputMatGrid.dataItem(selected[i]).MU_CODE,
                    "MU_EDESC": inputMatGrid.dataItem(selected[i]).MU_EDESC
                });

                var inputUnitPerItem = $("#inputMaterialsUnit").data("kendoComboBox");
                inputUnitPerItem.dataSource.data($scope.InputItemUnitList);
                inputUnitPerItem.dataSource.query();
                inputUnitPerItem.select(function (dataItems) {
                    return dataItems.MU_EDESC = inputMatGrid.dataItem(selected[i]).MU_EDESC;
                });


            }

        }
        $scope.inputDialogAction = "Update";
        $('#inputMaterialSetupModel').toggle();
    };

    $scope.editOutputMaterialSetup = function () {
        var outPutMatGrid = $("#outputMatGrid").data("kendoGrid");
        var selectedOutPut = outPutMatGrid.select();
        var dataRows = outPutMatGrid.items();
        $scope.OutputRowIndex = dataRows.index(selectedOutPut);
        if (selectedOutPut.length === 0) {
            alert('No record selected');
        } else {

            for (var i = 0; i < selectedOutPut.length; i++) {
                $scope.outPutMaterialSetupObj.PROCESS = outPutMatGrid.dataItem(selectedOutPut[i]).PROCESS_EDESC;
                $scope.outPutMaterialSetupObj.PROCESS_CODE = outPutMatGrid.dataItem(selectedOutPut[i]).PROCESS_CODE;
                $scope.outPutMaterialSetupObj.ITEM_NAME = outPutMatGrid.dataItem(selectedOutPut[i]).ITEM_NAME;
                $scope.outPutMaterialSetupObj.ITEM_CODE = outPutMatGrid.dataItem(selectedOutPut[i]).ITEM_CODE;
                $scope.outPutMaterialSetupObj.ITEM_EDESC = outPutMatGrid.dataItem(selectedOutPut[i]).ITEM_EDESC;
                $scope.outPutMaterialSetupObj.QUANTITY = outPutMatGrid.dataItem(selectedOutPut[i]).QUANTITY;
                $scope.outPutMaterialSetupObj.UNIT = outPutMatGrid.dataItem(selectedOutPut[i]).UNIT;
                $scope.outPutMaterialSetupObj.UNIT_CODE = outPutMatGrid.dataItem(selectedOutPut[i]).UNIT_CODE;
                $scope.outPutMaterialSetupObj.MU_CODE = outPutMatGrid.dataItem(selectedOutPut[i]).MU_CODE;
                $scope.outPutMaterialSetupObj.VALUATION_FLAG = outPutMatGrid.dataItem(selectedOutPut[i]).VALUATION_FLAG === "Y" ? true : false;
                $scope.outPutMaterialSetupObj.OUTPUT = outPutMatGrid.dataItem(selectedOutPut[i]).OUT_PUT;
                $scope.outPutMaterialSetupObj.OUT_PUT = outPutMatGrid.dataItem(selectedOutPut[i]).OUT_PUT;
                $scope.outPutMaterialSetupObj.REMARK = outPutMatGrid.dataItem(selectedOutPut[i]).REMARK;
                //  $scope.$apply();


                var outputMatItemAutoComplete = $("#outPutMaterialsItem").data("kendoAutoComplete");
                outputMatItemAutoComplete.value(outPutMatGrid.dataItem(selectedOutPut[i]).ITEM_EDESC);
                outputMatItemAutoComplete.trigger("change");

                $scope.OutputItemUnitList.push({
                    "MU_CODE": outPutMatGrid.dataItem(selectedOutPut[i]).MU_CODE,
                    "MU_EDESC": outPutMatGrid.dataItem(selectedOutPut[i]).MU_EDESC
                });

                var outputUnitPerItem = $("#outputMaterialsUnit").data("kendoComboBox");
                outputUnitPerItem.dataSource.data($scope.OutputItemUnitList);
                outputUnitPerItem.dataSource.query();
                outputUnitPerItem.select(function (dataItems) {
                    return dataItems.MU_EDESC = outPutMatGrid.dataItem(selectedOutPut[i]).MU_EDESC;
                });

            }
        }
        $scope.outPutDialogAction = "Update";
        $('#outputMaterialSetupModel').toggle();
    };

    $scope.openNewOutputMaterialSetup = function () {
        $scope.outPutDialogAction = "Finish";
        var inputCount = $("#inputIndexItemGrid").data("kendoGrid").dataSource.total();
        if ($scope.IsInputCreated && inputCount > 0 ) {
            $scope.outPutMaterialSetupObj.PROCESS = $scope.processCategoryRoutineObj.IN_ENGLISH;
            $scope.outPutMaterialSetupObj.PROCESS_CODE = $scope.processCategoryRoutineObj.PROCESS_CODE;
            $('#outputMaterialSetupModel').toggle();
        } else {
            displayPopupNotification("No input materials are available to create output", "error");
            return;
        }

    };

    $scope.closeInputMaterialSetupModel = function () {
        $('#inputMaterialSetupModel').toggle();
    };

    $scope.closeOutputMaterialSetupModel = function () {
        $('#outputMaterialSetupModel').toggle();
    };

    $scope.InputIndexItemList = [];
    $scope.ProductionUnitList = [];
    $scope.TimeTakenUnitList = [];
    $scope.OutputIndexItemList = [];
    $scope.InputPeriodList = [];
    $scope.OutputPeriodList = [];
    $scope.IsInputCreated = false;
    $scope.SaveInputMaterialSetup = function () {

        $scope.inputMaterialSetupObj.MODAL_INFO = "INPUT";

       

        var billOfMatGrid = $("#billOfMatGrid").data("kendoGrid");
        if ($scope.inputDialogAction === "Finish") {
          
            var tempObj = {

                "MODAL_INFO": $scope.inputMaterialSetupObj.MODAL_INFO,
                "PROCESS_CODE": $scope.inputMaterialSetupObj.PROCESS_CODE,
                "PROCESS_EDESC": $scope.inputMaterialSetupObj.PROCESS,
                "QUANTITY": $scope.inputMaterialSetupObj.QUANTITY,
                "ITEM_CODE": $scope.inputMaterialSetupObj.ITEM_CODE,
                "ITEM_EDESC": $scope.inputMaterialSetupObj.ITEM_NAME,
                "MU_EDESC": $scope.inputMaterialSetupObj.UNIT,
                "MU_CODE": $scope.inputMaterialSetupObj.UNIT_CODE,
                "UNIT_CODE": $scope.inputMaterialSetupObj.UNIT_CODE,
                "REMARK": $scope.inputMaterialSetupObj.REMARK

            };
            billOfMatGrid.dataSource.add(tempObj);
            $("#btnSaveRoutineDetail").show();

        }
        else {

            var inputMatItemAutoComplete = $("#inputMaterialsItem").data("kendoAutoComplete");

            var rowToUpdate = billOfMatGrid.dataSource.at($scope.InputRowIndex);
            rowToUpdate.set("MODAL_INFO", $scope.inputMaterialSetupObj.MODAL_INFO);
            rowToUpdate.set("PROCESS_CODE", $scope.inputMaterialSetupObj.PROCESS_CODE);
            rowToUpdate.set("PROCESS_EDESC", $scope.inputMaterialSetupObj.PROCESS);
            rowToUpdate.set("QUANTITY", $scope.inputMaterialSetupObj.QUANTITY);
            rowToUpdate.set("ITEM_EDESC", inputMatItemAutoComplete.value());
            rowToUpdate.set("ITEM_NAME", $scope.inputMaterialSetupObj.ITEM_NAME);
            rowToUpdate.set("ITEM_CODE", $scope.inputMaterialSetupObj.ITEM_CODE);
            rowToUpdate.set("MU_EDESC", $scope.inputMaterialSetupObj.UNIT);
            rowToUpdate.set("MU_CODE", $scope.inputMaterialSetupObj.UNIT_CODE);
            rowToUpdate.set("UNIT_CODE", $scope.inputMaterialSetupObj.UNIT_CODE);
            rowToUpdate.set("REMARK", $scope.inputMaterialSetupObj.REMARK);
        }

        var idex = $("#input_index_item").data('kendoComboBox');
        var prodUnit = $("#production_unit").data("kendoComboBox");



        if (idex.ITEM_CODE !== $scope.inputMaterialSetupObj.ITEM_CODE) {

            $scope.InputIndexItemList.push({
                "ITEM_CODE": $scope.inputMaterialSetupObj.ITEM_CODE,
                "ITEM_NAME": $scope.inputMaterialSetupObj.ITEM_NAME,
                "ITEM_EDESC": $scope.inputMaterialSetupObj.ITEM_EDESC
            });

            console.log("inserted Item======================>>>" + JSON.stringify($scope.InputIndexItemList));

            idex.dataSource.data($scope.InputIndexItemList);
            idex.dataSource.query();
            idex.select(0);
            $scope.routineDetailsSetupObj.INPUT_INDEX_ITEM = $scope.inputMaterialSetupObj.ITEM_CODE;

            var inputGrid = $("#inputIndexItemGrid").data("kendoGrid");
            inputGrid.dataSource.data($scope.InputIndexItemList);
            inputGrid.dataSource.query();


        }

        if (prodUnit.UNIT_CODE !== $scope.inputMaterialSetupObj.UNIT_CODE) {

            $scope.ProductionUnitList.push({
                "UNIT": $scope.inputMaterialSetupObj.UNIT
            });

            prodUnit.dataSource.data($scope.ProductionUnitList);
            prodUnit.dataSource.query();
            prodUnit.select(0);

            var prodUnitGrid = $("#productionUnitGrid").data("kendoGrid");
            prodUnitGrid.dataSource.data($scope.ProductionUnitList);
            prodUnitGrid.dataSource.query();
        }

        $('#inputMaterialSetupModel').toggle();

        $scope.inputMaterialSetupObj = {};

        $("#inputMaterialsItem").data("kendoAutoComplete").value("");
        $("#inputMaterialsUnit").data("kendoComboBox").value("");

        $scope.IsInputCreated = true;

    };

    $scope.clearInputMaterialDialog = function () {

    };

    $scope.SaveOutputMaterialSetup = function () {

        $scope.inputMaterialSetupObj.MODAL_INFO = "OUTPUT";

        var outputMatGrid = $("#outputMatGrid").data("kendoGrid");

        

        if ($scope.outPutDialogAction === 'Finish') {

            var tempObj = {
                "MODAL_INFO": $scope.outPutMaterialSetupObj.MODAL_INFO,
                "PROCESS_CODE": $scope.outPutMaterialSetupObj.PROCESS_CODE,
                "PROCESS_EDESC": $scope.outPutMaterialSetupObj.PROCESS,
                "QUANTITY": $scope.outPutMaterialSetupObj.QUANTITY,
                "ITEM_EDESC": $scope.outPutMaterialSetupObj.ITEM_NAME,
                "ITEM_CODE": $scope.outPutMaterialSetupObj.ITEM_CODE,
                "MU_EDESC": $scope.outPutMaterialSetupObj.UNIT,
                "MU_CODE": $scope.outPutMaterialSetupObj.UNIT_CODE,
                "UNIT_CODE": $scope.outPutMaterialSetupObj.UNIT_CODE,
                "REMARK": $scope.outPutMaterialSetupObj.REMARK,
                "OUT_PUT": $scope.outPutMaterialSetupObj.OUTPUT,
                "OUTPUT": $scope.outPutMaterialSetupObj.OUTPUT,
                "VALUATION_FLAG": $scope.outPutMaterialSetupObj.VALUATION_FLAG ? "Y" : "N"
            };
            outputMatGrid.dataSource.add(tempObj);

        }
        else {

            var outputItemAutoComplete = $("#outPutMaterialsItem").data("kendoAutoComplete");

            var rowToEdit = outputMatGrid.dataSource.at($scope.OutputRowIndex);
            //console.log("rowToEdit============>>" + JSON.stringify(rowToEdit));
            rowToEdit.set("MODAL_INFO", $scope.outPutMaterialSetupObj.MODAL_INFO);
            rowToEdit.set("PROCESS_CODE", $scope.outPutMaterialSetupObj.PROCESS_CODE);
            rowToEdit.set("PROCESS_EDESC", $scope.outPutMaterialSetupObj.PROCESS);
            rowToEdit.set("QUANTITY", $scope.outPutMaterialSetupObj.QUANTITY);
            rowToEdit.set("MU_EDESC", $scope.outPutMaterialSetupObj.UNIT);
            rowToEdit.set("MU_CODE", $scope.outPutMaterialSetupObj.UNIT_CODE);
            rowToEdit.set("UNIT_CODE", $scope.outPutMaterialSetupObj.UNIT_CODE);
            rowToEdit.set("OUT_PUT", $scope.outPutMaterialSetupObj.OUT_PUT);
            rowToEdit.set("OUT_PUT", $scope.outPutMaterialSetupObj.OUTPUT);
            rowToEdit.set("OUTPUT", $scope.outPutMaterialSetupObj.OUTPUT);
            rowToEdit.set("VALUATION_FLAG", $scope.outPutMaterialSetupObj.VALUATION_FLAG ? "Y" : "N");
            rowToEdit.set("ITEM_NAME", $scope.outPutMaterialSetupObj.ITEM_NAME);
            rowToEdit.set("ITEM_EDESC", outputItemAutoComplete.value());
            rowToEdit.set("ITEM_CODE", $scope.outPutMaterialSetupObj.ITEM_CODE);
            rowToEdit.set("REMARK", $scope.outPutMaterialSetupObj.REMARK);
        }

        var odex = $("#output_index_item").data('kendoComboBox');
        var timeTakenUnit = $("#time_taken_unit").data("kendoComboBox");
       

        if (odex.ITEM_CODE !== $scope.outPutMaterialSetupObj.ITEM_CODE) {
            console.log("odexItemCode====>>" + odex.ITEM_CODE);
            console.log("$scope.outPutMaterialSetupObj.ITEM_CODE====>>" + $scope.outPutMaterialSetupObj.ITEM_CODE);
            $scope.OutputIndexItemList.push({
                "ITEM_CODE": $scope.outPutMaterialSetupObj.ITEM_CODE,
                "ITEM_NAME": $scope.outPutMaterialSetupObj.ITEM_NAME,
                "ITEM_EDESC": $scope.outPutMaterialSetupObj.ITEM_EDESC
            });

            //Item comboBox
            odex.dataSource.data($scope.OutputIndexItemList);
            odex.dataSource.query();
            odex.select(0);
            $scope.routineDetailsSetupObj.OUTPUT_INDEX_ITEM = $scope.outPutMaterialSetupObj.ITEM_CODE;
            //Item Grid
            var inputGrid = $("#outPutIndexItemGrid").data("kendoGrid");
            inputGrid.dataSource.data($scope.OutputIndexItemList);
            inputGrid.dataSource.query();
        }

        if (timeTakenUnit.UNIT_CODE !== $scope.outPutMaterialSetupObj.UNIT_CODE) {

            $scope.TimeTakenUnitList.push({
                "UNIT": $scope.outPutMaterialSetupObj.UNIT,
                "UNIT_CODE": $scope.outPutMaterialSetupObj.UNIT_CODE
            });

            timeTakenUnit.dataSource.data($scope.TimeTakenUnitList);
            timeTakenUnit.dataSource.query();
            timeTakenUnit.select(0);

            var timeTakenUnitGrid = $("#timeTakenUnitGrid").data("kendoGrid");
            timeTakenUnitGrid.dataSource.data($scope.TimeTakenUnitList);
            timeTakenUnitGrid.dataSource.query();

        }

        $('#outputMaterialSetupModel').toggle();

        $scope.outPutMaterialSetupObj = {};
       

        $("#outPutMaterialsItem").data("kendoAutoComplete").value("");
        $("#outputMaterialsUnit").data("kendoComboBox").value("");

    };

    $scope.SaveRoutineDetailSetup = function () {
        debugger;
        var saveModal = {
            "RoutineDetail": $scope.routineDetailsSetupObj,
            "InputModel": $("#billOfMatGrid").data().kendoGrid.dataSource.view(),
            "OutputModel": $("#outputMatGrid").data().kendoGrid.dataSource.view(),
            "PROCESS_FLAG": $scope.selectedProcessFlag
        };
        if ($scope.routineDetailsSetupObj.INPUT_IN_PERIOD == null || $scope.routineDetailsSetupObj.INPUT_IN_PERIOD == 'undefined' || $scope.routineDetailsSetupObj.INPUT_IN_PERIOD == undefined || $scope.routineDetailsSetupObj.INPUT_IN_PERIOD == "") {            
            displayPopupNotification("Input period must be selected.", "warning");
            return;
        }
        if ($scope.routineDetailsSetupObj.OUTPUT_IN_PERIOD == null || $scope.routineDetailsSetupObj.OUTPUT_IN_PERIOD == 'undefined' || $scope.routineDetailsSetupObj.OUTPUT_IN_PERIOD == undefined || $scope.routineDetailsSetupObj.OUTPUT_IN_PERIOD == "") {
            displayPopupNotification("Output period must be selected.", "warning");
            return;
        }
        console.log("saveModal=================>>>" + JSON.stringify(saveModal));
        console.log("InputModel=================>>>" + JSON.stringify($("#billOfMatGrid").data().kendoGrid.dataSource.view()));
        console.log("OutputModel=================>>>" + JSON.stringify($("#outputMatGrid").data().kendoGrid.dataSource.view()));

        var routineResult = processSetupBomService.SaveRoutineDetailSetup(saveModal);
        routineResult.then(function (res) {
            console.log("res:::: " + JSON.stringify(res));
            if (res.data === "Successful") {
                DisplayBarNotificationMessage("Routine detail saved successfully");
                setTimeout(function () {
                    location.reload(true);
                }, 3000);
            }
        });

    };

    /* Input Material Setup Page End */

    //--------------------------------------------------------------------------------------------------------------------------------------------------------

});

DTModule.service('processSetupBomService', function ($http) {

    this.getProcessType = function () {

        var processRes = $http({
            method: "GET",
            url: "/api/SetupApi/getProcessType",
            dataType: "json"
        });
        return processRes;
    };

    this.getAllCategoryProcessRoutineTree = function () {

        var cprTree = $http({
            method: "GET",
            url: "/api/ProcessSetupBomApi/GetAllProcessCategoryRoutine",
            dataType: "json"
        });
        return cprTree;
    };

    this.SaveProcessCategorySetup = function (parameter) {

        var processCatSetup = $http({
            method: "POST",
            url: "/api/ProcessSetupBomApi/SaveProcessCategoryRoutine",
            data: JSON.stringify(parameter),
            dataType: "json"
        });

        return processCatSetup;
    };

    this.SaveProcessSetup = function (saveParam) {
        var allProcessSetup = $http({
            method: "POST",
            url: "/api/ProcessSetupBomApi/SaveProcessSetup",
            data: JSON.stringify(saveParam),
            dataType: "JSON"
        });
        return allProcessSetup;
    };

    this.SaveInputOutMaterialSetup = function (inputOutputParam) {

        var materialResponse = $http({
            method: "POST",
            url: "/api/ProcessSetupBomApi/SaveInputOutMaterial",
            data: JSON.stringify(inputOutputParam),
            dataType: "JSON"
        });

        return materialResponse;

    };

    this.SaveRoutineDetailSetup = function (routineParam) {

        var routineResponse = $http({
            method: "POST",
            url: "/api/ProcessSetupBomApi/SaveRoutineDetailSetup",
            data: JSON.stringify(routineParam),
            dataType: "JSON"

        });

        return routineResponse;
    };

    this.GetAllLocation = function () {
        var allLoc = $http({
            method: "GET",
            url: "/api/ProcessSetupBomApi/GetAllLocation",
            dataType: "JSON"
        });

        return allLoc;
    };

    this.GetAllItemForInputOutput = function () {
        var allItem = $http({
            method: "GET",
            url: "/api/ProcessSetupBomApi/GetAllItemForInputOutput",
            dataType: "JSON"
        });

        return allItem;
    };

    this.getProcessMuCode = function () {

    };

    this.GetChildProcessCode = function (processCode) {
        var cpCode = $http({
            method: "GET",
            url: "/api/ProcessSetupBomApi/GetChildProcessCode?processCode=" + processCode,
            dataType: "JSON"
        });

        return cpCode;
    };

    this.GetChildProcessDetail = function (processCode) {
        var cpDetail = $http({
            method: "GET",
            url: "/api/ProcessSetupBomApi/GetChildProcessDetail?processCode=" + processCode,
            dataType: "JSON"
        });
        return cpDetail;
    };

    this.GetInputRoutineDetail = function (processCode) {

        var inputRoutineDetail = $http({
            method: "GET",
            url: "/api/ProcessSetupBomApi/GetBillOfMaterialList?processCode=" + processCode,
            dataType: "JSON"
        });

        return inputRoutineDetail;
    };

    this.GetOutputRoutineDetail = function (processCode) {

        var outputRoutineDetail = $http({
            method: "GET",
            url: "/api/ProcessSetupBomApi/GetOutputMaterialList?processCode=" + processCode,
            dataType: "JSON"
        });
        return outputRoutineDetail;
    };

});