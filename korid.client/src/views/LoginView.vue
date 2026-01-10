<template>
  <div style="background-color: #f0f2f5; height: 100vh; display: flex; justify-content: center; align-items: center; margin: 0;">
    <div style="background: white; padding: 40px; border-radius: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.1); text-align: center;">
      <h1 style="color: #2c3e50;">Logowanie</h1>
      <p style="color: #606266;">Wprowadź swoje dane, aby przejść do panelu.</p>
      <button @click="$router.push('/admin')"
              style="background-color: #42b983; color: white; border: none; padding: 10px 20px; border-radius: 4px; cursor: pointer;">
        Zaloguj (test)
      </button>
    </div>
  </div>
</template>
