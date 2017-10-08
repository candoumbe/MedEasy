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
    kendo.cultures["yo"] = {
        name: "yo",
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
                pattern: ["-$ n","$ n"],
                decimals: 2,
                ",": ",",
                ".": ".",
                groupSize: [3],
                symbol: "₦"
            }
        },
        calendars: {
            standard: {
                days: {
                    names: ["Àìkú","Ajé","Ìṣẹ́gun","Ọjọ́\u0027rú","Ọjọ́\u0027bọ̀","Ẹtì","Àbámẹ́ta"],
                    namesAbbr: ["Àìk","Ajé","Ìṣg","Ọjr","Ọjb","Ẹti","Àbá"],
                    namesShort: ["Àì","Aj","Ìṣ","Ọj","Ọb","Ẹt","Àb"]
                },
                months: {
                    names: ["Oṣu Muharram","Oṣu Safar","Oṣu R Awwal","Oṣu R Aakhir","Oṣu J Awwal","Oṣu J Aakhira","Oṣu Rajab","Oṣu Sha\u0027baan","Oṣu Ramadhan","Oṣu Shawwal","Oṣu Dhul Qa\u0027dah","Oṣu Dhul Hijjah"],
                    namesAbbr: ["Oṣu Muharram","Oṣu Safar","Oṣu R Awwal","Oṣu R Aakhir","Oṣu J Awwal","Oṣu J Aakhira","Oṣu Rajab","Oṣu Sha\u0027baan","Oṣu Ramadhan","Oṣu Shawwal","Oṣu Dhul Qa\u0027dah","Oṣu Dhul Hijjah"]
                },
                AM: ["Òwúrọ́","òwúrọ́","ÒWÚRỌ́"],
                PM: ["Alẹ̀","alẹ̀","ALẸ̀"],
                patterns: {
                    d: "d/M/yyyy",
                    D: "dddd, dd MMMM, yyyy",
                    F: "dddd, dd MMMM, yyyy h:mm:ss tt",
                    g: "d/M/yyyy h:mm tt",
                    G: "d/M/yyyy h:mm:ss tt",
                    m: "dd MMMM",
                    M: "dd MMMM",
                    s: "yyyy'-'MM'-'dd'T'HH':'mm':'ss",
                    t: "h:mm tt",
                    T: "h:mm:ss tt",
                    u: "yyyy'-'MM'-'dd HH':'mm':'ss'Z'",
                    y: "MMMM,yyyy",
                    Y: "MMMM,yyyy"
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