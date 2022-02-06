using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.GameCenter;

public static class AchievementManager
{
	public enum AchievementId
	{
		Gym1, // Achievements for gym leader battles
		Gym2,
		Gym3,
		Dexes10, // Achievements for collected unique delt dexes
		Dexes25,
		Dexes50,
		Dexes75,
		AllDexes
	}

	public enum ScoreId
	{
		Composites, // Update amount of Composites collected
		Gyms, // # of completed gym leader battles
		HighestLevel, // Player's highest level Delt
		BattlesWon, // Total number of battles won
		DeltsRushed, // Total number of Delts rushed
		Time, // Total time playing the game
		DeltDex // Total number of unique deltdexes collected
	}

	public static bool Enabled 
	{
		get 
		{
			switch (Application.platform)
			{
				case RuntimePlatform.IPhonePlayer: return true;
				case RuntimePlatform.Android: return true;
				default: return false;
			};	
		}
	}

	private static readonly Dictionary<AchievementId, string> AchievementNames = new Dictionary<AchievementId, string>()
	{
		[AchievementId.Gym1] = "Gym1",
		[AchievementId.Gym2] = "Gym2",
		[AchievementId.Gym3] = "Gym3",
		[AchievementId.Dexes10] = "10Dexes",
		[AchievementId.Dexes25] = "25Dexes",
		[AchievementId.Dexes50] = "50Dexes",
		[AchievementId.Dexes75] = "75Dexes",
		[AchievementId.AllDexes] = "AllDexes"
	};

	private static readonly Dictionary<ScoreId, string> ScoreNames = new Dictionary<ScoreId, string>()
	{
		[ScoreId.Composites] = "Composites",
		[ScoreId.Gyms] = "Gyms",
		[ScoreId.HighestLevel] = "HighestLevel",
		[ScoreId.BattlesWon] = "BattlesWon",
		[ScoreId.DeltsRushed] = "DeltsRushed",
		[ScoreId.Time] = "Time",
		[ScoreId.DeltDex] = "DeltDex",
	};

	public static void Authenticate()
    {
		if (Enabled)
        {
			// Authenticate and register a ProcessAuthentication callback
			// This call needs to be made before we can proceed to other calls in the Social API
			Social.localUser.Authenticate(ProcessAuthentication);

			// Shows GameCenter banners for iOS
			GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);
		}
	}

	public static void ReportAchievement(AchievementId achievement)
    {
		ReportAchievement(achievement, 100);
    }

	public static void ReportAchievement(AchievementId achievement, double percentComplete)
    {
		if (!Enabled)
		{
			return;
		}

		if (Social.localUser.authenticated)
		{
			if (AchievementNames.TryGetValue(achievement, out var achievementName))
            {
				Social.ReportProgress(achievementName, percentComplete, (result) => {
					Debug.Log(result ? "Test reported!" : "Failed to report test achievement");
				});
			}
			else
            {
				Debug.LogError($"Failed to get achievement name for: {achievement}");
            }
		}
		else
		{
			Debug.Log("Social Error: localUser is NOT authenticated!");
		}
	}

	public static void ReportScore(ScoreId score, long scoreAmount)
    {
		if (!Enabled)
        {
			return;
        }

		if (Social.localUser.authenticated)
		{
			if (ScoreNames.TryGetValue(score, out var scoreName))
			{
				Social.ReportScore(scoreAmount, scoreName, (result) => {
					Debug.Log(result ? $"{score} score reported!" : $"Failed to report {score} score");
				});
			}
			else
			{
				Debug.LogError($"Failed to get score name for: {score}");
			}
		}
		else
		{
			Debug.Log("Social Error: localUser is NOT authenticated!");
		}
	}

	public static void ShowAchievements() {
		if (Enabled)
        {
			Social.ShowAchievementsUI();
		}
	}

	public static void ShowLeaderboard() {
		if (Enabled)
        {
			Social.ShowLeaderboardUI();
		}
	}

	// This function gets called when Authenticate completes
	// Note that if the operation is successful, Social.localUser will contain data from the server. 
	private static void ProcessAuthentication (bool success) {
		if (success) {
			Debug.Log ("Authenticated, checking achievements");

			// Request loaded achievements, and register a callback for processing them
			Social.LoadAchievements (ProcessLoadedAchievements);
		} else {
			Debug.Log ("Failed to authenticate");
		}
	}

	// This function gets called when the LoadAchievement call completes
	private static void ProcessLoadedAchievements (IAchievement[] achievements) {
		if (achievements.Length == 0) {
			Debug.Log ("Error: no achievements found");
		} else {
			Debug.Log ("Got " + achievements.Length + " achievements");
		}
	}
}