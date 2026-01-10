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
  <div class="flex items-center justify-center p-6 min-h-screen">
      <div class="hero-content flex-col lg:flex-row-reverse w-full  max-w-4xl min-h-128 mx-auto bg-gray-400/50 rounded-2xl shadow-xl">
        <div class="text-center lg:text-left lg:w-1/2 px-4">
          <h1 class="text-5xl font-bold">Login now!</h1>
          <p class="py-6">
            Zaloguj się do panelu administratora.
          </p>
        </div>

        <div class="card bg-base-100 w-full max-w-sm shrink-0 shadow-2xl lg:w-1/2 px-4">
          <div class="card-body">
            <form @submit.prevent="handleLogin" class="w-full">
              <fieldset class="fieldset">
                <label class="label">Email</label>
                <input
                  id="username"
                  v-model="username"
                  type="text"
                  class="input"
                  placeholder="Email"
                  :disabled="loading"
                  autocomplete="username"
                />

                <label class="label mt-3">Password</label>
                <input
                  id="password"
                  v-model="password"
                  type="password"
                  class="input"
                  placeholder="Password"
                  :disabled="loading"
                  autocomplete="current-password"
                />
                <div v-if="error" class="text-sm text-red-600 mb-2">
                  {{ error }}
                </div>

                <button
                  type="submit"
                  class="btn btn-neutral mt-4 w-full inline-flex items-center justify-center"
                  :disabled="loading"
                >
                  <span v-if="loading" class="inline-block w-4 h-4 border-2 border-white border-t-transparent rounded-full animate-spin mr-2" aria-hidden="true"></span>
                  <span v-if="!loading">Login</span>
                  <span v-else>Processing...</span>
                </button>
              </fieldset>
            </form>
          </div>
        </div>

      </div>
    </div>
</template>
