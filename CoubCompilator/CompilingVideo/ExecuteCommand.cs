using System;
using System.Diagnostics;

namespace CoubCompilator.CompilingVideo
{
    /// <summary>
    /// Class to execute some commands with specified file in constructor.
    /// Default is "cmd.exe"
    /// </summary>
    public class ExecuteCommand
    {
        private string _fileName;

        /// <summary>
        /// Default constructor. Executable file is "cmd.exe".
        /// </summary>
        public ExecuteCommand()
        {
            _fileName = "cmd.exe";
        }

        /// <summary>
        /// Constructor with custom specified file name. 
        /// </summary>
        /// <param name="fileName">File to execute command with</param>
        public ExecuteCommand(string fileName) : base()
        {
            _fileName = fileName;
        }

        /// <summary>
        /// Executing command with specified in constructor file
        /// </summary>
        /// <param name="command">Command to execute</param>
        public void Execute(string command)
        {
            Process process = new Process();
            process.StartInfo.FileName = _fileName;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.Start();
            process.StandardInput.WriteLine(command);
            process.StandardInput.Flush();
            process.StandardInput.Close();
            process.WaitForExit();
            Console.WriteLine(process.StandardOutput.ReadToEnd());
            process.Dispose();
        }
    }
}
