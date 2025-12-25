# Documentation Index

This directory contains internal documentation, planning materials, and sample files for the Expense Manager project.

## Main Documentation

For the primary project documentation, see the root directory:

- **[Main README](../README.md)** - Project overview, quick start, and setup
- **[ARCHITECTURE.md](../ARCHITECTURE.md)** - System architecture and design
- **[API_REFERENCE.md](../API_REFERENCE.md)** - Complete API endpoint documentation
- **[DEPLOYMENT.md](../DEPLOYMENT.md)** - Production deployment guide

## Component Documentation

- **[Frontend README](../frontend/README.md)** - React application documentation
- **[Backend README](../backend/README.md)** - .NET API documentation
- **[i18n Guide](../frontend/README_I18N.md)** - Internationalization implementation
- **[Performance Improvements](../backend/PERFORMANCE_IMPROVEMENTS.md)** - OCR optimization details
- **[Regex Specification](../backend/REGEX_EXTRACTION_SPECIFICATION.md)** - Extraction pattern documentation

## Internal Documentation

### Planning Documents (`_planning/`)

Contains original planning and design documents:
- `plan.md` - Initial project plan
- `backend.md` - Backend architecture planning
- `frontend-react-plan.md` - Frontend implementation plan

These are historical documents used during initial development.

### Sample Files (`samples/`)

Sample receipt and invoice files for testing OCR functionality.

## Documentation Organization

```
expense-manager/
├── README.md                    # Main project overview
├── ARCHITECTURE.md              # System architecture
├── API_REFERENCE.md             # API documentation
├── DEPLOYMENT.md                # Deployment guide
│
├── frontend/
│   ├── README.md               # Frontend documentation
│   └── README_I18N.md          # i18n guide
│
├── backend/
│   ├── README.md               # Backend documentation
│   ├── PERFORMANCE_IMPROVEMENTS.md
│   └── REGEX_EXTRACTION_SPECIFICATION.md
│
└── docs/
    ├── README.md               # This file
    ├── _planning/              # Historical planning docs
    └── samples/                # Sample receipt files
```

## Getting Started

If you're new to this project:

1. Start with the [Main README](../README.md) for an overview
2. Read [ARCHITECTURE.md](../ARCHITECTURE.md) to understand the system design
3. Follow the setup instructions in the component READMEs
4. Refer to [API_REFERENCE.md](../API_REFERENCE.md) for API details
5. Check [DEPLOYMENT.md](../DEPLOYMENT.md) for deployment instructions

## Contributing

When adding new documentation:

1. Place user-facing docs in the root directory
2. Place technical specs in component directories (frontend/, backend/)
3. Place internal/historical docs in docs/_planning/
4. Update this index when adding major documentation

## License

All documentation is proprietary. All rights reserved.
