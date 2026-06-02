import { BrowserRouter, Route, Routes } from "react-router-dom";
import { DashboardPage } from "../pages/DashboardPage";
import { LoginPage } from "../pages/LoginPage";
import { TicketDetailsPage } from "../pages/TicketDetailsPage";
import { TicketsPage } from "../pages/TicketsPage";
import { ProtectedRoute } from "./ProtectedRoute";


export function AppRouter() {
    return (
        <BrowserRouter>
           <Routes>
            <Route path= "/" element={<LoginPage />} />
            <Route
                path="/dashboard"
                element={
                    <ProtectedRoute>
                        <DashboardPage />
                    </ProtectedRoute>
                }
            />
            <Route
                path="/tickets"
                element={
                    <ProtectedRoute>
                        <TicketsPage />
                    </ProtectedRoute>
                }
            />
            <Route
                path="/tickets/:id"
                element={
                    <ProtectedRoute>
                        <TicketDetailsPage />
                    </ProtectedRoute>
                }
            />
           </Routes>
        </BrowserRouter>
    );
}