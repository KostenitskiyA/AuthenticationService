import React, { useState } from 'react';
import { useLazyLogInGoogleQuery, useLazyLogInQuery } from '../../api/authenticationApi';
import { InputText } from 'primereact/inputtext';
import { Button } from 'primereact/button';

import styles from './LogInPage.module.scss';

const LogInPage = () => {
    const [logIn] = useLazyLogInQuery();
    const [logInGoogle] = useLazyLogInGoogleQuery();
    const [email, setEmail] = useState<string>();
    const [password, setPassword] = useState<string>();

    const onEmailInput = (value: string) => setEmail(value);

    const onPasswordInput = (value: string) => setPassword(value);

    const onSubmit = () => logIn({ email, password });

    const onGoogleLogIn = () => logInGoogle();

    return (
        <div className={styles.LogInForm}>
            <div className={styles.InputControl}>
                <label htmlFor="email">Username</label>
                <InputText type="email" value={email} onChange={(event) => onEmailInput(event.target.value)} />
            </div>
            <div className={styles.InputControl}>
                <label htmlFor="password">Password</label>
                <InputText type="password" value={password} onChange={(event) => onPasswordInput(event.target.value)} />
            </div>
            <div className={styles.ButtonControl}>
                <Button label="Log in" onClick={onSubmit} />
                <Button label="Sign up" onClick={onSubmit} />
                <div className={styles.InputControl}>
                    <Button icon="pi pi-google" label="Login with Google" onClick={onGoogleLogIn} />
                </div>
            </div>
        </div>
    );
};

export default LogInPage;
