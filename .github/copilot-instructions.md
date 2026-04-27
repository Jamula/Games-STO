# Copilot Instructions

## Project Overview

This is a .NET 10 monorepo for managing Star Trek Online (STO) accounts on Xbox. It includes an AI-powered assistant with three frontends (Discord bot, Blazor web app, Copilot Chat agent), backed by SQLite with EF Core. The STO Wiki (stowiki.net MediaWiki API) is the primary reference data source.

## Solution Structure

```
STO.slnx                       # Solution file
src/
  STO.Core/                    # Shared library — models, enums, service interfaces
    Models/                    # EF Core entity models (Account, Character, Item, Build, etc.)
    Enums/                     # Career, Faction, ItemType, ItemRarity, TraitType, etc.
    Services/                  # Service interfaces (IAccountService, ICharacterService, etc.)
  STO.Data/                    # EF Core + SQLite — DbContext, migrations, service implementations
    Context/StoDbContext.cs     # Database context with all DbSets
    Services/                  # Service implementations using EF Core
    Migrations/                # EF Core migrations
  STO.Wiki/                    # MediaWiki API client for stowiki.net
  STO.Discord/                 # Discord bot (DSharpPlus)
  STO.Web/                     # Blazor Server web app
  STO.CopilotAgent/            # GitHub Copilot SDK integration
Engineering/                   # Legacy markdown — Engineering career builds
Science/                       # Legacy markdown — Science career builds
Tactical/                      # Legacy markdown — Tactical career builds
```

## Key Patterns

- **EF Core + SQLite**: All data in a single `sto.db` file. Run migrations with `dotnet ef database update --project src/STO.Data`
- **Service layer**: Interfaces in `STO.Core/Services/`, implementations in `STO.Data/Services/`
- **Wiki sync**: `STO.Wiki` fetches reference data (items, traits, ships) from `stowiki.net/api.php`
- **DI registration**: Each project has a `DependencyInjection.cs` with `AddXxx()` extension methods

## Build & Run

```bash
dotnet build STO.slnx          # Build everything
dotnet ef database update --project src/STO.Data  # Apply migrations
```

## Conventions

- C# with nullable reference types enabled
- async/await throughout
- Entity models use data annotations + Fluent API configuration
- All timestamps in UTC
