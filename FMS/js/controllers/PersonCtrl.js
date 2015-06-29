/// <reference path="../services/DictionaryService.js" />
/// <reference path="../services/PersonService.js" />
(function (window, angular) {
    'use strict';

    angular.module('fms').controller('PersonCtrl', ['$scope', '$state', '$q', 'DictionaryService', 'PersonService', function ($scope, $state, $q, DictionaryService, PersonService) {
        $scope.vm = { loader: { errors: {} }, collapse: {} };
        $scope.person = {};
        $scope.documents = {};

        var DOC_TYPES = { administrativePractice: 1, temporaryResidencePermit: 2, residence: 3, citizenship: 4, migrationRegistration: 5 };


        $scope.toggleDocuments = function (type) {
            $scope.vm.collapse[type] = !$scope.vm.collapse[type];
            if (!$scope.documents[type]) {
                loadDocuments($state.params.id, type);
            }
        };


        function loadDocuments(id, type) {
            $scope.vm.loader[type] = true;
            return PersonService.getDocuments(id, type).then(function (data) {
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
                Object.keys(doc.parameters).forEach(function (key) {
                    var prm = doc.parameters[key];
                    if (prm.prmType === 1) {
                        dicts[prm.prmName] = true;
                    }
                });
            });

            var promises = Object.keys(dicts).filter(function (key) {
                return !$scope.vm.dicts[key];
            }).map(function (key) {
                return loadDict(key);
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

        //TO DO: удалить эту функцию, если вариант с подзагрузкой документов по желанию их устроит
        function afterPersonLoadDocuments(person) {
            var promises = Object.keys(person.docsCount).filter(function (key) {
                return person.docsCount[key] > 0;
            }).map(function (key) {
                $scope.vm.collapse[key] = true;
                return loadDocuments(person.id, key);
            });

            return $q.all(promises);
        };

        function loadDict(name, ruName) {
            return DictionaryService.get(name, $scope.vm, ruName);
        };

        function init() {
            $scope.vm.loader.dicts = true;
            $q.all([loadDict('personCategory'), loadDict('personType'), loadDict('citizenship'), loadDict('privateDoc')]).finally(function () {
                $scope.vm.loader.dicts = false;
            });

            if ($state.params.id) {
                loadPerson($state.params.id);
            }
        };

        init();
    }]);

})(window, window.angular);