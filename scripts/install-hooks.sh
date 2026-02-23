#!/bin/bash
# Install git pre-commit hook for dotnet format
HOOK_DIR="$(git rev-parse --git-dir)/hooks"
HOOK_FILE="$HOOK_DIR/pre-commit"

cat > "$HOOK_FILE" << 'HOOK'
#!/bin/bash
echo "Running dotnet format check..."
dotnet format --verify-no-changes --verbosity quiet
if [ $? -ne 0 ]; then
    echo ""
    echo "ERROR: Code formatting violations found."
    echo "Run 'dotnet format' to fix them, then stage the changes."
    exit 1
fi
HOOK

chmod +x "$HOOK_FILE"
echo "Pre-commit hook installed successfully."
