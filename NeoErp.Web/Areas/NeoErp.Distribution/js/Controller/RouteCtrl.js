﻿
distributionModule.controller('RouteCtrl', function ($scope, DistSetupService, $routeParams) {   
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

    $scope.param = $routeParams.param;
    $scope.pageName = "Add Route";
    $scope.saveAction = "Save";
    $scope.orientation = "horizontal";
    $scope.TotalEntCount = {
        Total: 0,
        Resellers: 0,
        Dealers: 0,
        Distributors: 0,
        Hoardings: 0,
    };
    $scope.createPanel = false;

    $scope.areaSelectOptions = {
        dataTextField: "AREA_NAME",
        dataValueField: "AREA_CODE",
        height: 600,
        valuePrimitive: true,
        //maxSelectedItems: 1,
        headerTemplate: '<div class="col-md-offset-3"><strong>Area...</strong></div>',
        placeholder: "Select Area...",
        autoClose: false,
        dataBound: function (e) {
            var current = this.value();
            this._savedOld = current.slice(0);
            $("#" + e.sender.element[0].id + "_listbox").slimScroll({ 'height': '179px', 'scroll': 'scroll' });
        },
        change: areaChange,
        dataSource: {
            transport: {
                read: {
                    url: window.location.protocol + "//" + window.location.host + "/api/Distribution/GetDistArea",
                    dataType: "json"
                }
            }
        }
    };

    //grid
    var reportConfig = GetReportSetting("RouteSetup");
    $scope.grid = {
        dataSource: {
            type: "json",
            transport: {
                read: {
                    url: window.location.protocol + "//" + window.location.host + "/api/Setup/GetRouteList",
                    dataType: "json",
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
            //serverFiltering: false,
            //serverAggregates: true,
            model: {
                fields: {
                    // ASSIGN_DATE: { type: "date" },
                }
            },
            //serverPaging: false,
            //serverSorting: false,
            pageSize: reportConfig.defaultPageSize,
        },
        toolbar: kendo.template($("#toolbar-template").html()),
        excel: {
            fileName: "Received Schedule",
            allPages: true,
        },
        pdf: {
            fileName: "Route Setup",
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
        reorderable: false,
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
                SaveReportSetting('RouteSetup', 'grid');
        },
        columnHide: function (e) {
            if ($(".k-widget.k-reset.k-header.k-menu.k-menu-vertical").is(":visible") && checkboxItem != "")
                SaveReportSetting('RouteSetup', 'grid');
        },
        pageable: {
            refresh: true,
            pageSizes: reportConfig.itemPerPage,
            buttonCount: 5
        },
        pageable:true,
        scrollable: {
            virtual: true
        },
        dataBound: function (o) {
            GetSetupSetting("RouteSetup");
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

            UpdateReportUsingSetting("RouteSetup", "grid");
            $('div').removeClass('.k-header k-grid-toolbar');
        },
        columns: [
            {
                field: "ROUTE_CODE",
                title: "Route Code",
            },
             {
                 field: "ROUTE_NAME",
                 title: "Route Name",
            },
            {                
                title: "Outlets",
                template: function (dataItem) {
                    var result = dataItem.ENTITY_COUNT + " (";
                    if (dataItem.DIST_COUNT > 0)
                        result += "Distributor:" + dataItem.DIST_COUNT;
                    if (dataItem.RESELLER_COUNT > 0)
                        result += " Outlet:" + dataItem.RESELLER_COUNT;
                    if (dataItem.DEALER_COUNT > 0)
                        result += " Dealer:" + dataItem.DEALER_COUNT;
                    result += ")";
                    return result;
                }
            },
               {
                   title: "Action",
                   template: " <a class='fa fa-edit editAction' ng-click='UpdateRoute($event)' title='Edit'></a>&nbsp &nbsp<a class='fa fa-trash-o deleteAction' ng-click='deleteRoute($event)' title='delete'></a> "
               }



        ]

    };
    //
    var DataSource = window.location.protocol + "//" + window.location.host + "/api/Setup/GetBrandingRoutes";

    $scope.brd_grid = {
        dataSource: {
            type: "json",
            transport: {
                read: {
                    url: DataSource,
                    dataType: "json",
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
            //serverFiltering: false,
            //serverAggregates: true,
            model: {
                fields: {
                    // ASSIGN_DATE: { type: "date" },
                }
            },
            //serverPaging: false,
            //serverSorting: false,
            pageSize: reportConfig.defaultPageSize,
        },
        toolbar: kendo.template($("#toolbar-template").html()),
        excel: {
            fileName: "Received Schedule",
            allPages: true,
        },
        pdf: {
            fileName: "Route Setup",
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
        reorderable: false,
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
                SaveReportSetting('RouteSetup', 'grid');
        },
        columnHide: function (e) {
            if ($(".k-widget.k-reset.k-header.k-menu.k-menu-vertical").is(":visible") && checkboxItem != "")
                SaveReportSetting('RouteSetup', 'grid');
        },
        pageable: {
            refresh: true,
            pageSizes: reportConfig.itemPerPage,
            buttonCount: 5
        },
        pageable: true,
        scrollable: {
            virtual: true
        },
        dataBound: function (o) {
            GetSetupSetting("RouteSetup");
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

            UpdateReportUsingSetting("RouteSetup", "grid");
            $('div').removeClass('.k-header k-grid-toolbar');
        },
        columns: [
            {
                field: "ROUTE_CODE",
                title: "Route Code",
            },
             {
                 field: "ROUTE_NAME",
                 title: "Route Name",
            },
            {
                title: "Outlets",
                template: function (dataItem) {
                    var result = dataItem.ENTITY_COUNT + " (";
                    if (dataItem.DIST_COUNT > 0)
                        result += "Distributor:" + dataItem.DIST_COUNT;
                    if (dataItem.RESELLER_COUNT > 0)
                        result += " Outlet:" + dataItem.RESELLER_COUNT;
                    if (dataItem.DEALER_COUNT > 0)
                        result += " Dealer:" + dataItem.DEALER_COUNT;
                    if (dataItem.HOARDING_COUNT > 0)
                        result += " Hoarding:" + dataItem.HOARDING_COUNT;
                    result += ")";
                    return result;
                }
            },
               {
                   title: "Action",
                   template: " <a class='fa fa-edit editAction' ng-click='UpdateRoute($event)' title='Edit'></a>&nbsp &nbsp<a class='fa fa-trash-o deleteAction' ng-click='deleteRoute($event)' title='delete'></a> "
               }
        ]
    };

    ////create empty temp array
    var map, polyline, markers = [];

    $scope.initialize = function initialize() {
        //var myLatlng = new google.maps.LatLng('27.700769', '85.300140');

        var mapOptions = {
            zoom: 10,
            //center: myLatlng,
            mapTypeId: google.maps.MapTypeId.ROADMAP
        };

        map = new google.maps.Map(document.getElementById('map-canvas'), mapOptions);

        /***************** search box function *****************/
        // Create the search box and link it to the UI element.
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

            var bounds = new google.maps.LatLngBounds();

            if (places[0].geometry.viewport) {
                bounds.union(places[0].geometry.viewport);
            } else {
                bounds.extend(places[0].geometry.location);
            }
            map.fitBounds(bounds);
            document.getElementById('maplat').value = places[0].geometry.location.lat();
            document.getElementById('maplng').value = places[0].geometry.location.lng();
        });
        /***************** search box function *****************/


        /***************** map full screen button function *****************/
        map.controls[google.maps.ControlPosition.TOP_LEFT].push(document.getElementById('route-map-fullscreen'));

        $('#route-map-fullscreen').on('click', function () {
            if (!$(this).is('.map-fullscreen')) {
                $('body').css({
                    overflow: 'hidden'
                });

                $('#route-map-panel').css({
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
                $('#route-map-panel').css({
                    position: "relative",
                    top: 'auto',
                    left: 'auto',
                    height: "400px",
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
        });
        /***************** map full screen button function *****************/

        google.maps.event.addListenerOnce(map, 'idle', function () {
            //loaded fully
            document.getElementById('mapSearchBox').style.display = 'block';
            document.getElementById('route-map-fullscreen').style.display = 'block';
        });

        drawRoutePolyline();
    }

    function drawRoutePolyline(sortedData) {
        // Get the selected entities
        var orderedEntity = [];
        if ("undefined" == typeof (sortedData)) {
            $('select#multiselect-entity option:selected').each(function (k, v) {
                var idx = Number($(v).data('index'));
                orderedEntity[--idx] = v;
            });
        }
        else {
            $.each(sortedData, function (k, v) {
                var option = $('select#multiselect-entity option[data-key="' + v.id + '"]').get(0);
                option.setAttribute('data-index', (k + 1).toString());
                orderedEntity[k] = option;
            });
        }
        // Hide the previous markers
        if (markers.length) {
            $.each(markers, function (k, marker) {
                marker.setMap(null);
            });
            markers = [];
        }
        var bounds = new google.maps.LatLngBounds();
        var path = new google.maps.MVCArray();
        $.each(orderedEntity, function (k, v) {
            var position = new google.maps.LatLng(Number($(v).data('lat')), Number($(v).data('lng')));
            markers.push(new google.maps.Marker({
                position: position,
                map: map,
                title: $.trim($(v).text()),
                label: (k + 1).toString()
            }));
            path.push(position);
            bounds.extend(position);
        });

        if (polyline instanceof google.maps.Polyline) {
            polyline.setMap(null);
        }
        // Reinitialize polyline
        polyline = new google.maps.Polyline({
            map: map,
            path: path,
            strokeWeight: 3,
            strokeColor: '#F75850',
            strokeOpacity: 1,
            zIndex: 10,
            icons: [{
                icon: {
                    path: google.maps.SymbolPath.FORWARD_CLOSED_ARROW,
                    scale: 4,
                    strokeColor: '#393',
                    offset: '100%'
                }
            }],
            isVisible: true
        });
        function animatePath(line) {
            var count = 0;
            window.setInterval(function () {
                count = (count + 1) % 200;

                var icons = line.get('icons');
                icons[0].offset = (count / 2) + '%';
                line.set('icons', icons);
            }, 50);
        }
        animatePath(polyline);
        if (!bounds.isEmpty()) {
            map.fitBounds(bounds);
        }
    }

    $scope.initialize();

    $scope.RouteCreate = function (isValid, type) {
        if (!isValid) {
            displayPopupNotification("Invalid Field", "warning");
            return;
        }

        var routeEntityModel = [];
        $('select#multiselect-entity option:selected').each(function (i, obj) {
            routeEntityModel.push({ 'ENTITY_Code': $(obj).val().split('::')[1], 'ENTITY_TYPE': $(obj).data("type"), 'ORDER_NO': $(obj).data("key") });
        });

        var data = {
            ROUTE_CODE: $scope.Route_CODE,
            ROUTE_NAME: $scope.routeName,
            ROUTE_TYPE: type,
            AREA_Code: $scope.selectedArea,
            RouteEntityModel: routeEntityModel
        }
        

        if ($scope.saveAction == "Update") //update mode
        {
            DistSetupService.UpdateRoute(data).then(function (result) {
                if (result.data.STATUS_CODE === 200) {
                    displayPopupNotification(result.data.MESSAGE, "success");
                    $("#grid").data("kendoGrid").dataSource.read();
                    // $('.createPanel').hide();
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
            debugger;
            var grid = $("#grid").data("kendoGrid").dataSource.data();
            var hasChild = $.grep(grid, function (n) {
                return (n.ROUTE_NAME == data.ROUTE_NAME);
            });
            if (hasChild.length>0)
            {
                displayPopupNotification("Can not Inser Duplicate Route Name", "warning");
                return;
            }
            DistSetupService.AddRoute(data).then(function (result) {
                if (result.data.STATUS_CODE === 200) {
                    displayPopupNotification(result.data.MESSAGE, "success");
                    $("#grid").data("kendoGrid").dataSource.read();
                    //$('.createPanel').hide();
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

    $scope.AddButtonClickEvent = function () {
        $scope.clear();
        var multiSelect = $("#areaMultiSelect").data("kendoMultiSelect");
        multiSelect.trigger('change');
        $scope.selectedArea = null;
        var splitter = $("#splitter").data("kendoSplitter");
        splitter.options.panes[0].size = "0%";
        $scope.createPanel = true;
    }

    updateTreeMultiSelect = function (item) {
        var orderedEntity = [];
        var flag = false;
        DistSetupService.getSelectedCustomerByRouteCode(item.ROUTE_CODE).then(function (result) {
            $.each(result.data, function (index, value) {
                if ($('select#multiselect-entity option').length > 0) {
                    $('select#multiselect-entity option').each(function (i, obj) {
                        if ((value.ENTITY_TYPE + "::" + value.ENTITY_Code) == obj.value) {
                            orderedEntity.push(obj);
                            obj.selected = true;
                            $(obj).attr("selected", "selected");
                        }
                    });
                } else {
                    //if(!flag)
                        setTimeout(function () { updateTreeMultiSelect(item); },1000);
                    flag = true;
                    return;
                }

            });
            if (flag)
                return;
            var selectedData = [];
            $scope.TotalEntCount = {
                Total: 0,
                Resellers: 0,
                Dealers: 0,
                Distributors: 0,
                Hoardings: 0,
            };
            $scope.TotalEntCount.Total = orderedEntity.length;
            $.each(orderedEntity, function (i, v) {
                var d = {
                    id: $(v).data("key"),
                    section: $(v).data("section"),
                    text: $(v).text(),
                    value: $(v).val()
                }
                selectedData.push(d);
                if (d.value[0] == "R")
                    $scope.TotalEntCount.Resellers++;
                if (d.value[0] == "D")
                    $scope.TotalEntCount.Distributors++;
                if (d.value[0] == "P")
                    $scope.TotalEntCount.Dealers++;
                if (d.value[0] == "H")
                    $scope.TotalEntCount.Hoardings++;
            });
            drawRoutePolyline(selectedData);

            $('#multiselect-entity').treeMultiselect({
                searchable: true,
                searchParams: ['section', 'text'],
                sortable: true,
                onChange: function (data) {
                    drawRoutePolyline(data);
                    $scope.TotalEntCount = {
                        Total: data.length,
                        Resellers: 0,
                        Dealers: 0,
                        Distributors: 0,
                        Hoardings: 0,
                    };
                    for (var len = 0; len < data.length; len++) {
                        if (data[len].value[0] == "R")
                            $scope.TotalEntCount.Resellers++;
                        if (data[len].value[0] == "D")
                            $scope.TotalEntCount.Distributors++;
                        if (data[len].value[0] == "P")
                            $scope.TotalEntCount.Dealers++;
                        if (data[len].value[0] == "H")
                            $scope.TotalEntCount.Hoardings++;
                    }
                    $scope.$apply();
                },
            });
            if ($(".tree-multiselect").length > 1)
                $(".tree-multiselect").last().remove()

            $('#entity-wrapper').show();

            // Resize the height of the map
            var btnHeight = $('#btn-submit').closest('.row').outerHeight();
            var leftColHeight = $('#route-map-panel').closest('.row').find('.col-md-7').height();
            $('#route-map-panel').css({
                height: (leftColHeight - btnHeight) + 'px',
                display: 'block'
            });
            google.maps.event.trigger(map, "resize");
            //$('#multiselect-entity').data("treeMultiSelect").trigger("change");

        });
    }

    $scope.UpdateRoute = function (evt) {
        $scope.clear();
        var grid = $("#grid").data("kendoGrid");
        var row = evt.target.closest("tr");
        var item = grid.dataItem(row);


        $scope.Route_CODE = item.ROUTE_CODE;
        $scope.routeName = item.ROUTE_NAME
        $scope.pageName = "Update Route";
        $scope.saveAction = "Update";
        $scope.createPanel = true;

        DistSetupService.getAreaByRouteCode(item.ROUTE_CODE).then(function (result) {
            $scope.selectedArea = _.map(result.data, 'AREA_CODE');
            var multiSelect = $("#areaMultiSelect").data("kendoMultiSelect");
            multiSelect.value($scope.selectedArea);
            multiSelect.trigger('change');
            setTimeout(function () { updateTreeMultiSelect(item); })

        });
    }

    $scope.deleteRoute = function (evt) {


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
                    var grid = $("#grid").data("kendoGrid");
                    var row = evt.target.closest("tr");
                    var item = grid.dataItem(row);

                    var ROUTE_CODE = item.ROUTE_CODE;
                    var ROUTE_NAME = item.ROUTE_NAME;

                    var data = {
                        ROUTE_CODE: ROUTE_CODE,
                        ROUTE_NAME: ROUTE_NAME,
                    }

                    var deleteRoute = DistSetupService.deleteRoute(data);
                    deleteRoute.then(function (response) {
                        if (response.data.STATUS_CODE == 200) {
                            displayPopupNotification(response.data.MESSAGE, "Success");
                            $("#grid").data("kendoGrid").dataSource.read();
                            $scope.Cancel();
                        }
                        else {
                            displayPopupNotification(response.data.MESSAGE, "error");
                        }
                    });
                }
            }
        });
    }

    $scope.Cancel = function () {
        $scope.clear();
        $scope.createPanel = false;
        // google.maps.event.clearInstanceListeners(window);
        // google.maps.event.clearInstanceListeners(document);

    }

    $scope.clear = function () {
        $scope.pageName = "Add Route";
        $scope.Route_CODE = "";
        $scope.routeName = "";
        $scope.selectedArea = "";
        $("#areaMultiSelect").data("kendoMultiSelect").value([]);
        $scope.selectedArea = null;
        
        $.each($('#multiselect-entity option:selected'), function (index, obj) {
            $(obj).removeAttr("selected");
        });
        $('#multiselect-entity').off();
        $('.tree-multiselect').remove();
        $('#multiselect-entity').html('');
        $scope.TotalEntCount = {
            Total: 0,
            Resellers: 0,
            Dealers: 0,
            Distributors: 0,
            Hoardings: 0,
        };
        $scope.saveAction = "Save";
    }

    function areaChange(e) {
        var value = this.value();
        if (value.length > 0)
            value = "'" + this.value().join("','") + "'";
        else {
            value = "none";
        }
        DistSetupService.GetRouteDistributorList(value).then(function (result) {
            $scope.DistributorList = result.data;
            $('#maplat').html(27.700769);
            $('#maplng').html(85.300140);

            var res = result.data
           
            //splitter
            var splitter = $("#splitter").data("kendoSplitter");
            if (value === "none")
                splitter.options.panes[0].size = "0%";
            else if ($scope.DistributorList == undefined) {
                splitter.options.panes[0].size = "10%";
                $scope.SelectedCustomerMessage = true;
            }
            else if ($scope.DistributorList.distributors.length == 0 && $scope.DistributorList.resellers.length == 0 && $scope.DistributorList.dealers.length == 0) {
                splitter.options.panes[0].size = "10%";
                $scope.SelectedCustomerMessage = true;
            }
            else {
                splitter.options.panes[0].size = "45%";
                $scope.SelectedCustomerMessage = false;
            }

            splitter.resize(true);
            // Find the previous selected options and order
            var prevSelectedOptionsArray = $('#multiselect-entity option:selected');
            var prevSelectedOptionsObject = {}
            $.each(prevSelectedOptionsArray, function (k, pso) {
                prevSelectedOptionsObject[$(pso).val()] = pso;
            });
            //console.log("prev obj", prevSelectedOptionsObject);

            $('#multiselect-entity').off();
            $('.tree-multiselect').remove();
            $('#multiselect-entity').html('');
            for (var i = 0; i < res.distributors.length; i++) {
                var distributor = res.distributors[i];
                var entityTypeCode = 'D::' + distributor.DISTRIBUTOR_CODE;
                var selected = ("undefined" != typeof (prevSelectedOptionsObject[entityTypeCode])) ? 'selected="selected"' : "";
                var dataIndex = ("" != selected) ? 'data-index="' + $(prevSelectedOptionsObject[entityTypeCode]).data('index') + '"' : "";
                var section = distributor.AREA_NAME + "/Distributor";
                var option = '<option class="' + distributor.AREA_CODE + '" value="' + entityTypeCode + '" ' + selected + ' data-section="' + section + '"' + dataIndex + ' data-lat="' + distributor.LATITUDE + '" data-lng="' + distributor.LONGITUDE + '" data-addr="' + distributor.REGD_OFFICE_ADDRESS + '" data-type="D">' + distributor.DISTRIBUTOR_NAME + "(" + distributor.DISTRIBUTOR_CODE +")" + '</option>';
                $('#multiselect-entity').append(option);
            };
            for (var i = 0; i < res.resellers.length; i++) {
                var reseller = res.resellers[i];
                var entityTypeCode = 'R::' + reseller.RESELLER_CODE;
                var selected = ("undefined" != typeof (prevSelectedOptionsObject[entityTypeCode])) ? 'selected="selected"' : "";
                var dataIndex = ("" != selected) ? 'data-index="' + $(prevSelectedOptionsObject[entityTypeCode]).data('index') + '"' : "";
                var section = reseller.AREA_NAME + "/Reseller";
                var option = '<option class="' + reseller.AREA_CODE + '" value="' + entityTypeCode + '" ' + selected + ' data-section="' + section + '" ' + dataIndex + ' data-lat="' + reseller.LATITUDE + '" data-lng="' + reseller.LONGITUDE + '" data-addr="' + reseller.REGD_OFFICE_ADDRESS + '" data-type="R">' + reseller.RESELLER_NAME + "(" + reseller.RESELLER_CODE + ")" + '</option>';
                $('#multiselect-entity').append(option);
            };
            for (var i = 0; i < res.dealers.length; i++) {
                var dealer = res.dealers[i];
                var entityTypeCode = 'P::' + dealer.DEALER_CODE;
                var selected = ("undefined" != typeof (prevSelectedOptionsObject[entityTypeCode])) ? 'selected="selected"' : "";
                var dataIndex = ("" != selected) ? 'data-index="' + $(prevSelectedOptionsObject[entityTypeCode]).data('index') + '"' : "";
                var section = dealer.AREA_NAME + "/Dealer";
                var option = '<option class="' + dealer.AREA_CODE + '" value="' + entityTypeCode + '" ' + selected + ' data-section="' + section + '" ' + dataIndex + ' data-lat="' + dealer.LATITUDE + '" data-lng="' + dealer.LONGITUDE + '" data-addr="' + dealer.REGD_OFFICE_ADDRESS + '" data-type="P">' + dealer.DEALER_NAME + "(" + dealer.DEALER_CODE + ")" + '</option>';
                $('#multiselect-entity').append(option);
            };
            for (var i = 0; i < res.hoardings.length; i++) {
                var board = res.hoardings[i];
                var entityTypeCode = 'H::' + board.ENTITY_CODE;
                var selected = ("undefined" != typeof (prevSelectedOptionsObject[entityTypeCode])) ? 'selected="selected"' : "";
                var dataIndex = ("" != selected) ? 'data-index="' + $(prevSelectedOptionsObject[entityTypeCode]).data('index') + '"' : "";
                var section = board.AREA_NAME + "/Hoarding Boards";
                var option = '<option class="' + board.AREA_CODE + '" value="' + entityTypeCode + '" ' + selected + ' data-section="' + section + '" ' + dataIndex + ' data-lat="' + board.LATITUDE + '" data-lng="' + board.LONGITUDE + '" data-addr="' + board.ADDRESS + '" data-type="H">' + board.ENTITY_NAME + "(" + board.ENTITY_CODE + ")" + '</option>';
                $('#multiselect-entity').append(option);
            };

            drawRoutePolyline();
            $('#multiselect-entity').treeMultiselect({
                sortable: true,
                searchable: true,
                onChange: function (data) {
                    drawRoutePolyline(data);
                    $scope.TotalEntCount = {
                        Total: data.length,
                        Resellers: 0,
                        Dealers: 0,
                        Distributors: 0,
                        Hoardings: 0,
                    };
                    for (var len = 0; len < data.length; len++) {
                        if (data[len].value[0] == "R")
                            $scope.TotalEntCount.Resellers++;
                        if (data[len].value[0] == "D")
                            $scope.TotalEntCount.Distributors++;
                        if (data[len].value[0] == "P")
                            $scope.TotalEntCount.Dealers++;
                        if (data[len].value[0] == "H")
                            $scope.TotalEntCount.Hoardings++;
                    }
                    $scope.$apply();
                },
                searchParams: ['section', 'text']
            });

            $('#entity-wrapper').show();

            // Resize the height of the map
            var btnHeight = $('#btn-submit').closest('.row').outerHeight();
            var leftColHeight = $('#route-map-panel').closest('.row').find('.col-md-7').height();
            $('#route-map-panel').css({
                height: (leftColHeight - btnHeight) + 'px',
                display: 'block'
            });
            google.maps.event.trigger(map, "resize");
        });
    }

    //Form Validation Fires on Form Submit
    $('form').on('submit', function (e) {
        var isValid = true;
        $('.reqField').each(function (k, v) {
            if (!$.trim($(this).val())) {
                isValid = false;
                if (!$(this).next('.error').length) {
                    $(this).after('<div class="error false">This field is required.</div>');
                }
            }
            else {
                $(this).next('.error').remove();
            }
        });
        /*console.log(isValid);
        return false;*/
        if (!isValid) {
            return false;
        }
    });

});