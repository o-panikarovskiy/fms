(function (window, angular, undefined) {
    'use strict';
    angular.module('fms').directive('ngDateFix', ['$filter', function ($filter) {
        return {
            require: 'ngModel',
            priority: 1,
            link: function ($scope, $element, $attrs, ngModelCtrl) {
                ngModelCtrl.$formatters.length = 0;
                ngModelCtrl.$parsers.length = 0;

                ngModelCtrl.$formatters.push(function (date) {
                    return $filter('date')(date, 'yyyy-MM-dd');
                });

                ngModelCtrl.$parsers.push(function (valueFromInput) {
                    return valueFromInput;
                });

            }
        }
    }]);
})(window, window.angular);