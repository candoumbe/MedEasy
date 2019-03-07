import { createBrowserHistory } from 'history';
import * as React from 'react';
import { Route, RouteComponentProps, Switch, withRouter } from 'react-router-dom';
import { Home } from './components/Home';
import { Layout } from './components/Layout';
import { LoginForm } from './components/Login';
import { NotFoundComponent } from './components/NotFoundComponent';
import { PatientCreatePage } from './components/patient/PatientCreatePage';
import { PatientDetails } from './components/patient/PatientDetails';
import { PatientMainPage } from './components/patient/PatientMainPage';
import { SignUpForm } from './components/SignUp';
import { AuthenticationService } from './services/AuthenticationService';
import { RestClient } from "./System/RestClient";

let rootUrl = "https://localhost:51800";
let identityUrl = "https://localhost:51800";
let authService = new AuthenticationService(`${identityUrl}/auth/token`);

let computeDefaultHeaders: () => { [key: string]: string } = () => {
    let defaultHeaders: { [key: string]: string } = { "content-type": "application/json" };

    authService
        .getToken()
        .matchSome((token) => defaultHeaders['Authorization'] = `Bearer ${token.accessToken}`);

    return defaultHeaders;
};
let fnRenewAccessToken: () => Promise<void> = async () => await authService.renew();

const apis = {
    measures: {
        url: "https://localhost:63796/measures/",

    },

    patients: {
        url: "https://localhost:54003",
        client: new RestClient({
            baseUrl: "https://localhost:54003/patients",
            defaultHeaders: computeDefaultHeaders,
            beforeRequestCallback: fnRenewAccessToken,
        })
    },

};
const history = createBrowserHistory();

export const routes = <Layout authService={authService} history={history}>

    <Switch>
        <Route path='/home' exact component={Home} />
        <Route path={'/sign-in'} exact render={(props: RouteComponentProps<any>) => <LoginForm authService={authService} history={history} />} />
        <Route path={'/sign-up'} exact render={(props: RouteComponentProps<any>) => <SignUpForm token={`${identityUrl}/auth/token`} accountEndpoint={`${identityUrl}/identity/accounts`} />} />

        <Route path={'/patients/new'} render={(props: RouteComponentProps<any>) => <PatientCreatePage endpoint={apis.patients.url} restClient={apis.patients.client} />} />
        <Route path='/patients/details/:id' render={(props: RouteComponentProps<any>) => <PatientDetails
            restClient={apis.patients.client}
            measuresEndpoint={apis.measures.url}
            id={props.match.params.id} />} />
        <Route path='/patients/edit/:id' render={({ match }) => <PatientDetails measuresEndpoint={apis.measures.url} restClient={apis.patients.client} id={match.params.id} />} />
        <Route exact path='/patients' render={(props: any) => {

            return <PatientMainPage restClient={apis.patients.client} />;

        }} />
        <Route render={() => <NotFoundComponent text={"Page non trouvée"} />} />
    </Switch>

</Layout>;