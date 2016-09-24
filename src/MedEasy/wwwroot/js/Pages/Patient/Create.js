/// <reference path="../../system/string.ts" />
/// <reference path="../../../typings/kendo-ui/kendo-ui.d.ts" />
var MedEasy;
(function (MedEasy) {
    var Pages;
    (function (Pages) {
        var Patient;
        (function (Patient) {
            /**
             * Contains all JavaScript methods which are related to the Patient/Create view
             */
            var CreatePage = (function () {
                function CreatePage() {
                }
                Object.defineProperty(CreatePage.prototype, "map", {
                    get: function () {
                        if (!this._map) {
                            this._map = new CreatePageElementMap();
                        }
                        return this._map;
                    },
                    enumerable: true,
                    configurable: true
                });
                Object.defineProperty(CreatePage.prototype, "doctorName", {
                    /**
                     * Returns an object which contains one property "term" which holds the
                     *
                     */
                    get: function () {
                        return {
                            term: this._map.TbxMainDoctor.val()
                        };
                    },
                    enumerable: true,
                    configurable: true
                });
                CreatePage.init = function () {
                    var page = new CreatePage();
                    var tbxMainDoctor = page.map.TbxMainDoctor;
                    tbxMainDoctor.on("change", function (e) {
                        var source = $(e.target);
                        source.val(source.val().toTitleCase());
                    });
                };
                return CreatePage;
            }());
            Patient.CreatePage = CreatePage;
            $(document).ready(function () {
                CreatePage.init();
            });
            var CreatePageElementMap = (function () {
                function CreatePageElementMap() {
                }
                Object.defineProperty(CreatePageElementMap.prototype, "TbxMainDoctor", {
                    /**  */
                    get: function () {
                        return $("#MainDoctorId");
                    },
                    enumerable: true,
                    configurable: true
                });
                return CreatePageElementMap;
            }());
            Patient.CreatePageElementMap = CreatePageElementMap;
        })(Patient = Pages.Patient || (Pages.Patient = {}));
    })(Pages = MedEasy.Pages || (MedEasy.Pages = {}));
})(MedEasy || (MedEasy = {}));
//# sourceMappingURL=Create.js.map