/*
* Kendo UI v2015.1.408 (http://www.telerik.com/kendo-ui)
* Copyright 2015 Telerik AD. All rights reserved.
*
* Kendo UI commercial licenses may be obtained at
* http://www.telerik.com/purchase/license-agreement/kendo-ui-complete
* If you do not own a commercial license, this file shall be governed by the trial license terms.
*/
(function(f, define){
    define([ "./kendo.core" ], f);
})(function(){

(function(kendo) {
    function Node() {
        this.node = null;
    }

    Node.prototype = {
        remove: function() {
            this.node.parentNode.removeChild(this.node);
        },
        attr: {}
    };

    function NullNode() {
    }

    NullNode.prototype = {
        nodeName: "#null",
        attr: { style: {} },
        children: [],
        remove: function() {
        }
    };

    var NULL_NODE = new NullNode();

    function Element(nodeName, attr, children) {
        this.nodeName = nodeName;

        this.attr = attr || {};

        this.children = children || [];
    }

    Element.prototype = new Node();

    Element.prototype.appendTo = function(parent) {
        var node = document.createElement(this.nodeName);

        var children = this.children;

        for (var index = 0; index < children.length; index++) {
            children[index].render(node, NULL_NODE);
        }

        parent.appendChild(node);

        return node;
    };

    Element.prototype.render = function(parent, cached) {
        var node;

        if (cached.nodeName !== this.nodeName) {
            cached.remove();

            node = this.appendTo(parent);
        } else {
            node = cached.node;

            var index;

            var children = this.children;

            var length = children.length;

            var cachedChildren = cached.children;

            var cachedLength = cachedChildren.length;

            if (Math.abs(cachedLength - length) > 2) {
                this.render({
                    appendChild: function(node) {
                        parent.replaceChild(node, cached.node);
                    }
                }, NULL_NODE);

                return;
            }

            for (index = 0; index < length; index++) {
                children[index].render(node, cachedChildren[index] || NULL_NODE);
            }

            for (index = length; index < cachedLength; index++) {
                cachedChildren[index].remove();
            }
        }

        this.node = node;

        this.syncAttributes(cached.attr);

        this.removeAttributes(cached.attr);
    };

    Element.prototype.syncAttributes = function(cachedAttr) {
        var attr = this.attr;

        for (var name in attr) {
            var value = attr[name];

            var cachedValue = cachedAttr[name];

            if (name === "style") {
                this.setStyle(value, cachedValue);
            } else if (value !== cachedValue) {
                this.setAttribute(name, value, cachedValue);
            }
        }
    };

    Element.prototype.setStyle = function(style, cachedValue) {
        var node = this.node;
        var key;

        if (cachedValue) {
            for (key in style) {
                if (style[key] !== cachedValue[key]) {
                    node.style[key] = style[key];
                }
            }
        } else {
            for (key in style) {
                node.style[key] = style[key];
            }
        }
    };

    Element.prototype.removeStyle = function(cachedStyle) {
        var style = this.attr.style || {};
        var node = this.node;

        for (var key in cachedStyle) {
            if (style[key] === undefined) {
                node.style[key] = "";
            }
        }
    };

    Element.prototype.removeAttributes = function(cachedAttr) {
        var attr = this.attr;

        for (var name in cachedAttr) {
            if (name === "style") {
                this.removeStyle(cachedAttr.style);
            } else if (attr[name] === undefined) {
                this.removeAttribute(name);
            }
        }
    };

    Element.prototype.removeAttribute = function(name) {
        var node = this.node;

        if (name === "style") {
            node.style.cssText = "";
        } else if (name === "className") {
            node.className = "";
        } else {
            node.removeAttribute(name);
        }
    };

    Element.prototype.setAttribute = function(name, value, cachedValue) {
        var node = this.node;

        if (node[name] !== undefined) {
            node[name] = value;
        } else {
            node.setAttribute(name, value);
        }
    };

    function TextNode(nodeValue) {
        this.nodeValue = nodeValue;
    }

    TextNode.prototype = new Node();

    TextNode.prototype.nodeName = "#text";

    TextNode.prototype.render = function(parent, cached) {
        var node;

        if (cached.nodeName !== this.nodeName) {
            cached.remove();

            node = document.createTextNode(this.nodeValue);

            parent.appendChild(node);
        } else {
            node = cached.node;

            if (this.nodeValue !== cached.nodeValue) {
                node.nodeValue = this.nodeValue;
            }
        }

        this.node = node;
    };

    function HtmlNode(html) {
        this.html = html;
    }

    HtmlNode.prototype = {
       nodeName: "#html",
       attr: {},
       remove: function() {
           for (var index = 0; index < this.nodes.length; index++) {
               this.nodes[index].parentNode.removeChild(this.nodes[index]);
           }
       },
       render: function(parent, cached) {
           if (cached.nodeName !== this.nodeName || cached.html !== this.html) {
               cached.remove();

               var lastChild = parent.lastChild;

               parent.insertAdjacentHTML("beforeend", this.html);

               this.nodes = [];

               for (var child = lastChild ? lastChild.nextSibling : parent.firstChild; child; child = child.nextSibling) {
                   this.nodes.push(child);
               }
           } else {
               this.nodes = cached.nodes.slice(0);
           }
       }
    };

    function html(value) {
        return new HtmlNode(value);
    }

    function element(nodeName, attrs, children) {
        return new Element(nodeName, attrs, children);
    }

    function text(value) {
        return new TextNode(value);
    }

    function Tree(root) {
       this.root = root;
       this.children = [];
    }

    Tree.prototype = {
        html: html,
        element: element,
        text: text,
        render: function(children) {
            var cachedChildren = this.children;

            var index;

            var length;

            for (index = 0, length = children.length; index < length; index++) {
               children[index].render(this.root, cachedChildren[index] || NULL_NODE);
            }

            for (index = length; index < cachedChildren.length; index++) {
                cachedChildren[index].remove();
            }

            this.children = children;
        }
    };

    kendo.dom = {
        html: html,
        text: text,
        element: element,
        Tree: Tree
    };
})(window.kendo);

return window.kendo;

}, typeof define == 'function' && define.amd ? define : function(_, f){ f(); });