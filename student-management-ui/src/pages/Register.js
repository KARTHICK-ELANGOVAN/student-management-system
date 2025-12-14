import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { register } from '../api';

export default function Register() {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [message, setMessage] = useState(null);
  const navigate = useNavigate();

  async function submit(e) {
    e.preventDefault();
    try {
      await register({ username, password });
      setMessage('Registered. Please login.');
      setTimeout(() => navigate('/login'), 1000);
    } catch (err) {
      const payload = err?.response?.data;
      const text = payload?.message ?? (typeof payload === 'string' ? payload : payload ? JSON.stringify(payload) : err.message);
      setMessage(text);
    }
  }

  return (
    <div className="card" style={{ maxWidth: 480 }}>
      <h2>Register</h2>
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
          <button className="btn" type="submit">Register</button>
          <Link to="/login" className="btn secondary">Login</Link>
        </div>
      </form>
      {message && <div className={message && message.includes('Registered') ? 'message' : 'error'}>{String(message)}</div>}
    </div>
  );
}
