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
    kendo.cultures["kl-GL"] = {
        name: "kl-GL",
        numberFormat: {
            pattern: ["-n"],
            decimals: 2,
            ",": ".",
            ".": ",",
            groupSize: [3,0],
            percent: {
                pattern: ["-n %","n %"],
                decimals: 2,
                ",": ".",
                ".": ",",
                groupSize: [3,0],
                symbol: "%"
            },
            currency: {
                pattern: ["$ -n","$ n"],
                decimals: 2,
                ",": ".",
                ".": ",",
                groupSize: [3,0],
                symbol: "kr."
            }
        },
        calendars: {
            standard: {
                days: {
                    names: ["sapaat","ataasinngorneq","marlunngorneq","pingasunngorneq","sisamanngorneq","tallimanngorneq","arfininngorneq"],
                    namesAbbr: ["sap.","at.","marl.","ping.","sis.","tall.","arf."],
                    namesShort: ["sa","at","ma","pi","si","ta","ar"]
                },
                months: {
                    names: ["januaari","februaari","marsi","apriili","maaji","juuni","juuli","aggusti","septembari","oktobari","novembari","decembari"],
                    namesAbbr: ["jan","feb","mar","apr","mai","jun","jul","aug","sep","okt","nov","dec"]
                },
                AM: [""],
                PM: [""],
                patterns: {
                    d: "dd-MM-yyyy",
                    D: "MMMM d'.-at, 'yyyy",
                    F: "MMMM d'.-at, 'yyyy HH:mm:ss",
                    g: "dd-MM-yyyy HH:mm",
                    G: "dd-MM-yyyy HH:mm:ss",
                    m: "MMMM d'.-at'",
                    M: "MMMM d'.-at'",
                    s: "yyyy'-'MM'-'dd'T'HH':'mm':'ss",
                    t: "HH:mm",
                    T: "HH:mm:ss",
                    u: "yyyy'-'MM'-'dd HH':'mm':'ss'Z'",
                    y: "MMMM yyyy",
                    Y: "MMMM yyyy"
                },
                "/": "-",
                ":": ":",
                firstDay: 1
            }
        }
    }
})(this);


return window.kendo;

}, typeof define == 'function' && define.amd ? define : function(_, f){ f(); });