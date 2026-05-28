<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { getLinks, createLink, deleteLink, updateLink, dismissSuggestions } from '@/api/links'
import { getTags, createTag, deleteTag } from '@/api/tags'
import type { LinkDto, TagDto, PagedResult } from '@/api/types'
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

// Tag library
const tags = ref<TagDto[]>([])
const showNewTag = ref(false)
const newTagName = ref('')
const newTagColor = ref('#4f6ef7')
const newTagStart = ref('')
const newTagEnd = ref('')
const tagSaving = ref(false)

// Tag picker per link
const pickerLinkId = ref<string | null>(null)

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

async function loadTags() {
  try {
    tags.value = await getTags()
  } catch {
    // non-critical
  }
}

async function submitCreate() {
  createError.value = ''
  creating.value = true
  try {
    const link = await createLink({
      url: newUrl.value,
      title: newTitle.value.trim() || undefined,
    })
    newUrl.value = ''
    newTitle.value = ''
    if (paged.value) {
      paged.value = {
        ...paged.value,
        items: [link, ...paged.value.items],
        totalCount: paged.value.totalCount + 1,
      }
    }
  } catch (e: unknown) {
    createError.value = e instanceof Error ? e.message : 'Failed to create link'
  } finally {
    creating.value = false
  }
}

async function submitCreateTag() {
  if (!newTagName.value.trim()) return
  tagSaving.value = true
  try {
    const tag = await createTag({
      name: newTagName.value.trim(),
      color: newTagColor.value,
      startDate: newTagStart.value || undefined,
      endDate: newTagEnd.value || undefined,
    })
    tags.value = [...tags.value, tag]
    newTagName.value = ''
    newTagColor.value = '#4f6ef7'
    newTagStart.value = ''
    newTagEnd.value = ''
    showNewTag.value = false
  } catch {
    // ignore
  } finally {
    tagSaving.value = false
  }
}

async function handleDeleteTag(id: string) {
  try {
    await deleteTag(id)
    tags.value = tags.value.filter((t) => t.id !== id)
    // Remove deleted tag from any link in the current page
    if (paged.value) {
      paged.value = {
        ...paged.value,
        items: paged.value.items.map((l) => ({
          ...l,
          tags: l.tags.filter((t) => t.id !== id),
        })),
      }
    }
  } catch {
    // ignore
  }
}

async function toggleTag(link: LinkDto, tagId: string) {
  const currentIds = link.tags.map((t) => t.id)
  const newIds = currentIds.includes(tagId)
    ? currentIds.filter((id) => id !== tagId)
    : [...currentIds, tagId]
  try {
    const updated = await updateLink(link.id, {
      url: link.url,
      title: link.title || link.url,
      tagIds: newIds,
    })
    updateItem(updated)
  } catch {
    // ignore
  }
}

async function handleDismissSuggestions(link: LinkDto) {
  try {
    const updated = await dismissSuggestions(link.id)
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

function togglePicker(linkId: string) {
  pickerLinkId.value = pickerLinkId.value === linkId ? null : linkId
}

function closePicker() {
  pickerLinkId.value = null
}

function handleKeydown(e: KeyboardEvent) {
  if (e.key === 'Escape') closePicker()
}

function handleLogout() {
  auth.logout()
  router.push('/login')
}

onMounted(() => {
  loadPage(1)
  loadTags()
  window.addEventListener('keydown', handleKeydown)
})

onBeforeUnmount(() => {
  window.removeEventListener('keydown', handleKeydown)
})
</script>

<template>
  <div class="layout">
    <header>
      <span class="logo">MyMemory</span>
      <button class="btn-ghost" @click="handleLogout">Sign out</button>
    </header>

    <!-- Close tag picker when clicking outside -->
    <div v-if="pickerLinkId" class="picker-backdrop" @click="closePicker" />

    <main>
      <!-- Add link -->
      <section class="card">
        <h2>Add link</h2>
        <form @submit.prevent="submitCreate" class="add-form">
          <input v-model="newUrl" type="url" placeholder="https://example.com" required />
          <input v-model="newTitle" type="text" placeholder="Title (leave blank to auto-name)" />
          <button type="submit" :disabled="creating" class="btn-primary">
            {{ creating ? 'Saving…' : 'Save' }}
          </button>
        </form>
        <p v-if="createError" class="error">{{ createError }}</p>
      </section>

      <!-- Tags library -->
      <section class="card tags-library">
        <div class="tags-library-header">
          <h2>Tags</h2>
          <button class="btn-ghost btn-sm" @click="showNewTag = !showNewTag">
            {{ showNewTag ? 'Cancel' : '+ New tag' }}
          </button>
        </div>

        <!-- New tag form -->
        <form v-if="showNewTag" @submit.prevent="submitCreateTag" class="new-tag-form">
          <input v-model="newTagName" type="text" placeholder="Tag name" required maxlength="100" />
          <label class="color-label">
            <input v-model="newTagColor" type="color" class="color-input" />
            <span class="color-preview" :style="{ background: newTagColor }" />
          </label>
          <input v-model="newTagStart" type="date" title="Start date (optional)" />
          <input v-model="newTagEnd" type="date" title="End date (optional)" />
          <button type="submit" :disabled="tagSaving" class="btn-primary btn-sm">Add</button>
        </form>

        <!-- Tag list -->
        <div v-if="tags.length" class="tag-library-list">
          <span
            v-for="tag in tags"
            :key="tag.id"
            class="tag-lib-item"
            :style="{ '--tag-color': tag.color }"
          >
            <span class="tag-dot" />
            {{ tag.name }}
            <span v-if="tag.startDate || tag.endDate" class="tag-period">
              {{ tag.startDate ? tag.startDate.slice(0, 7) : '?' }}
              –
              {{ tag.endDate ? tag.endDate.slice(0, 7) : '?' }}
            </span>
            <button class="tag-delete-btn" @click="handleDeleteTag(tag.id)" aria-label="Delete tag">✕</button>
          </span>
        </div>
        <p v-else-if="!showNewTag" class="empty-tags">No tags yet.</p>
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

            <!-- Tags row -->
            <div class="tags-row">
              <span
                v-for="tag in link.tags"
                :key="tag.id"
                class="tag-pill"
                :style="{ '--tag-color': tag.color }"
              >
                <span class="tag-dot" />
                {{ tag.name }}
              </span>

              <!-- Tag picker trigger -->
              <div class="picker-wrap">
                <button
                  class="btn-add-tag"
                  :class="{ active: pickerLinkId === link.id }"
                  @click.stop="togglePicker(link.id)"
                  title="Add / remove tags"
                >
                  +
                </button>

                <div v-if="pickerLinkId === link.id" class="tag-picker" @click.stop>
                  <p v-if="!tags.length" class="picker-empty">No tags yet — create one above.</p>
                  <label
                    v-for="tag in tags"
                    :key="tag.id"
                    class="picker-item"
                  >
                    <input
                      type="checkbox"
                      :checked="link.tags.some((t) => t.id === tag.id)"
                      @change="toggleTag(link, tag.id)"
                    />
                    <span class="picker-dot" :style="{ background: tag.color }" />
                    {{ tag.name }}
                  </label>
                </div>
              </div>
            </div>

            <!-- Suggested tags -->
            <div v-if="link.suggestedTags.length" class="suggested">
              <span class="suggested-label">Suggested:</span>
              <span v-for="s in link.suggestedTags" :key="s" class="tag tag--suggested">{{ s }}</span>
              <button class="btn-dismiss" @click="handleDismissSuggestions(link)">Dismiss</button>
            </div>

            <button class="btn-delete" @click="handleDelete(link.id)" aria-label="Delete">✕</button>
          </li>
        </ul>

        <!-- Pagination -->
        <div v-if="paged && paged.totalPages > 1" class="pagination">
          <button :disabled="page === 1" class="btn-page" @click="loadPage(page - 1)">← Prev</button>
          <span>{{ page }} / {{ paged.totalPages }}</span>
          <button :disabled="page === paged.totalPages" class="btn-page" @click="loadPage(page + 1)">Next →</button>
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

/* Cards */
.card {
  background: #fff;
  border-radius: 10px;
  padding: 1.25rem 1.5rem;
  box-shadow: 0 1px 8px rgba(0, 0, 0, 0.06);
}

.card h2 {
  font-size: 1rem;
  font-weight: 600;
  margin-bottom: 0.75rem;
  color: #333;
}

/* Add link form */
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

/* Tags library */
.tags-library-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 0.75rem;
}

.tags-library-header h2 {
  margin-bottom: 0;
}

.new-tag-form {
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
  align-items: center;
  margin-bottom: 0.75rem;
  padding: 0.75rem;
  background: #f9f9f9;
  border-radius: 8px;
  border: 1px solid #eee;
}

.new-tag-form input[type='text'] {
  flex: 1 1 140px;
  padding: 0.45rem 0.7rem;
  border: 1px solid #ddd;
  border-radius: 6px;
  font: inherit;
  font-size: 0.875rem;
}

.new-tag-form input[type='date'] {
  padding: 0.45rem 0.5rem;
  border: 1px solid #ddd;
  border-radius: 6px;
  font: inherit;
  font-size: 0.8rem;
  color: #555;
}

.new-tag-form input:focus {
  outline: none;
  border-color: #4f6ef7;
}

.color-label {
  display: flex;
  align-items: center;
  gap: 0.3rem;
  cursor: pointer;
}

.color-input {
  width: 0;
  height: 0;
  padding: 0;
  border: none;
  opacity: 0;
  position: absolute;
}

.color-preview {
  display: inline-block;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  border: 2px solid #ddd;
  cursor: pointer;
  flex-shrink: 0;
}

.color-label:hover .color-preview {
  border-color: #aaa;
}

.tag-library-list {
  display: flex;
  flex-wrap: wrap;
  gap: 0.4rem;
}

.tag-lib-item {
  display: inline-flex;
  align-items: center;
  gap: 0.3rem;
  padding: 0.2rem 0.5rem 0.2rem 0.4rem;
  border-radius: 999px;
  font-size: 0.78rem;
  font-weight: 500;
  background: color-mix(in srgb, var(--tag-color) 12%, #fff);
  color: color-mix(in srgb, var(--tag-color) 80%, #000);
  border: 1px solid color-mix(in srgb, var(--tag-color) 30%, #fff);
}

.tag-dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background: var(--tag-color);
  flex-shrink: 0;
}

.tag-period {
  font-size: 0.68rem;
  opacity: 0.7;
  margin-left: 0.1rem;
}

.tag-delete-btn {
  background: none;
  border: none;
  padding: 0 0.1rem;
  font-size: 0.65rem;
  color: inherit;
  opacity: 0.5;
  cursor: pointer;
  line-height: 1;
  margin-left: 0.15rem;
}

.tag-delete-btn:hover {
  opacity: 1;
}

.empty-tags {
  font-size: 0.85rem;
  color: #aaa;
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

/* Tags row on link card */
.tags-row {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.35rem;
  min-height: 24px;
}

.tag-pill {
  display: inline-flex;
  align-items: center;
  gap: 0.28rem;
  padding: 0.18rem 0.55rem 0.18rem 0.4rem;
  border-radius: 999px;
  font-size: 0.75rem;
  font-weight: 500;
  background: color-mix(in srgb, var(--tag-color) 12%, #fff);
  color: color-mix(in srgb, var(--tag-color) 80%, #000);
  border: 1px solid color-mix(in srgb, var(--tag-color) 25%, #fff);
}

/* Tag picker */
.picker-wrap {
  position: relative;
}

.btn-add-tag {
  width: 22px;
  height: 22px;
  border-radius: 50%;
  border: 1.5px dashed #ccc;
  background: none;
  color: #aaa;
  font-size: 0.9rem;
  line-height: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.15s;
  padding: 0;
}

.btn-add-tag:hover,
.btn-add-tag.active {
  border-color: #4f6ef7;
  color: #4f6ef7;
  border-style: solid;
}

.picker-backdrop {
  position: fixed;
  inset: 0;
  z-index: 20;
}

.tag-picker {
  position: absolute;
  left: 0;
  top: calc(100% + 6px);
  z-index: 30;
  background: #fff;
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
  padding: 0.4rem 0;
  min-width: 160px;
  max-height: 200px;
  overflow-y: auto;
}

.picker-empty {
  padding: 0.5rem 0.75rem;
  font-size: 0.8rem;
  color: #aaa;
}

.picker-item {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  padding: 0.35rem 0.75rem;
  font-size: 0.85rem;
  color: #333;
  cursor: pointer;
  transition: background 0.1s;
}

.picker-item:hover {
  background: #f5f5f5;
}

.picker-item input[type='checkbox'] {
  accent-color: #4f6ef7;
  width: 14px;
  height: 14px;
  flex-shrink: 0;
}

.picker-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  flex-shrink: 0;
}

/* Suggested tags */
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

.tag {
  padding: 0.2rem 0.55rem;
  border-radius: 999px;
  font-size: 0.75rem;
  font-weight: 500;
}

.tag--suggested {
  background: #fef3c7;
  color: #92400e;
}

.btn-dismiss {
  margin-left: auto;
  padding: 0.2rem 0.6rem;
  background: none;
  border: 1px solid #f59e0b;
  color: #92400e;
  border-radius: 5px;
  font-size: 0.72rem;
  font-weight: 500;
  cursor: pointer;
  transition: background 0.15s;
}

.btn-dismiss:hover {
  background: #fef3c7;
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

/* Shared buttons */
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

.btn-primary.btn-sm {
  padding: 0.4rem 0.8rem;
  font-size: 0.8rem;
}

.btn-ghost {
  background: none;
  border: 1px solid #ddd;
  border-radius: 6px;
  padding: 0.35rem 0.8rem;
  font-size: 0.875rem;
  color: #555;
  transition: background 0.15s;
  cursor: pointer;
}

.btn-ghost:hover {
  background: #f5f5f5;
}

.btn-ghost.btn-sm {
  padding: 0.25rem 0.65rem;
  font-size: 0.8rem;
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
  cursor: pointer;
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
