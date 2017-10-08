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
    kendo.cultures["sk"] = {
        name: "sk",
        numberFormat: {
            pattern: ["-n"],
            decimals: 2,
            ",": " ",
            ".": ",",
            groupSize: [3],
            percent: {
                pattern: ["-n %","n %"],
                decimals: 2,
                ",": " ",
                ".": ",",
                groupSize: [3],
                symbol: "%"
            },
            currency: {
                pattern: ["-n $","n $"],
                decimals: 2,
                ",": " ",
                ".": ",",
                groupSize: [3],
                symbol: "EUR"
            }
        },
        calendars: {
            standard: {
                days: {
                    names: ["nedeľa","pondelok","utorok","streda","štvrtok","piatok","sobota"],
                    namesAbbr: ["ne","po","ut","st","št","pi","so"],
                    namesShort: ["ne","po","ut","st","št","pi","so"]
                },
                months: {
                    names: ["január","február","marec","apríl","máj","jún","júl","august","september","október","november","december"],
                    namesAbbr: ["1","2","3","4","5","6","7","8","9","10","11","12"]
                },
                AM: ["dop.","dop.","DOP."],
                PM: ["odp.","odp.","ODP."],
                patterns: {
                    d: "d.M.yyyy",
                    D: "d. MMMM yyyy",
                    F: "d. MMMM yyyy H:mm:ss",
                    g: "d.M.yyyy H:mm",
                    G: "d.M.yyyy H:mm:ss",
                    m: "d. MMMM",
                    M: "d. MMMM",
                    s: "yyyy'-'MM'-'dd'T'HH':'mm':'ss",
                    t: "H:mm",
                    T: "H:mm:ss",
                    u: "yyyy'-'MM'-'dd HH':'mm':'ss'Z'",
                    y: "MMMM yyyy",
                    Y: "MMMM yyyy"
                },
                "/": ".",
                ":": ":",
                firstDay: 1
            }
        }
    }
})(this);


return window.kendo;

}, typeof define == 'function' && define.amd ? define : function(_, f){ f(); });