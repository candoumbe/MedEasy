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
    kendo.cultures["ts"] = {
        name: "ts",
        numberFormat: {
            pattern: ["-n"],
            decimals: 0,
            ",": " ",
            ".": ",",
            groupSize: [3],
            percent: {
                pattern: ["-n%","%n"],
                decimals: 0,
                ",": " ",
                ".": ",",
                groupSize: [3],
                symbol: "%"
            },
            currency: {
                pattern: ["-$n","$n"],
                decimals: 2,
                ",": " ",
                ".": ",",
                groupSize: [3],
                symbol: "R"
            }
        },
        calendars: {
            standard: {
                days: {
                    names: ["Sonto","Musumbhunuku","Ravumbirhi","Ravunharhu","Ravumune","Ravuntlhanu","Mugqivela"],
                    namesAbbr: ["Son","Mus","Bir","Har","Ne","Tlh","Mug"],
                    namesShort: ["Son","Mus","Bir","Har","Ne","Tlh","Mug"]
                },
                months: {
                    names: ["Sunguti","Nyenyenyani","Nyenyankulu","Dzivamisoko","Mudyaxihi","Khotavuxika","Mawuwani","Mhawuri","Ndzhati","Nhlangula","Hukuri","N\u0027wendzamhala"],
                    namesAbbr: ["Sun","Yan","Kul","Dzi","Mud","Kho","Maw","Mha","Ndz","Nhl","Huk","N\u0027w"]
                },
                AM: ["AM","am","AM"],
                PM: ["PM","pm","PM"],
                patterns: {
                    d: "yyyy-MM-dd",
                    D: "yyyy MMMM d, dddd",
                    F: "yyyy MMMM d, dddd HH:mm:ss",
                    g: "yyyy-MM-dd HH:mm",
                    G: "yyyy-MM-dd HH:mm:ss",
                    m: "MMMM d",
                    M: "MMMM d",
                    s: "yyyy'-'MM'-'dd'T'HH':'mm':'ss",
                    t: "HH:mm",
                    T: "HH:mm:ss",
                    u: "yyyy'-'MM'-'dd HH':'mm':'ss'Z'",
                    y: "yyyy MMMM",
                    Y: "yyyy MMMM"
                },
                "/": "-",
                ":": ":",
                firstDay: 0
            }
        }
    }
})(this);


return window.kendo;

}, typeof define == 'function' && define.amd ? define : function(_, f){ f(); });