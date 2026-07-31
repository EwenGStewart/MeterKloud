# Copilot Instructions

## Project Guidelines
- For net energy reporting, user expects generation/net export values to remain negative (do not invert to positive).
- For CsvMultiLine2, generation (B channel) should be stored as positive values like NEM12, and net energy should be computed as E1 + E2 - B1 (negative indicates net generation/export).