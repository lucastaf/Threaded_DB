using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBRequestHandler
{
    public class ConsoleScreen
    {
        static int screenWidth = Console.WindowWidth;
        static int screenHeight = Console.WindowHeight;
        static int dividerX = Console.WindowWidth / 2;

        static int serverLine = 0;
        static int clientLine = 0;

        public ConsoleScreen()
        {
            WriteToClient("CLIENT");
            WriteToServer("SERVER");

        }

        public void WriteToServer(string message)
        {
            Console.SetCursorPosition(0, serverLine++);
            Console.Write(message.PadRight(dividerX - 1));
        }

        public void WriteToClient(string message)
        {
            Console.SetCursorPosition(dividerX + 1, clientLine++);
            Console.Write(message.PadRight(screenWidth - dividerX - 2));
        }

        public void DrawVerticalDivider()
        {
            for (int i = 0; i < screenHeight - 1; i++)
            {
                Console.SetCursorPosition(dividerX, i);
                Console.Write("|");
            }
        }
    }
}
