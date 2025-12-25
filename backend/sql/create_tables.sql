-- =====================================================
-- Expense Manager Database Schema
-- =====================================================
-- Drop tables if they exist (cascade to handle foreign keys)
DROP TABLE IF EXISTS "Expenses" CASCADE;
DROP TABLE IF EXISTS "Documents" CASCADE;
DROP TABLE IF EXISTS "Users" CASCADE;

-- =====================================================
-- Users Table
-- =====================================================
CREATE TABLE "Users" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "Email" VARCHAR(255) NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

-- Unique index on Email
CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");

-- =====================================================
-- Documents Table
-- =====================================================
CREATE TABLE "Documents" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,
    "OriginalFileName" VARCHAR(500) NOT NULL,
    "MimeType" VARCHAR(100) NOT NULL,
    "StoragePath" VARCHAR(1000) NOT NULL,
    "UploadedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "OcrText" TEXT,
    "ParsedJson" TEXT,
    "ParseStatus" INTEGER NOT NULL DEFAULT 0,
    "ParseError" TEXT,

    -- Foreign key to Users with cascade delete
    CONSTRAINT "FK_Documents_Users_UserId"
        FOREIGN KEY ("UserId")
        REFERENCES "Users" ("Id")
        ON DELETE CASCADE
);

-- Index on UserId for faster lookups
CREATE INDEX "IX_Documents_UserId" ON "Documents" ("UserId");

-- =====================================================
-- Expenses Table
-- =====================================================
CREATE TABLE "Expenses" (
    "Id" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    "UserId" UUID NOT NULL,
    "DocumentId" UUID,
    "BusinessName" VARCHAR(500) NOT NULL,
    "BusinessId" VARCHAR(100),
    "InvoiceNumber" VARCHAR(100),
    "ServiceDescription" VARCHAR(1000),
    "TransactionDate" TIMESTAMP WITH TIME ZONE NOT NULL,
    "AmountBeforeVat" DECIMAL(18,2) NOT NULL,
    "AmountAfterVat" DECIMAL(18,2) NOT NULL,
    "VatAmount" DECIMAL(18,2) NOT NULL,
    "Category" TEXT NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    "UpdatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),

    -- Foreign key to Users with cascade delete
    CONSTRAINT "FK_Expenses_Users_UserId"
        FOREIGN KEY ("UserId")
        REFERENCES "Users" ("Id")
        ON DELETE CASCADE,

    -- Foreign key to Documents with set null on delete
    CONSTRAINT "FK_Expenses_Documents_DocumentId"
        FOREIGN KEY ("DocumentId")
        REFERENCES "Documents" ("Id")
        ON DELETE SET NULL
);

-- Indexes for faster lookups
CREATE INDEX "IX_Expenses_UserId" ON "Expenses" ("UserId");
CREATE INDEX "IX_Expenses_DocumentId" ON "Expenses" ("DocumentId");

-- =====================================================
-- Verification Queries
-- =====================================================
-- List all tables
SELECT tablename FROM pg_tables WHERE schemaname = 'public';

-- Check table structures
\d "Users"
\d "Documents"
\d "Expenses"
