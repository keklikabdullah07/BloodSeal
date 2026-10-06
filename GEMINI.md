# 🩸 BloodSeal: Antigravity & Agent Directive

This file configures Antigravity and AI agents working on **BloodSeal**.

## Architectural & Technical Foundations
- **Engine:** Godot 4.7.x Mono (C#)
- **Target Framework:** .NET 10.0 (`net10.0`, C# 12+)
- **Renderer:** Compatibility (GL Compatibility / OpenGL 3)
- **Resolution:** 1920x1080 (16:9 Landscape), `stretch/mode = "canvas_items"`, `stretch/aspect = "expand"`

## Core Principles
1. **"Call Down, Signal Up":** Parents call child methods down; children emit C# events (`event Action`) or Godot signals up. Never traverse upwards via `GetParent().GetParent()`.
2. **Composition Over Inheritance:** Scripts must remain under 250 lines. Decompose complex actors into reusable components.
3. **Safe Node References:** Use `[Export]` node references or `GetNodeOrNull<T>()` in `_Ready()`. Always check `IsInstanceValid()`.
4. **Skills in `.agents/skills/`:**
   - `bloodseal-balance`: GDC Anthony Pecorella math curves, wave scaling, Boss enrage, 6h offline progress.
   - `bloodseal-art-style`: Dark gothic palette, 3-layer parallax, combat juice, independent particle lifecycles.
   - `godot-csharp`: C# .NET lifecycle, partial classes, type safety.
   - `game-feel`: Trauma screenshake, hit-freeze, visual punch.
   - `camera-systems`: 2D camera smoothing, clamping, follow algorithms.
   - `godot-shaders`: CanvasItem 2D shaders for screen vignetting and gothic fog.
   - `godot-resources`: Custom data-driven resource configurations.
   - `godot-ui-control`: Responsive Control nodes and themes.
   - `save-systems`: Safe serialization, save/load state hygiene.

## Verification Gate
Before completing any task:
1. Run `dotnet build` from repository root (must be 0 warnings, 0 errors).
2. Run Godot console with `--headless --quit-after 60` smoke test.
