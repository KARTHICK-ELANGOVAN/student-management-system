import axios from 'axios';

const api = axios.create();

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

export async function register(user) {
  return api.post('/api/auth/register', user);
}

export async function login(creds) {
  return api.post('/api/auth/login', creds);
}

export async function getStudents() {
  return api.get('/api/students');
}

export async function createStudent(data) {
  return api.post('/api/students', data);
}

export async function updateStudent(id, data) {
  return api.put(`/api/students/${id}`, data);
}

export async function deleteStudent(id) {
  return api.delete(`/api/students/${id}`);
}

export default api;
