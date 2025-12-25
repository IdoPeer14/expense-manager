# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the source code
COPY . ./
RUN dotnet publish -c Release -o out

# =========================
# Runtime stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Install Tesseract OCR with Hebrew and English language packs
# Also install poppler-utils for PDF-to-image conversion
RUN apt-get update && apt-get install -y \
    tesseract-ocr \
    tesseract-ocr-eng \
    tesseract-ocr-heb \
    libtesseract-dev \
    poppler-utils \
    && rm -rf /var/lib/apt/lists/*

# Set TESSDATA_PREFIX environment variable for Tesseract
# Tesseract 5.x on Debian 12 (Bookworm) - confirmed by build logs
ENV TESSDATA_PREFIX=/usr/share/tesseract-ocr/5/tessdata

# Debug: Check Tesseract installation and native libraries
RUN echo "=== Checking Tesseract installation ===" && \
    tesseract --version && \
    echo "--- Tessdata files ---" && \
    find /usr -name "*.traineddata" 2>/dev/null | head -10 && \
    echo "--- Native libraries ---" && \
    find /usr -name "libtesseract*" 2>/dev/null && \
    find /usr -name "liblept*" 2>/dev/null && \
    echo "--- Library dependencies ---" && \
    ldd /usr/bin/tesseract | grep -E "(tesseract|lept)" && \
    echo "=== End Tesseract check ==="

COPY --from=build /app/out .

# Render listens on port 8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ExpenseManager.Api.dll"]
