import { configureStore } from '@reduxjs/toolkit';
import { setupListeners } from '@reduxjs/toolkit/query';
import { authenticationApi } from '../api/authenticationApi';

export const store = configureStore({
    reducer: {
        [authenticationApi.reducerPath]: authenticationApi.reducer,
    },
    middleware: (getDefaultMiddleware) => getDefaultMiddleware().concat(authenticationApi.middleware),
});

setupListeners(store.dispatch);
