using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.GameCenter;

public class AchievementManager : MonoBehaviour {

#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)

	public static AchievementManager Inst;

	void Awake() {
		if (Inst == null) {
			Inst = this;
		} else if (Inst != this) {
			Destroy (this.gameObject);
		}
	}

	void Start () {
		// Authenticate and register a ProcessAuthentication callback
		// This call needs to be made before we can proceed to other calls in the Social API
		Social.localUser.Authenticate (ProcessAuthentication);

		// Shows GameCenter banners for iOS
		GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);
	}

	// Takes achievement name and reports progress
	public void ReportAchievement(string achievementID) {
		if (Social.localUser.authenticated) {
			Social.ReportProgress (achievementID, 100, (result) => {
				Debug.Log (result ? "Test reported!" : "Failed to report test achievement");
			});
		} else {
			Debug.Log ("Social Error: localUser is NOT authenticated!");
		}
	}

	// Update amount of Composites collected
	public void CompositeUpdate(double count) {
		Social.ReportProgress ("Composites", count, (result) => {
			if (!result) {
				Debug.Log ("Failed to composite achievement!");
			}
		});
	}

	// Achievements for gym leader battles
	public void GymLeaderBattles(string leaderName) {
		switch (leaderName) {
		case "Kane Varon": // Sigma Chi
			ReportAchievement ("Gym1");
			break;
		case "Brayden Figueroa": // Delta Sig
			ReportAchievement ("Gym2");
			break;
		case "Nick Scrivens": // Sigma Nu
			ReportAchievement ("Gym3");
			break;
		}
		// Update number of gyms defeated
		List<ItemData> allBadges = GameManager.Inst.allItems.FindAll (item => item.itemT == itemType.Badge);

		Social.ReportScore ((long)allBadges.Count, "Gyms", (result)=> {
			if (!result) {
				Debug.Log ("Failed to post battles won score!");
			}
		});
	}

	// Update Player's highest level Delt
	public void HighestLevelUpdate(long level) {
		foreach (DeltemonClass posseDelt in GameManager.Inst.deltPosse) {
			// If level is not heighest in posse
			if (level < posseDelt.level) {
				return;
			}
		}
		foreach (DeltemonData houseDelt in GameManager.Inst.houseDelts) {
			// If level is not heighest in posse
			if (level < houseDelt.level) {
				return;
			}
		}

		// If level is the highest player has, report score
		Social.ReportScore (level, "HighestLevel", (result)=> {
			if (!result) {
				Debug.Log ("Failed to post battles won score!");
			}
		});
	}

	// Score for most battles won
	public void BattlesWonUpdate(long count) {
		Social.ReportScore (count, "BattlesWon", (result)=> {
			if (!result) {
				Debug.Log ("Failed to post battles won score!");
			}
		});
	}

	// Score for most battles won
	public void DeltsRushedUpdate() {

		long count = GameManager.Inst.deltPosse.Count + GameManager.Inst.houseDelts.Count;

		Social.ReportScore (count, "DeltsRushed", (result)=> {
			if (!result) {
				Debug.Log ("Failed to post delts rushed score!");
			}
		});
	}

	// Update score for time spent in game
	public void TimeSpentUpdate(long timePlayed) {

		print ("Minutes played: " + timePlayed / 60);

		Social.ReportScore (timePlayed, "Time", (result)=> {
			if (!result) {
				Debug.Log ("Failed to post time score!");
			}
		});
	}

	// When user catches a new delt, update leaderboard
	public void UpdateDeltDexCount (double count) {

		// Score for most DeltDexes caught
		Social.ReportScore ((long)count, "DeltDex", (result)=> {
			if (!result) {
				Debug.Log ("Failed to post DeltDex score!");
			}
		});

		// Achievements for achieving certain # of dexes
		Social.ReportProgress ("10Dexes", (count*10), (result) => {
			if (!result) {
				Debug.Log ("Failed to post 10 Dex achievement!");
			}
		});
		Social.ReportProgress ("25Dexes", count*4, (result) => {
			if (!result) {
				Debug.Log ("Failed to post 25 Dex achievement!");
			}
		});
		Social.ReportProgress ("50Dexes", count*2, (result) => {
			if (!result) {
				Debug.Log ("Failed to post 50 Dex achievement!");
			}
		});
		Social.ReportProgress ("75Dexes", count*(4/3), (result) => {
			if (!result) {
				Debug.Log ("Failed to post 75 Dex achievement!");
			}
		});
		Social.ReportProgress ("AllDexes", count*0.85f, (result) => {
			if (!result) {
				Debug.Log ("Failed to post all Dex achievement!");
			}
		});
	}

	public void ShowAchievements() {
		Social.ShowAchievementsUI ();
	}

	public void ShowLeaderboard() {
		Social.ShowLeaderboardUI ();
	}

	// This function gets called when Authenticate completes
	// Note that if the operation is successful, Social.localUser will contain data from the server. 
	void ProcessAuthentication (bool success) {
		if (success) {
			Debug.Log ("Authenticated, checking achievements");

			// Request loaded achievements, and register a callback for processing them
			Social.LoadAchievements (ProcessLoadedAchievements);
		} else {
			Debug.Log ("Failed to authenticate");
		}
	}

	// This function gets called when the LoadAchievement call completes
	void ProcessLoadedAchievements (IAchievement[] achievements) {
		if (achievements.Length == 0) {
			Debug.Log ("Error: no achievements found");
		} else {
			Debug.Log ("Got " + achievements.Length + " achievements");
		}
	}
#endif
}