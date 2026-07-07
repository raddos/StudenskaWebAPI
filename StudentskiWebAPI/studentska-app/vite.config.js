import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
  plugins: [vue()],
  server: {
    proxy: {
      // Sve /api/* rute idu na lokalni C# Web API
      // PROMIJENI PORT ako tvoj API koristi drugačiji port!
      '/api': {
        target: 'https://localhost:59499', // port 
        changeOrigin: true,
        secure: false,       // ignorisati self-signed HTTPS certifikat
        rewrite: (path) => path  // putanja ostaje ista: /api/Students → /api/Students
      }
    }
  }
})
