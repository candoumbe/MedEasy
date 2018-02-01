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

const apis = {
    
    measures: { root: "http://localhost:63794/measures"},
    patients: { root: "http://localhost:54004"}
};
export const routes = <Layout>
    <Route exact path='/' component={Home} />
    <Route path='/fetchdata' component={FetchData} />
    <Switch>
        <Route path={'/patients/new'} render={(props: any) => <PatientCreatePage endpoint={`${apis.patients.root}`} />} />
        <Route path='/patients/details/:id' render={({ match }) => <PatientDetails endpoint={`${apis.patients.root}/patients/${match.params.id}`} />} />
        <Route path='/patients/edit/:id' render={({ match }) => <PatientDetails endpoint={`${apis.patients.root}/patients/${match.params.id}`} />} />
        <Route exact path='/patients' render={(props: any) => <PatientMainPage endpoint={`${apis.patients.root}`} />} />
        <Route render ={() => <NotFoundComponent  />} />
    </Switch>
</Layout>;
