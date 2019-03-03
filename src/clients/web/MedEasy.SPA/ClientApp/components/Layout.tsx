import * as React from 'react';
import { NavMenu } from './NavMenu';
import { AuthenticationService } from './../services/AuthenticationService';
import { BaseAuthenticatedComponent, BaseAuthenticatedComponentProps } from './BaseAuthenticatedComponent';
import { LoginForm } from './Login';
import { Redirect } from 'react-router';
import { Row, Col } from 'react-bootstrap';

export interface LayoutProps extends BaseAuthenticatedComponentProps {
    children?: React.ReactNode;
    authService: AuthenticationService
}



export class Layout extends BaseAuthenticatedComponent<LayoutProps, {}> {


    public constructor(props: LayoutProps) {
        super(props)
    }

    public render(): any {

        return this.props.authService.isConnected()
            ? (<div className='container-fluid'>
                <Row>
                    <Col sm={3}>
                        <NavMenu authService={this.props.authService} />
                    </Col>
                    <Col sm={9}>
                        {this.props.children}
                    </Col>
                </Row>
            </div>
            )
            : this.props.children
    }
}
