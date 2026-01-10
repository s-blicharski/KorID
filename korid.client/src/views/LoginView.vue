<template>
  <div class="login-wrapper">
    <div class="login-card">
      <h1>Logowanie</h1>
      <p class="subtitle">Wprowadź swoje dane, aby zalogować się do aplikacji.</p>

      <div class="app-info" v-if="applicationId">
        <strong>Application ID:</strong> {{ applicationId }}
      </div>
      <div class="app-info error" v-else>
        Brak Application ID w parametrze żądania. Nie można kontynuować logowania.
      </div>

      <form @submit.prevent="onSubmit" novalidate>
        <div class="field">
          <label for="email">Email</label>
          <input id="email" type="email" v-model="email" placeholder="adres@email.com" @blur="emailTouched = true" />
          <div class="error" v-if="emailTouched && !emailValid">Proszę podać poprawny adres e-mail.</div>
        </div>

        <div class="field">
          <label for="password">Hasło</label>
          <input id="password" type="password" v-model="password" placeholder="Hasło" @blur="passwordTouched = true" />
          <div class="error" v-if="passwordTouched && !passwordValid">Hasło musi mieć co najmniej 6 znaków.</div>
        </div>

        <div class="actions">
          <button type="submit" :disabled="submitting || !formValid || !applicationId" class="primary">
            {{ submitting ? 'Logowanie...' : 'Zaloguj' }}
          </button>
        </div>

        <div class="server-error" v-if="error">{{ error }}</div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue';
import { useRouter, useRoute } from 'vue-router';

const router = useRouter();
const route = useRoute();

const email = ref('');
const password = ref('');
const error = ref('');
const submitting = ref(false);

const emailTouched = ref(false);
const passwordTouched = ref(false);

// Read application id and optional redirectUrl from query params or route params
const applicationId = ref(route.query.applicationId || route.query.appId || route.params.applicationId || route.params.appId || '');
const redirectUrl = ref(route.query.redirectUrl || route.query.redirect || '');

// API base (try Vite env then fallback to localhost backend port)
const API_BASE = import.meta.env.VITE_API_URL || 'https://localhost:7162';

const emailValid = computed(() => {
  const re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@(([^<>()[\]\\.,;:\s@\"]+\.)+[^<>()[\]\\.,;:\s@\"]{2,})$/i;
  return re.test(email.value);
});

const passwordValid = computed(() => password.value.length >= 6);
const formValid = computed(() => emailValid.value && passwordValid.value);

async function onSubmit() {
  error.value = '';
  emailTouched.value = true;
  passwordTouched.value = true;

  if (!applicationId.value) {
    error.value = 'Brak Application ID. Nie można kontynuować.';
    return;
  }

  if (!formValid.value) return;

  submitting.value = true;

  try {
    // Call backend endpoint to validate user + permissions for given application
    // Expected request body: { email, password, applicationId, redirectUrl? }

    const payload = { email: email.value, password: password.value, applicationId: applicationId.value };
    if (redirectUrl.value) payload['redirectUrl'] = redirectUrl.value;

    const res = await fetch(`${API_BASE}/api/external/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    if (!res.ok) {
      // Try to read message from body
      let bodyText = '';
      try { const json = await res.json(); bodyText = json?.message || JSON.stringify(json); } catch { bodyText = await res.text(); }
      error.value = `Błąd serwera: ${res.status} ${bodyText}`;
      return;
    }

    const data = await res.json();
    if (data?.success) {
      // if backend provides redirectUrl, redirect back to calling application with username and status
      const target = data.redirectUrl || redirectUrl.value;
      if (target) {
        const params = new URLSearchParams();
        if (data.username) params.set('username', data.username);
        params.set('status', 'ok');
        const sep = target.includes('?') ? '&' : '?';
        window.location.href = target + sep + params.toString();
        return;
      }

      // Fallback: navigate to provided route inside SPA if backend returned route
      if (data.redirectRoute) {
        router.push(data.redirectRoute);
        return;
      }

      // If no redirect provided, show success message
      error.value = 'Zalogowano, ale brak adresu przekierowania.';
    } else {
      error.value = data?.message || 'Nieprawidłowe dane logowania lub brak uprawnień.';
    }
  } catch (e) {
    error.value = 'Błąd połączenia. Spróbuj ponownie.';
  } finally {
    submitting.value = false;
  }
}
</script>

<style scoped>
.login-wrapper {
  background-color: inherit;
  height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
  margin: 0;
}

.login-card {
  background: white;
  padding: 32px;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0,0,0,0.1);
  width: 360px;
  text-align: left;
}

.login-card h1 {
  color: #2c3e50;
  margin: 0 0 8px 0;
}

.subtitle {
  color: #606266;
  margin: 0 0 12px 0;
}

.app-info {
  margin-bottom: 12px;
  font-size: 13px;
}

.field {
  margin-bottom: 16px;
}

.field label {
  display: block;
  font-size: 14px;
  margin-bottom: 6px;
  color: #333;
}

.field input {
  width: 100%;
  padding: 8px 10px;
  border: 1px solid #dcdfe6;
  border-radius: 4px;
  box-sizing: border-box;
}

.error {
  color: #e74c3c;
  font-size: 12px;
  margin-top: 6px;
}

.actions {
  display: flex;
  justify-content: center;
}

button.primary {
  background-color: #42b983;
  color: white;
  border: none;
  padding: 10px 20px;
  border-radius: 4px;
  cursor: pointer;
}

button.primary[disabled] {
  opacity: 0.6;
  cursor: not-allowed;
}

.server-error {
  color: #e74c3c;
  margin-top: 12px;
  text-align: center;
}
</style>
