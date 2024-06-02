
var PMModule = angular.module('PMModule', ['ngRoute', 'ngMessages', 'datatables', 'kendo.directives', 'cfp.hotkeys', 'ngHandsontable', 'angularjs-dropdown-multiselect']).run(['$rootScope', function ($rootScope) {

}]);
PMModule.config(function ($routeProvider, $locationProvider) {
    $routeProvider.when('/',
        {
            templateUrl: function () {
                return '/ProjectManagement/Home/Dashboard';
            },
        });

    $routeProvider.when('/Dashboard',
        {
            templateUrl: function () {
                return '/ProjectManagement/Home/Dashboard';
            },
        });
    $routeProvider.when('/ProjectSetup', {
        templateUrl: '/ProjectManagement/Home/ProjectSetup',
    });
    $routeProvider.when('/Project', {
        templateUrl: '/ProjectManagement/Home/Project',
    });
    $routeProvider.when('/Dashboard', {
        templateUrl: '/ProjectManagement/Home/Dashboard',
    });
    $routeProvider.when('/RequisitionEntry', {
        templateUrl: '/ProjectManagement/Home/RequisitionEntry',
        controller: 'Requisition',

    });
    $routeProvider.when('/ConsumptionEntry', {
        templateUrl: '/ProjectManagement/Home/ConsumptionEntry',
        controller: 'consumption',

    });
    $routeProvider.when('/RequisitionEntry/:formcode/:orderno', {

        templateUrl: function (stateParams) {

            return '/ProjectManagement/Home/RequisitionEntry?formCode=' + stateParams.formcode + '&voucherNo=' + stateParams.orderno;
        },
        controller: 'Requisition',
        resolve:
        {
            formcode: function ($route) { return $route.current.params.formcode; },
            OrderNo: function ($route) { return $route.current.params.orderno; }
        }

    });
    $routeProvider.when('/AuxiliaryEntry', {
        templateUrl: '/ProjectManagement/Home/AuxiliaryEntry',
        controller: 'consumption',

    });
    $routeProvider.when('/PurchaseEntry', {
        templateUrl: '/ProjectManagement/Home/PurchaseEntry',
        controller: 'Purchase',

    });
    $routeProvider.when('/RequisitionReport', {
        templateUrl: '/ProjectManagement/Home/RequisitionReport',
    });
    $routeProvider.when('/RequisitionPendingReport', {
        templateUrl: '/ProjectManagement/Home/RequisitionPendingReport',
    });
    $routeProvider.when('/ConsumptionReport', {
        templateUrl: '/ProjectManagement/Home/ConsumptionReport',
    });
    $routeProvider.when('/PurchaseReport', {
        templateUrl: '/ProjectManagement/Home/PurchaseReport',
    });
    //$routeProvider.when('/ProjectDashboard', {
    //    templateUrl: '/ProjectManagement/Home/ProjectDashboard',
    //});
    $routeProvider.otherwise({
        redirectTo: function () {
            return '/ProjectManagement/Home/Dashboard';
        }
    })
    $locationProvider.html5Mode({ enable: true }).hashPrefix('!PM');
});

PMModule.config.$inject = ['$routeProvider', '$locationProvider'];
