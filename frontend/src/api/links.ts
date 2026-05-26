import { get, post, put, del } from './client'
import type { LinkDto, PagedResult, CreateLinkDto, UpdateLinkDto } from './types'

export const getLinks = (page = 1) =>
  get<PagedResult<LinkDto>>(`/api/links?page=${page}`)

export const createLink = (dto: CreateLinkDto) =>
  post<LinkDto>('/api/links', dto)

export const updateLink = (id: string, dto: UpdateLinkDto) =>
  put<LinkDto>(`/api/links/${id}`, dto)

export const deleteLink = (id: string) =>
  del<null>(`/api/links/${id}`)

export const confirmTags = (id: string) =>
  post<LinkDto>(`/api/links/${id}/tags/confirm`)
