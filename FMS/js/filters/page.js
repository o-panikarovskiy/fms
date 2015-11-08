(function(window, angular, undefined) {
    'use strict';
    /**
    * Pagdination filter
    * @param {Array} [arr] input array
    * @param {int} [page] current page
    * @param {int} [pageSize] items count on each page
    * @param {int} [firstPageNumber] first page number. Default 0
    * @return {Array} slice of input array
    */
    angular.module('fms').filter('page', [function() {
        return function(arr, page, pageSize, firstPageNumber) {
            if (angular.isArray(arr)) {
                firstPageNumber = firstPageNumber | 0;
                return arr.slice((page - firstPageNumber) * pageSize).slice(0, pageSize);
            };
            return arr;
        };
    }]);

})(window, window.angular);