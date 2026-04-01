# Load Tests

Performance load tests using [k6](https://k6.io/) for the ecommerce platform.

## Prerequisites

- Docker and Docker Compose
- Application services running via `docker compose --profile app up`
- A test user registered (default: `loadtest@example.com` / `LoadTest123!`)

## Test Scenarios

| Script | Description | Auth Required |
|--------|-------------|---------------|
| `product-browsing` | List, detail, and search products | No |
| `place-order` | Login → fetch products → place order → verify | Yes |
| `cancel-order` | Login → place order → cancel → verify status | Yes |

## Running

### All tests

```bash
cd load-tests
./run.sh
```

### Single test

```bash
./run.sh product-browsing
```

### Custom test user credentials

```bash
TEST_EMAIL=myuser@example.com TEST_PASSWORD=MyPass123! ./run.sh
```

### Without Docker (k6 installed locally)

```bash
k6 run scripts/product-browsing.js
k6 run --out json=results/output.json scripts/place-order.js
```

## Results

Results are saved to `load-tests/results/` in two formats per run:

- `<script>_<timestamp>.json` — summary export (p50/p95/p99, throughput, error rates)
- `<script>_<timestamp>_raw.json` — full metric stream (for detailed analysis)

The `results/` directory is gitignored.

## Thresholds

Each script defines pass/fail thresholds:

### product-browsing
| Metric | Threshold |
|--------|-----------|
| `http_req_duration` p95 | < 500ms |
| `http_req_duration` p99 | < 1000ms |
| `product_list_duration` p95 | < 400ms |
| `product_detail_duration` p95 | < 200ms |
| `product_search_duration` p95 | < 600ms |
| Error rate | < 5% |

### place-order
| Metric | Threshold |
|--------|-----------|
| `http_req_duration` p95 | < 1000ms |
| `http_req_duration` p99 | < 2000ms |
| `login_duration` p95 | < 500ms |
| `place_order_duration` p95 | < 1500ms |
| `get_order_duration` p95 | < 500ms |
| Error rate | < 10% |

### cancel-order
| Metric | Threshold |
|--------|-----------|
| `http_req_duration` p95 | < 1000ms |
| `http_req_duration` p99 | < 2000ms |
| `place_order_duration` p95 | < 1500ms |
| `cancel_order_duration` p95 | < 1000ms |
| `verify_cancelled_duration` p95 | < 500ms |
| Error rate | < 10% |

## Load Profiles

All scripts use a staged ramp-up profile:

1. **Ramp up** (30s) — gradually increase virtual users
2. **Steady state** (1m) — hold at base load
3. **Ramp to peak** (30s) — increase to peak load
4. **Sustained peak** (1m) — hold peak
5. **Ramp down** (30s) — cool down

Peak VUs: 50 (product-browsing), 20 (place-order), 15 (cancel-order).
