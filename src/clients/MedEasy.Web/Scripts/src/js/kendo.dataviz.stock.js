/*
* Kendo UI v2015.1.408 (http://www.telerik.com/kendo-ui)
* Copyright 2015 Telerik AD. All rights reserved.
*
* Kendo UI commercial licenses may be obtained at
* http://www.telerik.com/purchase/license-agreement/kendo-ui-complete
* If you do not own a commercial license, this file shall be governed by the trial license terms.
*/
(function(f, define){
    define([ "./kendo.dataviz.chart" ], f);
})(function(){

(function ($, undefined) {
    // Imports ================================================================
    var kendo = window.kendo,
        Class = kendo.Class,
        Observable = kendo.Observable,
        deepExtend = kendo.deepExtend,
        math = Math,
        proxy = $.proxy,

        util = kendo.util,
        last = util.last,
        renderTemplate = util.renderTemplate,

        dataviz = kendo.dataviz,
        defined = util.defined,
        filterSeriesByType = dataviz.filterSeriesByType,
        template = kendo.template,
        Chart = dataviz.ui.Chart,
        Selection = dataviz.Selection,
        addDuration = dataviz.addDuration,
        limitValue = util.limitValue,
        lteDateIndex = dataviz.lteDateIndex,
        toDate = dataviz.toDate,
        toTime = dataviz.toTime;

    // Constants =============================================================
    var AUTO_CATEGORY_WIDTH = 28,
        CHANGE = "change",
        CSS_PREFIX = "k-",
        DRAG = "drag",
        DRAG_END = "dragEnd",
        NAVIGATOR_PANE = "_navigator",
        NAVIGATOR_AXIS = NAVIGATOR_PANE,
        EQUALLY_SPACED_SERIES = dataviz.EQUALLY_SPACED_SERIES,
        ZOOM_ACCELERATION = 3,
        ZOOM = "zoom",
        ZOOM_END = "zoomEnd";

    // Stock chart ===========================================================
    var StockChart = Chart.extend({
        init: function(element, userOptions) {
            $(element).addClass(CSS_PREFIX + "chart");
            Chart.fn.init.call(this, element, userOptions);
        },

        _applyDefaults: function(options, themeOptions) {
            var chart = this,
                width = chart.element.width() || dataviz.DEFAULT_WIDTH;

            var stockDefaults = {
                seriesDefaults: {
                    categoryField: options.dateField
                },
                axisDefaults: {
                    categoryAxis: {
                        name: "default",
                        majorGridLines: {
                            visible: false
                        },
                        labels: {
                            step: 2
                        },
                        majorTicks: {
                            visible: false
                        },
                        maxDateGroups: math.floor(width / AUTO_CATEGORY_WIDTH)
                    }
                }
            };

            if (themeOptions) {
                themeOptions = deepExtend({}, themeOptions, stockDefaults);
            }

            if (!chart._navigator) {
                Navigator.setup(options, themeOptions);
            }

            Chart.fn._applyDefaults.call(chart, options, themeOptions);
        },

        _initDataSource: function(userOptions) {
            var options = userOptions || {},
                dataSource = options.dataSource,
                hasServerFiltering = dataSource && dataSource.serverFiltering,
                mainAxis = [].concat(options.categoryAxis)[0],
                naviOptions = options.navigator || {},
                select = naviOptions.select,
                hasSelect = select && select.from && select.to,
                filter,
                dummyAxis;

            if (hasServerFiltering && hasSelect) {
                filter = [].concat(dataSource.filter || []);

                dummyAxis = new dataviz.DateCategoryAxis(deepExtend({
                    baseUnit: "fit"
                }, mainAxis, {
                    categories: [select.from, select.to]
                }));

                dataSource.filter =
                    Navigator.buildFilter(dummyAxis.range().min, select.to)
                    .concat(filter);
            }

            Chart.fn._initDataSource.call(this, userOptions);
        },

        options: {
            name: "StockChart",
            dateField: "date",
            axisDefaults: {
                categoryAxis: {
                    type: "date",
                    baseUnit: "fit",
                    justified: true
                },
                valueAxis: {
                    narrowRange: true,
                    labels: {
                        format: "C"
                    }
                }
            },
            navigator: {
                select: {},
                seriesDefaults: {
                    markers: {
                        visible: false
                    },
                    tooltip: {
                        visible: true,
                        template: "#= kendo.toString(category, 'd') #"
                    },
                    line: {
                        width: 2
                    }
                },
                hint: {},
                visible: true
            },
            tooltip: {
                visible: true
            },
            legend: {
                visible: false
            }
        },

        _resize: function() {
            var t = this.options.transitions;

            this.options.transitions = false;
            this._fullRedraw();
            this.options.transitions = t;
        },

        _redraw: function() {
            var chart = this,
                navigator = chart._navigator;

            if (!this._dirty() && navigator && navigator.dataSource) {
                navigator.redrawSlaves();
            } else {
                chart._fullRedraw();
            }
        },

        _dirty: function() {
            var options = this.options;
            var series = [].concat(options.series, options.navigator.series);
            var seriesCount = $.grep(series, function(s) { return s && s.visible; }).length;
            var dirty = this._seriesCount !== seriesCount;
            this._seriesCount = seriesCount;

            return dirty;
        },

        _fullRedraw: function() {
            var chart = this,
                navigator = chart._navigator;

            if (!navigator) {
                navigator = chart._navigator = new Navigator(chart);
            }

            navigator._setRange();
            Chart.fn._redraw.call(chart);
            navigator._initSelection();
        },

        _onDataChanged: function() {
            var chart = this;

            Chart.fn._onDataChanged.call(chart);
            chart._dataBound = true;
        },

        _bindCategoryAxis: function(axis, data, axisIx) {
            var chart = this,
                categoryAxes = chart.options.categoryAxis,
                axesLength = categoryAxes.length,
                currentAxis;

            Chart.fn._bindCategoryAxis.apply(this, arguments);

            if (axis.name === NAVIGATOR_AXIS) {
                while (axisIx < axesLength) {
                    currentAxis = categoryAxes[axisIx++];
                    if (currentAxis.pane == NAVIGATOR_PANE) {
                        currentAxis.categories = axis.categories;
                    }
                }
            }
        },

        _trackSharedTooltip: function(coords) {
            var chart = this,
                plotArea = chart._plotArea,
                pane = plotArea.paneByPoint(coords);

            if (pane && pane.options.name === NAVIGATOR_PANE) {
                chart._unsetActivePoint();
            } else {
                Chart.fn._trackSharedTooltip.call(chart, coords);
            }
        },

        destroy: function() {
            var chart = this;

            chart._navigator.destroy();

            Chart.fn.destroy.call(chart);
        }
    });

    var Navigator = Observable.extend({
        init: function(chart) {
            var navi = this;

            navi.chart = chart;
            navi.options = deepExtend({}, navi.options, chart.options.navigator);

            navi._initDataSource();

            if (!defined(navi.options.hint.visible)) {
                navi.options.hint.visible = navi.options.visible;
            }

            chart.bind(DRAG, proxy(navi._drag, navi));
            chart.bind(DRAG_END, proxy(navi._dragEnd, navi));
            chart.bind(ZOOM, proxy(navi._zoom, navi));
            chart.bind(ZOOM_END, proxy(navi._zoomEnd, navi));
        },

        options: { },

        _initDataSource: function() {
            var navi = this,
                options = navi.options,
                autoBind = options.autoBind,
                dsOptions = options.dataSource;

            if (!defined(autoBind)) {
               autoBind = navi.chart.options.autoBind;
            }

            navi._dataChangedHandler = proxy(navi._onDataChanged, navi);

            if (dsOptions) {
                navi.dataSource = kendo.data.DataSource
                    .create(dsOptions)
                    .bind(CHANGE, navi._dataChangedHandler);

                if (autoBind) {
                    navi.dataSource.fetch();
                }
            }
        },

        _onDataChanged: function() {
            var navi = this,
                chart = navi.chart,
                series = chart.options.series,
                seriesIx,
                seriesLength = series.length,
                categoryAxes = chart.options.categoryAxis,
                axisIx,
                axesLength = categoryAxes.length,
                data = navi.dataSource.view(),
                currentSeries,
                currentAxis,
                naviCategories;

            for (seriesIx = 0; seriesIx < seriesLength; seriesIx++) {
                currentSeries = series[seriesIx];

                if (currentSeries.axis == NAVIGATOR_AXIS && chart._isBindable(currentSeries)) {
                    currentSeries.data = data;
                }
            }

            for (axisIx = 0; axisIx < axesLength; axisIx++) {
                currentAxis = categoryAxes[axisIx];

                if (currentAxis.pane == NAVIGATOR_PANE) {
                    if (currentAxis.name == NAVIGATOR_AXIS) {
                        chart._bindCategoryAxis(currentAxis, data, axisIx);
                        naviCategories = currentAxis.categories;
                    } else {
                        currentAxis.categories = naviCategories;
                    }
                }
            }

            if (chart._model) {
                navi.redraw();
                navi.filterAxes();

                if (!chart.options.dataSource || (chart.options.dataSource && chart._dataBound)) {
                    navi.redrawSlaves();
                }
            }
        },

        destroy: function() {
            var navi = this,
                dataSource = navi.dataSource;

            if (dataSource) {
                dataSource.unbind(CHANGE, navi._dataChangeHandler);
            }

            if (navi.selection) {
                navi.selection.destroy();
            }
        },

        redraw: function() {
            this._redrawSelf();
            this._initSelection();
        },

        _initSelection: function() {
            var navi = this,
                chart = navi.chart,
                options = navi.options,
                axis = navi.mainAxis(),
                axisClone = clone(axis),
                range = axis.range(),
                min = range.min,
                max = range.max,
                groups = axis.options.categories,
                select = navi.options.select,
                selection = navi.selection,
                from = toDate(select.from),
                to = toDate(select.to);

            if (groups.length === 0) {
                return;
            }

            if (selection) {
                selection.destroy();
                selection.wrapper.remove();
            }

            // "Freeze" the selection axis position until the next redraw
            axisClone.box = axis.box;

            selection = navi.selection = new Selection(chart, axisClone, {
                min: min,
                max: max,
                from: from,
                to: to,
                selectStart: $.proxy(navi._selectStart, navi),
                select: $.proxy(navi._select, navi),
                selectEnd: $.proxy(navi._selectEnd, navi),
                mousewheel: {
                    zoom: "left"
                }
            });

            if (options.hint.visible) {
                navi.hint = new NavigatorHint(chart.element, {
                    min: min,
                    max: max,
                    template: options.hint.template,
                    format: options.hint.format
                });
            }
        },

        _setRange: function() {
            var plotArea = this.chart._createPlotArea(true);
            var axis = plotArea.namedCategoryAxes[NAVIGATOR_AXIS];
            var axisOpt = axis.options;

            var range = axis.range();
            var min = range.min;
            var max = addDuration(range.max, axisOpt.baseUnitStep, axisOpt.baseUnit);

            var select = this.options.select || {};
            var from = toDate(select.from) || min;
            if (from < min) {
                from = min;
            }

            var to = toDate(select.to) || max;
            if (to > max) {
                to = max;
            }

            this.options.select = {
                from: from,
                to: to
            };

            this.filterAxes();
        },

        _redrawSelf: function(silent) {
            var plotArea = this.chart._plotArea;

            if (plotArea) {
                plotArea.redraw(last(plotArea.panes), silent);
            }
        },

        redrawSlaves: function() {
            var navi = this,
                chart = navi.chart,
                plotArea = chart._plotArea,
                slavePanes = plotArea.panes.slice(0, -1);

            // Update the original series before partial refresh.
            plotArea.srcSeries = chart.options.series;

            plotArea.redraw(slavePanes);
        },

        _drag: function(e) {
            var navi = this,
                chart = navi.chart,
                coords = chart._eventCoordinates(e.originalEvent),
                navigatorAxis = navi.mainAxis(),
                naviRange = navigatorAxis.range(),
                inNavigator = navigatorAxis.pane.box.containsPoint(coords),
                axis = chart._plotArea.categoryAxis,
                range = e.axisRanges[axis.options.name],
                select = navi.options.select,
                selection = navi.selection,
                duration,
                from,
                to;

            if (!range || inNavigator || !selection) {
                return;
            }

            if (select.from && select.to) {
                duration = toTime(select.to) - toTime(select.from);
            } else {
                duration = toTime(selection.options.to) - toTime(selection.options.from);
            }

            from = toDate(limitValue(
                toTime(range.min),
                naviRange.min, toTime(naviRange.max) - duration
            ));

            to = toDate(limitValue(
                toTime(from) + duration,
                toTime(naviRange.min) + duration, naviRange.max
            ));

            navi.options.select = { from: from, to: to };

            if (navi._liveDrag()) {
                navi.filterAxes();
                navi.redrawSlaves();
            }

            selection.set(
                from,
                to
            );

            navi.showHint(from, to);
        },

        _dragEnd: function() {
            var navi = this;

            navi.filterAxes();
            navi.filterDataSource();
            navi.redrawSlaves();

            if (navi.hint) {
                navi.hint.hide();
            }
        },

        _liveDrag: function() {
            var support = kendo.support,
                isTouch = support.touch,
                browser = support.browser,
                isFirefox = browser.mozilla,
                isOldIE = browser.msie && browser.version < 9;

            return !isTouch && !isFirefox && !isOldIE;
        },

        readSelection: function() {
            var navi = this,
                selection = navi.selection,
                src = selection.options,
                dst = navi.options.select;

            dst.from = src.from;
            dst.to = src.to;
        },

        filterAxes: function() {
            var navi = this,
                select = navi.options.select || {},
                chart = navi.chart,
                allAxes = chart.options.categoryAxis,
                from = select.from,
                to = select.to,
                i,
                axis;

            for (i = 0; i < allAxes.length; i++) {
                axis = allAxes[i];
                if (axis.pane !== NAVIGATOR_PANE) {
                    axis.min = toDate(from);
                    axis.max = toDate(to);
                }
            }
        },

        filterDataSource: function() {
            var navi = this,
                select = navi.options.select || {},
                chart = navi.chart,
                chartDataSource = chart.dataSource,
                hasServerFiltering = chartDataSource && chartDataSource.options.serverFiltering,
                axisOptions;

            if (navi.dataSource && hasServerFiltering) {
                axisOptions = new dataviz.DateCategoryAxis(deepExtend({
                    baseUnit: "fit"
                }, chart.options.categoryAxis[0], {
                    categories: [select.from, select.to]
                })).options;

                chartDataSource.filter(
                    Navigator.buildFilter(
                        addDuration(axisOptions.min, -axisOptions.baseUnitStep, axisOptions.baseUnit),
                        addDuration(axisOptions.max, axisOptions.baseUnitStep, axisOptions.baseUnit)
                    )
                );
            }
        },

        _zoom: function(e) {
            var navi = this,
                chart = navi.chart,
                delta = e.delta,
                axis = chart._plotArea.categoryAxis,
                select = navi.options.select,
                selection = navi.selection,
                categories = navi.mainAxis().options.categories,
                fromIx,
                toIx;

            if (!selection) {
                return;
            }

            fromIx = lteDateIndex(selection.options.from, categories);
            toIx = lteDateIndex(selection.options.to, categories);

            e.originalEvent.preventDefault();

            if (math.abs(delta) > 1) {
                delta *= ZOOM_ACCELERATION;
            }

            if (toIx - fromIx > 1) {
                selection.expand(delta);
                navi.readSelection();
            } else {
                axis.options.min = select.from;
                select.from = axis.scaleRange(-e.delta).min;
            }

            if (!kendo.support.touch) {
                navi.filterAxes();
                navi.redrawSlaves();
            }

            selection.set(select.from, select.to);

            navi.showHint(navi.options.select.from, navi.options.select.to);
        },

        _zoomEnd: function(e) {
            this._dragEnd(e);
        },

        showHint: function(from, to) {
            var navi = this,
                chart = navi.chart,
                plotArea = chart._plotArea;

            if (navi.hint) {
                navi.hint.show(
                    from,
                    to,
                    plotArea.backgroundBox()
                );
            }
        },

        _selectStart: function(e) {
            var chart = this.chart;
            chart._selectStart.call(chart, e);
        },

        _select: function(e) {
            var navi = this,
                chart = navi.chart;

            navi.showHint(e.from, e.to);

            chart._select.call(chart, e);
        },

        _selectEnd: function(e) {
            var navi = this,
                chart = navi.chart;

            if (navi.hint) {
                navi.hint.hide();
            }

            navi.readSelection();
            navi.filterAxes();
            navi.filterDataSource();
            navi.redrawSlaves();

            chart._selectEnd.call(chart, e);
        },

        mainAxis: function() {
            var plotArea = this.chart._plotArea;

            if (plotArea) {
                return plotArea.namedCategoryAxes[NAVIGATOR_AXIS];
            }
        }
    });

    Navigator.setup = function(options, themeOptions) {
        options = options || {};
        themeOptions = themeOptions || {};

        var naviOptions = deepExtend({}, themeOptions.navigator, options.navigator),
            panes = options.panes = [].concat(options.panes),
            paneOptions = deepExtend({}, naviOptions.pane, { name: NAVIGATOR_PANE });

        if (!naviOptions.visible) {
            paneOptions.visible = false;
            paneOptions.height = 0.1;
        }

        panes.push(paneOptions);

        Navigator.attachAxes(options, naviOptions);
        Navigator.attachSeries(options, naviOptions, themeOptions);
    };

    Navigator.attachAxes = function(options, naviOptions) {
        var categoryAxes,
            valueAxes,
            series = naviOptions.series || [];

        categoryAxes = options.categoryAxis = [].concat(options.categoryAxis);
        valueAxes = options.valueAxis = [].concat(options.valueAxis);

        var equallySpacedSeries = filterSeriesByType(series, EQUALLY_SPACED_SERIES);
        var justifyAxis = equallySpacedSeries.length === 0;

        var base = deepExtend({
            type: "date",
            pane: NAVIGATOR_PANE,
            roundToBaseUnit: !justifyAxis,
            justified: justifyAxis,
            _collapse: false,
            tooltip: { visible: false },
            labels: { step: 1 },
            autoBind: !naviOptions.dataSource,
            autoBaseUnitSteps: {
                minutes: [1],
                hours: [1, 2],
                days: [1, 2],
                weeks: [],
                months: [1],
                years: [1]
            },
            _overlap: false
        });
        var user = naviOptions.categoryAxis;

        categoryAxes.push(
            deepExtend({}, base, {
                    maxDateGroups: 200
                }, user, {
                name: NAVIGATOR_AXIS,
                baseUnit: "fit",
                baseUnitStep: "auto",
                labels: { visible: false },
                majorTicks: { visible: false }
            }), deepExtend({}, base, user, {
                name: NAVIGATOR_AXIS + "_labels",
                maxDateGroups: 20,
                baseUnitStep: "auto",
                autoBaseUnitSteps: {
                    minutes: []
                },
                majorTicks: { visible: true }
            }), deepExtend({}, base, user, {
                name: NAVIGATOR_AXIS + "_ticks",
                maxDateGroups: 200,
                majorTicks: {
                    visible: true,
                    width: 0.5
                },
                labels: { visible: false, mirror: true }
            })
        );

        valueAxes.push(deepExtend({
            name: NAVIGATOR_AXIS,
            pane: NAVIGATOR_PANE,
            majorGridLines: {
                visible: false
            },
            visible: false
        }, naviOptions.valueAxis));
    };

    Navigator.attachSeries = function(options, naviOptions, themeOptions) {
        var series = options.series = options.series || [],
            navigatorSeries = [].concat(naviOptions.series || []),
            seriesColors = themeOptions.seriesColors,
            defaults = naviOptions.seriesDefaults,
            i;

        for (i = 0; i < navigatorSeries.length; i++) {
            series.push(
                deepExtend({
                    color: seriesColors[i % seriesColors.length],
                    categoryField: naviOptions.dateField,
                    visibleInLegend: false,
                    tooltip: {
                        visible: false
                    }
                }, defaults, navigatorSeries[i], {
                    axis: NAVIGATOR_AXIS,
                    categoryAxis: NAVIGATOR_AXIS,
                    autoBind: !naviOptions.dataSource
                })
            );
        }
    };

    Navigator.buildFilter = function(from, to) {
        return [{
            field: "Date", operator: "gte", value: toDate(from)
        }, {
            field: "Date", operator: "lt", value: toDate(to)
        }];
    };

    var NavigatorHint = Class.extend({
        init: function(container, options) {
            var hint = this;

            hint.options = deepExtend({}, hint.options, options);

            hint.container = container;
            hint.chartPadding = {
                top: parseInt(container.css("paddingTop"), 10),
                left: parseInt(container.css("paddingLeft"), 10)
            };

            hint.template = hint.template;
            if (!hint.template) {
                hint.template = hint.template = renderTemplate(
                    "<div class='" + CSS_PREFIX + "navigator-hint' " +
                    "style='display: none; position: absolute; top: 1px; left: 1px;'>" +
                        "<div class='" + CSS_PREFIX + "tooltip " + CSS_PREFIX + "chart-tooltip'>&nbsp;</div>" +
                        "<div class='" + CSS_PREFIX + "scroll' />" +
                    "</div>"
                );
            }

            hint.element = $(hint.template()).appendTo(container);
        },

        options: {
            format: "{0:d} - {1:d}",
            hideDelay: 500
        },

        show: function(from, to, bbox) {
            var hint = this,
                middle = toDate(toTime(from) + toTime(to - from) / 2),
                options = hint.options,
                text = kendo.format(hint.options.format, from, to),
                tooltip = hint.element.find("." + CSS_PREFIX + "tooltip"),
                scroll = hint.element.find("." + CSS_PREFIX + "scroll"),
                scrollWidth = bbox.width() * 0.4,
                minPos = bbox.center().x - scrollWidth,
                maxPos = bbox.center().x,
                posRange = maxPos - minPos,
                range = options.max - options.min,
                scale = posRange / range,
                offset = middle - options.min,
                hintTemplate;

            if (hint._hideTimeout) {
                clearTimeout(hint._hideTimeout);
            }

            if (!hint._visible) {
                hint.element
                    .stop(false, true)
                    .css("visibility", "hidden")
                    .show();
                hint._visible = true;
            }

            if (options.template) {
                hintTemplate = template(options.template);
                text = hintTemplate({
                    from: from,
                    to: to
                });
            }

            tooltip
                .html(text)
                .css({
                    left: bbox.center().x - tooltip.outerWidth() / 2,
                    top: bbox.y1
                });

            scroll
                .css({
                    width: scrollWidth,
                    left: minPos + offset * scale,
                    top: bbox.y1 +
                         parseInt(tooltip.css("margin-top"), 10) +
                         parseInt(tooltip.css("border-top-width"), 10) +
                         tooltip.height() / 2
                });

            hint.element.css("visibility", "visible");
        },

        hide: function() {
            var hint = this;

            if (hint._hideTimeout) {
                clearTimeout(hint._hideTimeout);
            }

            hint._hideTimeout = setTimeout(function() {
                hint._visible = false;
                hint.element.fadeOut("slow");
            }, hint.options.hideDelay);
        }
    });

    function ClonedObject() { }
    function clone(obj) {
        ClonedObject.prototype = obj;
        return new ClonedObject();
    }

    // Exports ================================================================

    dataviz.ui.plugin(StockChart);

    deepExtend(dataviz, {
        Navigator: Navigator
    });

})(window.kendo.jQuery);

return window.kendo;

}, typeof define == 'function' && define.amd ? define : function(_, f){ f(); });