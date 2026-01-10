<template>
  <div style="padding: 2rem;">
    <h1>Demo External Application</h1>
    <p>This page simulates an external app initiating login at KorID.</p>

    <div style="margin-top: 1rem;">
      <label>Choose Application:
        <select v-model="appId">
          <option v-for="app in applications" :key="(app.id ?? app.Id)" :value="(app.id ?? app.Id)">{{ (app.id ?? app.Id) }} — {{ app.name ?? app.Name }} (clientId: {{ app.clientId ?? app.ClientId }})</option>
        </select>
      </label>
    </div>

    <div style="margin-top: 1rem;">
      <label>Callback URL: <input v-model="callbackUrl" style="width: 400px;" /></label>
    </div>

    <div style="margin-top: 1rem;">
      <button @click="startLogin" :disabled="!appId">Start login at KorID</button>
    </div>

    <div style="margin-top: 2rem; background:#f7f7f7; padding:1rem; border-radius:6px;">
      <strong>Note:</strong> This will redirect to the SPA route `/login` with query parameters `applicationId` and `redirectUrl`.
    </div>

    <div v-if="loading" style="margin-top:1rem">Loading applications...</div>
    <div v-if="errorMsg" style="margin-top:1rem; color: #c00">Error: {{ errorMsg }}</div>
    <div v-if="!loading && applications.length === 0 && !errorMsg" style="margin-top:1rem; color: #c00">No applications found on the API.</div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter();

const applications = ref([]);
const appId = ref('');
const callbackUrl = ref('http://localhost:5173/test/callback');
const loading = ref(true);
const errorMsg = ref('');

// API base (try Vite env then fallback to backend HTTPS to avoid HTTP->HTTPS redirect issues)
const API_BASE = import.meta.env.VITE_API_URL || 'https://localhost:7162';

onMounted(async () => {
  loading.value = true;
  errorMsg.value = '';
  try {
    const res = await fetch(`${API_BASE}/api/applications`, { mode: 'cors' });
    if (!res.ok) {
      const text = await res.text().catch(() => '');
      errorMsg.value = `Failed to fetch applications: ${res.status} ${text}`;
      console.error('Fetch applications failed', res.status, text);
      return;
    }

    const data = await res.json();
    applications.value = Array.isArray(data) ? data : [];
    if (applications.value.length > 0) {
      // default to first app's id (support both Id and id)
      appId.value = String(applications.value[0].id ?? applications.value[0].Id ?? '');
    }
  } catch (e) {
    errorMsg.value = e?.message || String(e);
    console.error('Error fetching applications', e);
  } finally {
    loading.value = false;
  }
});

function startLogin() {
  const params = new URLSearchParams();
  params.set('applicationId', appId.value);
  params.set('redirectUrl', callbackUrl.value);

  // Redirect to our LoginView with params
  router.push({ path: '/login', query: Object.fromEntries(params) });
}
</script>
