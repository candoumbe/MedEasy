import * as React from 'react';
import { Router, Route, Switch } from 'react-router';
import * as Fetch from "isomorphic-fetch";
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { FetchData } from './components/FetchData';
import { PatientMainPage } from './components/Patient/PatientMainPage';
import { PatientCreatePage } from './components/Patient/PatientCreatePage';
import { Counter } from './components/Counter';
import { Endpoint } from './restObjects/Endpoint';

const apiUrl = "http://localhost:5000/";

const routes =
    <Switch>
        <Route exact path='/' render={() => <Layout body={<Home />} />} />
        <Route exact path='/counter' render={() => <Layout body={<Counter />} />} />
        <Route exact path='/fetchdata' render={() => <Layout body={<FetchData />} />} />

        <Route exact path='/patients' render={() => <Layout body={<PatientMainPage />} />} />
        <Route exact path="/patients/new" render={() => <Layout body={<PatientCreatePage endpoint={apiUrl}  />} />} />
        <Route exact path='/patients' render={() => <Layout body={<PatientMainPage />} />} />
        <Route exact path="/patients/new" render={() => <Layout body={<PatientCreatePage endpoint={apiUrl} />} />} />

    </Switch >;

export default routes;

// Allow Hot Module Reloading
declare var module: any;
if (module.hot) {
    module.hot.accept();
}
