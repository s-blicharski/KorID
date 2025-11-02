import { defineConfig } from 'vite';
import plugin from '@vitejs/plugin-vue';

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [plugin()],
  server: {
    host: true,
    port: parseInt(process.env.PORT ?? "5173"),
    proxy: {
      '/api': {
        target:
          process.env.services__koridapi__https__0 ||
          process.env.services__koridapi__http__0,
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/api/, ''),
        secure: false
      }
    }
  },
  preview: {
    host: true,
    port: parseInt(process.env.PORT ?? "5173")
  }
});
