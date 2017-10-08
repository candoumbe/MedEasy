import * as React from 'react';
import { Route, Switch } from 'react-router-dom';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { FetchData } from './components/FetchData';
import { Counter } from './components/Counter';
import { PatientMainPage } from './components/patient/PatientMainPage';
import { PatientDetails } from './components/patient/PatientDetails';
import { PatientCreatePage } from './components/patient/PatientCreatePage';
import { NotFoundComponent } from './components/NotFoundComponent';

const apiRoot = 'http://localhost:5000';
export const routes = <Layout>
    <Route exact path='/' component={Home} />
    <Route path='/counter' component={Counter} />
    <Route path='/fetchdata' component={FetchData} />
    <Switch>
        <Route path={'/patients/new'} render={(props: any) => < PatientCreatePage endpoint={`${apiRoot}`} />} />
        <Route path='/patients/details/:id' render={({ match }) => <PatientDetails endpoint={`${apiRoot}/api/patients/${match.params.id}`} />} />
        <Route path='/patients/edit/:id' render={({ match }) => <PatientDetails endpoint={`${apiRoot}/api/patients/${match.params.id}`} />} />
        <Route exact path='/patients' render={(props: any) => <PatientMainPage endpoint={`${apiRoot}/api`} />} />
        <Route render ={() => <NotFoundComponent  />} />
    </Switch>
</Layout>;
