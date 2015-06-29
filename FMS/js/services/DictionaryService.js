(function (window, angular, undefined) {
    'use strict';

    angular.module('fms').factory('DictionaryService', ['$http', 'config', function ($http, config) {
        var DictionaryService = {};

        DictionaryService.loadDict = function (name) {
            return $http.get(config.API_ROOT + 'dictionary/' + name).then(function (res) {
                return res.data.dictionary;
            });
        };

        DictionaryService.get = function (name, vm, ruName) {
            if (!angular.isObject(vm.loader)) vm.loader = {};
            name = ruName || name;
            vm.loader[name] = true;
            return DictionaryService.loadDict(name).then(function (arr) {
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