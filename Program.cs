using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using ANDREICSLIB.ClassExtras;
using ANDREICSLIB.Helpers;
using ANDREICSLIB.Licensing;

namespace ApplicationBootstrap
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            AsyncHelpers.RunSync(() => Controller.Run(args));
        }
    }
}
