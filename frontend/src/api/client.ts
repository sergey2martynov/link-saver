import { useAuthStore } from '@/stores/auth'

async function request<T>(method: string, url: string, body?: unknown): Promise<T> {
  const auth = useAuthStore()
  const headers: Record<string, string> = { 'Content-Type': 'application/json' }
  if (auth.token) headers['Authorization'] = `Bearer ${auth.token}`

  const res = await fetch(url, {
    method,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined,
  })

  if (res.status === 204) return null as T

  const data = await res.json()
  if (!res.ok) throw new Error(data?.detail ?? data?.title ?? 'Request failed')
  return data as T
}

export const get = <T>(url: string) => request<T>('GET', url)
export const post = <T>(url: string, body?: unknown) => request<T>('POST', url, body)
export const put = <T>(url: string, body?: unknown) => request<T>('PUT', url, body)
export const del = <T>(url: string) => request<T>('DELETE', url)
