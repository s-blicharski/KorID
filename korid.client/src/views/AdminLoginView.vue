<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { fetchWrapper } from '../services/fetchWrapper';

interface LoginResponse {
  token: string;
  refreshToken?: string;
  expiration?: string;
}

const router = useRouter();
const username = ref('');
const password = ref('');
const loading = ref(false);
const error = ref('');

async function handleLogin() {
  error.value = '';
  if (!username.value || !password.value) {
    error.value = 'Please enter both username and password.';
    return;
  }

  loading.value = true;

  try {
    const response = await fetchWrapper.post<LoginResponse>(
      '/api/auth/admin/login',
      {
        username: username.value,
        password: password.value
      },
      { auth: false }
    );

    if (response && response.token) {
      localStorage.setItem('korid_token', response.token);
      await router.push('/admin');
    } else {
      error.value = 'Login successful but no token was received.';
    }

  } catch (err: unknown) {
    console.error('Login error:', err);

    const e = err as { data?: { message?: unknown }; status?: number } | undefined;

    if (e && e.data && typeof e.data.message === 'string') {
      error.value = e.data.message;
    } else if (e && e.status === 401) {
      error.value = 'Invalid credentials.';
    } else if (e && e.status === 404) {
      error.value = 'Login endpoint not found (404). Check API URL.';
    } else {
      error.value = 'An unexpected error occurred. Please try again.';
    }
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <div class="login-container">
    <div class="login-card">
      <h2>Admin Login</h2>

      <form @submit.prevent="handleLogin">
        <div class="form-group">
          <label for="username">Username</label>
          <input
            id="username"
            v-model="username"
            type="text"
            placeholder="Enter admin username"
            :disabled="loading"
            autocomplete="username"
          />
        </div>

        <div class="form-group">
          <label for="password">Password</label>
          <input
            id="password"
            v-model="password"
            type="password"
            placeholder="Enter password"
            :disabled="loading"
            autocomplete="current-password"
          />
        </div>

        <div v-if="error" class="error-alert">
          {{ error }}
        </div>

        <button type="submit" :disabled="loading" class="login-btn">
          <span v-if="loading" class="spinner"></span>
          <span v-else>Sign In</span>
        </button>
      </form>
    </div>
  </div>
</template>

<style scoped>

*{box-sizing:border-box}
.login-container{
  min-height:100vh;
  display:flex;
  align-items:center;
  justify-content:center;
  padding:32px;
}

/* karta logowania */
.login-card{
  width:100%;
  max-width:420px;
  background:linear-gradient(180deg, rgba(255,255,255,0.9), var(--card));
  border-radius:var(--radius);
  box-shadow:var(--shadow);
  padding:28px;
  border:1px solid rgba(15,23,42,0.04);
  backdrop-filter: blur(6px);
}

/* nagłówek */
.login-card h2{
  margin:0 0 18px 0;
  font-size:20px;
  font-weight:600;
  color:white;
}

/* formularz */
.form-group{
  display:flex;
  flex-direction:column;
  gap:8px;
  margin-bottom:14px;
}

.form-group label{
  font-size:13px;
  margin: 12px;
  user-select:none;
}

.form-group input{
  height:44px;
  padding:10px 12px;
  font-size:15px;
  border-radius:8px;
  border:1px solid rgba(15,23,42,0.07);
  background:linear-gradient(180deg, #fff, #fbfdff);
  color:#0f172a;
  outline:none;
  transition:box-shadow .14s ease, border-color .14s ease, transform .06s ease;
}

.form-group input::placeholder{color:#9ca3af}
.form-group input:focus{
  box-shadow:0 6px 18px rgba(43,108,176,0.12);
  border-color:var(--accent);
  transform:translateY(-1px);
}

/* komunikat błędu */
.error-alert{
  background: rgba(229,62,62,0.08);
  color:var(--danger);
  padding:10px 12px;
  border-radius:8px;
  margin-bottom:12px;
  font-size:14px;
  border:1px solid rgba(229,62,62,0.12);
}

/* przycisk logowania */
.login-btn{
  width:100%;
  height:46px;
  display:inline-flex;
  align-items:center;
  justify-content:center;
  gap:10px;
  padding:0 16px;
  background:var(--accent);
  color:#fff;
  font-weight:600;
  border:none;
  border-radius:10px;
  cursor:pointer;
  transition:transform .12s ease, box-shadow .12s ease, background .12s ease;
  box-shadow:0 6px 18px rgba(43,108,176,0.12);
}

.login-btn:hover{ background:var(--accent-600); transform:translateY(-2px) }
.login-btn:active{ transform:translateY(0) }
.login-btn:disabled{
  opacity:0.66;
  cursor:not-allowed;
  transform:none;
  box-shadow:none;
  background:linear-gradient(90deg, #9fb7db, #8bb0db);
}

/* spinner */
.spinner{
  width:18px;
  height:18px;
  border-radius:50%;
  border:2px solid rgba(255,255,255,0.22);
  border-top-color:rgba(255,255,255,0.92);
  animation:spin 0.9s linear infinite;
}
@keyframes spin{ to { transform:rotate(360deg) } }

/* responsywność */
@media (max-width:480px){
  .login-card{
    padding:20px;
    border-radius:10px;
  }
  .login-card h2{ font-size:18px }
  .form-group input{ height:42px }
  .login-btn{ height:44px }
}

/* accessibility: reduced motion */
@media (prefers-reduced-motion: reduce){
  .login-card, .form-group input, .login-btn, .login-btn:hover{ transition:none; animation:none }
}
</style>
