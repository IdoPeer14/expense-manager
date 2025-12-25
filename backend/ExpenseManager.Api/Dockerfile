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
ENV TESSDATA_PREFIX=/usr/share/tesseract-ocr/4.00/tessdata

# Debug: Check where tessdata actually is
RUN echo "=== Checking Tesseract installation ===" && \
    tesseract --version && \
    find /usr -name "*.traineddata" 2>/dev/null | head -10 && \
    echo "=== End Tesseract check ==="

COPY --from=build /app/out .

# Render listens on port 8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ExpenseManager.Api.dll"]
