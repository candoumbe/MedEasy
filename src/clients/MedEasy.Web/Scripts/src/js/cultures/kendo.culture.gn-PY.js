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
    kendo.cultures["gn-PY"] = {
        name: "gn-PY",
        numberFormat: {
            pattern: ["-n"],
            decimals: 2,
            ",": ".",
            ".": ",",
            groupSize: [3],
            percent: {
                pattern: ["-n %","n %"],
                decimals: 2,
                ",": ".",
                ".": ",",
                groupSize: [3],
                symbol: "%"
            },
            currency: {
                pattern: ["-n $","n $"],
                decimals: 0,
                ",": ".",
                ".": ",",
                groupSize: [3],
                symbol: "₲"
            }
        },
        calendars: {
            standard: {
                days: {
                    names: ["arateĩ","arakõi","araapy","ararundy","arapo","arapoteĩ","arapokõi"],
                    namesAbbr: ["teĩ","kõi","apy","ndy","po","oteĩ","okõi"],
                    namesShort: ["A1","A2","A3","A4","A5","A6","A7"]
                },
                months: {
                    names: ["jasyteĩ","jasykõi","jasyapy","jasyrundy","jasypo","jasypoteĩ","jasypokõi","jasypoapy","jasyporundy","jasypa","jasypateĩ","jasypakõi"],
                    namesAbbr: ["jteĩ","jkõi","japy","jrun","jpo","jpot","jpok","jpoa","jpor","jpa","jpat","jpak"]
                },
                AM: ["a.m.","a.m.","A.M."],
                PM: ["p.m.","p.m.","P.M."],
                patterns: {
                    d: "dd/MM/yyyy",
                    D: "dddd, dd MMMM, yyyy",
                    F: "dddd, dd MMMM, yyyy HH:mm:ss",
                    g: "dd/MM/yyyy HH:mm",
                    G: "dd/MM/yyyy HH:mm:ss",
                    m: "dd MMMM",
                    M: "dd MMMM",
                    s: "yyyy'-'MM'-'dd'T'HH':'mm':'ss",
                    t: "HH:mm",
                    T: "HH:mm:ss",
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