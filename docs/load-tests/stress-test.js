import http from "k6/http";
import { check } from "k6";

export const options = {
    thresholds: {
        http_req_failed: ["rate<0.02"],
        http_req_duration: ["p(95)<1300"],
    },
    scenarios: {
        dashboard_stress: {
            executor: "constant-vus",
            vus: 25,
            duration: "30s",
            exec: "dashboardScenario",
        },
        tickets_stress: {
            executor: "constant-vus",
            vus: 25,
            duration: "30s",
            exec: "ticketsScenario",
        },
    },
};

const baseUrl = "http://localhost:5132";

function loginAndGetToken() {
    const loginPayload = JSON.stringify({
        email: "admin@customersupport.com",
        password: "admin123",
    });

    const loginResponse = http.post(`${baseUrl}/api/Auth/login`, loginPayload, {
        headers: {
            "Content-Type": "application/json",
        },
    });

    check(loginResponse, {
        "login status is 200": (res) => res.status === 200,
    });

    return loginResponse.json("token");
}

export function dashboardScenario() {
    const token = loginAndGetToken();

    const response = http.get(`${baseUrl}/api/Dashboard/summary`, {
        headers: {
            Authorization: `Bearer ${token}`,
        },
    });

    check(response, {
        "dashboard status is 200": (res) => res.status === 200,
    });
}

export function ticketsScenario() {
    const token = loginAndGetToken();

    const response = http.get(`${baseUrl}/api/Tickets`, {
        headers: {
            Authorization: `Bearer ${token}`,
        },
    });

    check(response, {
        "tickets status is 200": (res) => res.status === 200,
    });
}