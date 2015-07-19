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

        $scope.createPerson = function () {
            DialogManager.showCreatePerson({ category: 'individual', type: 'applicant' }).then(function (person) {
                $state.go('root.person', { id: person.id });
            });
        }
    }]);

})(window, window.angular);