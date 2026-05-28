import { get, post, put, del } from './client'
import type { TagDto, CreateTagDto, UpdateTagDto } from './types'

export const getTags = () => get<TagDto[]>('/api/tags')
export const createTag = (dto: CreateTagDto) => post<TagDto>('/api/tags', dto)
export const updateTag = (id: string, dto: UpdateTagDto) => put<TagDto>(`/api/tags/${id}`, dto)
export const deleteTag = (id: string) => del<null>(`/api/tags/${id}`)
