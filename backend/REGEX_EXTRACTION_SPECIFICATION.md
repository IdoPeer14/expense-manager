# Regex-Based Receipt & Invoice Extraction Engine
## Specification Document v1.0

---

## Executive Summary

This document specifies a deterministic, rule-based text extraction system for parsing receipts and invoices in **Hebrew** and **English**. The system uses Regular Expressions (Regex) as the primary extraction mechanism, with AI serving only as a fallback for ambiguous cases.

**Design Principles:**
- **Deterministic**: Same input always produces same output
- **Explainable**: Every extraction can be traced to a specific pattern
- **Robust**: Tolerant to OCR noise, spacing variations, and formatting inconsistencies
- **Multilingual**: Native support for Hebrew (RTL) and English (LTR) documents
- **Maintainable**: Modular patterns, each field extracted independently

---

## System Architecture

### Extraction Pipeline

```
┌─────────────────────────────────────────────────────────────┐
│  1. OCR Text Input                                          │
│     (Raw, noisy, potentially unordered)                     │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────────┐
│  2. Text Normalization                                      │
│     - Unicode normalization (NFC)                           │
│     - Smart quotes → ASCII quotes                           │
│     - Multiple spaces → single space                        │
│     - Preserve Hebrew characters (U+0590 to U+05FF)         │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────────┐
│  3. Field-by-Field Regex Extraction                         │
│     - Apply ordered pattern matching per field              │
│     - First successful match wins (priority-based)          │
│     - Extract raw value + context                           │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────────┐
│  4. Post-Processing & Validation                            │
│     - Type conversion (string → decimal/date)               │
│     - Validation rules (range checks, format checks)        │
│     - Confidence scoring (0.0 - 1.0)                        │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────────┐
│  5. Fallback Logic                                          │
│     - Derive missing fields (e.g., beforeVAT = total - VAT) │
│     - Heuristic-based extraction for weak signals           │
│     - AI fallback for low-confidence extractions            │
└───────────────────────┬─────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────────┐
│  6. Structured Output                                       │
│     ParsedInvoiceData with confidence scores                │
└─────────────────────────────────────────────────────────────┘
```

---

## Field Extraction Specifications

### 1. Document Type

**Purpose:** Classify document as Receipt, Invoice, Tax Invoice, etc.

**Regex Pattern:**
```regex
(?:חשבונית\s*מס|חשבונית|קבלה|חש[׳']?\s*מס|tax\s*invoice|invoice|receipt)
```

**Supported Languages:** Hebrew, English

**Pattern Explanation:**
- `חשבונית\s*מס`: "Tax Invoice" in Hebrew (with optional space/punctuation)
- `חשבונית`: "Invoice" in Hebrew
- `קבלה`: "Receipt" in Hebrew
- `חש[׳']?\s*מס`: Abbreviated "Tax Invoice" (חש׳ מס / חש' מס)
- `tax\s*invoice`: English "Tax Invoice"
- `invoice|receipt`: English variants

**Example Inputs & Outputs:**

| OCR Text (Hebrew)        | Extracted Value    | Confidence |
|-------------------------|--------------------|------------|
| `חשבונית מס/קבלה`       | `Tax Invoice`      | 1.0        |
| `חשבונית #12345`        | `Invoice`          | 1.0        |
| `קבלה על תשלום`         | `Receipt`          | 1.0        |

| OCR Text (English)       | Extracted Value    | Confidence |
|-------------------------|--------------------|------------|
| `TAX INVOICE`           | `Tax Invoice`      | 1.0        |
| `Receipt No. 789`       | `Receipt`          | 1.0        |

**Edge Cases:**
- OCR may insert spaces in Hebrew compound words: `חשבונ ית` → normalize before matching
- Case variations in English: use `RegexOptions.IgnoreCase`

---

### 2. Invoice/Receipt Number

**Purpose:** Extract unique document identifier

**Regex Patterns (Priority Order):**

```regex
# Priority 1: Explicit invoice number with label
(?:חשבונית\s*(?:מס[׳']?)?|invoice)\s*(?:מס[׳']|מספר|#|num|no\.?|number)?\s*[:\-]?\s*(\d{4,12})

# Priority 2: Receipt number with label
(?:קבלה|receipt)\s*(?:מס[׳']|מספר|#|num|no\.?|number)?\s*[:\-]?\s*(\d{4,12})

# Priority 3: Generic number label (must be near top of document)
(?:מס[׳']|מספר|#|no\.?)\s*[:\-]?\s*(\d{4,12})

# Priority 4: Standalone numeric ID (8-12 digits, near document type keyword)
(?<=(?:חשבונית|invoice|קבלה|receipt).{0,50})(\d{8,12})
```

**Supported Languages:** Hebrew, English

**Pattern Explanation:**
- **Group 1**: Captures document type + explicit number label
- **Group 2**: Handles receipts separately (different numbering schemes)
- **Group 3**: Generic "Number:" or "מס:" labels
- **Group 4**: Lookbehind to find standalone numbers near document keywords

**Validation Rules:**
- Must be 4-12 digits (Israeli invoices typically 6-9 digits)
- Exclude numbers that match Business ID patterns (9 digits)
- Exclude dates (DDMMYYYY format)

**Example Inputs & Outputs:**

| OCR Text                           | Extracted Value | Pattern Used | Confidence |
|-----------------------------------|-----------------|--------------|------------|
| `חשבונית מס' 123456`              | `123456`        | Priority 1   | 1.0        |
| `Invoice No. 98765432`            | `98765432`      | Priority 1   | 1.0        |
| `קבלה #45678`                     | `45678`         | Priority 2   | 1.0        |
| `מס' 334455` (top of doc)         | `334455`        | Priority 3   | 0.85       |
| `Invoice\n12345678` (no label)    | `12345678`      | Priority 4   | 0.7        |

**Edge Cases:**
- **Multiple numbers found**: Use document position heuristic (prefer top 20% of document)
- **Booking IDs vs Invoice IDs**: Exclude if labeled as "הזמנה" / "Booking" / "Order"
- **Check numbers**: Exclude if near "המחאה" / "Check No."

---

### 3. Transaction Date

**Purpose:** Extract document issuance date

**Regex Patterns (Priority Order):**

```regex
# Priority 1: Explicit date label (Hebrew/English)
(?:תאריך|date)\s*[:\-]?\s*(\d{1,2})[\/\.\-](\d{1,2})[\/\.\-](\d{2,4})

# Priority 2: Common Israeli format (DD/MM/YYYY or DD.MM.YYYY)
\b(\d{1,2})[\/\.](\d{1,2})[\/\.](\d{4})\b

# Priority 3: ISO format (YYYY-MM-DD)
\b(\d{4})[\/\-](\d{1,2})[\/\-](\d{1,2})\b

# Priority 4: Compact format (DDMMYYYY)
\b(\d{2})(\d{2})(\d{4})\b

# Priority 5: Hebrew date format (יום/חודש/שנה)
(\d{1,2})[\/\.](\d{1,2})[\/\.](\d{2,4})
```

**Supported Languages:** Hebrew, English

**Pattern Explanation:**
- Captures 3 groups: day, month, year (or year, month, day for ISO)
- Supports separators: `/`, `.`, `-`
- Supports 2-digit and 4-digit years

**Post-Processing Logic:**
```csharp
// Normalize year
if (year < 100) year += 2000;

// Detect format ambiguity (DD/MM vs MM/DD)
if (firstPart > 12 && secondPart <= 12) {
    // firstPart must be day
    day = firstPart; month = secondPart;
} else if (secondPart > 12 && firstPart <= 12) {
    // secondPart must be day
    day = secondPart; month = firstPart;
} else {
    // Prefer Israeli format (DD/MM) unless context suggests otherwise
    day = firstPart; month = secondPart;
}

// Validation
if (day > 31 || month > 12 || year < 2000 || year > DateTime.Now.Year + 1)
    return null;
```

**Example Inputs & Outputs:**

| OCR Text                      | Extracted Value  | Pattern | Confidence |
|------------------------------|------------------|---------|------------|
| `תאריך: 25/12/2024`          | `2024-12-25`     | P1      | 1.0        |
| `Date: 25.12.2024`           | `2024-12-25`     | P1      | 1.0        |
| `12/03/2024`                 | `2024-03-12`     | P2      | 0.9        |
| `2024-03-15`                 | `2024-03-15`     | P3      | 1.0        |
| `25122024`                   | `2024-12-25`     | P4      | 0.8        |

**Edge Cases:**
- **Ambiguous dates** (e.g., 03/05/2024): Default to DD/MM/YYYY for Israeli documents
- **Multiple dates found**: Prefer dates near "תאריך" / "Date" labels
- **Future dates**: Allow up to 1 year in future (for subscriptions)
- **OCR corruption**: `O` may become `0`, `l` may become `1`

---

### 4. Business Name

**Purpose:** Extract merchant/supplier name

**Regex Patterns (Priority Order):**

```regex
# Priority 1: Explicit business name label
(?:שם\s*העסק|business\s*name|company\s*name)\s*[:\-]?\s*\n?\s*([^\n]{3,80})

# Priority 2: Limited company designations
([א-ת\s]+)\s*(?:בע[״"]מ|בע״מ|Ltd\.?|Inc\.?|LLC)

# Priority 3: First substantial line of document (heuristic)
^([^\n]{5,80})$
```

**Supported Languages:** Hebrew, English

**Pattern Explanation:**
- **Group 1**: Explicit labels in both languages
- **Group 2**: Captures company names with legal suffixes (Ltd., בע"מ)
- **Group 3**: Fallback to first line (assumes letterhead placement)

**Exclusion Filters (Applied Post-Match):**
```regex
# Exclude lines containing these keywords:
(חשבונית|invoice|receipt|קבלה|total|סה"כ|VAT|מע"מ|date|תאריך|\d{8,})

# Exclude lines that are:
# - Pure numbers
# - URLs
# - Email addresses
# - Shorter than 3 chars or longer than 80 chars
```

**Example Inputs & Outputs:**

| OCR Text                                  | Extracted Value           | Pattern | Confidence |
|------------------------------------------|---------------------------|---------|------------|
| `שם העסק: חברת הדפוס בע"מ`               | `חברת הדפוס בע"מ`         | P1      | 1.0        |
| `Business Name: Acme Corp Ltd.`          | `Acme Corp Ltd.`          | P1      | 1.0        |
| `טכנולוגיות ABC בע"מ` (first line)      | `טכנולוגיות ABC בע"מ`     | P2      | 0.95       |
| `Startup Inc.` (first line)              | `Startup Inc.`            | P2      | 0.95       |
| `SuperMarket 24/7` (no label, 3rd line)  | `SuperMarket 24/7`        | P3      | 0.7        |

**Edge Cases:**
- **Multi-line names**: Capture only first line to avoid capturing address
- **OCR splits**: "בע מ" → normalize to "בע״מ"
- **Ambiguous abbreviations**: "מ.כ." (Registered Dealer) vs "בע״מ" (Ltd.)

---

### 5. Business Identifier (Tax ID / Company Number)

**Purpose:** Extract government-issued business registration number

**Regex Patterns (Priority Order):**

```regex
# Priority 1: Israeli Company Number (ח.פ.)
(?:ח\.פ\.|ח״פ|חפ)\s*[:\-]?\s*([\d\-]{8,10})

# Priority 2: Licensed Dealer (עוסק מורשה)
(?:ע\.מ\.|עוסק\s*מורשה|ע״מ)\s*[:\-]?\s*([\d\-]{8,10})

# Priority 3: Licensed Partnership (עוסק פטור)
(?:ע\.פ\.|עוסק\s*פטור)\s*[:\-]?\s*([\d\-]{8,10})

# Priority 4: VAT Number (explicit)
(?:VAT\s*(?:No|Number|ID)|מע"מ\s*(?:מס|מספר))\s*[:\-]?\s*([\d\-]{8,12})

# Priority 5: Generic Tax ID / Company ID
(?:Company\s*ID|Tax\s*ID|Business\s*ID|Company\s*No)\s*[:\-]?\s*([\d\-]{8,12})

# Priority 6: Standalone 9-digit ID (Israeli standard)
\b(\d{9})\b
```

**Supported Languages:** Hebrew, English

**Pattern Explanation:**
- Israeli business IDs are 9 digits (some older formats: 8 digits)
- Hyphens may appear in various positions (513-123-456)
- Different business types use different prefixes

**Post-Processing:**
```csharp
// Normalize: remove all non-digits
string normalized = Regex.Replace(rawValue, @"[^\d]", "");

// Validate Israeli business ID (Luhn-like check)
if (normalized.Length == 9) {
    int checksum = CalculateIsraeliBusinessIDChecksum(normalized);
    if (!IsValidChecksum(checksum)) {
        confidence *= 0.7; // Lower confidence if checksum fails
    }
}
```

**Example Inputs & Outputs:**

| OCR Text                          | Extracted Value | Pattern | Confidence |
|----------------------------------|-----------------|---------|------------|
| `ח.פ. 513-123-456`               | `513123456`     | P1      | 1.0        |
| `עוסק מורשה: 51-3123456`         | `513123456`     | P2      | 1.0        |
| `VAT Number: IL-123456789`       | `123456789`     | P4      | 0.95       |
| `Company ID: 987654321`          | `987654321`     | P5      | 0.9        |
| `123456789` (standalone)         | `123456789`     | P6      | 0.6        |

**Edge Cases:**
- **Multiple IDs**: Some documents show both ח.פ. and ע.מ. → prioritize ח.פ.
- **Foreign IDs**: Non-Israeli VAT numbers may have letters (UK: GB123456789) → preserve
- **OCR errors**: `O` → `0`, `S` → `5`, `B` → `8`

---

### 6. Total Amount (After VAT)

**Purpose:** Extract final payable amount including all taxes

**Regex Patterns (Priority Order):**

```regex
# Priority 1: Explicit "Total Due" / "Total Amount"
(?:Total\s*Due|Total\s*Amount|Grand\s*Total|Amount\s*Due)\s*[:\-]?\s*(?:₪|NIS|\$|ILS)?\s*([\d,]+\.?\d{0,2})

# Priority 2: Hebrew "Total" (סה"כ / סכום כולל)
(?:סה["״']כ\s*לתשלום|סה["״']כ|סכום\s*כולל)\s*[:\-]?\s*(?:₪|NIS|\$|ILS)?\s*([\d,]+\.?\d{0,2})

# Priority 3: "Total including VAT" (explicit)
(?:Total\s*(?:including|incl\.?)\s*VAT|כולל\s*מע["״']מ)\s*[:\-]?\s*(?:₪|NIS|\$|ILS)?\s*([\d,]+\.?\d{0,2})

# Priority 4: Currency symbol followed by amount (bottom of document)
(?:₪|NIS|\$|ILS)\s*([\d,]+\.?\d{0,2})(?=\s*$)

# Priority 5: Largest monetary value in document (heuristic)
(?:₪|NIS|\$|ILS)\s*([\d,]+\.?\d{0,2})
```

**Supported Languages:** Hebrew, English

**Pattern Explanation:**
- Captures numeric value with optional thousands separators (`,`)
- Supports up to 2 decimal places
- Handles multiple currency symbols (₪, NIS, $, ILS)

**Post-Processing:**
```csharp
// Normalize: remove commas, parse decimal
string normalized = rawValue.Replace(",", "");
decimal amount = decimal.Parse(normalized, CultureInfo.InvariantCulture);

// Validation
if (amount <= 0 || amount > 1_000_000) {
    return null; // Unrealistic values
}

// Context validation
if (patternPriority >= 4) {
    // Check if near "total" keywords within 3 lines
    if (!IsNearKeyword(context, new[] {"total", "סה\"כ", "amount"})) {
        confidence *= 0.7;
    }
}
```

**Example Inputs & Outputs:**

| OCR Text                           | Extracted Value | Pattern | Confidence |
|-----------------------------------|-----------------|---------|------------|
| `Total Due: ₪1,250.50`            | `1250.50`       | P1      | 1.0        |
| `סה"כ לתשלום: 850.00 ₪`           | `850.00`        | P2      | 1.0        |
| `Total incl. VAT: $499.99`        | `499.99`        | P3      | 1.0        |
| `₪345.00` (last line)             | `345.00`        | P4      | 0.8        |
| `₪500.00` (mid-document)          | `500.00`        | P5      | 0.6        |

**Edge Cases:**
- **Multiple totals**: Prefer labeled amounts over unlabeled
- **Subtotals vs Grand Total**: Exclude "subtotal" / "סכום ביניים"
- **OCR decimal confusion**: Differentiate between `.` (decimal) and `,` (thousands)
  - Israeli format: `1.250,50` (European) vs `1,250.50` (Anglo-American)
  - Use locale hints or heuristics (Israeli docs often use `,` as decimal)

---

### 7. Amount Before VAT

**Purpose:** Extract net amount (pre-tax)

**Regex Patterns (Priority Order):**

```regex
# Priority 1: Explicit "Before VAT" label
(?:Amount\s*)?(?:Before|excl\.?|excluding)\s*VAT\s*[:\-]?\s*(?:₪|NIS|\$|ILS)?\s*([\d,]+\.?\d{0,2})

# Priority 2: Hebrew "Before VAT" (לפני מע"מ)
(?:סכום\s*)?לפני\s*מע["״']מ\s*[:\-]?\s*(?:₪|NIS|\$|ILS)?\s*([\d,]+\.?\d{0,2})

# Priority 3: Subtotal (often means pre-tax)
(?:Subtotal|Sub-Total|סכום\s*ביניים)\s*[:\-]?\s*(?:₪|NIS|\$|ILS)?\s*([\d,]+\.?\d{0,2})

# Priority 4: Net Amount (explicit)
(?:Net\s*Amount|Net|נטו)\s*[:\-]?\s*(?:₪|NIS|\$|ILS)?\s*([\d,]+\.?\d{0,2})
```

**Supported Languages:** Hebrew, English

**Fallback Logic (if no pattern matches):**
```csharp
// If we have Total and VAT, calculate:
if (totalAmount.HasValue && vatAmount.HasValue) {
    beforeVat = totalAmount.Value - vatAmount.Value;
    confidence = Math.Min(totalConfidence, vatConfidence) * 0.95;
}
```

**Example Inputs & Outputs:**

| OCR Text                          | Extracted Value | Pattern   | Confidence |
|----------------------------------|-----------------|-----------|------------|
| `Before VAT: ₪1,000.00`          | `1000.00`       | P1        | 1.0        |
| `לפני מע"מ: 720.00 ₪`            | `720.00`        | P2        | 1.0        |
| `Subtotal: $450.00`              | `450.00`        | P3        | 0.9        |
| `Net: ₪300.00`                   | `300.00`        | P4        | 0.9        |
| Calculated: 850 - 144.50         | `705.50`        | Fallback  | 0.85       |

**Edge Cases:**
- **Subtotal ambiguity**: "Subtotal" might include some taxes but not all (e.g., before sales tax but after service charge)
- **Multiple line items**: Ensure we capture document-level amount, not item-level

---

### 8. VAT / Tax Amount

**Purpose:** Extract value-added tax or sales tax amount

**Regex Patterns (Priority Order):**

```regex
# Priority 1: VAT with percentage label (e.g., "VAT (17%)")
VAT\s*\((\d+)%\)\s*[:\-]?\s*(?:₪|NIS|\$|ILS)?\s*([\d,]+\.?\d{0,2})

# Priority 2: Hebrew VAT label (מע"מ)
מע["״']מ\s*(?:\((\d+)%\))?\s*[:\-]?\s*(?:₪|NIS|\$|ILS)?\s*([\d,]+\.?\d{0,2})

# Priority 3: Generic "Tax" label
(?:Tax|Sales\s*Tax|GST|CGST\s*\+\s*SGST)\s*[:\-]?\s*(?:₪|NIS|\$|ILS)?\s*([\d,]+\.?\d{0,2})

# Priority 4: Amount followed by "VAT" (reversed format)
([\d,]+\.?\d{0,2})\s*(?:₪|NIS|\$|ILS)?\s*מע["״']מ

# Priority 5: Amount followed by "VAT" (English reversed)
([\d,]+\.?\d{0,2})\s*(?:₪|NIS|\$|ILS)?\s*VAT
```

**Supported Languages:** Hebrew, English

**Validation Logic:**
```csharp
// Israeli standard VAT rate: 17% (as of 2024)
const decimal ISRAEL_VAT_RATE = 0.17m;

// Validate VAT amount against total (if available)
if (totalAmount.HasValue) {
    decimal expectedVAT = totalAmount.Value * ISRAEL_VAT_RATE / (1 + ISRAEL_VAT_RATE);
    decimal deviation = Math.Abs(vatAmount - expectedVAT) / expectedVAT;

    if (deviation > 0.05) { // More than 5% deviation
        confidence *= 0.8;
    }
}
```

**Example Inputs & Outputs:**

| OCR Text                          | Extracted Value | Pattern | Confidence |
|----------------------------------|-----------------|---------|------------|
| `VAT (17%): ₪170.00`             | `170.00`        | P1      | 1.0        |
| `מע"מ (17%): 85.50 ₪`            | `85.50`         | P2      | 1.0        |
| `Sales Tax: $45.00`              | `45.00`         | P3      | 0.95       |
| `120.00 ₪ מע"מ`                  | `120.00`        | P4      | 0.9        |
| `$30.00 VAT`                     | `30.00`         | P5      | 0.9        |

**Fallback Logic:**
```csharp
// If VAT not found but we have total amount
if (totalAmount.HasValue && !vatAmount.HasValue) {
    vatAmount = totalAmount.Value * ISRAEL_VAT_RATE / (1 + ISRAEL_VAT_RATE);
    confidence = totalConfidence * 0.7; // Lower confidence for calculated value
}
```

**Edge Cases:**
- **Multiple VAT rates**: Some jurisdictions have different rates (reduced rate for food, etc.)
- **VAT-exempt items**: Document may state "VAT Exempt" or "פטור ממע"מ"
- **Indian GST**: CGST + SGST = Total GST (need to sum)
- **Reverse charge**: VAT may be marked as "Customer Responsibility"

---

### 9. Optional: Reference Numbers (Order ID / Booking ID)

**Purpose:** Extract auxiliary identifiers for correlation

**Regex Patterns (Priority Order):**

```regex
# Priority 1: Order ID
(?:Order\s*(?:ID|No|Number)|הזמנה\s*(?:מס|מספר))\s*[:\-#]?\s*([A-Z0-9\-]{4,20})

# Priority 2: Booking ID / Confirmation Number
(?:Booking\s*(?:ID|No|Number)|Confirmation|אישור\s*(?:מס|מספר))\s*[:\-#]?\s*([A-Z0-9\-]{4,20})

# Priority 3: Reference Number
(?:Reference|Ref|אסמכתא)\s*[:\-#]?\s*([A-Z0-9\-]{4,20})

# Priority 4: Transaction ID
(?:Transaction\s*(?:ID|No)|עסקה\s*מס)\s*[:\-#]?\s*([A-Z0-9\-]{4,20})
```

**Supported Languages:** Hebrew, English

**Pattern Explanation:**
- Captures alphanumeric IDs (4-20 characters)
- Supports hyphens in IDs (e.g., "ORD-2024-12345")
- Differentiated from Invoice Number by keyword context

**Example Inputs & Outputs:**

| OCR Text                              | Extracted Value      | Pattern | Type          |
|--------------------------------------|---------------------|---------|---------------|
| `Order ID: ORD-123456`               | `ORD-123456`        | P1      | Order ID      |
| `הזמנה מספר: 789-ABC`                | `789-ABC`           | P1      | Order ID      |
| `Booking No: BK20241225`             | `BK20241225`        | P2      | Booking ID    |
| `Reference: REF-2024-XYZ`            | `REF-2024-XYZ`      | P3      | Reference     |
| `Transaction ID: TXN987654321`       | `TXN987654321`      | P4      | Transaction   |

**Edge Cases:**
- **Confusion with Invoice Number**: Ensure these patterns run AFTER invoice number extraction
- **Multiple references**: A document may have both Order ID and Booking ID → store separately

---

## Advanced Extraction Techniques

### 1. Context-Aware Extraction

**Technique:** Use surrounding text to validate matches

```csharp
public bool IsMatchValid(Match match, string fullText, int maxDistanceLines = 3) {
    // Get position of match in text
    int matchPosition = match.Index;

    // Extract surrounding lines
    string context = GetContextLines(fullText, matchPosition, maxDistanceLines);

    // Check for confirming keywords
    bool hasConfirmingKeywords = ConfirmingKeywords.Any(kw =>
        context.Contains(kw, StringComparison.OrdinalIgnoreCase));

    // Check for negating keywords
    bool hasNegatingKeywords = NegatingKeywords.Any(kw =>
        context.Contains(kw, StringComparison.OrdinalIgnoreCase));

    return hasConfirmingKeywords && !hasNegatingKeywords;
}
```

### 2. Position-Based Scoring

**Technique:** Prioritize matches based on document position

```csharp
public float GetPositionConfidence(Match match, string fullText) {
    float position = (float)match.Index / fullText.Length;

    // For business name: prefer top 20%
    if (fieldType == FieldType.BusinessName) {
        return position < 0.2f ? 1.0f : 0.7f;
    }

    // For total amount: prefer bottom 30%
    if (fieldType == FieldType.TotalAmount) {
        return position > 0.7f ? 1.0f : 0.8f;
    }

    return 0.9f; // Default
}
```

### 3. Multi-Pass Extraction

**Technique:** Extract fields in dependency order

```csharp
// Pass 1: Extract independent fields
documentType = ExtractDocumentType(text);
invoiceNumber = ExtractInvoiceNumber(text);
businessId = ExtractBusinessId(text);

// Pass 2: Extract dependent fields (use Pass 1 results for context)
businessName = ExtractBusinessName(text, documentType);
date = ExtractDate(text, documentType);

// Pass 3: Extract monetary fields (validate against each other)
totalAmount = ExtractTotalAmount(text);
vatAmount = ExtractVATAmount(text, totalAmount);
beforeVatAmount = ExtractBeforeVAT(text, totalAmount, vatAmount);

// Pass 4: Validate and derive missing fields
ValidateMonetaryConsistency(ref totalAmount, ref vatAmount, ref beforeVatAmount);
```

---

## Text Normalization Pipeline

### Pre-Processing Steps

```csharp
public string NormalizeOCRText(string rawText) {
    if (string.IsNullOrWhiteSpace(rawText)) return string.Empty;

    // 1. Unicode normalization (combine diacritics)
    string normalized = rawText.Normalize(NormalizationForm.FormC);

    // 2. Convert smart quotes to ASCII
    normalized = normalized
        .Replace(""", "\"").Replace(""", "\"")
        .Replace("'", "'").Replace("'", "'")
        .Replace("״", "\"").Replace("׳", "'");

    // 3. Normalize Hebrew punctuation
    normalized = normalized
        .Replace("־", "-")  // Hebrew hyphen → ASCII hyphen
        .Replace("‪", "")   // Remove LTR mark
        .Replace("‫", "");  // Remove RTL mark

    // 4. Normalize whitespace (preserve newlines)
    normalized = Regex.Replace(normalized, @"[ \t]+", " ");
    normalized = Regex.Replace(normalized, @"\n{3,}", "\n\n");

    // 5. Fix common OCR errors in Hebrew
    normalized = normalized
        .Replace("מ ע מ", "מע\"מ")
        .Replace("ח פ", "ח.פ.")
        .Replace("ע מ", "ע.מ.");

    // 6. Fix common OCR errors in English
    normalized = normalized
        .Replace("V A T", "VAT")
        .Replace("0", "O") // Only in specific contexts
        .Replace("l", "I"); // Only in specific contexts

    return normalized.Trim();
}
```

---

## Confidence Scoring System

### Scoring Methodology

```csharp
public class ExtractionResult {
    public string? Value { get; set; }
    public float Confidence { get; set; }
    public string PatternUsed { get; set; }
    public ConfidenceFactors Factors { get; set; }
}

public class ConfidenceFactors {
    public float PatternPriority { get; set; }      // 0.0 - 1.0
    public float ContextValidation { get; set; }    // 0.0 - 1.0
    public float PositionScore { get; set; }        // 0.0 - 1.0
    public float CrossFieldValidation { get; set; } // 0.0 - 1.0
}

public float CalculateOverallConfidence(ConfidenceFactors factors) {
    return (factors.PatternPriority * 0.4f +
            factors.ContextValidation * 0.3f +
            factors.PositionScore * 0.2f +
            factors.CrossFieldValidation * 0.1f);
}
```

### Confidence Thresholds

| Confidence Range | Action                                      |
|-----------------|---------------------------------------------|
| 0.95 - 1.0      | Accept with high confidence                 |
| 0.8 - 0.95      | Accept with normal confidence               |
| 0.6 - 0.8       | Accept but flag for review                  |
| 0.4 - 0.6       | Trigger AI fallback                         |
| 0.0 - 0.4       | Reject extraction, mark as failed           |

---

## Fallback Logic Specification

### Decision Tree

```
┌─────────────────────────────────────────┐
│  Regex Extraction Attempt               │
└────────────┬────────────────────────────┘
             │
             ▼
      ┌──────────────┐
      │ Success?     │
      │ (conf > 0.8) │
      └──────┬───────┘
             │
        ┌────┴────┐
        │         │
       YES       NO
        │         │
        ▼         ▼
    ┌─────┐   ┌──────────────────────┐
    │DONE │   │ Try Derived Value    │
    └─────┘   │ (e.g., total - VAT)  │
              └──────┬───────────────┘
                     │
                     ▼
              ┌──────────────┐
              │ Success?     │
              │ (conf > 0.6) │
              └──────┬───────┘
                     │
                ┌────┴────┐
                │         │
               YES       NO
                │         │
                ▼         ▼
            ┌─────┐   ┌──────────────┐
            │DONE │   │ Heuristic    │
            └─────┘   │ (position,   │
                      │  context)    │
                      └──────┬───────┘
                             │
                             ▼
                      ┌──────────────┐
                      │ Success?     │
                      │ (conf > 0.4) │
                      └──────┬───────┘
                             │
                        ┌────┴────┐
                        │         │
                       YES       NO
                        │         │
                        ▼         ▼
                    ┌─────┐   ┌─────────┐
                    │DONE │   │ AI      │
                    └─────┘   │Fallback │
                              └─────────┘
```

### AI Fallback Prompt Template

```text
You are a document parser assistant. Extract the following field from the OCR text:

Field: {fieldName}
Expected Type: {dataType}
Languages: Hebrew, English

OCR Text:
---
{ocrText}
---

Previous Extraction Attempts:
- Regex extraction failed (confidence: {regexConfidence})
- Derived value failed (confidence: {derivedConfidence})

Instructions:
1. Identify the {fieldName} in the text
2. Return ONLY the extracted value (no explanation)
3. If not found, return: "NOT_FOUND"
4. Provide confidence score (0.0 - 1.0)

Response Format:
{
  "value": "extracted_value_here",
  "confidence": 0.85,
  "reasoning": "brief explanation"
}
```

---

## Implementation Guidelines

### Recommended Technology Stack

- **Language**: C# (.NET 8+)
- **Regex Engine**: `System.Text.RegularExpressions` with compiled patterns
- **Unicode Support**: Full Unicode 15.0 support (Hebrew U+0590 to U+05FF)
- **Performance**: Pre-compile all regex patterns at startup

### Code Structure

```
Services/
├── InvoiceParser.cs (main orchestrator)
├── Extractors/
│   ├── IFieldExtractor.cs (interface)
│   ├── DocumentTypeExtractor.cs
│   ├── InvoiceNumberExtractor.cs
│   ├── DateExtractor.cs
│   ├── BusinessNameExtractor.cs
│   ├── BusinessIdExtractor.cs
│   ├── AmountExtractor.cs (handles all monetary fields)
│   └── ReferenceNumberExtractor.cs
├── Normalizers/
│   ├── TextNormalizer.cs
│   └── HebrewTextNormalizer.cs
├── Validators/
│   ├── IFieldValidator.cs
│   ├── DateValidator.cs
│   ├── BusinessIdValidator.cs (checksum validation)
│   └── MonetaryValidator.cs (cross-field validation)
└── Fallbacks/
    ├── DerivedValueCalculator.cs
    ├── HeuristicExtractor.cs
    └── AIFallbackService.cs
```

### Performance Optimization

```csharp
// Pre-compile regex patterns at startup
private static readonly Regex InvoiceNumberPattern = new Regex(
    @"(?:חשבונית|invoice)\s*(?:מס[׳']|#|no\.?)?\s*[:\-]?\s*(\d{4,12})",
    RegexOptions.Compiled | RegexOptions.IgnoreCase
);

// Use span-based operations for large texts
public ReadOnlySpan<char> GetContextLines(ReadOnlySpan<char> text, int position, int lines) {
    // Implementation using spans for zero-allocation performance
}

// Cache normalized text
private readonly Dictionary<string, string> _normalizationCache = new();
```

### Testing Requirements

1. **Unit Tests**: Each extractor must have 20+ test cases covering:
   - Hebrew inputs
   - English inputs
   - Mixed Hebrew/English
   - OCR noise patterns
   - Edge cases

2. **Integration Tests**: Full pipeline tests with real OCR outputs

3. **Regression Tests**: Golden dataset of 100+ real invoices

4. **Performance Tests**: Process 1000 documents in < 10 seconds

---

## Known Limitations & Future Enhancements

### Current Limitations

1. **RTL/LTR Mixing**: Complex mixed-direction text may confuse pattern matching
2. **Handwritten Text**: OCR quality degrades significantly with handwriting
3. **Multi-Currency**: Currently assumes single currency per document
4. **Line Items**: Does not extract individual line item details

### Planned Enhancements

1. **Line Item Extraction**: Regex patterns for product lines with quantities/prices
2. **Multi-Currency Support**: Currency conversion and multi-currency totals
3. **Signature Detection**: Identify presence of signatures/stamps
4. **QR Code Parsing**: Extract structured data from QR codes
5. **Language Auto-Detection**: Automatically detect document language

---

## Appendix A: Hebrew Character Classes

```regex
[א-ת]          # All Hebrew letters (Alef to Tav)
[א-ת\s]        # Hebrew letters + whitespace
[א-ת\s\d]      # Hebrew letters + whitespace + digits
[׳״]           # Hebrew quotation marks (geresh, gershayim)
[₪]            # New Israeli Shekel symbol
```

---

## Appendix B: Common OCR Errors

### Hebrew OCR Errors

| Correct | OCR Error | Fix Pattern |
|---------|-----------|-------------|
| מע"מ    | מ ע מ     | `\bmע\s*מ\b` → `מע"מ` |
| ח.פ.    | ח פ       | `\bח\s*פ\b` → `ח.פ.` |
| בע"מ    | בע מ      | `\bבע\s*מ\b` → `בע"מ` |
| ₪       | שח        | Context-dependent |

### English OCR Errors

| Correct | OCR Error | Fix Pattern |
|---------|-----------|-------------|
| O       | 0         | In words: `\b0(?=[a-zA-Z])` → `O` |
| I       | l, 1      | In words: `\b[l1](?=[a-zA-Z])` → `I` |
| S       | 5         | In words: `5(?=[a-zA-Z])` → `S` |
| B       | 8         | In words: `8(?=[a-zA-Z])` → `B` |

---

## Appendix C: Validation Checkers

### Israeli Business ID Checksum

```csharp
public static bool ValidateIsraeliBusinessID(string id) {
    if (id.Length != 9 || !id.All(char.IsDigit)) return false;

    int sum = 0;
    for (int i = 0; i < 9; i++) {
        int digit = int.Parse(id[i].ToString());
        int multiplied = digit * ((i % 2) + 1);
        sum += multiplied > 9 ? multiplied - 9 : multiplied;
    }

    return sum % 10 == 0;
}
```

### Date Range Validation

```csharp
public static bool IsValidInvoiceDate(DateTime date) {
    DateTime minDate = new DateTime(2000, 1, 1);
    DateTime maxDate = DateTime.Now.AddYears(1); // Allow 1 year future

    return date >= minDate && date <= maxDate;
}
```

### Monetary Amount Validation

```csharp
public static bool ValidateVATConsistency(decimal total, decimal vat, decimal beforeVat) {
    // Check: total = beforeVat + vat (allow 1% rounding error)
    decimal calculatedTotal = beforeVat + vat;
    decimal deviation = Math.Abs(total - calculatedTotal) / total;

    return deviation < 0.01m; // Less than 1% error
}
```

---

## Document Version History

| Version | Date       | Changes                              | Author           |
|---------|-----------|--------------------------------------|------------------|
| 1.0     | 2024-12-25| Initial specification                | Backend Team     |

---

**End of Specification**
