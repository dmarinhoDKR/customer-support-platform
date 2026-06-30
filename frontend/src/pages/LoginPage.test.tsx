import { MemoryRouter } from "react-router-dom";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { vi } from "vitest";
import { LoginPage } from "./LoginPage";
import { api } from "../services/api";

const mockNavigate = vi.fn();

vi.mock("react-router-dom", async () => {
    const actual = await vi.importActual<typeof import("react-router-dom")>(
        "react-router-dom"
    );

    return {
        ...actual,
        useNavigate: () => mockNavigate,
    };
});

vi.mock("../services/api", () => ({
    api: {
        post: vi.fn(),
    },
}));

describe("LoginPage", () => {
    beforeEach(() => {
        vi.clearAllMocks();
        localStorage.clear();
    });

    it("faz login com sucesso e salva os dados no localStorage", async () => {
        vi.mocked(api.post).mockResolvedValue({
            data: {
                token: "fake-token",
                expiresAt: "2026-06-20T10:00:00Z",
                fullName: "Admin User",
                email: "admin@customersupport.com",
                role: "Admin",
            },
        });

        render(
            <MemoryRouter>
                <LoginPage />
            </MemoryRouter>
        );

        await userEvent.clear(screen.getByPlaceholderText("Email"));
        await userEvent.type(
            screen.getByPlaceholderText("Email"),
            "admin@customersupport.com"
        );

        await userEvent.clear(screen.getByPlaceholderText("Password"));
        await userEvent.type(screen.getByPlaceholderText("Password"), "admin123");

        await userEvent.click(screen.getByRole("button", { name: /sign in/i }));

        await waitFor(() => {
            expect(api.post).toHaveBeenCalledWith("/auth/login", {
                email: "admin@customersupport.com",
                password: "admin123"
            });
        });

        expect(localStorage.getItem("token")).toBe("fake-token");
        expect(localStorage.getItem("fullName")).toBe("Admin User");
        expect(localStorage.getItem("email")).toBe("admin@customersupport.com");
        expect(localStorage.getItem("role")).toBe("Admin");
        expect(mockNavigate).toHaveBeenCalledWith("/dashboard");
    });

    it("mostra erro quando o login falha", async () => {
        vi.mocked(api.post).mockRejectedValue(new Error("Invalid credentials"));

        render(
            <MemoryRouter>
                <LoginPage/>
            </MemoryRouter>
        );

        await userEvent.click(screen.getByRole("button", { name: /sign in/i}));

        expect(
            await screen.findByText("Invalid email or password.")
        ).toBeInTheDocument();

        expect(localStorage.getItem("token")).toBeNull();
        expect(mockNavigate).not.toHaveBeenCalled();
    });
});