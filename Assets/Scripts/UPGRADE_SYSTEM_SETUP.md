# Upgrade System Implementation Guide

## Step-by-Step Setup Instructions

### Part 1: Fix and Update Core Systems

#### Step 1.1: Update PlayerInventory to Support Upgrades

The PlayerInventory needs to check UpgradeSystem for capacity bonuses. We'll modify it to use the upgrade system.

#### Step 1.2: Update PlayerController to Support Upgrades

The PlayerController needs to apply movement speed upgrades properly.

#### Step 1.3: Ensure CoffeeMachine Uses Upgrades

CoffeeMachine already checks for upgrades, but we'll verify it's working correctly.

---

### Part 2: Create the Upgrade Shop UI

#### Step 2.1: Create UI Canvas Structure
1. In Unity, right-click in Hierarchy → UI → Canvas
2. Name it "UpgradeShopCanvas"
3. Set Canvas Scaler component:
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920x1080

#### Step 2.2: Create Shop Panel
1. Right-click UpgradeShopCanvas → UI → Panel
2. Name it "UpgradeShopPanel"
3. Set Anchor Presets: Middle-Center
4. Set RectTransform:
   - Width: 800
   - Height: 600
   - Position: (0, 0, 0)

#### Step 2.3: Create Shop Header
1. Right-click UpgradeShopPanel → UI → Text - TextMeshPro
2. Name it "ShopTitle"
3. Set Text: "UPGRADE SHOP"
4. Set Font Size: 48
5. Set Alignment: Center
6. Position: Top of panel

#### Step 2.4: Create Upgrade Item Prefab
1. Right-click UpgradeShopPanel → UI → Panel
2. Name it "UpgradeItemPrefab"
3. Set RectTransform:
   - Width: 750
   - Height: 120
   - Anchor: Top-Left

4. Add child TextMeshPro for upgrade name:
   - Name: "UpgradeName"
   - Font Size: 24
   - Position: Left side

5. Add child TextMeshPro for description:
   - Name: "UpgradeDescription"
   - Font Size: 18
   - Position: Below name

6. Add child TextMeshPro for cost:
   - Name: "UpgradeCost"
   - Font Size: 20
   - Text: "$XXX"
   - Position: Right side

7. Add child Button:
   - Name: "PurchaseButton"
   - Text: "BUY"
   - Position: Right side, below cost

8. Add child Image for "Purchased" indicator:
   - Name: "PurchasedBadge"
   - Set as inactive initially
   - Add TextMeshPro child: "SOLD OUT"

9. Make this a Prefab:
   - Drag UpgradeItemPrefab to Project window to create prefab
   - Delete from scene (we'll instantiate it)

#### Step 2.5: Create Scroll View for Upgrades
1. Right-click UpgradeShopPanel → UI → Scroll View
2. Name it "UpgradeScrollView"
3. Set RectTransform:
   - Anchor: Stretch-Stretch
   - Left/Right/Top/Bottom: 20
   - Position below title

4. Select Content GameObject inside ScrollView:
   - Add Vertical Layout Group component
   - Set Spacing: 10
   - Set Padding: 10
   - Set Child Force Expand: Width = true, Height = false

#### Step 2.6: Create Close Button
1. Right-click UpgradeShopPanel → UI → Button
2. Name it "CloseButton"
3. Set Text: "X" or "CLOSE"
4. Position: Top-Right corner
5. Set Size: 40x40

#### Step 2.7: Create Open Shop Button (in Main UI)
1. In your main UI Canvas, create a Button
2. Name it "OpenShopButton"
3. Set Text: "UPGRADES" or "SHOP"
4. Position: Wherever you want (e.g., top-right)

---

### Part 3: Create UpgradeShopUI Script

This script will handle displaying upgrades and handling purchases.

---

### Part 4: Connect Everything

1. Assign references in UpgradeShopUI
2. Connect buttons
3. Test the system

---

## Testing Checklist

- [ ] UpgradeSystem component exists in scene
- [ ] Shop UI opens/closes correctly
- [ ] Upgrades display correctly
- [ ] Can purchase upgrades when affordable
- [ ] Cannot purchase when not affordable
- [ ] Purchased upgrades show as "SOLD OUT"
- [ ] Machine speed upgrade works
- [ ] Inventory capacity upgrade works
- [ ] Movement speed upgrade works
- [ ] Upgrades persist (save/load)

