import * as React from "react";
import Button from "react-bootstrap/Button";
import Row from "react-bootstrap/Row";
import Table from "react-bootstrap/Table";
import { Link } from "react-router-dom";
import { Browsable } from "./../restObjects/Browsable";
import { PageOfResult } from "./../restObjects/PageOfResult";
import { RestClient } from "./../System/RestClient";
import { ErrorComponent } from "./ErrorComponent";
import { LoadingComponent } from "./LoadingComponent";



/**
 * Properties of the EndpointPageOfData component.
 */
export interface EndpointPageOfDataProps<TResource extends Browsable<MedEasy.DTO.Resource<string>>> {
    /** Datasource url */
    restClient: RestClient;
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

/**
 * 
 */
interface EndpointPageOfDataState<TResource extends Browsable<MedEasy.DTO.Resource<string>>> {
    pageOfResult: PageOfResult<TResource>;
    page: number;
    pageSize?: number;
    loading: boolean;
    error?: boolean;
    showModal?: boolean;
}


/**
 * Displays a list of resources in a Table.
 * 
 */
export class EndpointPageOfData<TResource extends Browsable<MedEasy.DTO.Resource<string>>> extends React.Component<EndpointPageOfDataProps<TResource>, EndpointPageOfDataState<TResource>> {
    private readonly restClient: RestClient;
    /**
     * Builds a new {EndpointPageOfData} instance
     * @param  props component's properties
     * @see {EndpointPageOfDataProps<TResource>}
     */
    public constructor(props: EndpointPageOfDataProps<TResource>) {
        super(props);
        this.state = {
            pageOfResult: PageOfResult.empty,
            page: 1,
            pageSize: props.pageSize,
            loading: true
        };
        this.restClient = this.props.restClient;
        this.loadData(1);
    }

    /**
     * Loads
     */
    private async loadData(page: number): Promise<void> {

        let optionalPage = await this.restClient.get<{ page: number, pageSize: number }, PageOfResult<TResource>>({ page, pageSize: this.state.pageSize });
        await optionalPage.match(
            async (pageOfResult) => {
                let newState: EndpointPageOfDataState<TResource> = {
                    pageOfResult: (await pageOfResult as PageOfResult<TResource>),
                    page: this.state.page,
                    pageSize: this.state.pageSize,
                    loading: false
                };

                this.setState(newState);

            },
            async () => {
                this.setState({
                    pageOfResult: PageOfResult.empty,
                    page: this.state.page,
                    pageSize: this.state.pageSize,
                    error: true,
                    loading: false
                });
            });
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
                <Row>
                    {
                        this.props.canCreate
                            ? <div>
                                <Link to={`/${resourceName.plural}/new`} className="btn btn-primary">
                                    <span className="glyphicon glyphicon-plus" aria-hidden="true"></span>&nbsp;New
                                </Link>
                            </div>
                            : null
                    }
                    <Row>
                        <Table className="table-condensed table" id={`tbl-${resourceName.plural}`} ref={`tbl-${resourceName.plural}`}>
                            <thead>
                                <tr>
                                    {headers}
                                </tr>
                            </thead>
                            <tbody>
                                {tbodyContent}
                            </tbody>

                        </Table>
                        <div>
                            <Button title="Refresh" onClick={() => this.loadData(this.state.page)}>
                                <span className="glyphicon glyphicon-refresh"></span>
                                <span className="sr-only">Refresh</span>
                            </Button>
                        </div>
                    </Row>
                </Row>
                <nav aria-label="Pagination">
                    <ul className="pager">
                        <li>
                            <Button className={`btn btn-default ${pageIndexes.length > 0 ? "" : "hidden"}`}
                                title={first.title}
                                disabled={this.state.page === 1}
                                aria-label="1"
                                onClick={() => this.loadData(1)}>
                                <span aria-hidden="true">&laquo;</span>
                            </Button>
                            {
                                pageIndexes.forEach((i) => {
                                    <Button key={`btn-pager-${i}`}
                                        onClick={() => this.loadData(i)}>
                                        <span aria-hidden="true"></span>&nbsp;{i}
                                    </Button>
                                })
                            }
                        </li>
                    </ul>
                </nav>


            </div>
        );
    }

}