# Agent Instructions

## Scope
These instructions apply to the entire repository.

## Project Overview
This project implements an ASP.NET-based media gallery backed by an existing Microsoft SQL Server database. The database schema must not be altered. All data access is performed through ADO.NET using efficient, paginated queries.

## Database Schema Reference
Use the following schema reference when crafting SQL queries or reasoning about data relationships. Remember that the schema is fixed and must not be changed.

### dbo.Blacklist
- `UserID` (bigint, NOT NULL)
- `BlacklistedOn` (datetime, NOT NULL)

### dbo.Messages
- `ChannelID` (bigint, NOT NULL)
- `MessageID` (bigint, NOT NULL)
- `UserID` (bigint, NOT NULL)
- `SentDate` (datetime, NOT NULL)
- `MessageText` (nvarchar(max), NULL)
- `PhotoID` (bigint, NULL)
- `VideoID` (bigint, NULL)

### dbo.MonitoredChannels
- `ChatId` (bigint, NOT NULL)
- `ChatTitle` (nvarchar(255), NOT NULL)
- `AddedOn` (datetime, NOT NULL)
- `IsActive` (bit, NOT NULL)

### dbo.Photos
- `PhotoID` (bigint, NOT NULL)
- `FilePath` (nvarchar(1024), NOT NULL)
- `AverageHash` (bigint, NOT NULL)
- `DifferenceHash` (bigint, NOT NULL)
- `PerceptualHash` (bigint, NOT NULL)
- `AddedOn` (datetime, NOT NULL)

### dbo.PhotoTags
- `PhotoID` (bigint, NOT NULL)
- `Tag` (nvarchar(255), NOT NULL)
- `Score` (float, NOT NULL)

### dbo.UserNames
- `UserID` (bigint, NOT NULL)
- `FirstName` (nvarchar(255), NULL)
- `LastName` (nvarchar(255), NULL)
- `Username` (nvarchar(255), NULL)

### dbo.Users
- `UserID` (bigint, NOT NULL)
- `LastUpdate` (datetime, NOT NULL)

### dbo.UserTags
- `UserID` (bigint, NOT NULL)
- `Tag` (nvarchar(255), NOT NULL)
- `Weight` (int, NOT NULL)

### dbo.Videos
- `VideoID` (bigint, NOT NULL)
- `FilePath` (nvarchar(1024), NOT NULL)
- `FileHash` (nvarchar(64), NOT NULL)
- `Frame0AverageHash` (bigint, NOT NULL)
- `Frame0DifferenceHash` (bigint, NOT NULL)
- `Frame0PerceptualHash` (bigint, NOT NULL)
- `Frame50AverageHash` (bigint, NOT NULL)
- `Frame50DifferenceHash` (bigint, NOT NULL)
- `Frame50PerceptualHash` (bigint, NOT NULL)
- `Frame10AverageHash` (bigint, NOT NULL)
- `Frame10DifferenceHash` (bigint, NOT NULL)
- `Frame10PerceptualHash` (bigint, NOT NULL)
- `AddedOn` (datetime, NOT NULL)

### dbo.VideoTags
- `VideoID` (bigint, NOT NULL)
- `Tag` (nvarchar(255), NOT NULL)
- `Score` (float, NOT NULL)

## Development Guidelines
- Do **not** modify the database schema or attempt to create migrations.
- Always use parameterized ADO.NET queries and apply pagination with sensible limits (e.g., `OFFSET/FETCH`).
- Avoid loading large result sets; fetch data in small pages.
- Surface media files by combining the configured root directory with the stored `FilePath` values.
- Ensure UI components support zoom, fullscreen, slideshow for photos, and smooth playback for videos.
- Respect any more specific instructions provided in nested directories.

