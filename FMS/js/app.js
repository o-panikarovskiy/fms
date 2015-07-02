(function (window, angular, undefined) {
    'use strict';

    angular.module('fms', ['ngAnimate', 'ui.router', 'ui.bootstrap', 'mgcrea.ngStrap', 'synergetica-file-upload'])
        .config(['$stateProvider', '$urlRouterProvider', '$httpProvider', '$locationProvider', '$datepickerProvider', 'config',
            function ($stateProvider, $urlRouterProvider, $httpProvider, $locationProvider, $datepickerProvider, config) {
                //routes config

                $stateProvider.state('root', { url: '', templateUrl: 'views/layout.root.html', abstract: true });
                $stateProvider.state('root.home', { url: '/', templateUrl: 'views/home.html', controller: 'HomeCtrl', access: 'user' });
                $stateProvider.state('root.search', { url: '/search', templateUrl: 'views/search.html', controller: 'SearchCtrl', access: 'user' });
                $stateProvider.state('root.search.results', { url: '/{id}', templateUrl: 'views/search/results.html', controller: 'SearchResultsCtrl', access: 'user' });
                $stateProvider.state('root.person', { url: '/person/{id}', templateUrl: 'views/person.html', controller: 'PersonCtrl', access: 'user' });
                $stateProvider.state('root.admin', { url: '/admin', templateUrl: 'views/admin.html', controller: 'AdminCtrl', access: 'admin' });
                $stateProvider.state('login', { url: '/login', templateUrl: 'views/login.html', controller: 'LoginCtrl', access: 'public' });

                //redirects config:
                $urlRouterProvider.rule(function ($injector, $location) {
                    //case insensetive match
                    var path = $location.path();
                    var normalized = path.toLowerCase();
                    if (path != normalized) {
                        return normalized;
                    };
                }).when('search/', 'search').otherwise('/');

                //global ajax settings:
                $httpProvider.interceptors.push(['$q', '$injector', 'config', function ($q, $injector, config) {
                    return {
                        request: function (reqConfig) {
                            var viewsUrl = /^views\//;
                            var appUrl = new RegExp('^' + config.API_ROOT);
                            if (appUrl.test(reqConfig.url)) {
                                var token = $injector.get('AuthService').token();
                                if (token) {
                                    reqConfig.headers = reqConfig.headers || {};
                                    reqConfig.headers.Authorization = token;
                                }
                            } else if (viewsUrl.test(reqConfig.url)) {
                                var rev = '?v=' + config.REVISION;
                                reqConfig.url = reqConfig.url + rev;
                            }
                            return reqConfig;
                        },
                        responseError: function (rejection) {
                            if (rejection.status === 401) {
                                $injector.get('AuthService').logout();
                                $injector.get('$state').go('login');
                            }
                            return $q.reject(rejection);
                        }
                    }
                }]);


                //enable html5 routes
                $locationProvider.html5Mode(true).hashPrefix('!');

                angular.extend($datepickerProvider.defaults, {
                    dateFormat: 'dd.MM.yyyy',
                    template: 'views/directives/date-picker.html',
                    startWeek: 1
                });
            }]);

})(window, window.angular);