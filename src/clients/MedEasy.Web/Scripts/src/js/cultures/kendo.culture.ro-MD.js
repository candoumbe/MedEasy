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
    kendo.cultures["ro-MD"] = {
        name: "ro-MD",
        numberFormat: {
            pattern: ["-n"],
            decimals: 0,
            ",": ".",
            ".": ",",
            groupSize: [3],
            percent: {
                pattern: ["-n%","%n"],
                decimals: 0,
                ",": ".",
                ".": ",",
                groupSize: [3],
                symbol: "%"
            },
            currency: {
                pattern: ["-n$","n$"],
                decimals: 2,
                ",": ".",
                ".": ",",
                groupSize: [3],
                symbol: "L"
            }
        },
        calendars: {
            standard: {
                days: {
                    names: ["duminică","luni","marți","miercuri","joi","vineri","sâmbătă"],
                    namesAbbr: ["Du","Lu","Ma","Mi","Jo","Vi","Sâ"],
                    namesShort: ["Du","Lu","Ma","Mi","Jo","Vi","Sâ"]
                },
                months: {
                    names: ["ianuarie","februarie","martie","aprilie","mai","iunie","iulie","august","septembrie","octombrie","noiembrie","decembrie"],
                    namesAbbr: ["ian.","feb.","mar.","apr.","mai","iun.","iul.","aug.","sept.","oct.","nov.","dec."]
                },
                AM: ["AM","am","AM"],
                PM: ["PM","pm","PM"],
                patterns: {
                    d: "dd.MM.yyyy",
                    D: "dddd, d MMMM yyyy",
                    F: "dddd, d MMMM yyyy HH:mm:ss",
                    g: "dd.MM.yyyy HH:mm",
                    G: "dd.MM.yyyy HH:mm:ss",
                    m: "d MMMM",
                    M: "d MMMM",
                    s: "yyyy'-'MM'-'dd'T'HH':'mm':'ss",
                    t: "HH:mm",
                    T: "HH:mm:ss",
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