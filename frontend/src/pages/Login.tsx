import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMsal } from '@azure/msal-react';
import { loginRequest } from '../config/authConfig';
import './Login.css';

export const Login = () => {
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const { instance, accounts } = useMsal();

  // Redirect if already authenticated
  useEffect(() => {
    if (accounts.length > 0) {
      navigate('/dashboard');
    }
  }, [accounts, navigate]);

  const handleLogin = async () => {
    setError('');
    setLoading(true);

    try {
      await instance.loginPopup(loginRequest);
      navigate('/dashboard');
    } catch (err: any) {
      console.error('Login error:', err);
      setError(err.message || 'An error occurred during login');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-container">
      <div className="login-card">
        <h1>Intune Management System</h1>
        <h2>Login with Microsoft</h2>
        
        {error && <div className="error-message">{error}</div>}
        
        <button 
          onClick={handleLogin} 
          disabled={loading} 
          className="btn-primary"
          style={{ marginTop: '20px' }}
        >
          {loading ? 'Signing in...' : 'Sign in with Microsoft'}
        </button>
        
        <p className="info-text">
          Note: Sign in with your Microsoft account to access Intune device management features.
        </p>
      </div>
    </div>
  );
};
