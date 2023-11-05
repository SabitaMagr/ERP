
distributionModule.controller('DistributorCtrl', function ($scope, $http, distributorService, $routeParams) {

    $scope.isViewLoading = false;
    $scope.$on('$routeChangeStart', function () {
        $scope.isViewLoading = true;
    });
    $scope.$on('$routeChangeSuccess', function () {
        $scope.isViewLoading = false;
    });
    $scope.$on('$routeChangeError', function () {
        $scope.isViewLoading = false;
    });
    $scope.showCustomerSelect = true;

    $scope.distGroupSelectOptions = {
        //close: function () {
        
        //    var selected = $("#distGroupSelect").data("kendoMultiSelect").dataItem();
        //    $scope.selectedGroup = typeof (selected) == 'undefined' ? [] : [String(selected.GROUPID)];
        //    //$scope.$apply();
        //},
        dataTextField: "GROUP_EDESC",
        dataValueField: "GROUPID",
        height: 600,
        valuePrimitive: true,
        maxSelectedItems: 1,
        headerTemplate: '<div class="col-md-offset-3"><strong>Group...</strong></div>',
        placeholder: "Select Group...",
        autoClose: false,
        select: function (e) {
            
        },
        dataBound: function (e) {
            var current = this.value();
            this._savedOld = current.slice(0);
            $("#" + e.sender.element[0].id + "_listbox").slimScroll({ 'height': '179px', 'scroll': 'scroll' });
        },
        dataSource: {
            transport: {
                read: {
                    url: window.location.protocol + "//" + window.location.host + "/api/Setup/getAllResellerGroups",
                    dataType: "json"
                }
            }
        }
    };
    $scope.dataSourceBrand = [];
    var productsDataSource = new kendo.data.DataSource({
        transport: {
            read: {
                //url: window.location.protocol + "//" + window.location.host + "/api/Distributor/GetDistributorItems",
                url: window.location.protocol + "//" + window.location.host + "/api/DistributionPurchase/GetAllItems",
                dataType: "json"
            }
        }
    });

    productsDataSource.fetch(function () {
        $scope.distBrandSelectOptions = {
            dataTextField: "BRAND_NAME",
            dataValueField: "BRAND_NAME",
            height: 600,
            valuePrimitive: true,
            headerTemplate: '<div class="col-md-offset-3"><strong>Brands...</strong></div>',
            placeholder: "Select Brands...",
            autoClose: false,
            dataBound: function (e) {
                $("#" + e.sender.element[0].id + "_listbox").slimScroll({ 'height': '179px', 'scroll': 'scroll' });
            },
            dataSource: new kendo.data.DataSource({
                data: _.uniq(this.data(), "BRAND_NAME"),
            }),
            change: function () {
                buildFilters(this.dataItems());
            }
        };
    });
    $scope.distItemsSelectOptions = {
        dataTextField: "ITEM_EDESC",
        dataValueField: "ITEM_CODE",
        height: 600,
        valuePrimitive: true,
        headerTemplate: '<div class="col-md-offset-3"><strong>Items...</strong></div>',
        placeholder: "Select Items...",
        autoClose: false,
        dataBound: function (e) {
            var current = this.value();
            this._savedOld = current.slice(0);
            $("#" + e.sender.element[0].id + "_listbox").slimScroll({ 'height': '179px', 'scroll': 'scroll' });
        },
        dataSource: productsDataSource

    };

    function buildFilters(dataItems) {
        var filters = [],
            length = dataItems.length,
            idx = 0, dataItem;
        if (length == 0) {
            $("#distItemsSelect").data("kendoMultiSelect").value("");
        }
        for (; idx < length; idx++) {
            dataItem = dataItems[idx];

            var data = $("#distItemsSelect").data("kendoMultiSelect").dataSource.data();
            var filterdata = _.filter(data, function (da) { return da.BRAND_NAME == dataItem.BRAND_NAME; });
            for (var i = 0; i < filterdata.length; i++) {
                filters.push(filterdata[i].ITEM_CODE);
            }

            $("#distItemsSelect").data("kendoMultiSelect").value(filters);

            //filters.push({
            //    field: "BRAND_NAME",
            //    operator: "eq",
            //    value: parseInt(dataItem.BRAND_NAME)
            //});
        }


    };
    $scope.pageName = "Add Distributor";
    $scope.saveAction = "Save";
    $scope.createPanel = false;
    reportConfig = GetReportSetting("DistributionSetup");

    //map
    var markersArray = [], map, marker;
    $scope.initialize = function () {
        var myLatlng = new google.maps.LatLng(27.70320076199206, 85.31524620117193);

        var mapOptions = {
            zoom: 10,
            center: myLatlng,
            mapTypeId: google.maps.MapTypeId.ROADMAP
        };

        map = new google.maps.Map(document.getElementById('map-canvas'), mapOptions);

        marker = new google.maps.Marker({
            map: map,
            position: myLatlng,
            title: 'Center',
            draggable: true
        });



        /***************** search box function *****************/
        // Create the search box and link it to the UI element.


        if (document.getElementById('mapSearchBox') == null)
            $("#distributor-map-panel").prepend('<div id="distributor-map-fullscreen"></div><input id="mapSearchBox" class="controls" type="text" placeholder="Search Box">');
        var input = document.getElementById('mapSearchBox');
        var searchBox = new google.maps.places.SearchBox(input);
        map.controls[google.maps.ControlPosition.TOP_LEFT].push(input);
        // Bias the SearchBox results towards current map's viewport.
        map.addListener('bounds_changed', function () {
            searchBox.setBounds(map.getBounds());
        });

        // Listen for the event fired when the user selects a prediction and retrieve
        // more details for that place.
        searchBox.addListener('places_changed', function () {
            var places = searchBox.getPlaces();
            // console.log(places);

            if (places.length == 0) {
                return;
            }

            if (!places[0].geometry) {
                console.log("Returned place contains no geometry");
                return;
            }

            var latlng = new google.maps.LatLng(places[0].geometry.location.lat(), places[0].geometry.location.lng());
            marker.setPosition(latlng);

            var bounds = new google.maps.LatLngBounds();

            if (places[0].geometry.viewport) {
                bounds.union(places[0].geometry.viewport);
            } else {
                bounds.extend(places[0].geometry.location);
            }
            map.fitBounds(bounds);
            $('#maplat').html(places[0].geometry.location.lat());
            $('#maplng').html(places[0].geometry.location.lng());
        });


        /***************** search box function *****************/


        google.maps.event.addListener(map, 'click', function (e) {
            marker.setPosition(e.latLng);
            $('#maplat').html(e.latLng.lat());
            $('#maplng').html(e.latLng.lng());
        });

        google.maps.event.addListener(marker, 'drag', function (e) {
            $('#maplat').html(e.latLng.lat());
            $('#maplng').html(e.latLng.lng());
        });


        /***************** map full screen button function *****************/
        map.controls[google.maps.ControlPosition.TOP_LEFT].push(document.getElementById('distributor-map-fullscreen'));
        document.getElementById('distributor-map-fullscreen').style.display = 'block';

        $('#distributor-map-fullscreen').on('click', function () {
            var mapCenter = map.getCenter();
            if (!$(this).is('.map-fullscreen')) {
                $('body').css({
                    overflow: 'hidden'
                });

                $('#distributor-map-panel').css({
                    position: "fixed",

                    top: 0,
                    left: 0,
                    height: $(window).height(),
                    width: "100%",
                    zIndex: 9999,
                    margin: "0 auto"
                });
                $(this).addClass('map-fullscreen');
            }
            else {

                $('#distributor-map-panel').css({
                    position: "relative",
                    top: 'auto',
                    left: 'auto',
                    height: "425px",
                    width: "100%",
                    zIndex: 1,
                    margin: "0 auto"
                });
                $('body').css({
                    overflow: 'auto'
                });
                $(this).removeClass('map-fullscreen');
            }
            google.maps.event.trigger(map, "resize");
            var latlng = new google.maps.LatLng(document.getElementById('maplat').value, document.getElementById('maplng').value);
            map.setCenter(latlng);
        });
        /***************** map full screen button function *****************/
    }
    $scope.initialize();

    $scope.mapReset = function () {
        var latlong = new google.maps.LatLng(27.700769, 85.300140);
        map.setCenter(latlong);
        map.setZoom(10);
        marker.setPosition(latlong);
        $('#maplat').html(27.700769);
        $('#maplng').html(85.300140);
        google.maps.event.trigger(document.getElementById('map-canvas'), 'resize');
    }

    $scope.distributorCreate = function (isValid) {
        debugger;
        if (!isValid) {
            displayPopupNotification("Invalid Field", "warning");
            return;
        }
        var selectedCustomer = $scope.selecteddistCustomer;
        var selectedCustomerData = _.filter($("#distCustomerMultiSelect").data("kendoMultiSelect").dataSource.data(), function (da) { return da.DISTRIBUTOR_CODE == selectedCustomer; });

        var selectedArea = $scope.selectedArea;
        var selectedGroup = $scope.selectedGroup;
        var ItemCode = $scope.selectedItems;
        if (_.isEmpty(ItemCode)) {
            ItemCode = $("#distItemsSelect").data("kendoMultiSelect").value();
        }
        var selectedCheckBox = $("#someSwitchOptionPrimary").is(":checked");
        if (selectedCheckBox == true) {
            var ACTIVE = "Y";
        }
        else {
            ACTIVE = "N";
        }

        var createCustomerCheckBox = $("#CustomerSwitchOption").is(":checked");
        if (createCustomerCheckBox == true) {
            var CUSTOMERFLAG = "Y";
        }
        else {
            CUSTOMERFLAG = "N";
        }
        var lat = $('#maplat').html();
        var long = $('#maplng').html();
        var data = {
            ACTIVE: ACTIVE,
            CUSTOMERFLAG: CUSTOMERFLAG,
            DISTRIBUTOR_CODE: selectedCustomer[0],
            AREA_CODE: $scope.selectedArea[0],
            ItemCode: ItemCode,
            DISTRIBUTOR_NAME: selectedCustomer == "" ? selectedCustomerData[0].CUSTOMER_EDESC : "",
            //GROUPID: $scope.selectedGroup[0] ? $scope.selectedGroup[0] : "",
            LATITUDE: $('#maplat').html(),
            LONGITUDE: $('#maplng').html(),
        }
        if ($scope.selectedGroup && $scope.selectedGroup[0])
            data.GROUPID = $scope.selectedGroup[0];

        if ($scope.saveAction == "Update") //update mode
        {
            distributorService.UpdateDistributor(data).then(function (result) {
                if (result.data.STATUS_CODE === 200) {
                    displayPopupNotification(result.data.MESSAGE, "success");
                    $("#grid").data("kendoGrid").dataSource.read();
                    $('#distCustomerMultiSelect').data('kendoMultiSelect').dataSource.read();
                    $scope.createPanel = false;
                }
                else {
                    displayPopupNotification(result.data.MESSAGE, "warning");
                }
            }, function (error) {
                displayPopupNotification("Error", "error");
            });
        }
        else { //add mode
            distributorService.AddDistributor(data).then(function (result) {
                if (result.data.STATUS_CODE === 200) {
                    displayPopupNotification(result.data.MESSAGE, "success");
                    $("#grid").data("kendoGrid").dataSource.read();
                    $('#distCustomerMultiSelect').data('kendoMultiSelect').dataSource.read();
                    $scope.createPanel = false;
                }
                else {
                    displayPopupNotification(result.data.MESSAGE, "warning");
                }
            }, function (error) {
                displayPopupNotification("Error", "error");
            });
        }




    }


    function getDateFormat(date) {
        return kendo.format("{0:" + reportConfig.dateFormat + "}", new Date(date));
    }




    //bind
    $scope.areaSelectOptions = {
        dataTextField: "AREA_NAME",
        dataValueField: "AREA_CODE",
        height: 600,
        change:GetIndividualReport,
        valuePrimitive: true,
        maxSelectedItems: 1,
        headerTemplate: '<div class="col-md-offset-3"><strong>Area...</strong></div>',
        placeholder: "Select Area...",
        autoClose: false,
        dataBound: function (e) {
            var current = this.value();
            this._savedOld = current.slice(0);
            $("#" + e.sender.element[0].id + "_listbox").slimScroll({ 'height': '179px', 'scroll': 'scroll' });
        },
        dataSource: {
            transport: {
                read: {
                    url: window.location.protocol + "//" + window.location.host + "/api/Distribution/GetDistArea",
                    dataType: "json"
                }
            }
        },

    };
    function GetIndividualReport(evt) {
        $http({
            method: 'GET',
            url: window.location.protocol + "//" + window.location.host + "/api/Distribution/GetIndividualGroup?SingleAreaCode=" + evt.sender.value()[0],
        }).then(function successCallback(response) {
            $scope.selectedGroup = response.data[0] ? [response.data[0].GROUPID] : null;
            //response.data[0] ? $("#distGroupSelect").data("kendoMultiSelect").value(response.data[0].GROUPID) : $("#distGroupSelect").data("kendoMultiSelect").value("");
        }, function errorCallback(response) {
        });
    }


    $scope.distCustomerSelectOptions = {
        dataTextField: "CUSTOMER_EDESC",
        dataValueField: "DISTRIBUTOR_CODE",
        height: 600,
        valuePrimitive: true,
        maxSelectedItems: 1,
        headerTemplate: '<div class="col-md-offset-3"><strong>Distributor...</strong></div>',
        placeholder: "Select Distributor...",
        autoClose: false,
        dataBound: function (e) {
            var current = this.value();
            this._savedOld = current.slice(0);
            $("#" + e.sender.element[0].id + "_listbox").slimScroll({ 'height': '179px', 'scroll': 'scroll' });
        },
        filtering: function (e) {
            
            if (!e.filter)
                e.filter = { value: "" };
            var searchVal = _.filter(this.dataSource.data(), function (da) { return da.DISTRIBUTOR_CODE == ""; });
            if (searchVal.length > 0)
                this.dataSource.remove(searchVal[0]);
            if (e.filter.value.trim() != "") {
                e.filter.value[0] = e.filter.value[0].toUpperCase();
                this.dataSource.add({ CUSTOMER_EDESC: e.filter.value, DISTRIBUTOR_CODE: "" });
            }
        },
        //close: function (e) {
        //    var searchVal = _.filter(this.dataSource.data(), function (da) { return da.DISTRIBUTOR_CODE == null; });
        //    if (searchVal.length > 0)
        //        this.dataSource.remove(searchVal[0]);
        //},
        dataSource: {
            transport: {
                read: {
                    url: window.location.protocol + "//" + window.location.host + "/api/Distribution/GetIndividualCustomer",
                    dataType: "json"
                }
            }
        }
    };
    var grid = $("#grid").kendoGrid({
        dataSource: {
            type: "json",
            transport: {
                read: {
                    url: window.location.protocol + "//" + window.location.host + "/api/Setup/GetDistributorList", // <-- Get data from here.
                    dataType: "json", // <-- The default was "jsonp".
                    type: "POST",
                    contentType: "application/json; charset=utf-8"
                },
                parameterMap: function (options, type) {
                    var paramMap = JSON.stringify($.extend(options, ReportFilter.filterAdditionalData()));
                    delete paramMap.$inlinecount; // <-- remove inlinecount parameter.
                    delete paramMap.$format; // <-- remove format parameter.
                    return paramMap;
                }
            },
            error: function (e) {
                displayPopupNotification("Sorry error occured while processing data", "error");
            },
            model: {
                fields: {
                    // ASSIGN_DATE: { type: "date" },
                }
            },
            group: { field: "GROUP_EDESC" },
            sort: {
                field: "WEIGHT",
                dir: "asc"
            },
            pageSize: 500,
        },
        toolbar: kendo.template($("#toolbar-template").html()),
        excel: {
            fileName: "PO Index",
            allPages: true,
        },
        pdf: {
            fileName: "Received Schedule",
            allPages: true,
            avoidLinks: true,
            pageSize: "auto",
            margin: {
                top: "2m",
                right: "1m",
                left: "1m",
                buttom: "1m",
            },
            landscape: true,
            repeatHeaders: true,
            scale: 0.8,
        },
        height: window.innerHeight - 50,
        sortable: true,
        reorderable: true,
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
                SaveReportSetting('DistributionSetup', 'grid');
        },
        columnHide: function (e) {
            if ($(".k-widget.k-reset.k-header.k-menu.k-menu-vertical").is(":visible") && checkboxItem != "")
                SaveReportSetting('DistributionSetup', 'grid');
        },
        pageable: {
            refresh: true,
            pageSizes: reportConfig.itemPerPage,
            buttonCount: 5
        },
        scrollable: {
            virtual: true
        },
        dataBound: function (o) {
            GetSetupSetting("DistributorSetup");
            var grid = o.sender;
            if (grid.dataSource.total() == 0) {
                var colCount = grid.columns.length;
                $(o.sender.wrapper)
                    .find('tbody')
                    .append('<tr class="kendo-data-row" style="font-size:12px;"><td colspan="' + colCount + '" class="alert alert-danger">Sorry, no data :(</td></tr>');
                displayPopupNotification("No Data Found.", "info");
            }
            else {
                var g = $("#grid").data("kendoGrid");
                for (var i = 0; i < g.columns.length; i++) {
                    g.showColumn(i);
                }
                $("div.k-group-indicator").each(function (i, v) {
                    g.hideColumn($(v).data("field"));
                });
            }

            UpdateReportUsingSetting("DistributionSetup", "grid");
            $('div').removeClass('.k-header k-grid-toolbar');
        },
        columns: [
            {
                field: "DISTRIBUTOR_CODE",
                title: "Code",
                width: "8%"
            },

            {
                field: "DISTRIBUTOR_NAME",
                title: "Distributor Name",
            },
            {
                field: "ADDRESS",
                title: "Address",
            },
            {
                field: "AREA_Name",
                title: "Area",

            },
            {
                field: "GROUP_EDESC",
                title: "Group",
                // groupHeaderTemplate: "<span data-code='01' > #= value # </span>"
            },
            {
                field: "ISACTIVE",
                title: "Status",
                template: "#= ISACTIVE == 'Y' ? 'Active' : 'InActive' # ",
                width: "9%"
            },
            {
                title: "Action",
                template: " <a class='fa fa-edit editAction' onclick='UpdateClickEvent($(this))' title='Edit'></a>&nbsp &nbsp<a class='fa fa-trash-o deleteAction' onclick='deleteDistributor($(this))' title='delete'></a> ",
                width: "7%"
            }



        ]

    }).data("kendoGrid");
    grid.table.kendoSortable({
        // cursor: "move",
        filter: ">tbody >tr",
        hint: function (element) { //customize the hint
            var table = $('<table style="width: 900px;" class="k-grid k-widget"></table>'),
                hint;

            table.append(element.clone()); //append the dragged element
            table.css("opacity", 0.7);

            return table; //return the hint element
        },
        placeholder: function (element) {
            return element.clone().addClass("k-state-hover").css("opacity", 1);
        },
        container: "#grid tbody",
        filter: ">tbody > tr:not('.k-grouping-row')",
        group: "gridGroup",

        change: function (e) {
            
            var grid = $("#grid").data("kendoGrid"),
                oldIndex = e.oldIndex,
                newIndex = e.newIndex,
                view = grid.dataSource.view(),
                dataItem = grid.dataSource.getByUid(e.item.data("uid"));
            dataItem.dirty = true;
            if (oldIndex < newIndex) {
                for (var i = oldIndex + 1; i <= newIndex; i++) {
                    sortAscDescFunction();
                }
            } else {
                for (var i = oldIndex - 1; i >= newIndex; i--) {
                    sortAscDescFunction();
                }
            }

            function sortAscDescFunction() {
                var distributorCodeArr = _.pluck(_(_.filter(grid.dataSource.data(), function (x) {
                    x["index"] = $("#grid").find("[data-uid='" + x.uid + "']").index();
                    return x.GROUPID == dataItem.GROUPID;
                })).sortBy("index"), "DISTRIBUTOR_CODE");
                var obj = {
                    OLD_INDEX: oldIndex,
                    NEW_INDEX: newIndex,
                    GROUPID: dataItem.GROUPID == null ? "''" : dataItem.GROUPID,
                    DISTRIBUTOR_CODE: dataItem.DISTRIBUTOR_CODE,
                    DISTRIBUTOR_LIST: distributorCodeArr
                }
                distributorService.UpdateOrder(obj).then(function (result) {
                    
                    $("#grid").data("kendoGrid").dataSource.read();
                    $("#grid").data("kendoGrid").dataSource.sort({
                        field: "WEIGHT",
                        dir: "asc"

                    });
                    if (result.data == "Success") {
                        displayPopupNotification("Order Saved", "success")
                    }
                    else {
                        displayPopupNotification("Something Went Wrong", "error")
                    }

                }, function (error) {
                    displayPopupNotification("Error Occur on Sorting,try again", "error");
                });

                    //setTimeout(function () {
                    //    $("#grid").data("kendoGrid").dataSource.pageSize(100);
                    //}, 1e3)
            }

        }

        

    }),

      

        //events
        $scope.AddClickEvent = function () {
        debugger;
            $("#distCustomerMultiSelect").data("kendoMultiSelect").value([]);
            $("#distCustomerMultiSelect").data("kendoMultiSelect").enable();
            $("#areaMultiSelect").data("kendoMultiSelect").value([]);
            $("#distGroupSelect").data("kendoMultiSelect").value([]);
            $("#distItemsSelect").data("kendoMultiSelect").value([]);
            $("#distBrandSelect").data("kendoMultiSelect").value([]);
            $('input[name=someSwitchOptionPrimary]').prop('checked', false);
            $('input[name=CustomerSwitchOption]').prop('checked', true);
            $('input[name=CustomerSwitchOption]').prop('disabled', false);

            $scope.showCustomerSelect = true;
            $scope.selectedArea = null;
            $scope.selecteddistCustomer = null;

            $scope.pageName = "Add Distributor";
            $scope.saveAction = "Save";
            $scope.createPanel = true;
            //update map   
            var latlong = new google.maps.LatLng(27.700769, 85.300140);
            map.setCenter(latlong);
            marker.setPosition(latlong);
            $('#maplat').html(27.700769);
            $('#maplng').html(85.300140);

            setTimeout(function () {
                google.maps.event.trigger(document.getElementById('map-canvas'), 'resize');
                map.setCenter(latlong);
            });

        }
    UpdateClickEvent = function (evt) {
        debugger;
        
        $scope.showCustomerSelect = false;
        $scope.pageName = "Update Distributor";
        $scope.saveAction = "Update";
        $scope.createPanel = true;
        var grid = $("#grid").data("kendoGrid");
        var row = $(evt).closest("tr");
        var item = grid.dataItem(row);
        var selectedArea = $.grep($("#areaMultiSelect").data("kendoMultiSelect").dataSource.data(), function (element, index) {
            return element.AREA_NAME == item.AREA_Name;
        });
        var selectedGroup = $.grep($("#distGroupSelect").data("kendoMultiSelect").dataSource.data(), function (element, index) {
            return element.GROUP_EDESC == item.GROUP_EDESC;
        });
        // $("#distCustomerMultiSelect").data("kendoMultiSelect").value([item.DISTRIBUTOR_CODE]);
        $scope.selecteddistCustomer = [item.DISTRIBUTOR_CODE];
        
        var arr = [];
        if (!_.isEmpty(item.ItemCodeString))
            $.each(item.ItemCodeString.split(','), function (i, obj) {
                arr.push(obj);
            });
        $("#distItemsSelect").data("kendoMultiSelect").value(arr);
        var selectedDataSource = $("#distItemsSelect").data("kendoMultiSelect").dataItems();
        var filterBrand = [];
        for (var i = 0; i < selectedDataSource.length; i++) {
            filterBrand.push(selectedDataSource[i].BRAND_NAME);
        }
        $("#distBrandSelect").data("kendoMultiSelect").value(filterBrand);
        $("#distCustomerMultiSelect").data("kendoMultiSelect").readonly();
        $("#areaMultiSelect").data("kendoMultiSelect").value([selectedArea[0].AREA_CODE]);
        if (selectedGroup[0]) {
            $("#distGroupSelect").data("kendoMultiSelect").value([selectedGroup[0].GROUPID]);
            $scope.selectedGroup = [selectedGroup[0].GROUPID];
        }
        else
            $("#distGroupSelect").data("kendoMultiSelect").value([]);


        $scope.selecteddistCustomer = [item.DISTRIBUTOR_CODE];
        $scope.selectedArea = [selectedArea[0].AREA_CODE];
        if (item.ACTIVE == "Y") {
            $('input[name=someSwitchOptionPrimary]').prop('checked', true);
        }
        else {
            $('input[name=someSwitchOptionPrimary]').prop('checked', false);
        }
       
            $('input[name=CustomerSwitchOption]').prop('disabled', true);
            
        

        //$scope.pageName = "Update Distributor";
        //$scope.saveAction = "Update";

        //update map   
        var latlong = new google.maps.LatLng(item.LATITUDE, item.LONGITUDE);
        map.setCenter(latlong);
        marker.setPosition(latlong);
        $('#maplat').html(item.LATITUDE);
        $('#maplng').html(item.LONGITUDE);

    }

   deleteDistributor = function (evt) {
        
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

                    var row = $(evt).closest("tr");
                    var item = grid.dataItem(row);
                    var DISTRIBUTOR_CODE = item.DISTRIBUTOR_CODE
                    data = {
                        DISTRIBUTOR_CODE: DISTRIBUTOR_CODE,
                    }

                     var deleteDistributor = distributorService.deleteDistributor(data);
                     deleteDistributor.then(function (response) {
                         
                        if (response.data.STATUS_CODE == 300) {
                            displayPopupNotification(response.data.MESSAGE, "success")
                            $("#grid").data("kendoGrid").dataSource.read();
                        }
                        else {
                            displayPopupNotification(response.data.MESSAGE, "error");
                        }
                    });
                }

            }
        });
    };

    $scope.cancelClickEvent = function () {
        $scope.createPanel = false;
    }

});