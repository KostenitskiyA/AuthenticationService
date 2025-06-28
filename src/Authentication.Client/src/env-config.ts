export const getEnvConfig = () => {
    return envConfig[process.env.NODE_ENV as keyof typeof envConfig];
};

export const envConfig = {
    development: {
        clientBaseUrl: 'http://localhost:5173',
        apiBaseUrl: 'http://localhost:5000/api',
    },
    staging: {
        clientBaseUrl: 'http://localhost:5173',
        apiBaseUrl: 'http://api:8080/api',
    },
};
