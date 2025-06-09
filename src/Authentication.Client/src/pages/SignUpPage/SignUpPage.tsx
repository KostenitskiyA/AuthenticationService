import React, { useState } from 'react';
import { useLazySignUpQuery } from '../../api/authenticationApi';
import { InputText } from 'primereact/inputtext';
import { Button } from 'primereact/button';

import styles from './SignUpPage.module.scss';

const SignUpPage = () => {
    const [signUp] = useLazySignUpQuery();
    const [name, setName] = useState<string>();
    const [email, setEmail] = useState<string>();
    const [password, setPassword] = useState<string>();

    const onNameInput = (value: string) => {
        setName(value);
    };

    const onEmailInput = (value: string) => {
        setEmail(value);
    };

    const onPasswordInput = (value: string) => {
        setPassword(value);
    };

    const onSubmit = async () => {
        await signUp({ name, email, password });
    };

    return (
        <div className={styles.SignUpForm}>
            <div className={styles.InputControl}>
                <label htmlFor="">Name</label>
                <InputText id="name" type="text" value={name} onChange={(event) => onNameInput(event.target.value)} />
            </div>
            <div className={styles.InputControl}>
                <label htmlFor="email">Username</label>
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
                <Button label="Sign up" onClick={onSubmit} />
            </div>
        </div>
    );
};

export default SignUpPage;
