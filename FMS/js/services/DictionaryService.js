(function (window, angular, undefined) {
    'use strict';

    angular.module('fms').factory('DictionaryService', ['$http', 'config', function ($http, config) {
        var DictionaryService = {};

        DictionaryService.loadDict = function (name, docType, category) {
            return $http.post(config.API_ROOT + 'dictionary', {
                name: name,
                docType: docType,
                category: category
            }).then(function (res) {
                return res.data.dictionary;
            });
        };

        DictionaryService.get = function (name, docType, category, vm) {
            if (!angular.isObject(vm.loader)) vm.loader = {};
            vm.loader[name] = true;
            return DictionaryService.loadDict(name, docType, category).then(function (arr) {
                if (!angular.isObject(vm.dicts)) vm.dicts = {};
                vm.dicts[name] = arr;
                return arr;
            }).finally(function () {
                vm.loader[name] = false;
            });
        }

        return DictionaryService;
    }]);

})(window, window.angular);