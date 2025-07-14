import React, { useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useLazyDeleteQuery, useLazyLogInQuery, useLazyRefreshQuery } from '../../api/authenticationApi';
import { googleLogInQuery } from '../../api/googleApi';
import { InputText } from 'primereact/inputtext';
import { Button } from 'primereact/button';
import styles from './LogInPage.module.scss';

const LogInPage = () => {
    const navigate = useNavigate();
    const [searchParams] = useSearchParams();
    const redirectURL = searchParams.get('redirectURL');

    const [logIn] = useLazyLogInQuery();
    const [refresh] = useLazyRefreshQuery();
    const [deleteUser] = useLazyDeleteQuery();
    const [email, setEmail] = useState<string>();
    const [password, setPassword] = useState<string>();

    const onEmailInput = (value: string) => setEmail(value);
    const onPasswordInput = (value: string) => setPassword(value);

    const onLogIn = async () => {
        const result = await logIn({ email, password });
        if (result.isSuccess) window.location.href = redirectURL;
    };
    const onGoogleLogIn = () => googleLogInQuery(redirectURL);
    const onSignUp = () => navigate(`/signup`);
    const onDelete = async () => await deleteUser();
    const onRefresh = async () => await refresh();

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
            <div className={styles.ButtonControl}>
                <Button label="Delete" onClick={onDelete} />
                <Button label="Refresh" onClick={onRefresh} />
            </div>
        </div>
    );
};

export default LogInPage;
