using Microsoft.VisualBasic;
using System.Threading.Tasks;

namespace ThreadAndAsyncAwait
{
    public  class Program
    {
       public static async Task Main(string[] args)
        {
            //Task t2 = Task2();
            Task t3 = Task3();

            //Task.WaitAll(t2, t3); ensure t2 and t2 completely before console write line and readkey below

            Task t4 =  Task4();  
            Task t5 =  Task5();

            var t6 = await Task6();
            var t7 = await Task7();
            
            Console.WriteLine($"{t6}");
            Console.WriteLine($"{t7}");
            Console.WriteLine("Hello, World!");
            Console.ReadKey();
        }

        #region not using async await
        private static Task Task3()
        {
            var task3 =  new Task((object ob) =>
            {
                string tentacvu = (string)ob;
                DoSomething(4, tentacvu, ConsoleColor.Yellow);
            }, "T3");
            task3.Start(); // run on seprerate thread

            task3.Wait();  // su dung wait se bi lock thread den khi bao gio xong moi thuc hien tiep cau lenh duoi
            Console.WriteLine("Task 3 is completed");
            return task3;
        }

        private static Task Task2()
        {
            Task t2 = new Task(() =>
            //ACtion()
            {
                DoSomething(10, "T2", ConsoleColor.Green);
            });
            t2.Start(); // run on seprerate thread

            t2.Wait(); // su dung wait se bi lock thread den khi bao gio xong moi thuc hien tiep cau lenh duoi
            Console.WriteLine("Task 2 is completed");
            return t2;
        }
        #endregion  

        #region USING async await on Task (not return result)
        private static async Task Task4()
        {
            var task4 = new Task((object ob) =>
            {
                string tentacvu = (string)ob;
                DoSomething(7, tentacvu, ConsoleColor.Red);
            }, "T4");
            task4.Start(); // run on seprerate thread
            await task4;  // khi nao thuc hien xong thi moi thuc hien tiep cau lenh o duoi
            Console.WriteLine("Task 4 is completed");
        }

        private static async Task Task5()
        {
            Task task5 = new Task(() =>
            {
                DoSomething(8, "T5", ConsoleColor.Magenta);
            });
            task5.Start(); // run on seprerate thread
            await task5; // khi nao thuc hien xong thi moi thuc hien tiep cau lenh o duoi
            Console.WriteLine("Task 5 is completed");
        }
        #endregion

        #region USING async await on Task<string> (return string value)
        private static async Task<string> Task6()
        {
            var task6 = new Task<string>((object ob) =>
            {
                string tentacvu = (string)ob;
                DoSomething(3, tentacvu, ConsoleColor.Cyan);
                return $"Task {tentacvu} is completed";
            }, "T6");
            task6.Start(); // run on seprerate thread
            var res = await task6;  // khi nao thuc hien xong thi moi thuc hien tiep cau lenh o duoi
            return res;
        }

        private static async Task<string> Task7()
        {
            var task7 = new Task<string>(() =>
            {
                DoSomething(3, "T7", ConsoleColor.DarkYellow);
                return ("Task 7 is completed");
            });
            task7.Start(); // run on seprerate thread
            var res = await task7;  // khi nao thuc hien xong thi moi thuc hien tiep cau lenh o duoi
            return res;
        }
        #endregion

        private static void DoSomething(int seconds, string mgs, ConsoleColor color)
        {
            lock(Console.Out)
            {
                Console.ForegroundColor = color;
                Console.WriteLine($"{mgs, 10}.... Start");
                Console.ResetColor();
            }

            for (int i = 0; i < seconds; i++)
            {
                lock (Console.Out)
                {
                    Console.ForegroundColor = color;
                    Console.WriteLine($"{mgs,10} {i,2}");
                    Console.ResetColor();
                }
                Thread.Sleep(1000);
            }

            lock (Console.Out)
            {
                Console.ForegroundColor = color;
                Console.WriteLine($"{mgs,10}.... Finish");
                Console.ResetColor();
            }

        }
    }
}