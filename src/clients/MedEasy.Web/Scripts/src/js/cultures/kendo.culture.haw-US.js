/*
* Kendo UI v2015.1.408 (http://www.telerik.com/kendo-ui)
* Copyright 2015 Telerik AD. All rights reserved.
*
* Kendo UI commercial licenses may be obtained at
* http://www.telerik.com/purchase/license-agreement/kendo-ui-complete
* If you do not own a commercial license, this file shall be governed by the trial license terms.
*/
(function(f, define){
    define([], f);
})(function(){

(function( window, undefined ) {
    var kendo = window.kendo || (window.kendo = { cultures: {} });
    kendo.cultures["haw-US"] = {
        name: "haw-US",
        numberFormat: {
            pattern: ["-n"],
            decimals: 2,
            ",": ",",
            ".": ".",
            groupSize: [3],
            percent: {
                pattern: ["-n %","n %"],
                decimals: 2,
                ",": ",",
                ".": ".",
                groupSize: [3],
                symbol: "%"
            },
            currency: {
                pattern: ["($n)","$n"],
                decimals: 2,
                ",": ",",
                ".": ".",
                groupSize: [3],
                symbol: "$"
            }
        },
        calendars: {
            standard: {
                days: {
                    names: ["Lāpule","Pōʻakahi","Poʻalua","Poʻakolu","Poʻahā","Poʻalima","Poʻaono"],
                    namesAbbr: ["Lp","P1","P2","P3","P4","P5","P6"],
                    namesShort: ["Lp","P1","P2","P3","P4","P5","P6"]
                },
                months: {
                    names: ["Ianuali","Pepeluali","Malaki","ʻApelila","Mei","Iune","Iulai","ʻAukake","Kepakemapa","ʻOkakopa","Nowemapa","Kekemapa"],
                    namesAbbr: ["Ian","Pep","Mal","ʻAp","Mei","Iun","Iul","ʻAuk","Kep","ʻOk","Now","Kek"]
                },
                AM: ["AM","am","AM"],
                PM: ["PM","pm","PM"],
                patterns: {
                    d: "M/d/yyyy",
                    D: "dddd, MMMM dd, yyyy",
                    F: "dddd, MMMM dd, yyyy h:mm:ss tt",
                    g: "M/d/yyyy h:mm tt",
                    G: "M/d/yyyy h:mm:ss tt",
                    m: "d MMMM",
                    M: "d MMMM",
                    s: "yyyy'-'MM'-'dd'T'HH':'mm':'ss",
                    t: "h:mm tt",
                    T: "h:mm:ss tt",
                    u: "yyyy'-'MM'-'dd HH':'mm':'ss'Z'",
                    y: "MMMM, yyyy",
                    Y: "MMMM, yyyy"
                },
                "/": "/",
                ":": ":",
                firstDay: 0
            }
        }
    }
})(this);


return window.kendo;

}, typeof define == 'function' && define.amd ? define : function(_, f){ f(); });