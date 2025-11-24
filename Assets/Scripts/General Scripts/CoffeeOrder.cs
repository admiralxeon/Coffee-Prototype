using UnityEngine;

[System.Serializable]
public enum CoffeeType
{
    Simple,     // 1 bean, fast (2s)
    Regular,     // 3 beans, normal (3s)
    Premium,     // 5 beans, slow (5s)
    DoubleShot   // 7 beans, very slow (7s)
}

[System.Serializable]
public class CoffeeOrder
{
    public CoffeeType type;
    public int beansRequired;
    public float brewingTime;
    public int basePayment;
    public string displayName;
    
    public CoffeeOrder(CoffeeType coffeeType)
    {
        this.type = coffeeType;
        
        switch (coffeeType)
        {
            case CoffeeType.Simple:
                beansRequired = 1;
                brewingTime = 2f;
                basePayment = 5;
                displayName = "Simple Coffee";
                break;
                
            case CoffeeType.Regular:
                beansRequired = 3;
                brewingTime = 3f;
                basePayment = 10;
                displayName = "Regular Coffee";
                break;
                
            case CoffeeType.Premium:
                beansRequired = 5;
                brewingTime = 5f;
                basePayment = 20;
                displayName = "Premium Coffee";
                break;
                
            case CoffeeType.DoubleShot:
                beansRequired = 7;
                brewingTime = 7f;
                basePayment = 35;
                displayName = "Double Shot";
                break;
        }
    }
    
    public static CoffeeOrder GetRandomOrder()
    {
        // Weighted random - more likely to get simple/regular orders
        float rand = Random.Range(0f, 100f);
        
        if (rand < 40f) return new CoffeeOrder(CoffeeType.Simple);
        if (rand < 75f) return new CoffeeOrder(CoffeeType.Regular);
        if (rand < 90f) return new CoffeeOrder(CoffeeType.Premium);
        return new CoffeeOrder(CoffeeType.DoubleShot);
    }
}

