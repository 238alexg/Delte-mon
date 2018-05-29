/*
 *	Battle Delts
 *	BattleUI.cs
 *	Copyright (c) Alex Geoffrey, 2018
 *	All Rights Reserved
 *
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BattleDelts.Battle;

namespace BattleDelts.UI
{
	public class BattleUI : MonoBehaviour
	{
        public GameObject MessageUI, PlayerOverview, MoveMenu, LevelUpUI, EvolveUI, NewMoveUI, PlayerOptions;
        public Color fullHealth, halfHealth, quarterHealth;
        public Sprite noStatus, daStatus, porkSprite, porkBack;
        
        #region REFACTOR
        // REFACTOR_TODO: Organize these by themed header
        [SerializeField] DeltInfoUI PlayerDeltInfo, OpponentDeltInfo;
        public Slider PlayerHealthBar, OppHealthBar; // REFACTOR_TODO: make this private (used in heal/hurt coroutine)
        [SerializeField] Text HealthText, LevelUpText;
        public Slider ExperienceSlider; // REFACTOR_TODO: make this private (used in awarding XP coroutine)
        [SerializeField] Image ExperienceBarBackground;
        
        [SerializeField] GameObject IsCaught;
        [SerializeField] MoveOption[] MoveOptions;
        [SerializeField] Sprite[] backgrounds;
        [SerializeField] Sprite[] podiums;

        Image PrevEvolveImage, NewEvolveImage, PlayerDeltSprite;

        BattleTurnProcess TurnProcess;
        BattleState State;
        
        // ON UPDATES: PUT FRATERNITY NAMES HERE
        // REFACTOR_TODO: Make this a property or a function
        string[] fraternityNames = { "Sigma Chi", "Delta Sigma", "Sigma Nu" };
        #endregion

        public void Initialize(BattleTurnProcess turnProcess, BattleState state)
        {
            TurnProcess = turnProcess;
            State = state;

            for (int i = 0; i < 4; i++)
            {
                MoveOptions[i].Initialize(i);
            }
        }

        public void LoadBackgroundAndPodium()
        {
            // REFACTOR_TODO: Use shaders to do background/animations instead of using these sprites
            string sceneName = GameManager.Inst.curSceneName;
            Sprite background;
            Sprite podium;

            // In a pink pork wonderland
            if (GameManager.Inst.pork)
            {
                background = backgrounds[4];
                podium = podiums[4];
            }
            // Is in the spooky DA graveyard
            else if (sceneName == "DA Graveyard")
            {
                background = backgrounds[3];
                podium = podiums[3];
            }
            // Is a fraternity 
            else if (fraternityNames.Contains(sceneName))
            {
                background = backgrounds[2];
                podium = podiums[2];
            }
            // Is a route
            else
            {
                if (Random.Range(0, 2) == 0)
                {
                    background = backgrounds[1];
                    podium = podiums[1];
                }
                else
                {
                    background = backgrounds[0];
                    podium = podiums[0];
                }
            }

            // REFACTOR_TODO: Use references for these
            // Set background and podium
            BattleManager.Inst.BattleUI.GetComponent<Image>().sprite = background;
            BattleManager.Inst.BattleUI.transform.GetChild(0).gameObject.transform.GetChild(0).GetComponent<Image>().sprite = podium;
        }

        public void PopulateBattlingDeltInfo(bool isPlayer, DeltemonClass switchIn)
        {
            DeltInfoUI deltInfo = isPlayer ? PlayerDeltInfo : OpponentDeltInfo;

            deltInfo.NameText.text = (switchIn.nickname + "   lvl. " + switchIn.level);
            deltInfo.HealthBar.maxValue = switchIn.GPA;
            deltInfo.HealthBar.value = switchIn.health;
            deltInfo.Status.sprite = switchIn.statusImage; // REFACTOR_TODO: This should not be stored in DeltemonClass
            
            if (GameManager.Inst.pork)
            {
                deltInfo.Image.sprite = isPlayer ? porkBack : porkSprite;
            }
            else
            {
                deltInfo.Image.sprite = isPlayer ? switchIn.deltdex.backImage : switchIn.deltdex.frontImage;
            }

            // Set player/opp specific info
            if (isPlayer)
            {
                if (GameManager.Inst.pork)
                {
                    deltInfo.HealthText.text = GameManager.Inst.pork ? ((int)switchIn.health + "/PORK") : ((int)switchIn.health + "/" + (int)switchIn.GPA);

                }
                deltInfo.ExperienceBar.maxValue = switchIn.XPToLevel;
                deltInfo.ExperienceBar.value = switchIn.experience;
            }
            else
            {
                SetIsCaughtIcon();
            }

            deltInfo.HealthBarImage.color = GetHealthBarColor(switchIn.health / switchIn.GPA);

            if (isPlayer) PopulateMoveset(switchIn);
        }

        public void SetIsCaughtIcon()
        {
            string OppDeltDexName = State.OpponentState.DeltInBattle.deltdex.nickname;
            foreach (DeltDexData dd in GameManager.Inst.deltDex)
            {
                if (dd.nickname == OppDeltDexName)
                {
                    IsCaught.SetActiveIfChanged(true);
                }
            }
            IsCaught.SetActiveIfChanged(false);
        }

        public void PopulateMoveset(DeltemonClass switchIn)
        {
            // Set moves, color, and PP left for each move
            for (int i = 0; i < 4; i++)
            {
                if (i < switchIn.moveset.Count)
                {
                    MoveOptions[i].Populate(switchIn.moveset[i]);
                }
                else
                {
                    MoveOptions[i].Clear();
                }
            }
        }
        
        // Flashes XP bar while recieving XP in battle
        public IEnumerator FlashXP()
        {
            // REFACTOR_TODO: Get a reference to this
            Image XPFill = ExperienceSlider.transform.GetChild(1).GetChild(0).GetComponent<Image>();
            Color normalColor = XPFill.color;

            // REFACTOR_TODO: Find a better way to do this than a coroutine?
            // Do until Coroutine is stopped
            while (true)
            {
                XPFill.color = Color.white;

                yield return new WaitForSeconds(0.3f);

                XPFill.color = normalColor;

                yield return new WaitForSeconds(0.3f);
            }
        }

        public void ShowDeltLevelUpText(string levelUpText)
        {
            // Perform level up, set LevelUp UI text
            LevelUpText.text = levelUpText;
            LevelUpUI.SetActiveIfChanged(true);
        }

        public void HideDeltLevelUpText()
        {
            LevelUpUI.SetActiveIfChanged(false);
        }

        // REFACTOR_TODO: Is this necessary?
        // New move button press starts coroutine
        //public void NewMoveButton(bool isLearn)
        //{
        //    StartCoroutine(NewMoveButtonCoroutine(isLearn));
        //}


        // User clicks Learn New Move or Don't Learn
        public void NewMoveButtonPress(bool isLearn)
        {
            // REFACTOR_TODO: Make references
            Text cancelText = NewMoveUI.transform.GetChild(5).GetChild(0).GetComponent<Text>();
            Text learnText = NewMoveUI.transform.GetChild(4).GetChild(0).GetComponent<Text>();
            DeltemonClass delt = State.PlayerState.DeltInBattle;

            // REFACTOR_TODO: Use button onPress actions instead of this awkward logic
            bool finishNewMove = false; // REFACTOR_TODO: what does this do?

            // Presses the cancel button
            if (!isLearn)
            {
                // Player affirms not learning new move
                if (cancelText.text == "Sure?")
                {
                    BattleManager.AddToBattleQueue(string.Format("{0} didn't learn the move {1}!", delt.nickname, delt.moveset[4].moveName));

                    // Remove tmp 5th new move
                    delt.moveset.RemoveAt(4);
                    NewMoveUI.SetActive(false);
                    UIManager.Inst.PosseUI.CloseMoveOverviews();
                    finishNewMove = true;
                }
                // Cancel button must be pressed again to confirm
                else
                {
                    cancelText.text = "Sure?";
                    learnText.text = "Switch" + System.Environment.NewLine + "Moves";
                }
            }
            // Presses learn new move button without selecting an old move first
            else if (!UIManager.Inst.PosseUI.MoveTwoOverview.gameObject.activeInHierarchy)
            {
                BattleManager.AddToBattleQueue("You must select a move to switch out first!");
            }
            // User tries to forget a move
            else
            {
                // Player affirms learning new move
                if (learnText.text == "Sure?")
                {

                    // Save index of move being forgotten
                    int forgetMoveIndex = UIManager.Inst.secondMoveLoaded;

                    // Set move overviews and new move ui to inactive
                    NewMoveUI.SetActive(false);
                    UIManager.Inst.PosseUI.CloseMoveOverviews();

                    BattleManager.AddToBattleQueue(string.Format("{0} forgot the move {1}!", delt.nickname, delt.moveset[forgetMoveIndex].moveName));

                    // Instantiate and learn new move
                    MoveClass newMove = Instantiate(delt.moveset[4], delt.transform);
                    delt.moveset[forgetMoveIndex] = newMove;

                    // Remove 5th move
                    delt.moveset.RemoveAt(4);

                    BattleManager.AddToBattleQueue(string.Format("{0} learned the move {1}!", delt.nickname, newMove.moveName));

                    finishNewMove = true;
                }
                // Cancel button must be pressed again to confirm
                else
                {
                    learnText.text = "Sure?";
                    cancelText.text = "Don't" + System.Environment.NewLine + "Learn";
                }
            }
        }

        public void PresentNewMoveUI(DeltemonClass playerDelt)
        {
            for (int i = 0; i < 4; i++)
            {
                MoveClass tmp = playerDelt.moveset[i];
                Transform button = NewMoveUI.transform.GetChild(i);
                button.GetComponent<Image>().color = tmp.majorType.background;
                button.GetChild(0).GetComponent<Text>().text = (tmp.moveName + System.Environment.NewLine + "PP: " + tmp.PP);
            }
            NewMoveUI.SetActive(true);
        }

        public void PresentMoveOptions()
        {
            PlayerOptions.SetActive(false);
            MoveMenu.SetActive(true);
        }

        public void HideMoveOptions()
        {
            MoveMenu.SetActive(false);
            PlayerOptions.SetActive(true);
        }

        public void RunButtonPress()
        {
            BattleManager.Inst.MoveSelection.TryRun();
        }
        
        public void UpdateHealthBarText(string text)
        {
            HealthText.text = text;
        }

        public void UpdateHealthBarColor(bool isPlayer, DeltemonClass delt)
        {
            Image healthBarBackground = isPlayer ? PlayerDeltInfo.HealthBarImage : OpponentDeltInfo.HealthBarImage;
            Color healthBarColor = GetHealthBarColor(delt.health / delt.GPA);

            if (healthBarBackground.color != healthBarColor)
            {
                healthBarBackground.color = healthBarColor;
            }
        }

        public Color GetHealthBarColor(float healthPercent)
        {
            if (healthPercent < 0.25f)
            {
                return quarterHealth;
            }
            else if (healthPercent < 0.5f)
            {
                return halfHealth;
            }
            else
            {
                return fullHealth;
            }
        }

        public void UpdateDeltStatus(bool isPlayer, statusType status)
        {
            Sprite statusImage = GetStatusImage(status);

            DeltInfoUI deltInfo = isPlayer ? PlayerDeltInfo : OpponentDeltInfo;
            deltInfo.Status.sprite = statusImage;

            // REFACTOR_TODO: Remove this
            PlayerBattleState player = isPlayer ? State.PlayerState : State.OpponentState;
            player.DeltInBattle.statusImage = statusImage;
        }

        public void UpdateTrainerPosseBalls()
        {
            // Make corresponding opponent trainer's UI Delt ball red
            int index = State.OpponentState.Delts.IndexOf(State.OpponentState.DeltInBattle);

            // REFACTOR_TODO: Add a reference to balls
            transform.GetChild(2).GetChild(4).GetChild(index).GetComponent<Image>().color = Color.red;
        }

        public void PresentEvolveUI(DeltemonClass evolvingDelt, DeltDexClass nextEvol)
        {
            Image prevEvolImage = EvolveUI.transform.GetChild(1).GetComponent<Image>();
            Image nextEvolImage = EvolveUI.transform.GetChild(2).GetComponent<Image>();

            // Set images for evolution animation
            prevEvolImage.sprite = evolvingDelt.deltdex.frontImage;
            nextEvolImage.sprite = nextEvol.frontImage;
            EvolveUI.SetActive(true);

            // Set battle image to new image
            PlayerDeltSprite.sprite = evolvingDelt.deltdex.backImage;
        }

        public void HideEvolveUI()
        { 
            EvolveUI.SetActive(false);
        }

        // REFACTOR_TODO: Either do a switch or use status to index into sprite array
        Sprite GetStatusImage(statusType status)
        {
            switch (status)
            {
                default:
                    return null;
            }
        }

        [System.Serializable]
        public class DeltInfoUI
        {
            public Text NameText;
            public Text HealthText;
            public Image Image;
            public Image Status;
            public Image HealthBarImage;
            public Slider HealthBar;
            public Slider ExperienceBar;
        }
        
        [System.Serializable]
        public class MoveOption
        {
            [System.NonSerialized] public MoveClass Move;

            [SerializeField] Button Button;
            [SerializeField] Image Background;
            [SerializeField] Text MoveText;

            int Index;

            public void Initialize(int index)
            {
                Index = index;
                Button.onClick.AddListener(() => ChooseMove(Index));
            }

            // Button calls for MoveMenu
            public void ChooseMove(int moveIndex)
            {
                BattleManager.Inst.MoveSelection.TryUseMove(Move);
            }

            public void Populate(MoveClass move)
            {
                Button.interactable = move.PPLeft > 0;

                // REFACTOR_TODO: Move this pork case somewhere?
                bool pork = GameManager.Inst.pork;
                Background.color = pork ? PorkManager.PorkColor : move.majorType.background;
                MoveText.text = pork ? "What is pork?!" + System.Environment.NewLine + "Porks: " + move.PPLeft + "/ PORK" :
                    move.moveName + System.Environment.NewLine + "PP: " + move.PPLeft + "/" + move.PP;
                Move = move;
            }

            public void Clear()
            {
                Background.color = Color.white;
                MoveText.text = "";
                Button.interactable = false;
            }
        }
    }
}