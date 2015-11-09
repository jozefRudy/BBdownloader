using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


public static class ProgressBar
{
    public static void DrawProgressBar(int complete, int maxVal, int barSize = 50, char progressCharacter = '#')
    {
        try
        {
            Console.CursorVisible = false;

            int left = Console.CursorLeft;
            decimal perc = 0;
            if (maxVal != 0)
                perc = (decimal)complete / (decimal)maxVal;
            int chars = (int)Math.Floor(perc / ((decimal)1 / (decimal)barSize));
            string p1 = String.Empty, p2 = String.Empty;

            for (int i = 0; i < chars; i++) p1 += progressCharacter;
            for (int i = 0; i < barSize - chars; i++) p2 += progressCharacter;

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(p1);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write(p2);

            Console.ResetColor();
            Console.Write(" {0}%", (perc * 100).ToString("N2"));
            Console.CursorLeft = left;
            if (complete == maxVal)
                Console.WriteLine();
        }
        catch (IOException e)
        {
            Console.WriteLine("no console available");
        }

    }
}
