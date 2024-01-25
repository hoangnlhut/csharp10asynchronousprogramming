using System.Diagnostics;

namespace StockAnalyzer.AttachedDetatched
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            /// 111111111
            /// it will immediately schedule the work for each nested task that the parent task creates
            /// The parent task is marked as COmpleted as soon as it's started all of its nested tasks
            /// A child task executes independently of its parent.

            //await Task.Factory.StartNew(() =>
            //{
            //    Task.Factory.StartNew(() => {
            //        Thread.Sleep(4000);
            //        Console.WriteLine("Complete Task 1");
            //    });
            //    Task.Factory.StartNew(() => {
            //        Thread.Sleep(7000);
            //        Console.WriteLine("Complete Task 2");
            //    });
            //    Task.Factory.StartNew(() => {
            //        Thread.Sleep(5000);
            //        Console.WriteLine("Complete Task 3");
            //    });
            //}, TaskCreationOptions.DenyChildAttach);

            /// 2222222222
            /// attach sub task to parent tasks using AttachedToParent so that the parent
            /// and child tasks are synchronized.
            /// it did'nt run the continuation until the attached child was completed because
            /// the parent and one of the child tasks are now synchronized

            //await Task.Factory.StartNew(() =>
            //{
            //    Task.Factory.StartNew(() => {
            //        Thread.Sleep(4000);
            //        Console.WriteLine("Complete Task 4");
            //    }, TaskCreationOptions.AttachedToParent);
            //    Task.Factory.StartNew(() => {
            //        Thread.Sleep(6000);
            //        Console.WriteLine("Complete Task 5");
            //    }, TaskCreationOptions.AttachedToParent);
            //    Task.Factory.StartNew(() => {
            //        Thread.Sleep(5000);
            //        Console.WriteLine("Complete Task 6");
            //    }, TaskCreationOptions.AttachedToParent);
            //});

            ///333333
            /// 

            var task = Task.Factory.StartNew(async () =>
            {
                await Task.Delay(200);
                return "Pluralsight";
            }).Unwrap();

            var result = await task;
            Console.WriteLine(result);

            Console.WriteLine("Goodbye");
            Console.ReadLine();
        }
    }
}