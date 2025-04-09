# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build/Test Commands
- Open project in Unity Editor: `unity -projectPath /path/to/super-mario-bros`
- Run game in Unity Editor: Press Play button in Unity Editor
- Build game: Use Unity Editor's "Build" menu
- Run tests: Use Unity Test Runner window from "Window > General > Test Runner"

## Code Style Guidelines
- **Naming Conventions**:
  - Use PascalCase for public classes, methods, and properties (e.g., `PlayerMovement`, `OnGroundCheck()`)
  - Use camelCase with underscore prefix for private fields (e.g., `_marioBody`, `_faceRightState`)
  - Use UPPERCASE for constants (e.g., `MaxOffset`, `EnemyPatrolTime`)
- **Class Organization**:
  - Public fields at top of class
  - Private fields next
  - Unity lifecycle methods (Start, Update, FixedUpdate) first
  - Custom methods after lifecycle methods
- **Documentation**:
  - Use XML comments for public methods and classes
  - Use inline comments for implementation details
- **Error Handling**: Use Debug.Log for error reporting and diagnostics