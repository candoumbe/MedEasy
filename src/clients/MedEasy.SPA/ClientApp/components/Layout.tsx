import * as React from 'react';
import { NavMenu } from './NavMenu';
import { AuthenticationService } from './../services/AuthenticationService';
import { BaseAuthenticatedComponent, BaseAuthenticatedComponentProps } from './BaseAuthenticatedComponent';
import { LoginForm } from './Login';

export interface LayoutProps extends BaseAuthenticatedComponentProps {
    children?: React.ReactNode;
    authService : AuthenticationService
}

export class Layout extends BaseAuthenticatedComponent<LayoutProps, {}> {


    public constructor(props: LayoutProps) {
        super(props)
    }

    public render() : JSX.Element {

        let component : JSX.Element;
        if (this.isLoggedIn()) {
            return (<div className='container-fluid'>
                <div className='row'>
                    <div className='col-sm-3'>
                        <NavMenu authService={this.props.authService}/>
                    </div>
                    <div className='col-sm-9'>
                        {this.props.children}
                    </div>
                </div>
            </div>
            )
        } else {
            return <LoginForm authService={this.props.authService} history={this.props.history}/>;
        }

    }
}
