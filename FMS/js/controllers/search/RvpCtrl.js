/// <reference path="../../app.js" />
(function (window, angular) {
    'use strict';

    angular.module('fms').controller('search.RvpCtrl', ['$scope', function ($scope) {
        $scope.$watch('searchModel.docs.rvp.isChecked', function (val) {
            if (val) {
                loadDicts();
            }
        });

        function loadDicts() {
            $scope.loadDict('admissionReasonRvp');
            $scope.loadDict('decisionBaseRvp');
            $scope.loadDict('decisionUserRVP');
        }
    }]);

})(window, window.angular);