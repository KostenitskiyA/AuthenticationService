import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { getEnvConfig } from '../env-config';

export const authenticationApi = createApi({
    reducerPath: 'authenticationApi',
    baseQuery: fetchBaseQuery({ baseUrl: `${getEnvConfig().apiBaseUrl}/authentication/` }),
    endpoints: (build) => ({
        logIn: build.query<void, ILogInRequest>({
            query: (data) => ({
                url: `login?redirectUrl=${getEnvConfig().clientBaseUrl}/status/`,
                method: 'POST',
                body: data,
            }),
        }),
        signUp: build.query<void, ISignUpRequest>({
            query: (data) => ({
                url: `signup?redirectUrl=${getEnvConfig().clientBaseUrl}/status/`,
                method: 'POST',
                body: data,
            }),
        }),
        logInGoogle: build.query<void, void>({
            query: () => ({
                url: `login-google?redirectUrl=${getEnvConfig().clientBaseUrl}/status/`,
                method: 'GET',
            }),
        }),
    }),
});

export const { useLazyLogInQuery, useLazySignUpQuery, useLazyLogInGoogleQuery } = authenticationApi;
