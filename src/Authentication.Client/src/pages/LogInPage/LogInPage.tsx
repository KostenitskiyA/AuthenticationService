import React, { useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useLazyLogInQuery } from '../../api/authenticationApi';
import { googleLogInQuery } from '../../api/googleApi';
import { InputText } from 'primereact/inputtext';
import { Button } from 'primereact/button';
import styles from './LogInPage.module.scss';

const LogInPage = () => {
    const location = useLocation();
    const navigate = useNavigate();

    const queryParams = new URLSearchParams(location.search);
    const redirectUrl = queryParams.get('redirectUrl');

    const [logIn] = useLazyLogInQuery();
    const [email, setEmail] = useState<string>();
    const [password, setPassword] = useState<string>();

    const onEmailInput = (value: string) => setEmail(value);
    const onPasswordInput = (value: string) => setPassword(value);
    const onLogIn = () => logIn({ request: { email, password }, redirectUrl });
    const onGoogleLogIn = () => googleLogInQuery(redirectUrl);
    const onSignUp = () => navigate('/signup');

    return (
        <div className={styles.LogInForm}>
            <div className={styles.InputControl}>
                <label htmlFor="email">Email</label>
                <InputText type="email" value={email} onChange={(event) => onEmailInput(event.target.value)} />
            </div>
            <div className={styles.InputControl}>
                <label htmlFor="password">Password</label>
                <InputText type="password" value={password} onChange={(event) => onPasswordInput(event.target.value)} />
            </div>
            <div className={styles.ButtonControl}>
                <Button label="Log in" onClick={onLogIn} />
                <div className={styles.InputControl}>
                    <Button icon="pi pi-google" label="Login with Google" onClick={onGoogleLogIn} />
                </div>
                <Button link label="Sign up" onClick={onSignUp} />
            </div>
        </div>
    );
};

export default LogInPage;
