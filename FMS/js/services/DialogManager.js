﻿/// <reference path="../app.js" />
(function (window, angular, undefined) {
    'use strict';

    angular.module('fms').service('DialogManager', ['$modal', '$rootScope', function ($modal, $rootScope) {
        var dialogStack = [];

        function openDialog(url, params, options, ctrl, scope) {
            var dlg = $modal.open(angular.extend({
                animation: true, //has backdrop bug if true: https://github.com/angular-ui/bootstrap/issues/3620
                backdrop: true,
                keyboard: true,
                backdropClass: '',
                windowClass: '',
                resolve: {
                    $scopeParams: function () {
                        return params;
                    }
                }
            }, {
                controller: ctrl,
                scope: scope,
                templateUrl: url
            }, options));

            dialogStack.push(dlg);
            return dlg.result['finally'](function () {
                dialogStack.pop();
            });
        }

        $rootScope.$on('$locationChangeStart', function () {
            dialogStack.forEach(function (dlg) {
                dlg.dismiss('window location change');
            });
        });

        this.showAlert = function (params, options, scope) {

            var ctrl = ['$scope', '$scopeParams', '$modalInstance', function ($scope, $scopeParams, $modalInstance) {
                $scope.vm = angular.extend({
                    text: '',
                    okBtn: 'Закрыть'
                }, $scopeParams);

                if ($scopeParams.danger) {
                    $scope.vm.head = $scope.vm.head || 'Ошибка!'
                    $scope.vm.danger = true;
                }

                $scope.ok = function () {
                    $modalInstance.close();
                };
            }];

            return openDialog('views/dialogs/alert.html', params, options, ctrl, scope);
        };

        this.showConfirm = function (params, options, scope) {

            var ctrl = ['$scope', '$scopeParams', '$modalInstance', function ($scope, $scopeParams, $modalInstance) {
                $scope.vm = angular.extend({
                    text: 'Вы уверены?',
                    okBtn: 'Да',
                    cancelBtn: 'Нет'
                }, $scopeParams);

                $scope.ok = function () {
                    $modalInstance.close();
                };

                $scope.close = function () {
                    $modalInstance.dismiss();
                };
            }];

            return openDialog('views/dialogs/confirm.html', params, options, ctrl, scope);
        };

        this.showChangePassword = function (params, options, scope) {

            var ctrl = ['$scope', '$modalInstance', 'AuthService', function ($scope, $modalInstance, AuthService) {
                $scope.vm = {};
                $scope.model = {};

                $scope.ok = function (form) {
                    $scope.vm.status = 0;
                    $scope.vm.statusText = '';
                    if (form.$valid) {
                        $scope.vm.isSendingRequest = true;
                        AuthService.changePassword($scope.model).then(function () {
                            $scope.vm.status = 200;
                        }).catch(function (rejection) {
                            if (rejection && rejection.status === 400 && rejection.data && rejection.data.modelState) {
                                $scope.vm.status = rejection.status;
                                $scope.vm.statusText = rejection.data.modelState.error[0];
                            }
                        }).finally(function () {
                            $scope.vm.isSendingRequest = false;
                        });
                    }
                };

                $scope.closeSuccess = function () {
                    $modalInstance.close($scope.model);
                }

                $scope.close = function () {
                    $modalInstance.dismiss();
                };
            }];

            return openDialog('views/dialogs/change.password.html', params, options, ctrl, scope);
        };

        this.showCreateDoc = function (params, options, scope) {

            var ctrl = ['$scope', '$scopeParams', '$modalInstance', 'DictionaryService', 'SearchService',
                function ($scope, $scopeParams, $modalInstance, DictionaryService, SearchService) {
                    $scope.vm = {};
                    $scope.model = angular.extend({}, $scopeParams);

                    $scope.ok = function (form) {
                        $modalInstance.close($scope.model);
                    };

                    $scope.close = function () {
                        $modalInstance.dismiss();
                    };

                    $scope.getPeople = function (name) {
                        var type = $scope.model.personFrom.type == 'applicant' ? 'host' : 'applicant';
                        return SearchService.peopleByName(name, type).then(function (res) {
                            return res;
                        });
                    }

                    function init() {
                        $scope.vm.placeholder = 'Введите имя ' + ($scope.model.personFrom.type == 'applicant' ?
                            'принимающей стороны' : 'соискателя');

                        DictionaryService.get('documentType', null, null, $scope.vm);
                    }

                    init();
                }];

            return openDialog('views/dialogs/create.doc.html', params, options, ctrl, scope);
        };

        this.showCreatePerson = function (params, options, scope) {

            var ctrl = ['$scope', '$scopeParams', '$modalInstance', 'DictionaryService', 'PersonService',
                function ($scope, $scopeParams, $modalInstance, DictionaryService, PersonService) {
                    $scope.vm = {};
                    $scope.model = angular.extend({}, $scopeParams);

                    $scope.ok = function (form) {
                        create($scope.model).then(function (person) {
                            $modalInstance.close(person);
                        });
                    };

                    $scope.close = function () {
                        $modalInstance.dismiss();
                    };

                    function create(person) {
                        $scope.vm.creating = true;
                        $scope.vm.error = null;
                        return PersonService.create(person).catch(function (rejection) {
                            if (rejection.status === 400) {
                                $scope.vm.error = rejection.data.exceptionMessage;
                                throw $scope.vm.error;
                            }
                        }).finally(function () {
                            $scope.vm.creating = false;
                        });
                    }

                    function init() {
                        DictionaryService.get('personCategory', null, null, $scope.vm);
                        DictionaryService.get('personType', null, null, $scope.vm);
                    }

                    init();
                }];

            return openDialog('views/dialogs/create.person.html', params, options, ctrl, scope);
        };

        this.showCreateMisc = function (params, options, scope) {

            var ctrl = ['$scope', '$scopeParams', '$modalInstance', function ($scope, $scopeParams, $modalInstance) {
                $scope.vm = {};
                $scope.model = angular.extend({}, $scopeParams.model);


                $scope.ok = function () {
                    $modalInstance.close($scope.model);
                };

                $scope.close = function () {
                    $modalInstance.dismiss();
                };
            }];

            return openDialog('views/dialogs/create.misc.html', params, options, ctrl, scope);
        };

        this.showCreateParameter = function (params, options, scope) {

            var ctrl = ['$scope', '$scopeParams', '$modalInstance',  function ($scope, $scopeParams, $modalInstance) {
                $scope.vm = {};
                $scope.model = {};


                $scope.ok = function () {
                    $modalInstance.close($scope.model);
                };

                $scope.close = function () {
                    $modalInstance.dismiss();
                };
           
            }];

            return openDialog('views/dialogs/create.param.html', params, options, ctrl, scope);
        };

    }]);

})(window, window.angular);