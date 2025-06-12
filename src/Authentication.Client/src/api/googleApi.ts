import { getEnvConfig } from '../env-config';

export const googleLogInQuery = (redirectUrl: string) =>
    (window.location.href = `${getEnvConfig().apiBaseUrl}/authentication/google/login?redirectUrl=${redirectUrl}`);
