using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemShop : MonoBehaviour {
	public GameObject ItemSelectionUI, ListItemObject;
	public Transform ItemOverviewUI, ItemListContent, SellListContent;
	public Animator Bergie;
	public List<SellableItem> itemsForSale;
	public Text TotalItems, Cost, NumberOfItemInBag, CurrentPlayerCoins, BuySellButtonText;
	public Slider numberOfItems;
	public Image ShopTab, SellTab;

	UIManager UIMan;
	GameManager GameMan;

	ItemClass curSelectedItem;
	int curItemPrice;
	bool allItemsLoaded, allSellItemsLoaded, hasBought, hasSold, hasInteracted, hasTriggered, isAnimating, isBuying;

	string[] startDialogues;

	// Initialize values
	void Start() {
		startDialogues = new string[] { 
			"Hey there... what can I get ya, good lookin'?",
			"Come here often?",
			"Guns, ammo, puns. You name it, I'll sell it.",
			"If you don't like my prices, check your privelege",
			"What's cookin', good lookin?",
			"I'm buyin' what you're sellin' if you're buyin' what I'm sellin'",
			"Give me your money, honey!",
			"I'm selling the goods, if you would sell me your heart.",
			"Look at this PYT... Price You Treasure",
			"I'll price you digits if you give me yours"
		};
		UIMan = UIManager.Inst;
		GameMan = GameManager.Inst;
		allItemsLoaded = false;
		hasTriggered = false;
		hasBought = false;
		hasInteracted = false;
		allSellItemsLoaded = false;
		isAnimating = false;
		isBuying = true;
		hasSold = false;
		CurrentPlayerCoins.text = "" + GameMan.coins;

		// Set door to go to back to last town
		DoorAction shopDoor = this.transform.GetChild (0).GetComponent <DoorAction> ();
		TownRecoveryLocation townRecov = GameMan.FindTownRecov ();
		shopDoor.xCoordinate = townRecov.ShopX;
		shopDoor.yCoordinate = townRecov.ShopY;
		shopDoor.sceneName = townRecov.townName;

		SortItems ();
	}

	// Custom sort function for House Delts. Prior 1 = level, prior 2 = alphabetical name
	public void SortItems() {
		List<itemType> priorityFilter = new List<itemType> {itemType.Ball, itemType.Usable, itemType.Repel, 
			itemType.Holdable, itemType.MegaEvolve, itemType.Quest, itemType.Move, itemType.Badge};
		itemsForSale.Sort(delegate(SellableItem itemA, SellableItem itemB) {
			int indexA = priorityFilter.IndexOf (itemA.item.itemT);
			int indexB = priorityFilter.IndexOf (itemB.item.itemT);
			// If index of itemA has higher priority
			if (indexA < indexB) {
				return -1;
			} else if (indexA == indexB) {
				int alph = string.Compare(itemA.item.itemName, itemB.item.itemName);
				if (alph < 0) {
					return -1;
				} else if (alph == 0) {
					return 0;
				} else {
					return 1;
				}
			} else {
				return 1;
			}
		});
	}

	// When store clerk sequence triggered
	IEnumerator OnTriggerEnter2D(Collider2D player) {
		if (!hasTriggered) {
            UIMan.MovementUI.Close();
			hasTriggered = true;
			isBuying = true;

			ItemSelectionUI.SetActive (true);
			Bergie.gameObject.SetActive (true);
			Bergie.SetTrigger ("SlideIn");
			yield return new WaitForSeconds (1);

			// Opening dialogue
			if (!hasInteracted) {
				// Pick random line
				UIMan.StartNPCMessage (startDialogues [Random.Range (0, startDialogues.Length)], "Salesman Bergie");
			} else {
				UIMan.StartNPCMessage ("Couldn't get enough, eh? Can't blame you.", "Salesman Bergie");
			}

			hasInteracted = true;

			// Present Store UI
			UIMan.StartMessage (null, null, () => OpenStoreMenu (true));
			UIMan.StartMessage (null, null, () => ItemSelectionUI.GetComponent <Animator>().SetBool ("SlideIn", true));
		}
	}

	// Loads items into scrollable list on Left side of screen
	public void OpenStoreMenu(bool isShop) {
		// Hide overview until an item is selected
		ItemOverviewUI.gameObject.SetActive (false);

		ItemListContent.parent.parent.gameObject.SetActive (isShop);
		SellListContent.parent.parent.gameObject.SetActive (!isShop);

		isBuying = isShop;

		// Set tab button colors
		if (isShop) {
			SellTab.color = new Color (0.494f, 0.847f, 0.322f, 0);
			ShopTab.color = new Color (0.494f, 0.847f, 0.322f, 1);
			BuySellButtonText.text = "BUY";
		} else {
			SellTab.color = new Color (0.494f, 0.847f, 0.322f, 1);
			ShopTab.color = new Color (0.494f, 0.847f, 0.322f, 0);
			BuySellButtonText.text = "SELL";
		}
		// Not all items loaded into Shop list or not all items loaded into Sell list
		if ((!allItemsLoaded && isShop) || (!allSellItemsLoaded && !isShop)) {
			Transform contentList;
			if (isShop) {
				contentList = ItemListContent;
				allItemsLoaded = true;
			} else {
				contentList = SellListContent;
				allSellItemsLoaded = true;
			}
			// Delete any children in item list
			foreach (Transform child in contentList) {
				Destroy(child.gameObject);
			}
			int i = -1, dexCount = GameMan.deltDex.Count;
			// Generate list with available items (based on # Delts caught (store) OR sellable items in inventory (sell))
			foreach (SellableItem item in itemsForSale) {
				i++;
				if (isShop) {
					// Do not add item to shop list if player Dexcount not high enough
					if (item.deltDexNumberRequired > dexCount) {
						continue;
					}
				} 
				// Do not add item to sell list if player does not have item
				else if (!GameMan.allItems.Exists (pi => pi.itemName == item.item.itemName)) {
					continue;
				}

				GameObject li = Instantiate (ListItemObject, contentList);
				Text[] texts = li.GetComponentsInChildren<Text> ();
				texts [0].text = item.item.itemName;
				if (isShop) {
					texts [1].text = "" + item.cost;
				} else {
					texts [1].text = "" + (int)(item.cost * 0.5f);
				}

				switch (item.item.itemT) {
				case itemType.Ball:
					li.GetComponent <Image> ().color = UIMan.itemColors [0];
					break;
				case itemType.Usable:
					li.GetComponent <Image> ().color = UIMan.itemColors [1];
					break;
				case itemType.Repel:
					li.GetComponent <Image> ().color = UIMan.itemColors [2];
					break;
				case itemType.Holdable:
					li.GetComponent <Image> ().color = UIMan.itemColors [3];
					break;
				case itemType.MegaEvolve:
					li.GetComponent <Image> ().color = UIMan.itemColors [4];
					break;
				case itemType.Quest:
					li.GetComponent <Image> ().color = UIMan.itemColors [5];
					break;
				case itemType.Move:
					li.GetComponent <Image> ().color = UIMan.itemColors [6];
					break;
				case itemType.Badge:
					li.GetComponent <Image> ().color = UIMan.itemColors [7];
					break;
				}

				Button b = li.transform.GetChild(2).gameObject.GetComponent<Button>();
				AddListener(b, i);
				li.transform.localScale = Vector3.one;
			}
		}
	}

	// Returns number of item currently in the player's bag
	public int FindNumberOfItem(ItemClass item) {
		ItemData addable = GameMan.allItems.Find (myItem => myItem.itemName == item.itemName);
		if (addable == null) {
			return 0;
		} else {
			return addable.numberOfItem;
		}
	}
	// Returns the maximum amount of items the player can buy with their current coin count
	public byte FindMaxAmountCanBuy(long coins, int price) {
		long amount = (coins / price);
		if (amount > 100) {
			return 100;
		} else {
			return (byte)amount;
		}
	}

	// Leave the store menu
	public void closeStoreMenu() {
		NumberOfItemInBag.text = "...";
		StartCoroutine (AnimateStoreClose ());
		if (hasBought) {
			UIMan.StartNPCMessage ("Thanks for coming! See you soon!", "Salesman Bergie");
		} else if (hasSold) {
			UIMan.StartNPCMessage ("Thanks for the stuff! Buy something next time!", "Salesman Bergie");
		} else {
			UIMan.StartNPCMessage ("Come back when you actually feel like buying something.", "Salesman Bergie");
		}
		UIMan.StartMessage (null, null, () => Bergie.SetTrigger ("SlideOut"));
		hasBought = false;
		UIMan.StartMessage (null, ResumeMoving ());
	}

	IEnumerator ResumeMoving() {
		yield return new WaitForSeconds (1);
		PlayerMovement.Inst.ResumeMoving ();
	}

	IEnumerator AnimateStoreClose() {
		ItemSelectionUI.GetComponent <Animator>().SetBool ("SlideIn", false);
		yield return new WaitForSeconds (0.5f);
		ItemSelectionUI.SetActive (false);
	}

	// Add listener to items to load their information into the UI
	void AddListener(Button b, int i) {
		b.onClick.AddListener (() => LoadItemIntoUI (i));
	}

	// Load selected item into UI and update curItemPrice
	public void LoadItemIntoUI(int index) {
		curSelectedItem = itemsForSale [index].item;

		ItemOverviewUI.GetChild (0).gameObject.GetComponent<Text> ().text = curSelectedItem.itemDescription;
		ItemOverviewUI.GetChild (1).gameObject.GetComponent<Image> ().sprite = curSelectedItem.itemImage;
		ItemOverviewUI.GetChild (2).gameObject.GetComponent<Text> ().text = curSelectedItem.itemName;
		ItemOverviewUI.GetChild (3).gameObject.GetComponent<Text> ().text = "" + curSelectedItem.itemT;

		// Show Overview if not active
		if (!ItemOverviewUI.gameObject.activeInHierarchy) {
			ItemOverviewUI.gameObject.SetActive (true);
		}

		// Update # items in bag text
		NumberOfItemInBag.text = "" + FindNumberOfItem(curSelectedItem);

		// Set buy/sell specific values
		if (isBuying) {
			curItemPrice = itemsForSale [index].cost;
			byte maxItem = FindMaxAmountCanBuy (GameMan.coins, curItemPrice);
			numberOfItems.maxValue = maxItem;
		} else {
			curItemPrice = (int)(itemsForSale [index].cost * 0.5);
			numberOfItems.maxValue = FindNumberOfItem(curSelectedItem);
		}
		// Set slider & cost to 1 item
		numberOfItems.value = 0;
		UpdateNumberAndCost ();
	}

	// Update text of number of Items and total cost
	public void UpdateNumberAndCost() {
		TotalItems.text = "" + numberOfItems.value;
		Cost.text = "" + (numberOfItems.value * curItemPrice);
	}

	// User tries to buy and item
	public void SelectItem() {
		if (!isAnimating) {
			isAnimating = true;
			if (isBuying) {
				hasBought = true;
				UIMan.StartMessage(null, animateCoinsAndNumberOfItem(true));
			} else {
				hasSold = true;
				UIMan.StartMessage(null, animateCoinsAndNumberOfItem(false));
				UIMan.EndNPCMessage ();
				UIMan.StartMessage ("Sold " + numberOfItems.value + " of " + curSelectedItem.itemName);
			}
		}
	}

	// Animates the number of items and coins text of the Shop UI
	IEnumerator animateCoinsAndNumberOfItem(bool isBuying) {
		long oldCoins = GameMan.coins;
		int oldItemNum = FindNumberOfItem (curSelectedItem);
		int newItemNum, changeValue;

		if (isBuying) {
			GameMan.AddItem (curSelectedItem, (int)numberOfItems.value, false);
			GameMan.coins -= (long)(numberOfItems.value * curItemPrice);
			newItemNum = oldItemNum + (int)numberOfItems.value;
			changeValue = -1;
			allSellItemsLoaded = false;
		} else {
			GameMan.RemoveItem (curSelectedItem, (int)numberOfItems.value);
			GameMan.coins += (long)(numberOfItems.value * curItemPrice);
			newItemNum = oldItemNum - (int)numberOfItems.value;
			changeValue = 1;
		}

		long nowCoins = GameMan.coins;
		float spendtime;

		if (Mathf.Abs (oldCoins - nowCoins) > 100) {
			spendtime = 0.0003f;
		} else {
			spendtime = 0.001f;
		}

		while (oldCoins != nowCoins) {
			oldCoins += changeValue;
			CurrentPlayerCoins.text = "" + oldCoins;
			yield return new WaitForSeconds (spendtime);
		}
		SoundEffectManager.Inst.PlaySoundImmediate ("coinDing");
		while (oldItemNum != newItemNum) {
			oldItemNum -= changeValue;
			NumberOfItemInBag.text = "" + oldItemNum;
			yield return new WaitForSeconds (0.01f);
		}
		SoundEffectManager.Inst.PlaySoundImmediate ("coinDing");

		isAnimating = false;

		// Reset number of items available to buy/sell for the user
		if (isBuying) {
			numberOfItems.maxValue = FindMaxAmountCanBuy (coins: GameMan.coins, price: curItemPrice);
		} else {
			numberOfItems.maxValue = FindNumberOfItem (curSelectedItem);
			// If player sold all of item, reload display
			if (newItemNum == 0) {
				allSellItemsLoaded = false;
				OpenStoreMenu (false);
			}
		}
		// If slider value greater than max, lower it to max
		if (numberOfItems.value > numberOfItems.maxValue) {
			numberOfItems.value = numberOfItems.maxValue;
		}

	}

	IEnumerator OnTriggerExit2D(Collider2D player) {
		// Wait for small amount so player doesn't re-enter
		yield return new WaitForSeconds (0.1f);

		hasTriggered = false;
		UIMan.EndNPCMessage ();
	}
}

[System.Serializable]
public class SellableItem {
	public byte deltDexNumberRequired;
	public int cost;
	public ItemClass item;
}
