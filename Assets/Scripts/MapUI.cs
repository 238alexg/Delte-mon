using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BattleDelts.UI
{
    public class MapUI : UIScreen
    {

        public static MapUI Inst { get; private set; }

        public Text SelectedTownText;
        public List<Button> MapButtons;
        public GameObject Shasta;
        public GameObject Autzen;

        // Set public static instance of MapManager
        private void Awake()
        {
            if (Inst != null)
            {
                DestroyImmediate(gameObject);
                return;
            }

            Inst = this;
        }

        public override void Open()
        {
            SelectedTownText.text = "";

            // Make map button interactable for discovered scene interactions
            foreach (SceneInteractionData si in GameManager.Inst.sceneInteractions)
            {
                try
                {
                    Button curMapBut = MapButtons.Find(but => but.name == si.sceneName);
                    curMapBut.interactable = si.discovered;
                }
                catch (System.Exception e)
                {
                    // REFACTOR_TODO: Fix errors from opening up the map
                    Debug.LogError("Opening map exception: " + e);
                }
            }

            if (GameManager.Inst.sceneInteractions.Exists(si => si.sceneName == "Autzen"))
            {
                Autzen.SetActive(GameManager.Inst.sceneInteractions.Find(si => si.sceneName == "Autzen").discovered);
            }
            if (GameManager.Inst.sceneInteractions.Exists(si => si.sceneName == "Shasta"))
            {
                Shasta.SetActive(GameManager.Inst.sceneInteractions.Find(si => si.sceneName == "Shasta").discovered);
            }

            base.Open();
        }

        public override void Close()
        {
            base.Close();
            PlayerMovement.Inst.ResumeMoving();
        }

        public void DriveToLocation(TownRecoveryLocation townRecov)
        {
            Close();
            UIManager.Inst.SwitchLocationAndScene(townRecov.RecovX, townRecov.RecovY, townRecov.townName);
        }

        public void mapButtonClick(int index)
        {
            SelectedTownText.text = MapButtons[index].name;
        }

        // Switch town locations
        public void driveButtonClick()
        {
            bool canDrive = false;

            if (SelectedTownText.text != "")
            {
                foreach (DeltemonClass delt in GameManager.Inst.deltPosse)
                {
                    if (delt.moveset.Exists(move => move.moveName == "Drive"))
                    {
                        if ((delt.item != null) && (delt.item.itemName == "Car Keys"))
                        {
                            canDrive = true;
                            break;
                        }
                    }
                }

                // If Delt has the Drive move and the keys item
                if (canDrive)
                {
                    TownRecoveryLocation townRecov = GameManager.Inst.townRecovs.Find(trl => trl.townName == SelectedTownText.text);
                    if (townRecov == null)
                    {
                        Debug.Log("FATAL ERROR; TOWN RECOV DATA DOES NOT EXIST!");
                    }
                    else
                    {
                        UIManager.Inst.BagMenuUI.Open();
                        UIManager.Inst.MapUI.DriveToLocation(townRecov);
                    }
                }
                else
                {
                    UIManager.Inst.StartMessage("One of your Delts must have the Drive move and the car keys item in order to drive!");
                }
            }
        }
    }
}