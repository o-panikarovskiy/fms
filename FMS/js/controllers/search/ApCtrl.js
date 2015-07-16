/// <reference path="../../app.js" />
(function (window, angular) {
    'use strict';

    angular.module('fms').controller('search.ApCtrl', ['$scope', function ($scope) {
        $scope.$watch('searchModel.docs.ap.isChecked', function (val) {
            if (val) loadDicts();
        });

        function loadDicts() {
            $scope.loadDict('Статья');
            $scope.loadDict('Вид правонарушения');
            $scope.loadDict('Орган рассмотрения');
            $scope.loadDict('Статус дела');
            $scope.loadDict('Принятое решение');
            $scope.loadDict('Тип взыскания');
        }
    }]);

})(window, window.angular);