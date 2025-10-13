export interface User {
  id: number;
  username: string;
  email: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  success: boolean;
  message: string;
  accessToken?: string;
  user?: User;
}

export interface Device {
  id: string;
  deviceName: string;
  operatingSystem: string;
  osVersion: string;
  complianceState: string;
  managementAgent: string;
  lastSyncDateTime?: string;
  userPrincipalName: string;
}

export interface Policy {
  id: number;
  name: string;
  description: string;
  policyType: string;
  configuration?: Record<string, any>;
  createdAt: string;
}

export interface PolicyRequest {
  name: string;
  description: string;
  policyType: string;
  configuration: Record<string, any>;
}

export interface DeployPolicyRequest {
  policyId: number;
  targetGroup: string;
  parameters: Record<string, string>;
}

export interface DeployPolicyResponse {
  success: boolean;
  message: string;
  intuneApplicationId?: string;
  intunewinFilePath?: string;
}
