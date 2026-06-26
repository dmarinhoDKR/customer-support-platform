import { render, screen } from "@testing-library/react";
import { vi } from "vitest";
import { AppRouter } from "./AppRouter";

vi.mock("../pages/LoginPage", () => ({
  LoginPage: () => <div>Login Page</div>,
}));

vi.mock("../pages/DashboardPage", () => ({
  DashboardPage: () => <div>Dashboard Page</div>,
}));

vi.mock("../pages/TicketsPage", () => ({
  TicketsPage: () => <div>Tickets Page</div>,
}));

vi.mock("../pages/TicketDetailsPage", () => ({
  TicketDetailsPage: () => <div>Ticket Details Page</div>,
}));

describe("AppRouter", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
    window.history.pushState({}, "", "/");
  });

  it("renderiza a rota de login em /", () => {
    window.history.pushState({}, "", "/");
    render(<AppRouter />);

    expect(screen.getByText("Login Page")).toBeInTheDocument();
  });

  it("redireciona dashboard para login quando não existe token", () => {
    window.history.pushState({}, "", "/dashboard");
    render(<AppRouter />);

    expect(screen.getByText("Login Page")).toBeInTheDocument();
  });

  it("renderiza dashboard quando existe token", () => {
    localStorage.setItem("token", "fake-token");
    window.history.pushState({}, "", "/dashboard");

    render(<AppRouter />);

    expect(screen.getByText("Dashboard Page")).toBeInTheDocument();
  });

  it("renderiza tickets quando existe token", () => {
    localStorage.setItem("token", "fake-token");
    window.history.pushState({}, "", "/tickets");

    render(<AppRouter />);

    expect(screen.getByText("Tickets Page")).toBeInTheDocument();
  });

  it("renderiza detalhes do ticket quando existe token", () => {
    localStorage.setItem("token", "fake-token");
    window.history.pushState({}, "", "/tickets/1");

    render(<AppRouter />);

    expect(screen.getByText("Ticket Details Page")).toBeInTheDocument();
  });
});