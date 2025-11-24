# Upgrade System - Values & Levels Reference Document

## Overview
This document outlines all upgrade values, levels, costs, and effects for the Coffee Shop game upgrade system.

---

## 📊 Upgrade Types Overview

| Upgrade Type | Effect | Base Value | Max Level | Stack Type |
|-------------|--------|------------|-----------|------------|
| Machine Speed | Reduces brewing time | 1.0x (100%) | 5 | Multiplicative |
| Inventory Capacity | Increases max beans | 3 beans | 3 | Additive |
| Movement Speed | Increases player speed | 1.0x (100%) | 5 | Multiplicative |
| Multiple Cups | Cups per brew | 1 cup | 3 | Set Value |
| Auto Collect | Auto-collects coffee | N/A | 1 | Boolean |
| Customer Patience | Increases patience time | 1.0x (100%) | 3 | Multiplicative |

---

## ⚙️ Machine Speed Upgrades

**Effect:** Reduces coffee brewing time (multiplies processing time)

| Level | Upgrade Name | Cost | Speed Multiplier | Brewing Time | Total Time Saved |
|-------|-------------|------|------------------|--------------|------------------|
| Base | - | - | 1.00x (100%) | 3.0s | 0s |
| 1 | Faster Brewing I | $50 | 0.75x (75%) | 2.25s | 0.75s |
| 2 | Faster Brewing II | $150 | 0.50x (50%) | 1.50s | 1.50s |
| 3 | Faster Brewing III | $300 | 0.35x (35%) | 1.05s | 1.95s |
| 4 | Faster Brewing IV | $500 | 0.25x (25%) | 0.75s | 2.25s |
| 5 | Ultra Fast Brewing | $1000 | 0.15x (15%) | 0.45s | 2.55s |

**Formula:** `actualTime = baseTime * multiplier`

**Example Calculation:**
- Base brewing time: 3.0 seconds
- Level 1: 3.0s × 0.75 = 2.25s
- Level 2: 3.0s × 0.50 = 1.50s (stacks multiplicatively)

**Note:** Upgrades stack multiplicatively. Level 1 + Level 2 = 0.75 × 0.50 = 0.375x (37.5% of original time)

---

## 🎒 Inventory Capacity Upgrades

**Effect:** Increases maximum beans player can carry

| Level | Upgrade Name | Cost | Capacity Bonus | New Max Beans | Total Capacity |
|-------|-------------|------|----------------|---------------|----------------|
| Base | - | - | +0 | 3 | 3 |
| 1 | Bigger Pockets I | $30 | +2 | 5 | 5 |
| 2 | Bigger Pockets II | $100 | +3 | 8 | 8 |
| 3 | Bigger Pockets III | $250 | +5 | 13 | 13 |

**Formula:** `maxCapacity = baseCapacity + sumOfBonuses`

**Example Calculation:**
- Base capacity: 3 beans
- Level 1: 3 + 2 = 5 beans
- Level 2: 3 + 2 + 3 = 8 beans (stacks additively)

**Coffee Cup Capacity:**
- Base: 5 cups
- Upgrade options (future):
  - Level 1: +3 cups ($40)
  - Level 2: +5 cups ($100)
  - Level 3: +7 cups ($200)

---

## 🏃 Movement Speed Upgrades

**Effect:** Increases player movement speed

| Level | Upgrade Name | Cost | Speed Multiplier | New Speed | Total Speed |
|-------|-------------|------|------------------|-----------|-------------|
| Base | - | - | 1.00x (100%) | 5.0 units/s | 5.0 |
| 1 | Quick Feet I | $40 | 1.20x (120%) | 6.0 units/s | 6.0 |
| 2 | Quick Feet II | $120 | 1.50x (150%) | 7.5 units/s | 7.5 |
| 3 | Quick Feet III | $250 | 1.80x (180%) | 9.0 units/s | 9.0 |
| 4 | Quick Feet IV | $500 | 2.20x (220%) | 11.0 units/s | 11.0 |
| 5 | Lightning Speed | $1000 | 2.50x (250%) | 12.5 units/s | 12.5 |

**Formula:** `actualSpeed = baseSpeed * multiplier`

**Example Calculation:**
- Base speed: 5.0 units/second
- Level 1: 5.0 × 1.20 = 6.0 units/s
- Level 2: 5.0 × 1.50 = 7.5 units/s (replaces previous multiplier)

**Note:** Movement speed upgrades replace the multiplier, they don't stack. Each level sets a new multiplier.

---

## ☕ Multiple Cups Per Brew Upgrades

**Effect:** Brews multiple coffee cups in one batch

| Level | Upgrade Name | Cost | Cups Per Brew | Beans Required | Total Cups |
|-------|-------------|------|---------------|----------------|------------|
| Base | - | - | 1 | 3 beans | 1 cup |
| 1 | Double Brew | $100 | 2 | 3 beans | 2 cups |
| 2 | Triple Brew | $300 | 3 | 3 beans | 3 cups |
| 3 | Quad Brew | $600 | 4 | 3 beans | 4 cups |

**Formula:** `cupsSpawned = cupsPerBrew`

**Example:**
- Base: 3 beans → 1 cup
- Level 1: 3 beans → 2 cups (same beans, double output!)
- Level 2: 3 beans → 3 cups (triple output!)

**Note:** This is a set value upgrade - each level replaces the previous value.

**Advanced Option:** Could require more beans for higher levels:
- Double Brew: 3 beans → 2 cups
- Triple Brew: 5 beans → 3 cups
- Quad Brew: 7 beans → 4 cups

---

## 🤖 Auto Collect Upgrade

**Effect:** Automatically collects coffee when ready (no manual interaction needed)

| Level | Upgrade Name | Cost | Effect |
|-------|-------------|------|--------|
| Base | - | - | Manual collection required |
| 1 | Auto Collector | $200 | Auto-collects when inventory has space |

**Implementation:**
- When coffee is ready, automatically adds to inventory if space available
- If inventory full, waits until space is available
- No interaction needed

**Future Enhancements:**
- Auto Collector Pro ($500): Auto-collects even if inventory full (stores in buffer)
- Smart Collector ($800): Auto-collects and auto-serves customers

---

## 😊 Customer Patience Upgrades

**Effect:** Increases customer patience time (customers wait longer)

| Level | Upgrade Name | Cost | Patience Multiplier | Base Patience | New Patience |
|-------|-------------|------|---------------------|---------------|--------------|
| Base | - | - | 1.00x (100%) | 30s | 30s |
| 1 | Patient Customers I | $80 | 1.30x (130%) | 30s | 39s |
| 2 | Patient Customers II | $200 | 1.60x (160%) | 30s | 48s |
| 3 | Very Patient Customers | $400 | 2.00x (200%) | 30s | 60s |

**Formula:** `actualPatience = basePatience * multiplier`

**Example:**
- Base patience: 30 seconds
- Level 1: 30s × 1.30 = 39 seconds
- Level 2: 30s × 1.60 = 48 seconds (replaces previous)

**Note:** Multipliers replace each other, don't stack.

---

## 💰 Cost Progression Formulas

### Linear Progression
**Formula:** `cost = baseCost + (level - 1) * increment`

**Example:** Machine Speed
- Level 1: $50
- Level 2: $50 + $100 = $150
- Level 3: $50 + $250 = $300

### Exponential Progression
**Formula:** `cost = baseCost * (multiplier ^ (level - 1))`

**Example:** Movement Speed (1.5x multiplier)
- Level 1: $40
- Level 2: $40 × 1.5² = $90 (rounded to $120)
- Level 3: $40 × 1.5³ = $135 (rounded to $250)

### Recommended Cost Progression

| Upgrade Type | Base Cost | Progression Type | Level 2 | Level 3 | Level 4 | Level 5 |
|-------------|-----------|------------------|---------|---------|---------|---------|
| Machine Speed | $50 | Exponential (2x) | $150 | $300 | $500 | $1000 |
| Inventory | $30 | Linear (+$70) | $100 | $250 | - | - |
| Movement Speed | $40 | Exponential (2x) | $120 | $250 | $500 | $1000 |
| Multiple Cups | $100 | Exponential (2x) | $300 | $600 | - | - |
| Auto Collect | $200 | Single | - | - | - | - |
| Customer Patience | $80 | Exponential (1.5x) | $200 | $400 | - | - |

---

## 📈 Upgrade Value Recommendations

### Early Game (Level 1 Upgrades)
**Recommended Order:**
1. **Bigger Pockets I** ($30) - Most cost-effective, immediate impact
2. **Quick Feet I** ($40) - Helps efficiency
3. **Faster Brewing I** ($50) - Reduces wait time

**Total Cost:** $120

### Mid Game (Level 2 Upgrades)
**Recommended Order:**
1. **Double Brew** ($100) - Doubles output efficiency
2. **Bigger Pockets II** ($100) - More capacity
3. **Faster Brewing II** ($150) - Even faster brewing
4. **Patient Customers I** ($80) - Reduces pressure

**Total Cost:** $430

### Late Game (Level 3+ Upgrades)
**Focus on:**
- **Triple/Quad Brew** - Maximum efficiency
- **Ultra Fast Brewing** - Minimal wait times
- **Lightning Speed** - Maximum mobility

---

## 🎮 Balance Considerations

### Power Level Comparison

| Upgrade | Cost | Impact Score (1-10) | Cost Efficiency |
|---------|------|---------------------|-----------------|
| Bigger Pockets I | $30 | 7 | ⭐⭐⭐⭐⭐ |
| Quick Feet I | $40 | 6 | ⭐⭐⭐⭐ |
| Faster Brewing I | $50 | 8 | ⭐⭐⭐⭐⭐ |
| Double Brew | $100 | 9 | ⭐⭐⭐⭐⭐ |
| Auto Collector | $200 | 7 | ⭐⭐⭐ |
| Patient Customers I | $80 | 5 | ⭐⭐⭐ |

**Impact Score:** How much the upgrade improves gameplay
**Cost Efficiency:** Value per dollar spent

### Recommended Balance Adjustments

**If upgrades feel too cheap:**
- Increase all costs by 1.5x
- Machine Speed: $75, $225, $450, $750, $1500
- Inventory: $45, $150, $375

**If upgrades feel too expensive:**
- Decrease all costs by 0.75x
- Machine Speed: $38, $113, $225, $375, $750
- Inventory: $23, $75, $188

**If progression feels too fast:**
- Increase cost multipliers
- Add more levels between upgrades
- Reduce effect values slightly

**If progression feels too slow:**
- Decrease cost multipliers
- Increase effect values
- Add cheaper intermediate upgrades

---

## 🔄 Upgrade Stacking Rules

### Multiplicative Stacking
- **Machine Speed:** Each upgrade multiplies the time reduction
  - Level 1: 0.75x
  - Level 2: 0.75x × 0.50x = 0.375x (stacks)
  
### Additive Stacking
- **Inventory Capacity:** Each upgrade adds to the total
  - Level 1: +2 beans
  - Level 2: +3 beans
  - Total: +5 beans (stacks)

### Replacement Stacking
- **Movement Speed:** Each upgrade replaces the multiplier
  - Level 1: 1.20x
  - Level 2: 1.50x (replaces, doesn't stack)
  
### Set Value
- **Multiple Cups:** Each upgrade sets a new value
  - Level 1: 2 cups
  - Level 2: 3 cups (replaces)

---

## 📝 Implementation Notes

### Current Default Upgrades (in code)
```csharp
// Faster Brewing
cost: $50
value: 0.75f (75% speed multiplier)

// Bigger Pockets
cost: $30
value: 2f (+2 beans)

// Quick Feet
cost: $40
value: 1.2f (120% speed multiplier)

// Double Brew
cost: $100
value: 2f (2 cups per brew)
```

### Adding More Upgrade Levels

To add Level 2 upgrades, modify `UpgradeSystem.cs`:

```csharp
availableUpgrades.Add(new Upgrade
{
    upgradeName = "Faster Brewing II",
    description = "Reduce brewing time by 50%",
    cost = 150,
    type = Upgrade.UpgradeType.MachineSpeed,
    value = 0.50f
});
```

### Upgrade Unlock System (Future)

You could add unlock requirements:
- Level 2 upgrades unlock after serving 10 customers
- Level 3 upgrades unlock after serving 50 customers
- Level 4 upgrades unlock after serving 100 customers

---

## 🎯 Recommended Upgrade Paths

### Path 1: Efficiency Focus
1. Bigger Pockets I ($30)
2. Faster Brewing I ($50)
3. Double Brew ($100)
4. Faster Brewing II ($150)
5. Triple Brew ($300)

**Total:** $630
**Focus:** Maximum output per time

### Path 2: Speed Focus
1. Quick Feet I ($40)
2. Faster Brewing I ($50)
3. Quick Feet II ($120)
4. Faster Brewing II ($150)
5. Auto Collector ($200)

**Total:** $560
**Focus:** Fastest gameplay

### Path 3: Balanced
1. Bigger Pockets I ($30)
2. Quick Feet I ($40)
3. Faster Brewing I ($50)
4. Patient Customers I ($80)
5. Double Brew ($100)

**Total:** $300
**Focus:** Well-rounded improvements

---

## 📊 Upgrade Value Tables (Quick Reference)

### Machine Speed
| Level | Cost | Multiplier | Time (3s base) |
|-------|------|------------|----------------|
| 1 | $50 | 0.75x | 2.25s |
| 2 | $150 | 0.50x | 1.50s |
| 3 | $300 | 0.35x | 1.05s |
| 4 | $500 | 0.25x | 0.75s |
| 5 | $1000 | 0.15x | 0.45s |

### Inventory Capacity
| Level | Cost | Bonus | Total Capacity |
|-------|------|-------|----------------|
| 1 | $30 | +2 | 5 beans |
| 2 | $100 | +3 | 8 beans |
| 3 | $250 | +5 | 13 beans |

### Movement Speed
| Level | Cost | Multiplier | Speed (5 base) |
|-------|------|------------|-----------------|
| 1 | $40 | 1.20x | 6.0 |
| 2 | $120 | 1.50x | 7.5 |
| 3 | $250 | 1.80x | 9.0 |
| 4 | $500 | 2.20x | 11.0 |
| 5 | $1000 | 2.50x | 12.5 |

### Multiple Cups
| Level | Cost | Cups | Efficiency |
|-------|------|------|-------------|
| 1 | $100 | 2 | 2x |
| 2 | $300 | 3 | 3x |
| 3 | $600 | 4 | 4x |

---

## 🔧 Configuration Values

### Base Values (Before Upgrades)
```csharp
// CoffeeMachine
processingTime = 3.0f seconds
beansRequired = 3

// PlayerInventory
maxBeans = 3
maxCoffees = 5

// PlayerController
moveSpeed = 5.0f units/second

// Customer
patienceTime = 30.0f seconds
```

### Upgrade System Defaults
```csharp
machineSpeedMultiplier = 1.0f (base)
inventoryCapacityBonus = 0 (base)
movementSpeedMultiplier = 1.0f (base)
cupsPerBrew = 1 (base)
```

---

## 📌 Notes for Game Designers

1. **Cost Scaling:** Use exponential scaling for high-impact upgrades
2. **Early Game:** Keep first upgrades cheap ($30-$50) for quick wins
3. **Mid Game:** $100-$300 range feels rewarding
4. **Late Game:** $500+ for significant power spikes
5. **Balance Testing:** Test with different upgrade orders
6. **Player Feedback:** Consider visual/audio feedback for each upgrade
7. **Progression Gates:** Consider requiring certain achievements to unlock higher tiers

---

## 🎨 Visual/UI Recommendations

### Upgrade Display
- Show current level vs max level (e.g., "Level 2/5")
- Display effect value clearly (e.g., "75% faster")
- Show cost vs current money
- Highlight affordable upgrades
- Show "NEW" badge for newly available upgrades

### Purchase Feedback
- Particle effects on purchase
- Sound effect (coin/purchase sound)
- Brief animation on upgrade icon
- Show "UPGRADED!" text briefly

---

## 📚 Future Expansion Ideas

### New Upgrade Types
1. **Bean Efficiency** - Use fewer beans per coffee
2. **Tip Multiplier** - Customers tip more
3. **Customer Spawn Rate** - More customers appear
4. **Coffee Quality** - Higher quality = more money
5. **Storage Expansion** - More coffee cup capacity
6. **Auto Bean Collection** - Auto-collect beans
7. **Multiple Machines** - Unlock additional machines
8. **Speed Boost** - Temporary speed boost ability

### Upgrade Tiers
- **Common** (White) - Basic upgrades
- **Rare** (Blue) - Better upgrades
- **Epic** (Purple) - Powerful upgrades
- **Legendary** (Gold) - Game-changing upgrades

---

**Last Updated:** [Current Date]
**Version:** 1.0
**Author:** Game Design Team

