using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeltDexUI : UIScreen {

    public GameObject ListDeltemonObject, curOverviewDex;
    public Transform DeltDexOverviewUI, DexListContent;
    public Button prevEvol, nextEvol;
    public Color[] rarityColor;
    public Sprite PorkSprite; // REFACTOR_TODO: Take all pork references and put them in a static script
    bool allDexesLoaded;
    
    // Animates opening of DeltDex and populates list with All entries
    public override void Open()
    {
        if (!allDexesLoaded)
        {
            foreach (Transform child in DexListContent.transform)
            {
                Destroy(child.gameObject);
            }
            int i = 0;
            foreach (DeltDexData dexdata in GameManager.Inst.deltDex)
            {
                // REFACTOR_TODO: Make this its own GO and access set properties on a populate call
                GameObject di = Instantiate(ListDeltemonObject, DexListContent);
                di.GetComponent<Image>().color = rarityColor[(int)dexdata.rarity];
                
                Text[] texts = di.GetComponentsInChildren<Text>();

                if ((dexdata.rarity == Rarity.VeryRare) || (dexdata.rarity == Rarity.Legendary))
                {
                    texts[0].color = Color.white;
                    texts[1].color = Color.white;
                    texts[2].color = Color.white;
                }

                if (GameManager.Inst.pork)
                {
                    texts[0].text = dexdata.nickname + " Pork";
                    texts[1].text = "Pork, What is";
                    texts[2].text = "01NK";
                }
                else
                {
                    texts[0].text = dexdata.nickname;
                    texts[1].text = dexdata.actualName;
                    texts[2].text = "" + dexdata.pin;
                }

                Button b = di.transform.GetChild(3).gameObject.GetComponent<Button>();
                AddDexButtonListener(b, i);
                di.transform.localScale = Vector3.one;
                i++;
            }
            allDexesLoaded = true;
        }
        base.Open();
        DeltDexOverviewUI.gameObject.SetActiveIfChanged(false);
    }

    // When dex list item pressed, loads that delt into dex overview UI
    void AddDexButtonListener(Button b, int i)
    {
        b.onClick.AddListener(() => LoadIntoDeltdexOverview(i));
    }

    // Loads DeltDex information into Dex Overview UI
    public void LoadIntoDeltdexOverview(int i) // REFACTOR_TODO: 
    {
        DeltDexData ddd = GameManager.Inst.deltDex[i];
        curOverviewDex = (GameObject)Resources.Load("Deltemon/DeltDex/" + ddd.nickname + "DD");
        DeltDexClass dex = curOverviewDex.transform.GetComponent<DeltDexClass>();

        // Set background colors based on major
        DeltDexOverviewUI.GetComponent<Image>().color = dex.major1.background;
        if (dex.major2.majorName == "NoMajor")
        {
            DeltDexOverviewUI.GetChild(0).gameObject.SetActiveIfChanged(false);
        }
        else
        {
            DeltDexOverviewUI.GetChild(0).gameObject.GetComponent<Image>().color = dex.major2.background;
            DeltDexOverviewUI.GetChild(0).gameObject.SetActiveIfChanged(true);
        }

        if (GameManager.Inst.pork)
        {
            // Set front and back image, respectively
            DeltDexOverviewUI.GetChild(1).gameObject.GetComponent<Image>().sprite = PorkSprite;
            DeltDexOverviewUI.GetChild(2).gameObject.GetComponent<Image>().sprite = BattleManager.Inst.porkBack;
            // Set names and description
            DeltDexOverviewUI.GetChild(5).gameObject.GetComponent<Text>().text = "What is " + dex.nickname + "!?";
            DeltDexOverviewUI.GetChild(6).gameObject.GetComponent<Text>().text = "Ribbert " + dex.deltName + " Rinderson"; ;
            DeltDexOverviewUI.GetChild(7).gameObject.GetComponent<Text>().text = dex.description;
        }
        else
        {
            // Set front and back image, respectively
            DeltDexOverviewUI.GetChild(1).gameObject.GetComponent<Image>().sprite = dex.frontImage;
            DeltDexOverviewUI.GetChild(2).gameObject.GetComponent<Image>().sprite = dex.backImage;
            // Set names and description
            DeltDexOverviewUI.GetChild(5).gameObject.GetComponent<Text>().text = dex.nickname;
            DeltDexOverviewUI.GetChild(6).gameObject.GetComponent<Text>().text = dex.deltName;
            DeltDexOverviewUI.GetChild(7).gameObject.GetComponent<Text>().text = dex.description;
        }

        // Set major images
        DeltDexOverviewUI.GetChild(3).gameObject.GetComponent<Image>().sprite = dex.major1.majorImage;
        DeltDexOverviewUI.GetChild(3).gameObject.GetComponent<Image>().preserveAspect = true;
        DeltDexOverviewUI.GetChild(4).gameObject.GetComponent<Image>().sprite = dex.major2.majorImage;
        DeltDexOverviewUI.GetChild(4).gameObject.GetComponent<Image>().preserveAspect = true;
        
        // Create base values string
        short total = 0;
        string baseValues = "";
        for (int index = 0; index < 6; index++)
        {
            total += dex.BVs[index];
            baseValues += System.Environment.NewLine + dex.BVs[index];
        }
        baseValues.Insert(0, total.ToString());

        // Set stats
        DeltDexOverviewUI.GetChild(8).gameObject.GetComponent<Text>().text = baseValues;

        // Set evolution buttons, onclick to load that evolution's dex to overview
        if (dex.prevEvol != null)
        {
            prevEvol.gameObject.SetActiveIfChanged(true);
            int dexIndex = GameManager.Inst.deltDex.FindIndex(dd => dd.actualName == dex.prevEvol.deltName);
            if (dexIndex != -1)
            {
                prevEvol.transform.GetChild(0).gameObject.GetComponent<Text>().text = dex.prevEvol.nickname;
                EvolButtonListener(prevEvol, dexIndex);
            }
            else
            {
                if (GameManager.Inst.pork)
                {
                    prevEvol.transform.GetChild(0).gameObject.GetComponent<Text>().text = "What is!?";
                }
                else
                {
                    prevEvol.transform.GetChild(0).gameObject.GetComponent<Text>().text = "???";
                }
            }
        }
        else
        {
            prevEvol.gameObject.SetActiveIfChanged(false);
        }
        if (dex.nextEvol != null)
        {
            nextEvol.gameObject.SetActiveIfChanged(true);
            int dexIndex = GameManager.Inst.deltDex.FindIndex(dd => dd.actualName == dex.nextEvol.deltName);
            if (dexIndex != -1)
            {
                nextEvol.transform.GetChild(0).gameObject.GetComponent<Text>().text = dex.nextEvol.nickname;
                EvolButtonListener(nextEvol, dexIndex);
            }
            else
            {
                if (GameManager.Inst.pork)
                {
                    nextEvol.transform.GetChild(0).gameObject.GetComponent<Text>().text = "What is!?";
                }
                else
                {
                    nextEvol.transform.GetChild(0).gameObject.GetComponent<Text>().text = "???";
                }
            }
        }
        else
        {
            nextEvol.gameObject.SetActiveIfChanged(false);
        }

        // Present UI when loaded
        if (!DeltDexOverviewUI.gameObject.activeInHierarchy)
        {
            DeltDexOverviewUI.gameObject.SetActiveIfChanged(true);
        }
    }

    DeltDexData GetDeltDexData()
    {
        return null;
    }

    // Evolution buttons load that evol into Dex Overview UI
    public void EvolButtonListener(Button b, int i)
    {
        b.onClick.AddListener(() => LoadIntoDeltdexOverview(i));
    }
}
