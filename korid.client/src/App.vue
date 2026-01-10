
<script setup lang="ts">
import HeaderComponent from "./components/HeaderComponent.vue";
import Footer from "./components/footer.vue";
</script>
<template>
  <HeaderComponent />
  <router-view />
  <Footer />
</template>

<style>
  body {
    margin: 0;
    padding: 0;
    font-family: sans-serif;
  }
</style>
