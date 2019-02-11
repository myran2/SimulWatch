using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;

namespace SimulWatch
{
    class CommandHandler
    {
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static Process videoPlayer = null;

        public static void ParseCommand(String command)
        {
            if (videoPlayer == null)
            {
                System.Windows.Forms.MessageBox.Show("No video player connected!");
                return;
            }

            //System.Windows.Forms.MessageBox.Show(command);
            String[] segments = command.TrimEnd('\n').Split(' ');
            String cmd = segments[0];
            Int32 time = Convert.ToInt32(segments[1]);

            if (cmd == "Pause")
            {
                HandleCommand(HotKeys.Pause(videoPlayer.ProcessName), time);
                return;
            }
            else if (cmd == "Next")
            {
                HandleCommand(HotKeys.NextChapter(videoPlayer.ProcessName), time);
                return;
            }
            else if (cmd == "Prev")
            {
                HandleCommand(HotKeys.PrevChapter(videoPlayer.ProcessName), time);
                return;
            }

            // Not a valid command, so exit
            else
            {
                System.Windows.Forms.MessageBox.Show(String.Format("Invalid command: {0}", command));
                return;
            }
        }

        public static void HandleCommand(String hotkey, Int32 executeTime)
        {
            double curTime = (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            //System.Windows.Forms.MessageBox.Show(String.Format("Wait {0} ms", Convert.ToInt32((executeTime - curTime) * 1000)));

            // Wait until the exact time that we should execute the command
            //Thread.Sleep(Convert.ToInt32((executeTime - curTime) * 1000));

            ShowWindow(videoPlayer.MainWindowHandle, 9); //SW_RESTORE from https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-showwindow
            SetForegroundWindow(videoPlayer.MainWindowHandle);

            SendKeys.SendWait(hotkey);
        }
    }
}
