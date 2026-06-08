import { useEffect, useState } from "react";
import type { ChangeEvent, CSSProperties, FormEvent } from "react";
import { Link } from "react-router-dom";
import { api } from "../services/api";
import type { Category, CreateTicketRequest, PagedResult, Ticket } from "../types/ticket";

const statusOptions = [
    { label: "All Statuses", value: "" },
    { label: "Open", value: "1" },
    { label: "In Progress", value: "2" },
    { label: "Waiting Customer", value: "3" },
    { label: "Resolved", value: "4" },
    { label: "Closed", value: "5" },
];

const priorityOptions = [
    { label: "All Priorities", value: "" },
    { label: "Low", value: "1" },
    { label: "Medium", value: "2" },
    { label: "High", value: "3" },
    { label: "Critical", value: "4" },
];

function getStatusBadgeStyle(status: string): CSSProperties {
    switch (status) {
        case "Open":
            return {
                backgroundColor: "#dbeafe",
                color: "#1d4ed8",
            };
        case "InProgress":
            return {
                backgroundColor: "#fef3c7",
                color: "#b45309",
            };
        case "WaitingCustomer":
            return {
                backgroundColor: "#ede9fe",
                color: "#6d28d9",
            };
        case "Resolved":
            return {
                backgroundColor: "#dcfce7",
                color: "#15803d",
            };
        case "Closed":
            return {
                backgroundColor: "#e5e7eb",
                color: "#374151",
            };
        default:
            return {
                backgroundColor: "#dbeafe",
                color: "#1d4ed8",
            };
    }
}

function getPriorityBadgeStyle(priority: string): CSSProperties {
    switch (priority) {
        case "Low":
            return {
                backgroundColor: "#ecfdf5",
                color: "#047857",
                border: "1px solid #a7f3d0",
            };
        case "Medium":
            return {
                backgroundColor: "#eff6ff",
                color: "#1d4ed8",
                border: "1px solid #bfdbfe",
            };
        case "High":
            return {
                backgroundColor: "#fff7ed",
                color: "#c2410c",
                border: "1px solid #fdba74",
            };
        case "Critical":
            return {
                backgroundColor: "#fef2f2",
                color: "#b91c1c",
                border: "1px solid #fca5a5",
            };
        default:
            return {
                backgroundColor: "#f8fafc",
                color: "#334155",
                border: "1px solid #e2e8f0",
            };
    }
}

function formatStatusLabel(status: string): string {
    switch (status) {
        case "InProgress":
            return "In Progress";
        case "WaitingCustomer":
            return "Waiting Customer";
        default:
            return status;
    }
}

function formatAssignedUserLabel(name?: string | null): string {
    if (!name) {
        return "Unassigned";
    }

    return name.endsWith(" User") ? name.replace(/ User$/, "") : name;
}

function getUserIdFromToken(): number | null {
    const token = localStorage.getItem("token");

    if (!token) {
        return null;
    }

    try {
        const payload = JSON.parse(atob(token.split(".")[1]));
        const rawUserId = 
            payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] ??
            payload.nameid ??
            payload.sub;

        if (!rawUserId) {
            return null;
        }

        const userId = Number(rawUserId);
        return Number.isNaN(userId) ? null : userId;
    }   catch {
        return null;
    }
}

export function TicketsPage() {
    const [result, setResult] = useState<PagedResult<Ticket> | null>(null);
    const [categories, setCategories] = useState<Category[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");
    const [createError, setCreateError] = useState("");
    const [creating, setCreating] = useState(false);
    const [limit] = useState(10);
    const [offset, setOffset] = useState(0);
    const [status, setStatus] = useState("");
    const [priority, setPriority] = useState("");
    const [title, setTitle] = useState("");
    const [description, setDescription] = useState("");
    const [categoryId, setCategoryId] = useState("");
    const [createPriority, setCreatePriority] = useState("2");

    async function loadTickets() {
        setError("");
        setLoading(true);

        try {
            const response = await api.get<PagedResult<Ticket>>("/tickets", {
                params: {
                    limit,
                    offset,
                    sortBy: "createdAt",
                    sortDirection: "desc",
                    status: status === "" ? undefined : Number(status),
                    priority: priority === "" ? undefined : Number(priority),
                },
            });

            setResult(response.data);
        }   catch {
            setError("Could not load tickets. Please try again later.");
        }   finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        void loadTickets();
    }, [limit, offset, status, priority]);

    useEffect(() => {
        async function loadCategories() {
            try {
                const response = await api.get<Category[]>("/categories");
                setCategories(response.data);
            }   catch {
                setCreateError("Could not load categories. Please refresh the page.");
            }
        }

        void loadCategories();
    }, []);

    function handleStatusChange(event: ChangeEvent<HTMLSelectElement>) {
        setStatus(event.target.value);
        setOffset(0);
    }

    function handlePriorityChange(event: ChangeEvent<HTMLSelectElement>) {
        setPriority(event.target.value);
        setOffset(0);
    }

    async function handleCreateTicket(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();
        setCreateError("");

        const createdByUserId = getUserIdFromToken();

        if (!createdByUserId) {
            setCreateError("You must be logged in to create a ticket.");
            return;
        }

        if (!title.trim() || !description.trim() || !categoryId) {
            setCreateError("Please fill in title, description and category.");
            return;
        }

        setCreating(true);

        try {
            const payload: CreateTicketRequest = {
                title: title.trim(),
                description: description.trim(),
                categoryId: Number(categoryId),
                createdByUserId,
                assignedToUserId: null,
                priority: Number(createPriority),
            };

            await api.post("/tickets", payload);

            setTitle("");
            setDescription("");
            setCategoryId("");
            setCreatePriority("2");

            await loadTickets();
        }   catch {
            setCreateError("Could not create ticket. Please try again later.");
        }   finally {
            setCreating(false);
        }
    }

    return (
        <div style={styles.page}>
            <div style={styles.wrapper}>
                <div style={styles.header}>
                    <div>
                        <h1 style={styles.title}>Tickets</h1>
                        <p style={styles.subtitle}>Support requests from the platform.</p>
                    </div>

                    <Link to="/dashboard" style={styles.linkButton}>
                        Back to Dashboard
                    </Link>
                </div>

                <div style={styles.createCard}>
                    <h2 style={styles.sectionTitle}>Create Ticket</h2>

                    <form onSubmit={handleCreateTicket} style={styles.form}>
                        <input
                            type="text"
                            placeholder="Title"
                            value={title}
                            onChange={(event) => setTitle(event.target.value)}
                            style={styles.input}
                        />

                        <textarea
                            placeholder="Describe the issue"
                            value={description}
                            onChange={(event) => setDescription(event.target.value)}
                            style={styles.textarea}
                        />

                        <div style={styles.formRow}>
                            <div style={styles.filterGroup}>
                                <label htmlFor="category-select" style={styles.filterLabel}>
                                    Category
                                </label>
                                <select
                                    id="category-select"
                                    value={categoryId}
                                    onChange={(event) => setCategoryId(event.target.value)}
                                    style={styles.select}
                                >
                                    <option value="">Select category</option>
                                    {categories.map((category) => (
                                        <option key={category.id} value={category.id}>
                                            {category.name}
                                        </option>
                                    ))}
                                </select>
                            </div>

                            <div style={styles.filterGroup}>
                                <label htmlFor="priority-select" style={styles.filterLabel}>
                                    Priority
                                </label>
                                <select
                                    id="priority-select"
                                    value={createPriority}
                                    onChange={(event) => setCreatePriority(event.target.value)}
                                    style={styles.select}
                                >
                                    {priorityOptions
                                      .filter((option) => option.value !== "")
                                      .map((option) => (
                                        <option key={option.label} value={option.value}>
                                            {option.label}
                                        </option>
                                      ))}
                                </select>
                            </div>
                        </div>

                        {createError ? <p style={styles.error}>{createError}</p> : null}

                        <button type="submit" style={styles.submitButton} disabled={creating}>
                            {creating ? "Creating..." : "Create Ticket"}
                        </button>
                    </form>
                </div>

                <div style={styles.filterCard}>
                    <div style={styles.filterGroup}>
                        <label htmlFor="status-filter" style={styles.filterLabel}>
                            Status
                        </label>
                        <select
                            id="status-filter"
                            value={status}
                            onChange={handleStatusChange}
                            style={styles.select}
                        >
                            {statusOptions.map((option) => (
                                <option key={option.label} value={option.value}>
                                    {option.label}
                                </option>
                            ))}
                        </select>
                    </div>

                    <div style={styles.filterGroup}>
                        <label htmlFor="priority-filter" style={styles.filterLabel}>
                            Priority
                        </label>
                        <select
                            id="priority-filter"
                            value={priority}
                            onChange={handlePriorityChange}
                            style={styles.select}
                        >
                            {priorityOptions.map((option) => (
                                <option key={option.label} value={option.value}>
                                    {option.label}
                                </option>
                            ))}
                        </select>
                    </div>
                </div>

                {loading ? <p style={styles.text}>Loading tickets...</p> : null}
                {error ? <p style={styles.error}>{error}</p> : null}

                {result ? (
                    <>
                        <div style={styles.paginationRow}>
                            <button
                                type="button"
                                style={{
                                    ...styles.paginationButton,
                                    opacity: offset === 0 ? 0.5 : 1,
                                    cursor: offset === 0 ? "not-allowed" : "pointer",
                                }}
                                onClick={() => setOffset((current) => Math.max(0, current - limit))}
                                disabled={offset === 0}
                            >
                                Previous
                            </button>

                            <button
                                type="button"
                                style={{
                                    ...styles.paginationButton,
                                    opacity: offset + limit >= result.totalCount ? 0.5 : 1,
                                    cursor: offset + limit >= result.totalCount ? "not-allowed" : "pointer",
                                }}
                                onClick={() => {
                                    if (offset + limit < result.totalCount) {
                                        setOffset((current) => current + limit);
                                    }
                                }}
                                disabled={offset + limit >= result.totalCount}
                            >
                                Next
                            </button>
                        </div>

                        <div style={styles.metaCard}>
                            <p style={styles.text}>Total: {result.totalCount}</p>
                            <p style={styles.text}>Limit: {result.limit ?? result.pageSize}</p>
                            <p style={styles.text}>Offset: {result.offset ?? 0}</p>
                        </div>

                        <div style={styles.list}>
                            {result.items.length === 0 ? (
                                <div style={styles.emptyCard}>
                                    <p style={styles.text}>No tickets found.</p>
                                </div>
                            ) : (
                                result.items.map((ticket) => (
                                    <Link
                                        key={ticket.id}
                                        to={`/tickets/${ticket.id}`}
                                        style={styles.ticketCardLink}
                                    >
                                        <div style={styles.ticketCard}>
                                            <div style={styles.ticketTop}>
                                                <h2 style={styles.ticketTitle}>{ticket.title}</h2>
                                                <span 
                                                    style={{
                                                        ...styles.badge,
                                                        ...getStatusBadgeStyle(ticket.status),
                                                    }}
                                                >
                                                    {formatStatusLabel(ticket.status)}
                                                </span>
                                            </div>

                                            <p style={styles.ticketDescription}>{ticket.description}</p>

                                            <div style={styles.infoRow}>
                                                <span 
                                                    style={{
                                                        ...styles.infoItem,
                                                        ...getPriorityBadgeStyle(ticket.priority),
                                                    }}
                                                >
                                                    Priority: {ticket.priority}
                                                </span>
                                                <span style={styles.infoItem}>
                                                    Category: {ticket.categoryName}
                                                </span>
                                                <span style={styles.infoItem}>
                                                    Assigned To: {formatAssignedUserLabel(ticket.assignedToUserName)}
                                                </span>
                                            </div>
                                        </div>
                                    </Link>
                                ))
                            )}
                        </div>
                    </>
                ) : null}
            </div>
        </div>
    );
}

const styles: Record<string, CSSProperties> = {
    page: {
        minHeight: "100vh",
        background:
            "linear-gradient(135deg, rgb(15, 23, 42) 0%, rgb(30, 41, 59) 100%)",
        padding: "40px 24px",
    },
    wrapper: {
        width: "100%",
        maxWidth: "1100px",
        margin: "0 auto",
        display: "flex",
        flexDirection: "column",
        gap: "24px",
    },
    header: {
        display: "flex",
        justifyContent: "space-between",
        alignItems: "center",
        gap: "16px",
        backgroundColor: "#ffffff",
        borderRadius: "20px",
        padding: "28px 32px",
        boxShadow: "0 20px 50px rgba(0, 0, 0, 0.25)",
    },
    title: {
        margin: 0,
        fontSize: "36px",
        color: "#0f172a",
    },
    subtitle: {
        marginTop: "10px",
        marginBottom: 0,
        color: "#475569",
        fontSize: "16px",
    },
    sectionTitle: {
        margin: 0,
        marginBottom: "16px",
        fontSize: "24px",
        color: "#0f172a",
    },
    linkButton: {
        textDecoration: "none",
        backgroundColor: "#0f172a",
        color: "#ffffff",
        padding: "12px 16px",
        borderRadius: "12px",
        fontSize: "14px",
    },
    createCard: {
        backgroundColor: "#ffffff",
        borderRadius: "20px",
        padding: "24px",
        boxShadow: "0 20px 50px rgba(0, 0, 0, 0.25)",
    },
    form: {
        display: "flex",
        flexDirection: "column",
        gap: "16px",
    },
    formRow: {
        display: "flex",
        flexWrap: "wrap",
        gap: "16px",
    },
    input: {
        border: "1px solid #cbd5e1",
        borderRadius: "12px",
        padding: "12px 14px",
        fontSize: "14px",
        color: "#0f172a",
        backgroundColor: "#ffffff",
    },
    textarea: {
        minHeight: "120px",
        border: "1px solid #cbd5e1",
        borderRadius: "12px",
        padding: "12px 14px",
        fontSize: "14px",
        color: "#0f172a",
        backgroundColor: "#ffffff",
        resize: "vertical",
    },
    submitButton: {
        border: "none",
        backgroundColor: "#0f172a",
        color: "#ffffff",
        padding: "12px 16px",
        borderRadius: "12px",
        fontSize: "14px",
        cursor: "pointer",
        alignSelf: "flex-start",
    },
    filterCard: {
        display: "flex",
        flexWrap: "wrap",
        gap: "16px",
        backgroundColor: "#ffffff",
        borderRadius: "20px",
        padding: "20px 24px",
        boxShadow: "0 20px 50px rgba(0, 0, 0, 0.25)",
    },
    filterGroup: {
        display: "flex",
        flexDirection: "column",
        gap: "8px",
        minWidth: "220px",
    },
    filterLabel: {
        fontSize: "14px",
        fontWeight: 600,
        color: "#334155",
    },
    select: {
        border: "1px solid #cbd5e1",
        borderRadius: "12px",
        padding: "12px 14px",
        fontSize: "14px",
        color: "#0f172a",
        backgroundColor: "#ffffff",
    },
    paginationRow: {
        display: "flex",
        gap: "12px",
    },
    paginationButton: {
        border: "none",
        backgroundColor: "#0f172a",
        color: "#ffffff",
        padding: "12px 16px",
        borderRadius: "12px",
        fontSize: "14px",
        cursor: "pointer",
    },
    metaCard: {
        display: "flex",
        justifyContent: "space-between",
        gap: "16px",
        backgroundColor: "#ffffff",
        borderRadius: "20px",
        padding: "20px 24px",
        boxShadow: "0 20px 50px rgba(0, 0, 0, 0.25)",
    },
    list: {
        display: "flex",
        flexDirection: "column",
        gap: "16px",
    },
    ticketCard: {
        backgroundColor: "#ffffff",
        borderRadius: "20px",
        padding: "24px",
        boxShadow: "0 20px 50px rgba(0, 0, 0, 0.2)",
    },
    ticketCardLink: {
        textDecoration: "none",
        display: "block",
    },
    ticketTop: {
        display: "flex",
        justifyContent: "space-between",
        alignItems: "flex-start",
        gap: "16px",
        marginBottom: "12px",
    },
    ticketTitle: {
        margin: 0,
        fontSize: "22px",
        color: "#0f172a",
    },
    badge: {
        borderRadius: "999px",
        padding: "6px 12px",
        fontSize: "12px",
        whiteSpace: "nowrap",
    },
    ticketDescription: {
        margin: 0,
        marginBottom: "16px",
        color: "#475569",
        lineHeight: 1.5,
    },
    infoRow: {
        display: "flex",
        flexWrap: "wrap",
        gap: "12px",
    },
    infoItem: {
        backgroundColor: "#f8fafc",
        border: "1px solid #e2e8f0",
        borderRadius: "12px",
        padding: "8px 12px",
        fontSize: "14px",
        color: "#334155",
    },
    emptyCard: {
        backgroundColor: "#ffffff",
        borderRadius: "20px",
        padding: "24px",
    },
    text: {
        margin: 0,
        color: "#475569",
        fontSize: "16px",
    },
    error: {
        margin: 0,
        color: "#fecaca",
        fontSize: "14px",
    },
};