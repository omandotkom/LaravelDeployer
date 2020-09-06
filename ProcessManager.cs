using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SLDeploye
{
    class ProcessManager
    {
        private static ProcessStartInfo psInfo;
        public static void SimpleProcessGenerator(string filename, string wd, string arg)
        {
            psInfo = new ProcessStartInfo();
            psInfo.WorkingDirectory = wd;
            psInfo.FileName = filename;
            var process = Process.Start(psInfo);

            process.WaitForExit();


        }
        
        public static void KillAll(string pname,DeployeSetting setting)
        {
            string clear2 = "/f /im php.exe";
            ProcessGenerator("taskkill", clear2, setting);

        }
        public static void ProcessGenerator(string filename, string arg,DeployeSetting setting)
        {
            psInfo = new ProcessStartInfo();
            psInfo.UseShellExecute = false;
            psInfo.RedirectStandardError = true;
            psInfo.RedirectStandardOutput = true;
            psInfo.RedirectStandardInput = true;
            psInfo.WindowStyle = ProcessWindowStyle.Hidden;
            psInfo.CreateNoWindow = true;
            psInfo.FileName = filename;
            if (setting.PROJECT !=null)
            {
                psInfo.WorkingDirectory = Path.GetDirectoryName(setting.PROJECT);
            }
                
            psInfo.Arguments = arg;
            var process = Process.Start(psInfo);
            var err = "";

            process.OutputDataReceived += (o, e) =>
            {
                if (e.Data == null) err = e.Data;
                else Console.WriteLine(e.Data);
            };
            process.BeginOutputReadLine();

            process.ErrorDataReceived += (o, e) =>
            {
                if (e.Data == null) err = e.Data;
                else Console.WriteLine(e.Data);
            };

            process.BeginErrorReadLine();
            process.WaitForExit();


        }

    }
}
