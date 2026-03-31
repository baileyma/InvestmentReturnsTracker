import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import axios from 'axios';
import './Register.scss';

const Register = () => {
  const [error, setError] = useState('');
  const navigate = useNavigate();
  const apiUrl = import.meta.env.VITE_API_URL;

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    const { username, email, password } = e.target;
    try {
      await axios.post(`${apiUrl}/Auth/register`, {
        username: username.value,
        email: email.value,
        password: password.value,
      });
      navigate('/login');
    } catch {
      setError('Registration failed. Username may already be taken.');
    }
  };

  return (
    <div className="Register">
      <h2 className="Register__title">Register</h2>
      <form className="Register__form" onSubmit={handleSubmit}>
        <label htmlFor="username">Username</label>
        <input id="username" name="username" type="text" required />
        <label htmlFor="email">Email</label>
        <input id="email" name="email" type="email" required />
        <label htmlFor="password">Password</label>
        <input id="password" name="password" type="password" required />
        {error && <p className="Register__error">{error}</p>}
        <button type="submit">Register</button>
      </form>
      <p>
        Already have an account? <Link to="/login">Login here</Link>
      </p>
    </div>
  );
};

export default Register;
