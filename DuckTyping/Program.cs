using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckTyping
{
    class Program
    {
        static void Main(string[] args)
        {
            ExecuteDuckEnumeration();
            ExecuteDuckTask().Wait();
        }

        static void ExecuteDuckEnumeration()
        {
            // DuckEnumeration
            var enumeration = new DuckEnumeration();
            foreach (var item in enumeration)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine();
        }

        static async Task ExecuteDuckTask()
        {
            // DuckTask
            var task = new DuckTask("async duck");
            var asyncItem = await task;
            Console.WriteLine(asyncItem);
            Console.WriteLine();
        }
    }
}
