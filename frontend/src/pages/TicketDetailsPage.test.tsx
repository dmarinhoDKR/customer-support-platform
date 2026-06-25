import { MemoryRouter, Route, Routes } from "react-router-dom";
import { render, screen, waitFor } from "@testing-library/react";
import { vi } from "vitest";
import { TicketDetailsPage } from "./TicketDetailsPage";
import { api } from "../services/api";

vi.mock("../services/api", () => ({
  api: {
    get: vi.fn(),
  },
}));

function buildTicket(
  overrides?: Partial<{
    id: number;
    title: string;
    description: string;
    status: string;
    priority: string;
    categoryId: number;
    categoryName: string;
    createdByUserId: number;
    createdByUserName: string;
    assignedToUserId: number | null;
    assignedToUserName: string | null;
    createdAt: string;
    updatedAt: string | null;
  }>
) {
  return {
    id: 1,
    title: "Critical production issue",
    description: "Main application flow is unavailable for multiple users.",
    status: "InProgress",
    priority: "Critical",
    categoryId: 1,
    categoryName: "Technical Support",
    createdByUserId: 1,
    createdByUserName: "Customer User",
    assignedToUserId: 2,
    assignedToUserName: "Agent User",
    createdAt: "2026-06-20T10:00:00Z",
    updatedAt: "2026-06-20T11:00:00Z",
    ...overrides,
  };
}

function buildComment(
  overrides?: Partial<{
    id: number;
    ticketId: number;
    userId: number;
    userName: string;
    content: string;
    createdAt: string;
  }>
) {
  return {
    id: 1,
    ticketId: 1,
    userId: 2,
    userName: "Agent User",
    content: "We are investigating the incident and collecting more details.",
    createdAt: "2026-06-20T10:30:00Z",
    ...overrides,
  };
}

function buildHistoryItem(
  overrides?: Partial<{
    id: number;
    ticketId: number;
    oldStatus: string;
    newStatus: string;
    changedByUserId: number;
    changedByUserName: string;
    changedAt: string;
  }>
) {
  return {
    id: 1,
    ticketId: 1,
    oldStatus: "Open",
    newStatus: "InProgress",
    changedByUserId: 2,
    changedByUserName: "Agent User",
    changedAt: "2026-06-20T10:45:00Z",
    ...overrides,
  };
}

function renderTicketDetailsPage(route = "/tickets/1") {
  render(
    <MemoryRouter initialEntries={[route]}>
      <Routes>
        <Route path="/tickets/:id" element={<TicketDetailsPage />} />
        <Route path="/tickets" element={<TicketDetailsPage />} />
      </Routes>
    </MemoryRouter>
  );
}

describe("TicketDetailsPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("carrega detalhes do ticket, comentários e histórico com sucesso", async () => {
    vi.mocked(api.get).mockImplementation((url) => {
      if (url === "/tickets/1") {
        return Promise.resolve({
          data: buildTicket(),
        });
      }

      if (url === "/tickets/1/comments") {
        return Promise.resolve({
          data: [buildComment()],
        });
      }

      if (url === "/tickets/1/status-history") {
        return Promise.resolve({
          data: [buildHistoryItem()],
        });
      }

      return Promise.reject(new Error("Unknown endpoint"));
    });

    renderTicketDetailsPage();

    expect(screen.getByText("Ticket Details")).toBeInTheDocument();

    await waitFor(() => {
      expect(api.get).toHaveBeenCalledWith("/tickets/1");
      expect(api.get).toHaveBeenCalledWith("/tickets/1/comments");
      expect(api.get).toHaveBeenCalledWith("/tickets/1/status-history");
    });

    expect(screen.getByText("Critical production issue")).toBeInTheDocument();
    expect(
      screen.getByText(
        "Main application flow is unavailable for multiple users."
      )
    ).toBeInTheDocument();

    expect(screen.getByText("In Progress")).toBeInTheDocument();
    expect(screen.getByText("Priority: Critical")).toBeInTheDocument();
    expect(screen.getByText("Category: Technical Support")).toBeInTheDocument();
    expect(screen.getByText("Assigned To: Agent")).toBeInTheDocument();
    expect(screen.getByText("Created By: Customer")).toBeInTheDocument();

    expect(screen.getByText("Comments")).toBeInTheDocument();
    expect(screen.getByText("Agent")).toBeInTheDocument();
    expect(
      screen.getByText(
        "We are investigating the incident and collecting more details."
      )
    ).toBeInTheDocument();

    expect(screen.getByText("Status History")).toBeInTheDocument();
    expect(screen.getByText("Open to In Progress")).toBeInTheDocument();
    expect(screen.getByText("Changed by: Agent")).toBeInTheDocument();
  });

  it("mostra erro quando falha ao carregar os detalhes", async () => {
    vi.mocked(api.get).mockRejectedValue(new Error("API error"));

    renderTicketDetailsPage();

    expect(
      await screen.findByText("Could not load ticket details.")
    ).toBeInTheDocument();
  });

  it("mostra estados vazios para comentários e histórico", async () => {
    vi.mocked(api.get).mockImplementation((url) => {
      if (url === "/tickets/1") {
        return Promise.resolve({
          data: buildTicket({
            title: "Invoice not generated",
            description:
              "Customer completed payment but did not receive invoice.",
            status: "Open",
            priority: "Medium",
            assignedToUserId: null,
            assignedToUserName: null,
            updatedAt: null,
          }),
        });
      }

      if (url === "/tickets/1/comments") {
        return Promise.resolve({
          data: [],
        });
      }

      if (url === "/tickets/1/status-history") {
        return Promise.resolve({
          data: [],
        });
      }

      return Promise.reject(new Error("Unknown endpoint"));
    });

    renderTicketDetailsPage();

    await waitFor(() => {
      expect(screen.getByText("Invoice not generated")).toBeInTheDocument();
    });

    expect(screen.getByText("Assigned To: Unassigned")).toBeInTheDocument();
    expect(screen.getByText("Updated At: Not updated yet")).toBeInTheDocument();
    expect(screen.getByText("No comments yet.")).toBeInTheDocument();
    expect(
      screen.getByText("No status changes recorded yet")
    ).toBeInTheDocument();
  });

  it("mostra erro quando o id do ticket não é informado", async () => {
    renderTicketDetailsPage("/tickets");

    expect(
      await screen.findByText("Ticket id was not provided.")
    ).toBeInTheDocument();

    expect(api.get).not.toHaveBeenCalled();
  });
});