import { MemoryRouter } from "react-router-dom";
import { render, screen, waitFor } from "@testing-library/react"
import { vi } from "vitest";
import { DashboardPage } from "./DashboardPage";
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
        get: vi.fn(),
    },
}));

describe("DashboardPage", () => {
    beforeEach(() => {
        vi.clearAllMocks();
        localStorage.clear();
        localStorage.setItem("token", "fake-token");
        localStorage.setItem("fullName", "Admin User");
        localStorage.setItem("role", "Admin");
    });

    it("carrega e exibe o resumo do dashboard", async () => {
        vi.mocked(api.get).mockResolvedValue({
            data: {
                totalTickets: 5,
                openTickets: 2,
                inProgressTickets: 1,
                waitingForCustomerTickets: 0,
                resolvedTickets: 1,
                closedTickets: 1,
                lowPriorityTickets: 0,
                mediumPriorityTickets: 1,
                highPriorityTickets: 2,
                criticalPriorityTickets: 1,
            },
        });

        render(
            <MemoryRouter>
                <DashboardPage />
            </MemoryRouter>
        );

        expect(screen.getByText("Dashboard")).toBeInTheDocument();
        expect(screen.getByText(/welcome back, admin user/i)).toBeInTheDocument();
        expect(screen.getByText(/your role is: admin/i)).toBeInTheDocument();

        await waitFor(() => {
            expect(api.get).toHaveBeenCalledWith("/dashboard/summary");
        });

        expect(screen.getByText("Ticket Summary")).toBeInTheDocument();
        expect(screen.getByText("Total Tickets")).toBeInTheDocument();
        expect(screen.getByText("Open")).toBeInTheDocument();
        expect(screen.getByText("In Progress")).toBeInTheDocument();
        expect(screen.getByText("Resolved")).toBeInTheDocument();
        expect(screen.getByText("Critical Priority")).toBeInTheDocument();

        expect(screen.getByText("5")).toBeInTheDocument();
        expect(screen.getByText("2")).toBeInTheDocument();
        expect(screen.getAllByText("1").length).toBeGreaterThan(0);
    });

    it("mostra mensagem de erro se o resumo falhar", async () => {
        vi.mocked(api.get).mockRejectedValue(new Error("API error"));

        render(
            <MemoryRouter>
                <DashboardPage />
            </MemoryRouter>
        );

        expect(
            await screen.findByText("Could not load dashboard summary.")
        ).toBeInTheDocument();
    });
});