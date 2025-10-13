import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { apiService } from '../services/apiService';
import type { Device } from '../types';
import './Dashboard.css';

export const Dashboard = () => {
  const [devices, setDevices] = useState<Device[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const navigate = useNavigate();

  useEffect(() => {
    const user = sessionStorage.getItem('user');
    if (!user) {
      navigate('/');
      return;
    }

    loadDevices();
  }, [navigate]);

  const loadDevices = async () => {
    try {
      setLoading(true);
      const data = await apiService.getDevices();
      setDevices(data);
      setError('');
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load devices');
      console.error('Error loading devices:', err);
    } finally {
      setLoading(false);
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
        <button onClick={() => navigate('/dashboard')} className="nav-btn active">
          Devices
        </button>
        <button onClick={() => navigate('/policies')} className="nav-btn">
          Policies
        </button>
      </nav>

      <main className="dashboard-content">
        <div className="content-header">
          <h2>Device Status</h2>
          <button onClick={loadDevices} className="btn-primary" disabled={loading}>
            {loading ? 'Refreshing...' : 'Refresh'}
          </button>
        </div>

        {error && (
          <div className="error-message">
            <p>{error}</p>
            <p className="error-note">
              Note: To view devices, you need to configure Azure AD credentials in the backend appsettings.json
            </p>
          </div>
        )}

        {loading && !error && <div className="loading">Loading devices...</div>}

        {!loading && !error && devices.length === 0 && (
          <div className="empty-state">
            <p>No devices found</p>
            <p className="empty-note">
              Make sure your Azure AD application has the correct permissions to read devices
            </p>
          </div>
        )}

        {!loading && devices.length > 0 && (
          <div className="devices-table">
            <table>
              <thead>
                <tr>
                  <th>Device Name</th>
                  <th>Operating System</th>
                  <th>OS Version</th>
                  <th>Compliance State</th>
                  <th>Management Agent</th>
                  <th>Last Sync</th>
                  <th>User</th>
                </tr>
              </thead>
              <tbody>
                {devices.map((device) => (
                  <tr key={device.id}>
                    <td>{device.deviceName}</td>
                    <td>{device.operatingSystem}</td>
                    <td>{device.osVersion}</td>
                    <td>
                      <span className={`status ${device.complianceState.toLowerCase()}`}>
                        {device.complianceState}
                      </span>
                    </td>
                    <td>{device.managementAgent}</td>
                    <td>
                      {device.lastSyncDateTime
                        ? new Date(device.lastSyncDateTime).toLocaleString()
                        : 'Never'}
                    </td>
                    <td>{device.userPrincipalName}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </main>
    </div>
  );
};
