#!/bin/bash
# Install git pre-commit hook for dotnet format + CMS lint/typecheck
HOOK_DIR="$(git rev-parse --git-dir)/hooks"
HOOK_FILE="$HOOK_DIR/pre-commit"

cat > "$HOOK_FILE" << 'HOOK'
#!/bin/bash

# ── .NET format check ─────────────────────────────
STAGED_CS=$(git diff --cached --name-only --diff-filter=ACM -- '*.cs')
if [ -n "$STAGED_CS" ]; then
    echo "Running dotnet format check on staged files..."
    INCLUDE_ARGS=""
    for f in $STAGED_CS; do
        INCLUDE_ARGS="$INCLUDE_ARGS --include $f"
    done

    dotnet format --verify-no-changes --verbosity quiet $INCLUDE_ARGS
    if [ $? -ne 0 ]; then
        echo ""
        echo "ERROR: Code formatting violations found."
        echo "Run 'dotnet format' to fix them, then stage the changes."
        exit 1
    fi
fi

# ── CMS lint + typecheck ──────────────────────────
STAGED_CMS=$(git diff --cached --name-only --diff-filter=ACM -- 'cms/src/**')
if [ -n "$STAGED_CMS" ]; then
    echo "Running CMS typecheck..."
    (cd cms && npx tsc --noEmit)
    if [ $? -ne 0 ]; then
        echo ""
        echo "ERROR: TypeScript errors found in CMS."
        exit 1
    fi

    echo "Running CMS lint..."
    (cd cms && npx eslint src --max-warnings 0 2>/dev/null)
    if [ $? -ne 0 ]; then
        echo ""
        echo "ERROR: Lint errors found in CMS."
        exit 1
    fi
fi
HOOK

chmod +x "$HOOK_FILE"
echo "Pre-commit hook installed successfully."
