import axios from 'axios';

const axiosClient = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
  timeout: 10000,
});

axiosClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('korid_token');
  if (token && config.headers) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

axiosClient.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error?.response?.status;
    if (status === 401) {
      // emit event so app can handle logout/redirect
      window.dispatchEvent(new CustomEvent('korid:unauthorized'));
    }
    return Promise.reject(error?.response?.data ?? error);
  }
);

export default axiosClient;

