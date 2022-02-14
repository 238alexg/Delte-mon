#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

public class DebugVisualizer : MonoBehaviour
{
    private const int DebugMenuWidth = 300;
    private const int Offset = 10;
    private const int GuiDefaultWidth = DebugMenuWidth - 2 * Offset;
    private const int DefaultGuiHeight = 20;


    private bool MinimizeDebugMenu = false;
    private bool ShowTallGrassZones = true;

    RectTransform PlayerTransform;

    private void Start()
    {
        PlayerTransform = (RectTransform)PlayerMovement.PlayMov.playerSprite.transform;
    }

    private void OnGUI()
    {
        if (Application.isPlaying)
        {
            if (MinimizeDebugMenu)
            {
                MinimizeDebugMenu = !GUI.Button(new Rect(0, 0, DebugMenuWidth, DefaultGuiHeight), "Show Debug Menu");
            }
            else
            {
                int heightOffset = 0;

                GUI.Box(new Rect(0, 0, 200, 200), "");
                MinimizeDebugMenu = GUI.Button(
                    new Rect(0, heightOffset, DebugMenuWidth, DefaultGuiHeight), 
                    "Hide Debug Menu");
                heightOffset += DefaultGuiHeight;

                ShowTallGrassZones = GUI.Toggle(
                    new Rect(Offset, heightOffset, GuiDefaultWidth, DefaultGuiHeight), 
                    ShowTallGrassZones, 
                    "Show Tall Grass Zones");
                heightOffset += DefaultGuiHeight;

                string currentWds = GetWdsPlayerIsIn();
                Debug.Log(currentWds);
                GUI.Label(
                    new Rect(Offset, heightOffset, GuiDefaultWidth, DefaultGuiHeight), 
                    $"Current WDS: {currentWds}");
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && ShowTallGrassZones)
        {
            if (GameManager.GameMan.Data.DeltSpawns == null)
            {
                return;
            }
            foreach (var section in GameManager.GameMan.Data.DeltSpawns.Values)
            {
                foreach (var sectionBound in section.Bounds)
                {
                    DrawBounds(sectionBound, Color.red, section.WildDeltSpawnId.ToString());
                }
            }
        }
    }

    private void DrawBounds(BoundsInt bounds, Color color, string centerText = null)
    {
        var topLeft = new Vector2(bounds.xMax, bounds.yMin);
        var bottomRight = new Vector2(bounds.xMin, bounds.yMax);

        Debug.DrawLine(bounds.min, topLeft, color);
        Debug.DrawLine(topLeft, bounds.max, color);
        Debug.DrawLine(bounds.max, bottomRight, color);
        Debug.DrawLine(bottomRight, bounds.min, color);

        if (centerText != null)
        {
            Handles.Label(bounds.center, centerText);
        }
    }

    private string GetWdsPlayerIsIn()
    {
        var playerPos = PlayerTransform.position;
        foreach(var spawn in GameManager.GameMan.Data.DeltSpawns.Values)
        {
            foreach(var section in spawn.Bounds)
            {
                if (section.xMin <= playerPos.x && section.xMax >= playerPos.x &&
                    section.yMin <= playerPos.y && section.yMax >= playerPos.y)
                {
                    return spawn.WildDeltSpawnId.ToString();
                }
            }
        }

        return "None";
    }
}

#endif