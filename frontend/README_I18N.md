# Internationalization (i18n) - Hebrew Support

## Overview
The app now supports both English and Hebrew with automatic RTL (Right-to-Left) layout switching.

## What's Implemented

### 1. **i18n Configuration**
- `src/i18n/config.js` - i18next configuration
- `src/i18n/locales/en.json` - English translations
- `src/i18n/locales/he.json` - Hebrew translations (עברית)

### 2. **Language Switcher**
- Added to Header component
- Persists language choice in localStorage
- Automatically switches HTML dir attribute (ltr/rtl)
- Shows language icon with toggle

### 3. **RTL Support**
- Automatic direction switching based on language
- Tailwind CSS configured for RTL
- Hebrew text displays right-to-left correctly

## How to Use Translations in Components

### Import and Use
```jsx
import { useTranslation } from 'react-i18next';

function MyComponent() {
  const { t } = useTranslation();

  return (
    <div>
      <h1>{t('auth.welcomeBack')}</h1>
      <button>{t('common.save')}</button>
    </div>
  );
}
```

### Translation Keys Structure
```
common.*          - Common UI text (save, cancel, delete, etc.)
auth.*            - Authentication related
dashboard.*       - Dashboard tabs
upload.*          - Upload page
form.*            - Form fields and validation
expenses.*        - Expenses management
categories.*      - Expense categories
dateRanges.*      - Date range filters
```

## Example: Updating LoginPage

### Before (hardcoded):
```jsx
<h1>Welcome Back</h1>
<button>Sign In</button>
```

### After (translated):
```jsx
const { t } = useTranslation();

<h1>{t('auth.welcomeBack')}</h1>
<button>{t('auth.signIn')}</button>
```

## Testing
1. Click the language switcher in header (עב / EN button)
2. Watch UI switch between English and Hebrew
3. Hebrew layout automatically becomes RTL

## Next Steps to Complete Full Translation

Update these components to use `t()`:
- [ ] LoginPage
- [ ] RegisterPage
- [ ] DashboardPage (tabs)
- [ ] UploadTab
- [ ] ExpensesTab
- [ ] ExpenseFilters
- [ ] ExpenseTable
- [ ] ExtractedDataForm
- [ ] Common components (Button, Input, Select, etc.)

## Adding New Translations
1. Add to `src/i18n/locales/en.json`
2. Add Hebrew translation to `src/i18n/locales/he.json`
3. Use in component with `t('your.new.key')`
