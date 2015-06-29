(function (window, angular, undefined) {
    'use strict';

    angular.module('fms').factory('ImportService', ['$http', 'config', function ($http, config) {
        var ImportService = {};

        ImportService.import = function (docType, id) {
            return $http.post(config.API_ROOT + 'import/' + docType + '/' + id).then(function (res) {
                return res.data;
            });
        }

        ImportService.listenProgress = function (id) {
            return $http.get(config.API_ROOT + 'import/progress/' + id).then(function (res) {
                return res.data;
            });
        }

        ImportService.currentProgress = function () {
            return $http.get(config.API_ROOT + 'import/progress').then(function (res) {
                return res.data;
            });
        }

        return ImportService;
    }]);

})(window, window.angular);