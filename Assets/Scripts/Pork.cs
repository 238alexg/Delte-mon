using UnityEngine;

public static class Pork
{
    public static Sprite PorkSprite;

    public static ItemClass Porkify(ItemClass item)
    {
        item.itemDescription = "What is pork!?";
        item.itemImage = PorkSprite;
        item.itemName = item.itemName + " Pork";

        return item;
    }
}
