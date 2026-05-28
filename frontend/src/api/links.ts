import { get, post, put, del } from './client'
import type { LinkDto, LinkFilter, PagedResult, CreateLinkDto, UpdateLinkDto } from './types'

export const getLinks = (page = 1, filter?: LinkFilter) => {
  const params = new URLSearchParams({ page: String(page) })
  filter?.tagIds?.forEach((id) => params.append('tagIds', id))
  if (filter?.dateFrom) params.set('dateFrom', filter.dateFrom)
  if (filter?.dateTo) params.set('dateTo', filter.dateTo)
  return get<PagedResult<LinkDto>>(`/api/links?${params}`)
}

export const createLink = (dto: CreateLinkDto) =>
  post<LinkDto>('/api/links', dto)

export const updateLink = (id: string, dto: UpdateLinkDto) =>
  put<LinkDto>(`/api/links/${id}`, dto)

export const deleteLink = (id: string) =>
  del<null>(`/api/links/${id}`)

export const dismissSuggestions = (id: string) =>
  del<LinkDto>(`/api/links/${id}/suggestions`)
