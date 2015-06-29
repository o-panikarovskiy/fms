(function (window, angular, undefined) {
    'use strict';

    angular.module('fms').directive('match', ['$parse', function ($parse) {
        return {
            require: 'ngModel',
            restrict: 'A',
            link: function ($scope, $element, $attrs, $ctrl) {

                var matchGetter = $parse($attrs.match);
                var caselessGetter = $parse($attrs.matchCaseless);

                $scope.$watch(getMatchValue, function () {
                    $ctrl.$$parseAndValidate();
                });

                $ctrl.$validators.match = function () {
                    var match = getMatchValue();
                    if (caselessGetter($scope) && angular.isString(match) && angular.isString($ctrl.$viewValue)) {
                        return $ctrl.$viewValue.toLowerCase() === match.toLowerCase();
                    }
                    return $ctrl.$viewValue === match;
                };

                function getMatchValue() {
                    var match = matchGetter($scope);
                    if (angular.isObject(match) && match.hasOwnProperty('$viewValue')) {
                        match = match.$viewValue;
                    }
                    return match;
                }
            }
        };
    }]);

})(window, window.angular);