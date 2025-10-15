const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5136/api';

export const apiConfig = {
  baseUrl: API_BASE_URL,
  endpoints: {
    devices: `${API_BASE_URL}/devices`,
    policies: `${API_BASE_URL}/policies`,
    deployPolicy: `${API_BASE_URL}/policies/deploy`,
  }
};
