<template>
  <div class="qr-generator-card">
    <h3>Twoja Przepustka</h3>

    <div v-if="passCode" class="qr-code-wrapper">
      <qrcode-vue :value="passCode" :size="250" level="H" />
      <p class="timer">Przepustka wygaśnie za: <strong>{{ timeLeft }}</strong> s</p>
    </div>

    <button v-else @click="generatePass" class="btn btn-primary">
      Generuj Przepustkę (Ważna 5 minut)
    </button>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import QrcodeVue from 'qrcode.vue';
import { fetchWrapper } from '@/services/fetchWrapper';

const passCode = ref('');
const timeLeft = ref(0);
let timer: ReturnType<typeof setInterval> | null = null;

const generatePass = async () => {
  try {
    const response = (await fetchWrapper.post('/access/generate-pass')) as { passCode: string; expiresIn: number };
    passCode.value = response.passCode;
    timeLeft.value = response.expiresIn;

    if (timer) clearInterval(timer);
    timer = setInterval(() => {
      timeLeft.value--;
      if (timeLeft.value <= 0) {
        clearInterval(timer as ReturnType<typeof setInterval>);
        passCode.value = ''; // Ukrywamy kod, bo stracił ważność
      }
    }, 1000);
  } catch (error) {
    alert("Nie udało się wygenerować przepustki. Upewnij się, że jesteś zalogowany.");
  }
};
</script>

<style scoped>
.qr-generator-card {
  text-align: center;
  padding: 20px;
  border: 1px solid #ddd;
  border-radius: 8px;
  max-width: 400px;
  margin: 0 auto;
}
.qr-code-wrapper { margin-top: 20px; }
.timer { color: red; margin-top: 15px; }
.btn-primary { padding: 10px 20px; cursor: pointer; }
</style>
