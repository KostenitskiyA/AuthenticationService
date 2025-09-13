import React, { useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useLazySignUpQuery } from '../../api/authenticationApi';
import { useNotificationContext } from '../../components/NotificationProvider';
import { googleLogInQuery } from '../../api/googleApi';
import { InputText } from 'primereact/inputtext';
import { Button } from 'primereact/button';
import styles from './SignUpPage.module.scss';

const SignUpPage = () => {
    const navigate = useNavigate();
    const notification = useNotificationContext();
    const [searchParams] = useSearchParams();
    const redirectURL = searchParams.get('redirectURL');

    const [signUp] = useLazySignUpQuery();
    const [name, setName] = useState<string>();
    const [email, setEmail] = useState<string>();
    const [password, setPassword] = useState<string>();

    const onNameInput = (value: string) => setName(value);
    const onEmailInput = (value: string) => setEmail(value);
    const onPasswordInput = (value: string) => setPassword(value);

    const onSignUp = async () => {
        const result = await signUp({ name, email, password });
        if (result.isSuccess) {
            notification.addMessage({
                severity: 'error',
                summary: 'Error calling https',
                detail: 'hello',
            });
            //window.location.href = redirectURL;
        }
    };
    const onGoogleLogIn = () => googleLogInQuery(redirectURL);
    const onLogIn = () => navigate(`/login`);

    return (
        <div className={styles.SignUpForm}>
            <div className={styles.InputControl}>
                <label htmlFor="">Name</label>
                <InputText id="name" type="text" value={name} onChange={(event) => onNameInput(event.target.value)} />
            </div>
            <div className={styles.InputControl}>
                <label htmlFor="email">Email</label>
                <InputText
                    id="email"
                    type="email"
                    value={email}
                    onChange={(event) => onEmailInput(event.target.value)}
                />
            </div>
            <div className={styles.InputControl}>
                <label htmlFor="password">Password</label>
                <InputText
                    id="password"
                    type="password"
                    value={password}
                    onChange={(event) => onPasswordInput(event.target.value)}
                />
            </div>
            <div className={styles.ButtonControl}>
                <Button label="Sign up" onClick={onSignUp} />
                <Button icon="pi pi-google" label="Login with Google" onClick={onGoogleLogIn} />
                <Button link label="Log in" onClick={onLogIn} />
            </div>
        </div>
    );
};

export default SignUpPage;
