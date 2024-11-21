import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  // /* @ts-ignore: Unreachable code error */
  // history: createWebHistory(import.meta.env.BASE_URL),
  history: createWebHistory('/dashboard'),
  routes: [
    {
      path: '/',
      name: 'home',
      component: () => import('../views/HomeView.vue'),
    },
    {
      path: '',
      name: 'empty-home',
      component: () => import('../views/HomeView.vue'),
    },
    {
      path: '/documentation',
      name: 'documentation',
      component: () => import('../views/Documentation.vue'),
    },
  ],
})

export default router
