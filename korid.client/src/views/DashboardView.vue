<template>
  <div style="max-width: 900px; margin: 2rem auto;">
    <div style="display:flex; justify-content:space-between; align-items:center;">
      <h1>Pulpit KorID</h1>
      <div>
        <span style="margin-right: 1rem;">
          {{ username }}
          <strong :style="{ color: isAdmin ? '#2c7' : '#888' }">[{{ roles.join(', ') || 'brak roli' }}]</strong>
        </span>
        <button @click="logout">Wyloguj</button>
      </div>
    </div>

    <p v-if="isAdmin" style="color:#2c7;">Tryb administratora — pełne zarządzanie.</p>
    <p v-else style="color:#888;">Tryb podglądu — tylko do odczytu.</p>

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
      </div>
    </div>

    <!-- Sekcja: Mój kod QR dostępu -->
    <div style="background:#f0f8ff; padding:1.5rem; border-radius:8px; border-left:4px solid #0066cc; margin-bottom:2rem;">
      <h2 style="margin-top:0;">📱 Mój kod dostępu</h2>
      <p style="color:#666; margin:0.5rem 0;">Wygeneruj swój unikalny kod QR. Możesz go zeskanować na innym urządzeniu lub udostępnić komuś innemu.</p>

      <button
        @click="generateMyQr"
        :disabled="generatingQr"
        style="margin-top:1rem; padding:0.75rem 1.5rem; background:#0066cc; color:white; border:none; border-radius:4px; cursor:pointer; font-size:14px; font-weight:bold;"
      >
        <span v-if="generatingQr">⏳ Generowanie...</span>
        <span v-else>✨ Wygeneruj kod QR</span>
      </button>

      <div v-if="myQrCode" style="margin-top:1rem; text-align:center;">
        <p style="color:#0066cc; font-weight:bold;">Twój kod dostępu (ważny 5 minut):</p>
        <img :src="myQrCode" alt="Mój kod QR" style="width:240px; border:2px solid #0066cc; border-radius:8px; margin:1rem 0;" />
        <p style="color:#666; font-size:12px;">Wyświetl kod na telefonie i zeskanuj go na stronie <a href="/scan" style="color:#0066cc;">skanowania</a></p>
      </div>
    </div>

    <p v-if="actionMsg" style="color:#c60;">{{ actionMsg }}</p>

    <div v-if="loading">Ładowanie...</div>
    <table v-else style="width:100%; border-collapse: collapse; margin-top: 1rem;">
      <thead>
        <tr><th style="text-align:left;border-bottom:1px solid #ccc;">ID</th>
            <th style="text-align:left;border-bottom:1px solid #ccc;">Użytkownik</th>
            <th style="text-align:left;border-bottom:1px solid #ccc;">Email</th>
            <th v-if="isAdmin" style="text-align:left;border-bottom:1px solid #ccc;">Akcje</th></tr>
      </thead>
      <tbody>
        <tr v-for="u in users" :key="u.id">
          <td>{{ u.id }}</td>
          <td>{{ u.username }}</td>
          <td>{{ u.email }}</td>
          <td v-if="isAdmin">
            <button @click="deleteUser(u.id)" style="color:#c00;">Usuń</button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed, onBeforeUnmount } from 'vue';
import { useRouter } from 'vue-router';
import { fetchWrapper } from '../services/fetchWrapper';
import { getRoles, getUsername } from '../services/jwt';

interface User { id: number; username: string; email: string; }

const router = useRouter();
const token = localStorage.getItem('korid_token');
const roles = ref<string[]>(getRoles(token));
const username = ref<string | null>(getUsername(token));
const isAdmin = computed(() => roles.value.includes('admin'));

const users = ref<User[]>([]);
const loading = ref(true);
const actionMsg = ref('');
const myQrCode = ref<string | null>(null);
const generatingQr = ref(false);
const qrObjectUrls: string[] = [];

async function generateMyQr() {
  generatingQr.value = true;
  try {
    const res = await fetch('/api/access/generate', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({})  // Pusty JSON body
    });
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    const blob = await res.blob();
    const url = URL.createObjectURL(blob);
    qrObjectUrls.push(url);
    myQrCode.value = url;
    actionMsg.value = 'Kod QR wygenerowany! Wyświetl go na telefonie i zeskanuj.';
  } catch (e: unknown) {
    const err = e as { message?: string } | undefined;
    actionMsg.value = `Błąd generowania kodu: ${err?.message ?? 'Spróbuj ponownie'}`;
  } finally {
    generatingQr.value = false;
  }
}

async function loadUsers() {
  loading.value = true;
  try {
    users.value = await fetchWrapper.get<User[]>('/users');
  } catch (e: unknown) {
    const err = e as { status?: number } | undefined;
    actionMsg.value = err?.status === 401 ? 'Sesja wygasła.' : 'Nie udało się pobrać użytkowników.';
  } finally {
    loading.value = false;
  }
}

async function deleteUser(id: number) {
  actionMsg.value = '';
  try {
    await fetchWrapper.del(`/users/${id}`);
    users.value = users.value.filter(u => u.id !== id);
    actionMsg.value = 'Usunięto użytkownika.';
  } catch (e: unknown) {
    const err = e as { status?: number } | undefined;
    actionMsg.value = err?.status === 403
      ? 'Brak uprawnień (403) — rola viewer nie może usuwać.'
      : 'Operacja nie powiodła się.';
  }
}

function logout() {
  localStorage.removeItem('korid_token');
  router.push('/scan');
}

function goToGate(gateName: string) {
  if (gateName === 'gate1') {
    router.push('/access/gate1');
  } else if (gateName === 'gate2') {
    router.push('/access/gate2');
  }
}

onMounted(loadUsers);
onBeforeUnmount(() => qrObjectUrls.forEach(URL.revokeObjectURL));
</script>

