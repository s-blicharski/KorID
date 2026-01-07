<script setup lang="ts">
import UserList from './components/admin/UserList.vue'
import OrganizationList from './components/admin/OrganizationList.vue'

console.log("Admin UI Mockup")
</script>

<template>
  <header>
    <img alt="Vue logo" class="logo" src="./assets/logo.svg" width="125" height="125" />
    <h1>Panel Administratora (Makieta)</h1>
  </header>

  <main>
    <UserList />
    <hr />
    <OrganizationList />
  </main>
</template>

<style scoped>
header {
  line-height: 1.5;
}

.logo {
  display: block;
  margin: 0 auto 2rem;
}

@media (min-width: 1024px) {
  header {
    display: flex;
    place-items: center;
    padding-right: calc(var(--section-gap) / 2);
  }

  .logo {
    margin: 0 2rem 0 0;
  }
}
</style>
