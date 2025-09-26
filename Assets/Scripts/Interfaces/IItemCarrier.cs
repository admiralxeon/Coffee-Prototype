public enum ItemType
{
    CoffeeBean,
    CoffeeCup,
    Money
}

public interface IItemCarrier
{
    bool CanCarryItem(ItemType itemType);
    bool TryAddItem(ItemType itemType);
    bool TryRemoveItem(ItemType itemType);
    int GetItemCount(ItemType itemType);
    int GetCapacity(ItemType itemType);
}