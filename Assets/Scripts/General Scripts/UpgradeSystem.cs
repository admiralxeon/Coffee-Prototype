using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Upgrade
{
    public string upgradeName;
    public string description;
    public int cost;
    public UpgradeType type;
    public float value; // The value this upgrade provides
    public bool isPurchased = false;
    
    public enum UpgradeType
    {
        MachineSpeed,        // Reduces brewing time
        InventoryCapacity,  // Increases max beans/coffee
        MovementSpeed,      // Increases player speed
        MultipleCups,       // Brews multiple cups at once
        AutoCollect,       // Auto-collects coffee when ready
        CustomerPatience    // Increases customer patience time
    }
}

public class UpgradeSystem : MonoBehaviour
{
    public static UpgradeSystem Instance { get; private set; }
    
    [Header("Available Upgrades")]
    [SerializeField] private List<Upgrade> availableUpgrades = new List<Upgrade>();
    
    [Header("Upgrade Effects")]
    [SerializeField] private float machineSpeedMultiplier = 1f;
    [SerializeField] private int inventoryCapacityBonus = 0;
    [SerializeField] private float movementSpeedMultiplier = 1f;
    [SerializeField] private int cupsPerBrew = 1;
    
    private float baseMachineSpeedMultiplier = 1f;
    private int baseInventoryCapacityBonus = 0;
    private float baseMovementSpeedMultiplier = 1f;
    private int baseCupsPerBrew = 1;
    
    // Events
    public System.Action<Upgrade> OnUpgradePurchased;
    public System.Action OnUpgradesChanged;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        InitializeDefaultUpgrades();
        LoadUpgrades();
    }
    
    private void OnDestroy()
    {
        SaveUpgrades();
    }
    
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
        // Reset multipliers to base values
        machineSpeedMultiplier = baseMachineSpeedMultiplier;
        inventoryCapacityBonus = baseInventoryCapacityBonus;
        movementSpeedMultiplier = baseMovementSpeedMultiplier;
        cupsPerBrew = baseCupsPerBrew;
        
        // Reapply all purchased upgrades
        foreach (var upgrade in availableUpgrades)
        {
            upgrade.isPurchased = PlayerPrefs.GetInt($"Upgrade_{upgrade.upgradeName}_Purchased", 0) == 1;
            if (upgrade.isPurchased)
            {
                // Reapply upgrade effects
                ApplyUpgrade(upgrade);
            }
        }
    }
    
    private void InitializeDefaultUpgrades()
    {
        if (availableUpgrades.Count == 0)
        {
            availableUpgrades.Add(new Upgrade
            {
                upgradeName = "Faster Brewing",
                description = "Reduce brewing time by 25%",
                cost = 50,
                type = Upgrade.UpgradeType.MachineSpeed,
                value = 0.75f
            });
            
            availableUpgrades.Add(new Upgrade
            {
                upgradeName = "Bigger Pockets",
                description = "Carry 2 more beans",
                cost = 30,
                type = Upgrade.UpgradeType.InventoryCapacity,
                value = 2f
            });
            
            availableUpgrades.Add(new Upgrade
            {
                upgradeName = "Quick Feet",
                description = "Move 20% faster",
                cost = 40,
                type = Upgrade.UpgradeType.MovementSpeed,
                value = 1.2f
            });
            
            availableUpgrades.Add(new Upgrade
            {
                upgradeName = "Double Brew",
                description = "Brew 2 cups at once",
                cost = 100,
                type = Upgrade.UpgradeType.MultipleCups,
                value = 2f
            });
        }
    }
    
    public bool PurchaseUpgrade(Upgrade upgrade)
    {
        if (upgrade.isPurchased)
        {
            Debug.LogWarning($"Upgrade {upgrade.upgradeName} already purchased!");
            return false;
        }
        
        if (GameManager.Instance == null || !GameManager.Instance.CanAfford(upgrade.cost))
        {
            Debug.LogWarning($"Cannot afford upgrade {upgrade.upgradeName}!");
            return false;
        }
        
        GameManager.Instance.SpendMoney(upgrade.cost);
        upgrade.isPurchased = true;
        ApplyUpgrade(upgrade);
        SaveUpgrades(); // Save immediately after purchase
        
        OnUpgradePurchased?.Invoke(upgrade);
        OnUpgradesChanged?.Invoke();
        
        Debug.Log($"Purchased upgrade: {upgrade.upgradeName}");
        return true;
    }
    
    private void ApplyUpgrade(Upgrade upgrade)
    {
        switch (upgrade.type)
        {
            case Upgrade.UpgradeType.MachineSpeed:
                machineSpeedMultiplier *= upgrade.value;
                UpdateAllMachines();
                break;
                
            case Upgrade.UpgradeType.InventoryCapacity:
                inventoryCapacityBonus += (int)upgrade.value;
                UpdatePlayerInventory();
                break;
                
            case Upgrade.UpgradeType.MovementSpeed:
                movementSpeedMultiplier *= upgrade.value;
                UpdatePlayerMovement();
                break;
                
            case Upgrade.UpgradeType.MultipleCups:
                cupsPerBrew = (int)upgrade.value;
                UpdateAllMachines();
                break;
        }
    }
    
    private void UpdateAllMachines()
    {
        CoffeeMachine[] machines = FindObjectsOfType<CoffeeMachine>();
        foreach (var machine in machines)
        {
            // Machines will check upgrade system when brewing
        }
    }
    
    private void UpdatePlayerInventory()
    {
        PlayerInventory inventory = FindObjectOfType<PlayerInventory>();
        if (inventory != null)
        {
            // Inventory will check upgrade system for capacity
        }
    }
    
    private void UpdatePlayerMovement()
    {
        // Movement speed is applied dynamically in PlayerController
        // This method is kept for future use if needed
    }
    
    // Getters for other systems
    public float GetMachineSpeedMultiplier() => machineSpeedMultiplier;
    public int GetInventoryCapacityBonus() => inventoryCapacityBonus;
    public float GetMovementSpeedMultiplier() => movementSpeedMultiplier;
    public int GetCupsPerBrew() => cupsPerBrew;
    
    public List<Upgrade> GetAvailableUpgrades() => availableUpgrades;
    public List<Upgrade> GetPurchasedUpgrades()
    {
        return availableUpgrades.FindAll(u => u.isPurchased);
    }
    
    public List<Upgrade> GetAffordableUpgrades()
    {
        if (GameManager.Instance == null) return new List<Upgrade>();
        return availableUpgrades.FindAll(u => !u.isPurchased && GameManager.Instance.CanAfford(u.cost));
    }
}

