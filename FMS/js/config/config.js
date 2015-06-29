(function (window, angular, undefined) {
    angular.module('fms').constant('config', {
        API_ROOT: 'api/',
        REVISION: '@REVISION'
    });
})(window, window.angular);