/// <reference path="../../app.js" />
(function (window, angular) {
    'use strict';

    angular.module('fms').controller('search.VngCtrl', ['$scope', function ($scope) {      
        $scope.$watch('searchModel.docs.vng.isChecked', function (val) {
            if (val) {
                loadDicts();
            }
        });

        function loadDicts() {
            $scope.loadDict('docActionTypeVng');
            $scope.loadDict('docAdmissionVng');
            $scope.loadDict('decisionTypeVng');
            $scope.loadDict('seriesVng');
        }
    }]);

})(window, window.angular);