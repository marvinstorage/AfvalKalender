# Privacy & Security Rules

- **NEVER** commit files containing real Dutch postcodes or addresses (e.g., test files, API responses, or generated ICS files with real user data like a real valid Dutch postcode or address).
- **NEVER** commit `.db` SQLite databases (like `afvalkalender.db`) or API cache files (`apicache/` JSONs) to prevent exposing personal data or temporary application state. Always ensure these are ignored in `.gitignore`.
