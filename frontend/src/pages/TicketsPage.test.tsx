import { MemoryRouter } from "react-router-dom";
import { render, screen, waitFor } from "@testing-library/react";
import { vi } from "vitest";
import { TicketsPage } from "./TicketsPage";
import { api } from "../services/api";

vi.mock("../services/api", () => ({
    api: {
        get: vi.fn(),
        post: vi.fn(),
    },
}));

describe("TicketsPage", () => {
    beforeEach(() => {
        vi.clearAllMocks();
        localStorage.clear();
        localStorage.setItem("token", "fake-token");
    });

    it("carrega tickets e categorias com sucesso", async () => {
        vi.mocked(api.get).mockImplementation((url) => {
            if (url === "/tickets") {
                return Promise.resolve({
                    data: {
                        items: [
                            {
                                id: 1,
                                title: "Critical production issue",
                                description:
                                  "Main application flow is unavaliable for multiple users.",
                                status: "Open",
                                priority: "Critical",
                                categoryId: 1,
                                categoryName: "Technical Support",
                                createdByUserId: 1,
                                createdByUserName: "Admin User",
                                assignedToUserId: 2,
                                assignedToUserName: "Agent User",
                                createdAt: "2026-06-20T10:00:00Z",
                                updatedAt: "2026-06-20T11:00:00Z",
                            },
                        ],
                        pageNumber: 1,
                        pageSize: 10,
                        totalCount: 1,
                        totalPages: 1,
                        limit: 10,
                        offset: 0,
                    },
                });
            }

            if (url === "/categories") {
                return Promise.resolve({
                    data: [
                        { id: 1, name: "Technical Support" },
                        { id: 2, name: "Billing" },
                    ],
                });
            }

            return Promise.reject(new Error("Unknown endpoint"));
        });

        render(
            <MemoryRouter>
                <TicketsPage />
            </MemoryRouter>
        );

        expect(screen.getByText("Tickets")).toBeInTheDocument();
        expect(
            screen.getByRole("heading", { name: "Create Ticket" })
        ).toBeInTheDocument();
        expect(
            screen.getByRole("button", { name: "Create Ticket" })
        ).toBeInTheDocument();

        await waitFor(() => {
            expect(api.get).toHaveBeenCalledWith("/tickets", {
                params: {
                    limit: 10,
                    offset: 0,
                    sortBy: "createdAt",
                    sortDirection: "desc",
                    status: undefined,
                    priority: undefined,
                },
            });
        });

        await waitFor(() => {
            expect(api.get).toHaveBeenCalledWith("/categories");
        });

        expect(screen.getByText("Critical production issue")).toBeInTheDocument();
        expect(
            screen.getByText(
                "Main application flow is unavaliable for multiple users."
            )
        ).toBeInTheDocument();

        expect(screen.getByText("Priority: Critical")).toBeInTheDocument();
        expect(screen.getByText("Category: Technical Support")).toBeInTheDocument();
        expect(screen.getByText("Assigned To: Agent")).toBeInTheDocument();

        expect(screen.getByText("Total: 1")).toBeInTheDocument();
        expect(screen.getByText("Limit: 10")).toBeInTheDocument();
        expect(screen.getByText("Offset: 0")).toBeInTheDocument();
    });

    it("mostra erro quando falha ao carregar tickets", async () => {
        vi.mocked(api.get).mockImplementation((url) => {
            if (url === "/tickets") {
                return Promise.reject(new Error("API error"));
            }

            if (url === "/categories") {
                return Promise.resolve({
                    data: [{ id: 1, name: "Technical Support"}],
                });
            }

            return Promise.reject(new Error("Unkbown endpoint"));
        });

        render(
            <MemoryRouter>
                <TicketsPage />
            </MemoryRouter>
        );

        expect(
            await screen.findByText("Could not load tickets. Please try again later.")
        ).toBeInTheDocument();
    });
});