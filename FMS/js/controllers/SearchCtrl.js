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
            },
            docs: {
                ap: {
                    isChecked: false
                },
                mu: {
                    isChecked: false
                },
                rvp: {
                    isChecked: false
                },
                vng: {
                    isChecked: false
                },
                ctz: {
                    isChecked: false
                }
            }
        };

        $scope.search = function () {
            $scope.vm.isSendingRequest = true;
            return SearchService.query(bindModel($scope.searchModel)).then(function (data) {
                $state.go('root.search.results', { id: data.id });
            }).finally(function () {
                $scope.vm.isSendingRequest = false;
            });
        }

        $scope.loadDict = function (name) {
            if (!$scope.vm.dicts[name]) {
                loadDict(name);
            }
        };

        function bindModel(model) {
            var m = angular.copy(model);

            if (m.docs) {
                var isAnyChecked = Object.keys(m.docs).some(function (key) {
                    return m.docs[key] && m.docs[key].isChecked
                });
                if (!isAnyChecked) {
                    delete m.docs;
                }
            };

            return m;
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