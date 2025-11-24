using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class EnhancedUIManager : MonoBehaviour
{
    public static EnhancedUIManager Instance { get; private set; }
    
    [Header("Money Display")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private GameObject moneyPopupPrefab;
    [SerializeField] private Transform moneyPopupParent;
    [SerializeField] private float popupDuration = 1.5f;
    
    [Header("Combo Display")]
    [SerializeField] private GameObject comboPanel;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI comboMultiplierText;
    [SerializeField] private float comboShowDuration = 2f;
    
    [Header("Statistics Display")]
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private TextMeshProUGUI customersServedText;
    [SerializeField] private TextMeshProUGUI satisfactionRateText;
    [SerializeField] private TextMeshProUGUI averageServeTimeText;
    [SerializeField] private TextMeshProUGUI earningsPerHourText;
    
    [Header("Inventory Display")]
    [SerializeField] private TextMeshProUGUI beanCountText;
    [SerializeField] private TextMeshProUGUI coffeeCountText;
    [SerializeField] private Image[] beanIcons;
    [SerializeField] private Image[] coffeeIcons;
    
    [Header("Animation Settings")]
    [SerializeField] private AnimationCurve popupCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private float moneyAnimationSpeed = 2f;
    
    private int currentMoney = 0;
    private int targetMoney = 0;
    private Coroutine moneyAnimationCoroutine;
    
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
    }
    
    private void Start()
    {
        InitializeUI();
        SubscribeToEvents();
        
        // Initialize money from GameManager
        if (GameManager.Instance != null)
        {
            currentMoney = GameManager.Instance.GetMoney();
            targetMoney = currentMoney;
            UpdateMoneyDisplay();
        }
    }
    
    private void Update()
    {
        // Sync money display with GameManager
        if (GameManager.Instance != null)
        {
            int newMoney = GameManager.Instance.GetMoney();
            if (newMoney != targetMoney)
            {
                targetMoney = newMoney;
                if (moneyAnimationCoroutine == null)
                {
                    moneyAnimationCoroutine = StartCoroutine(AnimateMoneyCounter());
                }
            }
        }
    }
    
    private void InitializeUI()
    {
        if (comboPanel != null) comboPanel.SetActive(false);
        if (statsPanel != null) statsPanel.SetActive(false);
        
        UpdateMoneyDisplay();
        UpdateInventoryDisplay();
    }
    
    private void SubscribeToEvents()
    {
        if (GameManager.Instance != null)
        {
            // Subscribe to money changes
            GameManager.Instance.OnMoneyChanged += OnMoneyChanged;
        }
        
        if (GameStatistics.Instance != null)
        {
            GameStatistics.Instance.OnComboUpdated += ShowCombo;
            GameStatistics.Instance.OnComboBroken += HideCombo;
            GameStatistics.Instance.OnStatsUpdated += UpdateStatistics;
        }
        
        PlayerInventory inventory = FindObjectOfType<PlayerInventory>();
        if (inventory != null)
        {
            inventory.OnInventoryChanged += OnInventoryChanged;
        }
    }
    
    public void ShowMoneyGain(int amount, Vector3 worldPosition)
    {
        // Animate money counter
        targetMoney = currentMoney + amount;
        if (moneyAnimationCoroutine != null)
        {
            StopCoroutine(moneyAnimationCoroutine);
        }
        moneyAnimationCoroutine = StartCoroutine(AnimateMoneyCounter());
        
        // Show popup
        if (moneyPopupPrefab != null && moneyPopupParent != null)
        {
            GameObject popup = Instantiate(moneyPopupPrefab, moneyPopupParent);
            TextMeshProUGUI popupText = popup.GetComponentInChildren<TextMeshProUGUI>();
            if (popupText != null)
            {
                popupText.text = $"+${amount}";
            }
            
            StartCoroutine(AnimateMoneyPopup(popup));
        }
    }
    
    private IEnumerator AnimateMoneyCounter()
    {
        while (currentMoney != targetMoney)
        {
            int difference = targetMoney - currentMoney;
            int step = Mathf.CeilToInt(Mathf.Abs(difference) * Time.deltaTime * moneyAnimationSpeed);
            if (step == 0) step = difference > 0 ? 1 : -1;
            
            currentMoney += step;
            if ((difference > 0 && currentMoney > targetMoney) || 
                (difference < 0 && currentMoney < targetMoney))
            {
                currentMoney = targetMoney;
            }
            
            UpdateMoneyDisplay();
            yield return null;
        }
    }
    
    private IEnumerator AnimateMoneyPopup(GameObject popup)
    {
        RectTransform rectTransform = popup.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = popup.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = popup.AddComponent<CanvasGroup>();
        
        Vector3 startPos = rectTransform.anchoredPosition;
        Vector3 endPos = startPos + Vector3.up * 100f;
        float elapsed = 0f;
        
        while (elapsed < popupDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / popupDuration;
            
            rectTransform.anchoredPosition = Vector3.Lerp(startPos, endPos, popupCurve.Evaluate(t));
            canvasGroup.alpha = 1f - t;
            
            yield return null;
        }
        
        Destroy(popup);
    }
    
    private void ShowCombo(int comboCount)
    {
        if (comboPanel == null) return;
        
        comboPanel.SetActive(true);
        
        if (comboText != null)
        {
            comboText.text = $"COMBO x{comboCount}";
        }
        
        if (comboMultiplierText != null)
        {
            // Calculate bonus multiplier (e.g., 5% per combo)
            float multiplier = 1f + (comboCount * 0.05f);
            comboMultiplierText.text = $"+{(multiplier - 1f) * 100f:F0}% Bonus";
        }
        
        // Cancel previous hide coroutine
        StopAllCoroutines();
        StartCoroutine(HideComboAfterDelay());
    }
    
    private void HideCombo(int comboCount)
    {
        if (comboPanel == null) return;
        comboPanel.SetActive(false);
    }
    
    private IEnumerator HideComboAfterDelay()
    {
        yield return new WaitForSeconds(comboShowDuration);
        if (comboPanel != null && GameStatistics.Instance != null && GameStatistics.Instance.GetCurrentCombo() == 0)
        {
            comboPanel.SetActive(false);
        }
    }
    
    private void UpdateMoneyDisplay()
    {
        if (moneyText != null)
        {
            moneyText.text = $"${currentMoney}";
        }
    }
    
    private void UpdateInventoryDisplay()
    {
        PlayerInventory inventory = FindObjectOfType<PlayerInventory>();
        if (inventory == null) return;
        
        int beans = inventory.GetItemCount(ItemType.CoffeeBean);
        int coffees = inventory.GetItemCount(ItemType.CoffeeCup);
        int maxBeans = inventory.GetCapacity(ItemType.CoffeeBean);
        int maxCoffees = inventory.GetCapacity(ItemType.CoffeeCup);
        
        if (beanCountText != null)
        {
            beanCountText.text = $"{beans}/{maxBeans}";
        }
        
        if (coffeeCountText != null)
        {
            coffeeCountText.text = $"{coffees}/{maxCoffees}";
        }
        
        // Update visual icons
        UpdateBeanIcons(beans, maxBeans);
        UpdateCoffeeIcons(coffees, maxCoffees);
    }
    
    private void UpdateBeanIcons(int current, int max)
    {
        if (beanIcons == null) return;
        
        for (int i = 0; i < beanIcons.Length; i++)
        {
            if (beanIcons[i] != null)
            {
                beanIcons[i].gameObject.SetActive(i < max);
                beanIcons[i].color = i < current ? Color.white : new Color(0.3f, 0.3f, 0.3f, 0.5f);
            }
        }
    }
    
    private void UpdateCoffeeIcons(int current, int max)
    {
        if (coffeeIcons == null) return;
        
        for (int i = 0; i < coffeeIcons.Length; i++)
        {
            if (coffeeIcons[i] != null)
            {
                coffeeIcons[i].gameObject.SetActive(i < max);
                coffeeIcons[i].color = i < current ? Color.white : new Color(0.3f, 0.3f, 0.3f, 0.5f);
            }
        }
    }
    
    private void UpdateStatistics()
    {
        if (GameStatistics.Instance == null) return;
        
        if (customersServedText != null)
        {
            customersServedText.text = $"Customers Served: {GameStatistics.Instance.GetTotalCustomersServed()}";
        }
        
        if (satisfactionRateText != null)
        {
            satisfactionRateText.text = $"Satisfaction: {GameStatistics.Instance.GetCustomerSatisfactionRate():F1}%";
        }
        
        if (averageServeTimeText != null)
        {
            averageServeTimeText.text = $"Avg Serve Time: {GameStatistics.Instance.GetAverageServeTime():F1}s";
        }
        
        if (earningsPerHourText != null)
        {
            earningsPerHourText.text = $"Earnings/Hour: ${GameStatistics.Instance.GetEarningsPerHour():F0}";
        }
    }
    
    private void OnInventoryChanged(ItemType itemType, int count)
    {
        UpdateInventoryDisplay();
    }
    
    private void OnMoneyChanged(int amount)
    {
        // This is called when money changes via GameManager event
        // The Update method will handle syncing the display
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMoneyChanged -= OnMoneyChanged;
        }
        
        if (GameStatistics.Instance != null)
        {
            GameStatistics.Instance.OnComboUpdated -= ShowCombo;
            GameStatistics.Instance.OnComboBroken -= HideCombo;
            GameStatistics.Instance.OnStatsUpdated -= UpdateStatistics;
        }
    }
    
    public void ToggleStatsPanel()
    {
        if (statsPanel != null)
        {
            statsPanel.SetActive(!statsPanel.activeSelf);
            if (statsPanel.activeSelf)
            {
                UpdateStatistics();
            }
        }
    }
}

