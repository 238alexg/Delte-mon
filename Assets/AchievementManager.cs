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

	// When user catches a new delt, update leaderboard
	public void UpdateDeltDexCount (int count) {
		Social.ReportScore (count, "DeltDex", (result)=> {
			if (!result) {
				Debug.Log ("Failed to post score!");
			}
		});

		// Achievements for achieving certain # of dexes
		if (count == 10) {
			ReportAchievement ("10Dexes");
		} else if (count == 25) {
			ReportAchievement ("25Dexes");
		} else if (count == 50) {
			ReportAchievement ("50Dexes");
		} else if (count == 75) {
			ReportAchievement ("75Dexes");
		} else if (count == 100) {
			ReportAchievement ("100Dexes");
		} else if (count == 115) {
			ReportAchievement ("115Dexes");
		}
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