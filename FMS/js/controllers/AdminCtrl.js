/// <reference path="../app.js" />
/// <reference path="../services/DialogManager.js" />
/// <reference path="../services/DictionaryService.js" />
(function (window, angular) {
    'use strict';

    angular.module('fms').controller('AdminCtrl', ['$scope', '$state', '$q', 'DictionaryService', 'DialogManager', function ($scope, $state, $q, DictionaryService, DialogManager) {
        $scope.vm = { loader: {} };
        $scope.model = {};

        $scope.setActiveMisc = function (m) {
            $scope.vm.activeMisc = m;
            if (!angular.isArray(m.values)) {
                loadMiscValues(m);
            }
        }

        $scope.addMisc = function (type) {
            DialogManager.showCreateMisc({ model: { name: '' } }).then(function (model) {
                if (type === 'person') {
                    model.docType = null;
                    model.personCategory = 1 | 2;//Individual+Legal
                } else {
                    model.docType = type;
                    model.personCategory = null;
                }
                return createMisc(model);
            }).catch(function (res) {
                if (res && res.status == 400 && res.data && res.data.message) {
                    DialogManager.showAlert({ text: res.data.message });
                }
            });
        }

        $scope.removeMisc = function (misc) {
            DialogManager.showConfirm().then(function () {
                return removeMisc(misc);
            }).catch(function (res) {
                if (res && res.status == 400 && res.data && res.data.message) {
                    DialogManager.showAlert({ text: res.data.message });
                }
            });
        }

        $scope.updateMisc = function (misc) {
            var model = angular.copy(misc);
            DialogManager.showConfirm({
                text: 'Изменение названия может испортить процедуру импорта.',
                okBtn: 'Продолжить'
            }).then(function () {
                return DialogManager.showCreateMisc({ model: model });
            }).then(function (data) {
                model = data;
                updateMisc(data);
            }).then(function () {
                angular.extend(misc, model);
            }).catch(function (res) {
                if (res && res.status == 400 && res.data && res.data.message) {
                    DialogManager.showAlert({ text: res.data.message });
                }
            });
        }

        function removeMisc(misc) {
            $scope.vm.loader.removeMisc = true;
            return DictionaryService.removeMisc(misc).then(function () {
                var type = misc.docType || 'person';
                var idx = $scope.misc[type].indexOf(misc);
                $scope.misc[type].splice(idx, 1);
            }).finally(function () {
                $scope.vm.loader.removeMisc = false;
            });
        }

        function createMisc(misc) {
            $scope.vm.loader.createMisc = true;
            return DictionaryService.createMisc(misc).then(function (misc) {
                var type = misc.docType || 'person';
                misc.values = [];
                $scope.misc[type].push(misc);
                $scope.vm.activeMisc = misc;
            }).finally(function () {
                $scope.vm.loader.createMisc = false;
            });
        }

        function updateMisc(misc) {
            $scope.vm.loader.updateMisc = true;
            return DictionaryService.updateMisc(misc).finally(function () {
                $scope.vm.loader.updateMisc = false;
            });
        }

        function loadMiscValues(misc) {
            misc.loading = true;
            return DictionaryService.miscValues(misc.id).then(function (list) {
                misc.values = list;
            }).finally(function () {
                delete misc.loading;
            });
        }

        function loadMisc() {
            $scope.vm.loader.misc = true;
            return DictionaryService.miscList().then(function (list) {
                $scope.misc = groupBy(list, function (misc) {
                    return misc.docType || 'person';
                });
                $scope.vm.activeMisc = $scope.misc.person[0];
                return loadMiscValues($scope.vm.activeMisc);
            }).finally(function () {
                $scope.vm.loader.misc = false;
            });
        }

        function loadAll() {
            $scope.vm.loader.all = true;
            $q.all([
                loadMisc()
            ]).finally(function () {
                $scope.vm.loader.all = false;
            });
        }

        function groupBy(arr, fn) {
            var group = Object.create(null);
            arr.forEach(function (item) {
                var key = fn(item);
                if (!group[key]) group[key] = [];
                group[key].push(item);
            });
            return group;
        }

        function init() {
            loadAll();
        }


        init();
    }]);

})(window, window.angular);