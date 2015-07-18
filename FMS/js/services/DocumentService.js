(function (window, angular, undefined) {
    'use strict';

    angular.module('fms').factory('DocumentService', ['$http', 'config', function ($http, config) {
        var DocumentService = {};

        DocumentService.save = function (doc) {
            return $http.put(config.API_ROOT + 'document/' + doc.id, doc).then(function (res) {
                return res.data;
            });
        };

        return DocumentService;
    }]);

})(window, window.angular);