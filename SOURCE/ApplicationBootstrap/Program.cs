using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace ApplicationBootstrap
{
    class Program
    {
        private const string name = "Application Bootstrap";
        private const string version = "1.0";
        
        const int minflag = 2;
        const int normalflag = 1;
        private const int maxflag = 3;
        const int hideflag = 0;
        //this console ptr
        static IntPtr hWnd;

        static void printInstructions(bool printIntro)
        {
            ShowWindow(hWnd, normalflag);
            String text = "";
            if (printIntro)
            {
                text = "\n"+name+" "+version +" developed by Andrei Gec(andreigec.net) (C)2012";
                text += "\nPurpose:forces an application to minimise or maximise on startup\nUseful for applications that disregard the shortcut option that is supposed to do the same";
            }

            text += "\n\nApplicationBootstrap.exe [\"path\"] [/min] [/max] [/normal]"
                    + "\n\npath\tpath to executable to run"
                    + "\nmin\tafter application starts, force a minimise"
                    + "\nmax\tafter application starts, force a maximise"
                    + "\nnormal\tafter application starts, force it to use a normal size"
                    +"\n\nExamples:\nApplicationBootstrap.exe \"c:/program files/foobar.exe\" /min\nWill execute the program and force it to minimise"
                    + "\n\nApplicationBootstrap.exe \"d:/test.exe\" /max\nWill execute the program and maximise it";
            Console.WriteLine(text);
        }

        static void KeyboardAndClose()
        {
            Console.Write("\nPress any key to exit application");
            Console.ReadKey(true);
            Environment.Exit(0);
        }

        static void printErrorMessage(String msg)
        {
            ShowWindow(hWnd, normalflag);
            Console.WriteLine("\nError:" + msg);
            printInstructions(false);
            KeyboardAndClose();
        }

        /// <summary>
        /// Executes a shell command synchronously.
        /// </summary>
        /// <param name="command">string command</param>
        /// <returns>string, as output of the command.</returns>
        static System.Diagnostics.Process ExecuteCommandSync(object command)
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
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
		public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        static void Main(string[] args)
        {
            String n = name + version;
            n=n.Replace(" ","");

            Console.Title = n;
            hWnd = FindWindow(null, n); //put your console window caption here
            ShowWindow(hWnd, hideflag);

            if (args.Length < 2 || args.Length > 4 || (args.Length == 1 && args[0].Equals("/?")))
            {
                printInstructions(true);
                KeyboardAndClose();
            }
            else
            {
                int num = 0;
                String path = "";
                bool ismin = false;
                bool ismax = false;
                bool isnormal = false;

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

                        if (s.ToLower().Equals("/min"))
                            ismin = true;
                        else if (s.ToLower().Equals("/max"))
                            ismax = true;
                        else if (s.ToLower().Equals("/normal"))
                            isnormal = true;
                    }

                    num++;
                }

                if (path.Length == 0)
                {
                    printErrorMessage("No Path Supplied");
                }
                if (ismin == false && ismax == false&&isnormal==false)
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
                
                
                int doflag=normalflag;
                if (ismin)
                    doflag = minflag;
                else if (ismax)
                {
                    doflag = maxflag;
                }
                else if (isnormal)
                    doflag = normalflag;

                //Thread.Sleep(4000);

                while (performed == false)
                {
                    //min
                    var a=ShowWindow(proc.MainWindowHandle, minflag);
                   // Console.WriteLine("a="+a.ToString());
                    //max
                    var b=ShowWindow(proc.MainWindowHandle, maxflag);
                    //Console.WriteLine("b=" + b.ToString());
                    //then perform user sizing
                    var c = ShowWindow(proc.MainWindowHandle, doflag);
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
