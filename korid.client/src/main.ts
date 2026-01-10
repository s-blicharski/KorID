import './assets/main.css'

import { createApp } from 'vue'
import App from './App.vue'
import router from './router' // To łączy się z Twoim plikiem router/index.ts

const app = createApp(App)

app.use(router) // To aktywuje mapę stron w całej aplikacji
app.mount('#app')
