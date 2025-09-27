using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game Stats")]
    [SerializeField] private int money = 0;
    [SerializeField] private int customersServed = 0;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI customersServedText;
    [SerializeField] private TextMeshProUGUI gameStatsText;
    
    [Header("Audio")]
    [SerializeField] private AudioClip moneyEarnedSound;
    private AudioSource audioSource;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    private void Start()
    {
        UpdateUI();
    }
    
    public void AddMoney(int amount)
    {
        money += amount;
        customersServed++;
        
        if (audioSource != null && moneyEarnedSound != null)
        {
            audioSource.PlayOneShot(moneyEarnedSound);
        }
        
        UpdateUI();
    }
    
    public int GetMoney()
    {
        return money;
    }
    
    public int GetCustomersServed()
    {
        return customersServed;
    }
    
    public void SpendMoney(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            UpdateUI();
        }
    }
    
    public bool CanAfford(int amount)
    {
        return money >= amount;
    }
    
    private void UpdateUI()
    {
        if (moneyText != null)
        {
            moneyText.text = $"Money: ${money}";
        }
        
        if (customersServedText != null)
        {
            customersServedText.text = $"Customers Served: {customersServed}";
        }
        
        if (gameStatsText != null)
        {
            gameStatsText.text = $"Money: ${money}\nCustomers: {customersServed}";
        }
    }
    
    public void ResetGame()
    {
        money = 0;
        customersServed = 0;
        UpdateUI();
    }
    
    public void SaveGame()
    {
        PlayerPrefs.SetInt("Money", money);
        PlayerPrefs.SetInt("CustomersServed", customersServed);
        PlayerPrefs.Save();
    }
    
    public void LoadGame()
    {
        money = PlayerPrefs.GetInt("Money", 0);
        customersServed = PlayerPrefs.GetInt("CustomersServed", 0);
        UpdateUI();
    }
    
    public float GetAverageEarningsPerCustomer()
    {
        return customersServed > 0 ? (float)money / customersServed : 0f;
    }
    
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGame();
        }
    }
    
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SaveGame();
        }
    }
}