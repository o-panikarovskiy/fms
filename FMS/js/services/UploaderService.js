(function (window, angular, undefined) {
    'use strict';

    angular.module('fms').factory('UploaderService', ['$state', 'FileUploader', 'AuthService', 'config', function ($state, FileUploader, AuthService, config) {
        var UploaderService = {};

        UploaderService.create = function (fn, params) {
            var onBeforeUploadItem = null;

            if (angular.isFunction(fn)) {
                onBeforeUploadItem = fn;
            } else if (angular.isObject(fn)) {
                params = fn;
            } else {
                params = params || {}
            };

            var result = new FileUploader(angular.merge({
                url: config.API_ROOT + 'upload',
                queueLimit: 1,
                autoUpload: true,
                removeAfterUpload: true,
                headers: {
                    Authorization: AuthService.token()
                }
            }, params));

            result.on('errorItem', function (item, response, status, headers) {
                if (status === 401) {
                    AuthService.clear();
                    $state.go('login');
                }
            });

            if (onBeforeUploadItem) {
                result.on('beforeUploadItem', onBeforeUploadItem);
            }

            return result;
        };

        return UploaderService;
    }]);

})(window, window.angular);