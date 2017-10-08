/*
* Kendo UI v2015.1.408 (http://www.telerik.com/kendo-ui)
* Copyright 2015 Telerik AD. All rights reserved.
*
* Kendo UI commercial licenses may be obtained at
* http://www.telerik.com/purchase/license-agreement/kendo-ui-complete
* If you do not own a commercial license, this file shall be governed by the trial license terms.
*/
(function(f, define){
    define([ "./kendo.listview", "./kendo.dropdownlist", "./kendo.upload" ], f);
})(function(){

(function($, undefined) {
    var kendo = window.kendo,
        Widget = kendo.ui.Widget,
        isPlainObject = $.isPlainObject,
        proxy = $.proxy,
        extend = $.extend,
        placeholderSupported = kendo.support.placeholder,
        browser = kendo.support.browser,
        isFunction = kendo.isFunction,
        trimSlashesRegExp = /(^\/|\/$)/g,
        CHANGE = "change",
        APPLY = "apply",
        ERROR = "error",
        CLICK = "click",
        NS = ".kendoFileBrowser",
        BREADCRUBMSNS = ".kendoBreadcrumbs",
        SEARCHBOXNS = ".kendoSearchBox",
        NAMEFIELD = "name",
        SIZEFIELD = "size",
        TYPEFIELD = "type",
        DEFAULTSORTORDER = { field: TYPEFIELD, dir: "asc" },
        EMPTYTILE = kendo.template('<li class="k-tile-empty"><strong>${text}</strong></li>'),
        TOOLBARTMPL = '<div class="k-widget k-filebrowser-toolbar k-header k-floatwrap">' +
                            '<div class="k-toolbar-wrap">' +
                                '# if (showUpload) { # ' +
                                    '<div class="k-widget k-upload"><div class="k-button k-button-icontext k-upload-button">' +
                                        '<span class="k-icon k-add"></span>#=messages.uploadFile#<input type="file" name="file" /></div></div>' +
                                '# } #' +

                                '# if (showCreate) { #' +
                                     '<button type="button" class="k-button k-button-icon"><span class="k-icon k-addfolder" /></button>' +
                                '# } #' +

                                '# if (showDelete) { #' +
                                    '<button type="button" class="k-button k-button-icon k-state-disabled"><span class="k-icon k-delete" /></button>&nbsp;' +
                                '# } #' +
                            '</div>' +
                            '<div class="k-tiles-arrange">' +
                                '<label>#=messages.orderBy#: <select /></label></a>' +
                            '</div>' +
                        '</div>';

    extend(true, kendo.data, {
        schemas: {
            "filebrowser": {
                data: function(data) {
                    return data.items || data || [];
                },
                model: {
                    id: "name",
                    fields: {
                        name: "name",
                        size: "size",
                        type: "type"
                    }
                }
            }
        }
    });

    extend(true, kendo.data, {
        transports: {
            "filebrowser": kendo.data.RemoteTransport.extend({
                init: function(options) {
                    kendo.data.RemoteTransport.fn.init.call(this, $.extend(true, {}, this.options, options));
                },
                _call: function(type, options) {
                    options.data = $.extend({}, options.data, { path: this.options.path() });

                    if (isFunction(this.options[type])) {
                        this.options[type].call(this, options);
                    } else {
                        kendo.data.RemoteTransport.fn[type].call(this, options);
                    }
                },
                read: function(options) {
                    this._call("read", options);
                },
                create: function(options) {
                    this._call("create", options);
                },
                destroy: function(options) {
                    this._call("destroy", options);
                },
                update: function() {
                    //updates are handled by the upload
                },
                options: {
                    read: {
                        type: "POST"
                    },
                    update: {
                        type: "POST"
                    },
                    create: {
                        type: "POST"
                    },
                    destroy: {
                        type: "POST"
                    }
                }
            })
        }
    });

    function bindDragEventWrappers(element, onDragEnter, onDragLeave) {
        var hideInterval, lastDrag;

        element
            .on("dragenter" + NS, function() {
                onDragEnter();
                lastDrag = new Date();

                if (!hideInterval) {
                    hideInterval = setInterval(function() {
                        var sinceLastDrag = new Date() - lastDrag;
                        if (sinceLastDrag > 100) {
                            onDragLeave();

                            clearInterval(hideInterval);
                            hideInterval = null;
                        }
                    }, 100);
                }
            })
            .on("dragover" + NS, function() {
                lastDrag = new Date();
            });
    }

    var offsetTop;
    if (browser.msie && browser.version < 8) {
        offsetTop = function (element) {
            return element.offsetTop;
        };
    } else {
        offsetTop = function (element) {
            return element.offsetTop - $(element).height();
        };
    }

    function concatPaths(path, name) {
        if(path === undefined || !path.match(/\/$/)) {
            path = (path || "") + "/";
        }
        return path + name;
    }

    function sizeFormatter(value) {
        if(!value) {
            return "";
        }

        var suffix = " bytes";

        if (value >= 1073741824) {
            suffix = " GB";
            value /= 1073741824;
        } else if (value >= 1048576) {
            suffix = " MB";
            value /= 1048576;
        } else  if (value >= 1024) {
            suffix = " KB";
            value /= 1024;
        }

        return Math.round(value * 100) / 100 + suffix;
    }

    function fieldName(fields, name) {
        var descriptor = fields[name];

        if (isPlainObject(descriptor)) {
            return descriptor.from || descriptor.field || name;
        }
        return descriptor;
    }

    var FileBrowser = Widget.extend({
        init: function(element, options) {
            var that = this;

            options = options || {};

            Widget.fn.init.call(that, element, options);

            that.element.addClass("k-filebrowser");

            that.element
                .on(CLICK + NS, ".k-filebrowser-toolbar button:not(.k-state-disabled):has(.k-delete)", proxy(that._deleteClick, that))
                .on(CLICK + NS, ".k-filebrowser-toolbar button:not(.k-state-disabled):has(.k-addfolder)", proxy(that._addClick, that))
                .on("keydown" + NS, "li.k-state-selected input", proxy(that._directoryKeyDown, that))
                .on("blur" + NS, "li.k-state-selected input", proxy(that._directoryBlur, that));

            that._dataSource();

            that.refresh();

            that.path(that.options.path);
        },

        options: {
            name: "FileBrowser",
            messages: {
                uploadFile: "Upload",
                orderBy: "Arrange by",
                orderByName: "Name",
                orderBySize: "Size",
                directoryNotFound: "A directory with this name was not found.",
                emptyFolder: "Empty Folder",
                deleteFile: 'Are you sure you want to delete "{0}"?',
                invalidFileType: "The selected file \"{0}\" is not valid. Supported file types are {1}.",
                overwriteFile: "A file with name \"{0}\" already exists in the current directory. Do you want to overwrite it?",
                dropFilesHere: "drop file here to upload",
                search: "Search"
            },
            transport: {},
            path: "/",
            fileTypes: "*.*"
        },

        events: [ERROR, CHANGE, APPLY],

        destroy: function() {
            var that = this;

            Widget.fn.destroy.call(that);

            that.dataSource
                .unbind(ERROR, that._errorHandler);

            that.element
                .add(that.list)
                .add(that.toolbar)
                .off(NS);

            kendo.destroy(that.element);
        },

        value: function() {
            var that = this,
                selected = that._selectedItem(),
                path,
                fileUrl = that.options.transport.fileUrl;

            if (selected && selected.get(TYPEFIELD) === "f") {
                path = concatPaths(that.path(), selected.get(NAMEFIELD)).replace(trimSlashesRegExp, "");
                if (fileUrl) {
                    path = isFunction(fileUrl) ? fileUrl(path) : kendo.format(fileUrl, encodeURIComponent(path));
                }
                return path;
            }
        },

        _selectedItem: function() {
            var listView = this.listView,
                selected = listView.select();

            if (selected.length) {
                return this.dataSource.getByUid(selected.attr(kendo.attr("uid")));
            }
        },

        _toolbar: function() {
            var that = this,
                template = kendo.template(TOOLBARTMPL),
                messages = that.options.messages,
                arrangeBy = [
                    { text: messages.orderByName, value: "name" },
                    { text: messages.orderBySize, value: "size" }
                ];

            that.toolbar = $(template({
                    messages: messages,
                    showUpload: that.options.transport.uploadUrl,
                    showCreate: that.options.transport.create,
                    showDelete: that.options.transport.destroy
                }))
                .appendTo(that.element)
                .find(".k-upload input")
                .kendoUpload({
                    multiple: false,
                    localization: {
                        dropFilesHere: messages.dropFilesHere
                    },
                    async: {
                        saveUrl: that.options.transport.uploadUrl,
                        autoUpload: true
                    },
                    upload: proxy(that._fileUpload, that),
                    error: function(e) {
                        that._error({ xhr: e.XMLHttpRequest, status: "error" });
                    }
                }).end();

            that.upload = that.toolbar
                .find(".k-upload input")
                .data("kendoUpload");

            that.arrangeBy = that.toolbar.find(".k-tiles-arrange select")
                .kendoDropDownList({
                    dataSource: arrangeBy,
                    dataTextField: "text",
                    dataValueField: "value",
                    change: function() {
                        that.orderBy(this.value());
                    }
                })
                .data("kendoDropDownList");

            that._attachDropzoneEvents();
        },

        _attachDropzoneEvents: function() {
            var that = this;

            if (that.options.transport.uploadUrl) {
                bindDragEventWrappers($(document.documentElement),
                    $.proxy(that._dropEnter, that),
                    $.proxy(that._dropLeave, that)
                );
                that._scrollHandler = proxy(that._positionDropzone, that);
            }
        },

        _dropEnter: function() {
            this._positionDropzone();
            $(document).on("scroll" + NS, this._scrollHandler);
        },

        _dropLeave: function() {
            this._removeDropzone();
            $(document).off("scroll" + NS, this._scrollHandler);
        },

        _positionDropzone: function() {
            var that = this,
                element = that.element,
                offset = element.offset();

            that.toolbar.find(".k-dropzone")
                .addClass("k-filebrowser-dropzone")
                .offset(offset)
                .css({
                    width: element[0].clientWidth,
                    height: element[0].clientHeight,
                    lineHeight: element[0].clientHeight + "px"
                });
        },

        _removeDropzone: function() {
            this.toolbar.find(".k-dropzone")
                .removeClass("k-filebrowser-dropzone")
                .css({ width: "", height: "", lineHeight: "", top: "", left: "" });
        },

        _deleteClick: function() {
            var that = this,
                item = that.listView.select(),
                message = kendo.format(that.options.messages.deleteFile, item.find("strong").text());

            if (item.length && that._showMessage(message, "confirm")) {
                that.listView.remove(item);
            }
        },

        _addClick: function() {
            this.createDirectory();
        },

        _getFieldName: function(name) {
            return fieldName(this.dataSource.reader.model.fields, name);
        },

        _fileUpload: function(e) {
            var that = this,
                options = that.options,
                fileTypes = options.fileTypes,
                filterRegExp = new RegExp(("(" + fileTypes.split(",").join(")|(") + ")").replace(/\*\./g , ".*."), "i"),
                fileName = e.files[0].name,
                fileNameField = NAMEFIELD,
                sizeField = SIZEFIELD,
                model;

            if (filterRegExp.test(fileName)) {
                e.data = { path: that.path() };

                model = that._createFile(fileName);

                if (!model) {
                    e.preventDefault();
                } else {
                    that.upload.one("success", function(e) {
                        model.set(fileNameField, e.response[that._getFieldName(fileNameField)]);
                        model.set(sizeField, e.response[that._getFieldName(sizeField)]);
                        that._tiles = that.listView.items().filter("[" + kendo.attr("type") + "=f]");
                    });
                }
            } else {
                e.preventDefault();
                that._showMessage(kendo.format(options.messages.invalidFileType, fileName, fileTypes));
            }
        },

        _findFile: function(name) {
            var data = this.dataSource.data(),
                idx,
                result,
                typeField = TYPEFIELD,
                nameField = NAMEFIELD,
                length;

            name = name.toLowerCase();

            for (idx = 0, length = data.length; idx < length; idx++) {
                if (data[idx].get(typeField) === "f" &&
                    data[idx].get(nameField).toLowerCase() === name) {

                    result = data[idx];
                    break;
                }
            }
            return result;
        },

        _createFile: function(fileName) {
            var that = this,
                idx,
                length,
                index = 0,
                model = {},
                typeField = TYPEFIELD,
                view = that.dataSource.view(),
                file = that._findFile(fileName);

            if (file && !that._showMessage(kendo.format(that.options.messages.overwriteFile, fileName), "confirm")) {
                return null;
            }

            if (file) {
                return file;
            }

            for (idx = 0, length = view.length; idx < length; idx++) {
                if (view[idx].get(typeField) === "f") {
                    index = idx;
                    break;
                }
            }

            model[typeField] = "f";
            model[NAMEFIELD] = fileName;
            model[SIZEFIELD] = 0;

            return that.dataSource.insert(++index, model);
        },

        createDirectory: function() {
            var that = this,
                idx,
                length,
                lastDirectoryIdx = 0,
                typeField = TYPEFIELD,
                nameField = NAMEFIELD,
                view = that.dataSource.data(),
                name = that._nameDirectory(),
                model = new that.dataSource.reader.model();

            for (idx = 0, length = view.length; idx < length; idx++) {
                if (view[idx].get(typeField) === "d") {
                    lastDirectoryIdx = idx;
                }
            }

            model.set(typeField, "d");
            model.set(nameField, name);

            that.listView.one("dataBound", function() {
                var selected = that.listView.items()
                    .filter("[" + kendo.attr("uid") + "=" + model.uid + "]"),
                    input = selected.find("input");

                if (selected.length) {
                    this.edit(selected);
                }

                this.element.scrollTop(selected.attr("offsetTop") - this.element[0].offsetHeight);

                setTimeout(function() {
                    input.select();
                });
            })
            .one("save", function(e) {
                var value = e.model.get(nameField);

                if (!value) {
                    e.model.set(nameField, name);
                } else {
                    e.model.set(nameField, that._nameExists(value, model.uid) ? that._nameDirectory() : value);
                }
            });

            that.dataSource.insert(++lastDirectoryIdx, model);
        },

        _directoryKeyDown: function(e) {
            if (e.keyCode == 13) {
                e.currentTarget.blur();
            }
        },

        _directoryBlur: function() {
            this.listView.save();
        },

        _nameExists: function(name, uid) {
            var data = this.dataSource.data(),
                typeField = TYPEFIELD,
                nameField = NAMEFIELD,
                idx,
                length;

            for (idx = 0, length = data.length; idx < length; idx++) {
                if (data[idx].get(typeField) === "d" &&
                    data[idx].get(nameField).toLowerCase() === name.toLowerCase() &&
                    data[idx].uid !== uid) {
                    return true;
                }
            }
            return false;
        },

        _nameDirectory: function() {
            var name = "New folder",
                data = this.dataSource.data(),
                directoryNames = [],
                typeField = TYPEFIELD,
                nameField = NAMEFIELD,
                candidate,
                idx,
                length;

            for (idx = 0, length = data.length; idx < length; idx++) {
                if (data[idx].get(typeField) === "d" && data[idx].get(nameField).toLowerCase().indexOf(name.toLowerCase()) > -1) {
                    directoryNames.push(data[idx].get(nameField));
                }
            }

            if ($.inArray(name, directoryNames) > -1) {
                idx = 2;

                do {
                    candidate = name + " (" + idx + ")";
                    idx++;
                } while ($.inArray(candidate, directoryNames) > -1);

                name = candidate;
            }

            return name;
        },

        orderBy: function(field) {
            this.dataSource.sort([
                { field: TYPEFIELD, dir: "asc" },
                { field: field, dir: "asc" }
            ]);
        },

        search: function(name) {
            this.dataSource.filter({
                field: NAMEFIELD,
                operator: "contains",
                value: name
            });
        },

        _content: function() {
            var that = this;

            that.list = $('<ul class="k-reset k-floats k-tiles" />')
                .appendTo(that.element)
                .on("dblclick" + NS, "li", proxy(that._dblClick, that));

            that.listView = new kendo.ui.ListView(that.list, {
                dataSource: that.dataSource,
                template: that._itemTmpl(),
                editTemplate: that._editTmpl(),
                selectable: true,
                autoBind: false,
                dataBinding: function(e) {
                    that.toolbar.find(".k-delete").parent().addClass("k-state-disabled");

                    if (e.action === "remove" || e.action === "sync") {
                        e.preventDefault();
                    }
                },
                dataBound: function() {
                    if (that.dataSource.view().length) {
                        that._tiles = this.items().filter("[" + kendo.attr("type") + "=f]");
                    } else {
                        this.wrapper.append(EMPTYTILE({ text: that.options.messages.emptyFolder }));
                    }
                },
                change: proxy(that._listViewChange, that)
            });
        },

        _dblClick: function(e) {
            var that = this,
                li = $(e.currentTarget);

            if (li.hasClass("k-edit-item")) {
                that._directoryBlur();
            }

            if (li.filter("[" + kendo.attr("type") + "=d]").length) {
                var folder = that.dataSource.getByUid(li.attr(kendo.attr("uid")));
                if (folder) {
                    that.path(concatPaths(that.path(), folder.get(NAMEFIELD)));
                    that.breadcrumbs.value(that.path());
                }
            } else if (li.filter("[" + kendo.attr("type") + "=f]").length) {
                that.trigger(APPLY);
            }
        },

        _listViewChange: function() {
            var selected = this._selectedItem();

            if (selected) {
                this.toolbar.find(".k-delete").parent().removeClass("k-state-disabled");

                if (selected.get(TYPEFIELD) === "f") {
                    this.trigger(CHANGE);
                }
            }
        },

        _dataSource: function() {
            var that = this,
                options = that.options,
                transport = options.transport,
                typeSortOrder = extend({}, DEFAULTSORTORDER),
                nameSortOrder = { field: NAMEFIELD, dir: "asc" },
                schema,
                dataSource = {
                    type: transport.type || "filebrowser",
                    sort: [typeSortOrder, nameSortOrder]
                };

            if (isPlainObject(transport)) {
                transport.path = proxy(that.path, that);
                dataSource.transport = transport;
            }

            if (isPlainObject(options.schema)) {
                dataSource.schema = options.schema;
            } else if (transport.type && isPlainObject(kendo.data.schemas[transport.type])) {
                schema = kendo.data.schemas[transport.type];
            }

            if (that.dataSource && that._errorHandler) {
                that.dataSource.unbind(ERROR, that._errorHandler);
            } else {
                that._errorHandler = proxy(that._error, that);
            }

            that.dataSource = kendo.data.DataSource.create(dataSource)
                .bind(ERROR, that._errorHandler);
        },

        _navigation: function() {
            var that = this,
                navigation = $('<div class="k-floatwrap"><input/><input/></div>')
                    .appendTo(this.element);

            that.breadcrumbs = navigation.find("input:first")
                    .kendoBreadcrumbs({
                        value: that.options.path,
                        change: function() {
                            that.path(this.value());
                        }
                    }).data("kendoBreadcrumbs");

            that.searchBox = navigation.parent().find("input:last")
                    .kendoSearchBox({
                        label: that.options.messages.search,
                        change: function() {
                            that.search(this.value());
                        }
                    }).data("kendoSearchBox");
        },

        _error: function(e) {
            var that = this,
                status;

            if (!that.trigger(ERROR, e)) {
                status = e.xhr.status;

                if (e.status == 'error') {
                    if (status == '404') {
                        that._showMessage(that.options.messages.directoryNotFound);
                    } else if (status != '0') {
                        that._showMessage('Error! The requested URL returned ' + status + ' - ' + e.xhr.statusText);
                    }
                } else if (status == 'timeout') {
                    that._showMessage('Error! Server timeout.');
                }
            }
        },

        _showMessage: function(message, type) {
            return window[type || "alert"](message);
        },

        refresh: function() {
            var that = this;
            that._navigation();
            that._toolbar();
            that._content();
        },

        _editTmpl: function() {
            var html = '<li class="k-tile k-state-selected" ' + kendo.attr("uid") + '="#=uid#" ';

            html += kendo.attr("type") + '="${' + TYPEFIELD + '}">';
            html += '#if(' + TYPEFIELD + ' == "d") { #';
            html += '<div class="k-thumb"><span class="k-icon k-folder"></span></div>';
            html += "#}else{#";
            html += '<div class="k-thumb"><span class="k-icon k-loading"></span></div>';
            html += "#}#";
            html += '#if(' + TYPEFIELD + ' == "d") { #';
            html += '<input class="k-input" ' + kendo.attr("bind") + '="value:' + NAMEFIELD + '"/>';
            html += "#}#";
            html += '</li>';

            return proxy(kendo.template(html), { sizeFormatter: sizeFormatter } );
        },

        _itemTmpl: function() {
            var that = this,
                html = '<li class="k-tile" ' + kendo.attr("uid") + '="#=uid#" ';

            html += kendo.attr("type") + '="${' + TYPEFIELD + '}">';
            html += '#if(' + TYPEFIELD + ' == "d") { #';
            html += '<div class="k-thumb"><span class="k-icon k-folder"></span></div>';
            html += "#}else{#";
            html += '<div class="k-thumb"><span class="k-icon k-file"></span></div>';
            html += "#}#";
            html += '<strong>${' + NAMEFIELD + '}</strong>';
            html += '#if(' + TYPEFIELD + ' == "f") { # <span class="k-filesize">${this.sizeFormatter(' + SIZEFIELD + ')}</span> #}#';
            html += '</li>';

            return proxy(kendo.template(html), { sizeFormatter: sizeFormatter } );
        },

        path: function(value) {
            var that = this,
                path = that._path || "";

            if (value !== undefined) {
                that._path = value.replace(trimSlashesRegExp, "") + "/";
                that.dataSource.read({ path: that._path });
                return;
            }

            if (path) {
                path = path.replace(trimSlashesRegExp, "");
            }

            return path === "/" || path === "" ? "" : (path + "/");
        }
    });

    var SearchBox = Widget.extend({
        init: function(element, options) {
            var that = this;

            options = options || {};

            Widget.fn.init.call(that, element, options);

            if (placeholderSupported) {
                that.element.attr("placeholder", that.options.label);
            }

            that._wrapper();

            that.element
                .on("keydown" + SEARCHBOXNS, proxy(that._keydown, that))
                .on("change" + SEARCHBOXNS, proxy(that._updateValue, that));

            that.wrapper
                .on(CLICK + SEARCHBOXNS, "a", proxy(that._click, that));

            if (!placeholderSupported) {
                that.element.on("focus" + SEARCHBOXNS, proxy(that._focus, that))
                    .on("blur" + SEARCHBOXNS, proxy(that._blur, that));
            }
        },

        options: {
            name: "SearchBox",
            label: "Search",
            value: ""
        },

        events: [ CHANGE ],

        destroy: function() {
            var that = this;

            that.wrapper
                .add(that.element)
                .add(that.label)
                .off(SEARCHBOXNS);

            Widget.fn.destroy.call(that);
        },

        _keydown: function(e) {
            if (e.keyCode === 13) {
                this._updateValue();
            }
        },

        _click: function(e) {
            e.preventDefault();
            this._updateValue();
        },

        _updateValue: function() {
            var that = this,
                value = that.element.val();

            if (value !== that.value()) {
                that.value(value);

                that.trigger(CHANGE);
            }
        },

        _blur: function() {
            this._updateValue();
            this._toggleLabel();
        },

        _toggleLabel: function() {
            if (!placeholderSupported) {
                this.label.toggle(!this.element.val());
            }
        },

        _focus: function() {
            this.label.hide();
        },

        _wrapper: function() {
            var element = this.element,
                wrapper = element.parents(".k-search-wrap");

            element[0].style.width = "";
            element.addClass("k-input");

            if (!wrapper.length) {
                wrapper = element.wrap($('<div class="k-widget k-search-wrap k-textbox"/>')).parent();
                if (!placeholderSupported) {
                    $('<label style="display:block">' + this.options.label + '</label>').insertBefore(element);
                }
                $('<a href="#" class="k-icon k-i-search k-search"/>').appendTo(wrapper);
            }

            this.wrapper = wrapper;
            this.label = wrapper.find(">label");
        },

        value: function(value) {
            var that = this;

            if (value !== undefined) {
                that.options.value = value;
                that.element.val(value);
                that._toggleLabel();
                return;
            }
            return that.options.value;
        }
    });

    var Breadcrumbs = Widget.extend({
        init: function(element, options) {
            var that = this;

            options = options || {};

            Widget.fn.init.call(that, element, options);

            that._wrapper();

            that.wrapper
                .on("focus" + BREADCRUBMSNS, "input", proxy(that._focus, that))
                .on("blur" + BREADCRUBMSNS, "input", proxy(that._blur, that))
                .on("keydown" + BREADCRUBMSNS, "input", proxy(that._keydown, that))
                .on(CLICK + BREADCRUBMSNS, "a.k-i-arrow-n:first", proxy(that._rootClick, that))
                .on(CLICK + BREADCRUBMSNS, "a:not(.k-i-arrow-n)", proxy(that._click, that));

            that.value(that.options.value);
        },

        options: {
            name: "Breadcrumbs",
            gap: 50
        },

        events: [ CHANGE ],

        destroy: function() {
            var that = this;

            Widget.fn.destroy.call(that);

            that.wrapper
                .add(that.wrapper.find("input"))
                .add(that.wrapper.find("a"))
                .off(BREADCRUBMSNS);
        },

        _update: function(val) {
            val = (val || "").charAt(0) === "/" ? val : ("/" + (val || ""));

            if (val !== this.value()) {
                this.value(val);
                this.trigger(CHANGE);
            }
        },

        _click: function(e) {
            e.preventDefault();
            this._update(this._path($(e.target).prevAll("a:not(.k-i-arrow-n)").addBack()));
        },

        _rootClick: function(e) {
            e.preventDefault();
            this._update("");
        },

        _focus: function() {
            var that = this,
                element = that.element;

            that.overlay.hide();
            that.element.val(that.value());

            setTimeout(function() {
               element.select();
            });
        },

        _blur: function() {
            if (this.overlay.is(":visible")) {
                return;
            }

            var that = this,
                element = that.element,
                val = element.val().replace(/\/{2,}/g, "/");

            that.overlay.show();
            element.val("");
            that._update(val);
        },

        _keydown: function(e) {
            var that = this;
            if (e.keyCode === 13) {
                that._blur();

                setTimeout(function() {
                    that.overlay.find("a:first").focus();
                });
            }
        },

        _wrapper: function() {
            var element = this.element,
                wrapper = element.parents(".k-breadcrumbs"),
                overlay;

            element[0].style.width = "";
            element.addClass("k-input");

            if (!wrapper.length) {
                wrapper = element.wrap($('<div class="k-widget k-breadcrumbs k-textbox"/>')).parent();
            }

            overlay = wrapper.find(".k-breadcrumbs-wrap");
            if (!overlay.length) {
                overlay = $('<div class="k-breadcrumbs-wrap"/>').appendTo(wrapper);
            }
            this.wrapper = wrapper;
            this.overlay = overlay;
        },

        refresh: function() {
            var html = "",
                value = this.value(),
                segments,
                segment,
                idx,
                length;

            if (value === undefined || !value.match(/^\//)) {
                value = "/" + (value || "");
            }

            segments = value.split("/");

            for (idx = 0, length = segments.length; idx < length; idx++) {
                segment = segments[idx];
                if (segment) {
                    if (!html) {
                        html += '<a href="#" class="k-icon k-i-arrow-n">root</a>';
                    }
                    html += '<a class="k-link" href="#">' + segments[idx] + '</a>';
                    html += '<span class="k-icon k-i-arrow-e">&gt;</span>';
                }
            }
            this.overlay.empty().append($(html));

            this._adjustSectionWidth();
        },

        _adjustSectionWidth: function() {
            var that = this,
                wrapper = that.wrapper,
                width = wrapper.width() - that.options.gap,
                links = that.overlay.find("a"),
                a;

            links.each(function(index) {
                a = $(this);

                if (a.parent().width() > width) {
                    if (index == links.length - 1) {
                        a.width(width);
                    } else {
                        a.prev().addBack().hide();
                    }
                }
            });
        },

        value: function(val) {
            if (val !== undefined) {
                this._value = val.replace(/\/{2,}/g, "/");
                this.refresh();
                return;
            }
            return this._value;
        },

        _path: function(trail) {
            return "/" + $.map(trail, function(b) {
                return $(b).text();
            }).join("/");
        }
    });

    kendo.ui.plugin(FileBrowser);
    kendo.ui.plugin(Breadcrumbs);
    kendo.ui.plugin(SearchBox);
})(window.kendo.jQuery);

return window.kendo;

}, typeof define == 'function' && define.amd ? define : function(_, f){ f(); });