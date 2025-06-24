import { getEnvConfig } from '../env-config';

export const googleLogInQuery = () =>
    (window.location.href = `${getEnvConfig().apiBaseUrl}/authentication/google/login`);
