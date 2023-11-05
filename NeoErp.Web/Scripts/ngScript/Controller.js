app.directive('select2', function ($timeout) {
    return {
        restrict: 'AC',
        link: function (scope, element, attrs) {
            $timeout(function () {
                element.show();
                $(element).select2();
            });
        }
    };
});

app.controller("menuSettingController", function ($scope, crudAJService, $timeout) {

    $scope.divMenu = false;
    $scope.divConfirm = false;
    GetAllMenus();
    GetModuleForMenu();
    //To Get all menu records  
    function GetAllMenus() {
        //$scope.gridOptions = {
        //    pageable: true,
        //    dataSource: {
        //        pageSize: 5,
        //        transport: {
        //            read: function (e) {
        //                var getMenuData = crudAJService.getMenus();
        //                getMenuData.then(function success(response) {
        //                    e.success(response.data)
        //                }, function error(response) {
        //                    alert('something went wrong')
        //                    console.log(response);
        //                })
        //            },
        //            update: function (e) {
        //                editMenu(e);
        //            },
        //            destroy: function(e) {
        //                deleteMenu(e);
        //            },
        //            create:function(e) {
        //                AddUpdateMenu();
        //            },
        //        }
        //    },
        //    columns: [{
        //        field: "MENU_NO",
        //        title: "Menu NO",
        //        width: "120px"
        //    }, {
        //        field: "MENU_EDESC",
        //        title: "Menu EDESC",
        //        width: "120px"
        //    }, {
        //        field: "MODULE_CODE",
        //        title: "Module Name",
        //        width: "120px"
        //    }, {
        //        field: "MENU_EDESC",
        //        title: "Full Path",
        //        width: "120px"
        //    }, {
        //        field: "MENU_EDESC",
        //        title: "Virtual Path",
        //        width: "120px"
        //    },
        //    { command: ["edit", "destroy"], title: "&nbsp;", width: 150 }],
        //    editable: {
        //        mode: "popup",
        //        template: kendo.template($("#template").html())
        //    }
        //};

        var getMenuData = crudAJService.getMenus();
        getMenuData.then(function (menu) {
            RemoveDataTableGrid();
            $scope.Menus = menu.data;
            $timeout(function () {
                DataTableGrid();
            }, 100)

        }, function () {
            toastr.error('Error in getting records');
        });
        GetPreMenu();
    }

    $scope.ReloadGrid = function () {
        GetAllMenus();
        GetModuleForMenu();
    };
    $scope.selectedItem;
    $scope.selectedPreMenuItem;
    $scope.GROUP_SKU_FLAG = "G";
    $scope.isVisible = false;
    //-----------------------------------------------------------------------------------------------

    $scope.moduleModel = [];
    $scope.menufordelete = [];

    $scope.moduleSettings = {
        smartButtonMaxItems: 3,
        smartButtonTextConverter: function (itemText, originalItem) {
            return itemText;
        }
    };


    //-----------------------------------------------------------------------------------------------
    $scope.ShowHide = function () {
        if ($scope.Action == "Update") {
            if ($scope.GROUP_SKU_FLAG == "G") {
                $scope.isVisible = false;
                $scope.VIRTUAL_PATH = 'javascript:;';
                $scope.FULL_PATH = '/' + $scope.MENU_EDESC;

            }
            else {
                $scope.isVisible = true;
                $scope.VIRTUAL_PATH = $scope.VIRTUAL_PATH;
                $scope.FULL_PATH = $scope.FULL_PATH;
            }
        }
        else {
            if ($scope.GROUP_SKU_FLAG == "G") {
                $scope.isVisible = false;
                $scope.VIRTUAL_PATH = 'javascript:;';
                $scope.FULL_PATH = '/' + $scope.MENU_EDESC;

            }
            else {
                $scope.isVisible = true;
                $scope.VIRTUAL_PATH = '';
                $scope.FULL_PATH = '';
            }
        }
    }
    $scope.editMenu = function (menu) {

        ClearFields();
        var dd = menu.COLOR.trim();
        $("#colorpicker").val(dd)

        var getMenuData = crudAJService.getMenu(menu.MENU_NO);
        getMenuData.then(function (_menu) {
            $scope.menu = _menu.data;
            $scope.MENU_NO = menu.MENU_NO;
            $scope.MENUNO = menu.MENUNO;
            $scope.MENU_EDESC = menu.MENU_EDESC;
            $scope.selectedItem = menu.MODULE_CODE;
            $scope.MENU_OBJECT_NAME = menu.MENU_OBJECT_NAME;
            $scope.FULL_PATH = menu.FULL_PATH;
            $scope.VIRTUAL_PATH = menu.VIRTUAL_PATH;
            $scope.ICON_PATH = menu.ICON_PATH;
            $scope.selectedPreMenuItem = menu.PRE_MENU_NO;
            $scope.COMPANY_CODE = menu.COMPANY_CODE;
            $scope.GROUP_SKU_FLAG = menu.GROUP_SKU_FLAG;
            $scope.CREATED_BY = menu.CREATED_BY;
            $scope.CREATED_DATE = menu.CREATED_DATE;
            $scope.moduleAbbs = menu.MODULE_ABBR;
            $scope.menuColor = menu.COLOR;
            //  $scope.colorpicker = menu.COLOR,
            $scope.menuDescription = menu.DESCRIPTION;
            $scope.Action = "Update";
            $scope.divMenu = true;
            $scope.ShowHide();
            CheckBoxChecked(menu.GROUP_SKU_FLAG);
            $timeout(function () {
                testSelect2();
            }, 10);
        }, function () {
            toast.error("Error in getting records");
            //alert('Error in getting records');
        });
    }
    $scope.AddUpdateMenu = function (isValid) {


        var value = $("#IconPath").val();
        var dd = $('#IconPath').val().indexOf(' ') >= 0;
        if (dd == false) {
            var CC = "fa";
            var ICON_PATH = CC + ' ' + value;
        }
        else {
            // alert("hehe");
            ICON_PATH = value;
        }
        if (isValid) {
            var Menu = {
                MENU_NO: $scope.MENU_NO,
                MENUNO: $scope.MENUNO,
                MENU_EDESC: $scope.MENU_EDESC,
                MODULE_CODE: $scope.selectedItem,
                MENU_OBJECT_NAME: $scope.MENU_OBJECT_NAME,
                FULL_PATH: $scope.FULL_PATH,
                VIRTUAL_PATH: $scope.VIRTUAL_PATH,
                ICON_PATH: ICON_PATH,
                PRE_MENU_NO: $scope.selectedPreMenuItem,
                COMPANY_CODE: $scope.COMPANY_CODE,
                GROUP_SKU_FLAG: $scope.GROUP_SKU_FLAG,
                CREATED_BY: $scope.CREATED_BY,
                MODULE_ABBR: $scope.moduleAbbs,
                COLOR: $("#colorpicker").val(),
                DESCRIPTION: $scope.menuDescription,
                CREATED_DATE: $scope.CREATED_DATE
            };
            var getMenuAction = $scope.Action;
            if (getMenuAction == "Update") {
                Menu.MENU_NO = $scope.MENU_NO;
                var getMenuData = crudAJService.updateMenu(Menu);
                getMenuData.then(function (msg) {
                    GetAllMenus();
                    toastr.success(msg.data);
                    //alert(msg.data);
                    $scope.divMenu = false;
                    GetMenuInTreeView();
                }, function () {
                    toastr.error('Error in updating record');
                    //alert('Error in updating record');
                });
            } else {
                var getMenuData = crudAJService.AddMenu(Menu);
                getMenuData.then(function (msg) {
                    GetAllMenus();
                    toastr.success(msg.data);
                    //alert(msg.data);
                    $scope.divMenu = false;
                    GetMenuInTreeView();
                }, function () {
                    toastr.error('Error in adding record');
                    //alert('Error in adding record');
                });
            }
        }
        else {

            toastr.error("There is empty field.");
        }
    }
    $scope.AddMenuDiv = function (menu) {

        ClearFields();
        $scope.moduleAbbs = "";
        $scope.menuColor = "";
        $scope.colorpicker = "";
        $scope.menuDescription = "";
        $scope.selectedItem = menu.MODULE_CODE;
        $scope.selectedPreMenuItem = menu.PRE_MENU_NO;
        $scope.Action = "Add New";
        $scope.VIRTUAL_PATH = 'javascript:;';
        $scope.FULL_PATH = '/' + $scope.MENU_EDESC;
        $timeout(function () {
            $scope.divMenu = true;
            $scope.$apply()
            testSelect2();
        }, 10);

    }
    $scope.tempMenu;
    $scope.setTemp = function (menu) {

        $scope.tempMenu = menu;
        $timeout(function () {
            $scope.divConfirm = true;
        }, 100)
    }

    $scope.deleteMenu = function () {
        var getMenuData = crudAJService.DeleteMenu($scope.tempMenu.MENU_NO);
        getMenuData.then(function (msg) {
            toastr.success(msg.data);
            $timeout(function () {
                $scope.divConfirm = false;
                GetAllMenus();
            }, 100)
            GetMenuInTreeView();
        }, function () {
            toastr.error('Error in deleting record');
        });

    }
    function GetModuleForMenu() {
        var getModuleData = crudAJService.GetModule();
        getModuleData.then(function (module) {
            $scope.modules = module.data;
            $scope.moduleData = module.data;
        });
    }
    function ClearFields() {
        $scope.menu = "";
        $scope.MENU_EDESC = "";
        $scope.MENU_OBJECT_NAME = "";
        $scope.selectedPreMenuItem = "";
        $scope.FULL_PATH = "";
        $scope.VIRTUAL_PATH = "";
        $scope.ICON_PATH = "";
        $scope.COMPANY_CODE = "";
        $scope.selectedItem = "";
    }
    $scope.Cancel = function () {

        $scope.divMenu = false;
        $scope.divConfirm = false;

    };
    function GetPreMenu() {
        var getPreMenu = crudAJService.GetPreMenu();
        getPreMenu.then(function (response) {
            $scope.preMenuItem = response.data;
        })
    }
});

app.controller("menuControlController", function ($scope, menuControlService, $timeout) {
    GetMenu();
    GetUser();
    $scope.GetAllMenuControl = function () {

        //var getMenuControlData = menuControlService.GetMenuControl();
        //getMenuControlData.then(function (menuControl) {
        //    RemoveDataTableGrid();
        //    $scope.MenuControls = menuControl.data;
        //    $timeout(function () {
        //        DataTableGrid();
        //    }, 100)
        //}, function () {
        //    $scope.lblMessage = "There is no controls.";
        //});
        params = ReportFilter.filterAdditionalData();
        var getMenuControlData = menuControlService.GetAllMenuControl(params);

        getMenuControlData.then(function (menuControl) {
            RemoveDataTableGrid();
            $scope.MenuControls = menuControl.data;
            $scope.Bind();
            $timeout(function () {
                DataTableGrid();
            }, 100)
        }, function () {
            $scope.lblMessage = "There is no controls.";
        });
    };
    $scope.Bind = function () {
        $('.k-list').slimScroll({
            height: '250px'
        });
    };
    $scope.GetAllMenuControl();
    $scope.ReloadGrid = function () {
        GetAllMenuControl();
        GetMenu();
        GetUser();
    }

    $scope.selectedUserItem;
    $scope.selectedMenuItem;
    $scope.GROUP_SKU_FLAG = 'G';
    $scope.ACCESS_FLAG = 'Y';
    //----------------------------------------multiple dropdown------------------------------------------------------

    $scope.menuModel = [];
    $scope.select2Options = {
        allowClear: true
    };

    $scope.menuControl = {
        smartButtonMaxItems: 3,
        enableSearch: true,
        smartButtonTextConverter: function (itemText, originalItem) {
            return itemText;
        }
    };

    $scope.userControl = {
        smartButtonMaxItems: 1,
        enableSearch: true,
        smartButtonTextConverter: function (itemText, originalItem) {
            return itemText;
        }
    }

    $scope.bulkmenuModel = [];

    $scope.bulkuserModel = [];

    //var out = [];

    //angular.forEach(data, function (obj) {
    //    out.push({
    //        Name: obj.id,
    //        Data: obj.name
    //    });
    //});
    //----------------------------------------------------------------------------------------------
    $scope.editMenuControl = function (menuControl) {

        var getMenuControlData = menuControlService.getMenuControl(menuControl.MENU_NO, menuControl.USER_NO);
        getMenuControlData.then(function (response) {

            $scope.menu = response.data;
            $scope.selectedUserItem = menuControl.USER_NO.toString();
            $scope.menuControl = menuControl.MENU_NO;
            $scope.selectedItem = menuControl.MENU_NO;
            $scope.COMPANY_EDESC = menuControl.COMPANY_EDESC;
            $scope.ACCESS_FLAG = menuControl.ACCESS_FLAG;
            $scope.BRANCH_CODE = menuControl.BRANCH_CODE;
            $scope.Action = "Update";
            $scope.divMenuControl = true;
            $timeout(function () {
                testSelect2("Update");
            }, 100);
        }, function () {
            toastr.error('Error while fetching data.');
        })
    }
    $scope.AddBulkMenuControlDiv = function () {
        $scope.BulkPermissionDiv = true;
    }

    $scope.USER_NO;

    $scope.AddBulkMenu = function () {

        var multipleMenu = _.pluck($scope.bulkmenuModel, 'id').join(', ');
        var multipleUser = _.pluck($scope.bulkuserModel, 'id').join(', ');
        var Menu = {
            MENU_NO: multipleMenu,
            USER_NO: multipleUser
        }
        var getMenuData = menuControlService.AddBulkUserAndMenu(Menu);
        getMenuData.then(function (msg) {
            $scope.GetAllMenuControl();
            toastr.success(msg.data);
            $scope.BulkPermissionDiv = false;
        }, function () {
            toastr.error('Error in adding record');
        });
    }

    $scope.AddUpdateMenuControl = function () {
        //$scope.MenuControls = "";
        //var multipleMenu = _.pluck($scope.menuModel, 'id').join(', ');
        var Menu = {
            MENU_NO: $scope.selectedItem,
            MENUNO: $scope.menuControl,
            USER_NO: $scope.selectedUserItem,
            BRANCH_CODE: $scope.BRANCH_CODE,
            COMPANY_EDESC: $scope.COMPANY_EDESC,
            ACCESS_FLAG: $scope.ACCESS_FLAG
        };
        var getMenuAction = $scope.Action;

        if (getMenuAction === "Update") {
            Menu.USER_NO = $scope.selectedUserItem;
            Menu.MENU_NO = $scope.selectedItem;
            Menu.MENUNO = $scope.menuControl;
            var getMenuData = menuControlService.updateMenuControl(Menu);
            getMenuData.then(function (msg) {
                if (msg.data === "Menu Control record updated invalid") {
                    toastr.warning(msg.data);
                } else {
                    toastr.success(msg.data);
                    $scope.divMenuControl = false;
                    $('.select2-container').select2('val', '');
                }
                $scope.GetAllMenuControl();

            }, function () {
                toastr.error('Error in updating record');
            });
        } else {
            var getMenuData = menuControlService.AddMenuControl(Menu);
            getMenuData.then(function (msg) {
                $scope.GetAllMenuControl();
                toastr.success(msg.data);
                $scope.divMenuControl = false;
                $('.select2-container').select2('val', '');
            }, function () {
                toastr.error('Error in adding record');
            });
        }
    }

    $scope.AddMenuControlDiv = function () {
        debugger;
        ClearFields();
        $timeout(function () {
            testSelect2("Add");
        }, 10);
        $scope.Action = "Add";
        $scope.divMenuControl = true;
    }
    $scope.tempMenu;
    $scope.setTemp = function (menu) {
        $scope.tempMenu = menu;
        $timeout(function () {
            $scope.divConfirm = true;
        }, 100)
    }
    $scope.deleteMenuControl = function () {
        var getMenuData = menuControlService.DeleteMenuControl($scope.tempMenu.MENU_NO, $scope.tempMenu.USER_NO);
        getMenuData.then(function (msg) {
            $scope.divConfirm = false;
            toastr.success(msg.data);
            $scope.GetAllMenuControl();
        }, function () {
            toastr.error('Error in deleting record');
        });
    }

    function GetUser() {
        getModuleData = menuControlService.GetUser();
        getModuleData.then(function (module) {
            $scope.users = module.data;
            $scope.bulkuserData = module.data;
        });
    }
    function GetMenu() {
        getModuleData = menuControlService.GetMenu();
        getModuleData.then(function (module) {
            $scope.menus = module.data;
            $scope.menuData = module.data;
            $scope.bulkmenuData = module.data;
        });
    }
    function ClearFields() {

        $scope.MENU_EDESC = "";
        $scope.USER_NO = "";
        $scope.MENU_NO = "";
        $scope.MENUNO = "";
        $scope.ACCESS_FLAG = "";
        $scope.selectedMenuItem = "";
        $scope.selectedUserItem = "";
        $scope.bulkmenuModel = [];
        $scope.bulkuserModel = [];
        $('.select2-container').select2('val', '');
    }

    $scope.Cancel = function () {
        ClearFields();

        $scope.divMenuControl = false;
        $scope.BulkPermissionDiv = false;
        $scope.divConfirm = false;

    };
});

app.controller("userWiseMenuPermissionCtrl", function ($scope, menuControlService, crudAJService) {
    GetAllMenuControl();
    GetMenu();
    GetUser();
    GetModule();
    function GetAllMenuControl() {
        var getMenuControlData = menuControlService.GetMenuControl();
        getMenuControlData.then(function (menuControl) {
            //document.DeleteDataTable();
            $scope.MenuControls = menuControl.data;
        }, function () {
            $scope.lblMessage = "There is no controls.";
        });
    }

    $scope.selectedUserItem;
    $scope.selectedMenuItem;
    $scope.editMenuControl = function (menuControl) {

        var getMenuControlData = menuControlService.GetMenuControlByID(menuControl.MENU_NO);
        getMenuControlData.then(function (response) {
            $scope.menuControl = response.data;
            $scope.selectedUserItem = menuControl.USER_NO;
            $scope.selectedMenuItem = menuControl.MENU_NO;
            $scope.COMPANY_CODE = menuControl.COMPANY_CODE;
            $scope.ACCESS_FLAG = menuControl.ACCESS_FLAG;
            $scope.BRANCH_CODE = menuControl.BRANCH_CODE;
            $scope.Action = "Update";
            $scope.divMenuControl = true;
        }, function () {
            toastr.error('Error while fetching data.');
        })
    }
    $scope.AddUpdateMenuControl = function () {

        var Menu = {
            MENU_NO: $scope.selectedMenuItem,
            MENUNO: $scope.selectedMenuItem,
            USER_NO: $scope.selectedUserItem,
            BRANCH_CODE: $scope.BRANCH_CODE,
            COMPANY_CODE: $scope.COMPANY_CODE,
            ACCESS_FLAG: $scope.ACCESS_FLAG
        };
        var getMenuAction = $scope.Action;

        if (getMenuAction === "Update") {
            Menu.USER_NO = $scope.USER_NO;
            Menu.MENU_NO = $scope.MENU_NO;
            Menu.MENUNO = $scope.MENUNO;
            var getMenuData = menuControlService.updateMenuControl(Menu);
            getMenuData.then(function (msg) {
                $scope.GetAllMenuControl();
                toastr.success(msg.data);
                $scope.divMenuControl = false;
            }, function () {
                toastr.error('Error in updating record');
            });
        } else {
            var getMenuData = menuControlService.AddMenuControl(Menu);
            getMenuData.then(function (msg) {
                $scope.GetAllMenuControl();
                toastr.success(msg.data);
                $scope.divMenuControl = false;
            }, function () {
                toastr.error('Error in adding record');
            });
        }
    }

    $scope.AddMenuPermissionDiv = function () {
        ClearFields();
        //GetMenu();
        //GetUser();
        $scope.Action = "Add";
        $scope.divMenuPermission = true;
    }

    $scope.deleteMenuControl = function (menu) {
        var getMenuData = menuControlService.DeleteMenuControl(menu.MENU_NO, menu.USER_NO);
        getMenuData.then(function (msg) {
            toastr.success(msg.data);
            $scope.GetAllMenuControl();
        }, function () {
            toastr.error('Error in deleting record');
        });
    }

    function GetUser() {
        getModuleData = menuControlService.GetUser();
        getModuleData.then(function (user) {
            $scope.users = user.data;
        });
    }
    function GetMenu() {
        getModuleData = menuControlService.GetMenu();
        getModuleData.then(function (module) {
            $scope.menus = module.data;
        });
    }
    function GetModule() {
        var getModuleData = crudAJService.GetModule();
        getModuleData.then(function (module) {
            $scope.modules = module.data;
        });
    }
    function ClearFields() {
        $scope.MENU_EDESC = "";
        $scope.USER_NO = "";
        $scope.MENU_NO = "";
        $scope.ACCESS_FLAG = "";
        $scope.selectedMenuItem = "";
        $scope.selectedUserItem = "";
    }
    $scope.Cancel = function () {
        ClearFields();
        $scope.divMenuPermission = false;
    };
});

app.controller("treeViewCtrl", function ($scope, crudAJService) {
    var datatree;
    GetMenuForTree();
    function GetMenuForTree() {
        var dataTree = crudAJService.GetPreMenu();
        datatree = dataTree.data;
    }

    $scope.treeData = new kendo.data.HierarchicalDataSource({
        data: [
            { text: "Item 1" },
            {
                text: "Item 2", items: [
                    { text: "SubItem 2.1" },
                    { text: "SubItem 2.2" }
                ]
            },
            { text: "Item 3" }
        ]
    });

    $scope.addAfter = function (item) {
        var array = item.parent();
        var index = array.indexOf(item);
        var newItem = makeItem();
        array.splice(index + 1, 0, newItem);
    };

    $scope.addBelow = function () {
        // can't get this to work by just modifying the data source
        // therefore we're using tree.append instead.
        var newItem = makeItem();
        $scope.tree.append(newItem, $scope.tree.select());
    };

    $scope.remove = function (item) {
        var array = item.parent();
        var index = array.indexOf(item);
        array.splice(index, 1);

        $scope.selectedItem = undefined;
    };
});


// Controller For Managing Print Template For application - aaku
app.controller("printTemplateController", function ($scope, printTemplateService, $timeout) {
    GetUser();
    GetAllForm();
    GetAllTemplate();

    $scope.GetAllMappedFormTemplate = function () {

        params = ReportFilter.filterAdditionalData();

        console.log("Params===========>>" + JSON.stringify(params));
        var getMappedFormTemplate = printTemplateService.GetAllMappedFormTemplate(params);

        getMappedFormTemplate.then(function (formTemplateControl) {
            RemoveDataTableGrid();
            $scope.FormTemplateControl = formTemplateControl.data;
            $scope.Bind();
            $timeout(function () {
                DataTableGrid();
            }, 100)
        }, function () { $scope.FormTemplateGetError = "No form has been mapped to template" });
    };

    $scope.Bind = function () {
        $('.k-list').slimScroll({
            height: '250px'
        });
    };

    $scope.GetAllMappedFormTemplate();

    $scope.ReloadGrid = function () {
        GetAllMappedFormTemplate();
        GetAllForm();
        GetAllTemplate();
    }

    $scope.selectedFormItem;
    $scope.selectedTemplateItem;
    $scope.selectedUserItem;

    $scope.GROUP_SKU_FLAG = 'G';
    $scope.ACCESS_FLAG = 'Y';


    $scope.formModel = [];
    $scope.templateModel = [];
    $scope.userDetails = {};


    $scope.select2Options = {
        allowClear: true
    };


    $scope.formControl = {
        smartButtonMaxItems: 3,
        enableSearch: true,
        smartButtonTextConverter: function (itemText, originalItem) {
            return itemText;
        }
    };

    $scope.userControl = {
        smartButtonMaxItems: 3,
        enableSearch: true,
        smartButtonTextConverter: function (itemText, originalItem) {
            return itemText;
        }
    };

    $scope.templateControl = {
        smartButtonMaxItems: 1,
        enableSearch: true,
        smartButtonTextConverter: function (itemText, originalItem) {
            return itemText;
        }
    };

    $scope.bulkFormModel = [];
    $scope.bulkTemplateModel = [];
    $scope.bulkuserModel = [];

    $scope.editFormTemplateMap = function (formTemplateControl) {
      //  debugger;
        var getEditedMappedTemplate = printTemplateService.GetAllMappedFormTemplateWithParam(formTemplateControl.FORM_CODE, formTemplateControl.TEMPLATE_NAME);
        getEditedMappedTemplate.then(function (response) {
            // $scope.FormWithCode = formTemplateControl.data;
            //  $scope.MAPPEDTEMPLATE = response.data;
            $scope.selectedFormItem = formTemplateControl.FORM_CODE;
            $scope.selectedUserItem = formTemplateControl.USER_ID;
            $scope.selectedTemplateItem = formTemplateControl.TEMPLATE_NAME;
            $scope.Action = "Update";
            $scope.addFormToTemplate = true;
            $timeout(function () {
                testSelect2("Update");
            }, 100);
        }, function () { toastr.error("Error while editing mapped template") });
    };

    $scope.GetBulkFormTemplateMap = function () {
        $scope.addBulkFormTemplate = true;
    };

    $scope.USER_NO;

    $scope.AddBulkFormTemplateMap = function () {
        //debugger;

        var formList = _.pluck($scope.bulkFormModel, 'id').join(',');
        var templateList = _.pluck($scope.bulkTemplateModel, 'id').join(',');
        var userList = _.pluck($scope.bulkuserModel, 'id').join(',');

        var MAPPEDTEMPLATE = {
            FORM_LIST: formList,
            TEMPLATE_LIST: templateList,
            USER_LIST: userList,
            COMPANY_EDESC: "",
            TEMPLATE_PATH: "~/Views/Shared/PrintTemplate/"
        }

        console.log("MappedTEmplate====================" + JSON.stringify(MAPPEDTEMPLATE));

        var getBulkMapTemplate = printTemplateService.AddBulkFormToTemplate(MAPPEDTEMPLATE);
        getBulkMapTemplate.then(function (msg) {
            $scope.GetAllMappedFormTemplate();
            toastr.success(msg.data);
            $scope.addBulkFormTemplate = false;
        }, function () {
            toastr.error("Error while adding bulk mapping");
        });
    };

    $scope.AddUpdateFormTemplateMap = function () {

        var mappedTemplate = {
            FORM_NAME: $scope.selectedFormItem,
            TEMPLATE_NAME: $scope.selectedTemplateItem,
            USER_NO: $scope.selectedUserItem,
            COMPANY_EDESC: "04",
            TEMPLATE_PATH: "~/Views/Shared/PrintTemplate/"
        };
        var getAction = $scope.Action;

        if (getAction === "Update") {

            mappedTemplate.FORM_NAME = $scope.selectedFormItem;
            mappedTemplate.TEMPLATE_NAME = $scope.selectedTemplateItem;
            mappedTemplate.USER_NO = $scope.selectedUserItem;
            mappedTemplate.COMPANY_EDESC = "04";
            mappedTemplate.TEMPLATE_PATH = "~/Views/Shared/PrintTemplate/";

            var updatedData = printTemplateService.UpdateMappedTemplate(mappedTemplate);
            updatedData.then(function (msg) {
                if (msg.data === "Error") {
                    toastr.error(msg.data);
                } else {
                    toastr.success(msg.data);
                    $scope.addFormToTemplate = false;
                    $('.select2-container').select2('val', '');
                }
                $scope.GetAllMappedFormTemplate();
            }, function () { toastr.error("Error while updating mapped template") });
        } else {

            var addMappedTemplate = printTemplateService.AddMappedTemplate(mappedTemplate);
            addMappedTemplate.then(function (msg) {
                $scope.GetAllMappedFormTemplate();
                toastr.success(msg.data);
                $scope.addFormToTemplate = false;
                $('.select2-container').select2('val', '');
            }, function () {
                toastr.error("Error while adding mapped template");
            });
        }
    };

    $scope.GetFormTemplateMap = function () {

        ClearFields();
        $timeout(function () {
            testSelect2("Add");
        }, 10);
        $scope.Action = "Add";
        $scope.addFormToTemplate = true;
    };

    $scope.tempForm;
    $scope.tempTemplate;

    $scope.setTemp = function (form) {
        $scope.tempForm = form;
        $timeout(function () {
            $scope.divConfirmToDelete = true;
        }, 100);
    };


    $scope.deleteMapFormTemplate = function () {
        var deleteMapping = printTemplateService.DeleteTemplateMapping($scope.tempForm.FORM_CODE, $scope.tempForm.TEMPLATE_NAME);
        deleteMapping.then(function (msg) {
            $scope.divConfirmToDelete = false;
            toastr.success(msg.data);
            $scope.GetAllMappedFormTemplate();
        }, function () { 'Error while deleting mapping'; });
    };

    function GetAllForm() {

        var allFormWithCode = printTemplateService.GetAllFormWithCode();


        allFormWithCode.then(function (formWithCode) {
            $scope.FormWithCode = formWithCode.data;
            $scope.BulkFormWithCode = formWithCode.data;
        }, function () { $scope.formErrorMessage = "Error while getting form" });

    }

    function GetAllTemplate() {

        var allTemplateWithCode = printTemplateService.GetAllTemplateWithCode();

        allTemplateWithCode.then(function (availableTemplate) {
            $scope.AvailableTemplate = availableTemplate.data;
            $scope.BulkAvailableTemplate = availableTemplate.data;
        }, function () { $scope.templateErrorMessage = "Error while getting template" });

    }

    $scope.selecCompanyAsUser = function (ovalue) {
        // angular.forEach($scope.users, function (value, key) {
        //console.log(key);
        //console.log(value);
        //console.log("loop value==" + JSON.stringify(value));
        //console.log("value only" + ovalue);
        // if (value.id.toString() === ovalue.toString()) {
        //    console.log(value.COMPANY_EDESC);
        //  }

        //  });
    };

    function GetUser() {

        getModuleData = printTemplateService.GetUserWithCompanyCode();
        getModuleData.then(function (module) {
            $scope.users = module.data;
            $scope.bulkuserData = module.data;
            // SetDefaultValue(module.data);
        });
    }

    function SetDefaultValue(userData) {

        for (var user in userData) {
            $scope.userDetails.USERNO = user.id;
            $scope.userDetails.NAME = user.label;
            $scope.userDetails.COMPANY = user.company;
        }
    }

    function ClearFields() {

    }

    $scope.Cancel = function () {
        ClearFields();

        $scope.addFormToTemplate = false;
        $scope.addBulkFormTemplate = false;
        $scope.divConfirmToDelete = false;
    };

});


//Controller For Managing Role and Access
app.controller("accessManagerController", function ($scope, accessManagerService, $window) {

    $scope.modulename = "Access Manager";
    $scope.action = "Save";
    $scope.AccessAction = "Apply";
    $scope.selectedUser = [];
    $scope.selectedAppModule = null;
    $scope.selectedControl = null;



    $scope.checkedUser = [];
    $scope.checkedCompany = [];
    $scope.showModuleDropDown = true;

    $scope.SaveModal = {
        checkedUser: null,
        checkedControl:null
    };

    function getDropdownUser() {
        var ddlUser = accessManagerService.getDropdownUser();
        ddlUser.then(function (res) {
            $scope.dropDownUser = res.data;

        });
    }

    getDropdownUser();

    function getAppModule() {
        var appModule = accessManagerService.getAppModule();
        appModule.then(function (res) {
            $scope.AppModule = res.data;
        });
    }

    getAppModule();

    // USER TREE 

    var userTreeUrl = window.location.protocol + "//" + window.location.host + "/AccessManager/GetAllUserTree";
    var userTreeDataSource = new kendo.data.HierarchicalDataSource({
        transport: {
            read: {
                url: userTreeUrl,
                dataType: "json"
            }
        },
        schema: {
            model: {
                id: "LOGIN_CODE",
                hasChildren: "hasBranch"
            }
        }
    });

    $("#userTreeView").kendoTreeView({
        checkboxes: {
            checkChildren: true
        },
        autoScroll: true,
        autoBind: true,
        dataSource: userTreeDataSource,
        dataTextField: "LOGIN_EDESC",
        scrollable: {
            virtual: true
        },
        check: onUserCheck,
        dataBound: function () {

        }

    });

    function onUserCheck(e) {

        var userview = $("#userTreeView").data("kendoTreeView");
        var data = userview.dataItem(e.node);
        $scope.checkedUser.push(data);
        var isExpanded = $(e.node).attr("data-expanded");
        if (data.PRE_USER_NO === "00") {
            if (typeof isExpanded === 'undefined' || isExpanded === false) {
                userview.expand(".k-item");
                userview.collapse(".k-item");
                userview.expand(e.node);
            }
        }
    }

    $scope.showAll = true;
    var selectedControlSetting = [];
    $scope.selectedControlSetting = [];

    var dataSource = new kendo.data.DataSource({
            transport: {
                read: {
                    url: window.location.protocol + "//" + window.location.host + "/AccessManager/GetAllAvailableControl",
                    dataType: "json"
                },
                parameterMap: function (options, operation) {
                    if (operation !== "read" && options.models) {
                        return {
                            models: kendo.stringify(options.models)
                        };
                    }
                }
            },
            batch: true,
            pageSize: 300,
            group: {
                field: "CONTROL_HEADING"
            },
            schema: {
                model: {
                    id: "CONTROL_CODE",
                    fields: {
                        CONTROL_CODE: { editable: false, nullable: true },
                        CONTROL_NAME: {type:"string", validation: {  required: true } },
                        NEW: { type: "boolean" },
                        VIEW: { type: "boolean" },
                        EDIT: { type: "boolean" },
                        RECYCLE: { type: "boolean" },
                        POSTPRINT: { type: "boolean" },
                        UNPOST: { type: "boolean" },
                        CHECK: { type: "boolean" },
                        VERIFY: { type: "boolean" }
                       
                    }
                }
            }
        });

    var grid = $("#allControlGrid").kendoGrid({
        dataSource: dataSource,
        pageable: true,
        selectable: "cell",
        //change:selectSingleCell,
        toolbar: kendo.template($("#toolbar-template").html()),
        //define dataBound event handler
        dataBound: onDataBound,
       // toolbar: ["save"],
        columns: [
           
            {
                title: 'Select All',
                headerTemplate: "<input type='checkbox' id='header-chb' class='k-checkbox header-checkbox'>",
                template: function (dataItem) {
                    return "<input type='checkbox' id='" + dataItem.CONTROL_CODE + "' class='k-checkbox row-checkbox'><label class='k-checkbox-label' for='" + dataItem.CONTROL_CODE + "'></label>";
                },
                width: 80
            },
            {
                field: "CONTROL_NAME", title: "Element"
            },
            {
                field: "NEW", title: "NEW", 
                template: function (dataItem) {
                    if (dataItem.NEW) {
                        return "<input type='checkbox' id='" + dataItem.CONTROL_CODE + "' class='k-checkbox singleCell'  checked=true value='" + dataItem.NEW + "'>";
                    } else {
                        return "<input type='checkbox' id='" + dataItem.CONTROL_CODE + "' class='k-checkbox singleCell'  data-bind='checked: dataItem.NEW' value='" + dataItem.NEW + "'>";
                    }
                    
                }
               
            },
            {
                field: "VIEW", title: "VIEW", 
                template: function (dataItem) {
                    if (dataItem.VIEW) {
                        return "<input type='checkbox' id='" + dataItem.CONTROL_CODE + "'  class='k-checkbox singleCell' checked=true value='" + dataItem.VIEW + "'>";
                    } else {
                        return "<input type='checkbox' id='" + dataItem.CONTROL_CODE + "'  class='k-checkbox singleCell' data-bind='checked: dataItem.VIEW' value='" + dataItem.VIEW + "'>";
                    }
                   
                }
            },
            {
                field: "EDIT", title: "EDIT",
                template: function (dataItem) {
                    if (dataItem.EDIT) {
                        return "<input type='checkbox' id='" + dataItem.CONTROL_CODE + "'  class='k-checkbox' checked=true value='" + dataItem.EDIT + "'>";
                    } else {
                        return "<input type='checkbox' id='" + dataItem.CONTROL_CODE + "'  class='k-checkbox' data-bind='checked: dataItem.EDIT' value='" + dataItem.EDIT + "'>";
                    }
                    
                }
              
            },
            {
                field: "RECYCLE", title: "Recycle",
                template: function (dataItem) {
                    if (dataItem.RECYCLE) {
                        return "<input type='checkbox' id='" + dataItem.CONTROL_CODE + "'  class='k-checkbox' checked=true value='" + dataItem.RECYCLE + "'>";
                    } else {
                        return "<input type='checkbox' id='" + dataItem.CONTROL_CODE + "'  class='k-checkbox' data-bind='checked: dataItem.RECYCLE' value='" + dataItem.RECYCLE + "'>";
                    }
                    
                }
            },
            {
                field: "POSTPRINT", title: "Post/Print",
                template: function (dataItem) {
                    if (dataItem.POSTPRINT) {
                        return "<input type='checkbox' id='" + dataItem.CONTROL_CODE + "'  class='k-checkbox' checked=true value='" + dataItem.POSTPRINT + "'>";
                    } else {
                        return "<input type='checkbox' id='" + dataItem.CONTROL_CODE + "'  class='k-checkbox' data-bind='checked: dataItem.POSTPRINT' value='" + dataItem.POSTPRINT + "'>";
                    }
                   
                }
            },
            {
                field: "UNPOST", title: "Unpost",
                template: function (dataItem) {
                    if (dataItem.UNPOST) {
                        return "<input type='checkbox' id='" + dataItem.CONTROL_CODE + "'  class='k-checkbox' checked=true value='" + dataItem.UNPOST + "'>";
                    } else {
                        return "<input type='checkbox' id='" + dataItem.CONTROL_CODE + "'  class='k-checkbox' data-bind='checked: dataItem.UNPOST' value='" + dataItem.UNPOST + "'>";
                    }
                    
                }
            },
            {
                field: "CHECK", title: "Check",
                template: function (dataItem) {
                    if (dataItem.CHECK) {
                        return "<input type='checkbox' id='" + dataItem.CONTROL_CODE + "'  class='k-checkbox' checked=true value='" + dataItem.CHECK + "'>";
                    } else {
                        return "<input type='checkbox' id='" + dataItem.CONTROL_CODE + "'  class='k-checkbox' data-bind='checked: dataItem.CHECK' value='" + dataItem.CHECK + "'>";
                    }
                     
                }
            },
            {
                field: "VERIFY", title: "Verify",
                template: function (dataItem) {
                    if (dataItem.VERIFY) {
                        return "<input type='checkbox' id='" + dataItem.CONTROL_CODE + "'  class='k-checkbox singleCell' checked=true value='" + dataItem.VERIFY + "'>";
                    } else {
                        return "<input type='checkbox' id='" + dataItem.CONTROL_CODE + "'  class='k-checkbox singleCell' data-bind='checked: dataItem.VERIFY' value='" + dataItem.VERIFY + "'>";
                    }
                     
                }
            },
            { field: "MORE", title: "More"}
        ],
        editable: "inline"
    }).data("kendoGrid");

    //bind click event to the checkbox
    grid.table.on("click", ".row-checkbox", selectRow);
    grid.table.on("click", ".singleCell", selectSingleCell);

    $('#header-chb').change(function (ev) {
        var checked = ev.target.checked;
        $('.row-checkbox').each(function (idx, item) {
            if (checked) {
                if (!($(item).closest('tr').is('.k-state-selected'))) {
                    $(item).click();
                   
                   
                }
            } else {
                if ($(item).closest('tr').is('.k-state-selected')) {
                    $(item).click();
                }
            }
        });
    });

    var checkedIds = {};
     selectedControlSetting = [];

    //on click of the checkbox:
     function selectRow() {
    var checked = this.checked,
        row = $(this).closest("tr"),
        grid = $("#allControlGrid").data("kendoGrid"),
             dataItem = grid.dataItem(row);

         console.log("dataItem:" + JSON.stringify(dataItem));

         checkedIds[dataItem.id] = checked;
        

    if (checked) {
        row.addClass("k-state-selected");
        row.find("input:checkbox").attr("checked", checked);
        row.find("input:checkbox").val(checked);
        dataItem.NEW = true;
        dataItem.VIEW = true;
        dataItem.EDIT = true;
        dataItem.RECYCLE = true;
        dataItem.POSTPRINT = true;
        dataItem.UNPOST = true;
        dataItem.CHECK = true;
        dataItem.VERIFY = true;
        selectedControlSetting.push(dataItem);
        $("#clearChecked").attr("disabled", false);
        var checkHeader = true;

        $.each(grid.items(), function (index, item) {
            if (!($(item).hasClass("k-state-selected"))) {
                checkHeader = false;
            }
        });

        $("#header-chb")[0].checked = checkHeader;

    } else {
        //-remove selection
        selectedControlSetting.length = 0;
        row.removeClass("k-state-selected");
        $("#header-chb")[0].checked = false;
        row.find("input:checkbox").attr("checked", false);
        $("#clearChecked").attr("disabled", true);
        dataItem.NEW = false;
        dataItem.VIEW = false;
        dataItem.EDIT = false;
        dataItem.RECYCLE = false;
        dataItem.POSTPRINT = false;
        dataItem.UNPOST = false;
        dataItem.CHECK = false;
        dataItem.VERIFY = false;
        selectedControlSetting.push(dataItem);
    }
    }

    function selectSingleCell(e) {

        var td = $(this).closest('td');
        var grid = $("#allControlGrid").data("kendoGrid"),
            row = $(this).closest('td').parent();
        dataItem = grid.dataItem($(td).parent());


        var checked = this.checked;

       

        if (checked) {
           // td.addClass("k-state-selected");
            //var selected = $(this).closest('td').select();
           // dataItem.set(grid.options.columns[selected.index() - 1].field, true);
            td.select();
            dataItem.set(grid.options.columns[td.select().index() - 1].field, true);
            selectedControlSetting.push(dataItem);

        } else {
           // td.removeClass("k-state-selected");
            //var selected = $(this).closest('td').select();
            //dataItem.set(grid.options.columns[selected.index() - 1].field, false);
            dataItem.set(grid.options.columns[td.select().index() - 1].field, false);
            selectedControlSetting.push(dataItem);
        }  

        //var checked = this.checked;
        //var td = $(this).closest('td');

        //if (checked) {
        //    td.addClass("k-state-selected");
        //} else {
        //    td.removeClass("k-state-selected");
        //}
        

       // console.log("check box event" )

       
        
       // var idx = selected.index();
        

        //var grid = $("#allControlGrid").data("kendoGrid"),
        //    dataItem = grid.dataItem($(e.target).closest("tr"));
        //    dataRows = grid.items();
        //var rowIndex = dataRows.index(grid.select());
        //console.log('idx=================>>>' + rowIndex);    
        //console.log('cellIndex=================>>>' + grid.cellIndex(rowIndex));    

        //dataItem.set("Discontinued", this.checked);

        //var checked = this.checked,
        //    row = $(this).closest("tr"),
        //    grid = $("#allControlGrid").data("kendoGrid"),
        //    dataItem = grid.dataItem(row);

        //console.log("dataItem:" + JSON.stringify(dataItem));

        //checkedIds[dataItem.id] = checked;


        //if (checked) {
        //    dataItem.NEW = true;
        //    dataItem.VIEW = true;
        //    dataItem.EDIT = true;
        //    dataItem.RECYCLE = true;
        //    dataItem.POSTPRINT = true;
        //    dataItem.UNPOST = true;
        //    dataItem.CHECK = true;
        //    dataItem.VERIFY = true;
        //    selectedControlSetting.push(dataItem);
        //    $("#clearChecked").attr("disabled", false);
        //    var checkHeader = true;

        //    $.each(grid.items(), function (index, item) {
        //        if (!($(item).hasClass("k-state-selected"))) {
        //            checkHeader = false;
        //        }
        //    });

        //    $("#header-chb")[0].checked = checkHeader;

        //} else {
        //    $("#header-chb")[0].checked = false;
        //   // row.find("input:checkbox").attr("checked", false);
        //    $("#clearChecked").attr("disabled", true);
        //    selectedControlSetting.push(dataItem);
        //}


       

       
        //var selected = this.select();
        //var row1 = this.select().closest("tr");
        //var item = grid.dataItem(row1);
        //var idx = selected.index();
        //console.log("Row1 Item====================>>>" + JSON.stringify(row1));
        //console.log("cell Item====================>>>" + JSON.stringify(item));
        //console.log("cell Item=======idx=============>>>" + JSON.stringify(idx));
        //console.log("actual field====================>>>" + JSON.stringify(row1.find("td:eq(" + idx + ")")));
        //console.log("actual title====================>>>" +  this.options.columns[idx-1].title );
        //console.log("actual field====================>>>" + this.options.columns[idx - 1].field);
        //var field = this.options.columns[idx - 1].field;
        //item[field] = true;
    }

    //on dataBound event restore previous selected rows:
     function onDataBound(e) {
         var view = this.dataSource.view();
      for (var i = 0; i < view.length; i++) {
            if (checkedIds[view[i].id]) {
                this.tbody.find("tr[data-uid='" + view[i].uid + "']")
                    .addClass("k-state-selected")
                    .find(".k-checkbox")
                    .attr("checked", "checked");
            }
      }
    }

    $scope.ClearCheck = function () {
        $("#allControlGrid  tbody").find('tr').each(
            function () {
                var IsAdd = $(this).hasClass('k-state-selected');
                if (IsAdd === true) {
                    $(this).removeClass('k-state-selected');
                    $(this).find("input:checkbox").attr("checked", false);
                }
            });
       
    };

    $scope.printGrid = function () {
        var gridElement = $('#allControlGrid'),
            printableContent = '',
            win = window.open('', '', 'width=800, height=500, resizable=1, scrollbars=1'),
            doc = win.document.open();

        var htmlStart =
            '<!DOCTYPE html>' +
            '<html>' +
            '<head>' +
            '<meta charset="utf-8" />' +
            '<title>Kendo UI Grid</title>' +
            '<link href="http://kendo.cdn.telerik.com/' + kendo.version + '/styles/kendo.common.min.css" rel="stylesheet" /> ' +
            '<style>' +
            'html { font: 11pt sans-serif; }' +
            '.k-grid { border-top-width: 0; }' +
            '.k-grid, .k-grid-content { height: auto !important; }' +
            '.k-grid-content { overflow: visible !important; }' +
            'div.k-grid table { table-layout: auto; width: 100% !important; }' +
            '.k-grid .k-grid-header th { border-top: 1px solid; }' +
            '.k-grouping-header, .k-grid-toolbar, .k-grid-pager > .k-link { display: none; }' +
            // '.k-grid-pager { display: none; }' + // optional: hide the whole pager
            '</style>' +
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
    };

    //-------------------------------Demo end---------------------------------


    function customBoolEditor(container, options) {
        var guid = kendo.guid();
        if (options.field === "NEW") {
            $('<input class="k-checkbox" id="' + guid + '" type="checkbox" name="NEW" data-type="boolean" data-bind="checked:NEW">').appendTo(container);
            $('<label class="k-checkbox-label" for="' + guid + '">&#8203;</label>').appendTo(container);
        } else if (options.field === "VIEW") {
            $('<input class="k-checkbox" id="' + guid + '" type="checkbox" name="VIEW" data-type="boolean" data-bind="checked:VIEW">').appendTo(container);
            $('<label class="k-checkbox-label" for="' + guid + '">&#8203;</label>').appendTo(container);
        } else if (options.field === "EDIT") {
            $('<input class="k-checkbox" id="' + guid + '" type="checkbox" name="EDIT" data-type="boolean" data-bind="checked:EDIT">').appendTo(container);
            $('<label class="k-checkbox-label" for="' + guid + '">&#8203;</label>').appendTo(container);
        } else if (options.field === "RECYCLE") {
            $('<input class="k-checkbox" id="' + guid + '" type="checkbox" name="RECYCLE" data-type="boolean" data-bind="checked:RECYCLE">').appendTo(container);
            $('<label class="k-checkbox-label" for="' + guid + '">&#8203;</label>').appendTo(container);
        } else if (options.field === "POSTPRINT") {
            $('<input class="k-checkbox" id="' + guid + '" type="checkbox" name="POSTPRINT" data-type="boolean" data-bind="checked:POSTPRINT">').appendTo(container);
            $('<label class="k-checkbox-label" for="' + guid + '">&#8203;</label>').appendTo(container);
        } else if (options.field === "UNPOST") {
            $('<input class="k-checkbox" id="' + guid + '" type="checkbox" name="UNPOST" data-type="boolean" data-bind="checked:UNPOST">').appendTo(container);
            $('<label class="k-checkbox-label" for="' + guid + '">&#8203;</label>').appendTo(container);
        } else if (options.field === "CHECK") {
            $('<input class="k-checkbox" id="' + guid + '" type="checkbox" name="CHECK" data-type="boolean" data-bind="checked:CHECK">').appendTo(container);
            $('<label class="k-checkbox-label" for="' + guid + '">&#8203;</label>').appendTo(container);
        } else if (options.field === "VERIFY") {
            $('<input class="k-checkbox" id="' + guid + '" type="checkbox" name="VERIFY" data-type="boolean" data-bind="checked:VERIFY">').appendTo(container);
            $('<label class="k-checkbox-label" for="' + guid + '">&#8203;</label>').appendTo(container);
        } else {
            console.log("lllll");
        }
    }

    function checkRow() {
        var dataItem = "";
        var checked = this.checked;
       
        if (checked) {
            var row = $(this).closest("tr"),
                grid = $("#allControlGrid").data("kendoGrid");  
            row.addClass("k-state-selected");
            dataItem = grid.dataItem(row);
          
            console.log("dataITem=-====================>>>" + JSON.stringify(dataItem));
            selectedControlSetting.push(dataItem);
            $scope.selectedControlSetting.push(dataItem);

        } else {
            row = $(this).closest("tr");
            row.find("input:checkbox").attr("checked", false);
            row.removeClass("k-state-selected");
            selectedControlSetting.pop();
            $scope.selectedControlSetting.pop();
        }

    }

    $scope.controlChangedOld = function () {
        
        $scope.showAll = false;
        $scope.showCompany = false;
        $scope.showDManagerTransaction = false;
        $scope.masterDefinitionControl = false;
        if ($scope.selectedControl === null || $scope.selectedControl === "") {
            selectedControlSetting = [];
            $scope.selectedControlSetting = [];
            $scope.showAll = true;
        } else if ($scope.selectedControl === "CNB") {
            selectedControlSetting = [];
            $scope.selectedControlSetting = [];
            $scope.showCompany = true;
        } else if ($scope.selectedControl === "DMT") {
            selectedControlSetting = [];
            $scope.selectedControlSetting = [];
            $scope.showDManagerTransaction = true;
        } else if ($scope.selectedControl === "MDC") {
            selectedControlSetting = [];
            $scope.selectedControlSetting = [];
            $scope.masterDefinitionControl = true;
        } else if ($scope.selectedControl === "MDL") {
            selectedControlSetting = [];
            $scope.selectedControlSetting = [];
            $scope.masterDefinitionControl = true;
        }
    };

    $scope.controlChanged = function () {
        var allControlGrid = $("#allControlGrid").data("kendoGrid");
        var headerCheck = $("#header-chb").is(":checked");
        if (headerCheck) {
            $("#header-chb").parent('span').removeClass('checked');
        }

        if ($scope.selectedUser.length !== 0) {

            if ($scope.selectedControl === null || $scope.selectedControl === "") {
                $scope.showModuleDropDown = true;
                $scope.selectedControlSetting.length = 0;
                selectedControlSetting.length = 0;
                allControlGrid.dataSource.transport.options.read.url = window.location.protocol + "//" + window.location.host + "/AccessManager/GetAllAvailableControl?userNo=" + $scope.selectedUser.USER_NO + "&selectedControl=" + $scope.selectedControl;
                allControlGrid.dataSource.read();
            } else if ($scope.selectedControl === "CNB") {
                $scope.showModuleDropDown = false;
                $scope.selectedControlSetting.length = 0;
                selectedControlSetting.length = 0;
                allControlGrid.dataSource.transport.options.read.url = window.location.protocol + "//" + window.location.host + "/AccessManager/GetAvailableCompanyCntrl?userNo=" + $scope.selectedUser.USER_NO + "&selectedControl=" + $scope.selectedControl;
                allControlGrid.dataSource.read();
            } else if ($scope.selectedControl === "DMT") {
                $scope.showModuleDropDown = true;
                $scope.selectedControlSetting.length = 0;
                selectedControlSetting.length = 0;
                allControlGrid.dataSource.transport.options.read.url = window.location.protocol + "//" + window.location.host + "/AccessManager/GetAvailableDocManagerCntrl?userNo=" + $scope.selectedUser.USER_NO + "&selectedControl=" + $scope.selectedAppModule;
                allControlGrid.dataSource.read();
                $scope.showDManagerTransaction = true;
            } else if ($scope.selectedControl === "MDC") {
                $scope.showModuleDropDown = false;
                $scope.selectedControlSetting.length = 0;
                selectedControlSetting.length = 0;
                allControlGrid.dataSource.transport.options.read.url = window.location.protocol + "//" + window.location.host + "/AccessManager/GetAvailableMasterDefinitionCntrl?userNo=" + $scope.selectedUser.USER_NO + "&selectedControl=" + $scope.selectedControl;
                allControlGrid.dataSource.read();
                $scope.masterDefinitionControl = true;
            } else if ($scope.selectedControl === "MDL") {
                $scope.showModuleDropDown = false;
                $scope.selectedControlSetting.length = 0;
                selectedControlSetting.length = 0;
                allControlGrid.dataSource.transport.options.read.url = window.location.protocol + "//" + window.location.host + "/AccessManager/GetAvailableModuleControl?userNo=" + $scope.selectedUser.USER_NO + "&selectedControl=" + $scope.selectedControl;
                allControlGrid.dataSource.read();
            } else if ($scope.selectedControl === "MSLV") {
                $scope.showModuleDropDown = true;
                $scope.selectedControlSetting.length = 0;
                selectedControlSetting.length = 0;
                allControlGrid.dataSource.transport.options.read.url = window.location.protocol + "//" + window.location.host + "/AccessManager/GetMasterSetupListViewControl?userNo=" + $scope.selectedUser.USER_NO + "&selectedControl=" + $scope.selectedControl;
                allControlGrid.dataSource.read();
            }

        } else {

            if ($scope.selectedControl === null || $scope.selectedControl === "") {
                $scope.showModuleDropDown = true;
                $scope.selectedControlSetting.length = 0;
                selectedControlSetting.length = 0;
                allControlGrid.dataSource.transport.options.read.url = window.location.protocol + "//" + window.location.host + "/AccessManager/GetAllAvailableControl";
                allControlGrid.dataSource.read();
            } else if ($scope.selectedControl === "CNB") {
                $scope.showModuleDropDown = false;
                $scope.selectedControlSetting.length = 0;
                selectedControlSetting.length = 0;
                allControlGrid.dataSource.transport.options.read.url = window.location.protocol + "//" + window.location.host + "/AccessManager/GetAvailableCompanyCntrl";
                allControlGrid.dataSource.read();
            } else if ($scope.selectedControl === "DMT") {
                $scope.showModuleDropDown = true;
                $scope.selectedControlSetting.length = 0;
                selectedControlSetting.length = 0;
                allControlGrid.dataSource.transport.options.read.url = window.location.protocol + "//" + window.location.host + "/AccessManager/GetAvailableDocManagerCntrl";
                allControlGrid.dataSource.read();
                $scope.showDManagerTransaction = true;
            } else if ($scope.selectedControl === "MDC") {
                $scope.showModuleDropDown = false;
                $scope.selectedControlSetting.length = 0;
                selectedControlSetting.length = 0;
                allControlGrid.dataSource.transport.options.read.url = window.location.protocol + "//" + window.location.host + "/AccessManager/GetAvailableMasterDefinitionCntrl";
                allControlGrid.dataSource.read();
                $scope.masterDefinitionControl = true;
            } else if ($scope.selectedControl === "MDL") {
                $scope.showModuleDropDown = false;
                $scope.selectedControlSetting.length = 0;
                selectedControlSetting.length = 0;
                allControlGrid.dataSource.transport.options.read.url = window.location.protocol + "//" + window.location.host + "/AccessManager/GetAvailableModuleControl";
                allControlGrid.dataSource.read();
            } else if ($scope.selectedControl === "MSLV") {
                $scope.showModuleDropDown = true;
                $scope.selectedControlSetting.length = 0;
                selectedControlSetting.length = 0;
                allControlGrid.dataSource.transport.options.read.url = window.location.protocol + "//" + window.location.host + "/AccessManager/GetMasterSetupListViewControl";
                allControlGrid.dataSource.read();
            }

        }
        $scope.AccessAction = "Update Apply";
    };

    $scope.dropDownModuleChanged = function () {
        var allControlGrid = $("#allControlGrid").data("kendoGrid");
        console.log("SelectedAppModeul==========>>>" + JSON.stringify($scope.selectedAppModule));
        if ($scope.selectedControl === "DMT") {
            $scope.selectedControlSetting.length = 0;
            selectedControlSetting.length = 0;
            allControlGrid.dataSource.transport.options.read.url = window.location.protocol + "//" + window.location.host + "/AccessManager/GetAvailableDocManagerCntrl?userNo=0&selectedControl=" + $scope.selectedAppModule.MODULE_CODE;
            allControlGrid.dataSource.read();
        } else if ($scope.selectedControl === null || $scope.selectedControl === "") {
            $scope.selectedAppModule.MODULE_CODE = "0";
            allControlGrid.dataSource.transport.options.read.url = window.location.protocol + "//" + window.location.host + "/AccessManager/GetAllAvailableControl";
            allControlGrid.dataSource.read();
        } else if ($scope.selectedControl === "MSLV") {
            allControlGrid.dataSource.transport.options.read.url = window.location.protocol + "//" + window.location.host + "/AccessManager/GetMasterSetupListViewControl?userNo=0&selectedControl=" + $scope.selectedAppModule.MODULE_CODE;
            allControlGrid.dataSource.read();
        }
    };

    function getCheckedUserItems(treeview) {
        var nodes = treeview.dataSource.view();
        return getCheckedUserNodes(nodes);
    }

    function getCheckedUserNodes(nodes) {
        var node, childCheckedNodes;
        var checkedNodes = [];

        for (var i = 0; i < nodes.length; i++) {
            node = nodes[i];
            if (node.checked) {
                checkedNodes.push(node);
            }

            if (node.hasChildren) {
                childCheckedNodes = getCheckedUserNodes(node.children.view());
                if (childCheckedNodes.length > 0) {
                    checkedNodes = checkedNodes.concat(childCheckedNodes);
                }
            }

        }

        return checkedNodes;
    }

    $scope.SearchAccess = function () {
        var allControlGrid = $("#allControlGrid").data("kendoGrid");
        var userTree = $("#userTreeView").data("kendoTreeView");
        if ($scope.selectedUser===null || $scope.selectedUser==="null") {
            allControlGrid.dataSource.transport.options.read.url = window.location.protocol + "//" + window.location.host + "/AccessManager/GetAllAvailableControl?userNo=" + 0 + "&selectedControl=" + $scope.selectedControl;
            allControlGrid.dataSource.read();
            $("#userTreeView .k-checkbox-wrapper input").prop("checked", false).trigger("change");
            $scope.selectedText = $("#ddlUser option:selected").html();
            updateCheck = userTree.findByText($scope.selectedText);
            $scope.AccessAction = "Apply";
            return;
        }
        $("#userTreeView .k-checkbox-wrapper input").prop("checked", false).trigger("change");
        $scope.selectedText = $("#ddlUser option:selected").html();
        updateCheck = userTree.findByText($scope.selectedText);
        userTree.dataItem(updateCheck).set("checked", true);
        $scope.AccessAction = "Update";
        var selectedControl = $scope.selectedControl;
       
        if (!selectedControl) {
            allControlGrid.dataSource.transport.options.read.url = window.location.protocol + "//" + window.location.host + "/AccessManager/GetAllAvailableControl?userNo=" + $scope.selectedUser.USER_NO + "&selectedControl=" + $scope.selectedControl;
            allControlGrid.dataSource.read();
        } else if (selectedControl === "CNB") {
            allControlGrid.dataSource.transport.options.read.url = window.location.protocol + "//" + window.location.host + "/AccessManager/GetAvailableCompanyCntrl?userNo=" + $scope.selectedUser.USER_NO + "&selectedControl=" + $scope.selectedControl;
            allControlGrid.dataSource.read();
        } else if (selectedControl === "DMT") {
            allControlGrid.dataSource.transport.options.read.url = window.location.protocol + "//" + window.location.host + "/AccessManager/GetAvailableDocManagerCntrl?userNo=" + $scope.selectedUser.USER_NO + "&selectedControl=" + $scope.selectedAppModule;
            allControlGrid.dataSource.read();
        } else if (selectedControl === "MDC") {
            allControlGrid.dataSource.transport.options.read.url = window.location.protocol + "//" + window.location.host + "/AccessManager/GetAvailableMasterDefinitionCntrl?userNo=" + $scope.selectedUser.USER_NO + "&selectedControl=" + $scope.selectedControl;
            allControlGrid.dataSource.read();
        } else if (selectedControl === "MSLV") {
            allControlGrid.dataSource.transport.options.read.url = window.location.protocol + "//" + window.location.host + "/AccessManager/GetMasterSetupListViewControl?userNo=" + $scope.selectedUser.USER_NO + "&selectedControl=" + $scope.selectedControl;
            allControlGrid.dataSource.read();
        } else if (selectedControl === "MDL") {
            allControlGrid.dataSource.transport.options.read.url = window.location.protocol + "//" + window.location.host + "/AccessManager/GetAvailableModuleControl?userNo=" + $scope.selectedUser.USER_NO + "&selectedControl=" + $scope.selectedControl;
            allControlGrid.dataSource.read();
        }

    };

    $scope.SaveAccess = function () {
            
        var userTree = $("#userTreeView").data("kendoTreeView");
        var checkedUser = getCheckedUserItems(userTree);

        if ($scope.AccessAction === "Update") {
            var controlGrid = $("#allControlGrid").data().kendoGrid.dataSource.data();
           
            var selectedControlSetting1111 = controlGrid;
            console.log("selectedControlSettings=============update=============>>>" + JSON.stringify(selectedControlSetting));
        } 

       

        if (checkedUser.length === 0 || selectedControlSetting.length === 0 || $window.selectedControlSetting) {

            displayPopupNotification("Please selected user and control", "warning");

        } else {

            $scope.SaveModal = {
                "isUpdate": false,
                "checkedUser": checkedUser,
                "checkedControl": selectedControlSetting
            };


            console.log("Scope.SaveModal=======================>>>" + JSON.stringify($scope.SaveModal));
            if ($scope.AccessAction === "Apply") {

                var savedResponse = accessManagerService.SaveUserAccessControl($scope.SaveModal);
                savedResponse.then(function (res) {
                    if (res.data === "Successfull") {
                        displayPopupNotification("Accessed Saved Successfully","success");
                        //DisplayBarNotificationMessage(res.data);
                        setTimeout(function () {
                            location.reload(true);
                        }, 3000);
                    } else {
                        displayPopupNotification("Error while saving access control","error");
                    }
                    
                });
            } else {
                $scope.SaveModal.isUpdate = true;
                var updateResponse = accessManagerService.SaveUserAccessControl($scope.SaveModal);
                updateResponse.then(function (res) {
                    if (res.data === "Successfull") {
                        displayPopupNotification("Accessed Updated Successfully","success");
                        //DisplayBarNotificationMessage(res.data);
                        setTimeout(function () {
                            location.reload(true);
                        }, 3000);

                    } else {
                        displayPopupNotification("Error while updating access control","error");
                    }
                   
                });
            }  
        }
    };

    $scope.saveObjectCompany;
    $scope.saveObjectDMT;
    $scope.saveObjectMDC;

  //--------------------------------------------------------------------------------------//
});

app.controller("changePasswordCtrl", function ($scope, $window, $http) {
    console.log("i am hrer");
    $scope.UserViewModel = {
        username_disabled: true,
        password_disabled: true,
        fullname_disabled: true,
        usertype_disabled: true,
        userno_disabled: true
        //usertypelist

    };
    $scope.UserViewModel = {
        username: '',
        password: '',
        fullname: '',
        usertype: '',
        userno: ''
        //usertypelist

    };
    // $scope.save = "Save";
    $scope.save = "Update";
    $scope.SaveChangeUserPassword = function (form, isValid) {
        //debugger;
        if (validateForm(isValid)) {
            var loadhs = window.location.protocol + "//" + window.location.host + "/api/Main/InsertChangeUserPassword";

            var clt = {
                USERNAME: form.username,
                PASSWORD: form.password,
                FULLNAME: form.fullname,
                USER_TYPE: form.usertype,
                SAVE_FLAG: $scope.save,
                USER_NO: form.userno
                //USER_NO: form.usertypelist
            };
            var response = $http({
                method: "post",
                url: loadhs,
                data: JSON.stringify(clt),
                dataType: "json",
                async: false
            });
            return response.then(function (result) {
                console.log(result);
               // debugger;
                if (result.data.STATUS_CODE === 500) { // internal error

                }
                if (result.data === "Success") {// OK
                    //debugger;

                    displayPopupNotification("Succesfully Saved");
                    Refreshdata();
                    $("#kGrid").data("kendoGrid").dataSource.read();
                }
                else if (result.data === "Updated") {
                   // debugger;
                    displayPopupNotification("Updated Succesfully");
                    //Refreshdata();
                    $scope.save = "Update";
                    $("#kGrid").data("kendoGrid").dataSource.read();
                }
                else if (result.data === "fail") {
                    //debugger;
                    displayPopupNotification("fail to save data");
                }
                else {
                   // debugger;
                }

            });
        }
        else {
            displayPopupNotification("Fields cannot be empty.", "error");
        }

    };
    validateForm = function (IsValid) {
        if (!IsValid) {
            displayPopupNotification("Input fileds are not valid, Please review form and Retry.", "warning");
            return false;
        }
        return true;
    };

    function Refreshdata() {
        $scope.UserViewModel.username = '';
        $scope.UserViewModel.password = '';
        $scope.UserViewModel.fullname = '';
        $scope.UserViewModel.usertype = '';
    }

    $scope.UserDetailsGridOptions = {

        dataSource: {
            type: "json",
            transport: {
                read: {
                    url: "/api/Main/GetAllUserDetails",
                    dataType: "json"
                },
                //update: {
                //    url: window.location.protocol + "//" + window.location.host + "/api/Main/UpdateUserPassword",
                //    type: "POST",
                //    contentType: "application/json; charset=utf-8",
                //    dataType: "json",
                //    complete: function (e) {
                //        if (e.responseJSON.STATUS_CODE === 200) {
                //            $.each($("#kGrid"), function (i, value) {

                //                $(value).data("kendoGrid").dataSource.read();
                //            });
                //            displayPopupNotification("Successfully Updated Item.", "success");
                //        }
                //    }

                //},
                parameterMap: function (options, operation) {

                  //  debugger;
                    if (operation !== "read" && options.models) {
                      //  debugger;
                        var data = options.models;
                        return JSON.stringify(data);
                    }
                }
            },
            batch: true,
            serverPaging: false,
            pageSize: 10,
            schema: {
                model: {
                    id: "LOGIN_CODE",
                    fields: {
                        USERNAME: { validation: { required: true } },
                        PASSWORD: { validation: { required: true } },
                        FULLNAME: { validation: { required: true } },
                        USER_TYPE: { validation: { required: true } }
                    }
                }
            }

        },
        scrollable: false,
        sortable: true,
        pageable: true,
        columns: [
            //{
            //    field: "CUSTOMER_SERIAL_NO",
            //    title: "S.N",
            //    width: "30px"
            //},
            {
                field: "USERNAME",
                title: "USER NAME",
                width: "80px"
            },
            {
                field: "USER_NO",
                title: "USER_NO",
                hidden: true,
                width: "80px"
            },
            {
                field: "FULLNAME",
                title: "FULL NAME",
                width: "80px"
            },
            {
                field: "USER_TYPE",
                title: "USER TYPE",
                width: "80px"
            },
            {
                field: "PASSWORD",
                title: "PASSWORD",
                width: "80px"
            },

            {
                template: '<a class="edit glyphicon glyphicon-edit" title="Edit" ng-click="edituserinfo(dataItem.USER_NO)" style="color:grey;"><span class="sr-only"></span> </a>',
                title: " ",
                width: "30px"
            }

        ],
        editable: "inline",
    };
    $scope.edituserinfo = function (userno) {
        var userinfourl = "/api/Main/GetUserDetailsByUserNo?userno=" + userno;
       // debugger;
        $http.get(userinfourl).then(function (response) {
          //  debugger;
            $scope.UserViewModel.username = response.data.USERNAME;
            $scope.UserViewModel.password = response.data.PASSWORD;
            $scope.UserViewModel.fullname = response.data.FULLNAME;
            $scope.UserViewModel.usertype = response.data.USER_TYPE;
            $scope.save = "Update";
            $scope.UserViewModel.userno = response.data.USER_NO;

            $scope.UserViewModel.username_disabled = true;
            $scope.UserViewModel.password_disabled = true;
            $scope.UserViewModel.fullname_disabled = true;
            $scope.UserViewModel.usertype_disabled = true;
            $scope.UserViewModel.userno_disabled = true;
            //$scope.UserViewModel.usertypelist = response.data.USER_NO;
        });

    };
    $scope.onSearch = function ($this) {
      //  debugger;
        var q = $("#txtSearchString").val();
        var grid = $("#kGrid").data("kendoGrid");
        grid.dataSource.query({
            page: 1,
            pageSize: 50,
            filter: {
                logic: "or",
                filters: [
                    { field: "USERNAME", operator: "contains", value: q },
                    { field: "PASSWORD", operator: "contains", value: q },
                    { field: "FULLNAME", operator: "contains", value: q },
                    { field: "USER_TYPE", operator: "contains", value: q }
                ]
            }
        });
    };

    //$scope.UserTypeDataSource = {
    //    type: "json",
    //    serverFiltering: true,

    //    transport: {
    //        read: {
    //            url: "/api/Main/GetAllUserType",

    //        },
    //        parameterMap: function (data, action) {
    //            debugger;
    //            var newParams;
    //            debugger;
    //            if (data.filter !== undefined) {

    //                if (data.filter.filters[0] !== undefined) {
    //                    newParams = {
    //                        filter: data.filter.filters[0].value
    //                    };
    //                    return newParams;
    //                }
    //                else {
    //                    newParams = {
    //                        filter: ""
    //                    };
    //                    return newParams;
    //                }
    //            }
    //            else {
    //                newParams = {
    //                    filter: ""
    //                };
    //                return newParams;
    //            }
    //        }
    //    }
    //};
});


app.controller('AddUserSetupCtrl', function ($scope, menuControlService, $http, $window, $filter) {
    $scope.saveupdatebtn = "Save";
    $scope.AddButtonClickEvent = function () {
        debugger;
        $scope.createPanel = true;
    }
    $scope.Cancel = function () {
        debugger;
        $scope.createPanel = false;
        $scope.ClearFields();
        $scope.pageName = "Add User";
        $scope.saveupdatebtn = "Save";
    }
    $scope.ClearFields = function () {
        debugger;
        $scope.UserSetup.LOGIN_CODE = "";
        $scope.UserSetup.LOGIN_EDESC = "";
        $scope.UserSetup.PASSWORD = "";
        $scope.UserSetup.EMPLOYEE_CODE = "";
        $scope.UserSetup.COMPANY_CODE = "";
        $scope.UserSetup.USER_LOCK_FLAG = "";
        $scope.UserSetup.SUPER_USER_FLAG = "";
    }
    $scope.UserSetup = {
        USER_NO: "",
        PRE_USER_NO: "",
        GROUP_SKU_FLAG: "",
        LOGIN_CODE: "",
        LOGIN_EDESC: "",
        PASSWORD: "",
        EMPLOYEE_CODE: "",
        USER_LOCK_FLAG: "",
        COMPANY_CODE: "",
        SUPER_USER_FLAG: "",
    }
    //Company
    var companyurl = window.location.protocol + "//" + window.location.host + "/api/Main/getAllCompany";
    $scope.accountsDataSource = new kendo.data.DataSource({
        transport: {
            read: {
                url: companyurl,
            }
        }
    });
    $scope.distCompanySelectOptions = {
        dataSource: $scope.accountsDataSource,
        optionLabel: "--Select Company--",
        dataTextField: "COMPANY_EDESC",
        dataValueField: "COMPANY_CODE",
        filter: "contains",
        change: function (e) {

        },
        dataBound: function () {
         
        }
    };

    //Employee
    var employeeurl = window.location.protocol + "//" + window.location.host + "/api/Main/getAllEmployeeList";
    $scope.EmployeeDataSource = new kendo.data.DataSource({
        transport: {
            read: {
                url: employeeurl,
            }
        }
    });
    $scope.distEmployeeSelectOptions = {
        dataSource: $scope.EmployeeDataSource,
        optionLabel: "--Select Employee--",
        dataTextField: "EMPLOYEE_EDESC",
        dataValueField: "EMPLOYEE_CODE",
        filter: "contains",
        change: function (e) {

        },
        dataBound: function () {
           
        }
    };

    //Save and Update Function
    $scope.SaveUser = function (isValid) {
        debugger;
        if (!isValid) {
            displayPopupNotification("Input fields are not valid. Please review and try again", "warning");
            return;
        }
        if ($scope.saveupdatebtn == "Save")
            {
        
             var createurl = window.location.protocol + "//" + window.location.host + "/api/Main/createUser";
              var model = {
            USER_NO: $scope.UserSetup.USER_NO,
            PRE_USER_NO: $scope.UserSetup.PRE_USER_NO,
            GROUP_SKU_FLAG: $scope.UserSetup.GROUP_SKU_FLAG,
            LOGIN_CODE: $scope.UserSetup.LOGIN_CODE,
            LOGIN_EDESC: $scope.UserSetup.LOGIN_EDESC,
            PASSWORD: $scope.UserSetup.PASSWORD,
            EMPLOYEE_CODE: $scope.UserSetup.EMPLOYEE_CODE,
            COMPANY_CODE: $scope.UserSetup.COMPANY_CODE,
            USER_LOCK_FLAG: $scope.UserSetup.USER_LOCK_FLAG,
            SUPER_USER_FLAG: $scope.UserSetup.SUPER_USER_FLAG,
        }
              $http({
              method: 'post',
              url: createurl,
              data: model
             }).then(function successcallback(response) {
            debugger;
            var grid = $('#grid').data("kendoGrid");
            grid.dataSource.read();
            displayPopupNotification(response.data.MESSAGE, response.data.TYPE);
            $scope.Cancel();
        }, function errorcallback(response) {
              displayPopupNotification("Something went wrong.Please try again later.", "error");
             });

        }
        else {
            if ($scope.saveupdatebtn == "Update") {
                var createurl = window.location.protocol + "//" + window.location.host + "/api/Main/updateUser";
                var model = {
                    USER_NO: $scope.UserSetup.USER_NO,
                    LOGIN_CODE: $scope.UserSetup.LOGIN_CODE,
                    LOGIN_EDESC: $scope.UserSetup.LOGIN_EDESC,
                    PASSWORD: $scope.UserSetup.PASSWORD,
                    EMPLOYEE_CODE: $scope.UserSetup.EMPLOYEE_CODE,
                    COMPANY_CODE: $scope.UserSetup.COMPANY_CODE,
                    USER_LOCK_FLAG: $scope.UserSetup.USER_LOCK_FLAG,
                    SUPER_USER_FLAG: $scope.UserSetup.SUPER_USER_FLAG,
                }
                $http({
                    method: 'post',
                    url: createurl,
                    data: model
                }).then(function successcallback(response) {
                    debugger;
                    var grid = $('#grid').data("kendoGrid");
                    grid.dataSource.read();
                    displayPopupNotification(response.data.MESSAGE, response.data.TYPE);
                    $scope.Cancel();
                }, function errorcallback(response) {
                    displayPopupNotification("Something went wrong.Please try again later.", "error");
                });

            }

        }

 }

    //grid
    $scope.grid = {
        dataSource: {
            type: "json",
            transport: {
                read: {
                    url: window.location.protocol + "//" + window.location.host + "/api/Main/GetUserList",
                    dataType: "json",
                    contentType: "application/json; charset=utf-8"
                },
            },
            error: function (e) {
                displayPopupNotification("Sorry error occured while processing data", "error");
            },
            pageSize: 500,
        },
        //toolbar: kendo.template($("#toolbar-template").html()),
        //excel: {
        //    fileName: "user",
        //    allPages: true,
        //},
       // height: window.innerHeight - 50,
        sortable: true,
        reorderable: true,
        //groupable: true,
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
       
        pageable: {
            refresh: true,
            buttonCount: 5
        },
        model: {
            fields: {
            }
        },
        columnMenuInit: function (e) {
            wordwrapmenu(e);
            checkboxItem = $(e.container).find('input[type="checkbox"]');
        },
        dataBound: function (o) {
            var grid = o.sender;
            if (grid.dataSource.total() == 0) {
                var colCount = grid.columns.length;
                $(o.sender.wrapper)
                    .find('tbody')
                    .append('<tr class="kendo-data-row" style="font-size:12px;"><td colspan="' + colCount + '" class="alert alert-danger">Sorry, no data :(</td></tr>');
                displayPopupNotification("No Data Found.", "info");
            }
            $('.k-grid td').css("white-space", "normal"); 
            $('div').removeClass('.k-header k-grid-toolbar');
        },
        columns: [
            {
                field: "LOGIN_CODE",
                title: "User name",
                width: "10%",
            },
             {
                 field: "PASSWORD",
                 title: "PASSWORD",
                 width: "20%"
             },

            {
                field: "CREATED_BY",
                title: "Created By",
                width: "20%",

            },
             {
                 field: "CREATED_DATE",
                 title: "Created Date",
                 template: "#= kendo.toString(kendo.parseDate(CREATED_DATE, 'yyyy-MM-dd'), 'MM/dd/yyyy') #",
                 width: "20%",

             },
            {
                title: "Actions",
                width: "10%",
                template: " <a class='fa fa-edit editAction' ng-click='UpdateUser(#:USER_NO#)' title='Edit'></a>&nbsp &nbsp<a class='fa fa-trash-o deleteAction' ng-click='delete(#:USER_NO#)' title='Delete'></a>  ",

            }
        ]
    };

    $scope.delete = function (usercode) {
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
                    var delUrl = window.location.protocol + "//" + window.location.host + "/api/Main/DeleteUserFromDb?usercode=" + usercode;
                    $http({
                        method: 'POST',
                        url: delUrl
                    }).then(function successCallback(response) {
                        if (response.data.MESSAGE == "DELETED") {
                            debugger;
                            var grid = $('#grid').data("kendoGrid");
                            grid.dataSource.read();
                            bootbox.hideAll();
                            displayPopupNotification("Data succesfully deleted ", "success");
                        }
                    }, function errorCallback(response) {
                        displayPopupNotification(response.data.STATUS_CODE, "error");
                    });

                }
                else if (result == false) {
                    bootbox.hideAll();
                }

            }
        });
    }



    $scope.UpdateUser = function (id) {
        debugger;
        var gridDs = $("#grid").data("kendoGrid").dataSource.data();
        var items = _.filter(gridDs, function (x) { return x.USER_NO == id });
        debugger;
        var emp = angular.copy(items[0]);
        $scope.UserSetup = angular.copy(emp);
        $scope.UserSetup.SUPER_USER_FLAG = emp.SUPER_USER_FLAG;
        $scope.UserSetup.USER_LOCK_FLAG = emp.USER_LOCK_FLAG;
        $scope.saveupdatebtn = "Update";
        $scope.createPanel = true;
    }
});


