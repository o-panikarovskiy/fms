
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

        $rootScope.$on("login:success", updateCurrentUser);

        function updateCurrentUser(event, user) {
            if (!user) return;
            user.isAdmin = AuthService.authorize('admin', user.roles);
            $scope.currentUser = user;
        }

        function init() {
            updateCurrentUser(null, AuthService.currentUser());
        }
        
        init();
    }]);

})(window, window.angular);