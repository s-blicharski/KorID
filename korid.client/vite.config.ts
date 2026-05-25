import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-vue';
import tailwindcss from '@tailwindcss/vite'
import { fileURLToPath, URL } from 'node:url';
// https://vitejs.dev/config/
export default defineConfig({
  plugins: [plugin(),
    tailwindcss(),],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  },
  server: {
    host: true,
    port: parseInt(process.env.PORT ?? "5173"),
    proxy: {
      '/api': {
        target: 'http://localhost:5222',
        changeOrigin: true,
        // Nie usuwaj /api, bo backend go oczekuje!
        secure: false
      }
    }
  },
  preview: {
    host: true,
    port: parseInt(process.env.PORT ?? "5173")
  }
});
