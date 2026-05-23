<template>
  <div style="padding: 2rem;">
    <h1>Demo External Application</h1>
    <p>This page simulates an external app initiating login at KorID.</p>

    <div style="margin-top: 1rem;">
      <label>
        Choose Application:
        <select v-model="appId">
          <option v-for="app in applications" :key="(app.id ?? app.Id)" :value="(app.id ?? app.Id)">{{ (app.id ?? app.Id) }} — {{ app.name ?? app.Name }} (clientId: {{ app.clientId ?? app.ClientId }})</option>
        </select>
      </label>
    </div>

    <div style="margin-top: 1rem;">
      <label>Callback URL: <input v-model="callbackUrl" style="width: 400px;" /></label>
    </div>

    <div style="margin-top: 1rem;">
      <!--<button @click="startLoginPopup" :disabled="!appId">Start login at KorID (popup)</button>-->
      <button @click="startLogin" :disabled="!appId" style="margin-left: .5rem">Start login at KorID (same window)</button>
    </div>

    <div style="margin-top: 2rem; background:#666; padding:1rem; border-radius:6px; color:#eee;">
      <strong>Note:</strong> This will redirect to the SPA route `/login` with query parameters `applicationId` and `redirectUrl`.
      The popup flow will postMessage back to this window when the callback is reached and then close the popup.
    </div>

    <div v-if="loading" style="margin-top:1rem">Loading applications...</div>
    <div v-if="errorMsg" style="margin-top:1rem; color: #c00">Error: {{ errorMsg }}</div>
    <div v-if="!loading && applications.length === 0 && !errorMsg" style="margin-top:1rem; color: #c00">No applications found on the API.</div>

    <div v-if="authResult" style="margin-top:1.5rem; padding:1rem; border:1px solid #ddd; border-radius:6px;">
      <h3>Login result received</h3>
      <div><strong>Type:</strong> {{ authResult.type }}</div>
      <div v-if="authResult.params">
        <h4 style="margin-top:.5rem">Parameters</h4>
        <ul>
          <li v-for="(v,k) in authResult.params" :key="k"><strong>{{ k }}:</strong> {{ v }}</li>
        </ul>
      </div>
      <div v-if="authResult.error" style="color:#c00; margin-top:.5rem"><strong>Error:</strong> {{ authResult.error }}</div>
    </div>
  </div>
</template>

<script setup>
  import { ref, onMounted, onBeforeUnmount } from 'vue';
  import { useRouter } from 'vue-router';

  const router = useRouter();

  const applications = ref([]);
  const appId = ref('');
  const callbackUrl = ref('/test/callback');
  const loading = ref(true);
  const errorMsg = ref('');
  const authResult = ref(null);
  let popupWindow = null;
  let popupCheckInterval = null;

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

    // Listen for postMessage from the callback popup
    window.addEventListener('message', onMessageReceived);
  });

  onBeforeUnmount(() => {
    window.removeEventListener('message', onMessageReceived);
    clearPopupInterval();
  });

  function onMessageReceived(e) {
    // Basic origin check - only accept messages from same origin
    try {
      if (e.origin !== window.location.origin) {
        // ignore messages from other origins
        return;
      }
    } catch {
      // ignore if origin not accessible
      return;
    }

    const data = e.data ?? {};
    // Expect an object like { type: 'korid_callback', params: { ... } }
    if (data && (data.type === 'korid_callback' || data.type === 'korid_error')) {
      authResult.value = data;
      // Close popup if still open
      try { popupWindow?.close(); } catch { }
      clearPopupInterval();
    }
  }

  function clearPopupInterval() {
    if (popupCheckInterval) {
      clearInterval(popupCheckInterval);
      popupCheckInterval = null;
    }
  }

  function startLogin() {
    // fallback same-window flow: navigate to /login route with query params
    const params = new URLSearchParams();
    params.set('applicationId', appId.value);
    params.set('redirectUrl', callbackUrl.value);

    router.push({ path: '/login', query: Object.fromEntries(params) });
  }

  function startLoginPopup() {
    // open the login route in a popup and wait for postMessage from the callback page
    const params = new URLSearchParams();
    params.set('applicationId', appId.value);
    params.set('redirectUrl', callbackUrl.value);

    const url = `${window.location.origin}/login?${params.toString()}`;
    // Open popup with reasonable size
    popupWindow = window.open(url, 'korid_login', 'width=480,height=800,noopener');

    if (!popupWindow) {
      errorMsg.value = 'Popup blocked. Please allow popups or use the same-window login.';
      return;
    }

    authResult.value = null;
    errorMsg.value = '';

    // Detect popup closed by user
    popupCheckInterval = setInterval(() => {
      try {
        if (!popupWindow || popupWindow.closed) {
          clearPopupInterval();
          popupWindow = null;
          if (!authResult.value) {
            errorMsg.value = 'Login popup closed before completing authentication.';
          }
        }
      } catch {
        // ignore cross-origin access errors while popup is on a different origin
      }
    }, 500);
  }
</script>
