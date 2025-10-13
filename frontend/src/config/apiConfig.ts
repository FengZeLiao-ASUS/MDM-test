const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

export const apiConfig = {
  baseUrl: API_BASE_URL,
  endpoints: {
    login: `${API_BASE_URL}/auth/login`,
    register: `${API_BASE_URL}/auth/register`,
    devices: `${API_BASE_URL}/devices`,
    policies: `${API_BASE_URL}/policies`,
    deployPolicy: `${API_BASE_URL}/policies/deploy`,
  }
};
