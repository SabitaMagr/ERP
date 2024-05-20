
var QMModule = angular.module('QMModule', ['ngRoute', 'ngMessages', 'datatables', 'kendo.directives', 'cfp.hotkeys', 'ngHandsontable', 'angularjs-dropdown-multiselect']).run(['$rootScope', function ($rootScope) {

}]);
QMModule.config(function ($routeProvider, $locationProvider) {
    $routeProvider.when('/',
        {
            templateUrl: function () {
                return '/QuotationManagement/Home/Dashboard';
            },
        });

    $routeProvider.when('/Dashboard',
        {
            templateUrl: function () {
                return '/QuotationManagement/Home/Dashboard';
            },
        });
    $routeProvider.when('/QuotationSetup', {
        templateUrl: '/QuotationManagement/Home/QuotationSetup',
    });
    $routeProvider.when('/QuotationDetail', {
        templateUrl: '/QuotationManagement/Home/QuotationDetail',
    });
    $routeProvider.when('/QuotationDetail/:id',
        {
            templateUrl: function (stateParams) {

                return '/QuotationManagement/Home/QuotationDetail?id=' + stateParams.id;
            },
            controller: 'quotationDetail',
            resolve: {
                module: function ($route) { return $route.current.params.id; }
            }

        });
    $routeProvider.when('/SummaryReport', {
        templateUrl: '/QuotationManagement/Home/SummaryReport',
    });
    $routeProvider.when('/TenderNoSetup', {
        templateUrl: '/QuotationManagement/Home/TenderSetup',
    });
    $routeProvider.otherwise({
        redirectTo: function () {
            return '/QuotationManagement/Home/Dashboard';
        }
    })
    $locationProvider.html5Mode({ enable: true }).hashPrefix('!QM');
});

QMModule.config.$inject = ['$routeProvider', '$locationProvider'];
