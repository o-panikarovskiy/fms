/// <reference path="../../app.js" />
(function (window, angular) {
    'use strict';

    angular.module('fms').controller('search.ApCtrl', ['$scope', function ($scope) {
        $scope.vm = { dicts: {} };

        $scope.$watch('searchModel.docs.ap.isChecked', function (val) {
            if (val) loadDicts();
        });

        function loadDicts() {
            $scope.loadDict('Статья', 'administrativePractice', null, $scope.vm);
            $scope.loadDict('Вид правонарушения', 'administrativePractice', null, $scope.vm);
            $scope.loadDict('Орган рассмотрения', 'administrativePractice', null, $scope.vm);
            $scope.loadDict('Статус дела', 'administrativePractice', null, $scope.vm);
            $scope.loadDict('Принятое решение', 'administrativePractice', null, $scope.vm);
            $scope.loadDict('Тип взыскания', 'administrativePractice', null, $scope.vm);
        }
    }]);

})(window, window.angular);