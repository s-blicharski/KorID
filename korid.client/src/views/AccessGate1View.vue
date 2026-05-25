<template>
  <div style="max-width: 600px; margin: 2rem auto;">
    <div style="background: #fff3cd; padding: 1.5rem; border-radius: 8px; border-left: 4px solid #ff9800; margin-bottom: 2rem;">
      <h1 style="margin-top: 0;">🔒 Strefa Dostępu #1 — Administratorzy</h1>
      <p style="color: #856404; margin: 0;">
        <strong>Wymagana rola:</strong> admin<br/>
        Ta strefa jest dostępna tylko dla administratorów systemu. Skanuj kod QR aby wejść.
      </p>
    </div>

    <div v-if="accessGranted" style="background: #d4edda; padding: 1.5rem; border-radius: 8px; border: 2px solid #28a745; text-align: center;">
      <h2 style="color: #155724; margin-top: 0;">✅ Dostęp Przyznany!</h2>
      <p style="color: #155724; margin: 0 0 1rem 0;">Jesteś zalogowany jako: <strong>{{ username }}</strong></p>
      <p style="color: #155724;">Rola: <strong>{{ userRoles.join(', ') }}</strong></p>
      <button @click="backToDashboard" style="margin-top: 1rem; padding: 0.75rem 1.5rem; background: #28a745; color: white; border: none; border-radius: 4px; cursor: pointer; font-weight: bold;">
        ← Wróć na pulpit
      </button>
      <button @click="goToGate2" style="margin-top: 1rem; margin-left: 1rem; padding: 0.75rem 1.5rem; background: #0066cc; color: white; border: none; border-radius: 4px; cursor: pointer; font-weight: bold;">
        → Przejdź do Strefy #2 →
      </button>
    </div>

    <div v-else>
      <div style="background: #f5f5f5; padding: 1.5rem; border-radius: 8px;">
        <h2 style="margin-top: 0;">Skanuj swój kod dostępu</h2>
        <p style="color: #666;">Pokaż kod QR ze swojego pulpitu aby wejść do tej strefy.</p>

        <div id="qr-reader-gate1" style="width: 100%; margin: 1.5rem 0;"></div>

        <div style="display:flex; gap: 0.5rem; flex-wrap: wrap; margin: 0.75rem 0 1rem;">
          <button
            @click="requestCameraPermission"
            style="padding: 0.5rem 1rem; background: #ff9800; color: white; border: none; border-radius: 4px; cursor: pointer;"
          >
            {{ cameraRequested ? 'Spróbuj ponownie włączyć kamerę' : 'Włącz kamerę' }}
          </button>
          <button
            @click="submitToken"
            style="padding: 0.5rem 1rem; background: #2c7; color: white; border: none; border-radius: 4px; cursor: pointer;"
          >
            Użyj tokenu ręcznie
          </button>
        </div>

        <p v-if="message" :style="{ color: messageType === 'error' ? '#c00' : '#2c7', margin: '1rem 0', fontWeight: 'bold' }">{{ message }}</p>

        <div v-if="message && message.includes('Brak dostępu do kamery')" style="background: #fff3cd; border: 1px solid #ffc107; border-radius: 4px; padding: 1rem; margin-top: 1rem;">
          <p style="color: #856404; margin: 0;">
            <strong>💡 Wskazówka:</strong> Użyj pola poniżej aby wpisać token ręcznie.
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
              @click="submitToken"
              style="padding: 0.5rem 1rem; background: #ff9800; color: white; border: none; border-radius: 4px; cursor: pointer; font-weight: bold;"
            >
              Weryfikuj
            </button>
          </div>
        </div>
      </div>

      <div style="margin-top: 2rem; text-align: center;">
        <button @click="backToDashboard" style="padding: 0.75rem 1.5rem; background: #ccc; color: #333; border: none; border-radius: 4px; cursor: pointer;">
          ← Anuluj
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, onBeforeUnmount, ref } from 'vue';
import { useRouter } from 'vue-router';
import { Html5Qrcode } from 'html5-qrcode';
import { getRoles, getUsername } from '../services/jwt';

const router = useRouter();
const token = localStorage.getItem('korid_token');
const username = ref<string | null>(getUsername(token));
const userRoles = ref<string[]>(getRoles(token));

const accessGranted = ref(false);
const message = ref('');
const messageType = ref<'error' | 'success'>('error');
const manualToken = ref('');
let scanner: Html5Qrcode | null = null;

interface VerifyResponse { token: string; username: string; roles: string[]; message?: string; }

async function verifyToken(token: string) {
  message.value = '';
  try {
    const res = await fetch('/api/access/verify', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ token })
    });

    if (!res.ok) {
      messageType.value = 'error';
      message.value = '❌ Kod jest nieprawidłowy lub wygasł.';
      return;
    }

    const data = (await res.json()) as VerifyResponse & { Roles?: string[]; Token?: string };
    const roles = data.roles || data.Roles || [];

    // Sprawdzenie czy user ma rolę "admin"
    if (!roles.includes('admin')) {
      messageType.value = 'error';
      message.value = '❌ Dostęp odrzucony! Twoja rola nie posiada uprawnień do tej strefy. Wymagana: admin';
      return;
    }

    messageType.value = 'success';
    message.value = '✅ Kod prawidłowy! Dostęp przyznany!';
    accessGranted.value = true;
    localStorage.setItem('korid_token', data.token || data.Token || '');
  } catch (e: unknown) {
    messageType.value = 'error';
    const err = e as { message?: string } | undefined;
    message.value = `❌ ${err?.message ?? 'Nie udało się zweryfikować kodu'}`;
  }
}

async function startScanner() {
  scanner = new Html5Qrcode('qr-reader-gate1');
  try {
    await scanner.start(
      { facingMode: 'environment' },
      { fps: 10, qrbox: { width: 250, height: 250 } },
      (decodedText) => { void verifyToken(decodedText); },
      () => { /* ignorujemy pojedyncze nieudane klatki */ }
    );
  } catch {
    message.value = 'Brak dostępu do kamery. Kliknij „Włącz kamerę” albo wpisz token ręcznie.';
  }
}

const cameraRequested = ref(false);

async function requestCameraPermission() {
  cameraRequested.value = true;
  try {
    const stream = await navigator.mediaDevices.getUserMedia({ video: true, audio: false });
    stream.getTracks().forEach(track => track.stop());
    await startScanner();
  } catch {
    messageType.value = 'error';
    message.value = 'Przeglądarka zablokowała kamerę. Otwórz ustawienia witryny i pozwól na dostęp do kamery.';
  }
}

async function stopScanner() {
  if (scanner) {
    try { await scanner.stop(); scanner.clear(); } catch { /* noop */ }
    scanner = null;
  }
}

function submitToken() {
  if (manualToken.value.trim()) void verifyToken(manualToken.value.trim());
}

function backToDashboard() {
  router.push('/dashboard');
}

function goToGate2() {
  router.push('/access/gate2');
}

onMounted(startScanner);
onBeforeUnmount(stopScanner);
</script>



