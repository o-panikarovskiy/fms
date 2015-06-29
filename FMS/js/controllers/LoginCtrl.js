(function (window, angular) {
    'use strict';

    angular.module('fms').controller('LoginCtrl', ['$scope', '$state', 'AuthService', function ($scope, $state, AuthService) {
        $scope.vm = {};
        $scope.model = {};

        $scope.login = function (form) {
            if (form.$valid) {
                $scope.vm.isSendingRequest = true;
                $scope.vm.errorState = false;
                AuthService.signIn($scope.model).then(function (userinfo) {
                    var route = $scope.redirectState.route;
                    var params = $scope.redirectState.params;
                    delete $scope.redirectState.route;
                    delete $scope.redirectState.params;

                    if (userinfo && AuthService.authorize('admin', userinfo.roles)) {
                        $state.go('root.admin');
                    } else if (route) {
                        $state.go(route, params);
                    } else {
                        $state.go('root.search');
                    }
                }).catch(function () {
                    $scope.vm.errorState = true;
                }).finally(function () {
                    $scope.vm.isSendingRequest = false;
                });
            }
        }
    }]);

})(window, window.angular);