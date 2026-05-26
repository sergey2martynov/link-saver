<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { getLinks, createLink, deleteLink, confirmTags } from '@/api/links'
import type { LinkDto, PagedResult } from '@/api/types'
import { useLinksHub } from '@/composables/useLinksHub'

const router = useRouter()
const auth = useAuthStore()

const paged = ref<PagedResult<LinkDto> | null>(null)
const loadError = ref('')
const page = ref(1)

const newUrl = ref('')
const newTitle = ref('')
const createError = ref('')
const creating = ref(false)

useLinksHub((linkId, name) => {
  if (!paged.value) return
  paged.value = {
    ...paged.value,
    items: paged.value.items.map((l) => (l.id === linkId ? { ...l, title: name } : l)),
  }
})

async function loadPage(p: number) {
  loadError.value = ''
  try {
    paged.value = await getLinks(p)
    page.value = p
  } catch (e: unknown) {
    loadError.value = e instanceof Error ? e.message : 'Failed to load links'
  }
}

async function submitCreate() {
  createError.value = ''
  creating.value = true
  try {
    const titleToSend = newTitle.value.trim() || undefined
    const link = await createLink({ url: newUrl.value, title: titleToSend })
    newUrl.value = ''
    newTitle.value = ''
    // Prepend to current page so it's immediately visible
    if (paged.value) {
      paged.value = {
        ...paged.value,
        items: [link, ...paged.value.items],
        totalCount: paged.value.totalCount + 1,
      }
    }
    // If no title was provided, NamingService will generate one and push it
    // via SignalR — no polling needed.
  } catch (e: unknown) {
    createError.value = e instanceof Error ? e.message : 'Failed to create link'
  } finally {
    creating.value = false
  }
}

async function handleConfirmTags(link: LinkDto) {
  try {
    const updated = await confirmTags(link.id)
    updateItem(updated)
  } catch {
    // ignore
  }
}

async function handleDelete(id: string) {
  try {
    await deleteLink(id)
    if (paged.value) {
      paged.value = {
        ...paged.value,
        items: paged.value.items.filter((l) => l.id !== id),
        totalCount: paged.value.totalCount - 1,
      }
    }
  } catch {
    // ignore
  }
}

function updateItem(updated: LinkDto) {
  if (!paged.value) return
  paged.value = {
    ...paged.value,
    items: paged.value.items.map((l) => (l.id === updated.id ? updated : l)),
  }
}

function handleLogout() {
  auth.logout()
  router.push('/login')
}

onMounted(() => loadPage(1))
</script>

<template>
  <div class="layout">
    <header>
      <span class="logo">MyMemory</span>
      <button class="btn-ghost" @click="handleLogout">Sign out</button>
    </header>

    <main>
      <!-- Add link form -->
      <section class="add-card">
        <h2>Add link</h2>
        <form @submit.prevent="submitCreate" class="add-form">
          <input
            v-model="newUrl"
            type="url"
            placeholder="https://example.com"
            required
          />
          <input
            v-model="newTitle"
            type="text"
            placeholder="Title (leave blank to auto-name)"
          />
          <button type="submit" :disabled="creating" class="btn-primary">
            {{ creating ? 'Saving…' : 'Save' }}
          </button>
        </form>
        <p v-if="createError" class="error">{{ createError }}</p>
      </section>

      <!-- Links list -->
      <section class="links-section">
        <p v-if="loadError" class="error">{{ loadError }}</p>

        <p v-else-if="paged && paged.items.length === 0" class="empty">
          No links yet. Add your first one above.
        </p>

        <ul v-else-if="paged" class="link-list">
          <li v-for="link in paged.items" :key="link.id" class="link-card">
            <div class="link-main">
              <a :href="link.url" target="_blank" rel="noopener" class="link-title">
                {{ link.title || link.url }}
              </a>
              <span v-if="!link.title" class="naming-badge">AI naming…</span>
              <span class="link-url">{{ link.url }}</span>
            </div>

            <!-- Confirmed tags -->
            <div v-if="link.tags.length" class="tags">
              <span v-for="tag in link.tags" :key="tag" class="tag">{{ tag }}</span>
            </div>

            <!-- Suggested tags awaiting confirmation -->
            <div v-if="link.suggestedTags.length" class="suggested">
              <span class="suggested-label">Suggested tags:</span>
              <span v-for="tag in link.suggestedTags" :key="tag" class="tag tag--suggested">
                {{ tag }}
              </span>
              <button class="btn-confirm" @click="handleConfirmTags(link)">Confirm</button>
            </div>

            <button class="btn-delete" @click="handleDelete(link.id)" aria-label="Delete">
              ✕
            </button>
          </li>
        </ul>

        <!-- Pagination -->
        <div v-if="paged && paged.totalPages > 1" class="pagination">
          <button
            :disabled="page === 1"
            class="btn-page"
            @click="loadPage(page - 1)"
          >
            ← Prev
          </button>
          <span>{{ page }} / {{ paged.totalPages }}</span>
          <button
            :disabled="page === paged.totalPages"
            class="btn-page"
            @click="loadPage(page + 1)"
          >
            Next →
          </button>
        </div>
      </section>
    </main>
  </div>
</template>

<style scoped>
.layout {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 1.5rem;
  height: 56px;
  background: #fff;
  border-bottom: 1px solid #e8e8e8;
  position: sticky;
  top: 0;
  z-index: 10;
}

.logo {
  font-weight: 700;
  font-size: 1.1rem;
  color: #4f6ef7;
}

main {
  max-width: 680px;
  width: 100%;
  margin: 2rem auto;
  padding: 0 1rem;
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

/* Add form */
.add-card {
  background: #fff;
  border-radius: 10px;
  padding: 1.25rem 1.5rem;
  box-shadow: 0 1px 8px rgba(0, 0, 0, 0.06);
}

.add-card h2 {
  font-size: 1rem;
  font-weight: 600;
  margin-bottom: 0.75rem;
  color: #333;
}

.add-form {
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
}

.add-form input {
  flex: 1 1 180px;
  padding: 0.55rem 0.75rem;
  border: 1px solid #ddd;
  border-radius: 6px;
  font: inherit;
  font-size: 0.9rem;
}

.add-form input:focus {
  outline: none;
  border-color: #4f6ef7;
}

/* Links list */
.links-section {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.empty {
  text-align: center;
  color: #999;
  padding: 2rem 0;
}

.link-list {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.link-card {
  background: #fff;
  border-radius: 10px;
  padding: 1rem 1.25rem;
  box-shadow: 0 1px 8px rgba(0, 0, 0, 0.06);
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  position: relative;
}

.link-main {
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
  padding-right: 1.5rem;
}

.link-title {
  font-weight: 600;
  color: #1a1a1a;
  font-size: 0.95rem;
}

.link-title:hover {
  color: #4f6ef7;
  text-decoration: none;
}

.naming-badge {
  display: inline-block;
  font-size: 0.7rem;
  color: #7c6c00;
  background: #fef9c3;
  border: 1px solid #fde047;
  border-radius: 999px;
  padding: 0.1rem 0.45rem;
  font-weight: 500;
}

.link-url {
  font-size: 0.8rem;
  color: #999;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

/* Tags */
.tags {
  display: flex;
  flex-wrap: wrap;
  gap: 0.35rem;
}

.tag {
  padding: 0.2rem 0.55rem;
  border-radius: 999px;
  font-size: 0.75rem;
  background: #eef1ff;
  color: #4f6ef7;
  font-weight: 500;
}

.suggested {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.35rem;
  background: #fffbeb;
  border: 1px solid #fde68a;
  border-radius: 6px;
  padding: 0.4rem 0.6rem;
}

.suggested-label {
  font-size: 0.75rem;
  color: #92400e;
  font-weight: 500;
}

.tag--suggested {
  background: #fef3c7;
  color: #92400e;
}

.btn-confirm {
  margin-left: auto;
  padding: 0.25rem 0.65rem;
  background: #4f6ef7;
  color: #fff;
  border: none;
  border-radius: 5px;
  font-size: 0.75rem;
  font-weight: 500;
  transition: background 0.15s;
}

.btn-confirm:hover {
  background: #3a58e0;
}

.btn-delete {
  position: absolute;
  top: 0.75rem;
  right: 0.75rem;
  background: none;
  border: none;
  color: #bbb;
  font-size: 0.85rem;
  line-height: 1;
  padding: 0.2rem;
  transition: color 0.15s;
}

.btn-delete:hover {
  color: #e53e3e;
}

/* Buttons */
.btn-primary {
  padding: 0.55rem 1.1rem;
  background: #4f6ef7;
  color: #fff;
  border: none;
  border-radius: 6px;
  font-size: 0.9rem;
  font-weight: 500;
  white-space: nowrap;
  transition: background 0.15s;
}

.btn-primary:hover:not(:disabled) {
  background: #3a58e0;
}

.btn-primary:disabled {
  opacity: 0.6;
}

.btn-ghost {
  background: none;
  border: 1px solid #ddd;
  border-radius: 6px;
  padding: 0.35rem 0.8rem;
  font-size: 0.875rem;
  color: #555;
  transition: background 0.15s;
}

.btn-ghost:hover {
  background: #f5f5f5;
}

/* Pagination */
.pagination {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 1rem;
  font-size: 0.875rem;
  color: #666;
  padding: 0.5rem 0;
}

.btn-page {
  background: none;
  border: 1px solid #ddd;
  border-radius: 6px;
  padding: 0.3rem 0.75rem;
  font-size: 0.875rem;
  color: #555;
  transition: background 0.15s;
}

.btn-page:hover:not(:disabled) {
  background: #f5f5f5;
}

.btn-page:disabled {
  opacity: 0.4;
  cursor: default;
}

.error {
  color: #e53e3e;
  font-size: 0.875rem;
}
</style>
