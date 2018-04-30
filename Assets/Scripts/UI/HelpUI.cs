using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BattleDelts.UI
{
    public class HelpUI : UIScreen
    {

        public Transform helpMenus, majorTabs;
        public Text helpUITitle;
        private int curHelpMenu, curMajor;

        // Animates Open of Help Menu
        public override void Open()
        {
            curHelpMenu = -1;
            curMajor = -1;
            base.Open();
        }

        // User clicks on a Menu
        public void HelpMenuButtonClick(int i)
        {
            // Remove last menu
            if (curHelpMenu != -1)
            {
                // If on the major menu, make current open 
                if ((curHelpMenu == 3) && (curMajor != -1))
                {
                    majorTabs.GetChild(curMajor).gameObject.SetActiveIfChanged(false);
                }

                root.transform.GetChild(3).GetChild(1).GetComponent<Scrollbar>().value = 1;
                helpMenus.GetChild(curHelpMenu).gameObject.SetActiveIfChanged(false);
            }

            curHelpMenu = i;

            // Get menu, set title to that menu. Set menu to active.
            GameObject helpMenu = helpMenus.GetChild(i).gameObject;
            helpUITitle.text = helpMenu.name;
            helpMenu.SetActiveIfChanged(true);
        }

        // Open Major Effectiveness Tab
        public void MajorButtonClick(int i)
        {

            // Remove last major menu
            if (curMajor != -1)
            {
                majorTabs.GetChild(curMajor).gameObject.SetActiveIfChanged(false);
            }
            majorTabs.GetChild(i + 1).gameObject.SetActiveIfChanged(true);
            curMajor = i + 1;
        }

        // Close Help Menu
        public override void Close()
        {
            // Reset Help Info to top of scrollable area
            root.transform.GetChild(3).GetChild(1).GetComponent<Scrollbar>().value = 1;

            // Remove last open help menu
            if (curHelpMenu != -1)
            {
                helpMenus.GetChild(curHelpMenu).gameObject.SetActiveIfChanged(false);
                curHelpMenu = -1;
                helpUITitle.text = "Select A Category";
            }

            base.Close();
        }

    }
}