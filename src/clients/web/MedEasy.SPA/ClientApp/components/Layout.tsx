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



export class Layout extends BaseAuthenticatedComponent<LayoutProps, { connected: boolean}> {


    public constructor(props: LayoutProps) {
        super(props)
        this.state = { connected: false };
    }

    public shouldComponentUpdate(nextProps: Readonly<LayoutProps>, nextState: Readonly<{ connected: boolean }>, nextContext: any) {
        return this.props.authService.isConnected() != this.state.connected;
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
            : <LoginForm authService={this.props.authService} history={this.props.history}/>
    }
}
