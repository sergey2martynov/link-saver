export interface LinkDto {
  id: string
  url: string
  title: string
  description: string | null
  tags: string[]
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

export interface CreateLinkDto {
  url: string
  title?: string
  description?: string
  tags?: string[]
}

export interface UpdateLinkDto {
  url: string
  title: string
  description?: string
  tags?: string[]
}
