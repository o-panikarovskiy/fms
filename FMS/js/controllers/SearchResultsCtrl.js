(function (window, angular) {
    'use strict';

    angular.module('fms').controller('SearchResultsCtrl', ['$scope', '$state', '$timeout', '$window', 'SearchService', function ($scope, $state, $timeout, $window, SearchService) {
        var isRequesting = false;

        $scope.pageChanged = function (page) {
            if (!isRequesting) {
                getResults($state.params.id, page - 1);
            }
        };

        $scope.openPerson = function (person) {
            var url = $state.href('root.person', { id: person.id });
            $window.open(url, "_blank");
        }

        function getResults(id, page, limit) {
            isRequesting = true;
            var timer = $timeout(function () {
                $scope.vm.isSearching = true;
            }, 500);
            return SearchService.search(id, page, limit).then(function (data) {
                $scope.people = data.people;
                angular.extend($scope.searchModel, data.query);
                $scope.vm.pagination.total = data.total;
            }).catch(function (rejection) {
                if (rejection && rejection.data && rejection.status == 400) {
                    $scope.vm.hasRequestError = true;
                };
            }).finally(function () {
                $timeout.cancel(timer);
                $scope.vm.isSearching = false;
                isRequesting = false;
            });
        }

        function init() {
            $scope.vm = angular.extend($scope.vm || {}, { pagination: {} });
            if ($state.params.id) {
                getResults($state.params.id);
                $scope.vm.isSearchBodyCollapsed = true;
            } else {
                $scope.vm.isSearchBodyCollapsed = false;
                $state.go('root.search');
            }
        }

        init();
    }]);

})(window, window.angular);