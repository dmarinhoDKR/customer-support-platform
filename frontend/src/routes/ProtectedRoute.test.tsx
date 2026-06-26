import { MemoryRouter, Route, Routes, useLocation } from "react-router-dom";
import { render, screen } from "@testing-library/react";
import { ProtectedRoute } from "./ProtectedRoute";

function LocationDisplay() {
    const location = useLocation();

    return <div>Current Path: {location.pathname}</div>;
}

describe("ProtectedRoute", () => {
    beforeEach(() => {
        localStorage.clear();
    });

    it("renderiza o conteúdo quando existe token", () => {
        localStorage.setItem("token", "fake-token");

        render(
            <MemoryRouter initialEntries={["/dashboard"]}>
                <Routes>
                    <Route
                      path="/dashboard"
                      element={
                        <ProtectedRoute>
                            <div>Protected Content</div>
                        </ProtectedRoute>
                      }
                    />
                </Routes>
            </MemoryRouter>
        );

        expect(screen.getByText("Protected Content")).toBeInTheDocument();
    });

    it("redireciona para login quando não existe token", () => {
        render(
            <MemoryRouter initialEntries={["/dashboard"]}>
                <Routes>
                    <Route path="/" element={<div>Login Page</div>} />
                    <Route
                      path="/dashboard"
                      element={
                        <ProtectedRoute>
                            <div>Protected Content</div>
                        </ProtectedRoute>
                      }
                    />
                </Routes>
            </MemoryRouter>
        );

        expect(screen.getByText("Login Page")).toBeInTheDocument();
        expect(screen.queryByText("Protected Content")).not.toBeInTheDocument();
    });

    it("redireciona com replace para login quando nao existir token", () => {
        render(
            <MemoryRouter initialEntries={["/dashboard"]}>
                <Routes>
                    <Route path="/" element={<><div>Login Page</div><LocationDisplay /></>} />
                    <Route
                      path="/dashboard"
                      element={
                        <ProtectedRoute>
                            <div>Protected Content</div>
                        </ProtectedRoute>
                      }
                    />
                </Routes>
            </MemoryRouter>
        );

        expect(screen.getByText("Login Page")).toBeInTheDocument();
        expect(screen.getByText("Current Path: /")).toBeInTheDocument();
        expect(screen.queryByText("Protected Content")).not.toBeInTheDocument();
    });
});