using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SetCalculator
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Please enter the universe set separated by ','");
            var SetU = Console.ReadLine().Trim().Split(',');

            Console.WriteLine("Please enter set A separated by ','");
            var SetA = Console.ReadLine().Trim().Split(',');

            Console.WriteLine("Please enter set B separated by ','");
            var SetB = Console.ReadLine().Trim().Split(',');

            Console.WriteLine("\n========================\n");

            Console.WriteLine($"U = {PrintSet(SetU)} A = {PrintSet(SetA)} B = {PrintSet(SetB)}");

            Console.WriteLine("2) B' {" + $"{String.Join(",", SetU.Except(SetB).OrderBy(x => x))}" + "}");
            
            Console.WriteLine("4) A' U B' {" + $"{String.Join(",", SetU.Except(SetA).Union(SetU.Except(SetB)).OrderBy(x => x))}" + "}");

            Console.WriteLine("6) A' ∩ B' {" + $"{String.Join(",", SetU.Except(SetA).Intersect(SetU.Except(SetB)).OrderBy(x => x))}" + "}");

            Console.WriteLine("8) A ∩ B' {" + $"{String.Join(",", SetA.Intersect(SetU.Except(SetB)).OrderBy(x => x))}" + "}");

            Console.WriteLine("10) A U B' {" + $"{String.Join(",", SetA.Union(SetU.Except(SetB)).OrderBy(x => x))}" + "}");

            Console.ReadKey();
        }

        public static String PrintSet(String[] Set)
        {
            return String.Concat("{", String.Join(",", Set) ,"}");
        }
    }
}
