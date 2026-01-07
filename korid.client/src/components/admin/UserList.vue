<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue'

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
    // Pobieramy dane z endpointu API
    const response = await fetch('http://localhost:5222/api/users')
    if (response.ok) {
      const data = await response.json()
      users.value = data
    }
  } catch (error) {
    console.error('Błąd pobierania użytkowników:', error)
  } finally {
    loading.value = false
  }
})

const deleteUser = async (id: number) => {
  if (!confirm('Czy na pewno chcesz usunąć tego użytkownika?')) return

  try {
    const response = await fetch(`http://localhost:5222/api/users/${id}`, {
      method: 'DELETE'
    })

    if (response.ok) {
      users.value = users.value.filter(u => u.id !== id)
    } else {
      alert('Wystąpił błąd podczas usuwania.')
    }
  } catch (error) {
    console.error('Błąd usuwania:', error)
  }
}

const saveUser = async () => {
  if (!editingUser.value) return

  try {
    const response = await fetch(`http://localhost:5222/api/users/${editingUser.value.id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(editForm)
    })

    if (response.ok) {
      // Aktualizujemy dane na liście lokalnej
      const index = users.value.findIndex(u => u.id === editingUser.value?.id)
      if (index !== -1) {
        users.value[index].username = editForm.username
        users.value[index].email = editForm.email
      }
      editingUser.value = null // Zamykamy modal
    } else {
      const errorData = await response.json().catch(() => null)
      alert(errorData?.message || 'Wystąpił błąd podczas zapisu.')
    }
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
            <button @click="editUser(user.id)">Edytuj</button>
            <button class="danger" @click="deleteUser(user.id)">Usuń</button>
          </td>
        </tr>
      </tbody>
    </table>

    <!-- Prosty Modal Edycji -->
    <div v-if="editingUser" class="modal-overlay">
      <div class="modal">
        <h3>Edycja użytkownika</h3>
        <form @submit.prevent="saveUser">
          <div class="form-group">
            <label>Nazwa użytkownika:</label>
            <input v-model="editForm.username" required />
          </div>
          <div class="form-group">
            <label>Email:</label>
            <input v-model="editForm.email" type="email" required />
          </div>
          <div class="actions">
            <button type="button" @click="editingUser = null">Anuluj</button>
            <button type="submit">Zapisz</button>
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