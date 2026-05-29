import { useEffect, useState } from "react";
import type { ChangeEvent, CSSProperties } from "react";
import { Link } from "react-router-dom";
import { api } from "../services/api";
import type { PagedResult, Ticket } from "../types/ticket";

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

export function TicketsPage() {
    const [result, setResult] = useState<PagedResult<Ticket> | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");
    const [limit] = useState(10);
    const [offset, setOffset] = useState(0);
    const [status, setStatus] = useState("");
    const [priority, setPriority] = useState("");

    useEffect(() => {
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
            } catch {
                setError("Could not load tickets.");
            } finally {
                setLoading(false);
            }
        }

        void loadTickets();
    }, [limit, offset, status, priority]);

    function handleStatusChange(event: ChangeEvent<HTMLSelectElement>) {
        setStatus(event.target.value);
        setOffset(0);
    }

    function handlePriorityChange(event: ChangeEvent<HTMLSelectElement>) {
        setPriority(event.target.value);
        setOffset(0);
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
                                    <div key={ticket.id} style={styles.ticketCard}>
                                        <div style={styles.ticketTop}>
                                            <h2 style={styles.ticketTitle}>{ticket.title}</h2>
                                            <span style={styles.badge}>{ticket.status}</span>
                                        </div>

                                        <p style={styles.ticketDescription}>{ticket.description}</p>

                                        <div style={styles.infoRow}>
                                            <span style={styles.infoItem}>
                                                Priority: {ticket.priority}
                                            </span>
                                            <span style={styles.infoItem}>
                                                Category: {ticket.categoryName}
                                            </span>
                                            <span style={styles.infoItem}>
                                                Assigned To: {ticket.assignedToUserId ?? "Unassigned"}
                                            </span>
                                        </div>
                                    </div>
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
    linkButton: {
        textDecoration: "none",
        backgroundColor: "#0f172a",
        color: "#ffffff",
        padding: "12px 16px",
        borderRadius: "12px",
        fontSize: "14px",
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
        backgroundColor: "#dbeafe",
        color: "#1d4ed8",
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