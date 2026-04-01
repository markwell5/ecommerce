#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
RESULTS_DIR="${SCRIPT_DIR}/results"

mkdir -p "${RESULTS_DIR}"

TIMESTAMP=$(date +%Y%m%d_%H%M%S)
SCRIPTS=("product-browsing" "place-order" "cancel-order")

# Allow running a single test: ./run.sh product-browsing
if [ $# -gt 0 ]; then
  SCRIPTS=("$1")
fi

echo "=== k6 Load Tests ==="
echo "Timestamp: ${TIMESTAMP}"
echo "Results:   ${RESULTS_DIR}"
echo ""

for script in "${SCRIPTS[@]}"; do
  echo "─── Running: ${script} ───"

  docker compose -f "${SCRIPT_DIR}/docker-compose.load-test.yml" run --rm \
    k6 run \
    --summary-export="/results/${script}_${TIMESTAMP}.json" \
    --out "json=/results/${script}_${TIMESTAMP}_raw.json" \
    "/scripts/${script}.js"

  echo ""
  echo "Results saved to: ${RESULTS_DIR}/${script}_${TIMESTAMP}.json"
  echo ""
done

echo "=== All tests complete ==="
echo ""
echo "Summary files:"
for script in "${SCRIPTS[@]}"; do
  echo "  ${RESULTS_DIR}/${script}_${TIMESTAMP}.json"
done
