(function (window, angular, undefined) {
    'use strict';

    angular.module('fms').factory('ReportsService', ['$http', 'config', function($http, config) {
        var ReportsService = {};

        ReportsService.query = function(name) {
            return $http.get(config.API_ROOT + 'reports/' + name).then(function(res) {
                return res.data;
            });
        }     

        return ReportsService;
    }]);

})(window, window.angular);