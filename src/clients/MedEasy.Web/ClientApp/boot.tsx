import './css/site.css';

import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { Router } from 'react-router';
import routes from './routes';
import createBrowserHistory from "history/createBrowserHistory";


// This code starts up the React app when it runs in a browser. It sets up the routing configuration
// and injects the app into a DOM element.

const history = createBrowserHistory();
ReactDOM.render(
    <Router history={ history } children={ routes } />,
    document.getElementById('react-app')
);
