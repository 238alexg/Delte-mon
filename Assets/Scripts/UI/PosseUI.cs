using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PosseUI : UIScreen {

    public GameObject DeltOverviewUI;
    public Transform MoveOneOverview, MoveTwoOverview;
    public List<Button> MoveOptions;
    [System.NonSerialized] public int overviewDeltIndex, firstMoveLoaded = -1, secondMoveLoaded = -1;
    DeltemonClass activeDelt;
    public Sprite noStatus;
    List<Color> rarityColor;
    
    // Load deltPosse into Deltemon UI
    public void Open(bool forceSwitchIn)
    {
        UIManager.Inst.currentUI = UIMode.Deltemon;
        DeltOverviewUI.SetActiveIfChanged(false);
        firstMoveLoaded = -1;

        int partySize = GameManager.Inst.deltPosse.Count;
        // Load in Delts
        for (int i = 0; i < 6; i++)
        {
            Transform statCube = root.transform.GetChild(i + 1);
            if (i < partySize)
            {
                DeltemonClass delt = GameManager.Inst.deltPosse[i];
                loadDeltIntoUI(delt, statCube);
            }
            else
            {
                statCube.gameObject.SetActiveIfChanged(false);
            }
        }

        // If in battle and player forced to switch, do not offer back or Give Item button
        if (forceSwitchIn)
        {
            root.transform.GetChild(7).GetChild(1).gameObject.SetActiveIfChanged(false);
            root.transform.GetChild(8).GetChild(10).gameObject.SetActiveIfChanged(false);
        }
        else
        {
            root.transform.GetChild(7).GetChild(1).gameObject.SetActiveIfChanged(true);
            root.transform.GetChild(8).GetChild(10).gameObject.SetActiveIfChanged(true);
        }

        base.Open();
    }

    // User clicks Swap Item
    public void RemoveDeltItemButtonPress()
    {
        DeltemonClass overviewDelt = GameManager.Inst.deltPosse[overviewDeltIndex];
        ItemClass removedItem = overviewDelt.item;

        overviewDelt.item = null;
        GameManager.Inst.AddItem(removedItem, 1, false);
    }

    // Delt Give Item click
    public void GiveDeltItemButtonPress()
    {

        // Remove move overviews if are up
        if (CloseMoveOverviews())
        {
            return;
        }

        // Make Delt white again (in case switch was pressed)
        root.transform.GetChild(overviewDeltIndex + 1).gameObject.GetComponent<Image>().color = Color.white;

        // Set active Delt
        activeDelt = GameManager.Inst.deltPosse[overviewDeltIndex];

        // If delt has item remove it
        if (activeDelt.item != null)
        {
            GameManager.Inst.AddItem(activeDelt.item);
            root.transform.GetChild(overviewDeltIndex + 1).transform.GetChild(4).GetComponent<Image>().sprite = noStatus;
            activeDelt.item = null;
        }

        // If item was already selected for giving
        // Note: Selected item must be holdable, megaevolve, usable, or move
        if (UIManager.Inst.ItemsUI.activeItem != null)
        {
            UIManager.Inst.ItemsUI.UseItem();
        }
        else
        {
            StartCoroutine(AnimateUIClose());
            UIManager.Inst.ItemsUI.Open();
        }
    }

    // Switch Delts in battle/order in delt posse
    public void SwitchDelt()
    {

        // Remove move overviews if are up
        if (CloseMoveOverviews())
        {
            return;
        }

        // To not confuse giving item/switching delts
        if (UIManager.Inst.ItemsUI.activeItem == null)
        {
            // Switch into battle
            if (UIManager.Inst.inBattle)
            {
                activeDelt = GameManager.Inst.deltPosse[overviewDeltIndex];
                if (activeDelt.curStatus == statusType.DA)
                {
                    UIManager.Inst.StartMessage(activeDelt.nickname + " has already DA'd!");
                }
                else if (activeDelt == BattleManager.Inst.curPlayerDelt)
                {
                    UIManager.Inst.StartMessage(activeDelt.nickname + " is already in battle!");
                }
                else
                {
                    StartCoroutine(AnimateUIClose());
                    BattleManager.Inst.chooseSwitchIn(activeDelt);
                }
                activeDelt = null;
            }
            // Select and save first delt for switching
            else
            {
                if (activeDelt == GameManager.Inst.deltPosse[overviewDeltIndex])
                {
                    activeDelt = null;
                    root.transform.GetChild(overviewDeltIndex + 1).gameObject.GetComponent<Image>().color = Color.white;
                }
                else
                {
                    activeDelt = GameManager.Inst.deltPosse[overviewDeltIndex];
                    root.transform.GetChild(overviewDeltIndex + 1).gameObject.GetComponent<Image>().color = rarityColor[1];
                }
            }
        }
        else
        {
            UIManager.Inst.StartMessage(UIManager.Inst.ItemsUI.activeItem.itemName + " has been unselected");
            UIManager.Inst.ItemsUI.activeItem = null;
        }
    }


    // Load delt into one of 6 UI stat cubes on left hand side of screen
    void loadDeltIntoUI(DeltemonClass delt, Transform statCube)
    {
        if (GameManager.Inst.pork)
        {
            statCube.transform.GetChild(1).GetComponent<Text>().text = delt.nickname + " Pork, " + delt.level;
            statCube.transform.GetChild(2).GetComponent<Image>().sprite = Pork.PorkSprite;
        }
        else
        {
            statCube.transform.GetChild(1).GetComponent<Text>().text = delt.nickname + ", " + delt.level;
            statCube.transform.GetChild(2).GetComponent<Image>().sprite = delt.deltdex.frontImage;
        }
        // Add item sprite to the info box
        if (delt.item != null)
        {
            statCube.transform.GetChild(4).GetComponent<Image>().sprite = delt.item.itemImage;
        }
        else
        {
            statCube.transform.GetChild(4).GetComponent<Image>().sprite = noStatus;
        }
        // XP Bar and Health Set
        Slider XP = statCube.GetChild(5).GetComponent<Slider>();
        Slider health = statCube.GetChild(6).GetComponent<Slider>();
        XP.maxValue = delt.XPToLevel;
        XP.value = delt.experience;
        health.maxValue = delt.GPA;
        health.value = delt.health;

        if (delt.health < (delt.GPA * 0.25))
        {
            health.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = BattleManager.Inst.quarterHealth;
        }
        else if (delt.health < (delt.GPA * 0.5))
        {
            health.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = BattleManager.Inst.halfHealth;
        }
        else
        {
            health.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = BattleManager.Inst.fullHealth;
        }
        // Add status sprite to the info box
        if (delt.curStatus != statusType.None)
        {
            statCube.GetChild(3).GetComponent<Image>().sprite = delt.statusImage;
        }
        else
        {
            statCube.GetChild(3).GetComponent<Image>().sprite = noStatus;
        }
        statCube.gameObject.SetActiveIfChanged(true);
    }

    // Load delt information into the overview panel
    public void loadDeltIntoPlayerOverview(int i)
    {

        // Remove move overviews if are up
        if (CloseMoveOverviews())
        {
            return;
        }

        // Switch order of delts in posse
        if (activeDelt != null && !UIManager.Inst.inBattle)
        {

            // If player selected a Delt that was different than the overview
            if (i != overviewDeltIndex)
            {
                // Get the Delt to switch positions with
                DeltemonClass tmp = GameManager.Inst.deltPosse[i];

                // Switch positions of the Delts in the posse
                GameManager.Inst.deltPosse[i] = activeDelt;
                GameManager.Inst.deltPosse[overviewDeltIndex] = tmp;

                // Update the overview index (since overview Delt is now in a new position)
                loadDeltIntoUI(activeDelt, root.transform.GetChild(i + 1));
                loadDeltIntoUI(tmp, root.transform.GetChild(overviewDeltIndex + 1));
            }

            // Return selected Delt to unselected color, unselect active Delt
            root.transform.GetChild(overviewDeltIndex + 1).gameObject.GetComponent<Image>().color = Color.white;
            activeDelt = null;
            overviewDeltIndex = i;
        }
        // Else selected Delt gets put into the overview
        else
        {
            overviewDeltIndex = i;
            DeltemonClass delt = GameManager.Inst.deltPosse[i];
            Text stats = DeltOverviewUI.transform.GetChild(2).GetComponent<Text>();
            Image frontSprite = DeltOverviewUI.transform.GetChild(3).GetComponent<Image>();
            Text nickname = DeltOverviewUI.transform.GetChild(4).GetComponent<Text>();
            Text actualName = DeltOverviewUI.transform.GetChild(5).GetComponent<Text>();
            Slider expBar = DeltOverviewUI.transform.GetChild(6).GetComponent<Slider>();
            Slider health = DeltOverviewUI.transform.GetChild(7).GetComponent<Slider>();

            DeltOverviewUI.GetComponent<Image>().color = delt.deltdex.major1.background;
            if (delt.deltdex.major2.majorName != "NoMajor")
            {
                DeltOverviewUI.transform.GetChild(0).gameObject.SetActiveIfChanged(true);
                DeltOverviewUI.transform.GetChild(0).GetComponent<Image>().color = delt.deltdex.major2.background;
            }
            else
            {
                DeltOverviewUI.transform.GetChild(0).gameObject.SetActiveIfChanged(false);
            }

            stats.text = "Lv. " + delt.level + System.Environment.NewLine + (int)delt.GPA + System.Environment.NewLine + (int)delt.Truth +
                System.Environment.NewLine + (int)delt.Courage + System.Environment.NewLine + (int)delt.Faith +
                System.Environment.NewLine + (int)delt.Power + System.Environment.NewLine + (int)delt.ChillToPull;

            if (GameManager.Inst.pork)
            {
                frontSprite.sprite = Pork.PorkSprite;
                nickname.text = "What is " + delt.nickname + " !?";
                actualName.text = "Loinel " + delt.deltdex.deltName + " Baconius";
            }
            else
            {
                frontSprite.sprite = delt.deltdex.frontImage;
                nickname.text = delt.nickname;
                actualName.text = delt.deltdex.deltName;
            }

            health.maxValue = delt.GPA;
            health.value = delt.health;
            expBar.maxValue = delt.XPToLevel;
            expBar.value = delt.experience;

            MoveClass tmpMove;
            // Set color and text of All Delt moves
            for (int index = 0; index < 4; index++)
            {
                if (index < delt.moveset.Count)
                {
                    tmpMove = delt.moveset[index];
                    if (GameManager.Inst.pork)
                    {
                        MoveOptions[index].GetComponent<Image>().color = new Color(0.967f, 0.698f, 0.878f);
                        MoveOptions[index].transform.GetChild(0).gameObject.GetComponent<Text>().text = ("What is pork!?" + System.Environment.NewLine + "Porks: " + tmpMove.PPLeft + "/ PORK");
                    }
                    else
                    {
                        MoveOptions[index].GetComponent<Image>().color = tmpMove.majorType.background;
                        MoveOptions[index].transform.GetChild(0).gameObject.GetComponent<Text>().text = (tmpMove.moveName + System.Environment.NewLine + "PP: " + tmpMove.PPLeft + "/" + tmpMove.PP);
                    }
                    MoveOptions[index].gameObject.SetActiveIfChanged(true);
                }
                else
                {
                    MoveOptions[index].gameObject.SetActiveIfChanged(false);
                }
            }

            Transform GiveItemButton = root.transform.GetChild(8).GetChild(10);
            if ((delt.item != null) && !UIManager.Inst.inBattle)
            {
                GiveItemButton.GetChild(0).GetComponent<Text>().text = "Swap Item";
            }
            else
            {
                GiveItemButton.GetChild(0).GetComponent<Text>().text = "Give Item";
            }

            DeltOverviewUI.SetActiveIfChanged(true);
        }
    }

    // Prepare MoveOneOverview with new move info
    public void SetLevelUpMove(MoveClass newMove, DeltemonClass curPlayerDelt)
    {
        overviewDeltIndex = GameManager.Inst.deltPosse.IndexOf(curPlayerDelt);

        // Temporarily add new move as 5th move
        curPlayerDelt.moveset.Add(newMove);

        MoveClick(4);
    }

    // Called on button down press on move from Delt's moveset
    public void MoveClick(int index)
    {
        Transform MoveOverview;
        MoveClass move = GameManager.Inst.deltPosse[overviewDeltIndex].moveset[index];

        // If move is already displayed, remove it
        if (firstMoveLoaded == index)
        {
            MoveOneOverview.gameObject.SetActiveIfChanged(false);
            firstMoveLoaded = -1;
            return;
        }
        else if (secondMoveLoaded == index)
        {
            MoveTwoOverview.gameObject.SetActiveIfChanged(false);
            secondMoveLoaded = -1;
            return;
        }

        // If first move overview already loaded, load into 2nd move overview
        if (firstMoveLoaded != -1)
        {
            MoveOverview = MoveTwoOverview;
            secondMoveLoaded = index;
        }
        else
        {
            MoveOverview = MoveOneOverview;
            firstMoveLoaded = index;
        }

        MoveOverview.GetComponent<Image>().color = move.majorType.background;
        MoveOverview.GetChild(0).GetComponent<Image>().sprite = move.majorType.majorImage;

        if (move.statType != statusType.None)
        {
            MoveOverview.GetChild(1).gameObject.SetActiveIfChanged(true);
            MoveOverview.GetChild(1).GetComponent<Image>().sprite = move.status;
        }
        else
        {
            MoveOverview.GetChild(1).gameObject.SetActiveIfChanged(false);
        }

        MoveOverview.GetChild(2).GetComponent<Text>().text = move.moveName;
        MoveOverview.GetChild(3).GetComponent<Text>().text = "" + move.movType;
        MoveOverview.GetChild(4).GetComponent<Text>().text = move.moveDescription;

        MoveOverview.GetChild(5).GetComponent<Text>().text = move.PP + System.Environment.NewLine + move.damage + System.Environment.NewLine +
            move.hitChance + System.Environment.NewLine + move.statType + System.Environment.NewLine + move.statusChance;

        MoveOverview.gameObject.SetActiveIfChanged(true);
    }

    // Remove move overviews if are up and return true
    public bool CloseMoveOverviews()
    {
        if ((firstMoveLoaded != -1) || (secondMoveLoaded != -1))
        {
            firstMoveLoaded = -1;
            secondMoveLoaded = -1;
            MoveOneOverview.gameObject.SetActiveIfChanged(false);
            MoveTwoOverview.gameObject.SetActiveIfChanged(false);
            return true;
        }
        return false;
    }

    // Close deltemon UI
    public void CloseDeltemon()
    {

        // Remove move overviews if are up
        if (CloseMoveOverviews())
        {
            return;
        }

        StartCoroutine(AnimateUIClose());
        activeDelt = null;
        UIManager.Inst.ItemsUI.activeItem = null;
    }
}
