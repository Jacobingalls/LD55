using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class TileConfig : ScriptableObject
{
    public TileBase[] tiles;

    public bool summonable;
    public bool walkable;
    public bool looksPlaceableButIsOccupied = false;
}
