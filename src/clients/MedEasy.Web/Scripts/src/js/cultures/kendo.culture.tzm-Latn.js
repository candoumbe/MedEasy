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
    kendo.cultures["tzm-Latn"] = {
        name: "tzm-Latn",
        numberFormat: {
            pattern: ["-n"],
            decimals: 2,
            ",": " ",
            ".": ",",
            groupSize: [3],
            percent: {
                pattern: ["-n %","n %"],
                decimals: 2,
                ",": " ",
                ".": ",",
                groupSize: [3],
                symbol: "%"
            },
            currency: {
                pattern: ["-n $","n $"],
                decimals: 2,
                ",": " ",
                ".": ",",
                groupSize: [3],
                symbol: "DA"
            }
        },
        calendars: {
            standard: {
                days: {
                    names: ["lh\u0027ed","letnayen","ttlata","larebâa","lexmis","ldjemâa","ssebt"],
                    namesAbbr: ["lh\u0027d","let","ttl","lar","lex","ldj","sse"],
                    namesShort: ["lh","lt","tt","la","lx","ld","ss"]
                },
                months: {
                    names: ["Yennayer","Furar","Meghres","Yebrir","Magu","Yunyu","Yulyu","Ghuct","Cutenber","Tuber","Nunember","Dujanbir"],
                    namesAbbr: ["Yen","Fur","Megh","Yeb","May","Yun","Yul","Ghu","Cut","Tub","Nun","Duj"]
                },
                AM: [""],
                PM: [""],
                patterns: {
                    d: "dd-MM-yyyy",
                    D: "dd MMMM, yyyy",
                    F: "dd MMMM, yyyy H:mm:ss",
                    g: "dd-MM-yyyy H:mm",
                    G: "dd-MM-yyyy H:mm:ss",
                    m: "d MMMM",
                    M: "d MMMM",
                    s: "yyyy'-'MM'-'dd'T'HH':'mm':'ss",
                    t: "H:mm",
                    T: "H:mm:ss",
                    u: "yyyy'-'MM'-'dd HH':'mm':'ss'Z'",
                    y: "MMMM, yyyy",
                    Y: "MMMM, yyyy"
                },
                "/": "-",
                ":": ":",
                firstDay: 6
            }
        }
    }
})(this);


return window.kendo;

}, typeof define == 'function' && define.amd ? define : function(_, f){ f(); });