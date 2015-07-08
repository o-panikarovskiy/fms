/// <reference path="../../app.js" />
(function (window, angular) {
    'use strict';

    angular.module('fms').controller('ApCtrl', ['$scope', function ($scope) {
        $scope.$watch('searchModel.docs.ap.isChecked', function (val) {
            if (val) loadDicts();
        });

        function loadDicts() {
            $scope.loadDict('article');
            $scope.loadDict('crimeType');
            $scope.loadDict('stateDepartment');
            $scope.loadDict('docStatus');
        }
    }]);

})(window, window.angular);