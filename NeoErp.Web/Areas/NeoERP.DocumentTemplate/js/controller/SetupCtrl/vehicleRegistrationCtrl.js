DTModule.controller('vehicleRegistrationCtrl', function ($scope, $rootScope, $http, $routeParams, $window, $filter) {
    $scope.saveupdatebtn = "Save";
    
    $scope.savegroup = false;
    $scope.editFlag = "N";
    $scope.treenodeselected = "N";
    $scope.treeSelectedDivisionCode = "";
  
   
    $scope.showModalForNew = function (event) {
        $scope.saveupdatebtn = "Save"
        $scope.getTransactionCode("N");
        $scope.editFlag = "N";
        $scope.ClearField();
        
        var vehicledate = $filter('date')(new Date(), 'dd-MMM-yyyy');
        $scope.VehicleSetup.TRANSACTION_DATE = vehicledate;
        $scope.VehicleSetup.VEHICLE_IN_DATE = vehicledate;
        var dt = new Date();
        var time = dt.getHours() + ":" + dt.getMinutes() + ":" + dt.getSeconds();
        $scope.VehicleSetup.IN_TIME = $scope.timeConvert(time);
        $scope.VehicleSetup.LOAD_IN_TIME = $scope.timeConvert(time);

        $("#CompanyModal").modal("toggle");
    }

    $scope.VehicleSetup = {
        TRANSACTION_NO: '',
        REFERENCE_NO: '',
        REFERENCE_FORM_CODE: '',
        VEHICLE_NAME: '',
        TRANSACTION_DATE: '',
        REMARKS: '',
        VEHICLE_OWNER_NAME:'',
        VEHICLE_OWNER_NO: '',
        DRIVER_NAME: '',
        DRIVER_LICENCE_NO:'',
        DRIVER_MOBILE_NO: '',
        IN_TIME: '',
        OUT_TIME: '',
        LOAD_IN_TIME: '',
        LOAD_OUT_TIME: '',
        TEAR_WT: '',
        GROSS_WT: '',
        NET_WT: '',
        QUANTITY: '',
        DESTINATION: '',
        BROKER_NAME: '',
        VEHICLE_IN_DATE: '',
        VEHICLE_OUT_DATE: '',
        WB_SLIP_NO: '',
        TRANSPORT_NAME: '',
        TOTAL_VEHICLE_HR:''

    }
    
    $scope.vechOption = {
        
        dataTextField: "VEHICLE_EDESC",
        dataValueField: "VEHICLE_EDESC",
        maxSelectedItems: 1,
        valuePrimitive: true,
        autoClose: true,
        headerTemplate: '<div class="col-md-offset-3"><strong>Select Vehicle No...</strong></div>',
        placeholder: "...Select Vehicle No...",
       
        select: function (e) {
            debugger;
            if (e.dataItem !== undefined) {
                debugger;
                $scope.VehicleSetup.VEHICLE_OWNER_NAME = e.dataItem.OWNER_NAME;
                $scope.VehicleSetup.VEHICLE_OWNER_NO = e.dataItem.OWNER_MOBILE_NO;
                $scope.VehicleSetup.DRIVER_NAME = e.dataItem.DRIVER_NAME;
                $scope.VehicleSetup.DRIVER_LICENCE_NO = e.dataItem.DRIVER_LICENCE_NO;
                $scope.VehicleSetup.DRIVER_MOBILE_NO = e.dataItem.DRIVER_MOBILE_NO;
            }

        },

        dataSource: {
            transport: {
                read: {
                    url: window.location.protocol + "//" + window.location.host + "/api/SetupApi/getVehicleList",
                    dataType: "json"
                }
            }
        },

        dataBound: function (e) {
            $(".k-list.k-reset").slimScroll({ 'height': '179px', 'scroll': 'scroll' });
        },
       
    }
    


    $scope.fillVehicleRegSetupForms = function (transactionCode) {
      
        var getResourcedetaisByUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/getVehicleDetailsByvehicleCode?transactionCode=" + transactionCode;
        $http({
            method: 'GET',
            url: getResourcedetaisByUrl,

        }).then(function successCallback(response) {
    
            $scope.VehicleSetup = response.data.DATA;
        
            $scope.VehicleSetup.TRANSACTION_NO = $scope.VehicleSetup.TRANSACTION_NO;
            if ($scope.VehicleSetup.VEHICLE_OUT_DATE == "0001-01-01T00:00:00")
            {
                $scope.VehicleSetup.VEHICLE_OUT_DATE = "";
            }
            if ($scope.VehicleSetup.TRANSACTION_DATE == "0001-01-01T00:00:00") {
                $scope.VehicleSetup.TRANSACTION_DATE = "";
            }
            $scope.VehicleSetup.VEHICLE_IN_DATE = $scope.VehicleSetup.VEHICLE_IN_DATE.replace('T00:00:00', '');
            $scope.VehicleSetup.VEHICLE_OUT_DATE = $scope.VehicleSetup.VEHICLE_OUT_DATE==null?'':$scope.VehicleSetup.VEHICLE_OUT_DATE.replace('T00:00:00', '');
            $scope.VehicleSetup.TRANSACTION_DATE = $scope.VehicleSetup.TRANSACTION_DATE==null?'':$scope.VehicleSetup.TRANSACTION_DATE.replace('T00:00:00', '');
            $scope.VehicleSetup.IN_TIME = $scope.timeConvert($scope.VehicleSetup.IN_TIME);
            $scope.VehicleSetup.OUT_TIME = $scope.timeConvert($scope.VehicleSetup.OUT_TIME);
            $scope.VehicleSetup.LOAD_IN_TIME = $scope.timeConvert($scope.VehicleSetup.LOAD_IN_TIME);
            $scope.VehicleSetup.LOAD_OUT_TIME = $scope.timeConvert($scope.VehicleSetup.LOAD_OUT_TIME);
           


            VehicledataFillDefered.resolve(response);


        }, function errorCallback(response) {

        });
    }

   
    $scope.edit = function (transactionCode) {
        
        $scope.editFlag = "Y";
        $scope.savegroup = false;
        $scope.saveupdatebtn = "Update";
        VehicledataFillDefered = $.Deferred();
        $scope.fillVehicleRegSetupForms(transactionCode);
        $.when(VehicledataFillDefered).done(function () {
            
            $("#CompanyModal").modal();


        });


    }

    $scope.timeConvert = function (timeString)
    {
        if (timeString != null && timeString != "")
        {
            var timeTokens = timeString.split(':');
            return new Date(1970, 0, 1, timeTokens[0], timeTokens[1], 00);
        }
        else {
            return null;
        }

    }

    $scope.vehicleCenterChildGridOptions = {
        dataSource: {
            type: "json",
            transport: {
                read: "/api/SetupApi/GetVehicleRegistration",
            },
            pageSize: 20,
            serverSorting: true
        },
        scrollable: true,
        scrollable: true,
        height: 660,
        sortable: true,
        pageable: true,
        groupable: true,
        resizable: true,
        dataBound: function (e) {
          //  $('#kGrid table').height($(window).height() - 150);
            $("#kGrid tbody tr").css("cursor", "pointer");
          
            $("#kGrid tbody tr").on('dblclick', function () {
                
                var vehicle = $(this).find('td span').html()
                $scope.edit(vehicle);
              
            })
        },
        columns: [
            {
                field: "TRANSACTION_NO",
                title: " Transaction No",
                width: "80px"
            },
            {
                field: "VEHICLE_NAME",
                title: "Vehicle No",
                width: "130px"
            },
            {
                field: "DESTINATION",
                title: "Destination",
                width: "130px"
            },
            {
                field: "TRANSACTION_DATE",
                title: "Transaction Date",
                template: "#= kendo.toString(kendo.parseDate(TRANSACTION_DATE, 'yyyy-MM-dd'), 'MM/dd/yyyy') #",
                width: "130px"
            },
            {
                field: "MITI",
                title: "MITI",
                width: "130px"
            },

            {
                field: "TRANSPORT_NAME",
                title: "Transport Name",
                width: "130px"
            }, {
                field: "IN_TIME",
                title: "In Time",
                width: "130px"
            },
            {
                field: "OUT_TIME",
                title: "Out Time",
                width: "130px"
            },
            {
                field: "REMARKS",
                title: "Remarks",
                width: "130px"
            },
            {
                field: "TRANSPORTER_CODE",
                title: "Transporter Code",
                width: "130px"
            },

            {
                field: "REFERENCE_NO",
                title: "Reference No",
                width: "130px"
            },
            {
                title: "Action ",
                template: '<a class="fa fa-pencil-square-o editAction" title="Edit" ng-click="edit(#:TRANSACTION_NO#)"><span class="sr-only"></span> </a><a class="fa fa-trash deleteAction" title="Delete" ng-click="delete(#:TRANSACTION_NO#)"><span class="sr-only"></span> </a>',
                width: "60px"
            }
        ],


    };

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

                  

                    var delUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/DeleteVehicle?vehicleCode=" + code;
                    $http({
                        method: 'POST',
                        url: delUrl
                    }).then(function successCallback(response) {



                        if (response.data.MESSAGE == "DELETED") {
                            

                            $("#CompanyModal").modal("hide");

                            var grid = $('#kGrid').data("kendoGrid");
                            grid.dataSource.read();
                            bootbox.hideAll();

                            displayPopupNotification("Data succesfully deleted ", "success");
                        }

                    }, function errorCallback(response) {

                        displayPopupNotification(response.data.STATUS_CODE, "error");

                    });

                }
                else if (result == false) {


                    $("#CompanyModal").modal("hide");
                    bootbox.hideAll();
                }

            }
        });
    }
    
    $scope.calculateNetWeight=function()
    {
        
        $scope.VehicleSetup.NET_WT = $scope.VehicleSetup.GROSS_WT - $scope.VehicleSetup.TEAR_WT;
    }

    $scope.saveNewVehicleReg = function (isValid) {

        if (!isValid) {
            displayPopupNotification("Input fields are not valid. Please review and try again", "warning");
            return;
        }
     
        if ($scope.saveupdatebtn == "Save") {


            var createurl = window.location.protocol + "//" + window.location.host + "/api/setupapi/createVehicleRegistration";
            $scope.data = 'none';

            var model = {
                TRANSACTION_NO: $scope.VehicleSetup.TRANSACTION_NO,
                VEHICLE_NAME: $scope.VehicleSetup.VEHICLE_NAME[0],
                REMARKS: $scope.VehicleSetup.REMARKS,
                VEHICLE_OWNER_NAME: $scope.VehicleSetup.VEHICLE_OWNER_NAME,
                VEHICLE_OWNER_NO: $scope.VehicleSetup.VEHICLE_OWNER_NO,
                DRIVER_NAME: $scope.VehicleSetup.DRIVER_NAME,
                DRIVER_LICENCE_NO: $scope.VehicleSetup.DRIVER_LICENCE_NO,
                DRIVER_MOBILE_NO: $scope.VehicleSetup.DRIVER_MOBILE_NO,
                IN_TIME: $scope.VehicleSetup.IN_TIME,
                OUT_TIME: $scope.VehicleSetup.OUT_TIME,
                LOAD_IN_TIME: $scope.VehicleSetup.LOAD_IN_TIME,
                LOAD_OUT_TIME: $scope.VehicleSetup.LOAD_OUT_TIME,
                TEAR_WT: $scope.VehicleSetup.TEAR_WT,
                QUANTITY: $scope.VehicleSetup.QUANTITY,
                GROSS_WT: $scope.VehicleSetup.GROSS_WT,
                NET_WT: $scope.VehicleSetup.NET_WT,
                DESTINATION: $scope.VehicleSetup.DESTINATION,
                BROKER_NAME: $scope.VehicleSetup.BROKER_NAME,
                VEHICLE_IN_DATE: $scope.VehicleSetup.VEHICLE_IN_DATE,
                VEHICLE_OUT_DATE: $scope.VehicleSetup.VEHICLE_OUT_DATE,
                WB_SLIP_NO: $scope.VehicleSetup.WB_SLIP_NO,
                TRANSPORT_NAME: $scope.VehicleSetup.TRANSPORT_NAME, 
                TRANSACTION_DATE: $scope.VehicleSetup.TRANSACTION_DATE,
                TOTAL_VEHICLE_HR: $scope.VehicleSetup.TOTAL_VEHICLE_HR,

            } 
           
            $http({
                method: 'post',
                url: createurl,
                data: model
            }).then(function successcallback(response) {

                if (response.data.MESSAGE == "INSERTED") {

                    $scope.companyArr = [];
                    $scope.companyArr.COMPANY_EDESC = "";

                    if ($scope.savegroup == false) { $("#CompanyModal").modal("toggle"); }
                    else { $("#CompanyModal").modal("toggle"); }

                    var grid = $("#kGrid").data("kendoGrid");
                    if (grid != undefined) {

                        grid.dataSource.read();
                    }

                    displayPopupNotification("data succesfully saved ", "success");
                }
                else {
                    displayPopupNotification("Something went wrong.Please try again later.", "error");
                }

            }, function errorcallback(response) {
                displayPopupNotification("Something went wrong.Please try again later.", "error");

            });
        }
        else {

            var updateurl = window.location.protocol + "//" + window.location.host + "/api/setupapi/updateVehicleRegistration";
         
            $scope.saveupdatebtn = "Update";
            var model = {
                TRANSACTION_NO: $scope.VehicleSetup.TRANSACTION_NO,
                VEHICLE_NAME: $scope.VehicleSetup.VEHICLE_NAME,
                REMARKS: $scope.VehicleSetup.REMARKS,
                VEHICLE_OWNER_NAME: $scope.VehicleSetup.VEHICLE_OWNER_NAME,
                VEHICLE_OWNER_NO: $scope.VehicleSetup.VEHICLE_OWNER_NO,
                DRIVER_NAME: $scope.VehicleSetup.DRIVER_NAME,
                DRIVER_LICENCE_NO: $scope.VehicleSetup.DRIVER_LICENCE_NO,
                DRIVER_MOBILE_NO: $scope.VehicleSetup.DRIVER_MOBILE_NO,
                IN_TIME: $scope.VehicleSetup.IN_TIME,
                OUT_TIME: $scope.VehicleSetup.OUT_TIME,
                LOAD_IN_TIME: $scope.VehicleSetup.LOAD_IN_TIME,
                LOAD_OUT_TIME: $scope.VehicleSetup.LOAD_OUT_TIME,
                TEAR_WT: $scope.VehicleSetup.TEAR_WT,
                QUANTITY: $scope.VehicleSetup.QUANTITY,
                GROSS_WT: $scope.VehicleSetup.GROSS_WT,
                NET_WT: $scope.VehicleSetup.NET_WT,
                DESTINATION: $scope.VehicleSetup.DESTINATION,
                BROKER_NAME: $scope.VehicleSetup.BROKER_NAME,
                VEHICLE_IN_DATE: $scope.VehicleSetup.VEHICLE_IN_DATE,
                VEHICLE_OUT_DATE: $scope.VehicleSetup.VEHICLE_OUT_DATE,
                WB_SLIP_NO: $scope.VehicleSetup.WB_SLIP_NO,
                TRANSPORT_NAME: $scope.VehicleSetup.TRANSPORT_NAME,
                TRANSACTION_DATE: $scope.VehicleSetup.TRANSACTION_DATE,
                TOTAL_VEHICLE_HR: $scope.VehicleSetup.TOTAL_VEHICLE_HR,
            }
            $http({
                method: 'post',
                url: updateurl,
                data: model
            }).then(function successcallback(response) {


                if (response.data.MESSAGE == "UPDATED") {
                    $("#CompanyModal").modal("toggle");
                    var grid = $("#kGrid").data("kendoGrid");

                    if (grid != undefined) {
                        grid.dataSource.read();
                    }
                    displayPopupNotification("data succesfully updated ", "success");
                }
                if (response.data.MESSAGE == "error") {
                    displayPopupNotification("Something went wrong.Please try again later.", "error");
                }

            }, function errorcallback(response) {

                displayPopupNotification("Something went wrong.Please try again later.", "error");

            });

        }

    }

    $scope.ClearField = function ()
    {
      $scope.saveupdatebtn = "Save";
      $scope.VehicleSetup = {
        VEHICLE_NAME: "",
        TRANSACTION_DATE: "",
        VEHICLE_OWNER_NAME: "",
        VEHICLE_OWNER_NO: "",
        IN_TIME: "",
        OUT_TIME: "",
        DRIVER_NAME: "",
        DRIVER_LICENCE_NO: "",
        DRIVER_MOBILE_NO: "",
        LOAD_IN_TIME: "",
        LOAD_OUT_TIME: "",
        TEAR_WT: "",
        GROSS_WT: "",
        NET_WT: "",
        QUANTITY: "",
        REMARKS: "",
        DESTINATION: "",
        BROKER_NAME: "",
        VEHICLE_IN_DATE: "",
        VEHICLE_OUT_DATE: "",
        WB_SLIP_NO: "",
       TOTAL_VEHICLE_HR:"",


      };
    
    }


    $scope.getTransactionCode = function (gFlag) {
        var gettrancsactionCodeByUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/getMaxTransactionCode?gFlag=" + gFlag;;
        $http({
            method: 'GET',
            url: gettrancsactionCodeByUrl,

        }).then(function successCallback(response) {

            $scope.VehicleSetup.TRANSACTION_NO = response.data.DATA;  
        }, function errorCallback(response) {
        });
    }

});

