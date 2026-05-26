<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { login } from '@/api/auth'

const router = useRouter()
const auth = useAuthStore()

const email = ref('')
const password = ref('')
const error = ref('')
const loading = ref(false)

async function submit() {
  error.value = ''
  loading.value = true
  try {
    const res = await login(email.value, password.value)
    auth.setToken(res.token)
    router.push('/links')
  } catch (e: unknown) {
    error.value = e instanceof Error ? e.message : 'Login failed'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="page">
    <div class="card">
      <h1>MyMemory</h1>
      <h2>Sign in</h2>

      <form @submit.prevent="submit">
        <div class="field">
          <label>Email</label>
          <input v-model="email" type="email" autocomplete="email" required />
        </div>
        <div class="field">
          <label>Password</label>
          <input v-model="password" type="password" autocomplete="current-password" required />
        </div>

        <p v-if="error" class="error">{{ error }}</p>

        <button type="submit" :disabled="loading" class="btn-primary">
          {{ loading ? 'Signing in…' : 'Sign in' }}
        </button>
      </form>

      <p class="alt-link">No account? <RouterLink to="/register">Register</RouterLink></p>
    </div>
  </div>
</template>

<style scoped>
.page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
}

.card {
  background: #fff;
  border-radius: 10px;
  padding: 2.5rem 2rem;
  width: 100%;
  max-width: 380px;
  box-shadow: 0 2px 16px rgba(0, 0, 0, 0.08);
}

h1 {
  font-size: 1.1rem;
  color: #4f6ef7;
  margin-bottom: 0.25rem;
}

h2 {
  font-size: 1.5rem;
  margin-bottom: 1.5rem;
}

.field {
  display: flex;
  flex-direction: column;
  gap: 0.3rem;
  margin-bottom: 1rem;
}

label {
  font-size: 0.875rem;
  font-weight: 500;
  color: #555;
}

input {
  padding: 0.6rem 0.75rem;
  border: 1px solid #ddd;
  border-radius: 6px;
  font: inherit;
  font-size: 0.95rem;
  transition: border-color 0.15s;
}

input:focus {
  outline: none;
  border-color: #4f6ef7;
}

.error {
  color: #e53e3e;
  font-size: 0.875rem;
  margin-bottom: 0.75rem;
}

.btn-primary {
  width: 100%;
  padding: 0.65rem;
  background: #4f6ef7;
  color: #fff;
  border: none;
  border-radius: 6px;
  font-size: 0.95rem;
  font-weight: 500;
  margin-top: 0.25rem;
  transition: background 0.15s;
}

.btn-primary:hover:not(:disabled) {
  background: #3a58e0;
}

.btn-primary:disabled {
  opacity: 0.6;
}

.alt-link {
  margin-top: 1.25rem;
  text-align: center;
  font-size: 0.875rem;
  color: #666;
}
</style>
