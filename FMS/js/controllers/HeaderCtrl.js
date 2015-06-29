(function (window, angular) {
    'use strict';

    angular.module('fms').controller('HeaderCtrl', ['$scope', '$state', 'AuthService', 'DialogManager', function ($scope, $state, AuthService, DialogManager) {
        $scope.logout = function () {
            AuthService.logout();
            $state.go('login');
        }

        $scope.changePassword = function () {
            DialogManager.showChangePassword();
        }
    }]);

})(window, window.angular);