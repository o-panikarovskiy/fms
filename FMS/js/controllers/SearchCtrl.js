(function (window, angular) {
    'use strict';

    angular.module('fms').controller('SearchCtrl', ['$scope', '$state', '$q', 'DictionaryService', 'SearchService', function ($scope, $state, $q, DictionaryService, SearchService) {
        $scope.vm = { loader: {} };
        $scope.searchModel = {
            person: {
                birthday: null,
                name: null,
                category: null,
                type: null
            }
        };


        $scope.search = function () {
            $scope.vm.isSendingRequest = true;
            return SearchService.query($scope.searchModel).then(function (data) {
                $state.go('root.search.results', { id: data.id });
            }).finally(function () {
                $scope.vm.isSendingRequest = false;
            });
        }

        function loadDict(name, enName) {
            return DictionaryService.get(name, $scope.vm, enName);
        }

        function init() {
            $scope.vm.loader.dicts = true;
            $q.all([loadDict('personCategory'), loadDict('personType'), loadDict('citizenship'), loadDict('privateDoc')]).finally(function () {
                $scope.vm.loader.dicts = false;
            });
        }

        init();
    }]);

})(window, window.angular);