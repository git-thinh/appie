using System;
using System.Threading;

namespace DelegateAsyncMethods
{
    delegate int SomeDelegate(int x);

    /// <summary>
    /// This code is written for the article 
    /// www.dotnetcurry.com/ShowArticle.aspx?ID=634
    /// </summary>
    class EndInvokePattern
    {
        static void Main(string[] args) {
            SomeDelegate sd = SquareNumber; // Create delegate instance
            Console.WriteLine("Before SquareNumber Method Invoke");
            IAsyncResult asyncRes = sd.BeginInvoke(10, null, null);            
            Console.WriteLine("Back to Main Method");
            int res = sd.EndInvoke(asyncRes);
            Console.WriteLine(res);
            Console.ReadLine();
        }

        static int SquareNumber(int a) {
            Console.WriteLine("SquareNumber Invoked. Processing..");
            Thread.Sleep(2000);
            return a * a;
        }
    }
}
