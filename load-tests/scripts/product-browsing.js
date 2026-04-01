import http from "k6/http";
import { check, sleep } from "k6";
import { Rate, Trend } from "k6/metrics";

const BASE_URL = __ENV.BASE_URL || "http://localhost:5001";

const errorRate = new Rate("errors");
const listLatency = new Trend("product_list_duration", true);
const detailLatency = new Trend("product_detail_duration", true);
const searchLatency = new Trend("product_search_duration", true);

export const options = {
  stages: [
    { duration: "30s", target: 10 },  // ramp up
    { duration: "1m", target: 10 },   // steady state
    { duration: "30s", target: 50 },  // ramp to peak
    { duration: "1m", target: 50 },   // sustained peak
    { duration: "30s", target: 0 },   // ramp down
  ],
  thresholds: {
    http_req_duration: ["p(95)<500", "p(99)<1000"],
    errors: ["rate<0.05"],
    product_list_duration: ["p(95)<400"],
    product_detail_duration: ["p(95)<200"],
    product_search_duration: ["p(95)<600"],
  },
};

export default function () {
  // Scenario 1: List products (paginated)
  const listRes = http.get(`${BASE_URL}/api/v1.0/products?page=1&pageSize=20`);
  listLatency.add(listRes.timings.duration);
  check(listRes, {
    "list: status 200": (r) => r.status === 200,
    "list: has items": (r) => JSON.parse(r.body).items.length > 0,
  }) || errorRate.add(1);

  sleep(0.5);

  // Scenario 2: Get product by ID
  const products = JSON.parse(listRes.body).items;
  if (products.length > 0) {
    const productId = products[Math.floor(Math.random() * products.length)].id;
    const detailRes = http.get(`${BASE_URL}/api/v1.0/products/${productId}`);
    detailLatency.add(detailRes.timings.duration);
    check(detailRes, {
      "detail: status 200": (r) => r.status === 200,
      "detail: has name": (r) => JSON.parse(r.body).name !== "",
    }) || errorRate.add(1);
  }

  sleep(0.5);

  // Scenario 3: Search products
  const searchTerms = ["phone", "laptop", "shirt", "camera", "headphones"];
  const term = searchTerms[Math.floor(Math.random() * searchTerms.length)];
  const searchRes = http.get(
    `${BASE_URL}/api/v1.0/products/search?q=${term}&page=1&pageSize=10`
  );
  searchLatency.add(searchRes.timings.duration);
  check(searchRes, {
    "search: status 200": (r) => r.status === 200,
  }) || errorRate.add(1);

  sleep(1);
}
