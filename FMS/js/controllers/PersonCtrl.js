/// <reference path="../services/DictionaryService.js" />
/// <reference path="../services/PersonService.js" />
/// <reference path="../app.js" />
/// <reference path="../services/DialogManager.js" />
(function (window, angular) {
    'use strict';

    angular.module('fms').controller('PersonCtrl', ['$scope', '$state', '$q', 'DictionaryService', 'PersonService', 'DocumentService', 'DialogManager',
        function ($scope, $state, $q, DictionaryService, PersonService, DocumentService, DialogManager) {
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

            $scope.addDoc = function (form) {
                DialogManager.showCreateDoc({ personFrom: $scope.person }).then(function (data) {
                    var model = { type: data.docType, personFromId: $scope.person.id };
                    if (data.docType == "migrationRegistration") model.personToId = data.personTo.id;
                    return createDocument(model);
                }).then(function (doc) {
                    $scope.vm.collapse[doc.type] = true;
                    return loadDocuments($scope.person.id, doc.type);
                });
            }

            $scope.removeDoc = function (doc) {
                DialogManager.showConfirm({ text: 'Вы уверены что хотите удалить документ?' }).then(function () {
                    return removeDocument(doc);
                }).then(function () {

                });
            }

            function savePerson(person) {
                $scope.vm.loader.savingPerson = true;
                $scope.vm.state.savingPerson = 0;
                return PersonService.save($scope.person).then(function () {
                    $scope.vm.state.savingPerson = 1;
                }).catch(function (res) {
                    $scope.vm.state.savingPerson = 2;
                    if (res.status === 400) {
                        DialogManager.showAlert({ text: res.data.exceptionMessage, head: 'Ошибка при сохранении' });
                        throw res.data.exceptionMessage;
                    }
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
                return DocumentService.create(doc).then(function (doc) {
                    $scope.vm.state.creatingDocument = 1;
                    $scope.person.docsCount[doc.type] = ($scope.person.docsCount[doc.type] | 0) + 1;
                    return doc;
                }).catch(function () {
                    $scope.vm.state.creatingDocument = 2;
                }).finally(function () {
                    $scope.vm.loader.creatingDocument = false;
                });
            }

            function removeDocument(doc) {
                $scope.vm.loader.removingDocument = true;
                $scope.vm.state.removingDocument = 0;
                return DocumentService.remove(doc).then(function (data) {
                    $scope.vm.state.removingDocument = 1;
                    $scope.person.docsCount[doc.type] = ($scope.person.docsCount[doc.type] | 0) - 1;
                    removeFromArr($scope.documents[doc.type], doc);
                    return data;
                }).catch(function () {
                    $scope.vm.state.removingDocument = 2;
                }).finally(function () {
                    $scope.vm.loader.removingDocument = false;
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
                        if (prm.prmType === 'misc') {
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

            function removeFromArr(arr, item) {
                var idx = arr.indexOf(item);
                if (~idx) arr.splice(idx, 1);
                return arr;
            }

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