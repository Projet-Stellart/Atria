using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atria.Scripts.IA;

public class MapDebug
{
    private static char[] chars =
    {
        '║',
        '═',
        '║',
        '═',
        '╔',
        '╚',
        '╝',
        '╗',
        '╠',
        '╩',
        '╣',
        '╦',
        '╬',
        '╬',
        '╬',
        '╬',
        '╥',
        '╞',
        '╨',
        '╡',
        '╥',
        '╞',
        '╨',
        '╡',
        '╥',
        '╡',
        '╨',
        '╞',
        '╫',
        '╪',
        '╫',
        '╪',
        '╪',
        '╫',
        '╪',
        '╫',
        ' ',
        ' ',
        ' ',
        ' ',
    };

    public static string GridToString(int[,,] grid, int layer)
    {
        string s = "";

        for (int y = 0; y < grid.GetLength(2); y++)
        {
            for (int x = 0; x < grid.GetLength(1); x++)
            {
            
                s += chars[grid[layer, x, y] - 1];
            }
            s += '\n';
        }

        return s;
    }

    public static void PrintMap(int[,,] grid)
    {
        for (int h = 0; h < grid.GetLength(0); h++)
        {
            Debug.Print("Layer " + h + ":\n" + MapDebug.GridToString(grid, h));
        }
    }
}