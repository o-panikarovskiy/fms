
(function (window, angular) {
    'use strict';

    angular.module('fms').controller('LayoutCtrl', ['$rootScope', '$scope', '$state', '$http', 'AuthService', function ($rootScope, $scope, $state, $http, AuthService) {
        $scope.gvm = {};
        $scope.redirectState = {};

        $rootScope.$on("$stateChangeStart", function (event, next, params) {
            if (!AuthService.authorize(next.access)) {
                event.preventDefault();
                if (AuthService.isAuthenticated()) {
                    $state.go('root.home');
                } else {
                    $scope.redirectState.route = next.name;
                    $scope.redirectState.params = params;
                    $state.go('login');
                }
            }
        });
    }]);

})(window, window.angular);