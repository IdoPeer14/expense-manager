#!/bin/bash
set -e

echo "========================================="
echo "Building Expense Manager for Deployment"
echo "========================================="

# Navigate to project root
cd "$(dirname "$0")"

echo ""
echo "Step 1: Building Frontend..."
echo "-----------------------------"
cd frontend
npm install
npm run build

echo ""
echo "Step 2: Copying Frontend to Backend wwwroot..."
echo "-----------------------------------------------"
cd ..
rm -rf backend/ExpenseManager.Api/wwwroot/*
cp -r frontend/dist/* backend/ExpenseManager.Api/wwwroot/

echo ""
echo "Step 3: Building Backend..."
echo "---------------------------"
cd backend/ExpenseManager.Api
dotnet build --configuration Release

echo ""
echo "========================================="
echo "Build Complete!"
echo "========================================="
echo "Frontend files copied to: backend/ExpenseManager.Api/wwwroot/"
echo "Backend built in: Release configuration"
echo ""
echo "To run locally:"
echo "  cd backend/ExpenseManager.Api"
echo "  dotnet run"
echo ""
echo "To deploy to Render:"
echo "  git add ."
echo "  git commit -m 'Update deployment'"
echo "  git push"
echo "========================================="
