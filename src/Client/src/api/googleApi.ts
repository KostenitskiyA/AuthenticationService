import { getEnvConfig } from '../getEnvConfig';

const { apiUrl } = getEnvConfig();

export const googleLogInQuery = (redirectUrl: string) =>
    (window.location.href = `${apiUrl}/google/login?redirectUrl=${encodeURIComponent(redirectUrl)}`);
