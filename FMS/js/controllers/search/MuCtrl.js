/// <reference path="../../app.js" />
(function (window, angular) {
    'use strict';

    angular.module('fms').controller('search.MuCtrl', ['$scope', function ($scope) {
        $scope.$watch('searchModel.docs.mu.isChecked', function (val) {
            if (val) {
                loadDicts();
            }
        });

        function loadDicts() {
            $scope.loadDict('cardMarkMu');
            $scope.loadDict('purposeOfEntryMu');
            $scope.loadDict('primaryExtendMu');
            $scope.loadDict('kPPMu');
        }
    }]);

})(window, window.angular);