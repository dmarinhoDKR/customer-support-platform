import React, { useState } from "react";
import type { CSSProperties } from "react";
import { useNavigate } from "react-router-dom";
import { api } from "../services/api";
import type { AuthResponse } from "../types/auth";

export function LoginPage() {
    const navigate = useNavigate();
    const [email, setEmail] = useState("agent@customersupport.com");
    const [password, setPassword] = useState("admin123");
    const [error, setError] = useState("");
    const [loading, setLoading] = useState(false);

    async function handleSubmit(
        event: React.SyntheticEvent<HTMLFormElement>
    ) {
        event.preventDefault();
        setError("");
        setLoading(true);

        try {
            const response = await api.post<AuthResponse>("/auth/login", {
                email,
                password,
            });

            localStorage.setItem("token", response.data.token);
            localStorage.setItem("fullName", response.data.fullName);
            localStorage.setItem("email", response.data.email);
            localStorage.setItem("role", response.data.role);
            
            navigate("/dashboard");
        }   catch {
            setError("Invalid email or password.");
        }   finally {
            setLoading(false);
        }
    }

    return (
        <div style={styles.page}>
            <div style={styles.card}>
                <h1 style={styles.title}>Customer Support</h1>
                <p style={styles.subtitle}>Sign in to access the platform.</p>

                <form onSubmit={handleSubmit} style={styles.form}>
                    <input
                        type="email"
                        placeholder="Email"
                        value={email}
                        onChange={(event) => setEmail(event.target.value)}
                        style={styles.input}
                    />

                    <input
                        type="password"
                        placeholder="Password"
                        value={password}
                        onChange={(event) => setPassword(event.target.value)}
                        style={styles.input}
                    />

                    {error ? <p style={styles.error}>{error}</p> : null}

                    <button type="submit" style={styles.button} disabled={loading}>
                        {loading ? "Signing in..." : "Sign in"}
                    </button>
                </form>
            </div>
        </div>
    );
}

const styles: Record<string, CSSProperties> = {
    page: {
        minHeight: "100vh",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        background:
            "linear-gradient(135deg, rgb(14, 23, 42) 0%, rgb(30, 41, 59) 100%)",
        padding: "24px",
    },
    card: {
        width: "100%",
        maxWidth: "420px",
        backgroundColor: "#ffffff",
        borderRadius: "20px",
        padding: "32px",
        boxShadow: "0 20px 50px rgba(0, 0, 0, 0.25)",
    },
    title: {
        margin: 0,
        fontSize: "32px",
        color: "#0f172a",
    },
    subtitle: {
        marginTop: "8px",
        marginBottom: "24px",
        color: "#475569",
    },
    form: {
        display: "flex",
        flexDirection: "column",
        gap: "16px",
    },
    input: {
        padding: "14px 16px",
        borderRadius: "12px",
        border: "1px solid #cbd5e1",
        fontSize: "16px",
    },
    error: {
        margin: 0,
        color: "#dc2626",
        fontSize: "14px",
    },
    button: {
        padding: "14px 16px",
        borderRadius: "12px",
        border: "none",
        backgroundColor: "#0f172a",
        color: "#ffffff",
        fontSize: "16px",
        cursor: "pointer",
    },
};