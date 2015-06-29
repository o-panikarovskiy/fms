(function (window, angular, undefined) {
    'use strict';

    angular.module('fms').service('DialogManager', ['$modal', '$rootScope', function ($modal, $rootScope) {
        var dialogStack = [];

        function openDialog(url, params, options, ctrl, scope) {
            var dlg = $modal.open(angular.extend({
                animation: false, //has backdrop bug if true: https://github.com/angular-ui/bootstrap/issues/3620
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

    }]);

})(window, window.angular);