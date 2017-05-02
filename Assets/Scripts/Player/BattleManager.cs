using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour {
	[Header("Battling Delts Info")]
	public DeltemonClass wildPool;
	public List <DeltemonClass> playerDelts;
	public List <DeltemonClass> oppDelts;
	public DeltemonClass curPlayerDelt;
	public DeltemonClass curOppDelt;
	byte[] PlayerStatAdditions;
	byte[] OppStatAdditions;
	bool playerBlocked, oppBlocked, PlayerDA, OppDA, actionsComplete, playerWon, finishLeveling;

	[Header("UI Info")]
	public UIManager UIManager;
	public GameManager gameManager;
	public GameObject BattleUI, MessageUI, PlayerOverview, MoveMenu, LevelUpUI, EvolveUI;
	public Color fullHealth, halfHealth, quarterHealth;
	public Sprite noStatus, daStatus;
	public Animator playerBattleAnim, oppBattleAnim;

	[Header("Player Overview UI")]
	public Image playerDeltSprite;
	public Slider playerHealth;
	public Image playerHealthBar;
	public Slider playerXP;
	public Text playerName;
	public Image playerStatus;
	public Text healthText;

	[Header("Opp Overview UI")]
	public Image oppDeltSprite;
	public Slider oppHealth;
	public Image oppHealthBar;
	public Text oppName;
	public Image oppStatus;
	public GameObject isCaught;

	[Header("Player Options")]
	public GameObject PlayerOptions;
	public List <Button> MoveOptions;
	public List <Text> moveText;

	[Header("Misc")]
	public ItemClass deltBall;
	int coinsWon;
	private bool DeltHasSwitched, forcePlayerSwitch;
	public bool isMessageToDisplay = false;
	public ItemClass activeItem;
	public AudioClip battleMusic;
	public Text levelupText;
	public Sprite porkBack;

	// Non-public
	AudioClip sceneMusic;
	ActionChoice oppChoice;
	ActionChoice playerChoice;
	List<ItemClass> trainerItems;
	string trainerName;
	NPCInteraction trainer;

	public static BattleManager BattleMan { get; private set; }

	private void Awake() {
		if (BattleMan != null) {
			DestroyImmediate(gameObject);
			return;
		}
		BattleMan = this;
	}

	void Start() {
		PlayerStatAdditions = new byte[6] { 0, 0, 0, 0, 0, 0 };
		OppStatAdditions = new byte[6] { 0, 0, 0, 0, 0, 0 };
		oppChoice = new ActionChoice ();
		playerChoice = new ActionChoice ();
		trainer = null;
		playerWon = false;
		forcePlayerSwitch = false;
	}

	// Function to initialize a new battle, for trainers and wild Delts
	public void initializeBattle() {
		// LATER: Set Enemy Podium and background
		//		BattleUI.transform.GetChild (0).gameObject.transform.GetChild (0).GetComponent<Image> ().sprite = 

		DeltHasSwitched = true;
		forcePlayerSwitch = false;
		playerWon = false;
		finishLeveling = false;
		PlayBattleMusic();

		// Clear temp battle stats for player and opponent
		ClearStats (true);
		ClearStats (false);

		// Set player delts
		playerDelts = gameManager.deltPosse;

		// Select current battling Delts, update UI
		DeltemonClass startingPlayerDelt = playerDelts.Find(delt => delt.curStatus != statusType.da);
		StartCoroutine(SwitchDelts (startingPlayerDelt, true));
	}

	// Initializes battle for a player vs. NPC battle
	public void StartTrainerBattle (NPCInteraction oppTrainer, bool isGymLeader) {
		initializeBattle ();

		trainer = oppTrainer;

		if (isGymLeader) {
			// LATER: Gym leader music.
		}
	
		// Set opp Delts going into battle
		oppDelts = trainer.oppDelts;

		// Set victory coins
		coinsWon = trainer.coins;

		// Set trainer items and name
		trainerItems = trainer.trainerItems;
		trainerName = trainer.NPCName;

		// Select current battling Delts, update UI
		StartCoroutine(SwitchDelts (oppDelts[0], false));

		// Opening trainer dialogue when battle starts
		foreach (string message in trainer.beforeBattleMessage) {
			UIManager.StartMessage (message);
		}

		// End NPC Messages and start turn
		UIManager.StartMessage (null, null, ()=>UIManager.EndNPCMessage());
		UIManager.StartMessage(null, null, ()=>TurnStart());

		// Remove NPC's notification chat bubble
		UIManager.StartMessage (null, null, ()=>UIManager.EndNPCMessage ());
	}

	// Start a battle originating from TallGrass.cs
	public void StartWildBattle (DeltemonClass oppDeltSpawn) {
		initializeBattle ();

		// Ensure Delt doesn't start with status affliction
		oppDeltSpawn.curStatus = statusType.none;
		oppDeltSpawn.statusImage = noStatus;

		StartCoroutine(SwitchDelts (oppDeltSpawn, false));

		UIManager.StartMessage("A wild " + oppDeltSpawn.deltdex.nickname + " appeared!", null, ()=>TurnStart());
	}

	void PlayBattleMusic() {
		AudioSource source = MusicManager.Instance.audiosource;
		sceneMusic = source.clip;
		source.clip = battleMusic;
		source.Play ();
	}

	// Pull up BattleMenu UI for beginning of turn
	void TurnStart() {
		PlayerOptions.SetActive (true);

		playerBlocked = false;
		oppBlocked = false;
		PlayerDA = false;
		OppDA = false;
		actionsComplete = false;
		DeltHasSwitched = false;

		// Choose Opponent Move
		if (curOppDelt.ownedByTrainer) {
			ChooseTrainerAction ();
		} else {
			ChooseWildMove ();
		}
	}

	// Trainer AI chooses Action for battle. Actions include using item, using move, or switching Delts
	void ChooseTrainerAction() {
		ItemClass chosenItem = ChooseTrainerItem ();

		// If item was chosen, use it
		if (chosenItem != null) {
			oppChoice.action = () => UseItem (false, chosenItem);
			oppChoice.type = actionT.Item;
			return;
		}

		// Choose best move for Delt
		MoveClass chosenMove = CalculateBestMove ();

		float stayInScore = 100;
		if (chosenMove != null) {
			stayInScore = CalculateStayScore (chosenMove);
			print ("Stay in score: " + stayInScore);
		}

		int bestSwitchScore;
		DeltemonClass switchIn = FindSwitchIn (out bestSwitchScore);

		print ("Switch score: " + bestSwitchScore);

		if (switchIn == null && chosenMove == null) {
			ForceOppLoss ();
		} else if (switchIn == null) {
			oppChoice.IENum = UseMove (chosenMove, false);
			oppChoice.type = actionT.Move;
		} else if (chosenMove == null) {
			oppChoice.IENum = SwitchDelts(switchIn, false);
			oppChoice.type = actionT.Switch;
		} else {
			// AI Determines if switch in is appropriate
			if (stayInScore >= bestSwitchScore) {
				oppChoice.IENum = UseMove (chosenMove, false);
				oppChoice.type = actionT.Move;
			} else {
				oppChoice.IENum = SwitchDelts(switchIn, false);
				oppChoice.type = actionT.Switch;
			}
		}
	}

	// Wild Delt move = random move from moveset
	void ChooseWildMove() {
		List<MoveClass> movesWithUses = new List<MoveClass>();
		foreach (MoveClass move in curOppDelt.moveset) {
			if (move.PPLeft > 0) {
				movesWithUses.Add (move);
			} 
		}
		if (movesWithUses.Count == 0) {
			ForceOppLoss ();
		} else {
			MoveClass randomMove = movesWithUses [Random.Range (0, movesWithUses.Count)];
			oppChoice.IENum = UseMove (randomMove, false);
			oppChoice.type = actionT.Move;
		}
	}

	// Decides if item should be used by the Trainer AI
	ItemClass ChooseTrainerItem() {
		// Decide if item is necessary
		ItemClass chosenItem = null;
		byte itemScore = 0;

		if ((curOppDelt.curStatus != statusType.none) && (curOppDelt.health < curOppDelt.GPA * 0.4f)) {
			foreach (ItemClass item in trainerItems) {
				if (item.cure == statusType.all && (item.statUpgrades [0] > 0)) {
					if (itemScore == 3) {
						if (item.statUpgrades [0] > chosenItem.statUpgrades [0]) {
							chosenItem = item;
						}
					} else {
						chosenItem = item;
						itemScore = 3;
					}
				} else if (item.statUpgrades [0] > 0) {
					if ((itemScore == 2) && (item.statUpgrades [0] > chosenItem.statUpgrades [0])) {
						chosenItem = item;
					} else if (itemScore < 2) {
						chosenItem = item;
						itemScore = 2;
					}
				} else if ((item.cure == statusType.all) && (itemScore == 0)) {
					chosenItem = item;
					itemScore = 1;
				}
			}
		} else if (curOppDelt.health < curOppDelt.GPA * 0.4f) {
			foreach (ItemClass item in trainerItems) {
				if (item.statUpgrades [0] > itemScore) {
					chosenItem = item;
					itemScore = item.statUpgrades [0];
				}
			}
		} else if (curOppDelt.curStatus != statusType.none) {
			foreach (ItemClass item in trainerItems) {
				if (item.cure == statusType.all) {
					chosenItem = item;
					break;
				}
			}
		}
		trainerItems.Remove (chosenItem);
		return chosenItem;
	}

	// Calc cumulative score for move buff
	float CalculateBuffScore(buffTuple buff) {
		float tmpBuffScore = 0;

		if (buff.buffT == buffType.Heal) {
			if (curOppDelt.health < 0.4 * curOppDelt.GPA) {
				tmpBuffScore = 2;
			} else {
				tmpBuffScore = 1;
			}

			tmpBuffScore *= buff.buffAmount;

		} else {
			byte index = 0;
			switch (buff.buffT) {
			case (buffType.Truth):
				index = 1;
				// Priority for if oppDelt has TruthAtk and it buffs oppDelt
				if (buff.isBuff && (curOppDelt.moveset.Exists (m => ((m.movType == moveType.TruthAtk) && (m.PPLeft > 0))))) {
					tmpBuffScore = 15 * buff.buffAmount;
				} 
				// Priority for if player has TruthAtk and it debuffs player
				else if (!buff.isBuff && (curPlayerDelt.moveset.Exists (m => ((m.movType == moveType.TruthAtk) && (m.PPLeft > 0))))) {
					tmpBuffScore = 15 * buff.buffAmount;
				}
				break;
			case (buffType.Courage):
				index = 2;
				// Priority for if oppDelt has powerAtk and it debuffs player
				if (!buff.isBuff && (curOppDelt.moveset.Exists (m => ((m.movType == moveType.PowerAtk) && (m.PPLeft > 0))))) {
					tmpBuffScore = 15 * buff.buffAmount;
				} 
				// Priority for if player has powerAtk and it buffs oppDelt
				else if (buff.isBuff && (curPlayerDelt.moveset.Exists (m => ((m.movType == moveType.PowerAtk) && (m.PPLeft > 0))))) {
					tmpBuffScore = 15 * buff.buffAmount;
				}
				break;
			case (buffType.Faith):
				index = 3;
				// Priority for if oppDelt has truthAtk and it debuffs player
				if (!buff.isBuff && (curOppDelt.moveset.Exists (m => ((m.movType == moveType.TruthAtk) && (m.PPLeft > 0))))) {
					tmpBuffScore = 15 * buff.buffAmount;
				} 
				// Priority for if player has truthAtk and it buffs oppDelt
				else if (buff.isBuff && (curPlayerDelt.moveset.Exists (m => ((m.movType == moveType.TruthAtk) && (m.PPLeft > 0))))) {
					tmpBuffScore = 15 * buff.buffAmount;
				}
				break;
			case (buffType.Power):
				index = 4;
				// Priority for if oppDelt has PowerAtk and it buffs oppDelt
				if (buff.isBuff && (curOppDelt.moveset.Exists (m => ((m.movType == moveType.PowerAtk) && (m.PPLeft > 0))))) {
					tmpBuffScore = 15 * buff.buffAmount;
				} 
				// Priority for if player has PowerAtk and it debuffs player
					else if (!buff.isBuff && (curPlayerDelt.moveset.Exists (m => (m.movType == moveType.PowerAtk) && (m.PPLeft > 0)))) {
					tmpBuffScore = 15 * buff.buffAmount;
				}
				break;
			case (buffType.ChillToPull):
				index = 5;
				float oppCTP = curOppDelt.ChillToPull + OppStatAdditions [index];
				float playerCTP = curPlayerDelt.ChillToPull + PlayerStatAdditions [index];

				// If opp speed is less than player's, and within an amendable range
				// Note: Buff type does not matter in this context
				if ((oppCTP < playerCTP) && (oppCTP > 0.80f * playerCTP)) {
					tmpBuffScore = 15 * buff.buffAmount;
				} 
				break;
			}

			// If a buff/debuff has already affected the Delt, lower the priority of the buff/debuff
			if (buff.isBuff && OppStatAdditions [index] > 0) {
				tmpBuffScore *= 0.8f;
			} else if (!buff.isBuff && OppStatAdditions [index] < 0) {
				tmpBuffScore *= 0.8f;
			}
		}

		return tmpBuffScore;
	}

	/* Choose best move, taking into account: 
		- Move effectiveness, power, hit chance, and crit chance. 
		- Effectiveness of buffs/debuffs and heals
		- Status effectiveness 
	*/
	public MoveClass CalculateBestMove() {
		MoveClass chosenMove = null; 
		float topScore = 0;
		float score;

		// Calculate move score for each move
		foreach (MoveClass move in curOppDelt.moveset) {
			score = 1;

			// Cannot use move if no PP left
			// If all Delt's move uses are exhausted, this will cause function to return null
			if (move.PPLeft <= 0) {
				continue;
			}

			// If opp has a blocking move
			if (move.movType == moveType.Block) {
				// If player has damaging status, add score
				if ((curPlayerDelt.curStatus == statusType.indebted) ||
					(curPlayerDelt.curStatus == statusType.roasted) ||
					(curPlayerDelt.curStatus == statusType.plagued)) {
						score = 300;
				} else {
					score = 30;
				}

				// If opponent has a damaging status, lower score
				if ((curOppDelt.curStatus == statusType.indebted) ||
				    (curOppDelt.curStatus == statusType.roasted) ||
				    (curOppDelt.curStatus == statusType.plagued)) {
					score *= 0.6f;
				}

				// Cannot use block twice in a row
				if (oppBlocked) {
					score = 0;
				}

				continue;
			} 

			// If move deals damage
			if (move.damage > 0) {

				// Set tmpScore to the base damage * effectiveness move deals
				score = moveDamage (move, curOppDelt, curPlayerDelt, false);
				score = score * moveTypeEffectivenessCalc (move.majorType, curPlayerDelt.deltdex.major1, curPlayerDelt.deltdex.major2);

				// More priority to crit chance
				score += (0.1f * move.critChance);

				// Finally, multiply score by damage and hit chance
				score *= move.damage * 0.01f * move.hitChance;
			}

			// Add score for every buff
			foreach (buffTuple buff in move.buffs) {
				score += CalculateBuffScore (buff);
			}

			// Add score if move has a status effect and player has no status
			if ((move.statType != statusType.none) && (curPlayerDelt.curStatus == statusType.none)) {
				score += (move.statusChance * 0.01f * 150);
			}

			print ("Score of move " + move.moveName + ": " + score);

			// If this move has the highest score, update top Score and chosenMove
			if (score > topScore) {
				chosenMove = move;
				topScore = score;
			}
		}

		if (chosenMove != null && chosenMove.movType == moveType.Block) {
			oppBlocked = true;
		}
		return chosenMove;
	}

	// Calculates how effective it would be to keep Delt in
	float CalculateStayScore(MoveClass chosenMove) {
		bool oppGoesFirst;
		float stayChance = 100;

		// Calculate who goes first and set oppGoesFirst variable
		if (curOppDelt.ChillToPull + (OppStatAdditions [5] * 0.1f * curOppDelt.deltdex.BVs[5]) > 
			curPlayerDelt.ChillToPull + (PlayerStatAdditions [5] * 0.1f * curPlayerDelt.deltdex.BVs[5])) {
			oppGoesFirst = true;
		} else {
			oppGoesFirst = false;
		}

		// If opponent has a status
		if (curOppDelt.curStatus != statusType.none) {
			// If move doesn't heal
			if (!chosenMove.buffs.Exists (b => b.buffT == buffType.Heal)) {

				// Lower health == lower stayChance
				if (curOppDelt.health < 0.35 * curOppDelt.GPA) {
					stayChance = 80;
				} 
				// Slightly higher GPA means higher stayChance
				else if (curOppDelt.health < 0.6 * curOppDelt.GPA) {
					stayChance = 90;
				}
			}
		}
		// If player goes first, and opp is hurt, lower stay chance
		if (!oppGoesFirst) {
			if (curOppDelt.health < curOppDelt.GPA * 0.1f) {
				stayChance *= 0.8f;
			} else if (curOppDelt.health < curOppDelt.GPA * 0.3f) {
				stayChance *= 0.9f;
			}
		}
		return stayChance;
	}

	// Find the best Delt for Trainer AI to switch into battle
	DeltemonClass FindSwitchIn(out int bestSwitchScore) {
		bestSwitchScore = -1000;
		byte switchEffectiveness = 0;
		float majorEffectiveness = 0;
		DeltemonClass switchIn = null;

		// Determine if a Delt better suited to fight curPlayerDelt exists
		foreach (DeltemonClass delt in oppDelts) {
			// Do not consider current Delt or DA'd Delts
			if ((delt.curStatus == statusType.da) || (delt == curOppDelt)) {
				continue;
			}
			// Reset score for switch in
			switchEffectiveness = 0;

			// Determine the effectiveness against current player's Delt
			foreach (MoveClass move in delt.moveset) {
				if ((move.movType == moveType.TruthAtk) || (move.movType == moveType.PowerAtk)) {
					majorEffectiveness = moveTypeEffectivenessCalc (move.majorType, curPlayerDelt.deltdex.major1, curPlayerDelt.deltdex.major2);
					if ((majorEffectiveness >= 2) && (move.PPLeft > 0)) {
						if (majorEffectiveness == 4) {
							switchEffectiveness += 50;
						} else {
							switchEffectiveness += 20;
						}
					}
				}
			}

			// Determine the effectiveness of other player's moves against it
			foreach (MoveClass move in curPlayerDelt.moveset) {
				majorEffectiveness = moveTypeEffectivenessCalc (move.majorType, curOppDelt.deltdex.major1, curOppDelt.deltdex.major2);
				if ((majorEffectiveness >= 2) && (move.PPLeft > 0)) {
					if (majorEffectiveness == 4) {
						if (switchEffectiveness < 50) {
							switchEffectiveness = 0;
						} else {
							switchEffectiveness -= 50;
						}
					} else {
						if (switchEffectiveness < 20) {
							switchEffectiveness = 0;
						} else {
							switchEffectiveness -= 20;
						}
					}
					break;
				}
			}

			// High health increases chances of switch, low health decreases
			if (delt.health > 0.8f * delt.GPA) {
				switchEffectiveness += 15;
			} else if (delt.health > 0.6f * delt.GPA) {
				switchEffectiveness += 5;
			}

			// If Delt has a status condition, less priority to switch
			if (delt.curStatus != statusType.none) {
				if (switchEffectiveness < 15) {
					switchEffectiveness = 0;
				} else {
					switchEffectiveness -= 15;
				}
			}

			// Set best possible switch
			if (switchEffectiveness > bestSwitchScore) {
				bestSwitchScore = switchEffectiveness;
				switchIn = delt;
			}
		}
		return switchIn;
	}

	// Player tries to run away
	public void Run() {
		if (curOppDelt.ownedByTrainer) {
			UIManager.StartMessage ("The other trainer looks as you as you prepare to run...");
			UIManager.StartMessage ("WHADDOYOUDO!?", null, ()=>TurnStart());
		} else {
			playerWon = true;
			UIManager.StartMessage ("The wild-eyed Delt blankly stares at you...");
			UIManager.StartMessage ("You sure showed him.", null, ()=>EndBattle ());
		}
	}

	// Player tries to switch in Delt
	public void chooseSwitchIn(DeltemonClass switchIn) {
		
		// If forcePlayerSwitch true, it is a forced switch (Delt has DA'd)
		if (DeltHasSwitched || forcePlayerSwitch) {
			StartCoroutine (SwitchDelts (switchIn, true));
			forcePlayerSwitch = false;
		} else {
			DeltHasSwitched = true;
			playerChoice.type = actionT.Switch;
			playerChoice.IENum = SwitchDelts (switchIn, true);
			StartCoroutine("fight");
		}
	}

	// Switching out Delts, loading into Battle UI, clearing temporary battle stats
	IEnumerator SwitchDelts(DeltemonClass switchIn, bool isPlayer) {
		DeltemonClass switchOut;
		Text name;
		Slider health;
		Image barColor;
		Animator spriteAnim;

		PlayerOptions.SetActive (false);

		// Set variable references for player or opponent
		if (isPlayer) {
			switchOut = curPlayerDelt;
			name = playerName;
			health = playerHealth;
			barColor = playerHealthBar;
			spriteAnim = playerDeltSprite.gameObject.GetComponent<Animator> ();
		} else {
			switchOut = curOppDelt;
			name = oppName;
			health = oppHealth;
			barColor = oppHealthBar;
			spriteAnim = oppDeltSprite.gameObject.GetComponent<Animator> ();
		}

		// Only do if not the first turn of battle
		if (isPlayer && (curPlayerDelt != null)) {
			spriteAnim.SetTrigger ("SlideOut");
			yield return new WaitForSeconds (1);
		} else if (!isPlayer && (curOppDelt != null)) {
			UIManager.StartMessage (switchIn.nickname + " has been switched in for " + switchOut.nickname, null, null, true);
			spriteAnim.SetTrigger ("SlideOut");
			yield return new WaitForSeconds (1);
		} 

		// Set current playing Delt
		if (isPlayer) {
			curPlayerDelt = switchIn;
		} else {
			curOppDelt = switchIn;
		}

		// Switch out UI name, health bar, and status
		name.text = (switchIn.nickname + "   lvl. " + switchIn.level);
		health.maxValue = switchIn.GPA;
		health.value = switchIn.health;

		// Set player/opp specific info
		if (isPlayer) {
			if (gameManager.pork) {
				healthText.text = ((int)curPlayerDelt.health + "/ PORK");
			} else {
				healthText.text = ((int)curPlayerDelt.health + "/" + (int)curPlayerDelt.GPA);
			}
			playerXP.maxValue = curPlayerDelt.XPToLevel;
			playerXP.value = curPlayerDelt.experience;
			if (gameManager.pork) {
				playerDeltSprite.sprite = porkBack;
			} else {
				playerDeltSprite.sprite = curPlayerDelt.deltdex.backImage;
			}
			playerStatus.sprite = curPlayerDelt.statusImage;
		} else {
			if (gameManager.pork) {
				oppDeltSprite.sprite = UIManager.porkSprite;
			} else {
				oppDeltSprite.sprite = curOppDelt.deltdex.frontImage;
			}
			oppStatus.sprite = curOppDelt.statusImage;
			if (gameManager.deltDex.Exists (dd => dd.nickname == curOppDelt.deltdex.nickname)) {
				isCaught.SetActive (true);
			} else {
				isCaught.SetActive (false);
			}
		}

		// Set color of health bar
		if  (switchIn.health < (0.25 * switchIn.GPA)) {
			barColor.color = quarterHealth;
		} else if (switchIn.health < (0.5 * switchIn.GPA)) {
			barColor.color = halfHealth;
		} else {
			barColor.color = fullHealth;
		}

		// Animate Delt coming in
		spriteAnim.SetTrigger ("SlideIn");

		// Wait for animation to finish
		yield return new WaitForSeconds (1);

		if (isPlayer && (switchOut != null)) {
			UIManager.StartMessage (switchIn.nickname + " has been switched in for " + switchOut.nickname, null, null, true);
		} else if (!isPlayer && (switchOut != null)) {
			UIManager.StartMessage (switchIn.nickname + " has been switched in for " + switchOut.nickname, null, null, true);
		}

		// Show player options only when switch has completed
		PlayerOptions.SetActive (true);

		// Clear the temporary stats of the Delt
		ClearStats (isPlayer);

		// Set moves, color, and PP left for each move
		MoveClass tmpMove;
		for (int i = 0; i < curPlayerDelt.moveset.Count; i++) {
			tmpMove = curPlayerDelt.moveset [i];
			// Pork option case, color = pink, PP = Porks
			if (gameManager.pork) {
				MoveOptions [i].GetComponent<Image> ().color = new Color (0.967f, 0.698f, 0.878f);
				moveText [i].GetComponent<Text> ().text = ("What is pork?!" + System.Environment.NewLine + "Porks: " + tmpMove.PPLeft + "/ PORK");
			} else {
				MoveOptions [i].GetComponent<Image> ().color = tmpMove.majorType.background;
				moveText [i].GetComponent<Text> ().text = (tmpMove.moveName + System.Environment.NewLine + "PP: " + tmpMove.PPLeft + "/" + tmpMove.PP);
			}
			if (tmpMove.PPLeft <= 0) {
				MoveOptions [i].interactable = false;
			} else {
				MoveOptions [i].interactable = true;
			}
		}
	}

	// Player tries to use an item
	void OpenBag() {
		playerChoice.action = () => ThrowBall (deltBall);
		playerChoice.type = actionT.Ball;
		// Later: implement other choices
	}
		
	// Player uses an Item on a Delt (after item validation in UIManager)
	public void UseItem(bool isPlayer, ItemClass item) {

		// If player chooses a ball, that is their action for the turn
		if (item.itemT == itemType.Ball) {
			playerChoice.action = () => ThrowBall (item);
			playerChoice.type = actionT.Ball;
			StartCoroutine (fight ());
			return;
		}

		DeltemonClass delt;
		if (isPlayer) {
			delt = curPlayerDelt;
		} else {
			delt = curOppDelt;
		}

		// Heal health
		if (item.statUpgrades [0] > 0) {
			delt.health += item.statUpgrades [0];
			UIManager.StartMessage (null,healDelt (isPlayer));
			if (delt.health >= delt.GPA) {
				delt.health = delt.GPA;
				UIManager.StartMessage (delt.nickname + "'s GPA is now full!");
			} else {
				UIManager.StartMessage (delt.nickname + "'s GPA got healed!");
			}
		}
		// Add item stat upgrades
		for (int i = 1; i < 6; i++) {
			if (item.statUpgrades [i] > 0) {
				PlayerStatAdditions [i] += item.statUpgrades [i];
				if (i == 1) {
					UIManager.StartMessage (delt.nickname + "'s Truth stat went up!");
				} else if (i == 2) {
					UIManager.StartMessage (delt.nickname + "'s Courage stat went up!");
				} else if (i == 3) {
					UIManager.StartMessage (delt.nickname + "'s Faith stat went up!");
				} else if (i == 4) {
					UIManager.StartMessage (delt.nickname + "'s Power stat went up!");
				} else {
					UIManager.StartMessage (delt.nickname + "'s ChillToPull stat went up!");
				}
			}
		}
	}

	// Player throws a ball at the opposing (hopefully wild) Delt
	public void ThrowBall(ItemClass item) {

		// If not a wild battle, Trainer bats away ball in disgust
		if (trainer != null) {
			UIManager.StartMessage ("You throw a " + item.itemName + ", but the trainer bats it away!");

			// LATER: Slide Delt out, slide trainer in

			UIManager.StartMessage ("\"What the hell, man?\"");

			// To add insult to injury
			coinsWon = (int)(coinsWon * 0.6f);

			// LATER: Slide trainer out, slide delt back in

			return;
		}

		PlayerOptions.SetActive (false);

		UIManager.StartMessage ("You threw a " + item.itemName + "!");

		float catchChance;
		float ballLevel = 1;
		float oppRarity = 1;

		switch (item.itemName) {
		case ("Geed Ball"): 
			ballLevel = 6;
			break;
		case ("Pledge Ball"):
			ballLevel = 5;
			break;
		case ("Neophyte Ball"):
			ballLevel = 4;
			break;
		case ("Member Ball"):
			ballLevel = 3;
			break;
		case ("Exec Ball"):
			ballLevel = 2;
			break;
		case ("Frat God Ball"):
			ballLevel = 1;
			break;
		}

		switch (curOppDelt.deltdex.rarity) {
		case (Rarity.VeryCommon):
			oppRarity = 1;
			break;
		case (Rarity.Common):
			oppRarity = 2;
			break;
		case (Rarity.Uncommon):
			oppRarity = 3;
			break;
		case (Rarity.Rare):
			oppRarity = 4;
			break;
		case (Rarity.VeryRare):
			oppRarity = 5;
			break;
		case (Rarity.Impossible):
			oppRarity = 6;
			break;
		case (Rarity.Legendary):
			oppRarity = 7;
			break;
		}

		// High enemy level lower catch chance (level*rarity range is 1 - 700)
		if (curOppDelt.level < 26) { // Range of (level * rarity) is 1 - 175
			catchChance = (175 - (curOppDelt.level * oppRarity)) / 175;
			print ("OG: " + catchChance);
		} else if (curOppDelt.level < 51) { // Range of (level * rarity) is 25 - 350
			catchChance = ((350 - (curOppDelt.level * oppRarity)) / 350) - 0.07f;
		} else {
			catchChance = ((700 - (curOppDelt.level * oppRarity)) / 700) - 0.07f;
		}

		// If catch chance too low, set beginning chance at 5%
		if (catchChance < 0.05f) {
			catchChance = 0.05f;
		}

		// Lower enemy health means higher catch chance
		catchChance *= ((curOppDelt.GPA - (curOppDelt.health - 1))/curOppDelt.GPA);

		// Better balls, better catch chance
		catchChance *= (20/(ballLevel + 19));

		// Convert to percentage
		catchChance *= 100;

		// Higher catch chance if enemy has a status
		if (curOppDelt.curStatus != statusType.none) {
			catchChance += (100-catchChance)/ballLevel;
		}

		float random = Random.Range (0, 100);

		// DEBUG ONLY
		print ("Catch Chance: " + catchChance + ", Random: " + random);

		// LATER: Implement shake animations
		if (random > catchChance) {
			if (random > (2 * catchChance)) {
				// No shakes
			} else if (random > (1.5 * catchChance)) {
				// 1 shake
			} else {
				// 2 shakes
			}
			UIManager.StartMessage(curOppDelt.nickname + " escaped!");
		} else {
			// 3 shakes and catch
			playerWon = true;
			UIManager.StartMessage(curOppDelt.nickname + " was caught!", null, ()=>gameManager.AddDelt (curOppDelt));
			UIManager.StartMessage(null, null, ()=>EndBattle());
		}
	}

	// Display moveset on MoveMenu UI
	public void moveMenu() {
		PlayerOptions.SetActive (false);
		MoveMenu.SetActive (true);
	}

	// Return to player battle options
	public void CloseMoveMenu() {
		MoveMenu.SetActive (false);
		PlayerOptions.SetActive (true);
	}

	// Button calls for MoveMenu
	public void chooseMove(int moveIndex) {
		MoveClass tmpMove = curPlayerDelt.moveset [moveIndex];

		// If player has uses left for this move
		if (tmpMove.PPLeft > 0) {
			
			// Do not allow player to block twice in a row
			if (tmpMove.movType == moveType.Block && playerBlocked) {
				UIManager.StartMessage ("You cannot block twice in a row!");
				return;
			}

			tmpMove.PPLeft--;
			MoveMenu.SetActive (false);
			moveText [moveIndex].GetComponent<Text> ().text = (tmpMove.moveName + System.Environment.NewLine + "PP: " + tmpMove.PPLeft + "/" + tmpMove.PP);

			// Disable button if no uses left
			if (tmpMove.PPLeft <= 0) {
				MoveOptions [moveIndex].interactable = false;
			}

			// Set player action and fight
			playerChoice.IENum = UseMove (tmpMove, true);
			playerChoice.type = actionT.Move;

			if (tmpMove.movType == moveType.Block) {
				playerBlocked = true;
			}

			StartCoroutine ("fight");
		} else {
			UIManager.StartMessage ("You don't have any more uses for this move!");
		}
	}

	// Execute fight with moves selected by player and opponent
	IEnumerator fight() {
		bool playerFirst;
		actionsComplete = false;

		PlayerOptions.SetActive (false);
		MoveMenu.SetActive (false);

		// Determine who goes first, make move
		if ((curPlayerDelt.ChillToPull + PlayerStatAdditions [5]) > (curOppDelt.ChillToPull + OppStatAdditions [5])) {
			playerFirst = true;
		} else {
			playerFirst = false;
		}

		//////////////////////////////////////////////////////////////////
		///            	  Move Priority / Loss Condition  	           ///
		//////////////////////////////////////////////////////////////////

		// If player or opponent has a non-move action
		if (playerChoice.type != actionT.Move) {
			if (playerChoice.type == actionT.Switch) {
				UIManager.StartMessage (null, playerChoice.IENum);
				yield return new WaitWhile(()=>UIManager.isMessageToDisplay);
			} else {
				playerChoice.action ();
			}
			if (!playerWon) {
				if (oppChoice.type == actionT.Switch || oppChoice.type == actionT.Move) {
					UIManager.StartMessage (null, oppChoice.IENum);
					yield return new WaitWhile (() => UIManager.isMessageToDisplay);
				} else {
					oppChoice.action ();
				}
			}
		} else if (oppChoice.type != actionT.Move) {
			if (oppChoice.type == actionT.Switch) {
				UIManager.StartMessage (null, oppChoice.IENum);
			} else {
				oppChoice.action ();
			}
			if (!playerWon) {
				if (playerChoice.type == actionT.Switch || playerChoice.type == actionT.Move) {
					UIManager.StartMessage (null, playerChoice.IENum);
					yield return new WaitWhile (() => UIManager.isMessageToDisplay);
				} else {
					playerChoice.action ();
				}
			}
		} else {
			// Else both have chosen moves, first move based on Delt CTP
			if (playerFirst) {
				if (oppBlocked) {
					UIManager.StartMessage (null, oppChoice.IENum);
					yield return new WaitWhile (() => UIManager.isMessageToDisplay);
					UIManager.StartMessage (curPlayerDelt.nickname + " was blocked!");
				} else {
					UIManager.StartMessage (null, playerChoice.IENum);
					yield return new WaitWhile (() => UIManager.isMessageToDisplay);
				}
			} else {
				if (playerBlocked) {
					UIManager.StartMessage (null, playerChoice.IENum);
					yield return new WaitWhile (() => UIManager.isMessageToDisplay);
					UIManager.StartMessage (curOppDelt.nickname + " was blocked!");
				} else {
					UIManager.StartMessage (null, oppChoice.IENum);
					yield return new WaitWhile (() => UIManager.isMessageToDisplay);
				}
			}

			// If 2nd player wasn't blocked, and neither Delt DA'd use their move
			if (!(PlayerDA || OppDA || playerBlocked || oppBlocked)) {
				// Perform second Delt's move
				if (playerFirst) {
					UIManager.StartMessage (null, oppChoice.IENum);
					yield return new WaitWhile (() => UIManager.isMessageToDisplay);
				} else {
					UIManager.StartMessage (null, playerChoice.IENum);
					yield return new WaitWhile (() => UIManager.isMessageToDisplay);
				}
			}
		}

		// Wait for all actions to complete before proceeding
		UIManager.StartMessage (null, null, () => actionsComplete = true);

		yield return new WaitUntil (() => actionsComplete);


		if (!playerWon) {
			// Post move status effect damage, etc.
			UIManager.StartMessage (null, PostMoveEffects ((playerBlocked && oppBlocked), playerFirst));
		}
	}

	// Returns true if move was a successful blocking move
	IEnumerator UseMove(MoveClass move, bool isPlayer) {
		DeltemonClass attacker;
		DeltemonClass defender;
		Animator attackerStatus;
		Animator defenderStatus;
		Animator attackerSprite;
		Animator defenderSprite;

		if (isPlayer) {
			attacker = curPlayerDelt;
			defender = curOppDelt;
			attackerStatus = playerBattleAnim;
			defenderStatus = oppBattleAnim;
			attackerSprite = playerDeltSprite.GetComponent <Animator> ();
			defenderSprite = oppDeltSprite.GetComponent <Animator> ();
		} else {
			attacker = curOppDelt;
			defender = curPlayerDelt;
			attackerStatus = oppBattleAnim;
			defenderStatus = playerBattleAnim;
			attackerSprite = oppDeltSprite.GetComponent <Animator> ();
			defenderSprite = playerDeltSprite.GetComponent <Animator> ();
		}

		//////////////////////////////////////////////////////////////////
		///                Attacker status effects                     ///
		//////////////////////////////////////////////////////////////////

		// if Delt is Drunk
		if (attacker.curStatus == statusType.drunk) {

			// LATER: DRUNK ANIMATION in null slot	----------------------------v
			yield return StartCoroutine (evolMessage (attacker.nickname + " is drunk..."));
			attackerStatus.SetTrigger ("Drunk");
			yield return new WaitForSeconds (1);

			// If Delt hurts himself
			if (Random.Range (0, 100) <= 30) {
				attacker.health = attacker.health - (attacker.GPA * 0.05f);

				attackerSprite.SetTrigger ("Hurt");
				yield return new WaitForSeconds (1);

				yield return StartCoroutine (evolMessage (attacker.nickname + " hurt itself in it's drunkeness!"));
				yield return StartCoroutine (hurtDelt (isPlayer));

				// Player DA's
				if (attacker.health <= 0) {
					attacker.health = 0;
					attacker.curStatus = statusType.da;
					yield return StartCoroutine (evolMessage (attacker.nickname + " has DA'd for being too drunk!"));
					StatusChange(isPlayer, daStatus);
					checkLoss (isPlayer);
				}
				yield break;
			}
			// Attacker relieved from drunk status
			else if (Random.Range (0, 100) <= 27) {
				yield return StartCoroutine (evolMessage (attacker.nickname + " has sobered up!"));
				StatusChange(isPlayer, noStatus);
				attacker.curStatus = statusType.none;
				attacker.statusImage = noStatus;
			}
		} 
		// If Delt is Asleep
		else if (attacker.curStatus == statusType.asleep) {
			
			yield return StartCoroutine (evolMessage (attacker.nickname + " is fast asleep."));
			// LATER: SLEEP ANIMATION

			// Delt wakes up
			if (Random.Range (0, 100) <= 20) {
				yield return StartCoroutine (evolMessage (attacker.nickname + " woke up!"));
				StatusChange(!isPlayer, noStatus);
				attacker.curStatus = statusType.none;
				attacker.statusImage = noStatus;
			} else {
				yield return StartCoroutine (evolMessage (attacker.nickname + " couldn't make that 8AM..."));
				// LATER: SLEEP ANIMATION
				yield break;
			}
		} 
		// If Delt is high
		else if (attacker.curStatus == statusType.high) {
			
			// LATER: HIGH ANIMATION in null slot	----------------------------v
			yield return StartCoroutine (evolMessage (attacker.nickname + " is high..."));

			// If Delt comes down
			if (Random.Range (0, 100) <= 21) {
				yield return StartCoroutine (evolMessage (attacker.nickname + " came down!"));
				StatusChange(!isPlayer, noStatus);
				attacker.curStatus = statusType.none;
				attacker.statusImage = noStatus;
			} else {
				// LATER: HIGH ANIMATION in null slot	-----------------------------------v
				yield return StartCoroutine (evolMessage (attacker.nickname + " is still pretty lit."));
				yield break;
			}
		}

		//////////////////////////////////////////////////////////////////
		///                 Attacker Move Outcome                      ///
		//////////////////////////////////////////////////////////////////

		// Display attack choice
		yield return StartCoroutine(evolMessage (attacker.nickname + " used " + move.moveName + "!"));

		// If the first move is a hit
		if (Random.Range(0,100) <= move.hitChance) {
			
			if (isPlayer) {
				playerDeltSprite.GetComponent <Animator> ().SetTrigger ("Attack");
			} else {
				oppDeltSprite.GetComponent <Animator> ().SetTrigger ("Attack");
			}
			yield return new WaitForSeconds (0.4f);


			if (move.movType == moveType.Block) {
				// attacker.nickname blocks!
				yield return StartCoroutine (evolMessage (attacker.nickname + " blocks!"));
				yield break;
			} 

			// If move is an attack
			if (move.damage > 0) {
				// Is an attack
				bool isCrit = false;
				float rawDamage = moveDamage (move, attacker, defender, isPlayer);
				float effectiveness;

				// If a critical hit
				if (Random.Range (0, 100) <= move.critChance) {
					rawDamage = rawDamage * 1.75f;
					isCrit = true;
				}
				// Determine major effectiveness modifier and apply to raw damage
				effectiveness = moveTypeEffectivenessCalc (move.majorType, defender.deltdex.major1, defender.deltdex.major2);
				rawDamage = rawDamage * effectiveness;

				// Multiply by random number from 0.85-1.00
				rawDamage = rawDamage * (0.01f * (float)Random.Range(85,100));

				// Return final damage
				defender.health = defender.health - rawDamage;

				if (isPlayer) {
					oppDeltSprite.GetComponent <Animator> ().SetTrigger ("Hurt");
				} else {
					playerDeltSprite.GetComponent <Animator>().SetTrigger ("Hurt");
				}
				yield return new WaitForSeconds (1);

				yield return StartCoroutine (hurtDelt (!isPlayer));

				// Messages for various effective hits
				if (effectiveness == 0) {
					yield return StartCoroutine (evolMessage ("It was about as effective Shayon's pledgeship..."));
				} else {
					if (isCrit) {
						yield return StartCoroutine (evolMessage ("It's a critical hit!"));
					}
					if (effectiveness <= 0.5f) {
						yield return StartCoroutine (evolMessage ("It's not very effective..."));
					} else if (effectiveness == 2) {
						yield return StartCoroutine (evolMessage ("It's super effective!"));
					} else if (effectiveness > 2) {
						yield return StartCoroutine (evolMessage ("It hit harder than the Shasta Trash Scandal!"));
					}
				}

				// If Delt passed out
				if (defender.health <= 0) {
					defender.health = 0;
					defender.curStatus = statusType.da;
					yield return StartCoroutine (evolMessage (defender.nickname + " has DA'd!"));
					StatusChange(!isPlayer, daStatus);
					checkLoss (!isPlayer);

					if ((isPlayer && OppDA) || (!isPlayer && PlayerDA)) {
						yield break;
					}
				} 
			}

			// declare index for de/buffs
			byte statIndex = 0;

			// Do move buffs
			foreach (buffTuple buff in move.buffs) {
				
				// Get index for stat addition
				switch (buff.buffT) {
				case buffType.Truth:
					statIndex = 1;
					break;
				case buffType.Courage:
					statIndex = 2;
					break;
				case buffType.Faith:
					statIndex = 3;
					break;
				case buffType.Power:
					statIndex = 4;
					break;
				case buffType.ChillToPull:
					statIndex = 5;
					break;
				}

				// If buff helps player
				if (buff.isBuff) {
					
					// If buff is a heal
					if (buff.buffT == buffType.Heal) {

						// Add health to Delt
						curPlayerDelt.health += buff.buffAmount;

						// Player health cannot exceed GPA
						if (curPlayerDelt.health > curPlayerDelt.GPA) {
							curPlayerDelt.health = curPlayerDelt.GPA;
						}

						// Animate heal of Delt
						yield return StartCoroutine (evolMessage (attacker.nickname + " made a deal with the Director of Academic Affairs!"));
						yield return StartCoroutine (healDelt (isPlayer));
					} 
					// If buff is a stat improvement
					else {
						
						// Add to Delt's stat additions
						if (isPlayer) {
							PlayerStatAdditions [statIndex] += buff.buffAmount;
							playerBattleAnim.SetTrigger ("Buff");
						} else {
							OppStatAdditions [statIndex] += buff.buffAmount;
							oppBattleAnim.SetTrigger ("Buff");
						}
						yield return new WaitForSeconds (0.5f);

						// Prompt message for user
						if (buff.buffAmount < 5) {
							yield return StartCoroutine (evolMessage (attacker.nickname + "'s " + buff.buffT + " stat went up!"));
						} else {
							yield return StartCoroutine (evolMessage (attacker.nickname + "'s " + buff.buffT + " stat went waaay up!"));
						}
					}
				} 
				// Else debuff hurts opponent
				else {
					// Subtract from Delt's stat additions
					if (isPlayer) {
						OppStatAdditions[statIndex] -= buff.buffAmount;
						oppBattleAnim.SetTrigger ("Debuff");
					} else {
						PlayerStatAdditions [statIndex] -= buff.buffAmount;
						playerBattleAnim.SetTrigger ("Debuff");
					}
					yield return new WaitForSeconds (0.5f);

					// Prompt message for user
					if (buff.buffAmount < 5) {
						yield return StartCoroutine (evolMessage (defender.nickname + "'s " + buff.buffT + " stat went down!"));
					} else {
						yield return StartCoroutine (evolMessage (defender.nickname + "'s " + buff.buffT + " stat went waaay down!"));
					}
				}
			}

			// If move has a status affliction and chance is met
			if ((move.statusChance > 0) && (Random.Range (0, 100) <= move.statusChance) && (defender.curStatus != move.statType)) {

				// Status animations!
				// LATER: Sound effects for each animation!!
				switch (move.statType) {
				case (statusType.drunk):
					defenderStatus.SetTrigger ("Drunk");
					break;
				case (statusType.asleep):
					print ("ASLEEP ANIM NEEDS TO BE IMPLEMENTED");
					break;
				case (statusType.high):
					print ("HIGH ANIM NEEDS TO BE IMPLEMENTED");
					break;
				case (statusType.indebted):
					defenderStatus.SetTrigger ("Indebted");
					break;
				case (statusType.plagued):
					print ("PLAGUED ANIM NEEDS TO BE IMPLEMENTED");
					break;
				case (statusType.roasted):
					print ("ROASTED ANIM NEEDS TO BE IMPLEMENTED");
					break;
				case (statusType.suspension):
					print ("SUSPENDED ANIM NEEDS TO BE IMPLEMENTED");
					break;
				}

				yield return new WaitForSeconds (1);

				// Update defender status
				defender.curStatus = move.statType;
				StatusChange(!isPlayer, move.status);

				yield return StartCoroutine (evolMessage (defender.nickname + " is now " + defender.curStatus + "!"));
			}
		} 
		// Attack missed!
		else {
			yield return StartCoroutine (evolMessage ("But " + attacker.nickname + " missed!"));
		}

		// Player loses/selects another Delt
		if (curPlayerDelt.health <= 0) { 
			checkLoss (true);
		}

		Debug.Log ("Opp health" + curOppDelt.health);

		// Opponent loses/selects another Delt
		if (curOppDelt.health <= 0) { 
			checkLoss (false);
		}
	}

	// Calculates move damage
	float moveDamage(MoveClass move, DeltemonClass attacker, DeltemonClass defender, bool isPlayer) {
		float levelDamage = (((2*(float)attacker.level) + 10))/250;
		float atkDefModifier;
		float otherMods;

		// Determine damage based on attacker and defender stats
		if (move.movType == moveType.PowerAtk) {
			if (isPlayer) {
				atkDefModifier = (float)((attacker.Power + PlayerStatAdditions [4]) / (defender.Courage + OppStatAdditions [4]));
			} else {
				atkDefModifier = (float)((attacker.Power + OppStatAdditions [4]) / (defender.Courage + PlayerStatAdditions [4]));
			}
		} else { // is moveType.TruthAtk
			if (isPlayer) {
				atkDefModifier = (float)((attacker.Truth + PlayerStatAdditions [1]) / (defender.Faith + OppStatAdditions [3]));
			} else {
				atkDefModifier = (float)((attacker.Truth + OppStatAdditions[1]) / (defender.Faith + PlayerStatAdditions[3]));
			}
		}

		// Extra damage if move is same major as Delt
		if ((move.majorType == attacker.deltdex.major1) || (move.majorType == attacker.deltdex.major2)) {
			otherMods = 1.5f;
		} else {
			otherMods = 1f;
		}

		levelDamage = (((levelDamage * atkDefModifier * move.damage) + 2) * otherMods);

		return levelDamage;
	}

	IEnumerator PostMoveEffects(bool blocked, bool playerFirst) {
		if (blocked) {
			if (playerFirst) {
				// <curPlayerDelt.nickname> blocked, but <curOppDelt.nickname> already went!
				UIManager.StartMessage (curPlayerDelt.nickname + " blocked, but " + curOppDelt.nickname + " already went!");
			} else {
				// <curOppDelt.nickname> blocked, but <curPlayerDelt.nickname> already went!
				UIManager.StartMessage (curOppDelt.nickname + " blocked, but " + curPlayerDelt.nickname + " already went!");
			}
		}


		//				 Post-Move Status Effects:					//

		// Opp gets hurt by negative status if not DA'd
		if (!OppDA) {
			if ((curOppDelt.curStatus == statusType.roasted) || (curOppDelt.curStatus == statusType.plagued)) {
				curOppDelt.health -= (curOppDelt.GPA * 0.125f);
				if (curOppDelt.curStatus == statusType.roasted) {
					UIManager.StartMessage (curOppDelt.nickname + " is hurt by it's burn!", hurtDelt (false));	
				} else if (curOppDelt.curStatus == statusType.plagued) {
					UIManager.StartMessage (curOppDelt.nickname + " is hurt by poison!", hurtDelt (false));
				}

				// If Delt passed out
				if (curOppDelt.health <= 0) {
					curOppDelt.health = 0;
					curOppDelt.curStatus = statusType.da;
					UIManager.StartMessage (curOppDelt.nickname + " has DA'd!", null, () => StatusChange (false, daStatus));

					// Opponent loses/selects another Delt
					checkLoss (false);
					if (playerWon) {
						yield break;
					}
				}
			}
		}

		// Player gets hurt by negative status if not DA'd
		if (!PlayerDA) {
			if ((curPlayerDelt.curStatus == statusType.roasted) || (curPlayerDelt.curStatus == statusType.plagued)) {
				curPlayerDelt.health -= (curPlayerDelt.GPA * 0.125f);
				if (curPlayerDelt.curStatus == statusType.roasted) {
					UIManager.StartMessage (curPlayerDelt.nickname + " is hurt by it's burn!", hurtDelt (true));
				} else if (curPlayerDelt.curStatus == statusType.plagued) {
					UIManager.StartMessage (curPlayerDelt.nickname + " is hurt by poison!", hurtDelt (true));
				}

				// If Delt passed out
				if (curPlayerDelt.health <= 0) {
					curPlayerDelt.health = 0;
					curPlayerDelt.curStatus = statusType.da;
					UIManager.StartMessage (curPlayerDelt.nickname + " has DA'd!", null, () => StatusChange (true, daStatus));

					// Player loses/selects another Delt
					checkLoss (true);
				}
			}
		}

		// DEBUG
		// UIManager.StartMessage (curPlayerDelt.nickname + "'s health is now " + curPlayerDelt.health + ", " + curOppDelt.nickname + "'s health is now " + curOppDelt.health, null, () => TurnStart());

		UIManager.StartMessage (null, null, () => TurnStart());
	}

	// Change status sprites
	void StatusChange(bool isPlayer, Sprite statusSprite) {
		if (isPlayer) {
			curPlayerDelt.statusImage = statusSprite;
			playerStatus.sprite = statusSprite;
		} else {
			curOppDelt.statusImage = statusSprite;
			oppStatus.sprite = statusSprite;
		}
	}

	// Calculate major effectiveness
	float moveTypeEffectivenessCalc(MajorClass attackerMove, MajorClass defenderM1, MajorClass defenderM2) {
		float effectiveness = 1f;
		// If attack is super effective
		if (attackerMove.veryEffective.Contains(defenderM1) || attackerMove.veryEffective.Contains(defenderM2)) {
			effectiveness *= 2;
		} // If attack is not very effective
		if (attackerMove.uneffective.Contains (defenderM1) || attackerMove.uneffective.Contains(defenderM2)) {
			effectiveness *= 0.5f;
		} // If attack does 0 damage
		if (attackerMove.zeroDamage.Contains (defenderM1) || attackerMove.zeroDamage.Contains (defenderM2)) {
			effectiveness = 0;
		}
		return effectiveness;
	}

	// Check loss condition, select new Delt if still playing
	void checkLoss(bool isPlayer) {
		if (isPlayer) {
			PlayerDA = true;
			// If playable Delt exists, switch
			foreach (DeltemonClass delt in playerDelts) {
				if (delt.curStatus != statusType.da) {
					DeltHasSwitched = true;
					forcePlayerSwitch = true;
					UIManager.StartMessage (gameManager.playerName + " must choose another Delt!", null, ()=>UIManager.OpenDeltemon(true));
					return;
				}
			}
		} else {
			// Award Action Values to the player's Delt
			if (curPlayerDelt.AVCount < 250) {
				
				curPlayerDelt.AVCount += curOppDelt.deltdex.AVAwardAmount;

				// Cap AV Count at 250
				if (curPlayerDelt.AVCount > 250) {
					curPlayerDelt.AVs [curOppDelt.deltdex.AVIndex] += (byte)(curOppDelt.deltdex.AVAwardAmount - (curPlayerDelt.AVCount - 250));
					curPlayerDelt.AVCount = 250;
				} else {
					curPlayerDelt.AVs [curOppDelt.deltdex.AVIndex] += curOppDelt.deltdex.AVAwardAmount;
				}
			}

			UIManager.StartMessage(null, awardXP ());

			OppDA = true;
			// If trainer battle, check if all Delts are DA'd
			if (trainer != null) {
				foreach (DeltemonClass delt in oppDelts) {
					// If any delts are not DA'd, find a switch in
					if (delt.curStatus != statusType.da) {
						int score = -1000;
						UIManager.StartMessage (null, SwitchDelts (FindSwitchIn (out score), false));
						return;
					}
				}
				// If exited loop, player wins
				playerWon = true;

				// Give achievements if any
				if (trainer.isGymLeader) {
					QuestManager.QuestMan.GymLeaderBattles (trainerName);

					// Heal all player Delts after defeating gym
					foreach (DeltemonClass delt in playerDelts) {
						delt.health = delt.GPA;
						delt.curStatus = statusType.none;
						delt.statusImage = noStatus;
						foreach (MoveClass move in delt.moveset) {
							move.PPLeft = move.PP;
						}
					}
				} else {
					QuestManager.QuestMan.BattleAcheivements (trainerName);
				}

				// Give player coins
				UIManager.StartMessage (gameManager.playerName + " has won " + coinsWon + " coins!", null, ()=>SoundEffectManager.SEM.PlaySoundImmediate("coinDing"));
				gameManager.coins += coinsWon;
			} 
			// If wild defeated, return Delt to the wild pool, and give player coins
			else {
				wildPool = curOppDelt;
				WildDeltCoinReward ();
				playerWon = true;

			}
		}
		UIManager.StartMessage(null, null, ()=>EndBattle ());
	}

	// Awards player XP and animates bar. Checks for Level up and presents level up window for each level up
	IEnumerator awardXP() {
		float XPNeededToLevel;
		float gainedSoFar = 0;
		float totalXPGained;
		if (trainer != null) {
			totalXPGained = 1.5f;
		} else {
			totalXPGained = 1;
		}
		totalXPGained *= 1.5f * (1550 - curOppDelt.deltdex.pinNumber);
		totalXPGained *= curOppDelt.level;
		totalXPGained *= 0.0714f;

		XPNeededToLevel = curPlayerDelt.XPToLevel;

		// Start expGain sound
		SoundEffectManager.SEM.PlaySoundImmediate ("ExpGain");

		// Animate health decrease
		while (gainedSoFar < totalXPGained) {
			gainedSoFar++;
			playerXP.value++;

			// Animation delay
			yield return new WaitForSeconds (0.001f);

			// If level up occurs
			if (playerXP.value == XPNeededToLevel) {
				Handheld.Vibrate ();

				SoundEffectManager.SEM.PlaySoundImmediate ("messageDing");

				// Perform levelup on Delt
				string[] lvlUpText = curPlayerDelt.levelUp ();

				// If the Delt's level causes it to evolve
				if (curPlayerDelt.level == curPlayerDelt.deltdex.evolveLevel) {
					Image prevEvolImage = EvolveUI.transform.GetChild (1).GetComponent <Image> ();
					Image nextEvolImage = EvolveUI.transform.GetChild (2).GetComponent <Image> ();

					// Set images for evolution animation
					prevEvolImage.sprite = curPlayerDelt.deltdex.frontImage;
					nextEvolImage.sprite = curPlayerDelt.deltdex.nextEvol.frontImage;
					EvolveUI.SetActive (true);

					// Text for before evolution animation
					if (gameManager.pork) {
						yield return StartCoroutine (evolMessage("WHAT IS PORKKENING?!?"));
						yield return StartCoroutine (evolMessage("TIME TO BECOME A HONKING BOARPIG!"));
					} else {
						yield return StartCoroutine (evolMessage("What's happening?"));
					}

					// Start evolution animation, wait to end
					EvolveUI.GetComponent <Animator>().SetBool ("Evolve", true);
					yield return new WaitForSeconds (6.5f);

					// Text for after evolution animation
					if (gameManager.pork) {
						yield return StartCoroutine (evolMessage("A NEW PORKER IS BORN!"));
						yield return StartCoroutine (evolMessage("A gush of pink bacon-smelling amneotic fluid from the evolution stains the ground."));
						yield return StartCoroutine (evolMessage("I wish this could have happened somewhere more private."));
					} else {
						yield return StartCoroutine (evolMessage(curPlayerDelt.nickname + " has evolved into " + curPlayerDelt.deltdex.nextEvol.nickname + "!"));
					}
					yield return new WaitUntil (() => UIManager.endMessage);

					// If Delt's name is not custom nicknamed by the player, make it the evolution's nickname
					if (curPlayerDelt.nickname == curPlayerDelt.deltdex.nickname) {
						curPlayerDelt.nickname = curPlayerDelt.deltdex.nextEvol.nickname;
					}

					// Set the deltdex to the evolution's deltdex
					// Note: This is how the Delt stays evolved
					curPlayerDelt.deltdex = curPlayerDelt.deltdex.nextEvol;

					// Set battle image to new image
					playerDeltSprite.sprite = curPlayerDelt.deltdex.backImage;

					// Prepare for next time Delt evolves
					EvolveUI.GetComponent <Animator>().SetBool ("Evolve", false);
					EvolveUI.SetActive (false);
				}

				// Perform level up, set LevelUp UI text
				levelupText.text = "";
				for (int i = 0; i < 7; i++) {
					levelupText.text = levelupText.text + lvlUpText [i] + System.Environment.NewLine;
				}

				// Reset XP
				playerXP.value = 0;
				playerXP.maxValue = curPlayerDelt.XPToLevel;

				// Bring health to full value
				playerHealth.maxValue = curPlayerDelt.GPA;
				playerHealth.value = curPlayerDelt.GPA;
				playerHealthBar.color = fullHealth;
				healthText.text = curPlayerDelt.GPA + " / " + curPlayerDelt.GPA;

				// Bring up Levelup UI
				LevelUpUI.SetActive (true);

				// Wait until user taps on Levelup UI to continue gaining XP
				yield return new WaitUntil (() => finishLeveling);

				SoundEffectManager.SEM.PlaySoundBlocking ("ExpGain");
				finishLeveling = false;
			}
			yield return null;
		}
		SoundEffectManager.SEM.source.Stop ();
		curPlayerDelt.experience = playerXP.value;
		UIManager.StartMessage (curPlayerDelt.nickname + " gained " + totalXPGained + " XP!", null, null, false);
	}

	// Helper IENumerator to reduce boilerplate code in awardXP (evolution)
	IEnumerator evolMessage(string message) {
		UIManager.endMessage = false;
		StartCoroutine (UIManager.displayMessage (message));
		yield return new WaitUntil (() => UIManager.endMessage);
	}

	public void LevelUpScreenQuit() {
		LevelUpUI.SetActive (false);
		finishLeveling = true;
	}

	// Calculates and message prompts user with coins won from wild battle
	void WildDeltCoinReward() {
		float multiplier;

		switch (curOppDelt.deltdex.rarity) {
		case Rarity.VeryCommon:
			multiplier = 0.1f;
			break;
		case Rarity.Common:
			multiplier = 0.2f;
			break;
		case Rarity.Uncommon:
			multiplier = 0.4f;
			break;
		case Rarity.Rare:
			multiplier = 0.8f;
			break;
		case Rarity.VeryRare:
			multiplier = 1.5f;
			break;
		default:
			multiplier = 5f;
			break;
		}

		short coins = (short)(multiplier * curOppDelt.level);
		if (coins == 0) {
			coins = 1;
		}
		gameManager.coins += coins;
		if (coins == 1) {
			UIManager.StartMessage (gameManager.playerName + " managed to pry a single coin from " + curOppDelt.nickname + " as he DA'd");
		} else if (coins < 10) {
			UIManager.StartMessage (gameManager.playerName + " pried a paltry sum of " + coins + " coins from " + curOppDelt.nickname);
		} else if (coins < 50) {
			UIManager.StartMessage (gameManager.playerName + " scored " + coins + " coins from " + curOppDelt.nickname);
		} else {
			UIManager.StartMessage (gameManager.playerName + " held " + curOppDelt.nickname + " hostage for a ransom of " + coins + " coins!");
		}
		// Add coins won to Player coins
		gameManager.coins += coins;
	}

	// Animate Health bar decreasing
	IEnumerator hurtDelt(bool isPlayer) {
		Slider healthBar;
		float health;
		float damage;
		DeltemonClass defender;

		if (isPlayer) {
			if (curPlayerDelt.health < 0) {
				curPlayerDelt.health = 0;
			}
			healthBar = playerHealth;
			health = curPlayerDelt.health;
			defender = curPlayerDelt;
		} else {
			if (curOppDelt.health < 0) {
				curOppDelt.health = 0;
			}
			healthBar = oppHealth;
			health = curOppDelt.health;
			defender = curOppDelt;
		}
		damage = healthBar.value - health;

		// Animate health decrease
		while (healthBar.value > health) {
			Debug.Log ("Healthbar: " + healthBar.value + " health: " + health);
			if (health <= 0) {
				healthBar.value = healthBar.value - (damage / 30);
			} else {
				healthBar.value = healthBar.value - (damage / 50);
			}

			// Set colors for lower health
			if (healthBar.value < (defender.GPA * 0.2)) {
				if (isPlayer && (playerHealthBar.color != quarterHealth)) {
					playerHealthBar.color = quarterHealth;
				} else if (!isPlayer && (oppHealthBar.color != quarterHealth)) {
					oppHealthBar.color = quarterHealth;
				}
			} else if (healthBar.value < (defender.GPA * 0.5)) {
				if (isPlayer && (playerHealthBar.color != halfHealth)) {
					playerHealthBar.color = halfHealth;
				} else if (!isPlayer && (oppHealthBar.color != halfHealth)) {
					oppHealthBar.color = halfHealth;
				}
			}

			// Update player health text
			if (isPlayer) {
				if (gameManager.pork) {
					healthText.text = ((int)healthBar.value + "/ PORK");
				} else {
					healthText.text = ((int)healthBar.value + "/" + (int)curPlayerDelt.GPA);
				}
			}

			// Animation delay
			yield return new WaitForSeconds (0.01f);

			// Set proper value at end of animation
			if (healthBar.value < health) {
				healthBar.value = health;
			}
			yield return null;
		}
	}

	// Animate Health bar increasing
	IEnumerator healDelt(bool isPlayer) {
		Slider healthBar;
		float health;
		float heal;
		DeltemonClass defender;

		if (isPlayer) {
			if (curPlayerDelt.health > curPlayerDelt.GPA) {
				curPlayerDelt.health = curPlayerDelt.GPA;
			}
			healthBar = playerHealth;
			health = curPlayerDelt.health;
			defender = curPlayerDelt;
		} else {
			if (curOppDelt.health > curOppDelt.GPA) {
				curOppDelt.health = curOppDelt.GPA;
			}
			healthBar = oppHealth;
			health = curOppDelt.health;
			defender = curOppDelt;
		}

		heal = healthBar.value - health;

		// Animate health decrease
		while (healthBar.value != health) {
			if (health == 0) {
				healthBar.value = healthBar.value + (heal / 30);
			} else {
				healthBar.value = healthBar.value + (heal / 50);
			}

			// Set colors for lower health
			if (healthBar.value >= (defender.GPA * 0.5)) {
				if (isPlayer && (playerHealthBar.color != quarterHealth)) {
					playerHealthBar.color = fullHealth;
				} else if (oppHealthBar.color != quarterHealth){
					oppHealthBar.color = fullHealth;
				}
			} else if (healthBar.value >= (defender.GPA * 0.2)) {
				if (isPlayer && (playerHealthBar.color != halfHealth)) {
					playerHealthBar.color = halfHealth;
				} else if (oppHealthBar.color != halfHealth) {
					oppHealthBar.color = halfHealth;
				}
			}

			// Update player health text
			if (isPlayer) {
				if (gameManager.pork) {
					healthText.text = ((int)healthBar.value + "/ PORK");
				} else {
					healthText.text = ((int)healthBar.value + "/ " + (int)curPlayerDelt.GPA);
				}
			}

			// Animation delay
			yield return new WaitForSeconds (0.01f);

			// So animation doesn't take infinite time
			if ((healthBar.value - health) < (defender.GPA / 200)) {
				healthBar.value = health;
			}
			yield return null;
		}
	}

	// Clear temp stats upon Delt switch/battle start
	void ClearStats(bool isPlayer) {
		if (isPlayer) {
			for (int i = 0; i < 6; i++) {
				PlayerStatAdditions [i] = 0;
			}
		} else {
			for (int i = 0; i < 6; i++) {
				OppStatAdditions [i] = 0;
			}
		}
	}

	// Ends the battle once a player has lost, stops battle coroutine
	void EndBattle() {
		// Clear queue of messages/actions
		UIQueueItem head = UIManager.queueHead;
		while (head.next != null) {
			UIQueueItem tmp = head.next;
			head.next = tmp.next;
		}

		// Stop battle
		StopCoroutine ("fight");

		// Play music, return to world UI
		UIManager.StartMessage(null, null, ()=>ReturnToSceneMusic());
		UIManager.StartMessage (null, null, ()=>UIManager.EndBattle());

		// Save the game
		UIManager.StartMessage(null, null, ()=>gameManager.Save());

		// Set values to null for start of next battle
		activeItem = null;
		curPlayerDelt = null;
		curOppDelt = null;
		DeltHasSwitched = false;

		// Set up animations to get ready for next slide in
//		playerDeltSprite.gameObject.GetComponent<Animator> ().Play ("Idle");
//		oppDeltSprite.gameObject.GetComponent<Animator> ().Play ("Idle");

		// If trainer battle, allow for dialogue/saving trainer defeat
		if (trainer != null && playerWon) {
			NPCInteraction tmpTrainer = trainer;
			UIManager.StartMessage (null, null, () => tmpTrainer.EndBattleActions ());
			trainer = null;
			trainerItems = null;
		} else if (playerWon) {
			// Wild battle, just start moving
			PlayerMovement.PlayMov.ResumeMoving ();
		} else {
			// Player Lost, heal their Delts and return to recov center
			UIManager.StartMessage (gameManager.playerName + " has run out of Delts!");
			foreach (DeltemonClass delt in playerDelts) {
				delt.health = delt.GPA;
				delt.curStatus = statusType.none;
				delt.statusImage = noStatus;
			}
			TownRecoveryLocation trl = gameManager.FindTownRecov ();
			UIManager.StartMessage (null, null, ()=>UIManager.SwitchLocationAndScene (trl.RecovX, trl.RecovY, trl.townName));
		}
	}

	// When opp Delts have no more PP Left
	void ForceOppLoss() {
		playerWon = true;
		if (curOppDelt.ownedByTrainer) {
			UIManager.StartMessage (trainerName + " looks at you in amazement...");
			UIManager.StartMessage ("\"My Delts... they have no more moves!\"");
			UIManager.StartMessage (gameManager.playerName + " has won " + coinsWon + " coins!");
			gameManager.coins += coinsWon;
		} else {
			// LATER: Run away sound in null slot
			UIManager.StartMessage ("Wild " + curOppDelt.nickname + " has run out of moves and ran away!");
			wildPool = curOppDelt;
		}
		UIManager.StartMessage(null, null, ()=>EndBattle ());
	}

	// Return to the music playing when player entered battle
	// LATER: Fade out battle music and play scene music
	public void ReturnToSceneMusic () {
		AudioSource source = MusicManager.Instance.audiosource;
		source.clip = sceneMusic;
		source.Play ();
	}
}

// Class to record the action choice and enum for the kind of action
class ActionChoice {
	public System.Action action;
	public IEnumerator IENum;
	public actionT type;
}

public enum actionT {
	Move,
	Item,
	Ball,
	Switch
}
