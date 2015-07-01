(function (window, angular, undefined) {
    'use strict';

    angular.module('fms').factory('PersonService', ['$http', 'config', function ($http, config) {
        var PersonService = {};

        PersonService.getOne = function (id) {
            return $http.get(config.API_ROOT + 'person/' + id).then(function (res) {
                return res.data;
            });
        };

        PersonService.getDocuments = function (id, type) {
            return $http.get(config.API_ROOT + 'person/' + id + '/document/' + type).then(function (res) {
                return res.data;
            });
        };

        PersonService.getFacts = function (id, factId) {
            return $http.get(config.API_ROOT + 'person/' + id + '/fact/' + factId).then(function (res) {
                return res.data;
            });
        };

        PersonService.save = function (person) {
            return $http.put(config.API_ROOT + 'person/' + person.id, person).then(function (res) {
                return res.data;
            });
        };

        return PersonService;
    }]);

})(window, window.angular);