(function (window, angular, undefined) {
    'use strict';

    angular.module('fms').directive('matchEq', ['$parse', function ($parse) {
        return {
            require: 'ngModel',
            restrict: 'A',
            link: function ($scope, $element, $attrs, $ctrl) {

                var matchGetter = $parse($attrs.matchEq);
                var caselessGetter = $parse($attrs.matchCaseless);

                $scope.$watch(getMatchValue, function () {
                    $ctrl.$$parseAndValidate();
                });

                $ctrl.$validators.matchEq = function () {
                    var matchEq = getMatchValue();
                    if (caselessGetter($scope) && angular.isString(matchEq) && angular.isString($ctrl.$viewValue)) {
                        return $ctrl.$viewValue.toLowerCase() === matchEq.toLowerCase();
                    }
                    return $ctrl.$viewValue === matchEq;
                };

                function getMatchValue() {
                    var matchEq = matchGetter($scope);
                    if (angular.isObject(matchEq) && matchEq.hasOwnProperty('$viewValue')) {
                        matchEq = matchEq.$viewValue;
                    }
                    return matchEq;
                }
            }
        };
    }]);

})(window, window.angular);