export const getEnvConfig = () => {
    return envConfig[process.env.NODE_ENV as keyof typeof envConfig];
};

export const envConfig = {
    development: {
        clientBaseUrl: 'http://localhost:5173',
        apiBaseUrl: 'http://localhost:5104/api',
    },
};
