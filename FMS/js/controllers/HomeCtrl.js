/// <reference path="../services/ImportService.js" />
/// <reference path="../services/DialogManager.js" />
/// <reference path="../services/ReportsService.js" />
(function(window, angular) {
    'use strict';

    angular.module('fms').controller('HomeCtrl', ['$scope', '$state', '$timeout', '$interval', '$window', 'UploaderService', 'ImportService', 'ReportsService', 'DialogManager', 'WebStorage',
        function($scope, $state, $timeout, $interval, $window, UploaderService, ImportService, ReportsService, DialogManager, WebStorage) {
            var listenPromise = null;
            $scope.vm = {
                loader: {},
                collapse: {},
                filter: {},
                order: {},
                page: {
                    currentPage: 1,
                    itemsOnPage: 100
                }
            };

            $scope.$on('$destroy', function() {
                if (listenPromise) {
                    $interval.cancel(listenPromise);
                };
            });

            $scope.$watch('vm.filter', syncFilter, true);

            $scope.togglePanel = function(type) {
                $scope.vm.collapse[type] = !$scope.vm.collapse[type];
            };

            $scope.uploader = UploaderService.create();

            $scope.uploader.on('beforeUploadItem', function() {
                $scope.vm.importProgress = { percent: 0 };
            });

            $scope.uploader.on('successItem', function(item, response, status, headers) {
                startImport(response.id);
            });

            $scope.uploader.on('completeAll', function() {
                $scope.vm.clearFileTrigger = !$scope.vm.clearFileTrigger;
            });

            $scope.uploader.on('errorItem', function(item, response, status, headers) {
                angular.extend($scope.vm.importProgress, { hasErrors: true, isCompleted: true, exceptionMessage: response.exceptionMessage });
            });

            $scope.notesFilterChanged = function(filter) {
                if (filter.note === null) {
                    delete filter.note;
                }
            };

            $scope.order = function(predicate) {
                var ord = $scope.vm.order;
                ord.reverse = (ord.predicate === predicate) ? !ord.reverse : false;
                ord.predicate = predicate;
            }

            $scope.openPerson = function(id) {
                var url = $state.href('root.person', { id: id });
                $window.open(url, '_blank');
            }

            function startImport(fileid) {
                return ImportService.import($scope.vm.docType, fileid).then(function(data) {
                    listenProgress(data.id);
                }).catch(function(rejection) {
                    $scope.vm.importProgress.hasErrors = true;
                });
            }

            function listenProgress(progressId) {

                if (listenPromise) {
                    $interval.cancel(listenPromise);
                };

                var isInRequest = false;
                listenPromise = $interval(function() {
                    if (!isInRequest) {
                        isInRequest = true;
                        ImportService.listenProgress(progressId).then(function(data) {
                            if (data.isCompleted) {
                                $interval.cancel(listenPromise);
                            };
                            angular.extend($scope.vm.importProgress, data);
                        }).catch(function(rejection) {
                            $interval.cancel(listenPromise);
                            if (rejection && rejection.data && rejection.status == 400) {
                                angular.extend($scope.vm.importProgress, rejection.data);
                            };
                        }).finally(function() {
                            isInRequest = false;
                        });
                    }
                }, 3000);

                return listenPromise;
            }

            function checkProgress() {
                return ImportService.currentProgress().then(function(data) {
                    if (data.id && data.isCompleted === false) {
                        $scope.vm.importProgress = data;
                        return listenProgress(data.id);
                    };
                });
            }

            function loadDocumentsOnControl() {
                $scope.vm.loader.documentsoncontrol = true;
                return ReportsService.query('documentsoncontrol').then(function(list) {
                    var notes = {};
                    $scope.docsoncontrol = list.map(function(item) {
                        item.docTypeRu = getDocTypeRu(item.docType);
                        item.daysCountStr = getDaysCountStr(item.daysCount);
                        if (!!item.note) notes[item.note] = true;
                        return item;
                    });
                    $scope.notes = Object.keys(notes);
                }).finally(function() {
                    $scope.vm.loader.documentsoncontrol = false;
                });
            }

            function getDocTypeRu(docType) {
                switch (docType) {
                    case 'administrativePractice':
                        return 'Административная практика';
                    case 'migrationRegistration':
                        return 'Миграционный учёт';
                    case 'temporaryResidencePermit':
                        return 'РВП';
                    case 'residence':
                        return 'ВНЖ';
                    case 'citizenship':
                        return 'Гражданство';
                    default:
                        return '';
                }
            }

            function getDaysCountStr(days) {
                var str = days >= 0 ? 'осталось дней: @days' : 'просрочено дней: @days';
                return str.replace('@days', Math.abs(days));
            }

            function syncFilter(filter) {
                var key = 'fms:filter';
                if (filter) {
                    return WebStorage.session(key, filter);
                } else {
                    return WebStorage.session(key);
                }
            }

            function init() {
                checkProgress();
                loadDocumentsOnControl();

                var filter = syncFilter();
                if (angular.isObject(filter)) {
                    angular.extend($scope.vm.filter, filter);
                }
            }

            init();
        }]);

})(window, window.angular);