import * as React from "react";
import { LoadingComponent } from "./LoadingComponent";
import { ErrorComponent } from "./ErrorComponent";
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
 * Properties of the EndpointPageOfData component.
 */
export interface EndpointPageOfDataProps<TResource extends BrowsableResource<MedEasy.DTO.Resource<string>>> {
    /** Datasource url */
    endpoint: string;
    /** Function to get access to the resource identifier */
    id: (item: TResource) => string;

    /** resources */
    resourceName: {
        collection?: string
        singular: string,
        plural: string
    };
    /** Number of items */
    pageSize?: number;

    /** Indicate if a new resource can be created */
    canCreate: boolean;

    /** Properties to display for each item of the list */
    columns: Map<string, (f: TResource) => any>;
    error?: boolean
}

interface EndpointPageOfDataState<TResource extends BrowsableResource<MedEasy.DTO.Resource<string>>> {
    pageOfResult: PageOfResult<TResource>;
    page: number;
    pageSize?: number;
    loading: boolean;
    error?: boolean
}


/**
 * Displays a list of resources in a Table.
 * 
 */
export class EndpointPageOfData<TResource extends BrowsableResource<MedEasy.DTO.Resource<string>>> extends React.Component<EndpointPageOfDataProps<TResource>, EndpointPageOfDataState<TResource>> {

    /**
     * Builds a new {EndpointPageOfData} instance
     * @param {EndpointPageOfDataProps<TResource>} props component's properties
     */
    public constructor(props: EndpointPageOfDataProps<TResource>) {
        super(props);
        this.state = {
            pageOfResult: PageOfResult.empty, page: 1, pageSize: props.pageSize, loading: true
        };
        this.loadData(1).then(() => console.log("loaded"));
    }

    /**
     * Loads
     */
    private async loadData(page: number): Promise<void> {
        //this.setState({ items: [], page: this.state.page, pageSize: this.state.pageSize, loading: true })
        let response: Response = await fetch(`${this.props.endpoint}?page=${page}&pageSize=${this.state.pageSize}`);
        if (response.ok) {
            let pageOfResult: PageOfResult<TResource> = await (response.json() as Promise<PageOfResult<TResource>>);
            let newState: EndpointPageOfDataState<TResource> = {
                pageOfResult: pageOfResult,
                page: this.state.page,
                pageSize: this.state.pageSize,
                loading: false
            };

            this.setState(newState);
        } else {
            this.setState({
                pageOfResult: PageOfResult.empty,
                page: this.state.page,
                pageSize: this.state.pageSize,
                error: true,
                loading: false
            })
        }
    }


    private renderTr(item: TResource, index: number) {
        let { columns } = this.props;

        let cells: Array<JSX.Element> = [];
        for (const [key, value] of columns) {
            let val = value(item);
            cells.push(<td key={`${key}-${index}`}>{val}</td>);
        }

        return (
            <tr key={item.resource.id}>
                {cells}
            </tr>
        );
    }


    public render(): JSX.Element {
        let { columns, pageSize, resourceName } = this.props;
        let { pageOfResult } = this.state;
        let { first, previous, next, last } = pageOfResult.links;
        let lastPageNumber: number = pageSize
            ? Math.ceil(pageOfResult.count / pageSize)
            : 1;
        let pageIndexes: Array<number> = [];
        let i = 2;
        while (i < lastPageNumber) {
            pageIndexes.push(i);
            i++;
        }

        let containerId = `tbl-${resourceName.plural}-container`;

        // building headers
        let headers: Array<JSX.Element> = [];
        for (const [key, val] of columns) {
            headers.push(<th key={key}>{key}</th>)
        }

        // building tbody
        let tbodyContent: JSX.Element | Array<JSX.Element>;
        if (this.state.loading) {
            tbodyContent = (
                <tr>
                    <td colSpan={columns.size} className='center'><LoadingComponent /></td>
                </tr>
            );
        }
        else if (this.state.error) {
            tbodyContent = (
                <tr>
                    <td colSpan={columns.size} className='center'><ErrorComponent /></td>
                </tr>
            );
        } else {
            if (this.state.pageOfResult.count === 0) {
                tbodyContent = (
                    <tr>
                        <td colSpan={columns.size} className='center'>No data</td>
                    </tr>
                );
            } else {
                tbodyContent = this.state.pageOfResult.items.map((item, index) => this.renderTr(item, index));
            }
        }


        return (
            <div id={containerId} ref={containerId}>
                <h1>{resourceName.plural}</h1>
                <div className="row">
                    {
                        this.props.canCreate
                            ? <div>
                                <Link to={`/${resourceName.plural}/new`} className="btn btn-primary">
                                    <span className="glyphicon glyphicon-plus" aria-hidden="true"></span>&nbsp;Create
                                </Link>
                            </div>
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
                        <div>
                            <button className="btn btn-defaut" type="button" onClick={() => this.loadData(this.state.page)}>
                                <span className="glyphicon glyphicon-refresh"></span>
                                <span className="sr-only">Refresh</span>
                            </button>
                        </div>
                    </div>
                </div>
                <nav aria-label="Pagination">
                    <ul className="pager">
                        <li>
                            <button className="btn btn-default"
                                title={first.title}
                                rel={first.relation}
                                aria-label="1"
                                onClick={() => this.loadData(1)}>
                                <span aria-hidden="true">&laquo;</span>
                            </button>
                            {
                                pageIndexes.forEach((i) => {
                                    <button
                                        className="btn btn-default"
                                        onClick={() => this.loadData(i)}>
                                        <span aria-hidden="true"></span>&nbsp;{i}
                                    </button>
                                })}
                            <button
                                className="btn btn-default"
                                disabled={lastPageNumber > 1}
                                title={last.title}
                                aria-label={lastPageNumber}
                                onClick={() => this.loadData(lastPageNumber)}>
                                <span aria-hidden="true">&raquo;</span>
                            </button>

                        </li>
                    </ul>
                </nav>


            </div>
        );
    }

}