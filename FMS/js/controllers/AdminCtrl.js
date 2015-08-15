/// <reference path="../app.js" />
/// <reference path="../services/DialogManager.js" />
/// <reference path="../services/DictionaryService.js" />
/// <reference path="../services/ParameterService.js" />
(function (window, angular) {
    'use strict';

    angular.module('fms').controller('AdminCtrl', ['$scope', '$state', '$q', 'DictionaryService', 'ParameterService', 'DialogManager',
        function ($scope, $state, $q, DictionaryService, ParameterService, DialogManager) {
            $scope.vm = {
                loader: {},
                filter: {},
                collapse: { misc: true, params: true }
            };

            $scope.togglePanel = function (type) {
                $scope.vm.collapse[type] = !$scope.vm.collapse[type];
                if (type === 'misc' && !$scope.misc) {
                    loadMisc();
                } else if (type === 'params' && !angular.isArray($scope.params)) {
                    loadParameters();
                }
            };

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
                });
            }

            $scope.removeMisc = function (misc) {
                DialogManager.showConfirm({
                    text: 'Удаление словаря может испортить процедуру импорта.',
                    okBtn: 'Продолжить'
                }).then(function () {
                    return removeMisc(misc);
                });
            }

            $scope.updateMisc = function (misc) {
                var copy = angular.copy(misc);
                DialogManager.showConfirm({
                    text: 'Изменение названия может испортить процедуру импорта.',
                    okBtn: 'Продолжить'
                }).then(function () {
                    return DialogManager.showCreateMisc({ model: copy });
                }).then(function (model) {
                    copy.name = model.name;
                    updateMisc(model);
                }).then(function () {
                    angular.extend(misc, copy);
                });
            }

            $scope.addMiscValue = function () {
                DialogManager.showCreateMisc({ model: { name: '' } }).then(function (model) {
                    return createMiscValue({
                        miscValue: model.name,
                        miscId: $scope.vm.activeMisc.id
                    });
                });
            }

            $scope.updateMiscValue = function (misc) {
                var copy = angular.copy(misc);
                DialogManager.showCreateMisc({ model: { name: misc.miscValue } }).then(function (model) {
                    copy.miscValue = model.name;
                    return updateMiscValue(copy);
                }).then(function () {
                    angular.extend(misc, copy);
                });
            }

            $scope.removeMiscValue = function (misc) {
                DialogManager.showConfirm().then(function () {
                    return removeMiscValue(misc);
                });
            }

            $scope.addParam = function () {                
                if (!$scope.misc) {
                    loadMisc();
                };
                DialogManager.showCreateParameter(null, null, $scope).then(createParam);
            }

            $scope.removeParam = function (param) {
                DialogManager.showConfirm().then(function () {
                    return removeParam(param);
                });
            }

            function removeMisc(misc) {
                $scope.vm.loader.removeMisc = true;
                return DictionaryService.removeMisc(misc).then(function () {
                    var type = misc.docType || 'person';
                    var idx = $scope.misc[type].indexOf(misc);
                    $scope.misc[type].splice(idx, 1);
                }).catch(showError).finally(function () {
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
                    return misc;
                }).catch(showError).finally(function () {
                    $scope.vm.loader.createMisc = false;
                });
            }

            function updateMisc(misc) {
                $scope.vm.loader.updateMisc = true;
                return DictionaryService.updateMisc(misc).catch(showError).finally(function () {
                    $scope.vm.loader.updateMisc = false;
                });
            }

            function createMiscValue(miscValue) {
                $scope.vm.loader.createMiscValue = true;
                return DictionaryService.createMiscValue(miscValue).then(function (miscValue) {
                    $scope.vm.activeMisc.values.push(miscValue);
                    return miscValue;
                }).catch(showError).finally(function () {
                    $scope.vm.loader.createMiscValue = false;
                });
            }

            function updateMiscValue(miscValue) {
                $scope.vm.loader.updateMiscValue = true;
                return DictionaryService.updateMiscValue(miscValue).catch(showError).finally(function () {
                    $scope.vm.loader.updateMiscValue = false;
                });
            }

            function removeMiscValue(misc) {
                $scope.vm.loader.removeMiscValue = true;
                return DictionaryService.removeMiscValue(misc).then(function () {
                    var type = misc.docType || 'person';
                    var idx = $scope.vm.activeMisc.values.indexOf(misc);
                    $scope.vm.activeMisc.values.splice(idx, 1);
                }).catch(showError).finally(function () {
                    $scope.vm.loader.removeMiscValue = false;
                });
            }

            function createParam(param) {
                $scope.vm.loader.createParam = true;
                return ParameterService.create(param).then(function (param) {
                    $scope.params.push(param);
                    return param;
                }).catch(showError).finally(function () {
                    $scope.vm.loader.createParam = false;
                });
            }

            function removeParam(param) {
                $scope.vm.loader.removeParam = true;
                return ParameterService.remove(param).then(function () {
                    var idx = $scope.params.indexOf(param);
                    $scope.params.splice(idx, 1);
                }).catch(showError).finally(function () {
                    $scope.vm.loader.removeParam = false;
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
                    return list;
                }).finally(function () {
                    $scope.vm.loader.misc = false;
                });
            }

            function loadParameters() {
                $scope.vm.loader.params = true;
                return ParameterService.list().then(function (list) {
                    $scope.params = list;
                }).finally(function () {
                    $scope.vm.loader.params = false;
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

            function showError(res) {
                if (res && res.status == 400 && res.data && res.data.message) {
                    DialogManager.showAlert({ text: res.data.message, danger: true });
                    throw new Error(res.data.message);
                }
                throw new Error(res);
            }

            function init() {

            }


            init();
        }]);

})(window, window.angular);