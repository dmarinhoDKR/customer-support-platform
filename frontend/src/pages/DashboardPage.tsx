import { Link, useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";
import type { CSSProperties } from "react";
import { api } from "../services/api";
import type { DashboardSummary } from "../types/dashboard";

export function DashboardPage() {
  const navigate = useNavigate();
  const fullName = localStorage.getItem("fullName") ?? "User";
  const role = localStorage.getItem("role") ?? "Unknown";

  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    async function loadSummary() {
      try {
        const response = await api.get<DashboardSummary>("/dashboard/summary");
        setSummary(response.data);
      } catch {
        setError("Could not load dashboard summary.");
      } finally {
        setLoading(false);
      }
    }

    void loadSummary();
  }, []);

  function handleLogout() {
    localStorage.removeItem("token");
    localStorage.removeItem("fullName");
    localStorage.removeItem("email");
    localStorage.removeItem("role");
    navigate("/");
  }

  return (
    <div style={styles.page}>
      <div style={styles.wrapper}>
        <div style={styles.card}>
          <h1 style={styles.title}>Dashboard</h1>
          <p style={styles.subtitle}>Welcome back, {fullName}.</p>
          <p style={styles.text}>Your role is: {role}</p>
          <button
            type="button"
            onClick={handleLogout}
            style={styles.logoutButton}
          >
            Logout
          </button>
          <Link to="/tickets" style={styles.linkButton}>
            View Tickets
          </Link>
        </div>

        <div style={styles.card}>
          <h2 style={styles.sectionTitle}>Ticket Summary</h2>

          {loading ? <p style={styles.text}>Loading summary...</p> : null}
          {error ? <p style={styles.error}>{error}</p> : null}

          {summary ? (
            <div style={styles.grid}>
              <div style={styles.metricBox}>
                <span style={styles.metricLabel}> Total Tickets</span>
                <strong style={styles.metricValue}>
                  {summary.totalTickets}
                </strong>
              </div>

              <div style={styles.metricBox}>
                <span style={styles.metricLabel}>Open</span>
                <strong style={styles.metricValue}>
                  {summary.openTickets}
                </strong>
              </div>

              <div style={styles.metricBox}>
                <span style={styles.metricLabel}>In Progress</span>
                <strong style={styles.metricValue}>
                  {summary.inProgressTickets}
                </strong>
              </div>

              <div style={styles.metricBox}>
                <span style={styles.metricLabel}>Resolved</span>
                <strong style={styles.metricValue}>
                  {summary.resolvedTickets}
                </strong>
              </div>

              <div style={styles.metricBox}>
                <span style={styles.metricLabel}>Waiting Customer</span>
                <strong style={styles.metricValue}>
                  {summary.waitingCustomerTickets}
                </strong>
              </div>

              <div style={styles.metricBox}>
                <span style={styles.metricLabel}>Unassigned</span>
                <strong style={styles.metricValue}>
                  {summary.unassignedTickets}
                </strong>
              </div>

              <div style={styles.metricBox}>
                <span style={styles.metricLabel}>Critical Priority</span>
                <strong style={styles.metricValue}>
                  {summary.criticalPriorityTickets}
                </strong>
              </div>
            </div>
          ) : null}
        </div>
      </div>
    </div>
  );
}

const styles: Record<string, CSSProperties> = {
  page: {
    minHeight: "100vh",
    background: "linear-gradient(135deg, rgb(15, 23, 42) 0%, rgb(30, 41, 59) 100%)",
    padding: "40px 24px",
  },
  card: {
    backgroundColor: "#ffffff",
    borderRadius: "20px",
    padding: "28px 32px",
    boxShadow: "0 20px 50px rgba(0, 0, 0, 0.25)",
  },
  title: {
    margin: 0,
    fontSize: "32px",
    color: "#0f172a",
  },
  subtitle: {
    marginTop: "12px",
    marginBottom: "8px",
    color: "#334155",
    fontSize: "18px",
  },
  text: {
    margin: 0,
    color: "#475569",
    fontSize: "16px",
  },
  logoutButton: {
    marginTop: "20px",
    padding: "12px 16px",
    borderRadius: "12px",
    border: "none",
    backgroundColor: "#0f172a",
    color: "#ffffff",
    fontSize: "16px",
    cursor: "pointer",
    alignSelf: "flex-start",
  },
  error: {
    margin: 0,
    color: "#dc2626",
    fontSize: "14px",
  },
  grid: {
    display: "grid",
    gridTemplateColumns: "repeat(auto-fit, minmax(180px, 1fr))",
    gap: "16px",
  },
  metricBox: {
    backgroundColor: "#f8fafc",
    border: "1px solid #e2e8f0",
    borderRadius: "16px",
    padding: "20px",
    display: "flex",
    flexDirection: "column",
    gap: "8px",
  },
  metricLabel: {
    color: "#475569",
    fontSize: "14px",
  },
  metricValue: {
    color: "#0f172a",
    fontSize: "28px",
  },
  wrapper: {
    width: "100%",
    maxWidth: "1100px",
    margin: "0 auto",
    display: "flex",
    flexDirection: "column",
    gap: "24px",
  },
  sectionTitle: {
    margin: 0,
    marginBottom: "20px",
    fontSize: "24px",
    color: "#0f172a",
  },
  linkButton: {
    display: "inline-block",
    marginTop: "12px",
    textDecoration: "none",
    backgroundColor: "#e2e8f0",
    color: "#0f172a",
    padding: "12px 16px",
    borderRadius: "12px",
    fontSize: "14px",
  },
};
