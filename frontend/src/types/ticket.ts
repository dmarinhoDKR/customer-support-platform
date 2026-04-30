export interface Ticket {
    id: number;
    title: string;
    description: string;
    status: string;
    priority: string;
    categoryId: number;
    categoryName: string;
    createdByUserId: number;
    assignedToUserId: number | null;
    createdAt: string;
    updatedAt: string;
}

export interface PagedResult<T> {
    items: T[];
    pageNumber: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
}