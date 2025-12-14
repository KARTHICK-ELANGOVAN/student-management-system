import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { getStudents, createStudent, updateStudent, deleteStudent } from '../api';

export default function Students() {
  const [students, setStudents] = useState([]);
  const [loading, setLoading] = useState(true);
  const [form, setForm] = useState({ name: '', rollNumber: '', address: '', grade: '' });
  const [editing, setEditing] = useState(null);
  const navigate = useNavigate();

  useEffect(() => {
    const token = localStorage.getItem('token');
    if (!token) return navigate('/login');
    fetchList();
    // eslint-disable-next-line
  }, []);

  async function fetchList() {
    setLoading(true);
    try {
      const res = await getStudents();
      setStudents(res.data);
    } catch (err) {
      if (err?.response?.status === 401) navigate('/login');
    } finally { setLoading(false); }
  }

  async function submit(e) {
    e.preventDefault();
    if (editing) {
      await updateStudent(editing, form);
      setEditing(null);
    } else {
      await createStudent(form);
    }
    setForm({ name: '', rollNumber: '', address: '', grade: '' });
    fetchList();
  }

  async function remove(id) { await deleteStudent(id); fetchList(); }

  function edit(s) { setEditing(s.id); setForm({ name: s.name, rollNumber: s.rollNumber, address: s.address, grade: s.grade }); }

  return (
    <div>
      <div className="toolbar">
        <h2>Students</h2>
        <div>
          <button className="btn" onClick={() => { setForm({ name: '', rollNumber: '', address: '', grade: '' }); setEditing(null); }}>New Student</button>
        </div>
      </div>

      <div className="card" style={{ marginBottom: 12 }}>
        <form className="form" onSubmit={submit}>
          <div className="form-row">
            <div style={{ flex: 1 }}>
              <label>Name</label>
              <input placeholder="Name" value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required />
            </div>
            <div style={{ width: 120 }}>
              <label>Roll</label>
              <input placeholder="Roll" value={form.rollNumber} onChange={(e) => setForm({ ...form, rollNumber: e.target.value })} required />
            </div>
            <div style={{ width: 120 }}>
              <label>Grade</label>
              <input placeholder="Grade" value={form.grade} onChange={(e) => setForm({ ...form, grade: e.target.value })} />
            </div>
          </div>

          <div>
            <label>Address</label>
            <input placeholder="Address" value={form.address} onChange={(e) => setForm({ ...form, address: e.target.value })} />
          </div>

          <div style={{ display: 'flex', gap: 8 }}>
            <button className="btn" type="submit">{editing ? 'Update' : 'Create'}</button>
            {editing && <button type="button" className="btn secondary" onClick={() => { setEditing(null); setForm({ name: '', rollNumber: '', address: '', grade: '' }); }}>Cancel</button>}
          </div>
        </form>
      </div>

      {loading ? <div>Loading...</div> : (
        <div className="card">
          <table className="table">
            <thead>
              <tr>
                <th>Name</th>
                <th>Roll</th>
                <th>Address</th>
                <th>Grade</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {students.map(s => (
                <tr key={s.id}>
                  <td>{s.name}</td>
                  <td>{s.rollNumber}</td>
                  <td>{s.address}</td>
                  <td>{s.grade}</td>
                  <td className="actions">
                    <button className="btn secondary" onClick={() => edit(s)}>Edit</button>
                    <button className="btn" onClick={() => remove(s.id)}>Delete</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
