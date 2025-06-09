import React from 'react';
import { Provider } from 'react-redux';
import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { store } from './store/store';

import Layout from './pages/Layout/Layout';
import LogInPage from './pages/LogInPage/LogInPage';
import SignUpPage from './pages/SignUpPage/SignUpPage';

import 'primereact/resources/themes/md-dark-indigo/theme.css';
import 'primeicons/primeicons.css';
import StatusPage from './pages/StatusPage/StatusPage';

const App = () => {
    return (
        <Provider store={store}>
            <BrowserRouter>
                <Routes>
                    <Route element={<Layout />}>
                        <Route path="/login" element={<LogInPage />} />
                        <Route path="/signup" element={<SignUpPage />} />
                        <Route path="/status" element={<StatusPage />} />
                    </Route>
                </Routes>
            </BrowserRouter>
        </Provider>
    );
};

export default App;
