import React, { useState, useEffect } from 'react';
import { Routes, Route, Link, useNavigate } from 'react-router-dom';
import Login from './pages/Login';
import Register from './pages/Register';
import Students from './pages/Students';

function App() {
  const [token, setToken] = useState(localStorage.getItem('token'));
  const navigate = useNavigate();

  useEffect(() => { if (token) localStorage.setItem('token', token); else localStorage.removeItem('token') }, [token]);

  function logout() { setToken(null); navigate('/login'); }

  return (
    <div className="container">
      <header className="app-header">
        <div className="brand">
          <div className="logo">SM</div>
          <div>
            <div className="title">Student Management</div>
            <div style={{ fontSize: 12, color: 'var(--muted)' }}>Manage students quickly</div>
          </div>
        </div>
        <nav className="nav">
          {token ? (
            <>
              <Link to="/students" className="btn secondary">Students</Link>
              <button className="btn" onClick={logout}>Logout</button>
            </>
          ) : (
            <>
              <Link to="/login" className="btn secondary">Login</Link>
              <Link to="/register" className="btn">Register</Link>
            </>
          )}
        </nav>
      </header>

      <Routes>
        <Route path="/login" element={<Login onLogin={(t) => setToken(t)} />} />
        <Route path="/register" element={<Register />} />
        <Route path="/students" element={<Students />} />
        <Route path="/" element={<div className="card">Welcome. Go to <Link to="/students">Students</Link></div>} />
      </Routes>
    </div>
  );
}

export default App;
