import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router';
const routes: Array<RouteRecordRaw> = [
  {
    path: '/',
    redirect: '/login'
  },
  {
    path: '/login',
    name: 'Login',
    component: () => import('../views/LoginView.vue')
  }, // <-- Pamiętaj o tym przecinku!
  {
    path: '/admin',
    name: 'Admin',
    component: () => import('../views/AdminView.vue')
  },
  {
    path: '/test',
    name: 'TestApp',
    component: () => import('../views/TestAppView.vue')
  },
  {
    path: '/test/callback',
    name: 'TestCallback',
    component: () => import('../views/TestCallbackView.vue')
  }
];
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
});

export default router;
