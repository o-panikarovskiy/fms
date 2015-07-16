(function (window, angular, undefined) {
    'use strict';

    angular.module('fms').factory('DictionaryService', ['$http', 'config', function ($http, config) {
        var DictionaryService = {};

        DictionaryService.loadDict = function (name, type) {
            return $http.get(config.API_ROOT + 'dictionary/' + name + '/' + (type || -1)).then(function (res) {
                return res.data.dictionary;
            });
        };

        DictionaryService.get = function (name, type, vm) {
            if (!angular.isObject(vm.loader)) vm.loader = {};
            vm.loader[name] = true;
            return DictionaryService.loadDict(name, type).then(function (arr) {
                if (!angular.isObject(vm.dicts)) vm.dicts = {};
                if (!angular.isObject(vm.dicts[name])) vm.dicts[name] = {};
                vm.dicts[name][type] = arr;
                return arr;
            }).finally(function () {
                vm.loader[name] = false;
            });
        }

        return DictionaryService;
    }]);

})(window, window.angular);