/// <reference path="../app.js" />
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

        DictionaryService.miscList = function () {
            return $http.get(config.API_ROOT + 'misc').then(function (res) {
                return res.data;
            });
        }      

        DictionaryService.createMisc = function (misc) {
            return $http.post(config.API_ROOT + 'misc', misc).then(function (res) {
                return res.data;
            });
        }

        DictionaryService.updateMisc = function (misc) {
            return $http.put(config.API_ROOT + 'misc', misc).then(function (res) {
                return res.data;
            });
        }

        DictionaryService.removeMisc = function (misc) {
            return $http.delete(config.API_ROOT + 'misc/' + misc.id).then(function (res) {
                return res.data;
            });
        }

        DictionaryService.miscValues = function (id) {
            return $http.get(config.API_ROOT + 'misc/' + id + '/values').then(function (res) {
                return res.data;
            });
        }

        DictionaryService.createMiscValue = function (miscValue) {
            return $http.post(config.API_ROOT + 'misc/' + miscValue.miscId + '/values', miscValue).then(function (res) {
                return res.data;
            });
        }

        DictionaryService.updateMiscValue = function (miscValue) {
            return $http.put(config.API_ROOT + 'misc/' + miscValue.miscId + '/values', miscValue).then(function (res) {
                return res.data;
            });
        }

        DictionaryService.removeMiscValue = function (miscValue) {
            return $http.delete(config.API_ROOT + 'misc/' + miscValue.miscId + '/values/' + miscValue.id).then(function (res) {
                return res.data;
            });
        }

        return DictionaryService;
    }]);

})(window, window.angular);