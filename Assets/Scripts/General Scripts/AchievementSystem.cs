using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Achievement
{
    public string id;
    public string title;
    public string description;
    public Sprite icon;
    public int targetValue;
    public int currentValue;
    public bool isUnlocked;
    public int rewardMoney;
    
    public Achievement(string id, string title, string description, int target, int reward = 0)
    {
        this.id = id;
        this.title = title;
        this.description = description;
        this.targetValue = target;
        this.currentValue = 0;
        this.isUnlocked = false;
        this.rewardMoney = reward;
    }
}

public class AchievementSystem : MonoBehaviour
{
    public static AchievementSystem Instance { get; private set; }
    
    [Header("Achievements")]
    [SerializeField] private List<Achievement> achievements = new List<Achievement>();
    
    // Events
    public System.Action<Achievement> OnAchievementUnlocked;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeAchievements();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeAchievements()
    {
        if (achievements.Count == 0)
        {
            achievements.Add(new Achievement("first_serve", "First Cup", "Serve your first customer", 1, 10));
            achievements.Add(new Achievement("serve_10", "Getting Started", "Serve 10 customers", 10, 50));
            achievements.Add(new Achievement("serve_50", "Coffee Master", "Serve 50 customers", 50, 200));
            achievements.Add(new Achievement("serve_100", "Coffee Legend", "Serve 100 customers", 100, 500));
            achievements.Add(new Achievement("perfect_day", "Perfect Day", "Serve 20 customers without losing any", 20, 300));
            achievements.Add(new Achievement("speed_demon", "Speed Demon", "Serve 5 customers in under 30 seconds", 5, 250));
            achievements.Add(new Achievement("money_maker", "Money Maker", "Earn $1000 total", 1000, 100));
            achievements.Add(new Achievement("bean_collector", "Bean Collector", "Collect 100 beans", 100, 150));
        }
        
        LoadAchievements();
    }
    
    public void IncrementAchievement(string achievementId, int amount = 1)
    {
        Achievement achievement = achievements.FirstOrDefault(a => a.id == achievementId);
        if (achievement == null || achievement.isUnlocked) return;
        
        achievement.currentValue += amount;
        
        if (achievement.currentValue >= achievement.targetValue)
        {
            UnlockAchievement(achievement);
        }
        
        SaveAchievements();
    }
    
    public void SetAchievementValue(string achievementId, int value)
    {
        Achievement achievement = achievements.FirstOrDefault(a => a.id == achievementId);
        if (achievement == null || achievement.isUnlocked) return;
        
        achievement.currentValue = value;
        
        if (achievement.currentValue >= achievement.targetValue)
        {
            UnlockAchievement(achievement);
        }
        
        SaveAchievements();
    }
    
    private void UnlockAchievement(Achievement achievement)
    {
        if (achievement.isUnlocked) return;
        
        achievement.isUnlocked = true;
        achievement.currentValue = achievement.targetValue;
        
        // Give reward
        if (achievement.rewardMoney > 0 && GameManager.Instance != null)
        {
            GameManager.Instance.AddMoney(achievement.rewardMoney);
        }
        
        OnAchievementUnlocked?.Invoke(achievement);
        Debug.Log($"Achievement Unlocked: {achievement.title} - {achievement.description}");
        
        // Show notification UI (you'll need to implement this)
        ShowAchievementNotification(achievement);
    }
    
    private void ShowAchievementNotification(Achievement achievement)
    {
        // TODO: Implement UI notification
        // This would show a popup like "Achievement Unlocked: Coffee Master!"
    }
    
    public List<Achievement> GetAchievements() => achievements;
    public List<Achievement> GetUnlockedAchievements() => achievements.Where(a => a.isUnlocked).ToList();
    public List<Achievement> GetLockedAchievements() => achievements.Where(a => !a.isUnlocked).ToList();
    
    public float GetCompletionPercentage()
    {
        if (achievements.Count == 0) return 0f;
        return (float)achievements.Count(a => a.isUnlocked) / achievements.Count * 100f;
    }
    
    private void SaveAchievements()
    {
        foreach (var achievement in achievements)
        {
            PlayerPrefs.SetInt($"Achievement_{achievement.id}_Value", achievement.currentValue);
            PlayerPrefs.SetInt($"Achievement_{achievement.id}_Unlocked", achievement.isUnlocked ? 1 : 0);
        }
        PlayerPrefs.Save();
    }
    
    private void LoadAchievements()
    {
        foreach (var achievement in achievements)
        {
            achievement.currentValue = PlayerPrefs.GetInt($"Achievement_{achievement.id}_Value", 0);
            achievement.isUnlocked = PlayerPrefs.GetInt($"Achievement_{achievement.id}_Unlocked", 0) == 1;
        }
    }
}

