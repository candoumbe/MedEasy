/*
* Kendo UI v2015.3.1214 (http://www.telerik.com/kendo-ui)
* Copyright 2015 Telerik AD. All rights reserved.
*
* Kendo UI commercial licenses may be obtained at
* http://www.telerik.com/purchase/license-agreement/kendo-ui-complete
* If you do not own a commercial license, this file shall be governed by the trial license terms.
*/
(function(f, define){
    define([ "./kendo.dom", "./kendo.data", "./kendo.columnsorter", "./kendo.editable", "./kendo.window", "./kendo.filtermenu", "./kendo.selectable", "./kendo.resizable", "./kendo.treeview.draganddrop" ], f);
})(function(){

(function(){



(function($, undefined) {
    var data = kendo.data;
    var extend = $.extend;
    var kendoDom = kendo.dom;
    var kendoDomElement = kendoDom.element;
    var kendoTextElement = kendoDom.text;
    var kendoHtmlElement = kendoDom.html;
    var ui = kendo.ui;
    var DataBoundWidget = ui.DataBoundWidget;
    var DataSource = data.DataSource;
    var ObservableArray = data.ObservableArray;
    var Query = data.Query;
    var Model = data.Model;
    var proxy = $.proxy;
    var map = $.map;
    var grep = $.grep;
    var inArray = $.inArray;
    var isPlainObject = $.isPlainObject;
    var push = Array.prototype.push;
    var STRING = "string";
    var CHANGE = "change";
    var ERROR = "error";
    var PROGRESS = "progress";
    var DOT = ".";
    var NS = ".kendoTreeList";
    var CLICK = "click";
    var MOUSEDOWN = "mousedown";
    var EDIT = "edit";
    var SAVE = "save";
    var EXPAND = "expand";
    var COLLAPSE = "collapse";
    var REMOVE = "remove";
    var DATABINDING = "dataBinding";
    var DATABOUND = "dataBound";
    var CANCEL = "cancel";
    var FILTERMENUINIT = "filterMenuInit";
    var COLUMNHIDE = "columnHide";
    var COLUMNSHOW = "columnShow";
    var HEADERCELLS = "th.k-header";
    var COLUMNREORDER = "columnReorder";
    var COLUMNRESIZE = "columnResize";
    var COLUMNMENUINIT = "columnMenuInit";
    var COLUMNLOCK = "columnLock";
    var COLUMNUNLOCK = "columnUnlock";
    var PARENTIDFIELD = "parentId";
    var DRAGSTART = "dragstart";
    var DRAG = "drag";
    var DROP = "drop";
    var DRAGEND = "dragend";

    var classNames = {
        wrapper: "k-treelist k-grid k-widget",
        header: "k-header",
        button: "k-button",
        alt: "k-alt",
        editCell: "k-edit-cell",
        group: "k-treelist-group",
        gridToolbar: "k-grid-toolbar",
        gridHeader: "k-grid-header",
        gridHeaderWrap: "k-grid-header-wrap",
        gridContent: "k-grid-content",
        gridContentWrap: "k-grid-content",
        gridFilter: "k-grid-filter",
        footerTemplate: "k-footer-template",
        loading: "k-loading",
        refresh: "k-i-refresh",
        retry: "k-request-retry",
        selected: "k-state-selected",
        status: "k-status",
        link: "k-link",
        withIcon: "k-with-icon",
        filterable: "k-filterable",
        icon: "k-icon",
        iconFilter: "k-filter",
        iconCollapse: "k-i-collapse",
        iconExpand: "k-i-expand",
        iconHidden: "k-i-none",
        iconPlaceHolder: "k-icon k-i-none",
        input: "k-input",
        dropPositions: "k-insert-top k-insert-bottom k-add k-insert-middle",
        dropTop: "k-insert-top",
        dropBottom: "k-insert-bottom",
        dropAdd: "k-add",
        dropMiddle: "k-insert-middle",
        dropDenied: "k-denied",
        dragStatus: "k-drag-status",
        dragClue: "k-drag-clue",
        dragClueText: "k-clue-text"
    };

    var defaultCommands = {
        create: {
            imageClass: "k-add",
            className: "k-grid-add",
            methodName: "addRow"
        },
        createchild: {
            imageClass: "k-add",
            className: "k-grid-add",
            methodName: "addRow"
        },
        destroy: {
            imageClass: "k-delete",
            className: "k-grid-delete",
            methodName: "removeRow"
        },
        edit: {
            imageClass: "k-edit",
            className: "k-grid-edit",
            methodName: "editRow"
        },
        update: {
            imageClass: "k-update",
            className: "k-primary k-grid-update",
            methodName: "saveRow"
        },
        canceledit: {
            imageClass: "k-cancel",
            className: "k-grid-cancel",
            methodName: "_cancelEdit"
        },
        excel: {
            imageClass: "k-i-excel",
            className: "k-grid-excel",
            methodName: "saveAsExcel"
        },
        pdf: {
            imageClass: "k-i-pdf",
            className: "k-grid-pdf",
            methodName: "saveAsPDF"
        }
    };

    var TreeListModel = Model.define({
        id: "id",

        parentId: PARENTIDFIELD,

        fields: {
            id: { type: "number" },
            parentId: { type: "number", nullable: true }
        },

        init: function(value) {
            Model.fn.init.call(this, value);

            this._loaded = false;

            if (!this.parentIdField) {
                this.parentIdField = PARENTIDFIELD;
            }

            this.parentId = this.get(this.parentIdField);
        },

        accept: function(data) {
            Model.fn.accept.call(this, data);

            this.parentId = this.get(this.parentIdField);
        },

        set: function(field, value, initiator) {
            if (field == PARENTIDFIELD && this.parentIdField != PARENTIDFIELD) {
                this[this.parentIdField] = value;
            }

            Model.fn.set.call(this, field, value, initiator);

            if (field == this.parentIdField) {
                this.parentId = this.get(this.parentIdField);
            }
        },

        loaded: function(value) {
            if (value !== undefined) {
                this._loaded = value;
            } else {
                return this._loaded;
            }
        },

        shouldSerialize: function(field) {
            return Model.fn.shouldSerialize.call(this, field) && field !== "_loaded" && field != "_error" && field != "_edit" && !(this.parentIdField !== "parentId" && field === "parentId");
        }
    });

    TreeListModel.parentIdField = PARENTIDFIELD;

    TreeListModel.define = function(base, options) {
        if (options === undefined) {
            options = base;
            base = TreeListModel;
        }

        var parentId = options.parentId || PARENTIDFIELD;

        options.parentIdField = parentId;

        var model = Model.define(base, options);

        if (parentId) {
            model.parentIdField = parentId;
        }

        return model;
    };

    function is(field) {
        return function(object) {
            return object[field];
        };
    }

    function not(func) {
        return function(object) {
            return !func(object);
        };
    }

    var TreeListDataSource = DataSource.extend({
        init: function(options) {
            DataSource.fn.init.call(this, extend(true, {}, {
                schema: {
                    modelBase: TreeListModel,
                    model: TreeListModel
                }
            }, options));
        },

        _createNewModel: function(data) {
            var model = {};
            var fromModel = data instanceof Model;

            if (fromModel) {
                model = data;
            }

            model = DataSource.fn._createNewModel.call(this, model);

            if (!fromModel) {
                if (data.parentId) {
                    data[model.parentIdField] = data.parentId;
                }
                model.accept(data);
            }

            return model;
        },

        _shouldWrap: function() {
            return true;
        },

        _readData: function(newData) {
            var data = this.data();
            newData = DataSource.fn._readData.call(this, newData);

            this._concat(newData, data);

            if (newData instanceof ObservableArray) {
                return newData;
            }

            return data;
        },

        _concat: function(source, target) {
            var targetLength = target.length;

            for (var i = 0; i < source.length; i++) {
                target[targetLength++] = source[i];
            }

            target.length = targetLength;
        },

        _readAggregates: function(data) {
            var result = extend(this._aggregateResult, this.reader.aggregates(data));
            if ("" in result) {
                result[this._defaultParentId()] = result[""];
                delete result[""];
            }

            return result;
        },

        remove: function(root) {
            var items = this._subtree(this._childrenMap(this.data()), root.id);

            this._removeItems(items);

            DataSource.fn.remove.call(this, root);
        },

        _filterCallback: function(query) {
            var i, item;
            var map = {};
            var result = [];
            var data = query.toArray();

            for (i = 0; i < data.length; i++) {
                item = data[i];

                while (item) {
                    if (!map[item.id]) {
                        map[item.id] = true;
                        result.push(item);
                    }

                    if (!map[item.parentId]) {
                        map[item.parentId] = true;
                        item = this.parentNode(item);

                        if (item) {
                            result.push(item);
                        }
                    } else {
                        break;
                    }
                }
            }

            return new Query(result);
        },

        _subtree: function(map, id) {
            var result = map[id] || [];
            var defaultParentId = this._defaultParentId();

            for (var i = 0, len = result.length; i < len; i++) {
                if (result[i].id !== defaultParentId) {
                    result = result.concat(this._subtree(map, result[i].id));
                }
            }

            return result;
        },

        // builds hash id -> children
        _childrenMap: function(data) {
            var map = {};
            var i, item, id, parentId;

            data = this._observeView(data);

            for (i = 0; i < data.length; i++) {
                item = data[i];
                id = item.id;
                parentId = item.parentId;

                map[id] = map[id] || [];
                map[parentId] = map[parentId] || [];
                map[parentId].push(item);
            }

            return map;
        },

        _calculateAggregates: function (data, options) {
            options = options || {};

            var result = {};
            var item, subtree, i;
            var filter = options.filter;

            if (filter) {
                data = Query.process(data, {
                    filter: filter,
                    filterCallback: proxy(this._filterCallback, this)
                }).data;
            }

            var map = this._childrenMap(data);

            // calculate aggregates for each subtree
            result[this._defaultParentId()] = new Query(this._subtree(map, this._defaultParentId())).aggregate(options.aggregate);

            for (i = 0; i < data.length; i++) {
                item = data[i];
                subtree = this._subtree(map, item.id);

                result[item.id] = new Query(subtree).aggregate(options.aggregate);
            }

            return result;
        },

        _queryProcess: function(data, options) {
            options = options || {};

            options.filterCallback = proxy(this._filterCallback, this);

            var defaultParentId = this._defaultParentId();
            var result = Query.process(data, options);
            var map = this._childrenMap(result.data);
            var hasLoadedChildren, i, item, children;

            data = map[defaultParentId] || [];

            for (i = 0; i < data.length; i++) {
                item = data[i];

                if (item.id === defaultParentId) {
                    continue;
                }

                children = map[item.id];
                hasLoadedChildren = !!(children && children.length);

                if (!item.loaded()) {
                    item.loaded(hasLoadedChildren || !item.hasChildren);
                }

                if (item.loaded() || item.hasChildren !== true) {
                    item.hasChildren = hasLoadedChildren;
                }

                if (hasLoadedChildren) {
                    //cannot use splice due to IE8 bug
                    data = data.slice(0, i + 1).concat(children, data.slice(i + 1));
                }
            }

            result.data = data;

            return result;
        },

        _queueRequest: function(options, callback) {
            // allow simultaneous requests (loading multiple items at the same time)
            callback.call(this);
        },

        _modelLoaded: function(id) {
            var model = this.get(id);
            model.loaded(true);
            model.hasChildren = this.childNodes(model).length > 0;
        },

        _modelError: function(id, e) {
            this.get(id)._error = e;
        },

        success: function(data, requestParams) {
            if (!requestParams || !requestParams.id) {
                this._data = this._observe([]);
            }

            return DataSource.fn.success.call(this, data, requestParams);
        },

        load: function(model) {
            var method = "_query";
            var remote = this.options.serverSorting || this.options.serverPaging || this.options.serverFiltering || this.options.serverGrouping || this.options.serverAggregates;
            var defaultPromise = $.Deferred().resolve().promise();

            if (model.loaded()) {
                if (remote) {
                    return defaultPromise;
                }
            } else if (model.hasChildren) {
                method = "read";
            }

            return this[method]({ id: model.id }).then(
                proxy(this._modelLoaded, this, model.id),
                proxy(this._modelError, this, model.id)
            );
        },

        contains: function(root, child) {
            var rootId = root.id;

            while (child) {
                if (child.parentId === rootId) {
                    return true;
                }

                child = this.parentNode(child);
            }

            return false;
        },

        _byParentId: function(id, defaultId) {
            var result = [];
            var view = this.view();
            var current;

            if (id === defaultId) {
                return [];
            }

            for (var i = 0; i < view.length; i++) {
                current = view.at(i);

                if (current.parentId == id) {
                    result.push(current);
                }
            }

            return result;
        },

        _defaultParentId: function() {
            return this.reader.model.fn.defaults[this.reader.model.parentIdField];
        },

        childNodes: function(model) {
            return this._byParentId(model.id, this._defaultParentId());
        },

        rootNodes: function() {
            return this._byParentId(this._defaultParentId());
        },

        parentNode: function(model) {
            return this.get(model.parentId);
        },

        level: function(model) {
            var result = -1;

            if (!(model instanceof TreeListModel)) {
                model = this.get(model);
            }

            do {
                model = this.parentNode(model);
                result++;
            } while (model);

            return result;
        },

        filter: function(value) {
            var baseFilter = DataSource.fn.filter;

            if (value === undefined) {
                return baseFilter.call(this, value);
            }

            baseFilter.call(this, value);
        }
    });

    TreeListDataSource.create = function(options) {
        if ($.isArray(options)) {
            options = { data: options };
        } else if (options instanceof ObservableArray) {
            options = { data: options.toJSON() };
        }

        return options instanceof TreeListDataSource ? options : new TreeListDataSource(options);
    };

    function isCellVisible() {
        return this.style.display !== "none";
    }

    function leafDataCells(container) {
        var rows = container.find(">tr:not(.k-filter-row)");

        var filter = function() {
            var el = $(this);
            return !el.hasClass("k-group-cell") && !el.hasClass("k-hierarchy-cell");
        };

        var cells = $();
        if (rows.length > 1) {
            cells = rows.find("th")
                .filter(filter)
                .filter(function() { return this.rowSpan > 1; });
        }

        cells = cells.add(rows.last().find("th").filter(filter));

        var indexAttr = kendo.attr("index");
        cells.sort(function(a, b) {
            a = $(a);
            b = $(b);

            var indexA = a.attr(indexAttr);
            var indexB = b.attr(indexAttr);

            if (indexA === undefined) {
                indexA = $(a).index();
            }
            if (indexB === undefined) {
                indexB = $(b).index();
            }

            indexA = parseInt(indexA, 10);
            indexB = parseInt(indexB, 10);
            return indexA > indexB ? 1 : (indexA < indexB ? -1 : 0);
        });

        return cells;
    }

    function createPlaceholders(options) {
        var spans = [];
        var className = options.className;

        for (var i = 0, level = options.level; i < level; i++) {
            spans.push(kendoDomElement("span", { className: className }));
        }

        return spans;
    }

    function columnsWidth(cols) {
        var colWidth, width = 0;

        for (var idx = 0, length = cols.length; idx < length; idx++) {
            colWidth = cols[idx].style.width;
            if (colWidth && colWidth.indexOf("%") == -1) {
                width += parseInt(colWidth, 10);
            }
        }

        return width;
    }

   function syncTableHeight(table1, table2) {
       table1 = table1[0];
       table2 = table2[0];

       if (table1.rows.length !== table2.rows.length) {
           var lockedHeigth = table1.offsetHeight;
           var tableHeigth = table2.offsetHeight;

           var row;
           var diff;
           if (lockedHeigth > tableHeigth) {
               row = table2.rows[table2.rows.length - 1];

               diff = lockedHeigth - tableHeigth;
           } else {
               row = table1.rows[table1.rows.length - 1];

               diff = tableHeigth - lockedHeigth;
           }
           row.style.height = row.offsetHeight + diff + "px";
       }
   }


    var Editor = kendo.Observable.extend({
        init: function(element, options) {
            kendo.Observable.fn.init.call(this);

            options = this.options = extend(true, {}, this.options, options);

            this.element = element;

            this.bind(this.events, options);

            this.model = this.options.model;

            this.fields = this._fields(this.options.columns);

            this._initContainer();

            this.createEditable();
        },

        events: [],

        _initContainer: function() {
            this.wrapper = this.element;
        },

        createEditable: function() {
            var options = this.options;

            this.editable = new ui.Editable(this.wrapper, {
                fields: this.fields,
                target: options.target,
                clearContainer: options.clearContainer,
                model: this.model
            });
        },

        _isEditable: function(column) {
            return column.field && this.model.editable(column.field);
        },

        _fields: function(columns) {
            var fields = [];
            var idx, length, column;

            for (idx = 0, length = columns.length; idx < length; idx++) {
                column = columns[idx];

                if (this._isEditable(column)) {
                    fields.push({
                        field: column.field,
                        format: column.format,
                        editor: column.editor
                    });
                }
            }

            return fields;
        },

        end: function() {
            return this.editable.end();
        },

        close: function() {
            this.destroy();
        },

        destroy: function() {
            this.editable.destroy();
            this.editable.element
                .find("[" + kendo.attr("container-for") + "]")
                .empty()
                .end()
                .removeAttr(kendo.attr("role"));

            this.model = this.wrapper = this.element = this.columns = this.editable = null;
        }
    });

    var PopupEditor = Editor.extend({
        init: function(element, options) {
            Editor.fn.init.call(this, element, options);

            this._attachHandlers();
            kendo.cycleForm(this.wrapper);

            this.open();
        },

        events: [
            CANCEL,
            SAVE
        ],

        options: {
            window: {
                modal: true,
                resizable: false,
                draggable: true,
                title: "Edit",
                visible: false
            }
        },

        _initContainer: function() {
            var options = this.options;
            var formContent = [];

            this.wrapper = $('<div class="k-popup-edit-form"/>')
                .attr(kendo.attr("uid"), this.model.uid)
                .append('<div class="k-edit-form-container"/>');

            if (options.template) {
                this._appendTemplate(formContent);
                this.fields = [];
            } else {
                this._appendFields(formContent);
            }
            this._appendButtons(formContent);

            new kendoDom.Tree(this.wrapper.children()[0]).render(formContent);

            this.wrapper.appendTo(options.appendTo);

            this.window = new ui.Window(this.wrapper, options.window);
        },

        _appendTemplate: function(form) {
            var template = this.options.template;

            if (typeof template === STRING) {
                template = window.unescape(template);
            }

            template = kendo.template(template)(this.model);

            form.push(kendoHtmlElement(template));
        },

        _appendFields: function(form) {
            var idx, length, column;
            var columns = this.options.columns;

            for (idx = 0, length = columns.length; idx < length; idx++) {
                column = columns[idx];

                if (column.command) {
                    continue;
                }

                form.push(kendoHtmlElement('<div class="k-edit-label"><label for="' + column.field + '">' + (column.title || column.field || "") + '</label></div>'));

                if (this._isEditable(column)) {
                    form.push(kendoHtmlElement('<div ' + kendo.attr("container-for") + '="' + column.field +
                                '" class="k-edit-field"></div>'));
                } else {
                    form.push(kendoDomElement("div", {
                            "class": "k-edit-field"
                        },
                        [ this.options.fieldRenderer(column, this.model) ]));
                }
            }
        },

        _appendButtons: function(form) {
            form.push(kendoDomElement("div", {
                "class": "k-edit-buttons k-state-default"
            }, this.options.commandRenderer()));
        },

        _attachHandlers: function() {
            var closeHandler = this._cancelProxy = proxy(this._cancel, this);
            this.wrapper.on(CLICK + NS, ".k-grid-cancel", this._cancelProxy);

            this._saveProxy = proxy(this._save, this);
            this.wrapper.on(CLICK + NS, ".k-grid-update", this._saveProxy);

            this.window.bind("close", function(e) {
                if (e.userTriggered) {
                    closeHandler(e);
                }
            });
        },

        _dettachHandlers: function() {
            this._cancelProxy = null;
            this._saveProxy = null;
            this.wrapper.off(NS);
        },

        _cancel: function(e) {
            this.trigger(CANCEL, e);
        },

        _save: function() {
            this.trigger(SAVE);
        },

        open: function() {
            this.window.center().open();
        },

        close: function() {
            this.window.bind("deactivate", proxy(this.destroy, this)).close();
        },

        destroy: function() {
            this.window.destroy();
            this.window = null;
            this._dettachHandlers();

            Editor.fn.destroy.call(this);
        }
    });

    var TreeList = DataBoundWidget.extend({
        init: function(element, options) {
            DataBoundWidget.fn.init.call(this, element, options);

            this._dataSource(this.options.dataSource);

            this._columns();
            this._layout();
            this._selectable();
            this._sortable();
            this._resizable();
            this._filterable();
            this._attachEvents();
            this._toolbar();
            this._scrollable();
            this._reorderable();
            this._columnMenu();
            this._minScreenSupport();
            this._draggable();

            if (this.options.autoBind) {
                this.dataSource.fetch();
            }

            if (this._hasLockedColumns) {
                var widget = this;
                this.wrapper.addClass("k-grid-lockedcolumns");
                this._resizeHandler = function()  { widget.resize(); };
                $(window).on("resize" + NS, this._resizeHandler);
            }

            kendo.notify(this);
        },

        _draggable: function() {
            var editable = this.options.editable;

            if (!editable || !editable.move) {
                return;
            }

            this._dragging = new kendo.ui.HierarchicalDragAndDrop(this.wrapper, {
                $angular: this.$angular,
                autoScroll: true,
                filter: "tbody>tr",
                itemSelector: "tr",
                allowedContainers: this.wrapper,
                hintText: function(row) {
                    var text = function() { return $(this).text(); };
                    var separator = "<span class='k-header k-drag-separator' />";
                    return row.children("td").map(text).toArray().join(separator);
                },
                contains: proxy(function(source, destination) {
                    var dest = this.dataItem(destination);
                    var src = this.dataItem(source);

                    return src == dest || this.dataSource.contains(src, dest);
                }, this),
                itemFromTarget: function(target) {
                    var tr = target.closest("tr");
                    return { item: tr, content: tr };
                },
                dragstart: proxy(function(source) {
                    this.wrapper.addClass("k-treelist-dragging");

                    var model = this.dataItem(source);

                    return this.trigger(DRAGSTART, { source: model });
                }, this),
                drag: proxy(function(e) {
                    e.source = this.dataItem(e.source);

                    this.trigger(DRAG, e);
                }, this),
                drop: proxy(function(e) {
                    e.source = this.dataItem(e.source);
                    e.destination = this.dataItem(e.destination);

                    this.wrapper.removeClass("k-treelist-dragging");

                    return this.trigger(DROP, e);
                }, this),
                dragend: proxy(function(e) {
                    var dest = this.dataItem(e.destination);
                    var src = this.dataItem(e.source);

                    src.set("parentId", dest ? dest.id : null);

                    e.source = src;
                    e.destination = dest;

                    this.trigger(DRAGEND, e);
                }, this),
                reorderable: false,
                dropHintContainer: function(item) {
                    return item.children("td:eq(1)"); // expandable column
                },
                dropPositionFrom: function(dropHint) {
                    return dropHint.prevAll(".k-i-none").length > 0 ? "after" : "before";
                }
            });
        },

        itemFor: function(model) {
            if (typeof model == "number") {
                model = this.dataSource.get(model);
            }

            return this.tbody.find("[" + kendo.attr("uid") + "=" + model.uid + "]");
        },

        _scrollable: function() {
            if (this.options.scrollable) {
                var scrollables = this.thead.closest(".k-grid-header-wrap");
                var lockedContent = $(this.lockedContent)
                    .bind("DOMMouseScroll" + NS + " mousewheel" + NS, proxy(this._wheelScroll, this));

                this.content.bind("scroll" + NS, function() {
                    scrollables.scrollLeft(this.scrollLeft);
                    lockedContent.scrollTop(this.scrollTop);
                });


                var touchScroller = kendo.touchScroller(this.content);

                if (touchScroller && touchScroller.movable) {
                    this._touchScroller = touchScroller;

                    touchScroller.movable.bind("change", function(e) {
                        scrollables.scrollLeft(-e.sender.x);
                        if (lockedContent) {
                            lockedContent.scrollTop(-e.sender.y);
                        }
                    });
                }
            }
        },

        _wheelScroll: function (e) {
            if (e.ctrlKey) {
                return;
            }

            var delta = kendo.wheelDeltaY(e);

            if (delta) {
                e.preventDefault();
                //In Firefox DOMMouseScroll event cannot be canceled
                $(e.currentTarget).one("wheel" + NS, false);

                this.content.scrollTop(this.content.scrollTop() + (-delta));
            }
        },

        _progress: function() {
            var messages = this.options.messages;

            if (!this.tbody.find("tr").length) {
                this._showStatus(
                    kendo.template(
                        "<span class='#= className #' /> #: messages.loading #"
                    )({
                        className: classNames.icon + " " + classNames.loading,
                        messages: messages
                    })
                );
            }
        },

        _error: function(e) {
            if (!this.dataSource.rootNodes().length) {
                this._render({ error: e });
            }
        },

        refresh: function(e) {
            e = e || {};

            if (e.action == "itemchange" && this.editor) {
                return;
            }

            if (this.trigger(DATABINDING)) {
                return;
            }

            this._cancelEditor();

            this._render();

            this._adjustHeight();

            this.trigger(DATABOUND);
        },

        _angularFooters: function(command) {
            var i, footer, aggregates;
            var allAggregates = this.dataSource.aggregates();
            var footerRows = this._footerItems();

            for (i = 0; i < footerRows.length; i++) {
                footer = footerRows.eq(i);
                aggregates = allAggregates[footer.attr("data-parentId")];

                this._angularFooter(command, footer.find("td").get(), aggregates);
            }
        },

        _angularFooter: function(command, cells, aggregates) {
            var columns = this.columns;
            this.angular(command, function() {
                return {
                    elements: cells,
                    data: map(columns, function(col){
                        return {
                            column: col,
                            aggregate: aggregates && aggregates[col.field]
                        };
                    })
                };
            });
        },

        items: function() {
            if (this._hasLockedColumns) {
                return this._items(this.tbody).add(this._items(this.lockedTable));
            } else {
                return this._items(this.tbody);
            }
        },

        _items: function(container) {
            return container.find("tr").filter(function() {
                return !$(this).hasClass(classNames.footerTemplate);
            });
        },

        _footerItems: function() {
            var container = this.tbody;
            if (this._hasLockedColumns) {
                container = container.add(this.lockedTable);
            }

            return container.find("tr").filter(function() {
                return $(this).hasClass(classNames.footerTemplate);
            });
        },

        dataItems: function() {
            var dataItems = kendo.ui.DataBoundWidget.fn.dataItems.call(this);
            if (this._hasLockedColumns) {
                var n = dataItems.length, tmp = new Array(2 * n);
                for (var i = n; --i >= 0;) {
                    tmp[i] = tmp[i + n] = dataItems[i];
                }
                dataItems = tmp;
            }

            return dataItems;
        },

        _showStatus: function(message) {
            var status = this.element.find(".k-status");
            var content = $(this.content).add(this.lockedContent);

            if (!status.length) {
                status = $("<div class='k-status' />").appendTo(this.element);
            }

            this._contentTree.render([]);
            if (this._hasLockedColumns) {
                this._lockedContentTree.render([]);
            }

            content.hide();

            status.html(message);
        },

        _hideStatus: function() {
            this.element.find(".k-status").remove();

            $(this.content).add(this.lockedContent).show();
        },

        _adjustHeight: function() {
            var element = this.element;
            var contentWrap = element.find(DOT + classNames.gridContentWrap);
            var header = element.find(DOT + classNames.gridHeader);
            var toolbar = element.find(DOT + classNames.gridToolbar);
            var height;
            var scrollbar = kendo.support.scrollbar();

            element.height(this.options.height);

            // identical code found in grid & scheduler :(
            var isHeightSet = function(el) {
                var initialHeight, newHeight;
                if (el[0].style.height) {
                    return true;
                } else {
                    initialHeight = el.height();
                }

                el.height("auto");
                newHeight = el.height();
                el.height("");

                return (initialHeight != newHeight);
            };

            if (isHeightSet(element)) {
                height = element.height() - header.outerHeight() - toolbar.outerHeight();
                contentWrap.height(height);

                if (this._hasLockedColumns) {
                    scrollbar = this.table[0].offsetWidth > this.table.parent()[0].clientWidth ? scrollbar : 0;
                    this.lockedContent.height(height - scrollbar);
                }
            }
        },

        _resize: function() {
            this._applyLockedContainersWidth();
            this._adjustHeight();
        },

        _minScreenSupport: function() {
            var any = this.hideMinScreenCols();

            if (any) {
                this.minScreenResizeHandler = proxy(this.hideMinScreenCols, this);
                $(window).on("resize", this.minScreenResizeHandler);
            }
        },
        hideMinScreenCols: function() {
            var cols = this.columns,
                any = false,
                screenWidth = (window.innerWidth > 0) ? window.innerWidth : screen.width;

            for (var i = 0; i < cols.length; i++) {
                var col = cols[i];
                var minWidth = col.minScreenWidth;
                if (minWidth !== undefined && minWidth !== null) {
                    any = true;
                    if (minWidth > screenWidth) {
                        this.hideColumn(col);
                    } else {
                        this.showColumn(col);
                    }
                }
            }
            return any;
        },

        destroy: function() {
            DataBoundWidget.fn.destroy.call(this);

            var dataSource = this.dataSource;

            dataSource.unbind(CHANGE, this._refreshHandler);
            dataSource.unbind(ERROR, this._errorHandler);
            dataSource.unbind(PROGRESS, this._progressHandler);

            if (this._resizeHandler) {
                $(window).off("resize" + NS, this._resizeHandler);
            }

            if (this._dragging) {
                this._dragging.destroy();
                this._dragging = null;
            }

            if (this.resizable) {
                this.resizable.destroy();
                this.resizable = null;
            }

            if (this.reorderable) {
                this.reorderable.destroy();
                this.reorderable = null;
            }

            if (this._draggableInstance && this._draggableInstance.element) {
                this._draggableInstance.destroy();
                this._draggableInstance = null;
            }

            if (this.minScreenResizeHandler) {
                $(window).off("resize", this.minScreenResizeHandler);
            }

            this._destroyEditor();

            this.element.off(NS);

            if (this._touchScroller) {
                this._touchScroller.destroy();
            }

            this._autoExpandable = null;

            this._refreshHandler = this._errorHandler = this._progressHandler = null;

            this.thead =
                this.content =
                this.tbody =
                this.table =
                this.element =
                this.lockedHeader =
                this.lockedContent = null;

            this._statusTree =
                this._headerTree =
                this._contentTree =
                this._lockedHeaderColsTree =
                this._lockedContentColsTree =
                this._lockedHeaderTree =
                this._lockedContentTree = null;
        },

        options: {
            name: "TreeList",
            columns: [],
            autoBind: true,
            scrollable: true,
            selectable: false,
            sortable: false,
            toolbar: null,
            height: null,
            columnMenu: false,
            messages: {
                noRows: "No records to display",
                loading: "Loading...",
                requestFailed: "Request failed.",
                retry: "Retry",
                commands: {
                    edit: "Edit",
                    update: "Update",
                    canceledit: "Cancel",
                    create: "Add new record",
                    createchild: "Add child record",
                    destroy: "Delete",
                    excel: "Export to Excel",
                    pdf: "Export to PDF"
                }
            },
            excel: {
                hierarchy: true
            },
            resizable: false,
            filterable: false,
            editable: false,
            reorderable: false
        },

        events: [
            CHANGE,
            EDIT,
            SAVE,
            REMOVE,
            EXPAND,
            COLLAPSE,
            DATABINDING,
            DATABOUND,
            CANCEL,
            DRAGSTART,
            DRAG,
            DROP,
            DRAGEND,
            FILTERMENUINIT,
            COLUMNHIDE,
            COLUMNSHOW,
            COLUMNREORDER,
            COLUMNRESIZE,
            COLUMNMENUINIT,
            COLUMNLOCK,
            COLUMNUNLOCK
        ],

        _toggle: function(model, expand) {
            var loaded = model.loaded();

            // reset error state
            if (model._error) {
                model.expanded = false;
                model._error = undefined;
            }

            // do not load items that are currently loading
            if (!loaded && model.expanded) {
                return;
            }

            // toggle expanded state
            if (typeof expand == "undefined") {
                expand = !model.expanded;
            }

            model.expanded = expand;

            if (!loaded) {
                this.dataSource.load(model)
                    .always(proxy(function() {
                        this._render();
                        this._syncLockedContentHeight();
                    }, this));
            }

            this._render();
            this._syncLockedContentHeight();
        },

        expand: function(row) {
            this._toggle(this.dataItem(row), true);
        },

        collapse: function(row) {
            this._toggle(this.dataItem(row), false);
        },

        _toggleChildren: function(e) {
            var icon = $(e.currentTarget);
            var model = this.dataItem(icon);
            var event = !model.expanded ? EXPAND : COLLAPSE;

            if (!this.trigger(event, { model: model })) {
                this._toggle(model);
            }

            e.preventDefault();
        },

        _attachEvents: function() {
            var icons = DOT + classNames.iconCollapse +
                ", ." + classNames.iconExpand +
                ", ." + classNames.refresh;
            var retryButton = DOT + classNames.retry;
            var dataSource = this.dataSource;

            this.element
                .on(MOUSEDOWN+ NS, icons, proxy(this._toggleChildren, this))
                .on(CLICK + NS, retryButton, proxy(dataSource.fetch, dataSource))
                .on(CLICK + NS, ".k-button[data-command]", proxy(this._commandClick, this));
        },

        _commandByName: function(name) {
            var columns = this.columns;
            var toolbar = $.isArray(this.options.toolbar) ? this.options.toolbar : [];
            var i, j, commands, currentName;

            name = name.toLowerCase();

            if (defaultCommands[name]) {
                return defaultCommands[name];
            }

            // command not found in defaultCommands, must be custom
            for (i = 0; i < columns.length; i++) {
                commands = columns[i].command;
                if (commands) {
                    for (j = 0; j < commands.length; j++) {
                        currentName = commands[j].name;

                        if (!currentName) {
                            continue;
                        }

                        if (currentName.toLowerCase() == name) {
                            return commands[j];
                        }
                    }
                }
            }

            // custom command in toolbar
            for (i = 0; i < toolbar.length; i++) {
                currentName = toolbar[i].name;

                if (!currentName) {
                    continue;
                }

                if (currentName.toLowerCase() == name) {
                    return toolbar[i];
                }
            }
        },

        _commandClick: function(e) {
            var button = $(e.currentTarget);
            var commandName = button.attr("data-command");
            var command = this._commandByName(commandName);
            var row = button.parentsUntil(this.wrapper, "tr");

            row = row.length ? row : undefined;

            if (command) {
                if (command.methodName) {
                    this[command.methodName](row);
                } else if (command.click) {
                    command.click.call(this, e);
                }

                e.preventDefault();
            }
        },

        _ensureExpandableColumn: function() {
            if (this._autoExpandable) {
                delete this._autoExpandable.expandable;
            }

            var visibleColumns = grep(this.columns, not(is("hidden")));
            var expandableColumns = grep(visibleColumns, is("expandable"));

            if (this.columns.length && !expandableColumns.length) {
                this._autoExpandable = visibleColumns[0];
                visibleColumns[0].expandable = true;
            }
        },

        _columns: function() {
            var columns = this.options.columns || [];

            this.columns = map(columns, function(column) {
                column = (typeof column === "string") ? { field: column } : column;

                return extend({ encoded: true }, column);
            });

            var lockedColumns = this._lockedColumns();
            if (lockedColumns.length > 0) {
                this._hasLockedColumns = true;
                this.columns = lockedColumns.concat(this._nonLockedColumns());
            }

            this._ensureExpandableColumn();

            this._columnTemplates();
            this._columnAttributes();
        },

        _columnTemplates: function() {
            var idx, length, column;
            var columns = this.columns;

            for (idx = 0, length = columns.length; idx < length; idx++) {
                column = columns[idx];
                if (column.template) {
                    column.template = kendo.template(column.template);
                }

                if (column.headerTemplate) {
                    column.headerTemplate = kendo.template(column.headerTemplate);
                }

                if (column.footerTemplate) {
                    column.footerTemplate = kendo.template(column.footerTemplate);
                }
            }
        },

        _columnAttributes: function() {
            // column style attribute is string, kendo.dom expects object
            var idx, length;
            var columns = this.columns;

            function convertStyle(attr) {
                var properties, i, declaration;

                if (attr && attr.style) {
                    properties = attr.style.split(";");
                    attr.style = {};

                    for (i = 0; i < properties.length; i++) {
                        declaration = properties[i].split(":");

                        var name = $.trim(declaration[0]);

                        if (name) {
                            attr.style[$.camelCase(name)] = $.trim(declaration[1]);
                        }
                    }
                }
            }

            for (idx = 0, length = columns.length; idx < length; idx++) {
                convertStyle(columns[idx].attributes);
                convertStyle(columns[idx].headerAttributes);
            }
        },

        _layout: function () {
            var columns = this.columns;
            var element = this.element;
            var layout = "";

            this.wrapper = element.addClass(classNames.wrapper);

            layout = "<div class='#= gridHeader #' style=\"padding-right: " + kendo.support.scrollbar() + "px;\">";

            if (this._hasLockedColumns) {
                layout += "<div class='k-grid-header-locked'>" +
                                "<table role='grid'>" +
                                    "<colgroup></colgroup>"+
                                    "<thead role='rowgroup' />" +
                                "</table>" +
                            "</div>";
            }

            layout += "<div class='#= gridHeaderWrap #'>" +
                            "<table role='grid'>" +
                                "<colgroup></colgroup>"+
                                "<thead role='rowgroup' />" +
                            "</table>" +
                        "</div>"+
                        "</div>";

            if (this._hasLockedColumns) {
                layout += "<div class='k-grid-content-locked'>" +
                                "<table role='treegrid' tabindex='0'>" +
                                    "<colgroup></colgroup>"+
                                    "<tbody />" +
                                "</table>" +
                            "</div>";
            }

            layout += "<div class='#= gridContentWrap #'>" +
                            "<table role='treegrid' tabindex='0'>" +
                                "<colgroup></colgroup>"+
                                "<tbody />" +
                            "</table>" +
                        "</div>";

            if (!this.options.scrollable) {
                layout =
                    "<table role='treegrid' tabindex='0'>" +
                        "<colgroup></colgroup>"+
                        "<thead class='#= gridHeader #' role='rowgroup' />" +
                        "<tbody />" +
                    "</table>";
            }

            if (this.options.toolbar) {
                layout = "<div class='#= header # #= gridToolbar #' />" + layout;
            }

            element.append(
                kendo.template(layout)(classNames) +
                "<div class='k-status' />"
            );

            this.toolbar = element.find(DOT + classNames.gridToolbar);

            var header = element.find(DOT + classNames.gridHeader).find("thead").addBack().filter("thead");
            this.thead = header.last();

            var content = element.find(DOT + classNames.gridContentWrap);
            if (!content.length) {
                content = element;
            } else {
                this.content = content;
            }

            this.table = content.find(">table");
            this.tbody = this.table.find(">tbody");

            if (this._hasLockedColumns) {
                this.lockedHeader = header.first().closest(".k-grid-header-locked");
                this.lockedContent = element.find(".k-grid-content-locked");
                this.lockedTable = this.lockedContent.children();
            }

            this._initVirtualTrees();

            this._renderCols();
            this._renderHeader();

            this.angular("compile", function() {
                return {
                    elements: header.find("th.k-header").get(),
                    data: map(columns, function(col) { return { column: col }; })
                };
            });
        },

        _initVirtualTrees: function() {
            this._headerColsTree = new kendoDom.Tree(this.thead.prev()[0]);
            this._contentColsTree = new kendoDom.Tree(this.tbody.prev()[0]);
            this._headerTree = new kendoDom.Tree(this.thead[0]);
            this._contentTree = new kendoDom.Tree(this.tbody[0]);
            this._statusTree = new kendoDom.Tree(this.element.children(".k-status")[0]);

            if (this.lockedHeader){
                this._lockedHeaderColsTree = new kendoDom.Tree(this.lockedHeader.find("colgroup")[0]);
                this._lockedContentColsTree = new kendoDom.Tree(this.lockedTable.find(">colgroup")[0]);
                this._lockedHeaderTree = new kendoDom.Tree(this.lockedHeader.find("thead")[0]);
                this._lockedContentTree = new kendoDom.Tree(this.lockedTable.find(">tbody")[0]);
            }
        },

        _toolbar: function() {
            var options = this.options.toolbar;
            var toolbar = this.toolbar;

            if (!options) {
                return;
            }

            if ($.isArray(options)) {
                var buttons = this._buildCommands(options);
                new kendoDom.Tree(toolbar[0]).render(buttons);
            } else {
                toolbar.append(kendo.template(options)({}));
            }

            this.angular("compile", function() {
                return { elements: toolbar.get() };
            });
        },

        _lockedColumns: function() {
            return grep(this.columns, is("locked"));
        },

        _nonLockedColumns: function() {
            return grep(this.columns, not(is("locked")));
        },

        _templateColumns: function() {
            return grep(this.columns, is("template"));
        },

        _flushCache: function() {
            if (this.options.$angular && this._templateColumns().length) {
                this._contentTree.render([]);
                if (this._hasLockedColumns) {
                    this._lockedContentTree.render([]);
                }
            }
        },

        _render: function(options) {
            options = options || {};

            var messages = this.options.messages;
            var data = this.dataSource.rootNodes();
            var selected = this.select().map(function(_, row) {
                return $(row).attr(kendo.attr("uid"));
            });

            this._absoluteIndex = 0;

            this._angularItems("cleanup");
            this._angularFooters("cleanup");
            this._flushCache();

            if (options.error) {
                // root-level error message
                this._showStatus(kendo.template(
                    "#: messages.requestFailed # " +
                    "<button class='#= buttonClass #'>#: messages.retry #</button>"
                )({
                    buttonClass: [ classNames.button, classNames.retry ].join(" "),
                    messages: messages
                }));
            } else if (!data.length) {
                // no rows message
                this._showStatus(kendo.htmlEncode(messages.noRows));
            } else {

                // render rows
                this._hideStatus();
                this._contentTree.render(this._trs({
                    columns: this._nonLockedColumns(),
                    aggregates: options.aggregates,
                    selected: selected,
                    data: data,
                    visible: true,
                    level: 0
                }));

                if (this._hasLockedColumns) {
                    this._absoluteIndex = 0;
                    this._lockedContentTree.render(this._trs({
                        columns: this._lockedColumns(),
                        aggregates: options.aggregates,
                        selected: selected,
                        data: data,
                        visible: true,
                        level: 0
                    }));
                }
            }

            if (this._touchScroller) {
                this._touchScroller.contentResized();
            }

            this._muteAngularRebind(function() {
                this._angularItems("compile");
                this._angularFooters("compile");
            });

            this._adjustRowsHeight();
        },

        _adjustRowsHeight: function() {
            if (!this._hasLockedColumns) {
                return;
            }

            var table = this.table;
            var lockedTable = this.lockedTable;
            var rows = table[0].rows;
            var length = rows.length;
            var idx;
            var lockedRows = lockedTable[0].rows;
            var containers = table.add(lockedTable);
            var containersLength = containers.length;
            var heights = [];

            var lockedHeaderRows = this.lockedHeader.find("tr");
            var headerRows = this.thead.find("tr");

            lockedHeaderRows.add(headerRows)
                .height("auto")
                .height(Math.max(lockedHeaderRows.height(), headerRows.height()));

            for (idx = 0; idx < length; idx++) {
                if (!lockedRows[idx]) {
                    break;
                }

                if (rows[idx].style.height) {
                    rows[idx].style.height = lockedRows[idx].style.height = "";
                }

                var offsetHeight1 = rows[idx].offsetHeight;
                var offsetHeight2 = lockedRows[idx].offsetHeight;
                var height = 0;

                if (offsetHeight1 > offsetHeight2) {
                    height = offsetHeight1;
                } else if (offsetHeight1 < offsetHeight2) {
                    height = offsetHeight2;
                }

                heights.push(height);
            }

            for (idx = 0; idx < containersLength; idx++) {
                containers[idx].style.display = "none";
            }

            for (idx = 0; idx < length; idx++) {
                if (heights[idx]) {
                    //add one to resolve row misalignment in IE
                    rows[idx].style.height = lockedRows[idx].style.height = (heights[idx] + 1) + "px";
                }
            }

            for (idx = 0; idx < containersLength; idx++) {
                containers[idx].style.display = "";
            }
        },

        _ths: function(columns) {
            var ths = [];
            var column, title, children, cellClasses, attr, headerContent;

            for (var i = 0, length = columns.length; i < length; i++) {
                column = columns[i];
                children = [];
                cellClasses = [classNames.header];

                if (column.headerTemplate) {
                    title = column.headerTemplate({});
                } else {
                    title = column.title || column.field || "";
                }

                if (column.headerTemplate) {
                    headerContent = kendoHtmlElement(title);
                } else {
                    headerContent = kendoTextElement(title);
                }

                if (column.sortable) {
                    children.push(kendoDomElement("a", { href: "#", className: classNames.link }, [
                        headerContent
                    ]));
                } else {
                    children.push(headerContent);
                }

                attr = {
                    "data-field": column.field,
                    "data-title": column.title,
                    "style": column.hidden === true ? { "display": "none" } : {},
                    className: cellClasses.join(" "),
                    "role": "columnheader"
                };

                attr = extend(true, {}, attr, column.headerAttributes);

                ths.push(kendoDomElement("th", attr, children));
            }

            return ths;
        },

        _cols: function(columns) {
            var cols = [];
            var width, attr;

            for (var i = 0; i < columns.length; i++) {
                if (columns[i].hidden === true) {
                    continue;
                }

                width = columns[i].width;
                attr = {};

                if (width && parseInt(width, 10) !== 0) {
                    attr.style = {
                        width: typeof width === "string" ? width : width + "px"
                    };
                }

                cols.push(kendoDomElement("col", attr));
            }

            return cols;
        },

        _renderCols: function() {
            var columns = this._nonLockedColumns();
            this._headerColsTree.render(this._cols(columns));

            if (this.options.scrollable) {
                this._contentColsTree.render(this._cols(columns));
            }

            if (this._hasLockedColumns) {
                columns = this._lockedColumns();
                this._lockedHeaderColsTree.render(this._cols(columns));
                this._lockedContentColsTree.render(this._cols(columns));
            }
        },

        _renderHeader: function() {
            var columns = this._nonLockedColumns();
            this._headerTree.render([kendoDomElement("tr", { "role": "row" }, this._ths(columns))]);

            if (this._hasLockedColumns) {
                columns = this._lockedColumns();
                this._lockedHeaderTree.render([kendoDomElement("tr", { "role": "row" }, this._ths(columns))]);
                this._applyLockedContainersWidth();
            }
        },

        _applyLockedContainersWidth: function() {
            if (!this._hasLockedColumns) {
                return;
            }

            var lockedWidth = columnsWidth(this.lockedHeader.find(">table>colgroup>col"));

            var headerTable = this.thead.parent();
            var nonLockedWidth = columnsWidth(headerTable.find(">colgroup>col"));

            var wrapperWidth = this.wrapper[0].clientWidth;
            var scrollbar = kendo.support.scrollbar();

            if (lockedWidth >= wrapperWidth) {
                lockedWidth = wrapperWidth - 3 * scrollbar;
            }

            this.lockedHeader
                .add(this.lockedContent)
                .width(lockedWidth);

            headerTable.add(this.table).width(nonLockedWidth);

            var width = wrapperWidth - lockedWidth - 2;
            this.content.width(width);
            headerTable.parent().width(width - scrollbar);
        },

        _trs: function(options) {
            var model, attr, className, hasChildren, childNodes, i, length;
            var rows = [];
            var level = options.level;
            var data = options.data;
            var dataSource = this.dataSource;
            var aggregates = dataSource.aggregates() || {};
            var columns = options.columns;

            for (i = 0, length = data.length; i < length; i++) {
                className = [];

                model = data[i];

                childNodes = model.loaded() && dataSource.childNodes(model);
                hasChildren = childNodes && childNodes.length;

                attr = { "role": "row" };

                attr[kendo.attr("uid")] = model.uid;

                if (hasChildren) {
                    attr["aria-expanded"] = !!model.expanded;
                }

                if (options.visible) {
                    if (this._absoluteIndex % 2 !== 0) {
                        className.push(classNames.alt);
                    }

                    this._absoluteIndex++;
                } else {
                    attr.style = { display: "none" };
                }

                if ($.inArray(model.uid, options.selected) >= 0) {
                    className.push(classNames.selected);
                }

                if (hasChildren) {
                    className.push(classNames.group);
                }

                if (model._edit) {
                    className.push("k-grid-edit-row");
                }

                attr.className = className.join(" ");

                rows.push(this._tds({
                    model: model,
                    attr: attr,
                    level: level
                }, columns, proxy(this._td, this)));

                if (hasChildren) {
                    rows = rows.concat(this._trs({
                        columns: columns,
                        aggregates: aggregates,
                        selected: options.selected,
                        visible: options.visible && !!model.expanded,
                        data: childNodes,
                        level: level + 1
                    }));
                }
            }

            if (this._hasFooterTemplate()) {
                attr = {
                    className: classNames.footerTemplate,
                    "data-parentId": model.parentId
                };

                if (!options.visible) {
                    attr.style = { display: "none" };
                }

                rows.push(this._tds({
                    model: aggregates[model.parentId],
                    attr: attr,
                    level: level
                }, columns, this._footerTd));
            }

            return rows;
        },

        _footerTd: function(options) {
            var content = [];
            var column = options.column;
            var template = options.column.footerTemplate || $.noop;
            var aggregates = options.model[column.field] || {};
            var attr = {
                "role": "gridcell",
                "style": column.hidden === true ? { "display": "none" } : {}
            };

            if (column.expandable) {
                content = content.concat(createPlaceholders({
                    level: options.level + 1,
                    className: classNames.iconPlaceHolder
                }));
            }

            if (column.attributes) {
                extend(attr, column.attributes);
            }

            content.push(kendoHtmlElement(template(aggregates) || ""));

            return kendoDomElement("td", attr, content);
        },

        _hasFooterTemplate: function() {
            return !!grep(this.columns, function(c) {
                return c.footerTemplate;
            }).length;
        },

        _tds: function(options, columns, renderer) {
            var children = [];
            var column;

            for (var i = 0, l = columns.length; i < l; i++) {
                column = columns[i];

                children.push(renderer({
                    model: options.model,
                    column: column,
                    level: options.level
                }));
            }

            return kendoDomElement("tr", options.attr, children);
        },

        _td: function(options) {
            var children = [];
            var model = options.model;
            var column = options.column;
            var iconClass;
            var attr = {
                "role": "gridcell",
                "style": column.hidden === true ? { "display": "none" } : {}
            };

            if (model._edit && column.field && model.editable(column.field)) {
                attr[kendo.attr("container-for")] = column.field;
            } else {
                if (column.expandable) {
                    children = createPlaceholders({ level: options.level, className: classNames.iconPlaceHolder });
                    iconClass = [classNames.icon];

                    if (model.hasChildren) {
                        iconClass.push(model.expanded ? classNames.iconCollapse : classNames.iconExpand);
                    } else {
                        iconClass.push(classNames.iconHidden);
                    }

                    if (model._error) {
                        iconClass.push(classNames.refresh);
                    } else if (!model.loaded() && model.expanded) {
                        iconClass.push(classNames.loading);
                    }

                    children.push(kendoDomElement("span", { className: iconClass.join(" ") }));

                    attr.style["white-space"] = "nowrap";
                }

                if (column.attributes) {
                    extend(true, attr, column.attributes);
                }

                if (column.command) {
                    if (model._edit) {
                        children = this._buildCommands(["update", "canceledit"]);
                    } else {
                        children = this._buildCommands(column.command);
                    }
                } else  {
                    children.push(this._cellContent(column, model));
                }
            }

            return kendoDomElement("td", attr, children);
        },

        _cellContent: function(column, model) {
            var value;

            if (column.template) {
                value = column.template(model);
            } else if (column.field) {
                value = model.get(column.field);
                if (value !== null && column.format) {
                    value = kendo.format(column.format, value);
                }
            }

            if (value === null || typeof value == "undefined") {
                value = "";
            }

            if (column.template || !column.encoded) {
                return kendoHtmlElement(value);
            } else {
                return kendoTextElement(value);
            }
        },

        _buildCommands: function(commands) {
            var i, result = [];

            for (i = 0; i < commands.length; i++) {
                result.push(this._button(commands[i]));
            }

            return result;
        },

        _button: function(command) {
            var name = (command.name || command).toLowerCase();
            var text = this.options.messages.commands[name];
            var icon = [];

            command = extend({}, defaultCommands[name], { text: text }, command);

            if (command.imageClass) {
                icon.push(kendoDomElement("span", {
                    className: [ "k-icon", command.imageClass ].join(" ")
                }));
            }

            return kendoDomElement(
                "button", {
                    "type": "button",
                    "data-command": name,
                    className: [ "k-button", "k-button-icontext", command.className ].join(" ")
                }, icon.concat([ kendoTextElement(command.text || command.name) ])
            );
        },

        _positionResizeHandle: function(e) {
            var th = $(e.currentTarget);
            var resizeHandle = this.resizeHandle;
            var position = th.position();
            var left = position.left;
            var cellWidth = th.outerWidth();
            var container = th.closest("div");
            var clientX = e.clientX + $(window).scrollLeft();
            var indicatorWidth = this.options.columnResizeHandleWidth || 3;

            left += container.scrollLeft();

            if (!resizeHandle) {
                resizeHandle = this.resizeHandle = $(
                    '<div class="k-resize-handle"><div class="k-resize-handle-inner" /></div>'
                );
            }

            var cellOffset = th.offset().left + cellWidth;
            var show = clientX > cellOffset - indicatorWidth && clientX < cellOffset + indicatorWidth;

            if(!show) {
                resizeHandle.hide();
                return;
            }

            container.append(resizeHandle);

            resizeHandle
                .show()
                .css({
                    top: position.top,
                    left: left + cellWidth - indicatorWidth - 1,
                    height: th.outerHeight(),
                    width: indicatorWidth * 3
                })
                .data("th", th);

            var that = this;
            resizeHandle.off("dblclick" + NS).on("dblclick" + NS, function () {
                //TODO handle frozen columns index
                var index= th.index();
                if ($.contains(that.thead[0], th[0])) {
                    index += grep(that.columns, function (val) { return val.locked && !val.hidden; }).length;
                }
                that.autoFitColumn(index);
            });
        },

        autoFitColumn: function (column) {
            var that = this,
                options = that.options,
                columns = that.columns,
                index,
                browser = kendo.support.browser,
                th,
                headerTable,
                isLocked,
                visibleLocked = that.lockedHeader ? leafDataCells(that.lockedHeader.find(">table>thead")).filter(isCellVisible).length : 0,
                col;

            //  retrieve the column object, depending on the method argument
            if (typeof column == "number") {
                column = columns[column];
            } else if (isPlainObject(column)) {
                column = grep(columns, function (item) {
                    return item === column;
                })[0];
            } else {
                column = grep(columns, function (item) {
                    return item.field === column;
                })[0];
            }

            if (!column || column.hidden) {
                return;
            }

            index = inArray(column, columns);
            isLocked = column.locked;

            if (isLocked) {
                headerTable = that.lockedHeader.children("table");
            } else {
                headerTable = that.thead.parent();
            }

            th = headerTable.find("[data-index='" + index + "']");

            var contentTable = isLocked ? that.lockedTable : that.table,
                footer = that.footer || $();

            if (that.footer && that.lockedContent) {
                footer = isLocked ? that.footer.children(".k-grid-footer-locked") : that.footer.children(".k-grid-footer-wrap");
            }

            var footerTable = footer.find("table").first();

            if (that.lockedHeader && visibleLocked >= index && !isLocked) {
                index -= visibleLocked;
            }

            // adjust column index, depending on previous hidden columns
            for (var j = 0; j < columns.length; j++) {
                if (columns[j] === column) {
                    break;
                } else {
                    if (columns[j].hidden) {
                        index--;
                    }
                }
            }

            // get col elements
            if (options.scrollable) {
                col = headerTable.find("col:not(.k-group-col):not(.k-hierarchy-col):eq(" + index + ")")
                    .add(contentTable.children("colgroup").find("col:not(.k-group-col):not(.k-hierarchy-col):eq(" + index + ")"))
                    .add(footerTable.find("colgroup").find("col:not(.k-group-col):not(.k-hierarchy-col):eq(" + index + ")"));
            } else {
                col = contentTable.children("colgroup").find("col:not(.k-group-col):not(.k-hierarchy-col):eq(" + index + ")");
            }

            var tables = headerTable.add(contentTable).add(footerTable);

            var oldColumnWidth = th.outerWidth();

            // reset the table and autofitted column widths
            // if scrolling is disabled, we need some additional repainting of the table
            col.width("");
            tables.css("table-layout", "fixed");
            col.width("auto");
            tables.addClass("k-autofitting");
            tables.css("table-layout", "");

            var newColumnWidth = Math.ceil(Math.max(th.outerWidth(), contentTable.find("tr").eq(0).children("td:visible").eq(index).outerWidth(), footerTable.find("tr").eq(0).children("td:visible").eq(index).outerWidth()));

            col.width(newColumnWidth);
            column.width = newColumnWidth;

            // if all visible columns have widths, the table needs a pixel width as well
            if (options.scrollable) {
                var cols = headerTable.find("col"),
                    colWidth,
                    totalWidth = 0;
                for (var idx = 0, length = cols.length; idx < length; idx += 1) {
                    colWidth = cols[idx].style.width;
                    if (colWidth && colWidth.indexOf("%") == -1) {
                        totalWidth += parseInt(colWidth, 10);
                    } else {
                        totalWidth = 0;
                        break;
                    }
                }

                if (totalWidth) {
                    tables.each(function () {
                        this.style.width = totalWidth + "px";
                    });
                }
            }

            if (browser.msie && browser.version == 8) {
                tables.css("display", "inline-table");
                setTimeout(function () {
                    tables.css("display", "table");
                }, 1);
            }

            tables.removeClass("k-autofitting");

            that.trigger(COLUMNRESIZE, {
                column: column,
                oldWidth: oldColumnWidth,
                newWidth: newColumnWidth
            });

            that._applyLockedContainersWidth();
            that._syncLockedContentHeight();
            that._syncLockedHeaderHeight();
        },

        _adjustLockedHorizontalScrollBar: function() {
            var table = this.table,
                content = table.parent();

            var scrollbar = table[0].offsetWidth > content[0].clientWidth ? kendo.support.scrollbar() : 0;
            this.lockedContent.height(content.height() - scrollbar);
        },

        _syncLockedContentHeight: function() {
            if (this.lockedTable) {
                if (!this._touchScroller) {
                    this._adjustLockedHorizontalScrollBar();
                }
                this._adjustRowsHeight(this.table, this.lockedTable);
            }
        },

        _syncLockedHeaderHeight: function() {
            if (this.lockedHeader) {
                var lockedTable = this.lockedHeader.children("table");
                var table = this.thead.parent();

                this._adjustRowsHeight(lockedTable, table);

                syncTableHeight(lockedTable, table);
            }
        },

        _resizable: function() {
            if (!this.options.resizable) {
                return;
            }

            if (this.resizable) {
                this.resizable.destroy();
            }

            var treelist = this;

            $(this.lockedHeader).find("thead").add(this.thead)
                .on("mousemove" + NS, "th", $.proxy(this._positionResizeHandle, this));

            this.resizable = new kendo.ui.Resizable(this.wrapper, {
                handle: ".k-resize-handle",
                start: function(e) {
                    var th = $(e.currentTarget).data("th");
                    var colSelector = "col:eq(" + $.inArray(th[0], th.parent().children().filter(":visible")) + ")";
                    var header, contentTable;

                    treelist.wrapper.addClass("k-grid-column-resizing");

                    if (treelist.lockedHeader && $.contains(treelist.lockedHeader[0], th[0])) {
                        header = treelist.lockedHeader;
                        contentTable = treelist.lockedTable;
                    } else {
                        header = treelist.thead.parent();
                        contentTable = treelist.table;
                    }

                    this.col = contentTable.children("colgroup").find(colSelector)
                          .add(header.find(colSelector));
                    this.th = th;
                    this.startLocation = e.x.location;
                    this.columnWidth = th.outerWidth();
                    this.table = this.col.closest("table");
                    this.totalWidth = this.table.width();
                },
                resize: function(e) {
                    var minColumnWidth = 11;
                    var delta = e.x.location - this.startLocation;

                    if (this.columnWidth + delta < minColumnWidth) {
                        delta = minColumnWidth - this.columnWidth;
                    }

                    this.table.width(this.totalWidth + delta);
                    this.col.width(this.columnWidth + delta);
                },
                resizeend: function() {
                    treelist.wrapper.removeClass("k-grid-column-resizing");

                    var field = this.th.attr("data-field");
                    var column = grep(treelist.columns, function(c) {
                        return c.field == field;
                    });
                    var newWidth = Math.floor(this.th.outerWidth());

                    column[0].width = newWidth;
                    treelist._resize();
                    treelist._adjustRowsHeight();

                    treelist.trigger(COLUMNRESIZE, {
                        column: column,
                        oldWidth: this.columnWidth,
                        newWidth: newWidth
                    });

                    this.table = this.col = this.th = null;
                }
            });
        },

        _sortable: function() {
            var columns = this.columns;
            var column;
            var sortableInstance;
            var cells = $(this.lockedHeader).add(this.thead).find("th");
            var cell, idx, length;
            var fieldAttr = kendo.attr("field");
            var sortable = this.options.sortable;

            if (!sortable) {
                return;
            }

            for (idx = 0, length = cells.length; idx < length; idx++) {
                column = columns[idx];

                if (column.sortable !== false && !column.command && column.field) {
                    cell = cells.eq(idx);

                    sortableInstance = cell.data("kendoColumnSorter");
                    if (sortableInstance) {
                        sortableInstance.destroy();
                    }

                    cell.attr(fieldAttr, column.field)
                        .kendoColumnSorter(
                            extend({}, sortable, column.sortable, {
                                dataSource: this.dataSource
                            })
                        );
                }
            }
        },

        _filterable: function() {
            var cells = $(this.lockedHeader).add(this.thead).find("th");
            var filterable = this.options.filterable;
            var idx, length, column, cell, filterMenuInstance;

            if (!filterable || this.options.columnMenu) {
                return;
            }

            var filterInit = proxy(function(e) {
                this.trigger(FILTERMENUINIT, { field: e.field, container: e.container });
            }, this);

            for (idx = 0, length = cells.length; idx < length; idx++) {
                column = this.columns[idx];
                cell = cells.eq(idx);

                filterMenuInstance = cell.data("kendoFilterMenu");
                if (filterMenuInstance) {
                    filterMenuInstance.destroy();
                }

                if (column.command || column.filterable === false) {
                    continue;
                }

                cell.kendoFilterMenu(extend(true, {}, filterable, column.filterable, {
                    dataSource: this.dataSource,
                    init: filterInit
                }));
            }
        },

        _change: function() {
            this.trigger(CHANGE);
        },

        _selectable: function() {
            var selectable = this.options.selectable;
            var filter;
            var element = this.table;
            var useAllItems;

            if (selectable) {
                selectable = kendo.ui.Selectable.parseOptions(selectable);

                if (this._hasLockedColumns) {
                    element = element.add(this.lockedTable);
                    useAllItems = selectable.multiple && selectable.cell;
                }

                filter = ">tbody>tr:not(.k-footer-template)";

                if (selectable.cell) {
                    filter = filter + ">td";
                }

                this.selectable = new kendo.ui.Selectable(element, {
                    filter: filter,
                    aria: true,
                    multiple: selectable.multiple,
                    change: proxy(this._change, this),
                    useAllItems: useAllItems,
                    continuousItems: proxy(this._continuousItems, this, filter, selectable.cell),
                    relatedTarget: !selectable.cell && this._hasLockedColumns ? proxy(this._selectableTarget, this) : undefined
                });
            }
        },

        _continuousItems: function(filter, cell) {
            if (!this.lockedContent) {
                return;
            }

            var lockedItems = $(filter, this.lockedTable);
            var nonLockedItems = $(filter, this.table);
            var columns = cell ? this._lockedColumns().length : 1;
            var nonLockedColumns = cell ? this.columns.length - columns : 1;
            var result = [];

            for (var idx = 0; idx < lockedItems.length; idx += columns) {
                push.apply(result, lockedItems.slice(idx, idx + columns));
                push.apply(result, nonLockedItems.splice(0, nonLockedColumns));
            }

            return result;
        },

        _selectableTarget: function(items) {
            var related;
            var result = $();
            for (var idx = 0, length = items.length; idx < length; idx ++) {
                related = this._relatedRow(items[idx]);

                if (inArray(related[0], items) < 0) {
                    result = result.add(related);
                }
            }

            return result;
        },

        _relatedRow: function(row) {
            var lockedTable = this.lockedTable;
            row = $(row);

            if (!lockedTable) {
                return row;
            }

            var table = row.closest(this.table.add(this.lockedTable));
            var index = table.find(">tbody>tr").index(row);

            table = table[0] === this.table[0] ? lockedTable : this.table;

            return table.find(">tbody>tr").eq(index);
        },

        select: function(value) {
            var selectable = this.selectable;

            if (!selectable) {
                return $();
            }

            if (typeof value !== "undefined") {
                if (!selectable.options.multiple) {
                    selectable.clear();

                    value = value.first();
                }

                if (this._hasLockedColumns) {
                    value = value.add($.map(value, proxy(this._relatedRow, this)));
                }
            }

            return selectable.value(value);
        },

        clearSelection: function() {
            var selected = this.select();

            if (selected.length) {
                this.selectable.clear();

                this.trigger(CHANGE);
            }
        },

        _dataSource: function(dataSource) {
            var ds = this.dataSource;

            if (ds) {
                ds.unbind(CHANGE, this._refreshHandler);
                ds.unbind(ERROR, this._errorHandler);
                ds.unbind(PROGRESS, this._progressHandler);
            }

            this._refreshHandler = proxy(this.refresh, this);
            this._errorHandler = proxy(this._error, this);
            this._progressHandler = proxy(this._progress, this);

            ds = this.dataSource = TreeListDataSource.create(dataSource);

            ds.bind(CHANGE, this._refreshHandler);
            ds.bind(ERROR, this._errorHandler);
            ds.bind(PROGRESS, this._progressHandler);
        },

        setDataSource: function(dataSource) {
            this._dataSource(dataSource);
            this._sortable();
            this._filterable();

            this._contentTree.render([]);

            if (this.options.autoBind) {
                this.dataSource.fetch();
            }
        },

        dataItem: function(element) {
            var row = $(element).closest("tr");
            var model = this.dataSource.getByUid(row.attr(kendo.attr("uid")));

            return model;
        },

        editRow: function(row) {
            var model;

            if (typeof row === STRING) {
                row = this.tbody.find(row);
            }

            model = this.dataItem(row);

            if (!model) {
                return;
            }

            if (this._editMode() != "popup") {
                model._edit = true;
            }

            this._cancelEditor();

            this._render();

            this._createEditor(model);

            this.trigger(EDIT, {
                container: this.editor.wrapper,
                model: model
            });
        },

        _cancelEdit: function(e) {
            e = extend(e, {
                container: this.editor.wrapper,
                model: this.editor.model
            });

            if (this.trigger(CANCEL, e)) {
                return;
            }

            this.cancelRow();
        },

        cancelRow: function() {
            this._cancelEditor();

            this._render();
        },

        saveRow: function() {
            var editor = this.editor;
            var args;

            if (!editor) {
                return ;
            }

            args = {
                model: editor.model,
                container: editor.wrapper
            };

            if (editor.end() && !this.trigger(SAVE, args)) {
                this.dataSource.sync();
            }
        },

        addRow: function(parent) {
            var editor = this.editor;
            var index = 0;
            var model = {};

            if (editor && !editor.end()) {
                return;
            }

            if (parent) {
                if (!(parent instanceof TreeListModel)) {
                    parent = this.dataItem(parent);
                }

                model[parent.parentIdField] = parent.id;
                index = this.dataSource.indexOf(parent) + 1;
                parent.set("expanded", true);

                this.dataSource.load(parent).then(proxy(this._insertAt, this, model, index));

                return;
            }

            this._insertAt(model, index);
        },

        _insertAt: function(model, index) {
            model = this.dataSource.insert(index, model);

            var row = this.itemFor(model);

            this.editRow(row);
        },

        removeRow: function(row) {
            var model = this.dataItem(row);
            var args = {
                model: model,
                row: row
            };

            if (model && !this.trigger(REMOVE, args)) {
                this.dataSource.remove(model);

                this.dataSource.sync();
            }
        },

        _cancelEditor: function() {
            var model;
            var editor = this.editor;

            if (editor) {
                model = editor.model;

                this._destroyEditor();

                this.dataSource.cancelChanges(model);

                model._edit = false;
            }
        },

        _destroyEditor: function() {
            if (!this.editor) {
                return;
            }

            this.editor.close();
            this.editor = null;
        },

        _createEditor: function(model) {
            var row = this.itemFor(model);

            row = row.add(this._relatedRow(row));

            var mode = this._editMode();

            var options = {
                columns: this.columns,
                model: model,
                target: this,
                clearContainer: false,
                template: this.options.editable.template
            };

            if (mode == "inline") {
                this.editor = new Editor(row, options);
            } else {
                extend(options, {
                    window: this.options.editable.window,
                    commandRenderer: proxy(function () {
                        return this._buildCommands(["update", "canceledit"]);
                    }, this),
                    fieldRenderer: this._cellContent,
                    save: proxy(this.saveRow, this),
                    cancel: proxy(this._cancelEdit, this),
                    appendTo: this.wrapper
                });

                this.editor = new PopupEditor(row, options);
            }
        },

        _editMode: function() {
            var mode = "inline",
                editable = this.options.editable;

            if (editable !== true) {
                if (typeof editable == "string") {
                    mode = editable;
                } else {
                    mode = editable.mode || mode;
                }
            }

            return mode.toLowerCase();
        },

        hideColumn: function(column) {
            this._toggleColumnVisibility(column, true);
        },

        showColumn: function(column) {
            this._toggleColumnVisibility(column, false);
        },

        _toggleColumnVisibility: function(column, hidden) {
            column = this._findColumn(column);

            if (!column || column.hidden === hidden) {
                return;
            }

            column.hidden = hidden;

            this._ensureExpandableColumn();

            this._renderCols();
            this._renderHeader();
            this._render();

            this._adjustTablesWidth();

            this.trigger(hidden ? COLUMNHIDE : COLUMNSHOW, { column: column });

            if (!hidden && !column.width) {
                this.table
                    .add(this.thead.closest("table"))
                    .width("");
            }
        },

        _findColumn: function(column) {
            if (typeof column == "number") {
                column = this.columns[column];
            } else if (isPlainObject(column)) {
                column = grep(this.columns, function(item) {
                    return item === column;
                })[0];
            } else {
                column = grep(this.columns, function(item) {
                    return item.field === column;
                })[0];
            }

            return column;
        },

        _adjustTablesWidth: function() {
            var idx, length;
            var cols = this.thead.prev().children();
            var colWidth, width = 0;

            for (idx = 0, length = cols.length; idx < length; idx++ ) {
                colWidth = cols[idx].style.width;
                if (colWidth && colWidth.indexOf("%") == -1) {
                    width += parseInt(colWidth, 10);
                } else {
                    width = 0;
                    break;
                }
            }


            if (width) {
                this.table
                    .add(this.thead.closest("table"))
                    .width(width);
            }
        },

        _reorderable: function() {
            if (!this.options.reorderable) {
                return;
            }

            var scrollable = this.options.scrollable === true;
            var selector = (scrollable ? ".k-grid-header:first " : "table:first>.k-grid-header ") + HEADERCELLS;
            var that = this;

            this._draggableInstance = new ui.Draggable(this.wrapper, {
                group: kendo.guid(),
                filter: selector,
                hint: function(target) {
                    return $('<div class="k-header k-drag-clue" />')
                    .css({
                        width: target.width(),
                        paddingLeft: target.css("paddingLeft"),
                        paddingRight: target.css("paddingRight"),
                        lineHeight: target.height() + "px",
                        paddingTop: target.css("paddingTop"),
                        paddingBottom: target.css("paddingBottom")
                    })
                    .html(target.attr(kendo.attr("title")) || target.attr(kendo.attr("field")) || target.text())
                    .prepend('<span class="k-icon k-drag-status k-denied" />');
                }
            });

            this.reorderable = new ui.Reorderable(this.wrapper, {
                draggable: this._draggableInstance,
                dragOverContainers: proxy(this._allowDragOverContainers, this),
                inSameContainer: function(e) {
                    return $(e.source).parent()[0] === $(e.target).parent()[0];
                },
                change: function(e) {
                    var newIndex = e.newIndex;
                    var oldIndex = e.oldIndex;
                    var before = e.position === "before";
                    var column = that.columns[oldIndex];

                    that.trigger(COLUMNREORDER, {
                        newIndex: newIndex,
                        oldIndex: oldIndex,
                        column: column
                    });

                    that.reorderColumn(newIndex, column, before);
                }
            });
        },

        _allowDragOverContainers: function(index) {
            return this.columns[index].lockable !== false;
        },

        reorderColumn: function(destIndex, column, before) {
            var lockChanged;
            var columns = this.columns;
            var sourceIndex = inArray(column, columns);
            var destColumn = columns[destIndex];
            var isLocked = !!destColumn.locked;
            var nonLockedColumnsLength = this._nonLockedColumns().length;

            if (sourceIndex === destIndex) {
                return;
            }

            if (isLocked && !column.locked && nonLockedColumnsLength == 1) {
                return;
            }

            if (!isLocked && column.locked && (columns.length - nonLockedColumnsLength == 1)) {
                return;
            }

            if (before === undefined) {
                before = destIndex < sourceIndex;
            }

            lockChanged = !!column.locked;
            lockChanged = lockChanged != isLocked;

            column.locked = isLocked;
            columns.splice(before ? destIndex : destIndex + 1, 0, column);
            columns.splice(sourceIndex < destIndex ? sourceIndex : sourceIndex + 1, 1);

            this._renderCols();

            //reorder column header manually
            var ths = $(this.lockedHeader).add(this.thead).find("th");
            ths.eq(sourceIndex)[before ? "insertBefore" : "insertAfter"](ths.eq(destIndex));

            var dom = this._headerTree.children[0].children;
            if (this._hasLockedColumns) {
                dom = this._lockedHeaderTree.children[0].children.concat(dom);
            }
            dom.splice(before ? destIndex : destIndex + 1, 0, dom[sourceIndex]);
            dom.splice(sourceIndex < destIndex ? sourceIndex : sourceIndex + 1, 1);
            if (this._hasLockedColumns) {
                this._lockedHeaderTree.children[0].children = dom.splice(0, this._lockedColumns().length);
                this._headerTree.children[0].children = dom;
            }

            this._applyLockedContainersWidth();

            this.refresh();

            if(!lockChanged) {
                return;
            }

            if (isLocked) {
                this.trigger(COLUMNLOCK, {
                    column: column
                });
            } else {
                this.trigger(COLUMNUNLOCK, {
                    column: column
                });
            }
        },

        lockColumn: function(column) {
            var columns = this.columns;

            if (typeof column == "number") {
                column = columns[column];
            } else {
                column = grep(columns, function(item) {
                    return item.field === column;
                })[0];
            }

            if (!column || column.hidden) {
                return;
            }

            var index = this._lockedColumns().length - 1;
            this.reorderColumn(index, column, false);
        },

        unlockColumn: function(column) {
            var columns = this.columns;

            if (typeof column == "number") {
                column = columns[column];
            } else {
                column = grep(columns, function(item) {
                    return item.field === column;
                })[0];
            }

            if (!column || column.hidden) {
                return;
            }

            var index = this._lockedColumns().length;
            this.reorderColumn(index, column, true);
        },

        _columnMenu: function() {
            var ths = $(this.lockedHeader).add(this.thead).find("th");
            var columns = this.columns;
            var options = this.options;
            var columnMenu = options.columnMenu;
            var column, menu, menuOptions, sortable, filterable;
            var initHandler = proxy(this._columnMenuInit, this);
            var lockedColumnsLength = this._lockedColumns().length;

            if (!columnMenu) {
                return;
            }

            if (typeof columnMenu == "boolean") {
                columnMenu = {};
            }

            for (var i = 0; i < ths.length; i++) {
                column = columns[i];
                if (!column.field) {
                    continue;
                }

                menu = ths.eq(i).data("kendoColumnMenu");
                if (menu) {
                    menu.destroy();
                }

                sortable = false;
                if (column.sortable !== false && columnMenu.sortable !== false && options.sortable !== false) {
                    sortable = extend({}, options.sortable, { compare: (column.sortable || {}).compare });
                }

                filterable = false;
                if (options.filterable && column.filterable !== false && columnMenu.filterable !== false) {
                    filterable = extend({ pane: this.pane }, column.filterable, options.filterable);
                }

                menuOptions = {
                    dataSource: this.dataSource,
                    values: column.values,
                    columns: columnMenu.columns,
                    sortable: sortable,
                    filterable: filterable,
                    messages: columnMenu.messages,
                    owner: this,
                    closeCallback: $.noop,
                    init: initHandler,
                    pane: this.pane,
                    lockedColumns: column.lockable !== false && lockedColumnsLength > 0
                };

                if (options.$angular) {
                    menuOptions.$angular = options.$angular;
                }

                ths.eq(i).kendoColumnMenu(menuOptions);
            }
        },

        _columnMenuInit: function(e) {
            this.trigger(COLUMNMENUINIT, { field: e.field, container: e.container });
        }
    });

    if (kendo.ExcelMixin) {
        kendo.ExcelMixin.extend(TreeList.prototype);
    }

    if (kendo.PDFMixin) {
        kendo.PDFMixin.extend(TreeList.prototype);

        TreeList.fn._drawPDF = function (progress) {
            var promise = new $.Deferred();

            this._drawPDFShadow({
                width: this.wrapper.width()
            }, {
                avoidLinks: this.options.pdf.avoidLinks
            })
            .done(function (group) {
                var args = {
                    page: group,
                    pageNumber: 1,
                    progress: 1,
                    totalPages: 1
                };

                progress.notify(args);
                promise.resolve(args.page);
            })
            .fail(function (err) {
                promise.reject(err);
            });

            return promise;
        };
    }

    extend(true, kendo.data, {
        TreeListDataSource: TreeListDataSource,
        TreeListModel: TreeListModel
    });

    extend(true, kendo.ui, {
        TreeList: TreeList
    });

    ui.plugin(TreeList);

})(window.kendo.jQuery);



})();

return window.kendo;

}, typeof define == 'function' && define.amd ? define : function(_, f){ f(); });