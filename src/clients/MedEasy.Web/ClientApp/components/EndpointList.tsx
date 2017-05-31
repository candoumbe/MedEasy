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


/**
 * Properties of the {EndpoinL} component.
 */
export interface EndpointListProps<TResource extends MedEasy.DTO.Resource<string>> {
    /** Datasource url */
    endpoint: string;
    /** resources */
    resourceName: {
        singular: string,
        plural: string
    };
    /** Number of items */
    pageSize: number;

    /** Indicate if a new resource can be created */
    canCreate: boolean;

    /** Properties to display for each item of the list */
    columns: Map<string, (f: TResource) => any>;
}

interface EndpointListState<TResource extends MedEasy.DTO.Resource<string>> {
    pageOfResult: PageOfResult<TResource>;
    page: number;
    pageSize: number;
    loading: boolean;
}


/**
 * Displays a list of resources in a Table.
 * 
 */
export class EndpointList<TResource extends MedEasy.DTO.Resource<string>> extends React.Component<EndpointListProps<TResource>, EndpointListState<TResource>> {

    /**
     * Builds a new {EndpointList} instance
     * @param {EndpointListProps<TResource>} props component's properties
     */
    public constructor(props: EndpointListProps<TResource>) {
        super(props);
        this.state = {
            pageOfResult: new PageOfResult<TResource>([], new PageRestResponseLink("#", "#"), 0), page: 1, pageSize: props.pageSize, loading: true
        };
        this.loadData(1);
    }

    /**
     * Loads
     */
    private loadData(page: number): void {
        //this.setState({ items: [], page: this.state.page, pageSize: this.state.pageSize, loading: true })
        fetch(`${this.props.endpoint}/api/${this.props.resourceName.plural}?page=${page}&pageSize=${this.state.pageSize}`)
            .then((response) => response.json() as Promise<PageOfResult<TResource>>)
            .then((pageOfResult) => {
                //let linkHeader: string = response.headers.has("Link")
                //    ? response.headers.get("Link") as string
                //    : `<${this.props.endpoint}/patients>;rel=first`

                //let links: LinQ.IEnumerable<Link> = LinQ.from(LinkHeader.read(linkHeader));

                //let firstPageLink = links.singleOrDefault(x => x.rel === "first", );
                //let nextPageLink = links.singleOrDefault(x => x.rel === "next");
                //let previousPageLink = links.singleOrDefault(x => x.rel === "previous");
                //let lastPageLink = links.single(x => x.rel === "last");

                //let pageLinks = new PageRestResponseLink(firstPageLink, lastPageLink, previousPageLink, nextPageLink);
                console.debug("Rendering list of items",
                    { pageOfResult: pageOfResult, page: this.state.page, pageSize: this.state.pageSize, loading: false }
                );
                this.setState({ pageOfResult: pageOfResult, page: this.state.page, pageSize: this.state.pageSize, loading: false });
            });
    }


    private renderTr(item: TResource) {
        let {columns} = this.props;

        let cells: Array<JSX.Element> = [];
        for (const [key, value] of columns) {
            let val = value(item);
            cells.push(<td key={val || item}>{val}</td>);
        }

        return (
            <tr key={item.id}>
                {cells}
            </tr>
        );
    }


    public render(): JSX.Element {
        let {columns, pageSize, resourceName} = this.props;
        let {pageOfResult} = this.state;
        let {first, previous, next, last} = pageOfResult.links;
        let lastPageNumber: number = Math.ceil(pageOfResult.count / pageSize);
        let pageIndexes: Array<number> = [];
        let i = 2;
        while (i < lastPageNumber) {
            pageIndexes.push(i);
            i++;
        }

        let containerId = `tbl-${resourceName.plural}-container`;
        let headers: Array<JSX.Element> = [];
        for (const [key, val] of columns) {
            headers.push(<th key={key}>{key}</th>)
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
                                {
                                    this.state.loading
                                        ? <tr><td colSpan={columns.size} className='center'><LoadingComponent /></td></tr>
                                        : this.state.pageOfResult.items.map((item) => this.renderTr(item))
                                }
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
                                onClick={() => this.setState({ pageOfResult: new PageOfResult<TResource>([], new PageRestResponseLink("#", "#"), 0), page: 1, pageSize: this.state.pageSize, loading: true })}>
                                <span aria-hidden="true">&laquo;</span>
                            </button>
                            {
                                pageIndexes.forEach((i) => {
                                    <button
                                        className="btn btn-default"
                                        onClick={() => this.setState({
                                            pageOfResult: new PageOfResult<TResource>([], new PageRestResponseLink("#", "#"), 0),
                                            page: 1,
                                            pageSize: this.state.pageSize,
                                            loading: true
                                        })}>
                                        <span aria-hidden="true">&laquo;</span>
                                    </button>
                                })}
                            <button
                                className="btn btn-default"
                                disabled={lastPageNumber > 1}
                                title={last.title}
                                aria-label={lastPageNumber}
                                onClick={() => this.setState({
                                    pageOfResult: new PageOfResult<TResource>([], new PageRestResponseLink("#", "#"), 0),
                                    page: lastPageNumber,
                                    pageSize: this.state.pageSize,
                                    loading: true
                                })}>
                                <span aria-hidden="true">&laquo;</span>
                            </button>

                        </li>
                    </ul>
                </nav>


            </div>
        );
    }

}