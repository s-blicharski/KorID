<script setup lang="ts">
  import { ref, onMounted, onBeforeUnmount } from 'vue';
  import { useRouter } from 'vue-router';
  import UserList from "../components/admin/UserList.vue";
  import AplicationsList from "../components/admin/AplicationsList.vue";
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
    const payloadJson = base64UrlDecode(parts[1]!);
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

  function goToGate(gateName: string) {
    if (gateName === 'gate1') {
      router.push('/access/gate1');
    } else if (gateName === 'gate2') {
      router.push('/access/gate2');
    }
  }

  function goToDashboard() {
    router.push('/dashboard');
  }

  function handleUnauthorized() {
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
  <div class="min-h-screen flex flex-col">
    <div class="flex flex-1 overflow-hidden">
      <main class="flex-1 p-6 overflow-auto ">
        <div class="flex items-cente justify-end mb-6">
          <div class="flex items-center  gap-3">
            <span v-if="username" class="font-semibold text-2xl"><i>Nazwa użytkownika: </i>{{ username }}</span>
            <button
              @click="logout"
              class="bg-red-500 hover:bg-red-600 text-white text-xl p-3 rounded-md transition"
            >
              Wyloguj się
            </button>
          </div>
        </div>

        <h1 class="text-2xl font-semibold mb-2">Panel Administratora</h1>

        <!-- Sekcja: Nawigacja do stref dostępu -->
        <div style="background:#f9f9f9; padding:1rem; border-radius:8px; margin-bottom:2rem;">
          <h3 style="margin-top:0;">🚪 Strefy dostępu:</h3>
          <div style="display:flex; gap:1rem; flex-wrap:wrap;">
            <button
              @click="goToGate('gate1')"
              style="padding:0.75rem 1.5rem; background:#ff9800; color:white; border:none; border-radius:4px; cursor:pointer; font-weight:bold;"
            >
              🔒 Strefa #1 (Admin)
            </button>
            <button
              @click="goToGate('gate2')"
              style="padding:0.75rem 1.5rem; background:#0066cc; color:white; border:none; border-radius:4px; cursor:pointer; font-weight:bold;"
            >
              🔍 Strefa #2 (Viewer)
            </button>
            <button
              @click="goToDashboard"
              style="padding:0.75rem 1.5rem; background:#28a745; color:white; border:none; border-radius:4px; cursor:pointer; font-weight:bold;"
            >
              📱 Mój Dashboard
            </button>
          </div>
        </div>

        <div class="space-y-6">
          <UserList />
          <AplicationsList />
        </div>
      </main>
    </div>
  </div>
</template>
