import { useEffect, useState } from "react";
import type { CSSProperties } from "react";
import { Link } from "react-router-dom";
import { api } from "../services/api";
import type { PagedResult, Ticket } from "../types/ticket";

export function TicketsPage() {
    const [result, setResult] = useState<PagedResult<Ticket> | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState("");

    useEffect(() => {
        async function loadTickets() {
            try {
                const response = await api.get<PagedResult<Ticket>>("/tickets", {
                    params: {
                        pageNumber: 1,
                        pageSize: 10,
                        sortBy: "createdAt",
                        sortDirection: "desc",
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
    }, []);

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

                {loading ? <p style={styles.text}>Loading tickets...</p> : null}
                {error ? <p style={styles.error}>{error}</p> : null}

                {result ? (
                    <>
                        <div style={styles.metaCard}>
                            <p style={styles.text}>Total: {result.totalCount}</p>
                            <p style={styles.text}>
                                Page: {result.totalPages === 0 ? 0 : result.pageNumber} / {result.totalPages}
                            </p>
                        </div>

                        <div style={styles.list}>
                            {result.items.map((ticket) => (
                                <div key={ticket.id} style={styles.ticketCard}>
                                    <div style={styles.ticketTop}>
                                        <h2 style={styles.ticketTitle}>{ticket.title}</h2>
                                        <span style={styles.badge}>{ticket.status}</span>
                                    </div>

                                    <p style={styles.ticketDescription}>{ticket.description}</p>

                                    <div style={styles.infoRow}>
                                        <span style={styles.infoItem}>Priority: {ticket.priority}</span>
                                        <span style={styles.infoItem}>Category: {ticket.categoryName}</span>
                                        <span style={styles.infoItem}>
                                            Assigned To: {ticket.assignedToUserId ?? "Unassigned"}
                                        </span>
                                    </div>
                                </div>
                            ))}

                            {result.items.length === 0 ? (
                                <div style={styles.emptyCard}>
                                    <p style={styles.text}>No tickets found.</p>
                                </div>
                            ) : null}
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