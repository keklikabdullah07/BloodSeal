---
name: bloodseal-art-style
description: "Dark gothic fantasy visual direction, color palette, parallax layer composition, particle rules, and combat VFX standards for BloodSeal."
---

# 🩸 BloodSeal Visual & Art Style Guide

This skill defines the artistic identity, color harmony, particle rules, and visual feedback guidelines for **BloodSeal**.

## 1. Color Palette (Harmonious Dark Gothic)

- **Blood Crimson:** `#D91A2A` / `Color(0.85, 0.1, 0.16)` — Core runes, blood slash arcs, critical alerts, moon halo.
- **Abyssal Void:** `#0A080F` / `Color(0.04, 0.03, 0.06)` — Background sky base, deep shadows.
- **Ruin Slate:** `#1A1624` / `Color(0.1, 0.08, 0.14)` — Spire silhouettes, ruined columns, castle ironwork.
- **Cobblestone Ground:** `#2A222B` / `Color(0.16, 0.13, 0.17)` — Walkway, foreground stones, graves.
- **Runic Amber Gold:** `#FFC845` / `Color(1.0, 0.78, 0.27)` — Gold currencies, rare loot, awakening seals.
- **Bone Pale:** `#E8E0D5` / `Color(0.91, 0.88, 0.83)` — Skull masks, hero skin highlights, damage text.

## 2. Layered Parallax Composition (16:9 1920x1080)

1. **Back Sky Layer (`motion_scale = (0.1, 0.1)`):** Deep black-purple gradient, large luminous Blood Moon (`Polygon2D` or sprite) with glowing outer corona.
2. **Mid Ruins Layer (`motion_scale = (0.4, 0.4)`):** Spire gothic silhouettes, iron fences, floating red ash and mist particles (`CPUParticles2D`).
3. **Foreground Layer (`motion_scale = (1.0, 1.0)`):** Walkable cobblestone path with red stain highlights, grave markers.

## 3. Combat Juice & VFX Rules

- **Slash Arc VFX:** Fast crescent light trail spawned on hero swing. Scaled +60% and shifted to flame-orange during Berserk mode.
- **Hit Flash:** Damaged enemies flash bright white-red for 0.06s before returning to base tint.
- **Blood Splatter:** Spawns 16 directional blood droplets with downward gravity (`gravity = Vector2(0, 400)`), short lifetime (0.35s).
- **Death Explosion:** Explodes into radial blood + black smoke particles. Spawns directly under `FXManager` so it finishes gracefully even when the enemy node is freed.
- **Tap Ripple:** Click/touch coordinates trigger an expanding shockwave ring and light camera shake.
- **Screen Shake:** Camera2D with trauma-decay formula:
  $$Offset = Random(-1, 1) \times 22 \times Trauma^2$$
  - Tap / Normal hit: $+0.10$ trauma
  - Critical / Berserk: $+0.35$ trauma
  - Player Damage: $+0.25$ trauma
- **Hit Freeze:** Critical strikes trigger an instant 45ms freeze frame (`Engine.TimeScale = 0.05`).
