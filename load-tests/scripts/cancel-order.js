import http from "k6/http";
import { check, sleep, fail } from "k6";
import { Rate, Trend } from "k6/metrics";
import { uuidv4 } from "https://jslib.k6.io/k6-utils/1.4.0/index.js";

const BASE_URL = __ENV.BASE_URL || "http://localhost:5002";
const USER_SERVICE_URL = __ENV.USER_SERVICE_URL || "http://localhost:5004";
const PRODUCT_SERVICE_URL = __ENV.PRODUCT_SERVICE_URL || "http://localhost:5001";
const TEST_EMAIL = __ENV.TEST_EMAIL || "loadtest@example.com";
const TEST_PASSWORD = __ENV.TEST_PASSWORD || "LoadTest123!";

const errorRate = new Rate("errors");
const placeOrderLatency = new Trend("place_order_duration", true);
const cancelOrderLatency = new Trend("cancel_order_duration", true);
const verifyLatency = new Trend("verify_cancelled_duration", true);

export const options = {
  stages: [
    { duration: "30s", target: 5 },
    { duration: "1m", target: 5 },
    { duration: "30s", target: 15 },
    { duration: "1m", target: 15 },
    { duration: "30s", target: 0 },
  ],
  thresholds: {
    http_req_duration: ["p(95)<1000", "p(99)<2000"],
    errors: ["rate<0.10"],
    place_order_duration: ["p(95)<1500"],
    cancel_order_duration: ["p(95)<1000"],
    verify_cancelled_duration: ["p(95)<500"],
  },
};

function login() {
  const res = http.post(
    `${USER_SERVICE_URL}/api/v1.0/auth/login`,
    JSON.stringify({ email: TEST_EMAIL, password: TEST_PASSWORD }),
    { headers: { "Content-Type": "application/json" } }
  );

  if (
    !check(res, { "login: status 200": (r) => r.status === 200 })
  ) {
    fail(`Login failed with status ${res.status}: ${res.body}`);
  }

  return JSON.parse(res.body).token;
}

function getProducts(token) {
  const res = http.get(`${PRODUCT_SERVICE_URL}/api/v1.0/products?page=1&pageSize=5`, {
    headers: { Authorization: `Bearer ${token}` },
  });

  if (res.status !== 200) return [];
  return JSON.parse(res.body).items;
}

function authHeaders(token) {
  return {
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
      "Idempotency-Key": uuidv4(),
    },
  };
}

export default function () {
  // Step 1: Login
  const token = login();
  sleep(0.5);

  // Step 2: Get products for order
  const products = getProducts(token);
  if (products.length === 0) {
    console.warn("No products available, skipping");
    errorRate.add(1);
    return;
  }

  // Step 3: Place an order
  const product = products[Math.floor(Math.random() * products.length)];
  const orderPayload = JSON.stringify({
    customerId: "loadtest-user",
    items: [
      {
        productId: product.id,
        productName: product.name,
        quantity: 1,
        unitPrice: product.price,
      },
    ],
  });

  const placeRes = http.post(
    `${BASE_URL}/api/v1.0/orders`,
    orderPayload,
    authHeaders(token)
  );
  placeOrderLatency.add(placeRes.timings.duration);

  if (
    !check(placeRes, {
      "place order: status 202": (r) => r.status === 202,
    })
  ) {
    errorRate.add(1);
    return;
  }

  const orderId = JSON.parse(placeRes.body).orderId;
  sleep(1);

  // Step 4: Cancel the order
  const cancelRes = http.post(
    `${BASE_URL}/api/v1.0/orders/${orderId}/cancel`,
    null,
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    }
  );
  cancelOrderLatency.add(cancelRes.timings.duration);
  check(cancelRes, {
    "cancel: status 202 or 409": (r) =>
      r.status === 202 || r.status === 409,
  }) || errorRate.add(1);

  sleep(0.5);

  // Step 5: Verify order status
  const verifyRes = http.get(`${BASE_URL}/api/v1.0/orders/${orderId}`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  verifyLatency.add(verifyRes.timings.duration);
  check(verifyRes, {
    "verify: status 200": (r) => r.status === 200,
  }) || errorRate.add(1);

  sleep(1);
}
