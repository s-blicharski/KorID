<template>
  <div class="qr-scanner-card">
    <h3>Skaner Przepustek (Tryb Ochrony)</h3>

    <div v-if="!scanResult" id="reader" class="scanner-window"></div>

    <div v-else class="result-window">
      <div :class="['alert', isSuccess ? 'alert-success' : 'alert-danger']">
        {{ resultMessage }}
      </div>
      <button @click="resetScanner" class="btn btn-secondary mt-3">
        Skanuj następną osobę
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';
import { Html5QrcodeScanner } from 'html5-qrcode';
import { fetchWrapper } from '@/services/fetchWrapper';

const scanResult = ref('');
const isSuccess = ref(false);
const resultMessage = ref('');
let scanner: Html5QrcodeScanner | null = null;

onMounted(() => {
  startScanner();
});

onUnmounted(() => {
  if (scanner) {
    scanner.clear().catch(error => console.error("Błąd zatrzymywania kamery:", error));
  }
});

const startScanner = () => {
  scanResult.value = '';
  scanner = new Html5QrcodeScanner(
    "reader",
    { fps: 10, qrbox: { width: 250, height: 250 } },
    false
  );
  scanner.render(onScanSuccess, onScanFailure);
};

const onScanSuccess = async (decodedText: string) => {
  // Od razu wyłączamy kamerę po odczycie
  if (scanner) {
    await scanner.clear();
  }
  scanResult.value = decodedText;

  try {
    // Wysyłamy kod do backendu celem weryfikacji i zużycia
    const response = (await fetchWrapper.post('/access/validate-pass', { PassCode: decodedText })) as { message: string };
    isSuccess.value = true;
    resultMessage.value = response.message;
  } catch (error: any) {
    // Przechwycenie błędu (np. z fetchWrapper gdy dostaniemy 403 Forbidden)
    isSuccess.value = false;
    resultMessage.value = error || 'Przepustka odrzucona (Nieważna lub zużyta).';
  }
};

const onScanFailure = () => {
  // Funkcja odpala się ciągle, gdy kamera nie widzi kodu. Ignorujemy to.
};

const resetScanner = () => {
  startScanner();
};
</script>

<style scoped>
.qr-scanner-card {
  max-width: 500px;
  margin: 0 auto;
  text-align: center;
  padding: 15px;
}
.scanner-window { width: 100%; }
.alert { padding: 15px; border-radius: 4px; font-weight: bold; margin-top: 20px;}
.alert-success { background-color: #d4edda; color: #155724; border: 1px solid #c3e6cb; }
.alert-danger { background-color: #f8d7da; color: #721c24; border: 1px solid #f5c6cb; }
.btn-secondary { padding: 8px 16px; cursor: pointer; margin-top: 15px; }
</style>
