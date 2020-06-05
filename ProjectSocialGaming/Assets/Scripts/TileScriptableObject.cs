using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tile", menuName = "Custom/New Tile")]
public class TileScriptableObject : ScriptableObject
{
    public Transform prefab;
    public int difficulty = 0;

    public int id = -1;

    public override int GetHashCode()
    {
        return id;
    }

    public override bool Equals(object other)
    {
        return other.GetType() != typeof(TileScriptableObject) && this.GetHashCode() == ((TileScriptableObject)other).GetHashCode();
    }

    public override string ToString()
    {
        return "Tile: " + id + "\nDifficulty: " + difficulty;
    }
}
