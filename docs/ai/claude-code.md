# Claude Code in MoreSpeakers: Skills & Commands Guide

This repository uses a hybrid system of **Skills** and **Slash Commands** in **Claude Code** to help you develop faster and more consistently. This guide explains what they are and when to use them.

## The Hybrid Approach

-   **`CLAUDE.md`**: Contains the most critical, high-level project constraints. It's always loaded, so it's kept short.
-   **Skills (`.claude/skills/`)**: Detailed Standard Operating Procedures (SOPs) for specialized tasks. Claude Code automatically detects when a skill is relevant based on your request and its description.
-   **Slash Commands (`.claude/commands/`)**: Explicit, manual triggers for common developer workflows. They provide a predictable “golden path” and often nudge Claude Code to apply the relevant Skill.

## Available Skills

### 1. .NET Feature Architect (`dotnet-feature`)
-   **Description**: Implements vertical slice features in the .NET 10 solution, covering Domain, Data, Managers, and Razor Pages/HTMX layers.
-   **When it's used**: Claude will automatically use this skill when asked to build new features, add endpoints, or create UI components, as its description matches these tasks.

### 2. SQL Schema Ops (`sql-schema`)
-   **Description**: Manages SQL Server schema changes via raw SQL scripts for .NET Aspire orchestration.
-   **When it's used**: Activates when you ask to modify the database. It enforces the critical project constraint of **NO EF MIGRATIONS** and guides the process of editing SQL scripts in `scripts/database/` and updating `AppHost.cs`.

### 3. QA Engineer (`qa-engineer`)
-   **Description**: Generates xUnit tests using FluentAssertions, Moq, and Bogus.
-   **When it's used**: Triggers when you ask for unit tests. It ensures all new tests conform to the project's standards, including the `Method_Scenario_ExpectedResult` naming convention.

## Available Slash Commands

Use these commands for a predictable, repeatable workflow.

-   ` /feature <name>`
    -   **Purpose**: Kicks off the creation of a new vertical slice feature.
    -   **Example**: `/feature "Add speaker profile picture upload"`

-   `/db-change <description>`
    -   **Purpose**: A guided process for making a database schema change.
    -   **Example**: `/db-change "Add a 'ProfilePictureUrl' column to the AspNetUsers table"`

-   `/test <target>`
    -   **Purpose**: Generate unit tests for a specific class or feature.
    -   **Example**: `/test "the new ProfilePictureManager class"`

-   `/docs <topic>`
    -   **Purpose**: Create or update project documentation.
    -   **Example**: `/docs "Update the README with new setup instructions"`
