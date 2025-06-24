import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { getEnvConfig } from '../env-config';

export const authenticationApi = createApi({
    reducerPath: 'authenticationApi',
    baseQuery: fetchBaseQuery({
        baseUrl: `${getEnvConfig().apiBaseUrl}/authentication`,
        credentials: 'include',
    }),
    endpoints: (build) => ({
        signUp: build.query<void, ISignUpRequest>({
            query: (data) => ({
                url: `signup`,
                method: 'POST',
                body: data,
            }),
        }),
        logIn: build.query<void, ILogInRequest>({
            query: (data) => ({
                url: `login`,
                method: 'POST',
                body: data,
            }),
        }),
    }),
});

export const { useLazySignUpQuery, useLazyLogInQuery } = authenticationApi;
