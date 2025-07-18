import React from 'react';
import { Provider } from 'react-redux';
import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { store } from './store/store';
import { NotificationProvider } from './components/NotificationProvider';
import Layout from './pages/Layout/Layout';
import SignUpPage from './pages/SignUpPage/SignUpPage';
import LogInPage from './pages/LogInPage/LogInPage';
import 'primereact/resources/themes/lara-dark-indigo/theme.css';
import 'primeicons/primeicons.css';

const App = () => {
    return (
        <Provider store={store}>
            <NotificationProvider>
                <BrowserRouter>
                    <Routes>
                        <Route element={<Layout />}>
                            <Route path="/" element={<LogInPage />} />
                            <Route path="/signup" element={<SignUpPage />} />
                            <Route path="/login" element={<LogInPage />} />
                        </Route>
                    </Routes>
                </BrowserRouter>
            </NotificationProvider>
        </Provider>
    );
};

export default App;
