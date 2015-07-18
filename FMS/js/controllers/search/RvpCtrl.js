/// <reference path="../../app.js" />
(function (window, angular) {
    'use strict';

    angular.module('fms').controller('search.RvpCtrl', ['$scope', function ($scope) {
        $scope.vm = { dicts: {} };

        $scope.$watch('searchModel.docs.rvp.isChecked', function (val) {
            if (val) {
                loadDicts();
            }
        });

        function loadDicts() {
            $scope.loadDict('Основание для приема', 'temporaryResidencePermit', null, $scope.vm);
            $scope.loadDict('Основание решения', 'temporaryResidencePermit', null, $scope.vm);
            $scope.loadDict('Пользователь решения', 'temporaryResidencePermit', null, $scope.vm);
        }
    }]);

})(window, window.angular);