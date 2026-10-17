using UnityEngine;

public class path
{
    public Vector2Int toVector;
    public float pathCost;

    public path(Vector2Int toVector, float pathCost)
    { 
        this.pathCost = pathCost;
        this.toVector = toVector;
    }
}
