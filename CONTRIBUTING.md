# Contributing to Dune: Spice & Sand

We welcome contributions that maintain 100% fidelity to Frank Herbert's original Dune novels.

## Lore Fidelity Requirements

1. **No AI/computers** - References to thinking machines are strictly forbidden
2. **Book-accurate terminology** - Use terms exactly as Herbert wrote them
3. **Citation required** - Every mechanic must reference specific book chapters
4. **No movie-only elements** - David Lynch's or Villeneuve's interpretations are not canon

## Development Setup

1. Install Unity 2022.3 LTS
2. Clone repository
3. Open project in Unity
4. Create feature branch: `git checkout -b feature/your-feature-name`

## Pull Request Process

1. Update README.md with details of changes if applicable
2. Add XML comments citing book references
3. Ensure all scripts compile without warnings
4. Test on Android device or emulator
5. Submit PR with clear description of changes

## Code Style

- Use PascalCase for classes, methods, properties
- Use camelCase for variables
- Add XML comments with book citations
- Follow Unity C# conventions

## Testing

- Test on min target: Android 8.0 (API 26)
- Test on mid-range device (e.g., Pixel 4a)
- Maintain 60 FPS during gameplay

## Book References Format

```csharp
/// <summary>
/// [Mechanic description]
/// Reference: Dune, Book I, Chapter 3, "The Delve"
/// </summary>
