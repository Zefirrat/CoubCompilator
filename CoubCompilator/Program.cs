using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoubCompilator.CoubClasses;
using CoubCompilator.Http;
using Newtonsoft.Json;

namespace CoubCompilator
{
    class Program
    {
        static void Main(string[] args)
        {
            string command;
            CompilatorsBody compilatorsBody = new CompilatorsBody();
        enterCommand:
            Console.Clear();
            Console.WriteLine("Enter the command:");
            command = Console.ReadLine();
            switch (command)
            {
                case "Work()":
                    compilatorsBody.Work();
                    break;
                case "CompileAll()":
                    compilatorsBody.CompileAll();
                    break;
                case "CompileMain()":
                    compilatorsBody.CompileMain();
                    break;
                case "Test()":
                    compilatorsBody.AddToBlackList("Foo");
                    break;
            }
            Console.ReadLine();
            goto enterCommand;
        }
    }
}
