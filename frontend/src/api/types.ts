export interface TagDto {
  id: string
  name: string
  color: string
  createdAt: string
  updatedAt: string | null
}

export interface CreateTagDto {
  name: string
  color: string
}

export interface UpdateTagDto {
  name: string
  color: string
}

export interface LinkDto {
  id: string
  url: string
  title: string
  tags: TagDto[]
  suggestedTags: string[]
  createdAt: string
  updatedAt: string | null
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface LinkFilter {
  tagIds?: string[]
  dateFrom?: string
  dateTo?: string
}

export interface CreateLinkDto {
  url: string
  title?: string
  tagIds?: string[]
}

export interface UpdateLinkDto {
  url: string
  title: string
  tagIds?: string[]
}
