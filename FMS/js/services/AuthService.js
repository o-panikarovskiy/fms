(function (window, angular, undefined) {
    'use strict';

    angular.module('fms').factory('AuthService', ['$http', 'WebStorage', 'config', function ($http, WebStorage, config) {
        var userRoles = {
            'anonymous': 1,
            'user': 2,
            'admin': 4
        };

        var accessLevels = {
            'anonymous': userRoles.anonymous,
            'public': userRoles.anonymous | userRoles.user | userRoles.admin,
            'user': userRoles.user | userRoles.admin,
            'admin': userRoles.admin
        }

        function toParam(object, prefix) {
            return Object.keys(object).map(function (key) {
                var value = object[key];
                var key = prefix ? prefix + '[' + key + ']' : key;

                if (value === null) {
                    value = window.encodeURIComponent(key) + '=';
                } else if (typeof (value) !== 'object') {
                    value = window.encodeURIComponent(key) + '=' + window.encodeURIComponent(value);
                } else {
                    value = toParam(value, key);
                }

                return value;
            }).join('&').replace(/%20/g, "+");
        }

        var AuthService = {};

        AuthService.signIn = function (model) {
            this.logout();

            model.grant_type = 'password';

            return $http({
                method: 'POST',
                url: config.API_ROOT + 'auth/token',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded; charset=utf-8'
                },
                data: toParam(model)
            }).then(function (res) {
                WebStorage.storage('fms:token', res.data.access_token);
                return res.data;
            }).then(function () {
                return $http.get(config.API_ROOT + 'auth/userinfo');
            }).then(function (res) {
                WebStorage.storage('fms:user', res.data);
                return res.data;
            });
        };

        AuthService.changePassword = function (model) {
            return $http.post(config.API_ROOT + 'auth/ChangePassword', model);
        }


        AuthService.logout = function () {
            WebStorage.storage('fms:token', null);
            WebStorage.storage('fms:user', null);
        }

        AuthService.token = function (token) {
            if (token === null || angular.isDefined(token)) {
                return WebStorage.storage('fms:token', token);
            } else {
                return WebStorage.storage('fms:token');
            }
        }

        AuthService.currentUser = function (user) {
            if (user === null || angular.isDefined(user)) {
                return WebStorage.storage('fms:user', user);
            } else {
                return WebStorage.storage('fms:user');
            }
        };

        AuthService.isAuthenticated = function () {
            return this.token() !== null;
        }

        AuthService.authorize = function (accessLevel, roles) {
            if (!angular.isArray(roles)) {
                var user = this.currentUser();
                if (user) {
                    roles = user.roles;
                } else {
                    roles = ['anonymous'];
                }
            };

            return roles.some(function (role) {
                return (accessLevels[accessLevel] & userRoles[role.toLowerCase()]) > 0;
            });
        };

        function successLogin(res) {
            WebStorage.storage('fms:user', res.data.data.userInfo);
            return res.data;
        };

        return AuthService;
    }]);

})(window, window.angular);