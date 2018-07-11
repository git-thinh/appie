using System;
using System.Threading;

namespace DelegateAsyncMethods
{
    delegate int SomeDelegateOne(int x);

    /// <summary>
    /// This code is written for the article 
    /// www.dotnetcurry.com/ShowArticle.aspx?ID=642
    /// </summary>
    class PollingPattern
    {
        static void Main(string[] args)
        {
            SomeDelegateOne sd = SquareNumber; // Create delegate instance
            Console.WriteLine("Before SquareNumber Method Invoke");
            IAsyncResult asyncRes = sd.BeginInvoke(10, null, null); // async call           
            Console.WriteLine("Back to Main Method");

            // Poll IAsyncResult.IsCompleted
            while (asyncRes.IsCompleted == false)
            {
                Console.WriteLine("Square Number still processing");
                Thread.Sleep(1000);  // emulate that method is busy
            }

            Console.WriteLine("Square Number processing completed");
            int res = sd.EndInvoke(asyncRes);
            Console.WriteLine(res);
            Console.ReadLine();
        }

        static int SquareNumber(int a)
        {
            Console.WriteLine("SquareNumber Invoked. Processing..");
            Thread.Sleep(2000);
            return a * a;
        }
    }
}
