export interface Ticket {
  id: number;
  title: string;
  description: string;
  status: string;
  priority: string;
  categoryId: number;
  categoryName: string;
  createdByUserId: number;
  createdByUserName: string;
  assignedToUserId: number | null;
  assignedToUserName?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;

  limit?: number;
  offset?: number;
}

export interface TicketComment {
  id: number;
  ticketId: number;
  userId: number;
  userName: string;
  content: string;
  createdAt: string;
}

export interface TicketStatusHistoryItem {
  id: number;
  ticketId: number;
  oldStatus: string;
  newStatus: string;
  changedByUserId: number;
  changedByUserName: string;
  changedAt: string;
}

export interface Category {
  id: number;
  name: string;
  description: string;
}

export interface CreateTicketRequest {
  title: string;
  description: string;
  categoryId: number;
  createdByUserId: number;
  assignedToUserId: number | null;
  priority: number;
}