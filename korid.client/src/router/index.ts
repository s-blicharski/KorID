import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router';
import { isExpired, hasRole } from '../services/jwt';

const routes: Array<RouteRecordRaw> = [
  { path: '/', redirect: '/scan' },
  { path: '/login', name: 'Login', component: () => import('../views/LoginView.vue') },
  { path: '/admin', name: 'Admin', meta: { requiresRole: 'admin' }, component: () => import('../views/AdminView.vue') },
  { path: '/admin/login', name: 'AdminLoginPage', component: () => import('../views/AdminLoginView.vue') },
  { path: '/scan', name: 'Scan', component: () => import('../views/ScanView.vue') },
  { path: '/dashboard', name: 'Dashboard', meta: { requiresAuth: true }, component: () => import('../views/DashboardView.vue') },
  { path: '/access/gate1', name: 'AccessGate1', meta: { requiresAuth: true }, component: () => import('../views/AccessGate1View.vue') },
  { path: '/access/gate2', name: 'AccessGate2', meta: { requiresAuth: true }, component: () => import('../views/AccessGate2View.vue') },
  { path: '/test', name: 'TestApp', component: () => import('../views/TestAppView.vue') },
  { path: '/test/callback', name: 'TestCallback', component: () => import('../views/TestCallbackView.vue') }
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
});

router.beforeEach((to) => {
  const requiresRole = to.matched
    .map(r => (r.meta as Record<string, unknown>)?.requiresRole as string | undefined)
    .find(Boolean);
  const requiresAuth = to.matched.some(r => (r.meta as Record<string, unknown>)?.requiresAuth === true) || !!requiresRole;

  if (!requiresAuth) return true;

  const token = localStorage.getItem('korid_token');

  // Waliduj realnie: token istnieje, nie wygasł, ma rolę (jeśli wymagana).
  if (!token || isExpired(token)) {
    localStorage.removeItem('korid_token');
    return { path: '/scan', query: { redirect: to.fullPath } };
  }
  if (requiresRole && !hasRole(token, requiresRole)) {
    // zalogowany, ale bez wymaganej roli — wpuść na pulpit podglądu
    return { path: '/dashboard' };
  }
  return true;
});

export default router;
