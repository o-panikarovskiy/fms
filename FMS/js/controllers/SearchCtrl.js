(function(window, angular) {
    'use strict';

    angular.module('fms').controller('SearchCtrl', ['$scope', '$state', '$q', 'DictionaryService', 'ParameterService', 'SearchService', function($scope, $state, $q, DictionaryService, ParameterService, SearchService) {
        var model = {
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

        $scope.vm = { loader: {} };
        $scope.searchModel = angular.copy(model);

        $scope.search = function() {
            $scope.vm.isSendingRequest = true;
            return SearchService.query(bindModel($scope.searchModel)).then(function(data) {
                $state.go('root.search.results', { id: data.id });
            }).finally(function() {
                $scope.vm.isSendingRequest = false;
            });
        }

        $scope.reset = function() {
            $scope.searchModel = angular.copy(model);
        }

        $scope.loadDict = function(name, docType, categody, vm) {
            vm = vm || $scope.vm;
            if (!vm.dicts[name]) {
                loadDict(name, docType, categody, vm);
            }
        };

        function bindModel(model) {
            var m = angular.copy(model);

            if (m.docs) {
                var isAnyChecked = Object.keys(m.docs).some(function(key) {
                    return m.docs[key] && m.docs[key].isChecked
                });
                if (!isAnyChecked) {
                    delete m.docs;
                }
            };

            return m;
        }

        function loadDict(name, docType, categody, vm) {
            $scope.vm.loader.dict = true;
            return DictionaryService.get(name, docType, categody, vm || $scope.vm).finally(function () {
                $scope.vm.loader.dict = false;
            });
        }

        function loadDocParams() {
            $scope.vm.loader.dict = true;
            return ParameterService.list().then(function(list) {
                var prms = {};
                list.map(function(item) {
                    if (!angular.isArray(prms[item.docType])) {
                        prms[item.docType] = [];
                    }
                    prms[item.docType].push(item);
                    return item;
                });
                $scope.parameters = prms;
            }).finally(function() {
                $scope.vm.loader.dict = false;
            });
        }

        function init() {
            $scope.vm.loader.dicts = true;
            $q.all([
                loadDict('personCategory'),
                loadDict('personType'),
                loadDict('Гражданство', null, 'individual'),
                loadDict('Личный документ', null, 'individual'),
                loadDocParams()
            ]).finally(function() {
                $scope.vm.loader.dicts = false;
            });
        }

        init();
    }]);

})(window, window.angular);