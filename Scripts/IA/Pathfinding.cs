using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;



// to implement : node.FCost, processing.Parent, start.GCost, grid[x, y, 0].Walkable, HCost
public static class Pathfinding
{

    public class CustomNode
    {
        public int X { get; }
        public int Y { get; }

        public int Z { get; }

        public bool North;
        public bool South;
        public bool East;
        public bool West;

        public int GCost;
        public int HCost;
        public int FCost => GCost + HCost;

        public CustomNode? Parent;
        public int Transition;

        public CustomNode(int x, int y, int z, TilePrefa data)
        {
            X = x;
            Y = y;
            Z = z;
            North = data.north != "space";
            South = data.south != "space";
            East = data.est != "space";
            West = data.west != "space";
            GCost = int.MaxValue;
            HCost = 0;
            Parent = null;
            Transition = data.walkTransition;
        }

        public CustomNode(int x, int y, int z, TilePrefa CompPrefa, int[,,] grid)
        {
            X = x;
            Y = y;
            Z = z;

            if (y-1 < 0)
            {
                North = false;
            }
            else
            {
                North = grid[z, x, y - 1] < 0 || CompPrefa.north != "space";
            }

            if (y + 1 >= grid.GetLength(2))
            {
                South = false;
            }
            else
            {
                South = grid[z, x, y + 1] < 0 || CompPrefa.south != "space";
            }

            if (x + 1 < 0)
            {
                East = false;
            }
            else
            {
                East = grid[z, x + 1, y] < 0 || CompPrefa.est != "space";
            }

            if (x - 1 < 0)
            {
                West = false;
            }
            else
            {
                West = grid[z, x - 1, y] < 0 || CompPrefa.west != "space";
            }

            GCost = int.MaxValue;
            HCost = 0;
            Parent = null;
            Transition = 0;
        }

        public bool Same(CustomNode? node)
        {
            if (node is null)
                return false;
            return X == node.X && Y == node.Y && Z == node.Z;
        }
    }

    private static CustomNode FindLowestValue(List<CustomNode> nodes)
    {
        if (nodes.Count == 0)
        {
            throw new ArgumentException("List is empty!");
        }

        CustomNode res = nodes[0];

        for (int i = 1; i < nodes.Count; i++)
        {
            CustomNode node = nodes[i];
            if (node.FCost < res.FCost)
            {
                res = node;
            }
        }

        return res;
    }

    public static int HeuristicCost(CustomNode a, CustomNode b)
    {
        int dx = (a.X - b.X);
        int dy = (a.Y - b.Y);
        int dz = (a.Z - b.Z);
        return ((int)Math.Round(Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz))));
    }

    private static List<Vector3I> BuildPath(CustomNode node)
    {
        if (node == null)
            return new List<Vector3I>();

        List<Vector3I> path = new List<Vector3I>();

        CustomNode? processing = node;

        while (processing != null)
        {
            path.Add(new Vector3I(processing.X, processing.Y, processing.Z));
            processing = processing.Parent;
        }

        return path;
    }

    public static Vector3I[] GetPath(Vector3I start, Vector3I goal, int[,,] grid, TilePrefa[] tileData)
    {
        CustomNode[,,] customNodes = new CustomNode[grid.GetLength(1), grid.GetLength(2), grid.GetLength(0)];

        for (int h = 0; h < grid.GetLength(0); h++)
        {
            for (int x = 0; x < grid.GetLength(1); x++)
            {
                for (int y = 0; y < grid.GetLength(2); y++)
                {
                    if (grid[h, x, y] < 0)
                    {
                        customNodes[x, y, h] = new CustomNode(x, y, h, tileData[-(grid[h, x, y] + 1)], grid);
                    }
                    else
                    {
                        customNodes[x, y, h] = new CustomNode(x, y, h, tileData[grid[h, x, y] - 1]);
                    }
                    // north c'est y -1, south y+1, west x-1, east x+1
                }
            }
        }

        CustomNode startNode;
        CustomNode endNode;

        if (grid[start.Z, start.X, start.Y] < 0)
        {
            startNode = new CustomNode(start.X, start.Y, start.Z, tileData[-(grid[start.Z, start.X, start.Y] + 1)], grid);
        }
        else
        {
            startNode = new CustomNode(start.X, start.Y, start.Z, tileData[grid[start.Z, start.X, start.Y] - 1]);
        }

        if (grid[goal.Z, goal.X, goal.Y] < 0)
        {
            endNode = new CustomNode(goal.X, goal.Y, goal.Z, tileData[-(grid[goal.Z, goal.X, goal.Y] + 1)], grid);
        }
        else
        {
            endNode = new CustomNode(goal.X, goal.Y, goal.Z, tileData[grid[goal.Z, goal.X, goal.Y] - 1]);
        }

        return AStar(customNodes, startNode, endNode).ToArray();
    }

    public static List<Vector3I> AStar(CustomNode[,,] grid, CustomNode start, CustomNode goal)
    {
        start.GCost = 0;
        start.HCost = HeuristicCost(start, goal);

        List<CustomNode> toProcess = new List<CustomNode>
        {
            start
        };

        while (toProcess.Count > 0)
        {
            CustomNode current = FindLowestValue(toProcess);

            if (current.Same(goal))
            {
                return BuildPath(current);
            }

            toProcess.Remove(current);


            for (int i = 0; i < 4; i++)
            {
                int x = current.X + (i % 2) * (i == 3 ? -1 : 1);
                int y = current.Y + ((i + 1) % 2) * (i == 2 ? -1 : 1);
                int z = current.Z;

                if (x < 0 || y < 0 || x >= grid.GetLength(0) || y >= grid.GetLength(1))
                {
                    continue;
                }

                if (i == 0)
                {
                    if (!current.South)
                    {
                        continue;
                    }
                }
                if (i == 1)
                {
                    if (!current.East)
                    {
                        continue;
                    }
                }
                if (i == 2)
                {
                    if (!current.North)
                    {
                        continue;
                    }
                }
                if (i == 3)
                {
                    if (!current.West)
                    {
                        continue;
                    }
                }

                if (grid[x, y, z].Same(goal))
                {
                    return BuildPath(current);
                }

                int tent = current.GCost + 1;

                if (tent < grid[x, y, z].GCost)
                {
                    grid[x, y, z].Parent = current;
                    if (current.Same(grid[x, y, z]))
                    {
                        throw new Exception("Invalid parent");
                    }
                    grid[x, y, z].GCost = tent;
                    grid[x, y, z].HCost = HeuristicCost(grid[x, y, z], goal);

                    if (!toProcess.Contains(grid[x, y, z]))
                    {
                        toProcess.Add(grid[x, y, z]);
                    }
                }
            }

            if (current.Transition != 0)
            {
                int x = current.X;
                int y = current.Y;
                int z = current.Z + current.Transition;

                if (grid[x, y, z].Same(goal))
                {
                    return BuildPath(current);
                }

                int tent = current.GCost + 1;

                if (tent < grid[x, y, z].GCost)
                {
                    grid[x, y, z].Parent = current;
                    if (current.Same(grid[x, y, z]))
                    {
                        throw new Exception("Invalid parent");
                    }
                    grid[x, y, z].GCost = tent;
                    grid[x, y, z].HCost = HeuristicCost(grid[x, y, z], goal);

                    if (!toProcess.Contains(grid[x, y, z]))
                    {
                        toProcess.Add(grid[x, y, z]);
                    }
                }
            }
        }

        return new List<Vector3I>();
    }

}