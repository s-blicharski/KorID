
<script setup lang="ts">
import { computed } from 'vue';
import { useRoute } from 'vue-router';
const route = useRoute();
const routeName = computed(() => (typeof route.name === 'string' ? route.name : ''));
</script>
<template>
  <div class="navbar bg-base-100 shadow-sm">
    <a class="btn btn-ghost text-xl">KorID / {{routeName}}</a>
    </div>
</template>
