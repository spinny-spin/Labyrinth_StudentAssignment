using UnityEngine;

public class weightedEdge
{
    public Vector2Int toVector;
    public float pathCost;

    public weightedEdge(Vector2Int toVector, float pathCost)
    { 
        this.pathCost = pathCost;
        this.toVector = toVector;
    }
}
