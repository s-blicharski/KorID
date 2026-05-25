<script setup lang="ts">
import { ref, onBeforeUnmount } from 'vue';

const API_BASE = import.meta.env.VITE_API_URL || 'http://localhost:5222';
const adminQr = ref<string | null>(null);
const viewerQr = ref<string | null>(null);
const objectUrls: string[] = [];

async function genFor(body: Record<string, string>): Promise<string> {
  const token = localStorage.getItem('korid_token');
  const res = await fetch(`${API_BASE}/api/access/generate`, {
    method: 'POST',
    headers: {'Content-Type': 'application/json','Authorization': `Bearer ${token}`},
    body: JSON.stringify(body)
  });
  if (!res.ok) throw new Error(`HTTP ${res.status}`);
  const blob = await res.blob();
  const url = URL.createObjectURL(blob);
  objectUrls.push(url);
  return url;
}

async function generate() {
  adminQr.value = await genFor({ username: 'admin.demo' });
  viewerQr.value = await genFor({ username: 'viewer.demo' });
}

onBeforeUnmount(() => objectUrls.forEach(URL.revokeObjectURL));
</script>

<template>
  <div style="padding:1.5rem;background:#f9f9f9;border-radius:8px;border: 1px solid #ddd;">
    <h2 style="margin-top:0;">📱 Kody dostępu QR</h2>
    <p style="color:#666;margin:0 0 1rem 0;">Kliknij przycisk poniżej, aby wygenerować dwa kody dostępu dla użytkowników:</p>

    <button @click="generate" style="padding:0.75rem 1.5rem;background:#2c7;color:white;border:none;border-radius:4px;cursor:pointer;font-size:16px;font-weight:bold;margin-bottom:1rem;">
      ✨ Wygeneruj parę kodów
    </button>

    <div v-if="adminQr || viewerQr" style="display:flex;gap:2rem;margin-top:1.5rem;flex-wrap:wrap;">
      <div v-if="adminQr" style="text-align:center;">
        <h3 style="color:#0066cc;margin:0 0 0.5rem 0;">👨‍💼 KOD ADMIN</h3>
        <p style="font-size:12px;color:#666;margin:0 0 1rem 0;">Pełne uprawnienia do zarządzania systemem.</p>
        <img :src="adminQr" alt="QR admin" style="width:220px;border:2px solid #0066cc;border-radius:4px;" />
      </div>
      <div v-if="viewerQr" style="text-align:center;">
        <h3 style="color:#888;margin:0 0 0.5rem 0;">👁️ KOD VIEWER</h3>
        <p style="font-size:12px;color:#666;margin:0 0 1rem 0;">Dostęp tylko do odczytu, bez możliwości edycji.</p>
        <img :src="viewerQr" alt="QR viewer" style="width:220px;border:2px solid #888;border-radius:4px;" />
      </div>
    </div>

    <div style="background:#fff3cd;border:1px solid #ffc107;border-radius:4px;padding:1rem;margin-top:1.5rem;">
      <p style="margin:0;color:#856404;font-size:14px;">
        <strong>⏰ Ważne:</strong> Kody są ważne przez 5 minut i można ich użyć tylko raz. Wyświetl je na telefonie i poproś użytkownika aby je zeskanował na stronie <a href="/scan" style="color:#0066cc;text-decoration:underline;">/scan</a>.
      </p>
    </div>
  </div>
</template>

