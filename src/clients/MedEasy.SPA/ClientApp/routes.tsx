import * as React from 'react';
import { Route, Switch, RouteComponentProps } from 'react-router-dom';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { PatientMainPage } from './components/patient/PatientMainPage';
import { PatientDetails } from './components/patient/PatientDetails';
import { PatientCreatePage } from './components/patient/PatientCreatePage';
import { NotFoundComponent } from './components/NotFoundComponent';
import { LoginForm } from './components/Login';
import { SignUpForm } from './components/SignUp';
import { createBrowserHistory, createMemoryHistory } from 'history';
import { AuthenticationService } from './services/AuthenticationService';
import { Fade } from 'react-bootstrap';
import { RestClient } from "./System/RestClient";
import { Browsable } from "./restObjects/Browsable";

const apis = {
    identity: {
        url: "https://localhost:51800"
    },
    measures: {
        url: "http://localhost:63794/measures/"
        
    },
    patients: {
        url: "http://localhost:54001"
    },

};
const history = createBrowserHistory();
let authService = new AuthenticationService(`${apis.identity.url}/auth/token`);
let computeDefaultHeaders: () => { [key: string]: string } = () => {
    let defaultHeaders: { [key: string]: string } = { "content-type": "application/json" };
    authService.getToken().match(
        (token) => { defaultHeaders['Authorization'] = `Bearer ${token.accessToken}` },
        () => { }
    );

    return defaultHeaders;
    
}

export const routes = <Layout authService={authService} history={history}>
    <Fade transitionAppear={true}>
        <Switch>
            <Route path='/home' exact component={Home} />
            <Route path={'/sign-in'} render={(props: RouteComponentProps<any>) => <LoginForm authService={authService} history={history} />} />
            <Route path={'/sign-up'} render={(props: RouteComponentProps<any>) => <SignUpForm token={`${apis.identity.url}/auth/token`} accountEndpoint={`${apis.identity.url}/identity/accounts`} history={history} />} />

            <Route path={'/patients/new'} render={(props: RouteComponentProps<any>) => <PatientCreatePage endpoint={apis.patients.url} />} />
            <Route path='/patients/details/:id' render={(props: RouteComponentProps<any>) => <PatientDetails endpoint={apis.patients.url} id={props.match.params.id} />} />
            <Route path='/patients/edit/:id' render={({ match }) => <PatientDetails endpoint={apis.patients.url} id={match.params.id} />} />
            <Route exact path='/patients' render={(props: any) => {

                let restClient = new RestClient<string, Browsable<MedEasy.DTO.Patient>>(`${apis.measures.url}/patients`, computeDefaultHeaders);
                return <PatientMainPage restClient={restClient} />;

            }} />
            <Route render={() => <NotFoundComponent text={"Page non trouvée"} />} />
        </Switch>
    </Fade>
</Layout>
