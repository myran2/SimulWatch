using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulWatch
{
    static class HotKeys
    {
        public static string Pause(string procName)
        {
            if (procName == "vlc")
                return " ";
            if (procName == "mpv")
                return " ";

            return " ";
        }

        public static string NextChapter(string procName)
        {
            if (procName == "vlc")
                return "+n"; //shift n
            if (procName == "mpv")
                return "{RIGHT}";

            return "{RIGHT}";
        }

        public static string PrevChapter(string procName)
        {
            if (procName == "vlc")
                return "+p"; //shift n
            if (procName == "mpv")
                return "{LEFT}";

            return "{LEFT}";
        }
    }
}
