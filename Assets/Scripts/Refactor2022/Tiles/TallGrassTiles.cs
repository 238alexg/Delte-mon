using UnityEngine;

public class TallGrassTiles : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameManager.GameMan.TileMapManager.OnTallGrassEnter();
    }
}
