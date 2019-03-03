import * as React from 'react';
import { Link, NavLink, Redirect } from 'react-router-dom';
import {
    Button, Navbar, NavbarBrand, NavbarHeader, NavbarCollapse, ListGroup, ListGroupItem, Nav, NavbarToggle
} from "react-bootstrap"
import { AuthenticationService } from './../services/AuthenticationService';

interface NavMenuProps {
    authService: AuthenticationService
}

export class NavMenu extends React.Component<NavMenuProps, { redirectToSignin: boolean }> {

    public constructor(props) {
        super(props);
        this.state = { redirectToSignin: false };
    }

    public render(): JSX.Element {

        let component: JSX.Element;
        if (this.state.redirectToSignin) {
            component = <Redirect to='/sign-in' />;
        } else {


            component = <div className='main-nav'>
                <div className='navbar navbar-inverse'>
                    <div className='navbar-header'>
                        <button type='button' className='navbar-toggle' data-toggle='collapse' data-target='.navbar-collapse'>
                            <span className='sr-only'>Toggle navigation</span>
                            <span className='icon-bar'></span>
                            <span className='icon-bar'></span>
                            <span className='icon-bar'></span>
                        </button>
                        <Link className='navbar-brand' to={'/home'}>MedEasy</Link>
                    </div>
                    <div className='clearfix'></div>
                    <div className='navbar-collapse collapse'>
                        <ul className='nav navbar-nav'>
                            <li>
                                <NavLink to={'/home'} exact activeClassName='active'>
                                    <span className='glyphicon glyphicon-home'></span> Home
                            </NavLink>
                            </li>

                            <li>
                                <NavLink to={'/sign-up'} activeClassName='active'>
                                    <span className='glyphicon glyphicon-home'></span> Sign up
                            </NavLink>
                            </li>
                            <li>
                                <NavLink to={'/patients'} activeClassName='active'>
                                    <span className='glyphicon glyphicon-th-list'></span> Patients
                            </NavLink>
                            </li>
                        </ul>
                    </div>
                    <div className='clearfix'></div>
                    <div className='navbar-footer'>
                        <div className="navbar-footer">
                            <Button bsStyle="danger" onClick={async () => {
                                await this.props.authService.logout();
                                this.setState({ redirectToSignin: true });
                            }}>Disconnect</Button>

                        </div>
                    </div>
                </div>
            </div>;
        }

        return component;
    }
}
