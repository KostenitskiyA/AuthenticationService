export const getEnvConfig = () => {
    const { MODE, VITE_CLIENT_URL, VITE_API_URL } = import.meta.env;

    return {
        mode: MODE,
        clientUrl: VITE_CLIENT_URL,
        apiUrl: VITE_API_URL,
    };
};
