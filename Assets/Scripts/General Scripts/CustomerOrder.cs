using UnityEngine;

[System.Serializable]
public enum CustomerType
{
    Regular,    // Normal patience, normal payment
    VIP,        // Low patience, high payment (3x)
    Patient,    // High patience, normal payment
    Rushed      // Very low patience, normal payment, bonus for speed
}

[System.Serializable]
public class CustomerOrderData
{
    public CustomerType customerType;
    public CoffeeOrder coffeeOrder;
    public float patienceMultiplier;
    public int paymentMultiplier;
    public string customerName;
    
    public CustomerOrderData(CustomerType type, CoffeeOrder order)
    {
        this.customerType = type;
        this.coffeeOrder = order;
        this.customerName = GetRandomName();
        
        switch (type)
        {
            case CustomerType.Regular:
                patienceMultiplier = 1f;
                paymentMultiplier = 1;
                break;
                
            case CustomerType.VIP:
                patienceMultiplier = 0.6f; // Less patient
                paymentMultiplier = 3; // Pays 3x
                break;
                
            case CustomerType.Patient:
                patienceMultiplier = 1.5f; // More patient
                paymentMultiplier = 1;
                break;
                
            case CustomerType.Rushed:
                patienceMultiplier = 0.5f; // Very impatient
                paymentMultiplier = 1;
                break;
        }
    }
    
    public int GetFinalPayment()
    {
        int basePayment = coffeeOrder.basePayment;
        return basePayment * paymentMultiplier;
    }
    
    public float GetPatienceTime(float basePatience)
    {
        return basePatience * patienceMultiplier;
    }
    
    private string GetRandomName()
    {
        string[] names = { "Alex", "Sam", "Jordan", "Casey", "Taylor", "Morgan", "Riley", "Avery", "Quinn", "Dakota" };
        return names[Random.Range(0, names.Length)];
    }
    
    public static CustomerOrderData GenerateRandomOrder()
    {
        CoffeeOrder coffeeOrder = CoffeeOrder.GetRandomOrder();
        
        // Weighted random for customer types
        float rand = Random.Range(0f, 100f);
        CustomerType type;
        
        if (rand < 60f) type = CustomerType.Regular;
        else if (rand < 75f) type = CustomerType.Patient;
        else if (rand < 90f) type = CustomerType.Rushed;
        else type = CustomerType.VIP;
        
        return new CustomerOrderData(type, coffeeOrder);
    }
}

