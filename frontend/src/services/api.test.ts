import { AxiosHeaders } from "axios";
import type { InternalAxiosRequestConfig } from "axios";
import { api } from "./api";

describe("api interceptor", () => {
  beforeEach(() => {
    localStorage.clear();
  });

  function getRequestInterceptor() {
    const handlers = (
      api.interceptors.request as typeof api.interceptors.request & {
        handlers?: Array<{
          fulfilled?: (
            config: InternalAxiosRequestConfig
          ) => InternalAxiosRequestConfig | Promise<InternalAxiosRequestConfig>;
        }>;
      }
    ).handlers;

    const interceptor = handlers?.[0]?.fulfilled;

    expect(interceptor).toBeDefined();

    return interceptor!;
  }

  it("adiciona o token no header Authorization quando existir token", async () => {
    localStorage.setItem("token", "fake-token");

    const interceptor = getRequestInterceptor();

    const config = await interceptor({
      headers: new AxiosHeaders(),
    } as InternalAxiosRequestConfig);

    expect(config.headers.Authorization).toBe("Bearer fake-token");
  });

  it("nao adiciona Authorization quando nao existir token", async () => {
    const interceptor = getRequestInterceptor();

    const config = await interceptor({
      headers: new AxiosHeaders(),
    } as InternalAxiosRequestConfig);

    expect(config.headers.Authorization).toBeUndefined();
  });

  it("mantem o baseURL configurado corretamente", () => {
    expect(api.defaults.baseURL).toBe("http://127.0.0.1:5132/api");
  });
});