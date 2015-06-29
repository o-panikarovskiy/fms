/// <reference path="../services/PersonService.js" />
(function (window, angular, undefined) {
    'use strict';
    angular.module('fms').directive('factHistory', ['PersonService', function (PersonService) {
        return {
            restrict: 'A',
            replace: true,
            scope: {
                personId: '=',
                factId: '='
            },
            templateUrl: function ($element, $attrs) {
                return $attrs.templateUrl || 'views/directives/fact-history.html'
            },
            link: function ($scope, $element, $attrs, $ctrl) {
                $scope.vm = {};

                $scope.onToggle = function (isOpen) {
                    if (isOpen && !$scope.vm.facts) {
                        var valueName = $attrs.valueName;
                        PersonService.getFacts($scope.personId, $scope.factId).then(function (data) {
                            $scope.vm.facts = data.facts.map(function (fact) {
                                return {
                                    date: fact.factDate,
                                    id: fact.id,
                                    value: fact[valueName]
                                }
                            });
                        });
                    }
                }
            }
        };
    }]);
})(window, window.angular);