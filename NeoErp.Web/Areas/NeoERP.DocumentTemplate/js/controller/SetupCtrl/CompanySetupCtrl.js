
DTModule.controller('CompanySetupCtrl', function ($scope, $rootScope, $http, $routeParams, $window, $filter) {
    $scope.saveupdatebtn = "Save";
    $scope.cmpanyArr;
    $scope.savegroup = false;
    $scope.editFlag = "N";
    $scope.treenodeselected = "N";
    $scope.treeSelectedDivisionCode = "";
    //To open the context menu and bind data use companydataFillDefered
    var companydataFillDefered = $.Deferred();

    $scope.companySetup =
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
            PARENT_BRANCH_CODE: '',
            LOGO_FILE_NAME: '',
            FOOTER_LOGO_FILE_NAME: ''
        }
    $scope.companyArr = $scope.companySetup;


    $scope.showModalForNew = function (event) {
        $scope.saveupdatebtn = "Save"
        $scope.editFlag = "N";
        $scope.clearFields();
        $("#CompanyModal").modal("toggle");
    }
    $scope.companyCenterChildGridOptions = {
        dataSource: {
            type: "json",
            transport: {
                read: "/api/SetupApi/GetCompanyGridData",
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
            //DisplayNoResultsFound($('#kGrid'));
            $("#kGrid tbody tr").on('dblclick', function () {
                var resourceCode = $(this).find('td span').html()
                $scope.edit(resourceCode);
                //var tree = $("#divisionRoottree").data("kendoTreeView");
                //tree.dataSource.read();
            })
        },
        columns: [
            {
                field: "COMPANY_CODE",
                title: "Company code",
                width: "80px"
            },
            {
                field: "COMPANY_EDESC",
                title: "Company Name",
                width: "120px"
            },
            {
                field: "ADDRESS",
                title: "Address",
                width: "130px"
            },
            {
                field: "EMAIL",
                title: "Email",
                width: "120px"
            },
            {
                field: "WEB",
                title: "Web",
                width: "120px"
            },

            {
                field: "TPIN_VAT_NO",
                title: "Vat No",
                width: "120px"
            },


            {
                title: "Action ",
                template: '<a class="fa fa-pencil-square-o editAction" title="Edit" ng-click="edit(#:COMPANY_CODE#)"><span class="sr-only"></span> </a><a class="fa fa-trash deleteAction" title="Delete" ng-click="delete(#:COMPANY_CODE#)"><span class="sr-only"></span> </a>',
                width: "60px"
            }
        ],


    };

    //Edit Function
    $scope.edit = function (cmpanyId) {
        debugger;
        $scope.editFlag = "Y";
        $scope.savegroup = false;
        $scope.saveupdatebtn = "Update";
        companydataFillDefered = $.Deferred();
        $scope.fillCompanySetupForms(cmpanyId);
        $.when(companydataFillDefered).done(function () {
            $("#CompanyModal").modal();
        });
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

                    //$scope.divisionsetup.GROUP_SKU_FLAG = "I";

                    var delUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/DeleteCompany?companyCode=" + code;
                    $http({
                        method: 'POST',
                        url: delUrl
                    }).then(function successCallback(response) {



                        if (response.data.MESSAGE == "DELETED") {
                            debugger;

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
    //for Bind while editing
    $scope.fillCompanySetupForms = function (cmpanyId) {
        debugger;
        var getResourcedetaisByUrl = window.location.protocol + "//" + window.location.host + "/api/SetupApi/getCompanyDetailsByCompanyCode?cmpanyId=" + cmpanyId;
        $http({
            method: 'GET',
            url: getResourcedetaisByUrl,

        }).then(function successCallback(response) {

            $scope.companySetup = response.data.DATA;
            $scope.companyArr = response.data.DATA;
            $scope.companyArr.COMPANY_CODE = $scope.companySetup.COMPANY_CODE;
            $scope.companyArr.VALID_DATE = $scope.companySetup.VALID_DATE.replace('T00:00:00', '');;




            companydataFillDefered.resolve(response);


        }, function errorCallback(response) {

        });
    }
    //Clear Fields
    $scope.clearFields = function () {

        $scope.companyArr.COMPANY_EDESC = "";
        $scope.companyArr.COMPANY_NDESC = "";
        $scope.companyArr.REMARKS = "";
        $scope.companyArr.ADDRESS = "";
        $scope.companyArr.TELEPHONE = " ";
        $scope.companyArr.EMAIL = " ";
        $scope.companyArr.FAX = "";
        $scope.companyArr.WEB = "";
        $scope.companyArr.VALID_DATE = "";
        $scope.companyArr.ABBR_CODE = "";
        $scope.companyArr.REGISTRATION_NO = " ";
        $scope.companyArr.LOGO_FILE_NAME = " ";
        $scope.companyArr.TPIN_VAT_NO = " ";
        $scope.companyArr.FOOTER_LOGO_FILE_NAME = " ";


    }
    

    //Save and Update Function
    $scope.saveNewCompany = function (isValid) {

        var validation = [
              { companyengname: $scope.companyform.companyengname.$invalid },
              { companynepname: $scope.companyform.companynepname.$invalid },
              { registration: $scope.companyform.registration.$invalid },
              { address: $scope.companyform.address.$invalid },
              { vatno: $scope.companyform.vatno.$invalid },
              { validtilldate: $scope.companyform.validtilldate.$invalid },
             

        ]

        if (validation[0].companyengname == true) {

            displayPopupNotification("Enter English Name", "warning");
            return
        }
        if (validation[1].companynepname == true) {

            displayPopupNotification("Enter Nepali Name ", "warning");

            return
        }
        if (validation[2].registration == true) {

            displayPopupNotification(" Enter Registration Number", "warning");

            return
        }

        if (validation[3].address == true) {

            displayPopupNotification(" Enter Address", "warning");

            return
        }
        if (validation[4].vatno == true) {

            displayPopupNotification("PAN Number Should be 9 Letter only", "warning");

            return
        }
        if (validation[5].validtilldate == true) {

            displayPopupNotification("Enter Date ", "warning");

            return
        }
       


        //if (!isValid) {
        //    displayPopupNotification("Input fields are not valid. Please review and try again", "warning");
        //    return;
        //}
        debugger;

        if ($scope.saveupdatebtn == "Save") {

            //file upload for Company Logo

            document.uploadFile();
            if ($('#logofile')[0].files[0] !== undefined) { $scope.companyArr.LOGO_FILE_NAME = $('#logofile')[0].files[0].name; }

            //File upload for footer logo
            document.uploadFile();
            if ($('#footerlogo')[0].files[0] !== undefined) { $scope.companyArr.FOOTER_LOGO_FILE_NAME = $('#footerlogo')[0].files[0].name; }


            var createurl = window.location.protocol + "//" + window.location.host + "/api/setupapi/createNewCompanyHead";
            $scope.data = 'none';
            var model = {
                COMPANY_CODE: $scope.companyArr.COMPANY_CODE,
                COMPANY_EDESC: $scope.companyArr.COMPANY_EDESC,
                COMPANY_NDESC: $scope.companyArr.COMPANY_NDESC,
                ADDRESS: $scope.companyArr.ADDRESS,
                TELEPHONE: $scope.companyArr.TELEPHONE,
                EMAIL: $scope.companyArr.EMAIL,
                FAX: $scope.companyArr.FAX,
                WEB: $scope.companyArr.WEB,
                VALID_DATE: $scope.companyArr.VALID_DATE,
                REMARKS: $scope.companyArr.REMARKS,
                FOOTER_LOGO_FILE_NAME: $scope.companyArr.FOOTER_LOGO_FILE_NAME,
                ABBR_CODE: $scope.companyArr.ABBR_CODE,
                REGISTRATION_NO: $scope.companyArr.REGISTRATION_NO,
                LOGO_FILE_NAME: $scope.companyArr.LOGO_FILE_NAME,
                TPIN_VAT_NO: $scope.companyArr.TPIN_VAT_NO,
            }
            debugger;
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

            var updateurl = window.location.protocol + "//" + window.location.host + "/api/setupapi/updateCompanyHead";

            //file upload for Company Logo

            document.uploadFile();
            if ($('#logofile')[0].files[0] !== undefined) { $scope.companyArr.LOGO_FILE_NAME = $('#logofile')[0].files[0].name; }
            else { $scope.companyArr.LOGO_FILE_NAME = $scope.companyArr.LOGO_FILE_NAME; }
            //File upload for footer logo
            document.uploadFile();
            if ($('#footerlogo')[0].files[0] !== undefined) { $scope.companyArr.FOOTER_LOGO_FILE_NAME = $('#footerlogo')[0].files[0].name; }
            else { $scope.companyArr.FOOTER_LOGO_FILE_NAME = $scope.companyArr.FOOTER_LOGO_FILE_NAME; }
            debugger;
            $scope.saveupdatebtn = "Update";
            var model = {
                COMPANY_CODE: $scope.companyArr.COMPANY_CODE,
                COMPANY_EDESC: $scope.companyArr.COMPANY_EDESC,
                COMPANY_NDESC: $scope.companyArr.COMPANY_NDESC,
                ADDRESS: $scope.companyArr.ADDRESS,
                TELEPHONE: $scope.companyArr.TELEPHONE,
                EMAIL: $scope.companyArr.EMAIL,
                FAX: $scope.companyArr.FAX,
                WEB: $scope.companyArr.WEB,
                VALID_DATE: $scope.companyArr.VALID_DATE,
                REMARKS: $scope.companyArr.REMARKS,
                FOOTER_LOGO_FILE_NAME: $scope.companyArr.FOOTER_LOGO_FILE_NAME,
                ABBR_CODE: $scope.companyArr.ABBR_CODE,
                REGISTRATION_NO: $scope.companyArr.REGISTRATION_NO,
                LOGO_FILE_NAME: $scope.companyArr.LOGO_FILE_NAME,
                TPIN_VAT_NO: $scope.companyArr.TPIN_VAT_NO,



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



    $scope.loadimage = function (img) {

        var imgfullpath = window.location.protocol + "//" + window.location.host + "/Areas/NeoERP.DocumentTemplate/images/" + img;

        $('#blah')
            .attr('src', imgfullpath)
            .width(140)
            .height(180);

        $('#txtFile')[0].files[0].name = img;
    };
});