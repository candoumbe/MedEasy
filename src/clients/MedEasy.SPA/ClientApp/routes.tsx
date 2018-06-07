import * as React from 'react';
import { Route, Switch } from 'react-router-dom';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { FetchData } from './components/FetchData';
//import { Counter } from './components/Counter';
import { PatientMainPage } from './components/patient/PatientMainPage';
import { PatientDetails } from './components/patient/PatientDetails';
import { PatientCreatePage } from './components/patient/PatientCreatePage';
import { NotFoundComponent } from './components/NotFoundComponent';

const api = "http://localhost:8500";
export const routes = <Layout>
    <Route exact path='/' component={Home} />
    <Route path='/fetchdata' component={FetchData} />
    <Switch>
        <Route path={'/patients/new'} render={(props: any) => <PatientCreatePage endpoint={api} />} />
        <Route path='/patients/details/:id' render={({ match }) => <PatientDetails endpoint={api} id={match.params.id} />} />
        <Route path='/patients/edit/:id' render={({ match }) => <PatientDetails endpoint={api} id={match.params.id} />} />
        <Route exact path='/patients' render={(props: any) => <PatientMainPage endpoint={api} />} />
        <Route render ={() => <NotFoundComponent  />} />
    </Switch>
</Layout>;
