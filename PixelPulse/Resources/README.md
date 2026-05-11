# Resources Folder

This folder contains embedded resources for the Pixel Pulse application.

## quotes.db
Pre-populated SQLite database containing all quotes. This file should be added as an embedded resource in the project file.

To generate the quotes.db file:
1. Run the DatabaseBuilder project: `.\PixelPulse.DatabaseBuilder\bin\Release\net8.0\PixelPulse.DatabaseBuilder.exe`
2. Copy the generated database from `%APPDATA%\PixelPulse\quotes.db` to this folder
3. Update the project file to include it as an embedded resource

The database should contain approximately 340,000+ quotes from all sources.
