import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useLazyLogInQuery } from '../../api/authenticationApi';
import { googleLogInQuery } from '../../api/googleApi';
import { InputText } from 'primereact/inputtext';
import { Button } from 'primereact/button';
import styles from './LogInPage.module.scss';

const LogInPage = () => {
    const navigate = useNavigate();

    const [logIn] = useLazyLogInQuery();
    const [email, setEmail] = useState<string>();
    const [password, setPassword] = useState<string>();

    const onEmailInput = (value: string) => setEmail(value);
    const onPasswordInput = (value: string) => setPassword(value);
    const onLogIn = () => logIn({ email, password });
    const onGoogleLogIn = () => googleLogInQuery();
    const onSignUp = () => navigate(`/signup`);

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
                <Button icon="pi pi-google" label="Login with Google" onClick={onGoogleLogIn} />
                <Button link label="Sign up" onClick={onSignUp} />
            </div>
        </div>
    );
};

export default LogInPage;
