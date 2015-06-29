(function (window, angular, undefined) {
    'use strict';

    angular.module('fms').factory('SearchService', ['$http', 'config', function ($http, config) {
        var SearchService = {};

        SearchService.query = function (model) {
            return $http.post(config.API_ROOT + 'search', model).then(function (res) {
                return res.data;
            });
        }

        SearchService.search = function (id, page, limit) {
            return $http.get(config.API_ROOT + 'search/' + id, { params: { page: page, limit: limit } }).then(function (res) {
                return res.data;
            });
        }

        return SearchService;
    }]);

})(window, window.angular);