using System.Collections.Generic;
using UnityEngine;


public static class PathfindingAlgorithm
{
    public static List<Vector2Int> FindShortestPath(Vector2Int start, Vector2Int goal, IMapData mapData)
    {
        // Most of your solution should be implemented in this method.

        Dictionary<Vector2Int, List<weightedEdge>> graph = BuildGraph(mapData);

        return CalcShortestPath(graph, start, goal);
    }

    static Dictionary<Vector2Int, List<weightedEdge>> BuildGraph(IMapData MD)
    {
        Dictionary<Vector2Int, List<weightedEdge>> graph = new Dictionary<Vector2Int, List<weightedEdge>>();

        for (int x = 0; x < MD.Width; x++)
        {
            for (int y = 0; y < MD.Height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                graph[pos] = new List<weightedEdge>();

                float rightCost = findRMovementCost(MD, pos);
                float leftCost = findLMovementCost(MD, pos);
                float upCost = findUMovementCost(MD, pos);
                float downCost = findDMovementCost(MD, pos);

                if (!float.IsPositiveInfinity(rightCost) && rightCost < float.MaxValue)
                { 
                    Vector2Int rightNeighbour = new Vector2Int(x + 1, y);
                    graph[pos].Add(new weightedEdge(rightNeighbour, rightCost));
                }

                if (!float.IsPositiveInfinity(leftCost) && leftCost < float.MaxValue)
                {
                    Vector2Int leftNeighbour = new Vector2Int(x - 1, y);
                    graph[pos].Add(new weightedEdge(leftNeighbour, leftCost));
                }

                if (!float.IsPositiveInfinity(upCost) && upCost < float.MaxValue)
                {
                    Vector2Int upNeighbour = new Vector2Int(x, y + 1);
                    graph[pos].Add(new weightedEdge(upNeighbour, upCost));
                }

                if (!float.IsPositiveInfinity(downCost) && downCost < float.MaxValue)
                {
                    Vector2Int downNeighbour = new Vector2Int(x, y - 1);
                    graph[pos].Add(new weightedEdge(downNeighbour, downCost));
                }
                //Nu ska vi hitta hopp också!
                //Lista eftersom IMapData har en funktion som ger oss en lista med hopp beroende på positionen!
                List<JumpConnection> jumps = MD.GetJumpsFrom(pos);

                //Om vi har hittat ett hopp, lägg in den i närhetslistan
                foreach (JumpConnection jump in jumps)
                    graph[pos].Add(new weightedEdge(jump.To, jump.Cost)); //Precis som med grannarna så lägger vi in vart noden är och för vilken kostnad
                //Ett hopp är enkelriktat


                //Tidskomplexiteten för att skapa denna graf ska vara O(Höjden * Bredden + Hopp) eftersom varje position besöks bara en gång men även kollar efter hopp
                //och så länge vi antar att det tar konstant tid att kolla grannarna / väggar / hopp
            }
        }

        return graph;
        //Varje position är en nyckel i grafen och dess värde är en närhetslista med viktade kanter som innehåller grannes position och kostnad
    }

    static List<Vector2Int> CalcShortestPath(Dictionary<Vector2Int, List<weightedEdge>> graph, Vector2Int start, Vector2Int goal)
    {
        Dictionary<Vector2Int, float> distance = new Dictionary<Vector2Int, float>(); //Används för att spara den kortaste distansen vi har hittat ännu till varje nod
        //Sen fyller vi den med att alla är oändliga, förutom start positionen

        foreach (Vector2Int nod in graph.Keys)
            distance[nod] = float.PositiveInfinity;

        distance[start] = 0.0f;

        Dictionary<Vector2Int, Vector2Int> cameFromWhere = new Dictionary<Vector2Int, Vector2Int>();

        OwnMinHeap queue = new OwnMinHeap();

        queue.Enqueue(start, 0f);

        while (queue.Count > 0)
        {
            OwnMinHeap.HeapEntry entry = queue.Dequeue();

            Vector2Int currentVector = entry.node;

            //Vi hoppar över ifall det finns en gammal entry som har en högre kostnad än en tidigare hittad entry
            if (entry.priority > distance[currentVector]) continue;

            if(currentVector == goal) break; //Har vi hittat målet? STOPP!!!!

            List<weightedEdge> currentEdges = graph[currentVector]; //alla edges för nuvarande noden

            // Relax: undersöker om vägen via nuvarande nod
            // ger en lägre kostnad till grannen.
            // Om ja: uppdaterar vi kortaste avståndet och previous,
            // samt lägger till grannen i min-heapen med nya kostnaden.
            foreach (weightedEdge edge in currentEdges)
            {
                Vector2Int neighbour = edge.toVector;

                float thatDistance = distance[currentVector] + edge.pathCost;

                if(thatDistance < distance[neighbour])
                {
                    distance[neighbour] = thatDistance;

                    cameFromWhere[neighbour] = currentVector;

                    queue.Enqueue(neighbour, thatDistance);
                }
            }
           
        }

        List<Vector2Int> finalPath = new List<Vector2Int>();
        Stack<Vector2Int> stack = new Stack<Vector2Int>();

        if (float.IsPositiveInfinity(distance[goal]))
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

    static float findRMovementCost(IMapData MD, Vector2Int pos)
    {
        if (pos.x + 1 >= MD.Width) //Om vi är utanför laborinten
            return float.PositiveInfinity;

        if (!MD.HasVerticalWall(pos.x + 1, pos.y)) //Om det inte finns en vägg så är det normal cost
            return 1f;

        return MD.GetVerticalWallCost(pos.x + 1, pos.y); //Om det finns en vägg kollar vi ifall den har en kostnad eller inte går att passera igenom
        //och då returneras positiva oändligheten
    }

    static float findLMovementCost(IMapData MD, Vector2Int pos)
    {
        if (pos.x - 1 < 0)
            return float.PositiveInfinity;

        if (!MD.HasVerticalWall(pos.x, pos.y))
            return 1f;

        return MD.GetVerticalWallCost(pos.x, pos.y);
    }

    static float findUMovementCost(IMapData MD, Vector2Int pos)
    {
        if (pos.y + 1 >= MD.Height)
            return float.PositiveInfinity;

        if(!MD.HasHorizontalWall(pos.x, pos.y + 1))
            return 1f;

        return MD.GetHorizontalWallCost(pos.x, pos.y + 1);
    }

    static float findDMovementCost(IMapData MD, Vector2Int pos)
    {
        if (pos.y - 1 < 0)
            return float.PositiveInfinity;

        if (!MD.HasHorizontalWall(pos.x, pos.y))
            return 1f;

        return MD.GetHorizontalWallCost(pos.x, pos.y);
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
