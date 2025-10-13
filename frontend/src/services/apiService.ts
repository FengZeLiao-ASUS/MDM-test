import axios from 'axios';
import type { AxiosInstance } from 'axios';
import { apiConfig } from '../config/apiConfig';
import type { LoginRequest, LoginResponse, Device, Policy, PolicyRequest, DeployPolicyRequest, DeployPolicyResponse } from '../types';

class ApiService {
  private client: AxiosInstance;

  constructor() {
    this.client = axios.create({
      baseURL: apiConfig.baseUrl,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Add request interceptor to include auth token
    this.client.interceptors.request.use(
      (config) => {
        const token = sessionStorage.getItem('authToken');
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => {
        return Promise.reject(error);
      }
    );
  }

  // Auth endpoints
  async login(credentials: LoginRequest): Promise<LoginResponse> {
    const response = await this.client.post<LoginResponse>('/auth/login', credentials);
    if (response.data.accessToken) {
      sessionStorage.setItem('authToken', response.data.accessToken);
    }
    return response.data;
  }

  async register(data: { username: string; email: string; password: string }): Promise<void> {
    await this.client.post('/auth/register', data);
  }

  // Device endpoints
  async getDevices(): Promise<Device[]> {
    const response = await this.client.get<{ devices: Device[]; totalCount: number }>('/devices');
    return response.data.devices;
  }

  // Policy endpoints
  async getPolicies(): Promise<Policy[]> {
    const response = await this.client.get<Policy[]>('/policies');
    return response.data;
  }

  async getPolicy(id: number): Promise<Policy> {
    const response = await this.client.get<Policy>(`/policies/${id}`);
    return response.data;
  }

  async createPolicy(policy: PolicyRequest): Promise<Policy> {
    const response = await this.client.post<Policy>('/policies', policy);
    return response.data;
  }

  async deletePolicy(id: number): Promise<void> {
    await this.client.delete(`/policies/${id}`);
  }

  async deployPolicy(request: DeployPolicyRequest): Promise<DeployPolicyResponse> {
    const response = await this.client.post<DeployPolicyResponse>('/policies/deploy', request);
    return response.data;
  }
}

export const apiService = new ApiService();
