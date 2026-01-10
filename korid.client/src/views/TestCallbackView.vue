<template>
  <div style="padding: 2rem;">
    <h1>Test App Callback</h1>
    <p>Parameters returned from KorID:</p>
    <ul>
      <li v-for="(v,k) in params" :key="k"><strong>{{ k }}:</strong> {{ v }}</li>
    </ul>
  </div>
</template>

<script setup>
import { computed } from 'vue';
import { useRoute } from 'vue-router';

const route = useRoute();

const params = computed(() => Object.fromEntries(route.query as Record<string, string>));
</script>
