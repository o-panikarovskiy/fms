(function (window, angular, undefined) {
    'use strict';

    angular.module('fms').factory('WebStorage', [function () {
        var WebStorage = {};

        WebStorage.storage = function (key, value) {
            if (!angular.isDefined(value)) {
                return JSON.parse(localStorage.getItem(key));
            } else if (value === null) {
                localStorage.removeItem(key);
                return null;
            } else {
                localStorage.setItem(key, JSON.stringify(value));
                return value;
            }
        };

        WebStorage.session = function (key, value) {
            if (!angular.isDefined(value)) {
                return JSON.parse(sessionStorage.getItem(key));
            } else if (value === null) {
                sessionStorage.removeItem(key);
                return null;
            } else {
                sessionStorage.setItem(key, JSON.stringify(value));
                return value;
            }
        };

        return WebStorage;
    }]);

})(window, window.angular);