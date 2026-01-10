<script setup lang="ts">
  import { ref, reactive, onMounted } from 'vue'

  interface Application {
    id?: number;
    Id?: number;
    name?: string;
    Name?: string;
    clientId?: string;
    ClientId?: string;
    clientSecret?: string;
    ClientSecret?: string;
    redirectUri?: string;
    RedirectUri?: string;
    organizationId?: number;
    OrganizationId?: number;
    organization?: { id?: number; name?: string; Name?: string };
    Organization?: { Id?: number; Name?: string };
  }

  interface User {
    id?: number;
    Id?: number;
    username?: string;
    Username?: string;
    email?: string;
    Email?: string;
  }

  interface UserApplication {
    userId: number;
    applicationId: number;
  }

  const API_BASE = import.meta.env.VITE_API_URL || 'http://localhost:5222'

  const applications = ref<Application[]>([])
  const users = ref<User[]>([])
  const userApplications = ref<UserApplication[]>([])
  const loading = ref(true)

  const editingApp = ref<Application | null>(null)
  const creating = ref(false)

  const form = reactive({
    id: 0,
    name: '',
    clientId: '',
    clientSecret: '',
    redirectUri: '',
    organizationId: 0
  })

  // Manage users modal
  const managingApp = ref<Application | null>(null)
  const managingChecked = ref<Record<string, boolean>>({}) // key: `${userId}:${appId}`

  onMounted(async () => {
    await loadAll()
  })

  async function loadAll() {
    loading.value = true
    try {
      const [appsRes, usersRes, uaRes] = await Promise.all([
        fetch(`${API_BASE}/api/applications`),
        fetch(`${API_BASE}/api/users`),
        fetch(`${API_BASE}/api/userApplications`)
      ])

      if (appsRes.ok) applications.value = await appsRes.json()
      else applications.value = []

      if (usersRes.ok) users.value = await usersRes.json()
      else users.value = []

      if (uaRes.ok) {
        const uaData = await uaRes.json()
        userApplications.value = Array.isArray(uaData)
          ? uaData.map((x: any) => ({ userId: x.userId ?? x.UserId ?? x.User?.Id ?? x.User?.id, applicationId: x.applicationId ?? x.ApplicationId ?? x.Application?.Id ?? x.Application?.id }))
          : []
      } else {
        userApplications.value = []
      }
    } catch (e) {
      console.error('Failed to load applications/users', e)
    } finally {
      loading.value = false
    }
  }

  function normalizeAppId(app: Application) {
    return app.id ?? app.Id ?? 0
  }
  function normalizeUserId(u: User) {
    return u.id ?? u.Id ?? 0
  }
  function getAppName(a: Application) {
    return a.name ?? a.Name ?? ''
  }
  function getClientId(a: Application) {
    return a.clientId ?? a.ClientId ?? ''
  }
  function getOrgName(a: Application) {
    return a.organization?.name ?? a.Organization?.Name ?? a.organization?.Name ?? ''
  }

  // Create
  function openCreate() {
    creating.value = true
    editingApp.value = null
    form.id = 0
    form.name = ''
    form.clientId = ''
    form.clientSecret = ''
    form.redirectUri = ''
    form.organizationId = 0
  }

  async function saveCreate() {
    try {
      const payload = {
        name: form.name,
        clientId: form.clientId,
        clientSecret: form.clientSecret,
        redirectUri: form.redirectUri,
        organizationId: Number(form.organizationId)
      }
      const res = await fetch(`${API_BASE}/api/applications`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      })
      if (!res.ok) {
        const txt = await res.text().catch(() => '')
        alert('Create failed: ' + res.status + ' ' + txt)
        return
      }
      await loadAll()
      creating.value = false
    } catch (e) {
      console.error('Create error', e)
      alert('Create error: ' + (e as Error).message)
    }
  }

  // Edit
  function openEdit(app: Application) {
    editingApp.value = app
    creating.value = false
    form.id = normalizeAppId(app)
    form.name = getAppName(app)
    form.clientId = getClientId(app)
    form.clientSecret = app.clientSecret ?? (app.ClientSecret ?? '')
    form.redirectUri = app.redirectUri ?? (app.RedirectUri ?? '')
    form.organizationId = Number(app.organizationId ?? app.OrganizationId ?? app.organization?.id ?? app.Organization?.Id ?? 0)
  }

  async function saveEdit() {
    try {
      const id = form.id
      const payload = {
        id,
        name: form.name,
        clientId: form.clientId,
        clientSecret: form.clientSecret,
        redirectUri: form.redirectUri,
        organizationId: Number(form.organizationId)
      }
      const res = await fetch(`${API_BASE}/api/applications/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      })
      if (!res.ok) {
        const txt = await res.text().catch(() => '')
        alert('Update failed: ' + res.status + ' ' + txt)
        return
      }
      await loadAll()
      editingApp.value = null
    } catch (e) {
      console.error('Update error', e)
      alert('Update error: ' + (e as Error).message)
    }
  }

  async function deleteApp(app: Application) {
    if (!confirm('Czy na pewno usun�� aplikacj�?')) return
    const id = normalizeAppId(app)
    try {
      const res = await fetch(`${API_BASE}/api/applications/${id}`, { method: 'DELETE' })
      if (!res.ok) {
        alert('Delete failed: ' + res.status)
        return
      }
      await loadAll()
    } catch (e) {
      console.error('Delete error', e)
    }
  }

  // Manage users for an application
  function openManageUsers(app: Application) {
    managingApp.value = app
    // prepare checked map
    managingChecked.value = {}
    const appId = normalizeAppId(app)
    for (const u of users.value) {
      const uid = normalizeUserId(u)
      const key = `${uid}:${appId}`
      managingChecked.value[key] = userApplications.value.some(ua => ua.userId === uid && ua.applicationId === appId)
    }
  }

  function closeManageUsers() {
    managingApp.value = null
    managingChecked.value = {}
  }

  async function toggleUserForApp(u: User, app: Application) {
    const uid = normalizeUserId(u)
    const aid = normalizeAppId(app)
    const key = `${uid}:${aid}`
    const shouldHave = !managingChecked.value[key]

    try {
      if (shouldHave) {
        // create grant
        const payload = { userId: uid, applicationId: aid, grantedAt: new Date().toISOString() }
        const res = await fetch(`${API_BASE}/api/userApplications`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(payload)
        })
        if (!res.ok) {
          const txt = await res.text().catch(() => '')
          alert('Grant failed: ' + res.status + ' ' + txt)
          return
        }
        managingChecked.value[key] = true
        userApplications.value.push({ userId: uid, applicationId: aid })
      } else {
        // remove grant
        const res = await fetch(`${API_BASE}/api/userApplications/${uid}/${aid}`, { method: 'DELETE' })
        if (!res.ok) {
          alert('Revoke failed: ' + res.status)
          return
        }
        managingChecked.value[key] = false
        userApplications.value = userApplications.value.filter(x => !(x.userId === uid && x.applicationId === aid))
      }
    } catch (e) {
      console.error('Toggle grant error', e)
      alert('Operation failed: ' + (e as Error).message)
    }
  }
</script>

<template>
  <div class="admin-panel">
    <h2>Lista Aplikacji</h2>

    <div style="margin-bottom:0.5rem;">
      <button @click="openCreate">Dodaj aplikacj�</button>
    </div>

    <div v-if="loading">�adowanie aplikacji...</div>

    <table v-else class="data-table">
      <thead>
        <tr>
          <th>ID</th>
          <th>Nazwa</th>
          <th>ClientId</th>
          <th>Organization</th>
          <th>Redirect URI</th>
          <th>Akcje</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="app in applications" :key="normalizeAppId(app)">
          <td>{{ normalizeAppId(app) }}</td>
          <td>{{ getAppName(app) }}</td>
          <td>{{ getClientId(app) }}</td>
          <td>{{ getOrgName(app) }}</td>
          <td>{{ app.redirectUri ?? app.RedirectUri }}</td>
          <td>
            <button @click="openEdit(app)">Edytuj</button>
            <button class="danger" @click="deleteApp(app)">Usu�</button>
            <button @click="openManageUsers(app)">Zarz�dzaj u�ytkownikami</button>
          </td>
        </tr>
      </tbody>
    </table>

    <!-- Create Modal -->
    <div v-if="creating" class="modal-overlay">
      <div class="modal">
        <h3>Dodaj aplikacj�</h3>
        <form @submit.prevent="saveCreate">
          <div class="form-group">
            <label>Nazwa</label>
            <input v-model="form.name" required />
          </div>
          <div class="form-group">
            <label>ClientId</label>
            <input v-model="form.clientId" required />
          </div>
          <div class="form-group">
            <label>ClientSecret</label>
            <input v-model="form.clientSecret" required />
          </div>
          <div class="form-group">
            <label>Redirect URI</label>
            <input v-model="form.redirectUri" />
          </div>
          <div class="form-group">
            <label>OrganizationId</label>
            <input v-model="form.organizationId" type="number" />
          </div>
          <div class="actions">
            <button type="button" @click="creating = false">Anuluj</button>
            <button type="submit">Utw�rz</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Edit Modal -->
    <div v-if="editingApp" class="modal-overlay">
      <div class="modal">
        <h3>Edytuj aplikacj�</h3>
        <form @submit.prevent="saveEdit">
          <div class="form-group">
            <label>Nazwa</label>
            <input v-model="form.name" required />
          </div>
          <div class="form-group">
            <label>ClientId</label>
            <input v-model="form.clientId" required />
          </div>
          <div class="form-group">
            <label>ClientSecret</label>
            <input v-model="form.clientSecret" />
          </div>
          <div class="form-group">
            <label>Redirect URI</label>
            <input v-model="form.redirectUri" />
          </div>
          <div class="form-group">
            <label>OrganizationId</label>
            <input v-model="form.organizationId" type="number" />
          </div>
          <div class="actions">
            <button type="button" @click="editingApp = null">Anuluj</button>
            <button type="submit">Zapisz</button>
          </div>
        </form>
      </div>
    </div>

    <!-- Manage Users Modal -->
    <div v-if="managingApp" class="modal-overlay">
      <div class="modal" style="width:700px; max-height:80vh; overflow:auto;">
        <h3>Zarz�dzaj u�ytkownikami dla: {{ getAppName(managingApp) }}</h3>
        <p>Lista wszystkich u�ytkownik�w. Zaznacz aby nada� dost�p do tej aplikacji.</p>

        <table class="data-table" style="margin-top:1rem;">
          <thead>
            <tr>
              <th>ID</th>
              <th>Nazwa</th>
              <th>Email</th>
              <th>Ma dost�p</th>
              <th>Akcja</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="u in users" :key="normalizeUserId(u)">
              <td>{{ normalizeUserId(u) }}</td>
              <td>{{ u.username ?? u.Username }}</td>
              <td>{{ u.email ?? u.Email }}</td>
              <td>
                <input type="checkbox"
                       :checked="managingChecked[`${normalizeUserId(u)}:${normalizeAppId(managingApp)}`]"
                       @change="toggleUserForApp(u, managingApp)" />
              </td>
              <td>
                <button @click="toggleUserForApp(u, managingApp)">
                  {{ managingChecked[`${normalizeUserId(u)}:${normalizeAppId(managingApp)}`] ? 'Cofnij dost�p' : 'Nadaj dost�p' }}
                </button>
              </td>
            </tr>
          </tbody>
        </table>

        <div class="actions" style="margin-top:1rem;">
          <button @click="closeManageUsers">Zamknij</button>
        </div>
      </div>
    </div>
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
    margin-right: 0.5rem;
  }

  .danger {
    color: red;
  }

  /* Modal styles */
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
    z-index: 50;
  }

  .modal {
    background: white;
    padding: 1.5rem;
    border-radius: 8px;
    color: black;
  }

  .form-group {
    margin-bottom: 0.75rem;
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
