import { MemoryRouter, Route, Routes } from "react-router-dom";
import { render, screen } from "@testing-library/react";
import { ProtectedRoute } from "./ProtectedRoute";

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
});