
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
    $routeProvider.when('/QuotationDetails', {
        templateUrl: '/QuotationManagement/Home/QuotationDetails',
    });
    $routeProvider.when('/SummaryReport', {
        templateUrl: '/QuotationManagement/Home/SummaryReport',
    });
    $routeProvider.otherwise({
        redirectTo: function () {
            return '/QuotationManagement/Home/Dashboard';
        }
    })
    $locationProvider.html5Mode({ enable: true }).hashPrefix('!QM');
});

QMModule.config.$inject = ['$routeProvider', '$locationProvider'];
