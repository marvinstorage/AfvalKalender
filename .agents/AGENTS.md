# Privacy & Security Rules

- **NEVER** commit files containing real Dutch postcodes or addresses (e.g., test files, API responses, or generated ICS files with real user data to a real valid dutch postcode or addresses).
- **NEVER** commit `.db` SQLite databases (like `afvalkalender.db`) or API cache files (`apicache/` JSONs) to prevent exposing personal data or temporary application state. Always ensure these are ignored in `.gitignore`.

# Feature Implementation Rules

- **ALWAYS** add unit tests, update `README.md`, update architecture documentation/diagrams, and add an ADR (Architecture Decision Record) when implementing a new feature.
