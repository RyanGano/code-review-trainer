import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  build: {
    rollupOptions: {
      // redirect.html is the MSAL redirect bridge page that popups land on.
      input: {
        main: 'index.html',
        redirect: 'redirect.html',
      },
    },
  },
})
