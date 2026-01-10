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
  },
  {
    path: '/admin',
    name: 'Admin',
    meta: { requiresAdminAuth: true },
    component: () => import('../views/AdminView.vue')
  },
  {
    path: '/admin/login',
    name: 'AdminLoginPage',
    component: () => import('../views/AdminLoginView.vue')
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

// Global guard: require admin token for routes with meta.requiresAdminAuth
router.beforeEach((to) => {
  const requiresAdmin = to.matched.some(record => (record.meta as Record<string, unknown>)?.requiresAdminAuth === true);
  if (!requiresAdmin) {
    return true;
  }

  const token = localStorage.getItem('korid_token');
  if (token) {
    return true;
  }

  // redirect to admin login and preserve original destination
  return { path: '/admin/login', query: { redirect: to.fullPath } };
});

export default router;
