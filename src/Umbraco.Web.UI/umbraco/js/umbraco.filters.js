/*! umbraco - v0.0.1-SNAPSHOT - 2013-06-03
 * http://umbraco.github.io/Belle
 * Copyright (c) 2013 Per Ploug, Anders Stenteberg & Shannon Deminick;
 * Licensed MIT
 */
'use strict';
define(['app', 'angular'], function (app, angular) {

    /**
    * @ngdoc filter 
    * @name umbraco.filters:umbTreeIconImage
    * @restrict E
    * @description This will properly render the tree icon image based on the tree icon set on the server
    **/
    function treeIconStyleFilter(treeIconHelper) {
        return function (treeNode) {
            if (treeNode.iconIsClass) {
                var converted = treeIconHelper.convertFromLegacy(treeNode);
                if (converted.startsWith('.')) {
                    //its legacy so add some width/height
                    return "height:16px;width:16px;";
                }
                return "";
            }
            return "background-image: url('" + treeNode.iconFilePath + "');height:16px;background-position:2px 0px";
        };
    };
    angular.module('umbraco').filter("umbTreeIconStyle", treeIconStyleFilter);

    /**
    * @ngdoc filter 
    * @name umbraco.filters:umbTreeIconClass
    * @restrict E
    * @description This will properly render the tree icon class based on the tree icon set on the server
    **/
    function treeIconClassFilter(treeIconHelper) {
        return function (treeNode, standardClasses) {
            if (treeNode.iconIsClass) {
                return standardClasses + " " + treeIconHelper.convertFromLegacy(treeNode);
            }
            //we need an 'icon-' class in there for certain styles to work so if it is image based we'll add this
            return standardClasses + " icon-custom-file";
        };
    };
    angular.module('umbraco').filter("umbTreeIconClass", treeIconClassFilter);



    angular.module('umbraco.filters', ["umbraco.services.tree"])
        .filter('interpolate', ['version', function(version) {
            return function(text) {
                return String(text).replace(/\%VERSION\%/mg, version);
            };
        }])
        .filter('propertyEditor', function() {
            return function(input) {
                return "views/propertyeditors/" + String(input).replace('.', '/') + "/editor.html";
            };
        });

return app;
});