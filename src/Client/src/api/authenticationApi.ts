import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react';
import { getEnvConfig } from '../getEnvConfig';

const { apiUrl } = getEnvConfig();

export const authenticationApi = createApi({
    reducerPath: 'authenticationApi',
    baseQuery: fetchBaseQuery({
        baseUrl: `${apiUrl}/authentication`,
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
        refresh: build.query<void, void>({
            query: () => ({
                url: `refresh`,
                method: 'POST',
            }),
        }),
        delete: build.query<void, void>({
            query: () => ({
                url: `delete`,
                method: 'DELETE',
            }),
        }),
    }),
});

export const { useLazySignUpQuery, useLazyLogInQuery, useLazyRefreshQuery, useLazyDeleteQuery } = authenticationApi;
