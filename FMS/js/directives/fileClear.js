(function (window, angular, undefined) {
    'use strict';
    angular.module('fms').directive('fileClear', [function () {
        return {
            restrict: 'A',
            scope: {
                model: '=fileClear'
            },
            link: function ($scope, $element, $attrs, $ctrl) {               
                $scope.$watch('model', function () {
                    $element.val('');
                });
            }
        };
    }]);
})(window, window.angular);