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
import { Browsable } from "./../restObjects/Browsable";
import { Guid } from "./../System/Guid";
import { Option } from "./../System/Option_Maybe";
import { RestClient } from "./../System/RestClient";
import { Row, Table, Button } from "react-bootstrap";


/**
 * Properties of the EndpointList component.
 */
export interface EndpointListProps<TResource extends Browsable<MedEasy.DTO.Resource<string>>> {
    /** resources */
    resourceName: {
        singular: string,
        plural: string
    };
    /** Number of items */
    count?: number;

    /** URLs to perform CRUD operations */
    capabilities: {
        create?: boolean,
        delete?: boolean,
        update?: boolean
    }

    /** Properties to display for each item of the list */
    columns: Map<string, (f: TResource) => any>;
    restClient: RestClient;
}

interface EndpointListState<TResource extends Browsable<MedEasy.DTO.Resource<string>>> {
    results: Array<TResource>;
    count?: number;
    loading: boolean;
    /** Indicates if the component should display a modal form to add a new resource */
    creating: boolean | undefined
}


/**
 * Displays a list of resources in a Table.
 * 
 */
export class EndpointList<TResource extends Browsable<MedEasy.DTO.Resource<string>>> extends React.Component<EndpointListProps<TResource>, EndpointListState<TResource>> {

    /** Default COUNT */
    public static readonly DEFAULT_COUNT: number = 10;
    

    /**
     * Builds a new {EndpointList} instance
     * @param {EndpointListProps<TResource>} props component's properties
     */
    public constructor(props: EndpointListProps<TResource>) {
        super(props);
        this.state = {
            results: [],
            loading: true,
            count: this.props.count || EndpointList.DEFAULT_COUNT,
            creating: false
        };
        

    }


    public componentDidMount(): void {
        this.loadData(this.state.count || EndpointList.DEFAULT_COUNT);
    }

    /**
     * Loads
     */
    private async loadData(count: number): Promise<void> {
        //this.setState({ items: [], page: this.state.page, pageSize: this.state.pageSize, loading: true })
        let optionalResult = await this.props.restClient.get<{ page: number, pageSize: number }, PageOfResult<Browsable<TResource>>>({ page: 1, pageSize: count });

        optionalResult.match(
            async results => {
                console.trace(await results);
                this.setState({});
            },
            () => this.setState({})
        );
    }

    private async deleteItem(id: string): Promise<void> {
        await this.loadData(this.state.count || EndpointList.DEFAULT_COUNT);
    }



    private async addItem() {

        let data;

    }


    private renderTr(item: TResource, index: number) {
        let { columns, capabilities } = this.props;

        let cells: Array<JSX.Element> = [];

        for (const [key, value] of columns) {
            let val = value(item);
            cells.push(<td key={`${key}-${index}`}>{val}</td>);
        }

        if (capabilities.delete || capabilities.update) {
            cells.push(
                <td>
                    <Button bsStyle='danger' onClick={async (event) => {
                        await this.deleteItem(item.resource.id);
                    }}>
                        <span className='glyphicon glyphicon-delete' aria-hidden="true"></span>&nbsp;Delete
                    </Button>
                </td>)
        }

        return (
            <tr key={item.resource.id}>
                {cells}
            </tr>
        );
    }


    public render(): JSX.Element {
        let { columns, count, resourceName, capabilities } = this.props;
        let { results } = this.state;

        let containerId = `tbl-${resourceName.plural}-container`;

        // building headers
        let headers: Array<JSX.Element> = [];
        for (const [key, val] of columns) {
            headers.push(<th key={key}>{key}</th>)
        }
        if (capabilities.delete || capabilities.update) {
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
            <div>

                <div id={containerId} ref={containerId}>
                    <h1>{resourceName.plural}</h1>
                    <Row>
                        {this.props.capabilities.create
                            ? (
                                <Row>
                                    <Button bsStyle='success' onClick={async (event) => await this.addItem()}>
                                        <span className='glyphicon glyphicon-plus' aria-hidden='true'></span>&nbsp;Add
                                    </Button>
                                </Row>
                            )
                            : null
                        }
                        <Row>
                            <Table condensed id={`tbl-${resourceName.plural}`} ref={`tbl-${resourceName.plural}`}>
                                <thead>
                                    <tr>
                                        {headers}
                                    </tr>
                                </thead>
                                <tbody>
                                    {tbodyContent}
                                </tbody>

                            </Table>
                        </Row>
                    </Row>
                </div>
            </div>
        );
    }

}