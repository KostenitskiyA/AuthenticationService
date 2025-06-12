import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { getEnvConfig } from '../env-config';

export const authenticationApi = createApi({
    reducerPath: 'authenticationApi',
    baseQuery: fetchBaseQuery({ baseUrl: `${getEnvConfig().apiBaseUrl}/authentication/` }),
    endpoints: (build) => ({
        signUp: build.query<void, { request: ISignUpRequest; redirectUrl: string }>({
            query: (data) => ({
                url: `signup?redirectUrl=${data.redirectUrl}`,
                method: 'POST',
                body: data.request,
            }),
        }),
        logIn: build.query<void, { request: ILogInRequest; redirectUrl: string }>({
            query: (data) => ({
                url: `login?redirectUrl=${data.redirectUrl}`,
                method: 'POST',
                body: data.request,
            }),
        }),
    }),
});

export const { useLazySignUpQuery, useLazyLogInQuery } = authenticationApi;
