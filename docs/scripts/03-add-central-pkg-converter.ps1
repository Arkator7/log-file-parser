# ----------------
# Enable Central Package Management
# ----------------
Write-Host "Enabling Central Package Management..."

dotnet new tool-manifest
dotnet tool install CentralisedPackageConverter

dotnet central-pkg-converter .

Write-Host "✅ Central Package Management enabled successfully."
