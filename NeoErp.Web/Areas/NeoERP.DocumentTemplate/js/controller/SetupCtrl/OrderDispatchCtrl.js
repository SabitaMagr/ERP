DTModule.controller('OrderDispatchCtrl', function ($scope, OrderDispatchService, $filter, $window) {


    $scope.FormName = "Order Dispatch Management";

    $scope.isDispatchGrid = "";

    $scope.Dispatch_From = $filter('date')(new Date(), 'dd-MMM-yyyy');
    $scope.Dispatch_To = ""; /*$filter('date')(new Date(), 'dd-MMM-yyyy');*/
    $scope.PlanningDate = $filter('date')(new Date(), 'dd-MMM-yyyy');
    $scope.PlanningTo = "";

   // $scope.Dispatch_no = "DN" + Math.floor(Math.random(1,99999));

    $scope.getDispatchNo = function () {
        var disPatchNo = OrderDispatchService.generateDispatchNo();
        disPatchNo.then(function (fs) {
            if (fs.data == null) {
                $scope.Dispatch_no = 01;
            } else {
                if (fs.data > 1 & fs.data <= 9) $scope.Dispatch_no = "0000" + fs.data;
                else if (fs.data >= 10 & fs.data <=99) $scope.Dispatch_no = "000" + fs.data;
                else $scope.Dispatch_no = "00" + fs.data;
            }
            
        });
        
    }

    $scope.getDispatchNo();


    $scope.getDispatchFrom = function () {
        var disFrom = OrderDispatchService.getDispatchFrom();
        disFrom.then(function (fs) {
            if (fs.data == null) {
                $scope.Dispatcher = "N/A";
            } else {
                $scope.Dispatcher = fs.data;
            }

        });
    };

    $scope.getDispatchFrom();

    $scope.showModalForNew = function (event) {
        $scope.saveupdatebtn = "Save"
        $scope.editFlag = "N";
        $("#OrderdispatchModal").modal("toggle");
    }

    $scope.selectedDocument = null;
    $scope.selectedDealer = null;

    $scope.getAllDocument = function () {

        var allDoc = OrderDispatchService.GetAllDocument();
        //  console.log("allDoc=====================>>> " + JSON.stringify(allDoc));
        allDoc.then(function (allDocList) {
            debugger;
            $scope.DocumentList = allDocList.data;
            $scope.DocumentList.push({ "FORM_CODE": "100000", "FORM_EDESC": "All" });
        }, function () { $scope.CompanyMessage = "Error while getting company list" });
    };
    $scope.getAllDocument();

    $scope.getAllPartyType = function () {
        var allPartyType = OrderDispatchService.getAllPartyType();
        allPartyType.then(function (partyRes) {
            $scope.AllPartyType = partyRes.data;
            //$scope.PDCFormObject.CREATED_BY = partyRes.data.CREATED_BY;
        });
    };

    $scope.getAllPartyType();

    $scope.searchParameter = {
        DocumentCode: "",
        FromDate: "",
        ToDate:"",
        PlanningDate:"",
        PlanningTo: "",
        CompBrnhList:[]
    }

    $scope.CompBrnh = {
        Company: "",
        Branch: ""
    };

    $scope.CompBrnhArr = [];

    $scope.findOrderDispatchForDocument = function (selCompany) {
       // console.log("selCompanyAtFindOrder=================>>>>" + JSON.stringify(selCompany));
       // console.log("selCompanyAtFindOrder==========Length=======>>>>" + selCompany.length);
        if (selCompany.length > 0) {
            for (var i = 0; i < selCompany.length; i++) {
                $scope.CompBrnh = {
                    Company: selCompany[i].pre_branch_code,
                    Branch: selCompany[i].branch_Code
                }
                $scope.CompBrnhArr.push($scope.CompBrnh);
            }
        }

        console.log("selectedDocument====================>>>" + JSON.stringify($scope.selectedDocument));

        if ($scope.selectedDocument) {
           showloader();
            $scope.searchParameter = {
            CompBrnhList: $scope.CompBrnhArr,
            DocumentCode: $scope.selectedDocument.FORM_CODE,
            FromDate: $('#englishdatedocument').val(),
            ToDate: $('#englishdatedocumentTo').val(),
            PlanningDate : $filter('date')(new Date(), 'dd-MMM-yyyy'),
            PlanningTo : $filter('date')(new Date(), 'dd-MMM-yyyy'),
        };
        var orderDispatch = OrderDispatchService.FindAllDataToDispatch($scope.searchParameter);

        console.log("Order Dispatch==================>>>" + JSON.stringify(orderDispatch));
        orderDispatch.then(function (od) {
            if (od.data) {
                $scope.isDispatchGrid = 'Y';
                $scope.DispatchOrder = od.data;
                $scope.BindDispatchGrid($scope.DispatchOrder,$scope.isDispatchGrid);

            } else {
                console.log("Dadta is blacnk================");
              
                displayPopupNotification("no dispatch order to show", "warning");
            }
            }, function () { $scope.DispatchMessage = "Error while getting dispatch List" });
        } else {
            displayPopupNotification("Please select document for dispatching", "error");
            return;
        }
    };

    var checkedItems = [];
    var checkedIds = {};
    var multiSelectedBrach = [];
    $scope.checkedItemToSave = [];

    $scope.dispatchOrder = function () {
        debugger;
        for (i = 0; i < $scope.checkedItemToSave.length; i++) {
            $scope.checkedItemToSave[i]["PLANNING_QTY"] = $("#txtPlanning_" + $scope.checkedItemToSave[i].DISPATCH_NO).val();
            $scope.checkedItemToSave[i]["PENDING_TO_DISPATCH"] = $("#txtPendingToDispatch_" + $scope.checkedItemToSave[i].DISPATCH_NO).val();
            $scope.checkedItemToSave[i]["PLANNING_AMT"] = $("#txtPlanningAmount_" + $scope.checkedItemToSave[i].DISPATCH_NO).val();
        }
      
        var savedOrder = OrderDispatchService.SaveAllDispatchedOrder($scope.checkedItemToSave);
        savedOrder.then(function (so) {
            if (so.data == "Saved Successfully") {
                displayPopupNotification("Order succesfully dispatched .", "success");
                setTimeout(function () {
                    location.reload(true);
                },4000);
            } else {
               // alert(so.data)
                displayPopupNotification(so.data, "error");
            }
        }, function () { $scope.OrderMessage = "Error while saving dispatch Order !!" });

    };


    $scope.ConvertEngToNep = function () {
        console.log(this);

        var engdate = $("#englishDate5").val();
        var nepalidate = ConvertEngDateToNep(engdate);
        $("#nepaliDate5").val(nepalidate);
        $("#nepaliDate51").val(nepalidate);
    };

    $scope.ConvertNepToEng = function ($event) {

        //$event.stopPropagation();
        console.log($(this));
        var date = BS2AD($("#nepaliDate5").val());
        var date1 = BS2AD($("#nepaliDate51").val());
        $("#englishdatedocument").val($filter('date')(date, "dd-MMM-yyyy"));
        $("#englishdatedocument1").val($filter('date')(date1, "dd-MMM-yyyy"));
        $('#nepaliDate5').trigger('change')
        $('#nepaliDate51').trigger('change')
    };

    $scope.ConvertEngToNepang = function (data) {
        $("#nepaliDate5").val(AD2BS(data));
       
    };

    $scope.ConvertEngToNepang1 = function (data) {

         $("#nepaliDate51").val(AD2BS(data));
    }

    $scope.someDateFn = function () {

        var engdate = $filter('date')(new Date(new Date().setDate(new Date().getDate() - 1)), 'dd-MMM-yyyy');
        //var engdate1 = $filter('date')(new Date(new Date().setDate(new Date().getDate() - 2)), 'dd-MMM-yyyy');
        var a = ConvertEngDateToNep(engdate);
        //var a1 = ConvertEngDateToNep(engdate1);
        $scope.Dispatch_From = engdate;
        $scope.NepaliDate = a;
        $scope.Dispatch_To = a;
        $scope.PlanningTo = ConvertEngDateToNep($filter('date')(new Date(new Date().setDate(new Date().getDate())), 'dd-MMM-yyyy'));
      //  $scope.PlanningDate = a;

    };

    $scope.someDateFn();

    $scope.monthSelectorOptionsSingle = {
        open: function () {

            var calendar = this.dateView.calendar;

            calendar.wrapper.width(this.wrapper.width() - 6);
        },
        change: function () {

            $scope.ConvertEngToNepang(kendo.toString(this.value(), 'yyyy-MM-dd'))
        },
        format: "dd-MMM-yyyy",

       
        dateInput: true
    };

    $scope.monthSelectorOptionsSingle1 = {
        open: function () {

            var calendar = this.dateView.calendar;

            calendar.wrapper.width(this.wrapper.width() - 6);
        },
        change: function () {

            $scope.ConvertEngToNepang1(kendo.toString(this.value(), 'yyyy-MM-dd'))
        },
        format: "dd-MMM-yyyy",


        dateInput: true
    };

    $scope.viewPlannedReport = function () {

        if ($scope.selectedDocument.FORM_CODE) {
            showloader();
            $scope.searchParameter = {
                DocumentCode: $scope.selectedDocument,
                FromDate: $scope.Dispatch_From,
                PlanningDate: $scope.Dispatch_From
            };

            var allPlannedReport = OrderDispatchService.GetAllPlannedReport($scope.searchParameter);
            allPlannedReport.then(function (apl) {
                if (apl.data) {
                    $scope.isDispatchGrid = 'N';
                    $scope.PlannedReport = apl.data;
                    $scope.BindDispatchGrid($scope.PlannedReport,$scope.isDispatchGrid)
                } else {

                }
            }, function () { displayPopupNotification("Error while getting planned report", "error"); });
        } else {
            displayPopupNotification("Please select document for viewing plannned report!!!","error");
        }
       
    };

    $scope.openMultiBranch = function () {
        $("#dispatchTagModal").toggle();
    }

    $scope.close = function () {
        $("#dispatchTagModal").toggle();
    }

    $scope.BindDispatchGrid = function (data, isDispatchGrid) {
        checkedItems = [];
        if ($scope.isDispatchGrid == 'Y') {
          
            $("#dispatchGrid").kendoGrid({
                dataSource: {
                    data: data,
                    pageSize: 20,
                    schema: {
                        model: {
                            fields: {
                                QUANTITY: { type: "number" },
                                UNIT_PRICE: { type: "number" },
                                DUE_QTY: { type: "number" }
                            }
                        },
                    },
                },
                toolbar: kendo.template($("#toolbar-template").html()),
                width: 700,
                scrollable: true,
                filterable: true,
                //sortable: true,
                pageable: true,
                reorderable: true,
                resizable: true,
                columnMenu: true,
                dataBound: function (e) {
                    $(".checkbox").on("click", selectRow);

                    var view = this.dataSource.data();
                    for (var j = 0; j < checkedItems.length; j++) {
                        for (var i = 0; i < view.length; i++) {
                            if (checkedItems[j].VOUCHER_NO == view[i].VOUCHER_NO && checkedItems[j].SERIAL_NO == view[i].SERIAL_NO) {
                                this.tbody.find("tr[data-uid='" + view[i].uid + "']")
                                    .addClass("k-state-selected")
                                    .find(".checkbox")
                                    .attr("checked", "checked");
                            }
                        }
                    }
                    var grid = e.sender;
                    if (grid.dataSource.total() == 0) {
                        var colCount = grid.columns.length + 1;
                        $(e.sender.wrapper)
                            .find('tbody')
                            .append('<tr class="kendo-data-row" style="font-size:12px;"><td colspan="' + colCount + '" class="alert alert-danger">Sorry, No Data Found For Given Filter. </td></tr>');
                    }

                },
                columns: [
                    // { field: "DISPATCH_NO", title: "Order No", width: 5,hidden:true },
                    { field: "ORDER_NO", title: "Order No", width: 40 },
                    { field: "MITI", title: "BS Date", width: 25 },
                    { field: "PARTY_TYPE_EDESC", title: "Dealer Name", width: 50 },
                    { field: "CUSTOMER_EDESC", title: "Party Name", width: 50 },
                    { field: "ADDRESS", title: "Destination", width: 30 },
                    { field: "ITEM_EDESC", title: "Product Name", width: 40 },
                    { field: "QUANTITY", title: "Order Qty.", width: 25 },
                    { field: "UNIT_PRICE", title: "Rate", width: 20 },
                    {
                        template: "<input type='checkbox' id='isDispatch_#=DISPATCH_NO#' class='checkbox row-checkbox chkDispatch' onclick='toogleDispatch(this)' /><label class='k-checkbox-label' for='${ORDER_NO}'></label>",
                        width: 30,
                        title: "Disptch(Y/N)"
                    },
                    { field: "QUANTITY", title: "Order Pending Qty", width: 40 },
                    { field: "PendingToPlanning", title: "Pending To Planning", width: 40 },
                    { field: "PendingToDispatch", title: "Pending To dispatch", width: 40 },
                    {
                        template: '<input type="text"  id="txtPlanning_#=DISPATCH_NO#" onkeyup="validateDispatch(this)" style="color:black;display:none;" title="Double click to edit" />',
                        editable:true,
                        title: "Planning Qty", width: 30
                    },
                    {
                        template: '<input type="text" id="txtPendingToDispatch_#=DISPATCH_NO#" style="color:black;display:none" />',
                        title: "Pending To Dispatch", width: 35
                    },
                    {
                        template: '<input type="text" id="txtPlanningAmount_#=DISPATCH_NO#" style="color:black;display:none" />',
                        title: "Planning Amt", width: 25
                    },
                    {
                        field: "EXCISE_AMOUNT", title: "Excise Amt", width: 22
                    },
                    {
                        field: "VAT_AMOUNT", title: "VAT Amt", width: 22
                    }
                ]
            });

            hideloader();
            function selectRow() {
                var checked = this.checked,
                    row = $(this).closest("tr"),
                    grid = $("#dispatchGrid").data("kendoGrid"),
                    dataItem = grid.dataItem(row);
                debugger;
                if (checked) {
                    row.addClass("k-state-selected");
                    $(this).attr('checked', true);
                    checkedIds[dataItem.ORDER_NO] = checked;
                    checkedItems.push({
                        "DISPATCH_NO": dataItem.DISPATCH_NO,
                        "VOUCHER_DATE": dataItem.ORDER_DATE,
                        "VOUCHER_NO": dataItem.ORDER_NO,
                        "CUSTOMER_CODE": dataItem.CUSTOMER_CODE,
                        "FROM_LOCATION": $scope.Dispatcher,
                        "TO_LOCATION": "TEST",
                        "QUANTITY": dataItem.QUANTITY,
                        "COMPANY_CODE": dataItem.COMPANY_CODE,
                        "BRANCH_CODE": dataItem.BRANCH_CODE,
                        "MITI": dataItem.MITI,
                        "DEALER_NAME": dataItem.PARTY_TYPE_EDESC,
                        "PARTY_NAME": dataItem.CUSTOMER_EDESC,
                        "DESTINATION": dataItem.ADDRESS,
                        "PRODUCT_NAME": dataItem.ITEM_EDESC,
                        "ORDER_QTY": dataItem.QUANTITY,
                        "UNIT_PRICE": dataItem.UNIT_PRICE,
                        "ORDER_PENDING_QTY": dataItem.QUANTITY,
                        "PENDING_TO_PLANNING": dataItem.DUE_QTY,
                        "PLANNING_QTY": $("#txtPlanning_" + dataItem.DISPATCH_NO).val(),
                        "PENDING_TO_DISPATCH": $("#txtPendingToDispatch_" + dataItem.DISPATCH_NO).val(),
                        "PLANNING_AMT": $('#txtPlanningAmount_' + dataItem.DISPATCH_NO).val(),
                        "EXCISE_AMT": dataItem.EXCISE_AMT,
                        "VAT_AMT": dataItem.VAT_AMT,
                        "ITEM_CODE": dataItem.ITEM_CODE,
                        "FORM_CODE": dataItem.FORM_CODE,
                        "MANUAL_NO": $scope.Dispatch_no

                    });
                    $scope.checkedItemToSave = checkedItems;
                } else {

                    for (var i = 0; i < checkedItems.length; i++) {
                        if (checkedItems[i].VOUCHER_NO == dataItem.VOUCHER_NO && checkedItems[i].SERIAL_NO == dataItem.SERIAL_NO) {
                            checkedItems.splice(i, 1);
                        }
                    }
                    row.removeClass("k-state-selected");
                }
            }

        } else {
           
            $("#dispatchGrid").kendoGrid({
                dataSource: {
                    data: data,
                    pageSize: 20,
                    schema: {
                        model: {
                            fields: {
                                QUANTITY: { type: "number" },
                                UNIT_PRICE: { type: "number" },
                                DUE_QTY: { type: "number" }
                            }
                        },
                    },
                },
                toolbar: kendo.template($("#toolbar-template").html()),
                width: 700,
                scrollable: true,
                filterable: true,
                sortable: true,
                pageable: true,
                reorderable: true,
                resizable: true,
                columnMenu: true,
                dataBound: function (e) {
                    $(".checkbox").on("click", selectRow);

                    var view = this.dataSource.data();
                    for (var j = 0; j < checkedItems.length; j++) {
                        for (var i = 0; i < view.length; i++) {
                            if (checkedItems[j].VOUCHER_NO == view[i].VOUCHER_NO && checkedItems[j].SERIAL_NO == view[i].SERIAL_NO) {
                                this.tbody.find("tr[data-uid='" + view[i].uid + "']")
                                    .addClass("k-state-selected")
                                    .find(".checkbox")
                                    .attr("checked", "checked");
                            }
                        }
                    }
                    var grid = e.sender;
                    if (grid.dataSource.total() == 0) {
                        var colCount = grid.columns.length + 1;
                        $(e.sender.wrapper)
                            .find('tbody')
                            .append('<tr class="kendo-data-row" style="font-size:12px;"><td colspan="' + colCount + '" class="alert alert-danger">Sorry, No Data Found For Given Filter. </td></tr>');
                    }

                },
                columns: [
                    { field: "ORDER_NO", title: "Order No", width: 40 },
                    { field: "MITI", title: "BS Date", width: 25 },
                    { field: "ORDER_DATE", title: "Order Date", width: 25 },
                    
                    { field: "PARTY_TYPE_EDESC", title: "Dealer Name", width: 50 },
                    { field: "CUSTOMER_EDESC", title: "Party Name", width: 50 },
                    { field: "ADDRESS", title: "Destination", width: 30 },
                    { field: "ITEM_EDESC", title: "Product Name", width: 40 },
                    { field: "QUANTITY", title: "Order Qty.", width: 25 },
                    { field: "UNIT_PRICE", title: "Rate", width: 20 },
                    {
                        template: "<input type='checkbox' id='isDispatch_#=DISPATCH_NO#' class='checkbox row-checkbox chkDispatch'  disabled /><label class='k-checkbox-label' for='${ORDER_NO}'></label>",
                        width: 30,
                        title: "Disptch(Y/N)"
                    },

                    { field: "QUANTITY", title: "Order Pending Qty", width: 40 },

                    { field: "DUE_QTY", title: "Pending To Planning", width: 40 },
                    { field: "PendingToPlanning", title: "Pending To Planning New", width: 40 },
                    { field: "PendingToDispatch", title: "Pending To dispatch New", width: 40 },
                    {
                        template: '<input type="text"  id="txtPlanning_#=DISPATCH_NO#" onkeyup="validateDispatch(this)" style="display:none"/>',
                        title: "Planning Qty", width: 25
                    },
                    {
                        template: '<input type="text" id="txtPendingToDispatch_#=DISPATCH_NO#" style="display:none" />',
                        title: "Pending To Dispatch", width: 35
                    },
                    {
                        template: '<input type="text" id="txtPlanningAmount_#=DISPATCH_NO#" style="display:none" />',
                        title: "Planning Amt", width: 25
                    },
                    {
                        field: "EXCISE_AMOUNT", title: "Excise Amt", width: 22
                    },
                    {
                        field: "VAT_AMOUNT", title: "VAT Amt", width: 22
                    }
                ]
            });

            hideloader();
        }
    };
  
    $scope.printDispatch = function () {

        var gridElement = $('#dispatchGrid'),
            printableContent = '',
            win = window.open('', '', 'width=800, height=500, resizable=1, scrollbars=1'),
            doc = win.document.open();

        var htmlStart =
            '<!DOCTYPE html>' +
            '<html>' +
            '<head>' +
            '<meta charset="utf-8" />' +
            '<title>Print Dispatch Grid</title>' +
            '</head>' +
            '<body>';

        var htmlEnd =
            '</body>' +
            '</html>';

        var gridHeader = gridElement.children('.k-grid-header');
        if (gridHeader[0]) {
            var thead = gridHeader.find('thead').clone().addClass('k-grid-header');
            printableContent = gridElement
                .clone()
                .children('.k-grid-header').remove()
                .end()
                .children('.k-grid-content')
                .find('table')
                .first()
                .children('tbody').before(thead)
                .end()
                .end()
                .end()
                .end()[0].outerHTML;
        } else {
            printableContent = gridElement.clone()[0].outerHTML;
        }

        doc.write(htmlStart + printableContent + htmlEnd);
        doc.close();
        win.print();

    }
   
});

DTModule.service('OrderDispatchService', function ($http) {

    this.generateDispatchNo = function () {
        var disNo = $http({
            method: "GET",
            url: "/api/OrderDispatchApi/GenerateDispatchNo",
            dataType: "json",
        });
        return disNo;
    }

    this.getDispatchFrom = function () {
        var disNo = $http({
            method: "GET",
            url: "/api/OrderDispatchApi/GetDispatcher",
            dataType: "json",
        });
        return disNo;
    }

    this.GetAllDocument = function () {
        var allDocument = $http({
            method: "GET",
            url: "/api/OrderDispatchApi/GetAllDocument",
            dataType:"json",
        });
        return allDocument;
    };


    this.FindAllDataToDispatch = function (params) {
      
        var dataToDispatch = $http({
            method: "POST",
            url: "/api/OrderDispatchApi/FindDataToDispatch",
            data: JSON.stringify(params),
            dataType: "json",
        });

        return dataToDispatch;
    };


    this.SaveAllDispatchedOrder = function (params) {
        var dispatchedData = $http({
            method: "POST",
            url: "/api/OrderDispatchApi/SaveDispatchOrder",
            data: JSON.stringify(params),
            dataType: "json"

        });

        return dispatchedData;
    };

    this.GetAllPlannedReport = function (params) {
        var plannedReport = $http({
            method: "POST",
            url: "/api/OrderDispatchApi/GetAllPlannedReport",
            data:JSON.stringify(params),
            dataType:"json"
        });

        return plannedReport;
    };

    this.getAllPartyType = function () {
        var allPT = $http({
            method: "GET",
            url: "/api/CustomFormApi/GetAllPartyType",
            dataType: "JSON"
        });
        return allPT;
    };

});