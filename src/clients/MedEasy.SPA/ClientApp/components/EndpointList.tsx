// A '.tsx' file enables JSX support in the TypeScript compiler, 
// for more information see the following page on the TypeScript wiki:
// https://github.com/Microsoft/TypeScript/wiki/JSX

import * as React from "react";
import { LoadingComponent } from "./LoadingComponent";
import { GenericGetPagedResponse } from "./../restObjects/GenericGetPagedResponse";
import { LinkHeader } from "./../restObjects/LinkHeader";
import { Link } from "react-router-dom";
import { PageRestResponseLink } from "./../restObjects/PagedRestResponseLink";
import * as LinQ from "linq";
import { Endpoint } from "./../restObjects/Endpoint";
import { Form } from "./../restObjects/Form";
import { PageOfResult } from "./../restObjects/PageOfResult";
import { BrowsableResource } from "./../restObjects/BrowsableResource";
import { Guid } from "./../System/Guid";


/**
 * Properties of the EndpointList component.
 */
export interface EndpointListProps<TResource extends BrowsableResource<MedEasy.DTO.Resource<string>>> {
    /** resources */
    resourceName: {
        singular: string,
        plural: string
    };
    /** Number of items */
    count?: number;

    /** URLs to perform CRUD operations */
    urls: {
        create?: string,
        read: string,
        delete?: string,
        update?: string
    }

    /** Properties to display for each item of the list */
    columns: Map<string, (f: TResource) => any>;
}

interface EndpointListState<TResource extends BrowsableResource<MedEasy.DTO.Resource<string>>> {
    results: Array<TResource>;
    count?: number;
    loading: boolean;
}


/**
 * Displays a list of resources in a Table.
 * 
 */
export class EndpointList<TResource extends BrowsableResource<MedEasy.DTO.Resource<string>>> extends React.Component<EndpointListProps<TResource>, EndpointListState<TResource>> {

    /** Default COUNT */
    public static readonly DEFAULT_COUNT: number = 10;


    /**
     * Builds a new {EndpointList} instance
     * @param {EndpointListProps<TResource>} props component's properties
     */
    public constructor(props: EndpointListProps<TResource>) {
        super(props);
        this.state = {
            results: [], loading: true, count: this.props.count || EndpointList.DEFAULT_COUNT
        };

       
       
    }


    public componentDidMount() : void{
        this.loadData(this.state.count || EndpointList.DEFAULT_COUNT);
    }

    /**
     * Loads
     */
    private async loadData(count: number): Promise<void> {
        //this.setState({ items: [], page: this.state.page, pageSize: this.state.pageSize, loading: true })
        let response : Response = await fetch(`${this.props.urls.read}?count=${count}`);
        
        if (response.ok) {
            let results = await ( response.json() as Promise<Array<TResource>>);
            let newState: EndpointListState<TResource> = { results: results, count: count, loading: false };
            console.debug("New state", newState);
            this.setState(newState);
        } else {
            this.setState({  })
        }
    }

    private async deleteItem(id: string): Promise<void> {
        if (this.props.urls.delete) {
            let response: Response = await fetch(`${this.props.urls.delete}/${id}`, { method : "DELETE" });
            if (response.ok) {
                await this.loadData(this.state.count || EndpointList.DEFAULT_COUNT);
            }
        }
    }



    private async addItem() {

        let data;

    }

    private renderTr(item: TResource, index: number) {
        let { columns, urls } = this.props;

        let cells: Array<JSX.Element> = [];
        
        for (const [key, value] of columns) {
            let val = value(item);
            cells.push(<td key={`${key}-${index}`}>{val}</td>);
        }

        if (urls.delete || urls.update) {
            cells.push(
                <td>
                    <button className='btn btn-danger' onClick={async (event) => {
                        await this.deleteItem(item.resource.id);
                    }}>
                        <span className='glyphicon glyphicon-delete' aria-hidden="true"></span>&nbsp;Delete
                    </button>
                </td>)
        }

        return (
            <tr key={item.resource.id}>
                {cells}
            </tr>
        );
    }


    public render(): JSX.Element {
        let { columns, count, resourceName, urls } = this.props;
        let { results } = this.state;

        let containerId = `tbl-${resourceName.plural}-container`;

        // building headers
        let headers: Array<JSX.Element> = [];
        for (const [key, val] of columns) {
            headers.push(<th key={key}>{key}</th>)
        }
        if (urls.delete || urls.update) {
            headers.push(<th key={Guid.newGuid().toString()}></th>)
        }

        // building tbody
        let tbodyContent: JSX.Element | Array<JSX.Element>;
        if (this.state.loading) {
            tbodyContent = (
                <tr>
                    <td colSpan={columns.size} className='center'><LoadingComponent /></td>
                </tr>
            );
        } else {
            if (results.length === 0) {
                tbodyContent = (
                    <tr>
                        <td colSpan={columns.size} className='center'>No data</td>
                    </tr>
                );
            } else {
                tbodyContent = results.map((item, index) => this.renderTr(item, index));
            }
        }


        return (
            <div id={containerId} name={containerId} ref={containerId}>
                <h1>{resourceName.plural}</h1>
                <div className="row">
                    {this.props.urls.create
                        ? (
                            <div className="row">
                                <button className='btn btn-success' onClick={async (event) => await this.addItem()}>
                                    <span className='glyphicon glyphicon-plus' aria-hidden='true'></span>&nbsp;Add
                                </button>
                            </div>
                        )
                        : null
                    }
                    <div className="row">
                        <table className="table-condensed table" id={`tbl-${resourceName.plural}`} ref={`tbl-${resourceName.plural}`}>
                            <thead>
                                <tr>
                                    {headers}
                                </tr>
                            </thead>
                            <tbody>
                                {tbodyContent}
                            </tbody>

                        </table>
                    </div>
                </div>
            </div>
        );
    }

}