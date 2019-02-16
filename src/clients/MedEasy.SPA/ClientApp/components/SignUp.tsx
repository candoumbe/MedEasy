import * as React from 'react';

import { FormComponent } from './FormComponent';
import { Form } from '../restObjects/Form';
import { FormField } from '../restObjects/FormField';
import { BaseAuthenticatedComponent, BaseAuthenticatedComponentProps } from './BaseAuthenticatedComponent';
import { Link } from 'react-router-dom';
import { AuthenticationService } from './../services/AuthenticationService';
import { AccountService } from './../services/AccountService';
import { SignUpFailedException } from './../System/Exceptions/SignUpException';
import { Exception } from './../System/Exceptions/Exception';
import { Guid } from './../System/Guid';

/**
 * Renders a form to log into the application
 */

interface SignUpFormState {
    name: string;
    username: string;
    password: string;
    confirmPassword: string;
    email: string;
    errors?: { [name: string]: string }
    isBusy: boolean,
    tenantId: Guid
}

interface SignUpFormProps extends BaseAuthenticatedComponentProps {
    token: string;
    /** URL to the accounts endpoint */
    accountEndpoint: string;
}

export class SignUpForm extends BaseAuthenticatedComponent<SignUpFormProps, SignUpFormState>{

    private readonly authService: AuthenticationService;
    private readonly accountService: AccountService;

    public constructor(props: SignUpFormProps) {
        super(props);
        this.state = {
            name: "Test",
            username: "",
            email: "",
            password: "",
            confirmPassword: "",
            tenantId: Guid.newGuid(),
            isBusy: false
        };

        this.authService = new AuthenticationService(this.props.token);
        this.accountService = new AccountService(this.props.accountEndpoint, this.props.token);
    }

    private isValid(): boolean {
        return this.state.username && this.state.username.trim().length > 0
            && this.state.password && this.state.password.trim().length > 0
            && this.state.confirmPassword && this.state.confirmPassword.trim().length > 0
            && this.state.confirmPassword && this.state.confirmPassword.trim().length > 0
            && this.state.password == this.state.confirmPassword
            ;
    }


    public render(): JSX.Element {

        let fields: Array<FormField> = [
            { label: "Name", name: "name", type: "string", required: true },
            { label: "Username", name: "username", type: "string", required: true },
            { label: "Email", name: "email", type: "email", required: true },
            { label: "Password", name: "password", type: "string", required: true, secret: true },
            { label: "Confirm password", name: "confirmPassword", type: "string", required: true, secret: true },
            { label: "Tenant id", name: "tenantId", type: "string", required: true, secret: false, value : String(this.state.tenantId)}
        ];

        let form: Form = {
            items: fields,
            meta: { relation: 'create-form', method: "POST", href: this.props.token }
        }

        let submit: React.EventHandler<React.FormEvent<HTMLFormElement>> = async (event) => {
            event.preventDefault();

            try {
                this.setState((prevState) => {
                    let defaultState: SignUpFormState = {
                        name: "",
                        username: "",
                        password: "",
                        email: "",
                        confirmPassword: "",
                        isBusy: false,
                        tenantId : Guid.newGuid()
                    }
                    let newState = Object.assign<SignUpFormState, SignUpFormState>(defaultState, prevState);
                    newState.isBusy = true;
                    return newState
                });
                let token = await this.accountService.signUp(this.state);

                console.trace(`received token : '${token}'`);

                this.props.history.replace('/');
            } catch (e) {
                let exception: Exception = e;
                if (exception instanceof SignUpFailedException) {
                    this.setState((prevState) => {
                        let defaultState: SignUpFormState = {
                            name: prevState.name,
                            username: prevState.username,
                            password: prevState.password,
                            email: prevState.email,
                            confirmPassword: prevState.confirmPassword,
                            tenantId : prevState.tenantId,
                            isBusy: false,
                            errors : e.errors
                        }
                        let newState = Object.assign<SignUpFormState, SignUpFormState>(defaultState, prevState);
                        newState.isBusy = true;
                        return newState
                    });
                }
            } finally {
                this.setState((prevState) => {
                    let defaultState: SignUpFormState = {
                        name: prevState.name || "",
                        username: prevState.username || "",
                        password: prevState.password || "",
                        email: prevState.email || "",
                        confirmPassword: prevState.confirmPassword || "",
                        tenantId: prevState.tenantId || Guid.newGuid(),
                        isBusy: false,
                        errors: prevState.errors
                    };
                    let newState = Object.assign<SignUpFormState, SignUpFormState>(defaultState, prevState);
                    newState.isBusy = false;
                    return newState
                });
            }


        };

        let handleChange: (name: string, value: any) => void = (name, value) => {
            this.setState((prevState, props) => {
                let newState = Object.assign({}, prevState);
                newState[name] = value;
                return newState;
            });
        };
        let formComponent = <FormComponent form={form} handleSubmit={submit}
            onChange={handleChange}
            errors={this.state.errors}>

            <nav className='center-block'>
                <button type="submit" className="btn btn-primary btn-xs-12 btn-sm-6" disabled={!this.isValid()}>
                    <span className="glyphicon glyphicon-enter"></span>&nbsp;Sign up
                </button>
                <span> or </span>
                <Link to={"/sign-in"}>I have a account</Link>
            </nav>
        </FormComponent>
        return formComponent;
    }

}