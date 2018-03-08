using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BattleDelts
{
    namespace UI
    {
        public class ItemsUI : UIScreen
        {

            public GameObject ListItemObject, curOverviewItem;
            public Transform ItemOverviewUI, ItemListContent;
            bool allItemsLoaded;
            public Color[] itemColors;

            public ItemClass activeItem;
            DeltemonClass activeDelt;

            // Animates opening up items and populates list with All item entries
            public override void Open()
            {
                // Remove move overviews if are up
                if (UIManager.Inst.PosseUI.CloseMoveOverviews())
                {
                    return;
                }

                ItemOverviewUI.gameObject.SetActiveIfChanged(false);

                // REFACTOR_TODO: Find a better way to reference state of items
                if (UIManager.Inst.BagMenuUI.gameObject.activeInHierarchy || UIManager.Inst.currentUI == UIMode.Deltemon || UIManager.Inst.inBattle)
                {
                    UIManager.Inst.currentUI = UIMode.Items;
                    if (!allItemsLoaded)
                    {
                        foreach (Transform child in ItemListContent.transform)
                        {
                            Destroy(child.gameObject);
                        }
                        int i = 0;
                        try
                        {
                            foreach (ItemData item in GameManager.Inst.allItems)
                            {
                                GameObject li = Instantiate(ListItemObject, ItemListContent);
                                Text[] texts = li.GetComponentsInChildren<Text>();

                                li.GetComponent<Image>().color = GetItemTypeColor(item.itemT);

                                texts[0].text = item.itemName;
                                texts[1].text = "X" + item.numberOfItem;
                                Button b = li.transform.GetChild(2).gameObject.GetComponent<Button>();
                                AddListener(b, i);
                                li.transform.localScale = Vector3.one;
                                i++;
                            }
                        }
                        catch (System.Exception e)
                        {
                            print(e);
                        }
                        allItemsLoaded = true;
                    }
                    base.Open();
                }
            }

            Color GetItemTypeColor(itemType itemType)
            {
                switch (itemType)
                {
                    case itemType.Ball:
                        return itemColors[0];
                    case itemType.Usable:
                        return itemColors[1];
                    case itemType.Repel:
                        return itemColors[2];
                    case itemType.Holdable:
                        return itemColors[3];
                    case itemType.MegaEvolve:
                        return itemColors[4];
                    case itemType.Quest:
                        return itemColors[5];
                    case itemType.Move:
                        return itemColors[6];
                    case itemType.Badge:
                        return itemColors[7];
                    default:
                        return Color.gray;
                }
            }

            // User presses Use Item on the ItemsUI. If Delt already selected, tried to use item. Else presents Delts to use Item on
            public void ChooseItem()
            {
                ItemClass item = curOverviewItem.transform.GetComponent<ItemClass>();
                if (UIManager.Inst.inBattle)
                {
                    if (item.itemT == itemType.Usable || item.itemT == itemType.Ball)
                    {
                        activeItem = item;

                        if ((activeDelt != null) || (item.itemT == itemType.Ball))
                        {
                            UseItem();
                        }
                        else
                        {
                            Open();
                        }
                    }
                    else
                    {
                        UIManager.Inst.StartMessage("Cannot give " + item.itemName + " to Delts in battle!");
                    }
                }
                else
                {
                    activeItem = item;

                    if ((item.itemT == itemType.Usable) || (item.itemT == itemType.Holdable)
                        || (item.itemT == itemType.MegaEvolve) || (item.itemT == itemType.Move))
                    {

                        // If item has to be to a Delt
                        if (activeDelt == null)
                        {
                            Open();
                        }
                        else
                        {
                            UseItem();
                        }
                    }
                    else if (item.itemT == itemType.Repel)
                    {
                        UseItem();
                    }
                    else
                    {
                        UIManager.Inst.StartMessage(activeItem.itemName + " cannot be given to Delts!");
                    }
                }
            }

            // Bring up instance of item into the Item Overview UI
            public void loadItemIntoUI(int index)
            {
                ItemData item = GameManager.Inst.allItems[index];

                curOverviewItem = (GameObject)Resources.Load("Items/" + item.itemName);
                ItemClass dispItem = curOverviewItem.transform.GetComponent<ItemClass>();

                // REFACTOR_TODO: Porkify this
                // Pork setting condition
                if (GameManager.Inst.pork)
                {
                    ItemOverviewUI.GetChild(0).gameObject.GetComponent<Text>().text = "What is pork!?";
                    ItemOverviewUI.GetChild(1).gameObject.GetComponent<Image>().sprite = Pork.PorkSprite;
                    ItemOverviewUI.GetChild(2).gameObject.GetComponent<Text>().text = dispItem.itemName + "Pork";
                    ItemOverviewUI.GetChild(3).gameObject.GetComponent<Text>().text = dispItem.itemT + " Pork";
                }
                else
                {
                    ItemOverviewUI.GetChild(0).gameObject.GetComponent<Text>().text = dispItem.itemDescription;
                    ItemOverviewUI.GetChild(1).gameObject.GetComponent<Image>().sprite = dispItem.itemImage;
                    ItemOverviewUI.GetChild(2).gameObject.GetComponent<Text>().text = dispItem.itemName;
                    ItemOverviewUI.GetChild(3).gameObject.GetComponent<Text>().text = "" + dispItem.itemT;
                }

                // Show Overview if not active
                if (!ItemOverviewUI.gameObject.activeInHierarchy)
                {
                    ItemOverviewUI.gameObject.SetActiveIfChanged(true);
                }
            }

            // Use an item on a delt, called if both have been selected
            public void UseItem()
            {

                // Active delt will be null if item is a ball
                if ((activeItem.itemT != itemType.Ball) && (activeItem.itemT != itemType.Repel))
                {
                    if ((activeDelt.curStatus == statusType.DA) && (activeItem.itemT == itemType.Usable) && ((activeItem.cure != statusType.DA) && (activeItem.cure != statusType.All)))
                    {
                        UIManager.Inst.StartMessage(activeDelt.nickname + " has DA'd and refuses your " + activeItem.itemName + "!");
                        return;
                    }
                }

                // Remove item from inventory
                GameManager.Inst.RemoveItem(activeItem);
                base.Close();

                if (UIManager.Inst.inBattle)
                {
                    // If player throwing a ball/using item on current battling Delt
                    if ((activeItem.itemT == itemType.Ball) || (activeDelt == BattleManager.Inst.curPlayerDelt))
                    {
                        if (UIManager.Inst.PosseUI.gameObject.activeInHierarchy)
                        {
                            UIManager.Inst.PosseUI.Close();
                        }
                        BattleManager.Inst.ChooseItem(true, activeItem);
                    }
                    // Do animation in Deltemon UI and skip player turn
                    else
                    {
                        DeltemonItemOutcome();
                        UIManager.Inst.StartMessage(null, null, () => UIManager.Inst.PosseUI.Close());
                        UIManager.Inst.StartMessage(null, null, () => BattleManager.Inst.ChooseItem(true, activeItem, false));
                    }
                }
                else
                {
                    if ((activeItem.itemT == itemType.Holdable) || (activeItem.itemT == itemType.MegaEvolve))
                    {
                        activeDelt.item = activeItem;
                        if (QuestManager.QuestMan.DeltItemQuests(activeDelt))
                        {
                            // LATER: Quest or something happens?
                        }
                        else
                        {
                            if (root.activeInHierarchy)
                            {
                                UIManager.Inst.StartMessage(null, null, () => UIManager.Inst.PosseUI.Open());
                            }
                            UIManager.Inst.StartMessage("Gave " + activeItem.itemName + " to " + activeDelt.nickname + "!");
                            // REFACTOR_TODO: Declare public variables and set these references in editor
                            UIManager.Inst.PosseUI.transform.GetChild(UIManager.Inst.PosseUI.overviewDeltIndex + 1).GetChild(4).GetComponent<Image>().sprite = activeItem.itemImage;
                        }
                    }
                    else if (activeItem.itemT == itemType.Usable)
                    {
                        if (!UIManager.Inst.PosseUI.gameObject.activeInHierarchy)
                        {
                            UIManager.Inst.PosseUI.Open();
                        }
                        DeltemonItemOutcome();
                    }
                    else if (activeItem.itemT == itemType.Repel)
                    {
                        UIManager.Inst.BagMenuUI.Open();
                        if (GameManager.Inst.pork)
                        {
                            UIManager.Inst.StartMessage("POOOORRRRK! POOOORK All OVER MY BOOODYYY!");
                            UIManager.Inst.StartMessage("You feel like you'll be just porkin' fine for " + activeItem.statUpgrades[0] + " steporks.");
                        }
                        else
                        {
                            UIManager.Inst.StartMessage("You smeared " + activeItem.itemName + " on yourself to ward off Delts!");
                            UIManager.Inst.StartMessage("You feel like you'll be safe for around " + activeItem.statUpgrades[0] + " more steps.");
                        }
                        PlayerMovement.Inst.repelStepsLeft += activeItem.statUpgrades[0];
                    }
                }
                UIManager.Inst.StartMessage(null, null, () => activeDelt = null);
                UIManager.Inst.StartMessage(null, null, () => activeItem = null);
            }

            // Applies all affects of items given to Delts in the Deltemon UI
            void DeltemonItemOutcome()
            {
                UIManager.Inst.StartMessage("Gave " + activeItem.itemName + " to " + activeDelt.nickname + "!");

                // Check for status improvements
                if (activeDelt.curStatus != statusType.None)
                {

                    // Cure Delt status, occurs if:
                    // 1) Item cure is the same as Delt's ailment
                    // 2) Item cures any ailment (but only DA status if the item ALSO heals GPA)
                    if ((activeItem.cure == activeDelt.curStatus) ||
                        ((activeItem.cure == statusType.All) && (activeDelt.curStatus != statusType.DA)) ||
                        ((activeItem.cure == statusType.All) && (activeItem.statUpgrades[0] > 0))
                    )
                    {
                        activeDelt.curStatus = statusType.None;
                        activeDelt.statusImage = UIManager.Inst.PosseUI.noStatus;
                        UIManager.Inst.PosseUI.transform.GetChild(UIManager.Inst.PosseUI.overviewDeltIndex + 1).GetChild(3).GetComponent<Image>().sprite = UIManager.Inst.PosseUI.noStatus;
                    }
                    // If item doesn't heal and doesn't cure Delt's status, it is ineffective
                    else if (activeItem.statUpgrades[0] == 0)
                    {
                        UIManager.Inst.StartMessage("This item accomplished nothing!");
                    }
                }

                // If the item heals GPA
                if (activeItem.statUpgrades[0] > 0)
                {

                    // If the Delt's health is already full
                    if (activeDelt.health == activeDelt.GPA)
                    {
                        UIManager.Inst.StartMessage(activeDelt.nickname + "'s GPA is already full!");
                    }
                    // Animate healing the Delt in the Deltemon UI
                    else
                    {
                        activeDelt.health += activeItem.statUpgrades[0];
                        UIManager.Inst.StartMessage(null, healDeltemon());
                    }
                }
            }

            // Heal a Deltemon while in the Deltemon UI (with use of a restorative item)
            public IEnumerator healDeltemon()
            {
                UIManager.Inst.PosseUI.Close();

                Slider healthBar = UIManager.Inst.PosseUI.transform.GetChild(UIManager.Inst.PosseUI.overviewDeltIndex + 1).GetChild(6).GetComponent<Slider>();
                Image healthBarFill = healthBar.transform.GetChild(1).GetChild(0).GetComponent<Image>();
                float increment;
                float heal = activeDelt.health - healthBar.value;

                if (activeDelt.health > activeDelt.GPA)
                {
                    activeDelt.health = activeDelt.GPA;
                }

                // If was a full heal, increment faster
                if (activeDelt.health == activeDelt.GPA)
                {
                    increment = heal / 30;
                }
                else
                {
                    increment = heal / 50;
                }

                // Animate health decrease
                while (healthBar.value < activeDelt.health)
                {

                    healthBar.value += increment;

                    // Set colors for lower health
                    if ((healthBar.value >= (activeDelt.GPA * 0.5f)) && (healthBarFill.color != BattleManager.Inst.fullHealth))
                    {
                        healthBarFill.color = BattleManager.Inst.fullHealth;
                    }
                    else if ((healthBar.value >= (activeDelt.GPA * 0.25f)) && (healthBarFill.color != BattleManager.Inst.fullHealth))
                    {
                        healthBarFill.color = BattleManager.Inst.halfHealth;
                    }

                    // Animation delay
                    yield return new WaitForSeconds(0.01f);

                    // So animation doesn't take infinite time
                    if (healthBar.value > activeDelt.health)
                    {
                        healthBar.value = activeDelt.health;
                    }
                    yield return null;
                }

                UIManager.Inst.PosseUI.overviewDeltIndex = -1;
            }

            // Close items
            public override void Close()
            {
                activeItem = null;

                if (activeDelt != null)
                {
                    activeDelt = null;
                    UIManager.Inst.PosseUI.Open(false);
                    UIManager.Inst.PosseUI.loadDeltIntoPlayerOverview(UIManager.Inst.PosseUI.overviewDeltIndex);
                }

                base.Close();
            }

            void AddListener(Button b, int i)
            {
                b.onClick.AddListener(() => loadItemIntoUI(i));
            }
        }
    }
}
