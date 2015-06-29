(function (window, angular, undefined) {
    'use strict';
    angular.module('fms').directive('fileDialog', [function () {
        return {          
            restrict: 'A',
            link: function ($scope, $element, $attrs, $ctrl) {
                var $fileInput = $element.find('input');

                $element.on('click', function () {
                    if ($fileInput) {
                        $fileInput[0].click();
                    }
                });
            }
        };
    }]);
})(window, window.angular);