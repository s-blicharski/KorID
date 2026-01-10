<script setup lang="ts">
import { ref, onMounted, reactive } from 'vue';
import { fetchWrapper } from '../../services/fetchWrapper';

interface User {
  id: number;
  username: string;
  email: string;
  role?: string;
}

const users = ref<User[]>([]);
const loading = ref(true);
const editingUser = ref<User | null>(null);

const editForm = reactive({
  username: '',
  email: ''
});

onMounted(async () => {
  try {
    // use fetchWrapper which prefixes /api and attaches token
    const data = await fetchWrapper.get<User[]>('/users');
    users.value = data ?? [];
  } catch (error) {
    console.error('B³¹d pobierania u¿ytkowników:', error);
    alert('Nie uda³o siê pobraæ u¿ytkowników. SprawdŸ konsolê.');
  } finally {
    loading.value = false;
  }
});

const deleteUser = async (id: number) => {
  if (!confirm('Czy na pewno chcesz usun¹æ tego u¿ytkownika?')) return;

  try {
    await fetchWrapper.del(`/users/${id}`);
    users.value = users.value.filter(u => u.id !== id);
  } catch (error) {
    console.error('B³¹d usuwania:', error);
    alert('Wyst¹pi³ b³¹d podczas usuwania.');
  }
};

const saveUser = async () => {
  if (!editingUser.value) return;

  try {
    await fetchWrapper.put(`/users/${editingUser.value.id}`, {
      username: editForm.username,
      email: editForm.email
    });
    const index = users.value.findIndex(u => u.id === editingUser.value?.id);
    if (index !== -1) {
      users.value[index].username = editForm.username;
      users.value[index].email = editForm.email;
    }
    editingUser.value = null;
  } catch (error) {
    console.error('B³¹d edycji:', error);
    const err = error as { message?: string } | undefined;
    alert(err?.message ?? 'Wyst¹pi³ b³¹d podczas zapisu.');
  }
};

const editUser = (id: number) => {
  const user = users.value.find(u => u.id === id);
  if (user) {
    editingUser.value = user;
    editForm.username = user.username;
    editForm.email = user.email;
  }
};
</script>

<template>
  <div class="admin-panel">
    <h2>Lista U¿ytkowników</h2>

    <div v-if="loading">£adowanie danych...</div>

    <table v-else class="data-table">
      <thead>
        <tr>
          <th>ID</th>
          <th>Nazwa u¿ytkownika</th>
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
            <button class="danger" @click="deleteUser(user.id)">Usuñ</button>
          </td>
        </tr>
      </tbody>
    </table>

    <!-- Prosty Modal Edycji -->
    <div v-if="editingUser" class="modal-overlay">
      <div class="modal">
        <h3>Edycja u¿ytkownika</h3>
        <form @submit.prevent="saveUser">
          <div class="form-group">
            <label>Nazwa u¿ytkownika:</label>
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
  background: white;
  padding: 2rem;
  border-radius: 8px;
  width: 400px;
  color: black;
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