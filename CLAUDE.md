# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a **Visual Studio Extension (VSIX)** that adds SQL IntelliSense/autocomplete to **SQL Server Management Studio (SSMS) v22**. It's a proof-of-concept written in C# targeting .NET Framework 4.8.1.

## Build & Run

Open `SsmsAutocompletion.slnx` in Visual Studio 2022. Build with standard VS build commands (`Ctrl+Shift+B`).

**Debug launch**: Configured to start SSMS with an isolated experimental instance (`/rootsuffix Exp`). Post-build copies the VSIX to:
```
C:\Program Files\Microsoft SQL Server Management Studio 22\Release\Common7\IDE\Extensions\AutoCompletion
```

There is no CLI build/test/lint toolchain — this project is developed entirely through Visual Studio.

## Architecture

The extension uses MEF (Managed Extensibility Framework) and the Visual Studio Editor SDK to hook into SSMS's SQL editor.

**Data flow when the user types:**
```
Keystroke → SqlCommandFilter.Exec()
  → triggers SqlCompletionSource.AugmentCompletionSession()
  → returns keyword list
  → RETURN/TAB commits the selected suggestion
```

**Key classes:**

- `SsmsAutocompletionPackage` — `AsyncPackage` entry point; auto-loads in both solution and no-solution contexts.
- `SqlCommandFilter` (`IOleCommandTarget`) — intercepts keystrokes; triggers autocomplete on letter/digit/dot, commits on RETURN/TAB, dismisses on other keys.
- `SqlCompletionSource` / `SqlCompletionSourceProvider` — MEF-exported `ICompletionSource`; currently returns a hard-coded list (SELECT, FROM, WHERE).
- `VsTextViewCreationSqlListener` / `SqlCompletionController` — hooks text view creation to attach the command filter.
- `Test` class — stub helpers for SMO-based database metadata (uses reflection to pull connection info from SSMS UI); partially implemented.

**Content type**: `"SQL"` — the extension only activates on SQL editor windows.

**Current limitations**: Suggestions are hard-coded keywords; the `Test` class contains the scaffolding for real database-driven completions (tables, columns) but it is not yet wired up.

## Development Rules

**Always use the SSMS SQL parser for context detection — never regex.**

The SSMS parser (`Microsoft.SqlServer.Management.SqlParser`) provides a full token manager via `ParseResult.Script.TokenManager`. Use it for all SQL context detection:

- `tokenManager.FindToken(line, column)` — find the token at the cursor position
- `tokenManager.GetPreviousSignificantTokenIndex(index)` — walk backwards through significant tokens
- `tokenManager.GetText(index)` — read token text

This correctly handles partial identifiers being typed, multi-line queries, comments, and string literals. Regex over raw SQL text is fragile by comparison and must not be used as a substitute.

The existing `IContextDetector.IsAfterKeyword(parseResult, line, column, keyword)` is the reference implementation of this pattern.
