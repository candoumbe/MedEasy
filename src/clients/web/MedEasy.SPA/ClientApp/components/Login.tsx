import * as React from 'react';
import { Link, Redirect, NavLink } from 'react-router-dom';
import { Form } from '../restObjects/Form';
import { FormField } from '../restObjects/FormField';
import { AuthenticationService } from './../services/AuthenticationService';
import { LoginFailedException } from './../System/Exceptions/LoginFailedException';
import { BaseAuthenticatedComponentProps } from './BaseAuthenticatedComponent';
import { FormComponent } from './FormComponent';
import { Button, InputGroup } from 'react-bootstrap';

/**
 * Renders a form to log into the application
 */

interface LoginFormState {
    username: string;
    password: string;
    errors?: { [name: string]: string },

    isBusy?: boolean;
    isConnected?: boolean;
}

interface LoginFormProps extends BaseAuthenticatedComponentProps {
    authService: AuthenticationService
}

export class LoginForm extends React.Component<LoginFormProps, LoginFormState>{

    private form: Form;

    public constructor(props: LoginFormProps) {
        super(props);
        this.state = {
            username: "",
            password: "",
            isBusy: false,
            isConnected: false
        };

        let fields: Array<FormField> = [
            { label: "Email", name: "username", type: "string", required: true },
            { label: "Password", name: "password", type: "string", required: true, secret: true }
        ];

        this.form = {
            items: fields,
            meta: { relation: 'login-form', method: "POST", href: this.props.authService.url }
        }
    }

    private isValid: () => boolean = () => {

        let username: string | undefined = this.state.username || "";
        let password: string | undefined = this.state.password || "";

        return username.trim().length > 0
            && password.trim().length > 0;

    };

    public render(): JSX.Element {

        let component: JSX.Element;

        if (this.state.isConnected) {
            component = <Redirect to='/home' />
        } else {
            let submit: React.EventHandler<React.FormEvent<HTMLFormElement>> = async (event) => {
                event.preventDefault();
                let errorMessage: string;
                this.setState((prevState, props) => {
                    let newState = Object.assign<LoginFormState, LoginFormState>({ username: prevState.username, password: prevState.password }, prevState);
                    newState.isBusy = true;
                    newState.errors = {};

                });
                try {
                    this.props.authService.login(this.state.username, this.state.password)
                        .then(() => {
                            console.trace(`${this.state.username} logged in`);
                            console.trace(`${this.state.username} will navigate to home page`);

                            this.setState({ isConnected: true });
                        });

                } catch (e) {
                    if (e instanceof LoginFailedException) {
                        this.setState((prevState, props) => {
                            let newState: LoginFormState = Object.assign<LoginFormState, LoginFormState>({ username: prevState.username, password: prevState.password }, prevState);
                            newState.isBusy = false;
                            newState.errors = Object.assign({ "": errorMessage }, e.errors);

                            return newState;
                        });

                    }
                }
            }
            let handleChange: (name: string, value: any) => void = (name, value) => {
                this.setState((prevState, props) => {
                    let newState = Object.assign({}, prevState);
                    newState[name] = value;
                    return newState;
                });
            };

            component = (
                <FormComponent form={this.form} handleSubmit={submit}
                    onChange={handleChange} errors={this.state.errors}>

                    <nav className='center-block'>
                        <Button as="submit" bsStyle="primary" disabled={!this.isValid() || this.state.isBusy}>
                            <span className="glyphicon glyphicon-enter" aria-hidden="true"></span>&nbsp;Sign in
                        </Button>
                        <span> or </span>
                        <Link to={"/sign-up"} replace>create an account</Link>
                    </nav>
                </FormComponent>
            );
        }
        return component;
    }

}
