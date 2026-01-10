<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount } from 'vue';
import { useRouter } from 'vue-router';
import OrganizationList from "../components/admin/OrganizationList.vue";
import UserList from "../components/admin/UserList.vue";

const router = useRouter();
const username = ref<string | null>(null);

function base64UrlDecode(input: string) {
  // Replace URL-safe characters, add padding if needed
  input = input.replace(/-/g, '+').replace(/_/g, '/');
  const pad = input.length % 4;
  if (pad === 2) input += '==';
  else if (pad === 3) input += '=';
  else if (pad !== 0) return null;
  try {
    return decodeURIComponent(Array.prototype.map.call(atob(input), (c: string) => {
      return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(''));
  } catch {
    return null;
  }
}

function getUsernameFromToken(): string | null {
  const token = localStorage.getItem('korid_token');
  if (!token) return null;
  const parts = token.split('.');
  if (parts.length < 2) return null;
  const payloadJson = base64UrlDecode(parts[1]);
  if (!payloadJson) return null;
  try {
    const payload = JSON.parse(payloadJson) as Record<string, unknown>;
    // common claim used by the backend: JwtRegisteredClaimNames.UniqueName => "unique_name"
    return (payload['unique_name'] as string)
      ?? (payload['username'] as string)
      ?? (payload['name'] as string)
      ?? null;
  } catch {
    return null;
  }
}

function logout() {
  localStorage.removeItem('korid_token');
  username.value = null;
  router.push('/admin/login');
}

function handleUnauthorized() {
  // token expired / invalid — force logout
  logout();
}

onMounted(() => {
  username.value = getUsernameFromToken();
  window.addEventListener('korid:unauthorized', handleUnauthorized as EventListener);
});

onBeforeUnmount(() => {
  window.removeEventListener('korid:unauthorized', handleUnauthorized as EventListener);
});
</script>

<template>
  <div style="display:flex; flex-direction:column; height:100vh;">
    <!-- Top navbar -->
    <header style="display:flex; align-items:center; justify-content:space-between; padding:12px 20px; background:#1f2937; color:white;">
      <div style="display:flex; align-items:center; gap:12px;">
        <h2 style="margin:0; font-size:18px;">KorID Admin</h2>
        <span style="opacity:0.8;">/ Panel Administratora</span>
      </div>

      <div style="display:flex; align-items:center; gap:12px;">
        <span v-if="username" style="font-weight:600;">{{ username }}</span>
        <button @click="logout" style="background:#ef4444; color:white; border:none; padding:8px 12px; border-radius:6px; cursor:pointer;">
          Logout
        </button>
      </div>
    </header>

    <div style="display: flex; flex:1; overflow:hidden;">
      <aside style="width: 220px; background-color: #2c3e50; color: white; padding: 20px;">
        <h3 style="margin-top:0;">Menu boczne</h3>
        <p>Dashboard</p>
        <p>U¿ytkownicy</p>
      </aside>

      <main style="flex: 1; padding: 20px; overflow:auto;">
        <h1>Panel Administratora</h1>
        <p>Widzisz to, bo jesteœ na œcie¿ce /admin.</p>
        <OrganizationList />
        <UserList />
      </main>
    </div>
  </div>
</template>