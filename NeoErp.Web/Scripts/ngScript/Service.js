//app.service('menuService', ['$http', '$q', '$timeout', function ($http, $q, $timeout) {
//    return {
//        getMenu: function () {
//            // This could be $http or any other promise returning service.
//            // Use a deferred and $timeout to simulate a network request.
//            var deferred = $q.defer()

//            $timeout(function () {
//                var results = $http({
//                    method: "post",
//                    url: "/MenuSetting/GetMenuByMenuNo",
//                    params: {
//                        menuNo: JSON.stringify(menuNo)
//                    }
//                })
//                .success(function (data) {
//                    return data.data;
//                });
//                deferred.resolve(results);
//            }, 2000);

//            return deferred.promise;
//        },
//    };
//}]);

app.service("crudAJService", ['$http', '$q', '$timeout', function ($http, $q, $timeout) {

    //get All Menus
    this.getMenus = function () {
        return $http.get("/MenuSetting/MenuSettings");
    };

    // get Menu by MenuNo
    this.getMenu = function (menuNo) {
        var deferred = $q.defer()

        $timeout(function () {
            var results = $http({
                method: "post",
                url: "/MenuSetting/GetMenuByMenuNo",
                params: {
                    menuNo: JSON.stringify(menuNo)
                }
            })
            .success(function (data) {
                return data.data;
            });
            deferred.resolve(results);
        }, 100);

        return deferred.promise;
    }

    //this.getMenu = function (menuNo) {
    //    var response = $http({
    //        method: "post",
    //        url: "/MenuSetting/GetMenuByMenuNo",
    //        params: {
    //            menuNo: JSON.stringify(menuNo)
    //        }
    //    });
    //    console.log(response);
    //    return response;
    //}

    // Update Menu 
    this.updateMenu = function (menu) {
        var response = $http({
            method: "post",
            url: "/MenuSetting/UpdateMenu",
            data: JSON.stringify(menu),
            dataType: "json"
        });
        return response;
    }

    // Add Menu
    this.AddMenu = function (menu) {
        var response = $http({
            method: "post",
            url: "/MenuSetting/AddMenu",
            data: JSON.stringify(menu),
            dataType: "json"
        });
        return response;
    }

    //Delete Menu
    this.DeleteMenu = function (menuNo) {
        var response = $http({
            method: "post",
            url: "/MenuSetting/DeleteMenu",
            params: {
                menuNo: JSON.stringify(menuNo)
            }
        });
        return response;
    }
    // Get Module in dropdown
    this.GetModule = function () {
        var response = $http({
            method: "post",
            url: "/MenuSetting/GetModule"
        });
        return response;
    }
    this.GetPreMenu = function () {
        var response = $http({
            method: "post",
            url: "/MenuSetting/GetPreMenu"
        });
        return response;
    }
}]);

app.service("menuControlService", function ($http) {
    //get All Menus
    this.GetMenuControl = function () {
        return $http.get("/MenuControl/GetMenuControl");
    };
    //this.GetMenuControl = function () {
    //    var response = $http({
    //        method: 'GET',
    //        url: window.location.protocol + "//" + window.location.host + "/MenuControl/GetMenuControl",
    //    });
    //    
    //    return response;
    //}


    this.GetAllMenuControl = function (parms) {
        
        var response = $http({
            
            method: "post",
            url: window.location.protocol + "//" + window.location.host + "/MenuControl/GetAllMenuControl",
            data: JSON.stringify(parms),
            dataType: "json"
            
        });
        
        return response;
    }
    // get Menu by MenuNo
    this.getMenuControl = function (menuNo, UserNo) {
        var response = $http({
            method: "post",
            url: "/MenuControl/getMenuControlByID",
            params: {
                menuNo: JSON.stringify(menuNo),
                userNo: JSON.stringify(UserNo)
            }
        });
        return response;
    }

    // Update Menu 
    this.updateMenuControl = function (menu) {
        
        var response = $http({
            method: "post",
            url: "/MenuControl/UpdateMenuControl",
            data: JSON.stringify(menu),
            dataType: "json"
        });
        return response;
    }

    // Add Menu
    this.AddMenuControl = function (menu) {
        var response = $http({
            method: "post",
            url: "/MenuControl/AddMenuControl",
            data: JSON.stringify(menu),
            dataType: "json"
        });
        return response;
    }
    // bulk user menu add
    this.AddBulkUserAndMenu = function (menu) {
        var response = $http({
            method: "post",
            url: "/MenuControl/AddBulkMenuUser",
            data: JSON.stringify(menu),
            dataType: "json"
        });
        return response;
    }
    //Delete Menu
    this.DeleteMenuControl = function (menuNo, userNo) {
        
        var response = $http({
            method: "post",
            url: "/MenuControl/DeleteMenuControl",
            params: {
                menuNo: JSON.stringify(menuNo),
                userNo: JSON.stringify(userNo)
            }
        });
        return response;
    }

    this.GetMenu = function () {
        var response = $http({
            method: "post",
            url: "/MenuControl/GetMenu"
        });
        return response;
    }
    this.GetUser = function () {
        var response = $http({
            method: "post",
            url: "/MenuControl/GetUser"
        });
        return response;
    }

});


//Service to fetch print template and operation to print template - aaku

app.service("printTemplateService", function ($http) {

    this.GetAllMappedFormTemplate = function (params) {
        var response = $http({
            method: "post",
            url: window.location.protocol + "//" + window.location.host + "/PrintTemplate/GetAllMappedFormTemplate",
            data: JSON.stringify(params),
            dataType: "json"
        });

        return response;
    };

    this.GetAllFormWithCode = function () {
        var formResponse = $http({
            method: "GET",
            url: window.location.protocol + "//" + window.location.host + "/PrintTemplate/GetAllFormWithCode",
            dataType: "json"
        });

        return formResponse;
    };

    this.GetAllTemplateWithCode = function () {
        var templateResponse = $http({
            method: "GET",
            url: window.location.protocol + "//" + window.location.host + "/PrintTemplate/GetAllTemplateWithCode",
            dataType:"json"
        });

        return templateResponse;
    }

    this.GetUserWithCompanyCode = function () {
        var userResponse = $http({
            method: "GET",
            url: window.location.protocol + "//" + window.location.host + "/PrintTemplate/GetAllUserWithCompanyCode",
            dataType:"json"
        });

        return userResponse;
    }

    this.UpdateMappedTemplate = function (mappedTemplate) {

        var updatedResponse = $http({
            method: "post",
            url: "/PrintTemplate/UpdateMappedTemplate",
            data: JSON.stringify(mappedTemplate),
            dataType: "json"
        });
        return updatedResponse;
    }

    this.AddBulkFormToTemplate = function (MAPPEDTEMPLATE) {

        var bulkResponse = $http({
            method: "post",
            url: "/PrintTemplate/AddBulkMappedTemplate",
            data: JSON.stringify(MAPPEDTEMPLATE),
            dataType: "json"
        });
        return bulkResponse;
    }

    this.AddMappedTemplate = function (mappedTemplate) {

        var addedResponse = $http({
            method: "post",
            url: "/PrintTemplate/AddMappedTemplate",
            data: JSON.stringify(mappedTemplate),
            dataType: "json"
        });
        return addedResponse;
    }

    this.DeleteTemplateMapping = function (formCode,templateName) {

        var deleteResponse = $http({
            method: "post",
            url: "/PrintTemplate/DeleteTemplateMapping",
            params: {
                formCode: JSON.stringify(formCode),
                templateName: JSON.stringify(templateName)
            }
        });
        return deleteResponse;
    }

    this.GetAllMappedFormTemplateWithParam = function (formCode, templateName) {
        debugger;
        var editResponse = $http({
            method: "GET",
            url: window.location.protocol + "//" + window.location.host + "/PrintTemplate/GetMappedTemplateForEdit",
            params: { formCode: JSON.stringify(formCode), templateName: JSON.stringify(templateName) },
            dataType: "json"
        });

        return editResponse;
    }
});


app.service("accessManagerService", function ($http) {


    this.getDropdownUser = function () {
        var userResponse = $http({
            method: "GET",
            url: window.location.protocol + "//" + window.location.host + "/AccessManager/GetDropdownUser",
            dataType: "json"
        });

        return userResponse;
    };

    this.getAppModule = function () {
        var moduleRes = $http({
            method: "GET",
            url: window.location.protocol + "//" + window.location.host + "/AccessManager/GetAppModuleDDL",
            dataType: "json"
        });
        return moduleRes;
    }

    this.getAllUserTree = function () {
        var formResponse = $http({
            method: "GET",
            url: window.location.protocol + "//" + window.location.host + "/AccessManager/GetAllUserTree",
            dataType: "json"
        });

        return formResponse;
    };


    this.SaveUserAccessControl = function (userCompanyObj) {
       

        var savedResponse = $http({
            method: "post",
            url: "/AccessManager/SaveUserAccessControl",
            data: userCompanyObj,
            dataType: "json"
        })/*.then(function (sucRes) { return sucRes; }, function (errRes) { return errRes; })*/;
        return savedResponse;
    };

     //this.UpdateUserAndCompany = function (userCompanyObj) {
    //    var updateResponse = $http({
    //        method: "post",
    //        url: "/AccessManager/UpdateUserAndCompany",
    //        data: JSON.stringify(userCompanyObj),
    //        dataType: "json"
    //    });

    //    return updateResponse;
    //};

     //this.GetAccessedCompanyNBranch = function (selectedUser) {

    //    var response = $http({
    //        method: "post",
    //        url: window.location.protocol + "//" + window.location.host + "/AccessManager/GetAccessedControl",
    //        params: {
    //            selectedUser: JSON.stringify(selectedUser)
    //        },
    //        dataType: "json"
    //    });
    //    return response;
    //}


});


//app.service("crudAJService", function ($http) {

//    //get All Menus
//    this.getMenus = function () {
//        return $http.get("/MenuSetting/MenuSettings");
//    };

//    // get Menu by MenuNo
//    this.getMenu = function (menuNo) {
//        var response = $http({
//            method: "post",
//            url: "/MenuSetting/GetMenuByMenuNo",
//            params: {
//                menuNo: JSON.stringify(menuNo)
//            }
//        });
//        console.log(response);
//        return response;
//    }

//    // Update Menu 
//    this.updateMenu = function (menu) {
//        var response = $http({
//            method: "post",
//            url: "/MenuSetting/UpdateMenu",
//            data: JSON.stringify(menu),
//            dataType: "json"
//        });
//        return response;
//    }

//    // Add Menu
//    this.AddMenu = function (menu) {
//        var response = $http({
//            method: "post",
//            url: "/MenuSetting/AddMenu",
//            data: JSON.stringify(menu),
//            dataType: "json"
//        });
//        return response;
//    }

//    //Delete Menu
//    this.DeleteMenu = function (menuNo) {
//        var response = $http({
//            method: "post",
//            url: "/MenuSetting/DeleteMenu",
//            params: {
//                menuNo: JSON.stringify(menuNo)
//            }
//        });
//        return response;
//    }
//    // Get Module in dropdown
//    this.GetModule = function () {
//        var response = $http({
//            method: "post",
//            url: "/MenuSetting/GetModule"
//        });
//        return response;
//    }
//    this.GetPreMenu = function () {
//        var response = $http({
//            method: "post",
//            url: "/MenuSetting/GetPreMenu"
//        });
//        return response;
//    }
//});
//app.service("menuControlService", function ($http) {
//    //get All Menus
//    this.GetMenuControl = function () {
//        return $http.get("/MenuControl/GetMenuControl");
//    };
//    // get Menu by MenuNo
//    this.getMenuControl = function (menuNo, UserNo) {
//        var response = $http({
//            method: "post",
//            url: "/MenuControl/getMenuControlByID",
//            params: {
//                menuNo: JSON.stringify(menuNo),
//                userNo:JSON.stringify(UserNo)
//            }
//        });
//        return response;
//    }

//    // Update Menu 
//    this.updateMenuControl = function (menu) {
//        var response = $http({
//            method: "post",
//            url: "/MenuControl/UpdateMenuControl",
//            data: JSON.stringify(menu),
//            dataType: "json"
//        });
//        return response;
//    }

//    // Add Menu
//    this.AddMenuControl = function (menu) {
//        var response = $http({
//            method: "post",
//            url: "/MenuControl/AddMenuControl",
//            data: JSON.stringify(menu),
//            dataType: "json"
//        });
//        return response;
//    }

//    //Delete Menu
//    this.DeleteMenuControl = function (menuNo, userNo) {
//        
//        var response = $http({
//            method: "post",
//            url: "/MenuControl/DeleteMenuControl",
//            params: {
//                menuNo: JSON.stringify(menuNo),
//                userNo: JSON.stringify(userNo)
//            }
//        });
//        return response;
//    }
//    this.GetMenu = function () {
//        var response = $http({
//            method: "post",
//            url: "/MenuControl/GetMenu"
//        });
//        return response;
//    }
//    this.GetUser = function () {
//        var response = $http({
//            method: "post",
//            url: "/MenuControl/GetUser"
//        });
//        return response;
//    }
    
//});