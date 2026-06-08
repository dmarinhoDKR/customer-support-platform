import { useEffect, useState } from "react";
import type { CSSProperties } from "react";
import { Link, useParams } from "react-router-dom";
import { api } from "../services/api";
import type {
  Ticket,
  TicketComment,
  TicketStatusHistoryItem,
} from "../types/ticket";

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
        backgroundColor: "#d1fae5",
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

function formatDateTime(value?: string | null, emptyLabel = "Not updated yet"): string {
  if (!value) {
    return emptyLabel;
  }

  return new Date(value).toLocaleString();
}

export function TicketDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const [ticket, setTicket] = useState<Ticket | null>(null);
  const [comments, setComments] = useState<TicketComment[]>([]);
  const [history, setHistory] = useState<TicketStatusHistoryItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    async function loadTicketDetails() {
      if (!id) {
        setError("Ticket id was not provided.");
        setLoading(false);
        return;
      }

      setError("");
      setLoading(true);

      try {
        const [ticketResponse, commentsResponse, historyResponse] =
          await Promise.all([
            api.get<Ticket>(`/tickets/${id}`),
            api.get<TicketComment[]>(`/tickets/${id}/comments`),
            api.get<TicketStatusHistoryItem[]>(`/tickets/${id}/status-history`),
          ]);

        setTicket(ticketResponse.data);
        setComments(commentsResponse.data);
        setHistory(historyResponse.data);
      } catch {
        setError("Could not load ticket details.");
      } finally {
        setLoading(false);
      }
    }

    void loadTicketDetails();
  }, [id]);

  return (
    <div style={styles.page}>
      <div style={styles.wrapper}>
        <div style={styles.header}>
          <div>
            <h1 style={styles.title}>Ticket Details</h1>
            <p style={styles.subtitle}>
              Detailed view of the selected support request
            </p>
          </div>

          <div style={styles.headerActions}>
            <Link to="/tickets" style={styles.secondaryLinkButton}>
              Back to Tickets
            </Link>
            <Link to="/dashboard" style={styles.linkButton}>
              Dashboard
            </Link>
          </div>
        </div>

        {loading ? <p style={styles.text}>Loading ticket details...</p> : null}
        {error ? <p style={styles.error}>{error}</p> : null}

        {!loading && !error && ticket ? (
          <>
            <div style={styles.mainCard}>
              <div style={styles.ticketTop}>
                <div>
                  <h2 style={styles.ticketTitle}>{ticket.title}</h2>
                  <p style={styles.ticketDescription}>{ticket.description}</p>
                </div>

                <span
                  style={{
                    ...styles.badge,
                    ...getStatusBadgeStyle(ticket.status),
                  }}
                >
                  {formatStatusLabel(ticket.status)}
                </span>
              </div>

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
                  Assigned To:{" "}
                  {formatAssignedUserLabel(ticket.assignedToUserName)}
                </span>
                <span style={styles.infoItem}>
                  Created By: {formatAssignedUserLabel(ticket.createdByUserName)}
                </span>
                <span style={styles.infoItem}>
                  Created At: {formatDateTime(ticket.createdAt, "Unknown")}
                </span>
                <span style={styles.infoItem}>
                  Updated At: {formatDateTime(ticket.updatedAt)}
                </span>
              </div>
            </div>

            <div style={styles.sectionCard}>
              <h3 style={styles.sectionTitle}>Comments</h3>

              {comments.length === 0 ? (
                <p style={styles.text}>No comments yet.</p>
              ) : (
                <div style={styles.sectionList}>
                  {comments.map((comment) => (
                    <div key={comment.id} style={styles.listItem}>
                      <div style={styles.listItemHeader}>
                        <strong style={styles.listItemTitle}>
                          {formatAssignedUserLabel(comment.userName)}
                        </strong>
                        <span style={styles.metaText}>
                          {formatDateTime(comment.createdAt)}
                        </span>
                      </div>
                      <p style={styles.listItemText}>{comment.content}</p>
                    </div>
                  ))}
                </div>
              )}
            </div>

            <div style={styles.sectionCard}>
              <h3 style={styles.sectionTitle}>Status History</h3>

              {history.length === 0 ? (
                <p style={styles.text}>No status changes recorded yet</p>
              ) : (
                <div style={styles.sectionList}>
                  {history.map((item) => (
                    <div key={item.id} style={styles.listItem}>
                      <div style={styles.listItemHeader}>
                        <strong style={styles.listItemTitle}>
                          {formatStatusLabel(item.oldStatus)} to{" "}
                          {formatStatusLabel(item.newStatus)}
                        </strong>
                        <span style={styles.metaText}>
                          {formatDateTime(item.changedAt)}
                        </span>
                      </div>
                      <p style={styles.listItemText}>
                        Changed by:{" "}
                        {formatAssignedUserLabel(item.changedByUserName)}
                      </p>
                    </div>
                  ))}
                </div>
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
  headerActions: {
    display: "flex",
    gap: "12px",
    flexWrap: "wrap",
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
  secondaryLinkButton: {
    textDecoration: "none",
    backgroundColor: "#e2e8f0",
    color: "#0f172a",
    padding: "12px 16px",
    borderRadius: "12px",
    fontSize: "14px",
  },
  mainCard: {
    backgroundColor: "#ffffff",
    borderRadius: "20px",
    padding: "24px",
    boxShadow: "0 20px 50px rgba(0, 0, 0, 0.25)",
  },
  sectionCard: {
    backgroundColor: "#ffffff",
    borderRadius: "20px",
    padding: "24px",
    boxShadow: "0 20px 50px rgba(0, 0, 0, 0.25)",
  },
  sectionTitle: {
    margin: 0,
    marginBottom: "16px",
    fontSize: "22px",
    color: "#0f172a",
  },
  ticketTop: {
    display: "flex",
    justifyContent: "space-between",
    alignItems: "flex-start",
    gap: "16px",
    marginBottom: "16px",
  },
  ticketTitle: {
    margin: 0,
    fontSize: "28px",
    color: "#0f172a",
  },
  badge: {
    borderRadius: "999px",
    padding: "8px 14px",
    fontSize: "12px",
    whiteSpace: "nowrap",
  },
  ticketDescription: {
    marginTop: "14px",
    marginBottom: 0,
    color: "#475569",
    lineHeight: 1.6,
    fontSize: "16px",
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
  sectionList: {
    display: "flex",
    flexDirection: "column",
    gap: "16px",
  },
  listItem: {
    border: "1px solid #e2e8f0",
    borderRadius: "16px",
    padding: "16px",
    backgroundColor: "#f8fafc",
  },
  listItemHeader: {
    display: "flex",
    justifyContent: "space-between",
    gap: "12px",
    flexWrap: "wrap",
    marginBottom: "10px",
  },
  listItemTitle: {
    color: "#0f172a",
  },
  listItemText: {
    margin: 0,
    color: "#475569",
    lineHeight: 1.5,
  },
  metaText: {
    color: "#64748b",
    fontSize: "13px",
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
