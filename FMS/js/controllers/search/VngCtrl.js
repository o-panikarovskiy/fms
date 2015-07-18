/// <reference path="../../app.js" />
(function (window, angular) {
    'use strict';

    angular.module('fms').controller('search.VngCtrl', ['$scope', function ($scope) {
        $scope.vm = { dicts: {} };

        $scope.$watch('searchModel.docs.vng.isChecked', function (val) {
            if (val) {
                loadDicts();
            }
        });

        function loadDicts() {
            $scope.loadDict('Тип дела', 'residence', null, $scope.vm);
            $scope.loadDict('Основание дела', 'residence', null, $scope.vm);
            $scope.loadDict('Тип решения', 'residence', null, $scope.vm);
            $scope.loadDict('Серия ВНЖ', 'residence', null, $scope.vm);
        }
    }]);

})(window, window.angular);