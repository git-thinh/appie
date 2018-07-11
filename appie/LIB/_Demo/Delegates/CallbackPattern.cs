using System;
using System.Threading;
using System.Runtime.Remoting.Messaging;

namespace DelegateAsyncMethods
{
    delegate int SomeDelegateTwo(int x);

    /// <summary>
    /// This code is written for the article 
    /// www.dotnetcurry.com/ShowArticle.aspx?ID=642
    /// </summary>
    class CallbackPattern
    {
        static void Main(string[] args)
        {
            SomeDelegateTwo sd = SquareNumber; // Create delegate instance
            Console.WriteLine("Before SquareNumber Method Invoke");
            // async call
            IAsyncResult asyncRes = sd.BeginInvoke(10, new AsyncCallback(CallbackMethod), null);         
            Console.WriteLine("Back to Main Method. Doing Extra Processing...");
            Thread.Sleep(500);
            Console.WriteLine("Main method processing completed");
            Console.ReadLine();
        }

        static int SquareNumber(int a)
        {
            Console.WriteLine("SquareNumber Invoked. Processing..");
            Thread.Sleep(3000);
            return a * a;
        }

        // Callback method
        static void CallbackMethod(IAsyncResult asyncRes)
        {
            Console.WriteLine("Callback invoked");
            AsyncResult ares = (AsyncResult)asyncRes;
            SomeDelegateTwo delg = (SomeDelegateTwo)ares.AsyncDelegate;
            int result = delg.EndInvoke(asyncRes);
            Console.WriteLine(result);
        }
    }
}
