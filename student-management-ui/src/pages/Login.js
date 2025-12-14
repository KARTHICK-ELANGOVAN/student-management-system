import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { login } from '../api';

export default function Login({ onLogin }) {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState(null);
  const navigate = useNavigate();

  async function submit(e) {
    e.preventDefault();
    try {
      const res = await login({ username, password });
      const token = res.data.token;
      // persist token synchronously so protected routes can read it immediately
      localStorage.setItem('token', token);
      onLogin(token);
      navigate('/students');
    } catch (err) {
      const payload = err?.response?.data;
      const text = payload?.message ?? (typeof payload === 'string' ? payload : payload ? JSON.stringify(payload) : err.message);
      setError(text);
    }
  }

  return (
    <div className="card" style={{ maxWidth: 420 }}>
      <h2>Login</h2>
      <form className="form" onSubmit={submit}>
        <div>
          <label>Username</label>
          <input autoFocus value={username} onChange={(e) => setUsername(e.target.value)} required />
        </div>
        <div>
          <label>Password</label>
          <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} required />
        </div>
        <div style={{ display: 'flex', gap: 8 }}>
          <button className="btn" type="submit">Login</button>
          <Link to="/register" className="btn secondary">Register</Link>
        </div>
      </form>
      {error && <div className="error">{String(error)}</div>}
    </div>
  );
}
