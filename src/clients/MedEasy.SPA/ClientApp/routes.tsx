import * as React from 'react';
import { Route } from 'react-router-dom';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { FetchData } from './components/FetchData';
import { Counter } from './components/Counter';
import { PatientMainPage } from './components/patient/PatientMainPage';

const apiRoot = 'http://localhost:5000/api';
export const routes = <Layout>
    <Route exact path='/' component={ Home } />
    <Route path='/counter' component={ Counter } />
    <Route path='/fetchdata' component={FetchData} />
    <Route path='/patients' render={(props : any) => <PatientMainPage endpoint={apiRoot} /> } />
</Layout>; 
