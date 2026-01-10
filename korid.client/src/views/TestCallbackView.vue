<template>
  <div style="padding: 2rem;">
    <h1>Test App Callback</h1>
    <p>Parameters returned from KorID:</p>
    <ul>
      <li v-for="(v,k) in params" :key="k"><strong>{{ k }}:</strong> {{ v }}</li>
    </ul>

    <div style="margin-top:1rem;">
      <em v-if="posted">Posted result to opener and closed the window (if opened as popup).</em>
      <em v-else>Opened directly — you can copy parameters or close this tab.</em>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { computed, onMounted, ref } from 'vue';
  import { useRoute } from 'vue-router';

  const route = useRoute();
  const posted = ref(false);

  const params = computed(() => ({ ...route.query }));

  onMounted(() => {
    // If this page was opened as a popup by the external app, post the params back to the opener and close.
    try {
      if (window.opener && !window.opener.closed) {
        // Send an object so the opener can distinguish types
        window.opener.postMessage({ type: 'korid_callback', params: params.value }, window.location.origin);
        posted.value = true;
        // Give the opener a moment to process then close the popup
        setTimeout(() => {
          try { window.close(); } catch { }
        }, 250);
      }
    } catch (e) {
      // If posting fails due to cross-origin, fall back to leaving params visible for manual copy.
      console.error('Failed to postMessage to opener', e);
    }
  });
</script>
