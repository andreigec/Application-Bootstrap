using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ANDREICSLIB.ClassExtras;
using ANDREICSLIB.Helpers;
using ANDREICSLIB.Licensing;

namespace ApplicationBootstrap
{
    public static class Controller
    {
        public static AssemblyValues v;
        private const String HelpString = "";

        private static readonly String OtherText =
            @"©" + DateTime.Now.Year +
            @" Andrei Gec (http://www.andreigec.net)

Licensed under GNU LGPL (http://www.gnu.org/)

Zip Assets © SharpZipLib (http://www.sharpdevelop.net/OpenSource/SharpZipLib/)
";

        public enum Operation
        {
            None = -1,
            Min = 2,
            Max = 3,
            Normal = 1,
            Hide = 0,
            Close = 4
        }

        //this console ptr
        private static IntPtr hWnd;

        private static void printInstructions(bool printIntro)
        {

            ShowWindow(hWnd, (int)Operation.Normal);
            String text = "";
            if (printIntro)
            {
                text = "\n" + v.GetAppString() + " developed by Andrei Gec(andreigec.net)";
                text +=
                    "\nPurpose:forces an application to minimise or maximise on startup\nUseful for applications that disregard the shortcut option that is supposed to do the same";
            }

            text += "\n\nApplicationBootstrap.exe [\"path\"] [/min] [/max] [/normal] [/close]"
                    + "\n\npath\tpath to executable to run"
                    + "\nmin\tafter application starts, force a minimise"
                    + "\nmax\tafter application starts, force a maximise"
                    + "\nclose\t after application starts, close the window"
                    + "\nnormal\tafter application starts, force it to use a normal size"
                    +
                    "\n\nExamples:\nApplicationBootstrap.exe \"c:/program files/foobar.exe\" /min\nWill execute the program and force it to minimise"
                    + "\n\nApplicationBootstrap.exe \"d:/test.exe\" /max\nWill execute the program and maximise it";
            Console.WriteLine(text);
        }

        private static void KeyboardAndClose()
        {
            Console.Write("\nPress any key to exit application");
            Console.ReadKey(true);
            Environment.Exit(0);
        }

        private static void printErrorMessage(String msg)
        {
            ShowWindow(hWnd, (int)Operation.Normal);
            Console.WriteLine("\nError:" + msg);
            printInstructions(false);
            KeyboardAndClose();
        }

        /// <summary>
        /// Executes a shell command synchronously.
        /// </summary>
        /// <param name="command">string command</param>
        /// <returns>string, as output of the command.</returns>
        private static System.Diagnostics.Process ExecuteCommandSync(object command)
        {
            try
            {
                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                String procstr = command + "";
                var procStartInfo =
                    new ProcessStartInfo(procstr);

                // The following commands are needed to redirect the standard output.
                // This means that it will be redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                // Do not create the black window.
                procStartInfo.CreateNoWindow = true;
                // Now we create a process, assign its ProcessStartInfo and start it
                var proc = new System.Diagnostics.Process();
                procStartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                proc.StartInfo = procStartInfo;
                proc.Start();
                return proc;
                // Get the output into a string
                //string result = proc.StandardOutput.ReadToEnd();
                // Display the command output.
                // Console.WriteLine(result);
            }
            catch (Exception objException)
            {
                // Log the exception
            }
            return null;
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool CloseWindow(IntPtr hWnd);

        public static async Task Run(string[] args)
        {
            v = AssemblyExtras.GetCallingAssemblyInfo();
            var n = v.GetAppString();
            Console.Title = n;

            hWnd = FindWindow(null, n); //put your console window caption here
            ShowWindow(hWnd, (int)Controller.Operation.Hide);

            if (args.Length < 2 || args.Length > 4 || (args.Length == 1 && args[0].Equals("/?")))
            {
                printInstructions(true);
                await Licensing.UpdateConsole(HelpString, OtherText);
                KeyboardAndClose();
            }
            else
            {

                int num = 0;
                String path = "";
                var op = Operation.None;

                bool quote = false;
                foreach (var s in args)
                {
                    if (num == 0)
                    {
                        path = s;
                        var count = path.Count(ss => ss == '\"');
                        if (count == 1)
                            quote = true;
                    }
                    else
                    {
                        if (quote)
                        {
                            path += s;
                            if (path.Contains("\""))
                                quote = false;
                            continue;
                        }

                        var sl = s.ToLower();

                        switch (sl)
                        {
                            case "/min":
                                op = Operation.Min;
                                break;

                            case "/max":
                                op = Operation.Max;
                                break;

                            case "/normal":
                                op = Operation.Normal;
                                break;

                            case "/close":
                                op = Operation.Close;
                                break;
                        }
                    }

                    num++;
                }

                if (path.Length == 0)
                {
                    printErrorMessage("No Path Supplied");
                }
                if (op == Operation.None)
                {
                    printErrorMessage("Provide either min or max flag");
                }
                //get file to run
                //String sss = "start " + '"' + path + '"';
                String sss = " " + path;
                var proc = ExecuteCommandSync(sss);
                bool performed = false;
                const int maxtimeouts = 10000;
                int timeout = 0;
                const int timeoutinterval = 1000;

                //Thread.Sleep(4000);

                while (performed == false)
                {
                    if (op == Operation.Close)
                    {
                        CloseWindow(proc.MainWindowHandle);
                        performed = true;
                        continue;
                    }
                    //min
                    var a = ShowWindow(proc.MainWindowHandle, (int)Operation.Min);
                    // Console.WriteLine("a="+a.ToString());
                    //max
                    var b = ShowWindow(proc.MainWindowHandle, (int)Operation.Max);
                    //Console.WriteLine("b=" + b.ToString());
                    //then perform user sizing
                    var c = ShowWindow(proc.MainWindowHandle, (int)op);
                    //Console.WriteLine("c=" + c.ToString());

                    if (c)
                        performed = true;
                    Thread.Sleep(timeoutinterval);
                    timeout += timeoutinterval;
                    if (timeout >= maxtimeouts)
                        return;
                }
            }

        }
    }
}
