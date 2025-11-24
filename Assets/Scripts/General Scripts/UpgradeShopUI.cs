using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class UpgradeShopUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject upgradeItemPrefab;
    [SerializeField] private Transform upgradeContainer; // Content of ScrollView
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI moneyDisplay;
    
    [Header("Audio")]
    [SerializeField] private AudioClip purchaseSound;
    [SerializeField] private AudioClip errorSound;
    
    private AudioSource audioSource;
    private List<GameObject> upgradeItemInstances = new List<GameObject>();
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Hide shop panel initially
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
    }
    
    private void Start()
    {
        SetupButtons();
        RefreshUpgradeList();
        
        // Subscribe to upgrade system events
        if (UpgradeSystem.Instance != null)
        {
            UpgradeSystem.Instance.OnUpgradePurchased += OnUpgradePurchased;
            UpgradeSystem.Instance.OnUpgradesChanged += RefreshUpgradeList;
        }
        
        // Subscribe to money changes
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMoneyChanged += OnMoneyChanged;
        }
    }
    
    private void SetupButtons()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseShop);
        }
    }
    
    public void OpenShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            RefreshUpgradeList();
            UpdateMoneyDisplay();
        }
    }
    
    public void CloseShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
    }
    
    public void ToggleShop()
    {
        if (shopPanel != null)
        {
            if (shopPanel.activeSelf)
            {
                CloseShop();
            }
            else
            {
                OpenShop();
            }
        }
    }
    
    private void RefreshUpgradeList()
    {
        if (UpgradeSystem.Instance == null || upgradeContainer == null) return;
        
        // Clear existing upgrade items
        foreach (GameObject item in upgradeItemInstances)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }
        upgradeItemInstances.Clear();
        
        // Create upgrade items
        List<Upgrade> upgrades = UpgradeSystem.Instance.GetAvailableUpgrades();
        foreach (Upgrade upgrade in upgrades)
        {
            CreateUpgradeItem(upgrade);
        }
    }
    
    private void CreateUpgradeItem(Upgrade upgrade)
    {
        if (upgradeItemPrefab == null || upgradeContainer == null) return;
        
        GameObject itemInstance = Instantiate(upgradeItemPrefab, upgradeContainer);
        upgradeItemInstances.Add(itemInstance);
        
        // Find child components
        TextMeshProUGUI nameText = itemInstance.transform.Find("UpgradeName")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI descText = itemInstance.transform.Find("UpgradeDescription")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI costText = itemInstance.transform.Find("UpgradeCost")?.GetComponent<TextMeshProUGUI>();
        Button purchaseButton = itemInstance.transform.Find("PurchaseButton")?.GetComponent<Button>();
        GameObject purchasedBadge = itemInstance.transform.Find("PurchasedBadge")?.gameObject;
        
        // Set upgrade information
        if (nameText != null)
        {
            nameText.text = upgrade.upgradeName;
        }
        
        if (descText != null)
        {
            descText.text = upgrade.description;
        }
        
        if (costText != null)
        {
            costText.text = $"${upgrade.cost}";
        }
        
        // Handle purchased state
        if (upgrade.isPurchased)
        {
            if (purchasedBadge != null)
            {
                purchasedBadge.SetActive(true);
            }
            if (purchaseButton != null)
            {
                purchaseButton.interactable = false;
                purchaseButton.GetComponentInChildren<TextMeshProUGUI>().text = "SOLD OUT";
            }
            if (costText != null)
            {
                costText.text = "PURCHASED";
                costText.color = Color.green;
            }
        }
        else
        {
            if (purchasedBadge != null)
            {
                purchasedBadge.SetActive(false);
            }
            
            // Check if affordable
            bool canAfford = GameManager.Instance != null && GameManager.Instance.CanAfford(upgrade.cost);
            
            if (purchaseButton != null)
            {
                purchaseButton.interactable = canAfford;
                purchaseButton.onClick.RemoveAllListeners();
                purchaseButton.onClick.AddListener(() => PurchaseUpgrade(upgrade));
                
                // Update button color based on affordability
                ColorBlock colors = purchaseButton.colors;
                colors.normalColor = canAfford ? Color.white : Color.gray;
                purchaseButton.colors = colors;
            }
            
            if (costText != null)
            {
                costText.color = canAfford ? Color.white : Color.red;
            }
        }
    }
    
    private void PurchaseUpgrade(Upgrade upgrade)
    {
        if (UpgradeSystem.Instance == null) return;
        
        bool success = UpgradeSystem.Instance.PurchaseUpgrade(upgrade);
        
        if (success)
        {
            // Play success sound
            if (audioSource != null && purchaseSound != null)
            {
                audioSource.PlayOneShot(purchaseSound);
            }
            
            // Refresh the list to update UI
            RefreshUpgradeList();
            UpdateMoneyDisplay();
        }
        else
        {
            // Play error sound
            if (audioSource != null && errorSound != null)
            {
                audioSource.PlayOneShot(errorSound);
            }
        }
    }
    
    private void OnUpgradePurchased(Upgrade upgrade)
    {
        RefreshUpgradeList();
        UpdateMoneyDisplay();
    }
    
    private void OnMoneyChanged(int amount)
    {
        UpdateMoneyDisplay();
        RefreshUpgradeList(); // Refresh to update affordability
    }
    
    private void UpdateMoneyDisplay()
    {
        if (moneyDisplay != null && GameManager.Instance != null)
        {
            moneyDisplay.text = $"Money: ${GameManager.Instance.GetMoney()}";
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (UpgradeSystem.Instance != null)
        {
            UpgradeSystem.Instance.OnUpgradePurchased -= OnUpgradePurchased;
            UpgradeSystem.Instance.OnUpgradesChanged -= RefreshUpgradeList;
        }
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMoneyChanged -= OnMoneyChanged;
        }
    }
}

