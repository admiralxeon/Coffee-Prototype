using UnityEngine;
using System.Collections.Generic;

public class GameStatistics : MonoBehaviour
{
    public static GameStatistics Instance { get; private set; }
    
    [Header("Statistics")]
    private int totalCustomersServed = 0;
    private int totalMoneyEarned = 0;
    private int customersLost = 0;
    private float totalPlayTime = 0f;
    private float averageServeTime = 0f;
    private int longestCombo = 0;
    private int currentCombo = 0;
    private float lastServeTime = 0f;
    private float comboTimeWindow = 10f; // Seconds to maintain combo
    
    [Header("Session Stats")]
    private int sessionCustomersServed = 0;
    private int sessionMoneyEarned = 0;
    private float sessionStartTime = 0f;
    
    private List<float> serveTimes = new List<float>();
    
    // Events
    public System.Action<int> OnComboUpdated;
    public System.Action<int> OnComboBroken;
    public System.Action OnStatsUpdated;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            sessionStartTime = Time.time;
            LoadStatistics();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        totalPlayTime += Time.deltaTime;
        
        // Check if combo should break
        if (currentCombo > 0 && Time.time - lastServeTime > comboTimeWindow)
        {
            BreakCombo();
        }
    }
    
    public void RecordCustomerServed(int payment, float serveTime)
    {
        totalCustomersServed++;
        sessionCustomersServed++;
        totalMoneyEarned += payment;
        sessionMoneyEarned += payment;
        
        serveTimes.Add(serveTime);
        UpdateAverageServeTime();
        
        // Combo system
        if (Time.time - lastServeTime <= comboTimeWindow)
        {
            currentCombo++;
            if (currentCombo > longestCombo)
            {
                longestCombo = currentCombo;
            }
            OnComboUpdated?.Invoke(currentCombo);
        }
        else
        {
            currentCombo = 1;
            OnComboUpdated?.Invoke(currentCombo);
        }
        
        lastServeTime = Time.time;
        OnStatsUpdated?.Invoke();
        SaveStatistics();
    }
    
    public void RecordCustomerLost()
    {
        customersLost++;
        BreakCombo();
        OnStatsUpdated?.Invoke();
    }
    
    private void BreakCombo()
    {
        if (currentCombo > 0)
        {
            OnComboBroken?.Invoke(currentCombo);
            currentCombo = 0;
        }
    }
    
    private void UpdateAverageServeTime()
    {
        if (serveTimes.Count == 0) return;
        
        float sum = 0f;
        foreach (float time in serveTimes)
        {
            sum += time;
        }
        averageServeTime = sum / serveTimes.Count;
        
        // Keep only last 100 serve times to prevent memory issues
        if (serveTimes.Count > 100)
        {
            serveTimes.RemoveAt(0);
        }
    }
    
    // Getters
    public int GetTotalCustomersServed() => totalCustomersServed;
    public int GetTotalMoneyEarned() => totalMoneyEarned;
    public int GetCustomersLost() => customersLost;
    public float GetTotalPlayTime() => totalPlayTime;
    public float GetAverageServeTime() => averageServeTime;
    public int GetLongestCombo() => longestCombo;
    public int GetCurrentCombo() => currentCombo;
    public int GetSessionCustomersServed() => sessionCustomersServed;
    public int GetSessionMoneyEarned() => sessionMoneyEarned;
    public float GetSessionPlayTime() => Time.time - sessionStartTime;
    
    public float GetCustomerSatisfactionRate()
    {
        int total = totalCustomersServed + customersLost;
        if (total == 0) return 100f;
        return (float)totalCustomersServed / total * 100f;
    }
    
    public float GetEarningsPerHour()
    {
        if (totalPlayTime == 0) return 0f;
        return (totalMoneyEarned / totalPlayTime) * 3600f; // Per hour
    }
    
    public void ResetSession()
    {
        sessionCustomersServed = 0;
        sessionMoneyEarned = 0;
        sessionStartTime = Time.time;
        currentCombo = 0;
    }
    
    private void SaveStatistics()
    {
        PlayerPrefs.SetInt("TotalCustomersServed", totalCustomersServed);
        PlayerPrefs.SetInt("TotalMoneyEarned", totalMoneyEarned);
        PlayerPrefs.SetInt("CustomersLost", customersLost);
        PlayerPrefs.SetFloat("TotalPlayTime", totalPlayTime);
        PlayerPrefs.SetInt("LongestCombo", longestCombo);
        PlayerPrefs.Save();
    }
    
    private void LoadStatistics()
    {
        totalCustomersServed = PlayerPrefs.GetInt("TotalCustomersServed", 0);
        totalMoneyEarned = PlayerPrefs.GetInt("TotalMoneyEarned", 0);
        customersLost = PlayerPrefs.GetInt("CustomersLost", 0);
        totalPlayTime = PlayerPrefs.GetFloat("TotalPlayTime", 0f);
        longestCombo = PlayerPrefs.GetInt("LongestCombo", 0);
    }
}

