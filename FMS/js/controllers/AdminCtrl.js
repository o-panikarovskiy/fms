(function (window, angular) {
    'use strict';

    angular.module('fms').controller('AdminCtrl', ['$scope', '$state', 'AuthService', function ($scope, $state, AuthService) {
        $scope.vm = {};
        $scope.model = {};
    }]);

})(window, window.angular);