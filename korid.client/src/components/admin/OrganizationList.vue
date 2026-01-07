<script setup lang="ts">
import { ref } from 'vue'

// Statyczne dane do makiety
const organizations = ref([
  { id: 1, name: 'Korpo A', nip: '1234567890', status: 'Aktywna' },
  { id: 2, name: 'Firma B', nip: '0987654321', status: 'Zawieszona' }
])
</script>

<template>
  <div class="admin-panel">
    <h2>Lista Organizacji</h2>
    
    <table class="data-table">
      <thead>
        <tr>
          <th>ID</th>
          <th>Nazwa</th>
          <th>NIP</th>
          <th>Status</th>
          <th>Akcje</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="org in organizations" :key="org.id">
          <td>{{ org.id }}</td>
          <td>{{ org.name }}</td>
          <td>{{ org.nip }}</td>
          <td>{{ org.status }}</td>
          <td>
            <button>Szczegóły</button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style scoped>
.admin-panel {
  padding: 1rem;
  margin-top: 2rem;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 1rem;
}

.data-table th, .data-table td {
  border: 1px solid var(--color-border);
  padding: 0.5rem;
  text-align: left;
}

button {
  cursor: pointer;
}
</style>