import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { apiService } from '../services/apiService';
import type { Policy, PolicyRequest, DeployPolicyRequest } from '../types';
import './Policies.css';

export const Policies = () => {
  const [policies, setPolicies] = useState<Policy[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [selectedPolicy, setSelectedPolicy] = useState<Policy | null>(null);
  const [deployLoading, setDeployLoading] = useState(false);
  const [deployMessage, setDeployMessage] = useState('');
  const navigate = useNavigate();

  const [newPolicy, setNewPolicy] = useState<PolicyRequest>({
    name: '',
    description: '',
    policyType: 'Application',
    configuration: {},
  });

  useEffect(() => {
    const user = sessionStorage.getItem('user');
    if (!user) {
      navigate('/');
      return;
    }

    loadPolicies();
  }, [navigate]);

  const loadPolicies = async () => {
    try {
      setLoading(true);
      const data = await apiService.getPolicies();
      setPolicies(data);
      setError('');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load policies');
    } finally {
      setLoading(false);
    }
  };

  const handleCreatePolicy = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await apiService.createPolicy(newPolicy);
      setShowCreateForm(false);
      setNewPolicy({
        name: '',
        description: '',
        policyType: 'Application',
        configuration: {},
      });
      loadPolicies();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to create policy');
    }
  };

  const handleDeletePolicy = async (id: number) => {
    if (!confirm('Are you sure you want to delete this policy?')) {
      return;
    }

    try {
      await apiService.deletePolicy(id);
      loadPolicies();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete policy');
    }
  };

  const handleDeployPolicy = async (policy: Policy) => {
    setSelectedPolicy(policy);
    setDeployLoading(true);
    setDeployMessage('');

    try {
      const deployRequest: DeployPolicyRequest = {
        policyId: policy.id,
        targetGroup: 'All Devices',
        parameters: {},
      };

      const response = await apiService.deployPolicy(deployRequest);
      
      if (response.success) {
        setDeployMessage(
          `✓ ${response.message}\n` +
          (response.intunewinFilePath ? `File: ${response.intunewinFilePath}\n` : '') +
          (response.intuneApplicationId ? `App ID: ${response.intuneApplicationId}` : '')
        );
      } else {
        setDeployMessage(`✗ ${response.message}`);
      }
    } catch (err: any) {
      setDeployMessage(`✗ ${err.response?.data?.message || 'Deployment failed'}`);
    } finally {
      setDeployLoading(false);
    }
  };

  const handleLogout = () => {
    sessionStorage.clear();
    navigate('/');
  };

  const user = JSON.parse(sessionStorage.getItem('user') || '{}');

  return (
    <div className="dashboard">
      <header className="dashboard-header">
        <h1>Intune Device Management</h1>
        <div className="user-info">
          <span>Welcome, {user.username}</span>
          <button onClick={handleLogout} className="btn-secondary">Logout</button>
        </div>
      </header>

      <nav className="dashboard-nav">
        <button onClick={() => navigate('/dashboard')} className="nav-btn">
          Devices
        </button>
        <button onClick={() => navigate('/policies')} className="nav-btn active">
          Policies
        </button>
      </nav>

      <main className="dashboard-content">
        <div className="content-header">
          <h2>Policy Management</h2>
          <button onClick={() => setShowCreateForm(!showCreateForm)} className="btn-primary">
            {showCreateForm ? 'Cancel' : 'Create Policy'}
          </button>
        </div>

        {error && <div className="error-message">{error}</div>}

        {showCreateForm && (
          <div className="create-policy-form">
            <h3>Create New Policy</h3>
            <form onSubmit={handleCreatePolicy}>
              <div className="form-group">
                <label>Name</label>
                <input
                  type="text"
                  value={newPolicy.name}
                  onChange={(e) => setNewPolicy({ ...newPolicy, name: e.target.value })}
                  required
                />
              </div>
              
              <div className="form-group">
                <label>Description</label>
                <textarea
                  value={newPolicy.description}
                  onChange={(e) => setNewPolicy({ ...newPolicy, description: e.target.value })}
                  required
                />
              </div>
              
              <div className="form-group">
                <label>Policy Type</label>
                <select
                  value={newPolicy.policyType}
                  onChange={(e) => setNewPolicy({ ...newPolicy, policyType: e.target.value })}
                >
                  <option value="Application">Application</option>
                  <option value="Configuration">Configuration</option>
                  <option value="Compliance">Compliance</option>
                </select>
              </div>

              <button type="submit" className="btn-primary">Create</button>
            </form>
          </div>
        )}

        {loading && <div className="loading">Loading policies...</div>}

        {!loading && policies.length === 0 && !showCreateForm && (
          <div className="empty-state">
            <p>No policies found</p>
            <p>Create a new policy to get started</p>
          </div>
        )}

        {!loading && policies.length > 0 && (
          <div className="policies-grid">
            {policies.map((policy) => (
              <div key={policy.id} className="policy-card">
                <h3>{policy.name}</h3>
                <p className="policy-description">{policy.description}</p>
                <div className="policy-meta">
                  <span className="policy-type">{policy.policyType}</span>
                  <span className="policy-date">
                    {new Date(policy.createdAt).toLocaleDateString()}
                  </span>
                </div>
                <div className="policy-actions">
                  <button
                    onClick={() => handleDeployPolicy(policy)}
                    className="btn-deploy"
                    disabled={deployLoading && selectedPolicy?.id === policy.id}
                  >
                    {deployLoading && selectedPolicy?.id === policy.id
                      ? 'Deploying...'
                      : 'Deploy to Intune'}
                  </button>
                  <button
                    onClick={() => handleDeletePolicy(policy.id)}
                    className="btn-delete"
                  >
                    Delete
                  </button>
                </div>
                {selectedPolicy?.id === policy.id && deployMessage && (
                  <div className={`deploy-message ${deployMessage.startsWith('✓') ? 'success' : 'error'}`}>
                    <pre>{deployMessage}</pre>
                  </div>
                )}
              </div>
            ))}
          </div>
        )}
      </main>
    </div>
  );
};
