#!/bin/bash
# Install git pre-commit hook for dotnet format
HOOK_DIR="$(git rev-parse --git-dir)/hooks"
HOOK_FILE="$HOOK_DIR/pre-commit"

cat > "$HOOK_FILE" << 'HOOK'
#!/bin/bash
# Get staged .cs files only
STAGED_CS=$(git diff --cached --name-only --diff-filter=ACM -- '*.cs')
if [ -z "$STAGED_CS" ]; then
    exit 0
fi

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
HOOK

chmod +x "$HOOK_FILE"
echo "Pre-commit hook installed successfully."
