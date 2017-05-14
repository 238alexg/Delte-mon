using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.GameCenter;

public class AchievementManager : MonoBehaviour {

	public static AchievementManager AchieveMan;

	void Awake() {
		if (AchieveMan == null) {
			AchieveMan = this;
		} else if (AchieveMan != this) {
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

	// Score for most battles won
	public void GymsDefeatedUpdate() {
		long count = GameManager.GameMan.allItems.FindAll (item => item.itemName.Contains ("Badge")).Count;

		Social.ReportScore ((long)count, "Gyms", (result)=> {
			if (!result) {
				Debug.Log ("Failed to post battles won score!");
			}
		});
	}

	// Update Player's highest level Delt
	public void HighestLevelUpdate(long level) {
		foreach (DeltemonClass posseDelt in GameManager.GameMan.deltPosse) {
			// If level is not heighest in posse
			if (level < posseDelt.level) {
				return;
			}
		}
		foreach (DeltemonData houseDelt in GameManager.GameMan.houseDelts) {
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
	public void DeltsRushedUpdate(long count) {
		Social.ReportScore (count, "DeltsRushed", (result)=> {
			if (!result) {
				Debug.Log ("Failed to post delts rushed score!");
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
}