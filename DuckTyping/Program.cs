using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuckTyping
{

    public interface IService
    {
        string Echo(string input);
    }

    public class ServiceImpl : IService
    {
        public string Echo(string input)
        {
            return $"Echo: {input}";
        }
    }

    public class DummyImpl : IService
    {
        private readonly IService instance;

        public DummyImpl(IService x)
        {
            this.instance = x;
        }

        public string Echo(string input)
        {
            Console.WriteLine("Begin Echo");
            var stamp = DateTime.Now;
            var result = instance.Echo(input);
            var x = DateTime.Now.Subtract(stamp);
            Console.WriteLine(x.ToString());
            Console.WriteLine(result);
            Console.WriteLine("End Echo");
            return result;
        }

        public void ArrayTest()
        {
            var arr = new int[1];
            arr[0] = 1;

            Console.WriteLine(arr.Length);
            Console.WriteLine(arr[0]);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ExecuteDuckTyping().Wait();

            //ExecuteProxy();
        }

        private static void ExecuteProxy()
        {
            var service = CustomProxy<IService>.Wrap(new ServiceImpl());
            var result = service.Echo("hello");
        }

        static async Task ExecuteDuckTyping()
        {
            // DuckEnumeration
            var enumeration = new DuckEnumeration();
            foreach (var item in enumeration)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine();

            // DuckTask
            var task = new DuckTask("async duck");
            var asyncItem = await task;
            Console.WriteLine(asyncItem);
            Console.WriteLine();
        }
    }
}
