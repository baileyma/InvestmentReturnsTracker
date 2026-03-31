import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import axios from 'axios';
import { useAuth } from '../../context/AuthContext';
import './Login.scss';

const Login = () => {
  const [error, setError] = useState('');
  const { login } = useAuth();
  const navigate = useNavigate();
  const apiUrl = import.meta.env.VITE_API_URL;

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    const { username, password } = e.target;
    try {
      const response = await axios.post(`${apiUrl}/Auth/login`, {
        username: username.value,
        password: password.value,
      });
      login(response.data.jwtToken);
      navigate('/');
    } catch {
      setError('Invalid username or password.');
    }
  };

  return (
    <div className="Login">
      <h2 className="Login__title">Login</h2>
      <form className="Login__form" onSubmit={handleSubmit}>
        <label htmlFor="username">Username</label>
        <input id="username" name="username" type="text" required />
        <label htmlFor="password">Password</label>
        <input id="password" name="password" type="password" required />
        {error && <p className="Login__error">{error}</p>}
        <button type="submit">Login</button>
      </form>
      <p>
        No account? <Link to="/register">Register here</Link>
      </p>
    </div>
  );
};

export default Login;
