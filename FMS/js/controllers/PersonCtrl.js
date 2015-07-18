/// <reference path="../services/DictionaryService.js" />
/// <reference path="../services/PersonService.js" />
/// <reference path="../app.js" />
(function (window, angular) {
    'use strict';

    angular.module('fms').controller('PersonCtrl', ['$scope', '$state', '$q', 'DictionaryService', 'PersonService', 'DocumentService',
        function ($scope, $state, $q, DictionaryService, PersonService, DocumentService) {
            $scope.vm = {
                loader: {
                    errors: {}
                },
                collapse: {},
                state: {},
                modelUpdate: {
                    updateOn: 'default blur',
                    debounce: {
                        'default': 500,
                        'blur': 0
                    }
                }
            };

            $scope.person = {};
            $scope.documents = {};

            $scope.toggleDocuments = function (type) {
                $scope.vm.collapse[type] = !$scope.vm.collapse[type];
                if (!$scope.documents[type]) {
                    loadDocuments($scope.person.id, type);
                }
            };

            $scope.save = function (form) {
                if (form.$valid) {
                    savePerson($scope.person);
                }
            }

            $scope.saveDoc = function (form, doc) {
                if (form.$valid) {
                    saveDocument(doc);
                }
            }

            $scope.createDoc = function (form) {
                if (form.$valid) {
                    createDocument({
                        type: $scope.vm.newDocType,
                        personFromId: $scope.person.id
                    }).then(function (doc) {

                    });
                }
            }

            function savePerson(person) {
                $scope.vm.loader.savingPerson = true;
                $scope.vm.state.savingPerson = 0;
                return PersonService.save($scope.person).then(function () {
                    $scope.vm.state.savingPerson = 1;
                }).catch(function () {
                    $scope.vm.state.savingPerson = 2;
                }).finally(function () {
                    $scope.vm.loader.savingPerson = false;
                });
            }

            function saveDocument(doc) {
                $scope.vm.loader.savingDocument = true;
                $scope.vm.state.savingDocument = 0;
                return DocumentService.save(doc).then(function () {
                    $scope.vm.state.savingDocument = 1;
                }).catch(function () {
                    $scope.vm.state.savingDocument = 2;
                }).finally(function () {
                    $scope.vm.loader.savingDocument = false;
                });
            }

            function createDocument(doc) {
                $scope.vm.loader.creatingDocument = true;
                $scope.vm.state.creatingDocument = 0;
                return DocumentService.create(doc).then(function (resDoc) {
                    $scope.vm.state.creatingDocument = 1;
                    $scope.person.docsCount[doc.type] = ($scope.person.docsCount[doc.type] | 0) + 1;
                    return resDoc;
                }).catch(function () {
                    $scope.vm.state.creatingDocument = 2;
                }).finally(function () {
                    $scope.vm.loader.creatingDocument = false;
                });
            }

            function loadDocuments(personId, type) {
                $scope.vm.loader[type] = true;
                return PersonService.getDocuments(personId, type).then(function (data) {
                    $scope.documents[type] = data.documents;
                    loadAdditionalDicts(data.documents);
                    return data;
                }).catch(function (rejection) {
                    $scope.vm.loader.errors[type] = true;
                }).finally(function () {
                    $scope.vm.loader[type] = false;
                });
            };

            function loadAdditionalDicts(documents) {
                var dicts = {};
                documents.forEach(function (doc) {
                    doc.parameters.forEach(function (prm) {
                        if (prm.prmType === 1) {
                            dicts[prm.dicId] = true;
                        }
                    });
                });

                var promises = Object.keys(dicts).filter(function (key) {
                    return !$scope.vm.dicts[key];
                }).map(function (key) {
                    return loadDict(key, null, null);
                });

                $scope.vm.loader.addDicts = true;
                return $q.all(promises).finally(function () {
                    $scope.vm.loader.addDicts = false;
                });
            };

            function loadPerson(id) {
                $scope.vm.loader.person = true;
                return PersonService.getOne(id).then(function (person) {
                    $scope.person = person;
                    return person;
                }).catch(function (rejection) {
                    $scope.vm.loader.errors.person = true;
                }).finally(function () {
                    $scope.vm.loader.person = false;
                });
            };

            function loadDict(name, docType, category) {
                return DictionaryService.get(name, docType, category, $scope.vm);
            };

            function init() {
                $scope.vm.loader.dicts = true;
                $q.all([loadDict('personCategory'), loadDict('personType'), loadDict('Гражданство', null, 'individual'), loadDict('Личный документ', null, 'individual')]).finally(function () {
                    $scope.vm.loader.dicts = false;
                });

                if ($state.params.id) {
                    loadPerson($state.params.id);
                }
            };

            init();
        }]);

})(window, window.angular);