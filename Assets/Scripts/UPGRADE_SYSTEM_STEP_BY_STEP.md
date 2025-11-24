# Upgrade System - Complete Step-by-Step Implementation Guide

## ✅ Part 1: Core System Setup (Already Done!)

The core systems have been updated:
- ✅ `UpgradeSystem.cs` - Created
- ✅ `PlayerInventory.cs` - Updated to use upgrade bonuses
- ✅ `PlayerController.cs` - Updated to apply movement speed upgrades
- ✅ `CoffeeMachine.cs` - Already checks for upgrade system
- ✅ `UpgradeShopUI.cs` - Created

---

## 📋 Part 2: Unity Scene Setup

### Step 1: Add UpgradeSystem to Scene

1. **In Unity Hierarchy:**
   - Right-click → Create Empty
   - Name it: `UpgradeSystem`
   - Position: Doesn't matter (it's a manager)

2. **Add Component:**
   - Select `UpgradeSystem` GameObject
   - Add Component → `UpgradeSystem` script
   - The system will auto-initialize with default upgrades

3. **Verify:**
   - Check Inspector - you should see 4 default upgrades listed
   - No errors in Console

---

### Step 2: Create Upgrade Shop UI Canvas

1. **Create Canvas:**
   - Right-click in Hierarchy → UI → Canvas
   - Name it: `UpgradeShopCanvas`
   - Set Canvas Scaler:
     - UI Scale Mode: `Scale With Screen Size`
     - Reference Resolution: `1920 x 1080`
     - Match: `0.5` (or your preference)

2. **Create Shop Panel:**
   - Right-click `UpgradeShopCanvas` → UI → Panel
   - Name it: `UpgradeShopPanel`
   - Set RectTransform:
     - Anchor Presets: **Middle-Center** (hold Alt+Shift)
     - Width: `800`
     - Height: `600`
     - Position: `(0, 0, 0)`
   - Set Image Color: Dark semi-transparent (e.g., RGBA: 0, 0, 0, 200)

---

### Step 3: Create Shop Header

1. **Title Text:**
   - Right-click `UpgradeShopPanel` → UI → Text - TextMeshPro
   - Name it: `ShopTitle`
   - Set RectTransform:
     - Anchor: Top-Stretch
     - Top: `-20`
     - Left/Right: `20`
     - Height: `60`
   - Set TextMeshPro:
     - Text: `UPGRADE SHOP`
     - Font Size: `48`
     - Alignment: **Center**
     - Color: White or Gold

2. **Money Display:**
   - Right-click `UpgradeShopPanel` → UI → Text - TextMeshPro
   - Name it: `MoneyDisplay`
   - Set RectTransform:
     - Anchor: Top-Right
     - Top: `-20`
     - Right: `-20`
     - Width: `200`
     - Height: `40`
   - Set TextMeshPro:
     - Text: `Money: $0`
     - Font Size: `24`
     - Alignment: **Right**

---

### Step 4: Create Scroll View for Upgrades

1. **Create Scroll View:**
   - Right-click `UpgradeShopPanel` → UI → Scroll View
   - Name it: `UpgradeScrollView`
   - Set RectTransform:
     - Anchor: Stretch-Stretch
     - Left: `20`
     - Right: `20`
     - Top: `80` (below title)
     - Bottom: `60` (above close button)

2. **Configure Scroll View:**
   - Select `UpgradeScrollView`:
     - Horizontal: **Unchecked**
     - Vertical: **Checked**
     - Movement Type: `Elastic`
     - Scroll Sensitivity: `20`

3. **Configure Content:**
   - Select `Content` (child of ScrollView):
     - Add Component → **Vertical Layout Group**
     - Set Vertical Layout Group:
       - Spacing: `15`
       - Padding: `Left: 10, Right: 10, Top: 10, Bottom: 10`
       - Child Force Expand: **Width: ✓, Height: ✗**
       - Child Control Size: **Width: ✓, Height: ✓**
     - Set RectTransform:
       - Width: `760` (or match parent minus padding)

---

### Step 5: Create Upgrade Item Prefab

1. **Create Upgrade Item Panel:**
   - Right-click `Content` (inside ScrollView) → UI → Panel
   - Name it: `UpgradeItemPrefab`
   - Set RectTransform:
     - Width: `740`
     - Height: `120`
   - Set Image Color: Slightly lighter than shop panel (e.g., RGBA: 30, 30, 30, 255)

2. **Add Upgrade Name:**
   - Right-click `UpgradeItemPrefab` → UI → Text - TextMeshPro
   - Name it: `UpgradeName`
   - Set RectTransform:
     - Anchor: Top-Left
     - Left: `15`
     - Top: `-15`
     - Width: `400`
     - Height: `35`
   - Set TextMeshPro:
     - Text: `Upgrade Name`
     - Font Size: `28`
     - Font Style: **Bold**
     - Alignment: **Left**

3. **Add Description:**
   - Right-click `UpgradeItemPrefab` → UI → Text - TextMeshPro
   - Name it: `UpgradeDescription`
   - Set RectTransform:
     - Anchor: Top-Left
     - Left: `15`
     - Top: `-50`
     - Width: `400`
     - Height: `50`
   - Set TextMeshPro:
     - Text: `Upgrade description here`
     - Font Size: `18`
     - Alignment: **Left**
     - Color: Light gray

4. **Add Cost Text:**
   - Right-click `UpgradeItemPrefab` → UI → Text - TextMeshPro
   - Name it: `UpgradeCost`
   - Set RectTransform:
     - Anchor: Top-Right
     - Right: `-15`
     - Top: `-15`
     - Width: `150`
     - Height: `30`
   - Set TextMeshPro:
     - Text: `$100`
     - Font Size: `24`
     - Alignment: **Right**
     - Font Style: **Bold**

5. **Add Purchase Button:**
   - Right-click `UpgradeItemPrefab` → UI → Button
   - Name it: `PurchaseButton`
   - Set RectTransform:
     - Anchor: Bottom-Right
     - Right: `-15`
     - Bottom: `15`
     - Width: `150`
     - Height: `40`
   - Set Button:
     - Remove Text child (we'll add custom text)
   - Add TextMeshPro child:
     - Right-click `PurchaseButton` → UI → Text - TextMeshPro
     - Name it: `ButtonText`
     - Set Text: `BUY`
     - Font Size: `20`
     - Alignment: **Center**
     - Font Style: **Bold**

6. **Add Purchased Badge:**
   - Right-click `UpgradeItemPrefab` → UI → Image
   - Name it: `PurchasedBadge`
   - Set RectTransform:
     - Anchor: Top-Right
     - Right: `-15`
     - Top: `-15`
     - Width: `150`
     - Height: `40`
   - Set Image:
     - Color: Green (RGBA: 0, 200, 0, 200)
   - Add TextMeshPro child:
     - Right-click `PurchasedBadge` → UI → Text - TextMeshPro
     - Set Text: `PURCHASED`
     - Font Size: `20`
     - Alignment: **Center**
     - Font Style: **Bold**
   - **Set PurchasedBadge as INACTIVE** (uncheck checkbox in Inspector)

7. **Make Prefab:**
   - Drag `UpgradeItemPrefab` from Hierarchy to Project window (create a Prefabs folder if needed)
   - Delete `UpgradeItemPrefab` from Hierarchy (we'll instantiate it via script)

---

### Step 6: Create Close Button

1. **Add Close Button:**
   - Right-click `UpgradeShopPanel` → UI → Button
   - Name it: `CloseButton`
   - Set RectTransform:
     - Anchor: Bottom-Center
     - Bottom: `-10`
     - Width: `200`
     - Height: `50`
   - Set Button Text: `CLOSE` or `X`
   - Font Size: `24`

---

### Step 7: Create Open Shop Button (Main UI)

1. **In your main game UI Canvas:**
   - Right-click your main UI Canvas → UI → Button
   - Name it: `OpenShopButton`
   - Set RectTransform:
     - Position wherever you want (e.g., top-right corner)
     - Width: `150`
     - Height: `50`
   - Set Button Text: `UPGRADES` or `SHOP`
   - Font Size: `20`

---

## 🔧 Part 3: Connect Scripts

### Step 8: Add UpgradeShopUI Component

1. **Add Component:**
   - Select `UpgradeShopCanvas` (or `UpgradeShopPanel`)
   - Add Component → `UpgradeShopUI` script

2. **Assign References in Inspector:**
   - **Shop Panel**: Drag `UpgradeShopPanel` here
   - **Upgrade Item Prefab**: Drag the prefab you created
   - **Upgrade Container**: Drag `Content` (from ScrollView) here
   - **Close Button**: Drag `CloseButton` here
   - **Money Display**: Drag `MoneyDisplay` here
   - **Purchase Sound**: (Optional) Add AudioClip
   - **Error Sound**: (Optional) Add AudioClip

### Step 9: Connect Open Shop Button

1. **Select `OpenShopButton`** (in main UI)
2. **In Button component:**
   - Click `+` in OnClick()
   - Drag `UpgradeShopCanvas` (or GameObject with UpgradeShopUI) to object field
   - Select: `UpgradeShopUI` → `ToggleShop()` or `OpenShop()`

---

## 🎮 Part 4: Testing

### Step 10: Test the System

1. **Play the game**
2. **Earn some money** by serving customers
3. **Click the "UPGRADES" button** - Shop should open
4. **Check upgrades display:**
   - Should see 4 upgrades listed
   - Costs should display correctly
   - Money display should show current money
5. **Try purchasing:**
   - If you have enough money: Button should be enabled, click to buy
   - If not enough: Button should be grayed out
   - After purchase: Should show "PURCHASED" badge
6. **Test upgrades work:**
   - **Machine Speed**: Brew coffee - should be faster
   - **Inventory Capacity**: Try collecting beans - should hold more
   - **Movement Speed**: Move around - should be faster
7. **Close shop** - Click close button

---

## 🐛 Troubleshooting

### Shop doesn't open:
- Check `UpgradeShopUI` component is added
- Check `shopPanel` reference is assigned
- Check button OnClick() is connected

### Upgrades don't display:
- Check `upgradeItemPrefab` reference is assigned
- Check `upgradeContainer` reference is assigned (should be Content)
- Check UpgradeSystem exists in scene
- Check Console for errors

### Can't purchase upgrades:
- Check you have enough money
- Check GameManager.Instance exists
- Check UpgradeSystem.Instance exists
- Check Console for errors

### Upgrades don't work:
- **Machine Speed**: Check CoffeeMachine uses UpgradeSystem (already done)
- **Inventory**: Check PlayerInventory uses UpgradeSystem (already done)
- **Movement**: Check PlayerController uses UpgradeSystem (already done)

### UI looks wrong:
- Check Canvas Scaler settings
- Check RectTransform anchors
- Check font sizes aren't too large
- Try adjusting panel sizes

---

## ✨ Optional Enhancements

### Add Save/Load for Upgrades:
The UpgradeSystem doesn't save purchases yet. You can add:
```csharp
// In UpgradeSystem.cs, add to Awake():
LoadUpgrades();

// Add methods:
private void SaveUpgrades()
{
    foreach (var upgrade in availableUpgrades)
    {
        PlayerPrefs.SetInt($"Upgrade_{upgrade.upgradeName}_Purchased", upgrade.isPurchased ? 1 : 0);
    }
    PlayerPrefs.Save();
}

private void LoadUpgrades()
{
    foreach (var upgrade in availableUpgrades)
    {
        upgrade.isPurchased = PlayerPrefs.GetInt($"Upgrade_{upgrade.upgradeName}_Purchased", 0) == 1;
        if (upgrade.isPurchased)
        {
            ApplyUpgrade(upgrade);
        }
    }
}
```

### Add More Upgrades:
Edit `UpgradeSystem.cs` → `InitializeDefaultUpgrades()` to add more upgrades.

### Add Visual Effects:
- Add particle effects when purchasing
- Add sound effects
- Add animation when shop opens/closes

---

## 📝 Summary Checklist

- [ ] UpgradeSystem component added to scene
- [ ] UpgradeShopCanvas created
- [ ] UpgradeShopPanel created with proper sizing
- [ ] Shop title and money display added
- [ ] ScrollView created and configured
- [ ] UpgradeItemPrefab created with all UI elements
- [ ] Prefab saved and removed from scene
- [ ] Close button created
- [ ] OpenShopButton created in main UI
- [ ] UpgradeShopUI component added and references assigned
- [ ] OpenShopButton connected to ToggleShop()
- [ ] Tested: Shop opens/closes
- [ ] Tested: Upgrades display correctly
- [ ] Tested: Can purchase upgrades
- [ ] Tested: Upgrades actually work in-game

---

## 🎉 You're Done!

The upgrade system should now be fully functional. Players can:
- Open the shop
- View available upgrades
- Purchase upgrades with money
- See purchased upgrades marked as "SOLD OUT"
- Experience the upgrades in gameplay

Good luck with your portfolio project! 🚀

