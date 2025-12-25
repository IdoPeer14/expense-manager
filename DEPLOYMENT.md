# Deployment Guide - Expense Manager

## Overview
This application is a full-stack expense manager with:
- **Frontend**: React + Vite SPA
- **Backend**: ASP.NET Core 8.0 Web API
- **Database**: PostgreSQL
- **Deployment**: Render.com

## Architecture
The backend serves both:
1. REST API endpoints (`/api/*`)
2. Static frontend files (SPA with client-side routing)

## Local Development

### Build Everything
```bash
./build-and-deploy.sh
```

This script will:
1. Build the frontend (React/Vite)
2. Copy dist files to `backend/ExpenseManager.Api/wwwroot/`
3. Build the backend (.NET)

### Run Locally
```bash
cd backend/ExpenseManager.Api
dotnet run
```

The app will be available at `http://localhost:5000`

## Deployment to Render

### Prerequisites
- Frontend and backend must be built together
- Frontend files must be in `backend/ExpenseManager.Api/wwwroot/`
- Git repository pushed to GitHub

### Deploy Steps

1. **Build the application**:
   ```bash
   ./build-and-deploy.sh
   ```

2. **Commit changes**:
   ```bash
   git add .
   git commit -m "Deploy: Updated frontend and backend"
   git push
   ```

3. **Render will automatically**:
   - Detect the push
   - Run the build command (configured in Render dashboard)
   - Deploy the new version

### Render Configuration

**Build Command**:
```bash
cd backend/ExpenseManager.Api && dotnet publish -c Release -o out
```

**Start Command**:
```bash
cd backend/ExpenseManager.Api/out && ./ExpenseManager.Api
```

**Environment Variables**:
- `DATABASE_URL`: PostgreSQL connection (auto-configured by Render)
- `JWT_SECRET`: Your JWT secret key
- `ASPNETCORE_ENVIRONMENT`: Production

### Important Notes

1. **SPA Routing**: The backend is configured to serve `index.html` for all non-API routes, enabling direct URL access and page refresh.

2. **Static Files**: Frontend assets are served from `wwwroot/` using ASP.NET Core's static file middleware.

3. **CORS**: Configured to allow requests from:
   - `http://localhost:5173` (local dev)
   - `https://expense-manager-uad5.onrender.com` (production)

4. **Routes**:
   - API routes: `/api/*`
   - Health check: `/health`
   - All other routes: Serve React SPA

## Troubleshooting

### "Not Found" on Direct URL Access
- Ensure `MapFallbackToFile("index.html")` is configured in `Program.cs`
- Verify frontend files are in `wwwroot/`
- Check that `UseStaticFiles()` middleware is added

### Frontend Not Loading
- Build frontend: `cd frontend && npm run build`
- Copy to wwwroot: `cp -r frontend/dist/* backend/ExpenseManager.Api/wwwroot/`
- Verify files exist in wwwroot

### API Not Accessible
- Check CORS configuration in `Program.cs`
- Verify API routes are prefixed with `/api/`
- Check Render logs for errors

## File Structure
```
expense-manager/
├── frontend/
│   ├── dist/              # Build output (generated)
│   └── src/               # React source
├── backend/
│   └── ExpenseManager.Api/
│       ├── wwwroot/       # Frontend static files (copied from dist)
│       ├── Program.cs     # ASP.NET Core configuration
│       └── Controllers/   # API endpoints
└── build-and-deploy.sh    # Build script
```

## Updating the Application

1. Make changes to frontend or backend
2. Run `./build-and-deploy.sh`
3. Test locally
4. Commit and push to deploy

## Quick Reference

**Build frontend only**:
```bash
cd frontend && npm run build
```

**Copy to wwwroot**:
```bash
cp -r frontend/dist/* backend/ExpenseManager.Api/wwwroot/
```

**Test backend locally**:
```bash
cd backend/ExpenseManager.Api && dotnet run
```

**Full deployment**:
```bash
./build-and-deploy.sh
git add .
git commit -m "Deploy update"
git push
```
