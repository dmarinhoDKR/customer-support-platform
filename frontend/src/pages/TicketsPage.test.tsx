import { MemoryRouter } from "react-router-dom";
import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { vi } from "vitest";
import { TicketsPage } from "./TicketsPage";
import { api } from "../services/api";

vi.mock("../services/api", () => ({
  api: {
    get: vi.fn(),
    post: vi.fn(),
  },
}));

const categoriesResponse = [
  { id: 1, name: "Technical Support" },
  { id: 2, name: "Billing" },
];

function buildTicketsResponse({
  items = [],
  totalCount = 0,
  offset = 0,
  limit = 10,
}: {
  items?: Array<{
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
    updatedAt: string;
  }>;
  totalCount?: number;
  offset?: number;
  limit?: number;
}) {
  return {
    data: {
      items,
      pageNumber: Math.floor(offset / limit) + 1,
      pageSize: limit,
      totalCount,
      totalPages: totalCount === 0 ? 0 : Math.ceil(totalCount / limit),
      limit,
      offset,
    },
  };
}

function buildTicket(overrides?: Partial<{
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
  updatedAt: string;
}>) {
  return {
    id: 1,
    title: "Critical production issue",
    description: "Main application flow is unavailable for multiple users.",
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
    ...overrides,
  };
}

function mockCategories() {
  return Promise.resolve({ data: categoriesResponse });
}

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
          data: buildTicketsResponse({
            items: [buildTicket()],
            totalCount: 1,
          }).data,
        });
      }

      if (url === "/categories") {
        return mockCategories();
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
        "Main application flow is unavailable for multiple users."
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
        return mockCategories();
      }

      return Promise.reject(new Error("Unknown endpoint"));
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

  it("cria ticket com sucesso", async () => {
    const user = userEvent.setup();

    const fakePayload = btoa(
      JSON.stringify({
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier":
          "1",
      })
    );

    localStorage.setItem("token", `header.${fakePayload}.signature`);

    vi.mocked(api.get).mockImplementation((url) => {
      if (url === "/tickets") {
        return Promise.resolve(buildTicketsResponse({ items: [] }));
      }

      if (url === "/categories") {
        return mockCategories();
      }

      return Promise.reject(new Error("Unknown endpoint"));
    });

    vi.mocked(api.post).mockResolvedValue({ data: {} });

    render(
      <MemoryRouter>
        <TicketsPage />
      </MemoryRouter>
    );

    await user.type(screen.getByPlaceholderText("Title"), "Test ticket from frontend");
    await user.type(
      screen.getByPlaceholderText("Describe the issue"),
      "This ticket was created during frontend testing."
    );
    await user.selectOptions(screen.getByLabelText("Category"), "1");
    await user.click(
      screen.getByRole("button", { name: "Create Ticket" })
    );

    await waitFor(() => {
      expect(api.post).toHaveBeenCalledWith("/tickets", {
        title: "Test ticket from frontend",
        description: "This ticket was created during frontend testing.",
        categoryId: 1,
        createdByUserId: 1,
        assignedToUserId: null,
        priority: 2,
      });
    });

    await waitFor(() => {
      expect(
        vi.mocked(api.get).mock.calls.filter(([url]) => url === "/tickets").length
      ).toBeGreaterThan(1);
    });
  });

  it("aplica filtros de status e prioridade", async () => {
    const user = userEvent.setup();

    vi.mocked(api.get).mockImplementation((url, config) => {
      if (url === "/tickets") {
        return Promise.resolve(
          buildTicketsResponse({
            items: [],
            offset: config?.params?.offset ?? 0,
          })
        );
      }

      if (url === "/categories") {
        return mockCategories();
      }

      return Promise.reject(new Error("Unknown endpoint"));
    });

    render(
      <MemoryRouter>
        <TicketsPage />
      </MemoryRouter>
    );

    const statusFilter = screen.getByLabelText("Status");
    const filterCard = statusFilter.closest("div")?.parentElement;

    expect(filterCard).not.toBeNull();

    const priorityFilter = within(filterCard as HTMLElement).getByLabelText(
      "Priority"
    );

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

    await user.selectOptions(statusFilter, "2");
    await user.selectOptions(priorityFilter, "4");

    await waitFor(() => {
      expect(api.get).toHaveBeenCalledWith("/tickets", {
        params: {
          limit: 10,
          offset: 0,
          sortBy: "createdAt",
          sortDirection: "desc",
          status: 2,
          priority: 4,
        },
      });
    });
  });

  it("avança para a próxima página ao clicar em Next", async () => {
    const user = userEvent.setup();

    vi.mocked(api.get).mockImplementation((url, config) => {
      if (url === "/tickets") {
        const currentOffset = config?.params?.offset ?? 0;

        return Promise.resolve(
          buildTicketsResponse({
            items: [
              buildTicket({
                id: currentOffset === 0 ? 1 : 2,
                title:
                  currentOffset === 0
                    ? "First page ticket"
                    : "Second page ticket",
                description: "Ticket loaded during pagination test.",
                priority: "Medium",
                assignedToUserId: null,
                assignedToUserName: null,
              }),
            ],
            totalCount: 20,
            offset: currentOffset,
          })
        );
      }

      if (url === "/categories") {
        return mockCategories();
      }

      return Promise.reject(new Error("Unknown endpoint"));
    });

    render(
      <MemoryRouter>
        <TicketsPage />
      </MemoryRouter>
    );

    await waitFor(() => {
      expect(screen.getByText("First page ticket")).toBeInTheDocument();
    });

    await user.click(screen.getByRole("button", { name: "Next" }));

    await waitFor(() => {
      expect(api.get).toHaveBeenCalledWith("/tickets", {
        params: {
          limit: 10,
          offset: 10,
          sortBy: "createdAt",
          sortDirection: "desc",
          status: undefined,
          priority: undefined,
        },
      });
    });

    await waitFor(() => {
      expect(screen.getByText("Second page ticket")).toBeInTheDocument();
    });

    expect(screen.getByText("Offset: 10")).toBeInTheDocument();
  });
});