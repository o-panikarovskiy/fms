/// <reference path="../../app.js" />
(function (window, angular) {
    'use strict';

    angular.module('fms').controller('search.MuCtrl', ['$scope', function ($scope) {
        $scope.vm = { dicts: {} };

        $scope.$watch('searchModel.docs.mu.isChecked', function (val) {
            if (val) {
                loadDicts();
            }
        });

        function loadDicts() {
            $scope.loadDict('Цель въезда', 'migrationRegistration', null, $scope.vm);
            $scope.loadDict('Первично/Продлено', 'migrationRegistration', null, $scope.vm);
            $scope.loadDict('Отметка проставлена', 'migrationRegistration', null, $scope.vm);
            $scope.loadDict('КПП въезда', 'migrationRegistration', null, $scope.vm);
        }
    }]);

})(window, window.angular);