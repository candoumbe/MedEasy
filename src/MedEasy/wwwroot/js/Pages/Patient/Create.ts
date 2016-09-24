/// <reference path="../../system/string.ts" />
/// <reference path="../../../typings/kendo-ui/kendo-ui.d.ts" />
namespace MedEasy.Pages.Patient
{

    /**
     * Contains all JavaScript methods which are related to the Patient/Create view
     */
    export class CreatePage  implements IPage<CreatePageElementMap>
    {
        private _map: CreatePageElementMap;

        
        public get map(): CreatePageElementMap
        {
            if (!this._map)
            {
                this._map = new CreatePageElementMap();
            }

            return this._map;
        }

        /**
         * Returns an object which contains one property "term" which holds the
         * 
         */
        public get doctorName(): { term: string }
        {
            return {
                term: this._map.TbxMainDoctor.val()
            };
        }


        public static init(): void
        {
            let page: CreatePage = new CreatePage();
            let tbxMainDoctor = page.map.TbxMainDoctor
            tbxMainDoctor.on("change", (e) =>
            {
                let source = $(e.target)
                source.val((<string>source.val()).toTitleCase());
            });
        }
    }

    $(document).ready(() =>
    {
        CreatePage.init();
    });


    export class CreatePageElementMap implements IPageElementMap
    {


        /**  */
        public get TbxMainDoctor(): JQuery
        {
            return $("#MainDoctorId");
        }
    }


    
}