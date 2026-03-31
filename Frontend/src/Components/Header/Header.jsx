import { useNavigate } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';
import './Header.scss';

const Header = () => {
  const { isAuthenticated, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <nav className="Header">
      Invesment Tracker Website
      {isAuthenticated && (
        <button onClick={handleLogout}>Logout</button>
      )}
    </nav>
  );
};

export default Header;
