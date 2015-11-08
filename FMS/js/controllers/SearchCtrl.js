(function(window, angular) {
    'use strict';

    angular.module('fms').controller('SearchCtrl', ['$scope', '$state', '$q', '$timeout', '$window', 'DictionaryService', 'ParameterService', 'SearchService',
        function($scope, $state, $q, $timeout, $window, DictionaryService, ParameterService, SearchService) {
            var model = {
                person: {
                    birthday: null,
                    name: null,
                    category: null,
                    type: null
                },
                docs: {
                    administrativePractice: {
                        docParams: {}
                    },
                    migrationRegistration: {
                        docParams: {}
                    },
                    temporaryResidencePermit: {
                        docParams: {}
                    },
                    residence: {
                        docParams: {}
                    },
                    citizenship: {
                        docParams: {}
                    }
                }
            };

            $scope.$on('$stateChangeSuccess', reset);

            $scope.vm = { loader: {}, pagination: {} };

            $scope.searchModel = angular.copy(model);

            $scope.search = function() {
                $scope.vm.isSendingRequest = true;
                return SearchService.query($scope.searchModel).then(function(data) {
                    $state.go('root.search.results', { id: data.id });
                    getResults(data.id);
                }).finally(function() {
                    $scope.vm.isSendingRequest = false;
                });
            }

            $scope.reset = function () {
                reset();
                $state.go('root.search');
            };

            $scope.toggleDocSearch = function(type) {
                if (angular.isArray($scope.parameters[type])) {
                    loadAdditionalDicts($scope.parameters[type]);
                }
            };

            $scope.pageChanged = function(page) {
                getResults($state.params.id, page - 1);
            };

            $scope.openPerson = function(person) {
                var url = $state.href('root.person', { id: person.id });
                $window.open(url, "_blank");
            };

            function reset() {
                delete $scope.people;
                $scope.searchModel = angular.copy(model);
                $scope.vm.pagination.total = 0;
                $scope.vm.isSearchBodyCollapsed = false;
            }

            function loadDict(name, docType, categody, vm) {
                $scope.vm.loader.dict = true;
                return DictionaryService.get(name, docType, categody, vm || $scope.vm).finally(function() {
                    $scope.vm.loader.dict = false;
                });
            }

            function loadAdditionalDicts(parameters) {
                var dicts = {};
                parameters.forEach(function(prm) {
                    if (prm.type === 'misc') {
                        dicts[prm.miscParentId] = true;
                    }
                })

                var promises = Object.keys(dicts).filter(function(key) {
                    return !angular.isArray($scope.vm.dicts[key]);
                }).map(function(key) {
                    return loadDict(key, null, null);
                });

                $scope.vm.loader.addDicts = true;
                return $q.all(promises).finally(function() {
                    $scope.vm.loader.addDicts = false;
                });
            };           

            function loadDocParams() {
                $scope.vm.loader.params = true;
                return ParameterService.list().then(function(list) {
                    var parameters = {};
                    list.map(function(item) {
                        if (!angular.isArray(parameters[item.docType])) {
                            parameters[item.docType] = [];
                        }
                        parameters[item.docType].push(item);
                        return item;
                    });
                    $scope.parameters = parameters;
                    return parameters;
                }).finally(function() {
                    $scope.vm.loader.params = false;
                });
            }

            function getResults(id, page, limit) {
                var timer = $timeout(function() {
                    $scope.vm.isSearching = true;
                }, 500);
                return SearchService.search(id, page, limit).then(function(data) {
                    $scope.people = data.people;
                    angular.extend($scope.searchModel, data.query);
                    $scope.vm.pagination.total = data.total;
                    $scope.vm.isSearchBodyCollapsed = true;
                    return data;
                }).catch(function(rejection) {
                    if (rejection && rejection.status) {
                        $scope.vm.hasRequestError = true;
                    };
                }).finally(function() {
                    $timeout.cancel(timer);
                    $scope.vm.isSearching = false;
                });
            }

            function init() {
                $scope.vm.loader.all = true;

                var promises = [
                    loadDict('personCategory'),
                    loadDict('personType'),
                    loadDict('Гражданство', null, 'individual'),
                    loadDict('Личный документ', null, 'individual')
                ];

                var subPromise = [loadDocParams()];
                if ($state.params.id) {
                    subPromise.push(getResults($state.params.id));                   
                }

                promises.push($q.all(subPromise).then(function() {
                    var parameters = [];
                    Object.keys($scope.searchModel.docs).filter(function(key) {
                        return $scope.searchModel.docs[key].isChecked;
                    }).map(function(key) {
                        parameters = parameters.concat($scope.parameters[key]);
                        return key;
                    });
                    loadAdditionalDicts(parameters);
                }));

                $q.all(promises).finally(function() {
                    $scope.vm.loader.all = false;
                });
            }

            init();
        }]);

})(window, window.angular);