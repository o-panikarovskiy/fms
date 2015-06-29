/// <reference path="../services/ImportService.js" />
(function (window, angular) {
    'use strict';

    angular.module('fms').controller('HomeCtrl', ['$scope', '$state', '$timeout', '$interval', 'UploaderService', 'ImportService', function ($scope, $state, $timeout, $interval, UploaderService, ImportService) {
        var listenPromise = null;
        $scope.vm = {};

        $scope.$on('$destroy', function () {
            if (listenPromise) {
                $interval.cancel(listenPromise);
            };
        });

        $scope.uploader = UploaderService.create();

        $scope.uploader.on('beforeUploadItem', function () {
            $scope.vm.importProgress = { percent: 0 };
        });

        $scope.uploader.on('successItem', function (item, response, status, headers) {
            startImport(response.id);
        });

        $scope.uploader.on('completeAll', function () {
            $scope.vm.clearFileTrigger = !$scope.vm.clearFileTrigger;
        });

        $scope.uploader.on('errorItem', function (item, response, status, headers) {
            angular.extend($scope.vm.importProgress, { hasErrors: true, isCompleted: true, exceptionMessage: response.exceptionMessage });
        });

        function startImport(fileid) {
            return ImportService.import($scope.vm.docType, fileid).then(function (data) {
                listenProgress(data.id);
            }).catch(function (rejection) {
                $scope.vm.importProgress.hasErrors = true;
            });
        }

        function listenProgress(progressId) {

            if (listenPromise) {
                $interval.cancel(listenPromise);
            };

            var isInRequest = false;
            listenPromise = $interval(function () {
                if (!isInRequest) {
                    isInRequest = true;
                    ImportService.listenProgress(progressId).then(function (data) {
                        if (data.isCompleted) {
                            $interval.cancel(listenPromise);
                        };
                        angular.extend($scope.vm.importProgress, data);
                    }).catch(function (rejection) {
                        $interval.cancel(listenPromise);
                        if (rejection && rejection.data && rejection.status == 400) {
                            angular.extend($scope.vm.importProgress, rejection.data);
                        };
                    }).finally(function () {
                        isInRequest = false;
                    });
                }
            }, 3000);

            return listenPromise;
        }

        function checkProgress() {
            return ImportService.currentProgress().then(function (data) {
                if (data.id && data.isCompleted === false) {
                    $scope.vm.importProgress = data;
                    return listenProgress(data.id);
                };
            });
        }

        function init() {
            checkProgress();
        }

        init();
    }]);

})(window, window.angular);