/// <reference path="../../app.js" />
(function (window, angular) {
    'use strict';

    angular.module('fms').controller('search.CtzCtrl', ['$scope', function ($scope) {
        $scope.$watch('searchModel.docs.ctz.isChecked', function (val) {
            if (val) {
                loadDicts();
            }
        });

        function loadDicts() {
            $scope.loadDict('docActionTypeCtz');
            $scope.loadDict('docAdmissionVng');
            $scope.loadDict('decisionTypeVng');
            $scope.loadDict('seriesVng');
        }
    }]);

})(window, window.angular);