/// <reference path="../../app.js" />
(function (window, angular) {
    'use strict';

    angular.module('fms').controller('search.CtzCtrl', ['$scope', function ($scope) {
        $scope.vm = { dicts: {} };

        $scope.$watch('searchModel.docs.ctz.isChecked', function (val) {
            if (val) {
                loadDicts();
            }
        });

        function loadDicts() {
            $scope.loadDict('Тип дела', 'citizenship', null, $scope.vm);
            $scope.loadDict('Основание для приема', 'citizenship', null, $scope.vm);
            $scope.loadDict('Решение', 'citizenship', null, $scope.vm);
            $scope.loadDict('Основание решения', 'citizenship', null, $scope.vm);
        }
    }]);

})(window, window.angular);