(function (window, angular, undefined) {
    'use strict';

    angular.module('fms').factory('ParameterService', ['$http', 'config', function ($http, config) {
        var ParameterService = {};

        ParameterService.list = function () {
            return $http.get(config.API_ROOT + 'parameters').then(function (res) {
                return res.data;
            });
        };

        ParameterService.create = function (param) {
            return $http.post(config.API_ROOT + 'parameters', param).then(function (res) {
                return res.data;
            });
        };

        ParameterService.remove = function (param) {
            return $http.delete(config.API_ROOT + 'parameters/' + param.id).then(function (res) {
                return res.data;
            });
        };

        return ParameterService;
    }]);

})(window, window.angular);