---
name: bloodseal-balance
description: "Mathematical progression formulas, wave scaling, Boss enrage, offline progress, and economy balance rules for BloodSeal."
---

# 🩸 BloodSeal Game Balance & Mathematical Economy Standards

This skill defines the official mathematical curves, balancing standards, and economy hygiene for **BloodSeal**, based on Anthony Pecorella's GDC framework for Idle / Incremental RPGs.

## 1. Upgrade Cost Scaling (Exponential)

Standard stats (ATK, Max HP, Range):
$$Cost = BaseCost \times 1.15^{(Level - 1)}$$

High-impact stats (ATK Speed, Lifesteal):
$$Cost = BaseCost \times GrowthFactor^{(Level - 1)}$$
- **ATK Speed:** $GrowthFactor = 1.18$ (Cap: 3.5 attacks/sec)
- **Lifesteal:** $GrowthFactor = 1.22$ (Cap: 25.0%)

## 2. Enemy Wave Scaling (Linear-Polynomial)

- **Minion HP:** $Wave \times 25 + 50$
- **Minion ATK:** $Wave \times 3 + 5$
- **Minion Gold Reward:** $Wave \times 5 + 10$
- **Boss HP (Every 10 waves):** $Wave \times 220 + 450$
- **Boss ATK:** $Wave \times 14 + 25$
- **Boss Gold Reward:** $Wave \times 60 + 250$

### Boss Enrage Curve
- Boss has no hard countdown kill timer.
- Every 5 seconds elapsed in the boss fight:
  $$BossDamage = BaseBossDamage \times (1.0 + 0.25 \times EnrageCycles)$$
  Aura shifts progressively to darker blood crimson.

## 3. Safe Farm Defeat Loop
- If the hero dies on Boss Wave $N$, hero retreats immediately to Wave $N - 1$.
- Wave $N - 1$ operates as infinite safe farm mode until the player triggers "Retry Boss".

## 4. Offline Progress Math (Hard-Capped)
- Timestamps stored in `DateTimeOffset.UtcNow.ToUnixTimeSeconds()`.
- Maximum offline duration capped at **6 Hours** (21,600 seconds).
- Formula:
  $$OfflineGold = ClearedWaveGoldPerSec \times \min(SecondsOffline, 21600)$$

## 5. Big Number Hygiene
- All damage and gold variables must use `long` or `double` to prevent 32-bit integer overflow.
- Standard representation:
  - `< 1,000`: Exact
  - `>= 1,000`: `1.2K`
  - `>= 1,000,000`: `4.5M`
  - `>= 1,000,000,000`: `12.8B`
  - `>= 1,000,000,000,000`: `3.1T`
