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
            CompilatorsBody  compilatorsBody = new CompilatorsBody();
            compilatorsBody.Work();
            Console.ReadLine();
            
        }
    }
}
