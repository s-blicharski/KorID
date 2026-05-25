<script setup lang="ts">
import { onMounted, onBeforeUnmount, ref, watch } from 'vue';
import { useRouter } from 'vue-router';
import { Html5Qrcode } from 'html5-qrcode';
import { fetchWrapper } from '@/services/fetchWrapper';
import { getRoles, getUsername } from '../services/jwt';

const router = useRouter();
const scanMode = ref(true);
const status = ref<'idle' | 'scanning' | 'verifying' | 'error'>('idle');
const message = ref('');
const manualToken = ref('');
const cameraRequested = ref(false);
let scanner: Html5Qrcode | null = null;

interface VerifyResponse { token: string; username: string; roles: string[]; message?: string; }

function goToAdmin() {
  scanMode.value = false;
}

function goToAdminLogin() {
  router.push('/admin/login');
}

async function verify(token: string) {
  status.value = 'verifying';
  message.value = '';
  try {
    const res = await fetchWrapper.post<VerifyResponse>('/access/verify', { token }, { auth: false });
    localStorage.setItem('korid_token', res.token);
    const roles = getRoles(res.token);
    message.value = `Zalogowano jako ${getUsername(res.token)} (role: ${roles.join(', ') || 'brak'})`;
    await stopScanner();
    await router.push('/dashboard');
  } catch (e: unknown) {
    status.value = 'error';
    const err = e as { data?: { message?: string } } | undefined;
    message.value = err?.data?.message ?? 'Nie udało się zweryfikować kodu QR.';
  }
}

async function startScanner() {
  status.value = 'scanning';
  message.value = '';
  scanner = new Html5Qrcode('qr-reader');
  try {
    await scanner.start(
      { facingMode: 'environment' },
      { fps: 10, qrbox: { width: 250, height: 250 } },
      (decodedText) => { void verify(decodedText); },
      () => { /* ignorujemy pojedyncze nieudane klatki */ }
    );
  } catch {
    status.value = 'error';
    message.value = 'Brak dostępu do kamery. Kliknij „Włącz kamerę” albo sprawdź ustawienia przeglądarki.';
  }
}

async function requestCameraPermission() {
  cameraRequested.value = true;
  try {
    const stream = await navigator.mediaDevices.getUserMedia({ video: true, audio: false });
    stream.getTracks().forEach(track => track.stop());
    await startScanner();
  } catch {
    status.value = 'error';
    message.value = 'Przeglądarka zablokowała kamerę. Otwórz ustawienia witryny i pozwól na dostęp do kamery.';
  }
}

async function stopScanner() {
  if (scanner) {
    try { await scanner.stop(); scanner.clear(); } catch { /* noop */ }
    scanner = null;
  }
}

function submitManual() {
  if (manualToken.value.trim()) void verify(manualToken.value.trim());
}

watch(scanMode, async (newVal) => {
  if (newVal) {
    await startScanner();
  } else {
    await stopScanner();
  }
});

onMounted(() => {
  if (scanMode.value) startScanner();
});
onBeforeUnmount(stopScanner);
</script>

<template>
  <div style="max-width: 600px; margin: 2rem auto;">
    <div style="text-align: center; margin-bottom: 2rem;">
      <h1 style="margin: 0 0 1rem 0;">System Przystępu KorID</h1>
      <p style="color: #666; margin: 0;">Wybierz sposób dostępu:</p>
    </div>

    <div style="display: flex; gap: 1rem; margin-bottom: 2rem; flex-wrap: wrap;">
      <button
        @click="scanMode = true"
        :style="{
          flex: 1,
          minWidth: '200px',
          padding: '1rem',
          backgroundColor: scanMode ? '#2c7' : '#ddd',
          color: scanMode ? 'white' : '#333',
          border: 'none',
          borderRadius: '8px',
          fontSize: '16px',
          cursor: 'pointer',
          fontWeight: 'bold'
        }"
      >
        📱 Skanuj kod QR
      </button>
      <button
        @click="goToAdmin"
        :style="{
          flex: 1,
          minWidth: '200px',
          padding: '1rem',
          backgroundColor: !scanMode ? '#0066cc' : '#ddd',
          color: !scanMode ? 'white' : '#333',
          border: 'none',
          borderRadius: '8px',
          fontSize: '16px',
          cursor: 'pointer',
          fontWeight: 'bold'
        }"
      >
        👨‍💼 Jestem adminem
      </button>
    </div>

    <div v-if="scanMode" style="background: #f5f5f5; padding: 1.5rem; border-radius: 8px;">
      <h2 style="margin-top: 0;">Skanowanie kodu dostępu</h2>
      <p style="color: #666;">Skieruj kamerę na kod QR (admin lub viewer).</p>

      <div id="qr-reader" style="width: 100%; margin: 1rem 0;"></div>

      <div style="display:flex; gap: 0.5rem; flex-wrap: wrap; margin: 0.75rem 0 1rem;">
        <button
          @click="requestCameraPermission"
          style="padding: 0.5rem 1rem; background: #0066cc; color: white; border: none; border-radius: 4px; cursor: pointer;"
        >
          {{ cameraRequested ? 'Spróbuj ponownie włączyć kamerę' : 'Włącz kamerę' }}
        </button>
        <button
          @click="submitManual"
          style="padding: 0.5rem 1rem; background: #2c7; color: white; border: none; border-radius: 4px; cursor: pointer;"
        >
          Użyj tokenu ręcznie
        </button>
      </div>

      <p v-if="message" :style="{ color: status === 'error' ? '#c00' : '#2c7', margin: '1rem 0' }">{{ message }}</p>

      <div style="background: #fff3cd; border: 1px solid #ffc107; border-radius: 4px; padding: 1rem; margin: 1rem 0;">
        <p style="margin: 0; color: #856404; font-size: 14px;">
          <strong>❓ Nie masz kodu QR?</strong> Poproś administratora o kod dostępu. Jeśli jesteś adminem, <a href="#" @click.prevent="goToAdmin" style="color: #0066cc; text-decoration: underline;">kliknij tutaj</a> aby wygenerować kody dla użytkowników.
        </p>
      </div>

      <div style="margin-top: 1.5rem; border-top: 1px solid #ddd; padding-top: 1rem;">
        <p style="font-size: 13px; color:#666; margin: 0 0 0.5rem 0;">Awaryjnie — wklej token z kodu:</p>
        <div style="display: flex; gap: 0.5rem;">
          <input
            v-model="manualToken"
            placeholder="token z QR"
            style="flex: 1; padding: 0.5rem; border: 1px solid #ccc; border-radius: 4px;"
          />
          <button
            @click="submitManual"
            style="padding: 0.5rem 1rem; background: #2c7; color: white; border: none; border-radius: 4px; cursor: pointer;"
          >
            Zaloguj
          </button>
        </div>
      </div>
    </div>

    <div v-else style="background: #f0f4f8; padding: 2rem; border-radius: 8px; text-align: center;">
      <h2 style="margin-top: 0;">👨‍💼 Panel Administratora</h2>
      <p style="color: #666; margin-bottom: 1.5rem;">Zaloguj się aby zarządzać systemem i użytkownikami:</p>

      <div style="background: white; padding: 1.5rem; border-radius: 8px; margin-bottom: 1.5rem; border-left: 4px solid #0066cc;">
        <p style="margin: 0; color: #333; text-align: left;">
          <strong>📋 Co możez robić:</strong><br/>
          • Zarządzać użytkownikami<br/>
          • Zarządzać aplikacjami<br/>
          • Przeglądać audyt systemu
        </p>
      </div>

      <div style="display: flex; gap: 1rem; justify-content: center; flex-wrap: wrap;">
        <button
          @click="goToAdminLogin"
          style="padding: 0.75rem 1.5rem; background: #0066cc; color: white; border: none; border-radius: 4px; cursor: pointer; font-size: 16px; font-weight: bold;"
        >
          🔐 Zaloguj się jako admin
        </button>
        <button
          @click="scanMode = true"
          style="padding: 0.75rem 1.5rem; background: #ccc; color: #333; border: none; border-radius: 4px; cursor: pointer; font-size: 16px;"
        >
          ← Wróć do skanowania
        </button>
      </div>
    </div>
  </div>
</template>

