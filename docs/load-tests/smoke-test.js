import http from "k6/http";
import { check, sleep } from "k6";

export const options = {
    vus: 5,
    duration: "20s",
};

const baseUrl = "http://localhost:5132";

export default function () {
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
        "login returned token": (res) => {
            const body = res.json();
            return body.token && body.token.length > 0;
        },
    });

    const token = loginResponse.json().token;

    const authHeaders = {
        headers: {
            Authorization: `Bearer ${token}`,
        },
    };

    const ticketsResponse = http.get(`${baseUrl}/api/Tickets`, authHeaders);

    check(ticketsResponse, {
        "tickets status is 200": (res) => res.status === 200,
    });

    const summaryResponse = http.get(
        `${baseUrl}/api/Dashboard/summary`,
        authHeaders
    );

    check(summaryResponse, {
        "summary status is 200": (res) => res.status === 200,
    });

    sleep(1);
}