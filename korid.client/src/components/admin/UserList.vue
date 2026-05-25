<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue'
import { fetchWrapper } from '../../services/fetchWrapper';

interface User {
  id: number;
  username: string;
  email: string;
  role?: string;
}

const users = ref<User[]>([])
const loading = ref(true)
const editingUser = ref<User | null>(null)

const editForm = reactive({
  username: '',
  email: ''
})

onMounted(async () => {
  try {
    users.value = await fetchWrapper.get<User[]>('/users');
  } catch (error) {
    console.error('Błąd pobierania użytkowników:', error)
  } finally {
    loading.value = false
  }
})

const deleteUser = async (id: number) => {
  if (!confirm('Czy na pewno chcesz usunąć tego użytkownika?')) return
  try {
    await fetchWrapper.del(`/users/${id}`);
    users.value = users.value.filter(u => u.id !== id);
  } catch (error) {
    console.error('Błąd usuwania:', error)
  }
}

const saveUser = async () => {
  if (!editingUser.value) return
  try {
    await fetchWrapper.put(`/users/${editingUser.value.id}`, editForm);
    const idx = users.value.findIndex(u => u.id === editingUser.value?.id);
    if (idx !== -1) {
      const userToUpdate = users.value[idx];

      if (userToUpdate) {
        userToUpdate.username = editForm.username;
        userToUpdate.email = editForm.email;
      }
    }
    editingUser.value = null;   // zamknij modal
  } catch (error) {
    console.error('Błąd edycji:', error)
  }
}

const editUser = (id: number) => {
  const user = users.value.find(u => u.id === id)
  if (user) {
    editingUser.value = user
    editForm.username = user.username
    editForm.email = user.email
  }
}
</script>

<template>
  <div class="admin-panel">
    <h2>Lista Użytkowników</h2>

    <div v-if="loading">Ładowanie danych...</div>

    <table v-else class="data-table">
      <thead>
        <tr>
          <th>ID</th>
          <th>Nazwa użytkownika</th>
          <th>Email</th>
          <th>Rola</th>
          <th>Akcje</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="user in users" :key="user.id">
          <td>{{ user.id }}</td>
          <td>{{ user.username }}</td>
          <td>{{ user.email }}</td>
          <td>{{ user.role }}</td>
          <td>
            <button @click="editUser(user.id)" class="btn">Edytuj</button>
            <button class="btn danger" @click="deleteUser(user.id)">Usuń</button>
          </td>
        </tr>
      </tbody>
    </table>

    <!-- Prosty Modal Edycji -->
    <div v-if="editingUser" class="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
      <div class="bg-white w-full max-w-md rounded-lg shadow-xl p-6 mx-4">
        <h3 class="text-lg font-semibold mb-4">Edycja użytkownika</h3>
        <form @submit.prevent="saveUser" class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Nazwa użytkownika</label>
            <input
              v-model="editForm.username"
              required
              class="block w-full rounded-md border border-gray-300 px-3 py-2 text-sm text-gray-900 placeholder-gray-400 shadow-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
          </div>

          <div>
            <label class="block text-sm font-medium text-gray-700 mb-1">Email</label>
            <input
              v-model="editForm.email"
              type="email"
              required
              class="block w-full rounded-md border border-gray-300 px-3 py-2 text-sm text-gray-900 placeholder-gray-400 shadow-sm focus:outline-none focus:ring-2 focus:ring-indigo-500"
            />
          </div>

          <div class="flex justify-end gap-3 mt-2">
            <button
              type="button"
              @click="editingUser = null"
              class="btn px-4 py-2 bg-gray-100 text-gray-700 rounded-md hover:bg-gray-200 transition"
            >
              Anuluj
            </button>
            <button
              type="submit"
              class="btn px-4 py-2 bg-indigo-600 text-white rounded-md hover:bg-indigo-700 transition"
            >
              Zapisz
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<style scoped>
.admin-panel {
  padding: 1rem;
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
  margin-right: 0.5rem;
}

.danger {
  color: red;
}

/* Style dla modala */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0,0,0,0.5);
  display: flex;
  justify-content: center;
  align-items: center;
}

.modal {
  background: white; /* lub var(--color-background) jeśli używasz zmiennych */
  padding: 2rem;
  border-radius: 8px;
  width: 400px;
  color: black; /* Wymuszamy czarny tekst dla czytelności na białym tle */
}

.form-group {
  margin-bottom: 1rem;
  display: flex;
  flex-direction: column;
}

.actions {
  display: flex;
  justify-content: flex-end;
  gap: 1rem;
  margin-top: 1rem;
}
</style>
