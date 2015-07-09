/// <reference path="../../app.js" />
(function (window, angular) {
    'use strict';

    angular.module('fms').controller('search.ApCtrl', ['$scope', function ($scope) {
        $scope.$watch('searchModel.docs.ap.isChecked', function (val) {
            if (val) loadDicts();
        });

        function loadDicts() {
            $scope.loadDict('article');
            $scope.loadDict('crimeType');
            $scope.loadDict('stateDepartment');
            $scope.loadDict('docStatus');
            $scope.loadDict('decreeStr');
            $scope.loadDict('penaltyType');
        }
    }]);

})(window, window.angular);