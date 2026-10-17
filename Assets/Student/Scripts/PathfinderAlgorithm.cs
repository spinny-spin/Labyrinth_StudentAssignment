using NUnit.Framework.Constraints;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public static class PathfindingAlgorithm
{
    // TODO: Implement the pathfinding assignment here.
    // Find the lowest-cost path from start to goal using the map dimensions, wall costs and directed jumps exposed by IMapData.
    public static List<Vector2Int> FindShortestPath(Vector2Int start, Vector2Int goal, IMapData mapData)
    {
        // Most of your solution should be implemented in this method.

        Dictionary<Vector2Int, List<Vector2Int>> graph = BuildGraph(mapData);

        return CalcShortestPath(graph, start, goal);
    }

    static Dictionary<Vector2Int, List<Vector2Int>> BuildGraph(IMapData MD)
    {
        Dictionary<Vector2Int, List<Vector2Int>> graph = new Dictionary<Vector2Int, List<Vector2Int>>();

        for (int x = 0; x < MD.Width; x++)
        {
            for (int y = 0; y < MD.Height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                graph[pos] = new List<Vector2Int>();

                if (findRNeighbour(MD, pos))
                { 
                    Vector2Int rightNeighbour = new Vector2Int(x + 1, y);
                    graph[pos].Add(rightNeighbour);
                }

                if (findLNeighbour(MD, pos))
                {
                    Vector2Int leftNeighbour = new Vector2Int(x - 1, y);
                    graph[pos].Add(leftNeighbour);
                }

                if (findUNeighbour(MD, pos))
                {
                    Vector2Int upNeighbour = new Vector2Int(x, y + 1);
                    graph[pos].Add(upNeighbour);
                }

                if (findDNeighbour(MD, pos))
                {
                    Vector2Int downNeighbour = new Vector2Int(x, y - 1);
                    graph[pos].Add(downNeighbour);
                }

                //Tidskomplexiteten för att skapa denna graf ska vara O(Höjden * Bredden) eftersom varje position besöks bara en gång
                //och så länge vi antar att det tar konstant tid att kolla grannarna / väggar
            }
        }

        return graph;
        //Detta skapar en graf från mappens data från json filen. Mappens bredd och höjd skapar nycklarna till listorna som positionen håller
        //Dessa positioner håller en lista med vector2int som symboliserar grannarna
    }

    static List<Vector2Int> CalcShortestPath(Dictionary<Vector2Int, List<Vector2Int>> graph, Vector2Int start, Vector2Int goal)
    { 
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        HashSet<Vector2Int> alreadyVisited = new HashSet<Vector2Int>(); //Hashset är perfekt för detta, då det endast tar O(1) tid att kolla ifall ett element finns istället för att loopa igenom en lista

        Dictionary<Vector2Int, Vector2Int> cameFromWhere = new Dictionary<Vector2Int, Vector2Int>(); //Även här tar det endast O(1) tid att kolla en nyckels värde !

        queue.Enqueue(start);
        alreadyVisited.Add(start);

        while (queue.Count > 0)
        {
            Vector2Int currentVector = queue.Dequeue();

            if (currentVector == goal) break;

            List<Vector2Int> currentNeighbours = graph[currentVector];

            for(int i = 0; i < currentNeighbours.Count; i++) 
            {
                if (!alreadyVisited.Contains(currentNeighbours[i]))
                { 
                    alreadyVisited.Add(currentNeighbours[i]);
                    queue.Enqueue(currentNeighbours[i]);
                    cameFromWhere[currentNeighbours[i]] = currentVector;
                }
            }
        }

        List<Vector2Int> finalPath = new List<Vector2Int>();
        Stack<Vector2Int> stack = new Stack<Vector2Int>();

        if (!alreadyVisited.Contains(goal))
            return null; // Ingen väg till målet finns (ifall målet är omringad av väggar tex)
        

        Vector2Int current = goal; //Vi börjar på goal, sen pushar vi varje vector på en stack hela vägen till starten, då när vi kör pop så får vi hela listan i rätt ordning!!
        
        while(current != start)
        {
            stack.Push(current);
            current = cameFromWhere[current];
        }

        stack.Push(start);

        while(stack.Count > 0)
            finalPath.Add(stack.Pop());

        return finalPath;
    }

    static bool findRNeighbour(IMapData MD, Vector2Int pos)
    {
        if (pos.x + 1 < MD.Width && !MD.HasVerticalWall(pos.x + 1, pos.y))
            return true;

        return false;
    }

    static bool findLNeighbour(IMapData MD, Vector2Int pos)
    {
        if (pos.x - 1 > -1 && !MD.HasVerticalWall(pos.x, pos.y))
            return true;

        return false;
    }

    static bool findUNeighbour(IMapData MD, Vector2Int pos)
    {
        if (pos.y + 1 < MD.Height && !MD.HasHorizontalWall(pos.x, pos.y + 1))
            return true;

        return false;
    }

    static bool findDNeighbour(IMapData MD, Vector2Int pos)
    {
        if (pos.y - 1 > -1 && !MD.HasHorizontalWall(pos.x, pos.y))
            return true;

        return false;
    }

    public static bool IsMovementBlocked(Vector2Int from, Vector2Int to, IMapData mapData)
    {
        if (mapData.TryGetJumpCost(from, to, out _))
            return false;

        int deltaX = to.x - from.x;
        int deltaY = to.y - from.y;

        if (Mathf.Abs(deltaX) + Mathf.Abs(deltaY) != 1)
            return true;

        if (deltaX != 0)
        {
            int wallX = deltaX > 0 ? to.x : from.x;
            float wallCost = mapData.GetVerticalWallCost(wallX, from.y);
            return float.IsPositiveInfinity(wallCost) || wallCost >= float.MaxValue;
        }

        int wallY = deltaY > 0 ? to.y : from.y;
        float horizontalWallCost = mapData.GetHorizontalWallCost(from.x, wallY);
        return float.IsPositiveInfinity(horizontalWallCost) || horizontalWallCost >= float.MaxValue;
    }
}
