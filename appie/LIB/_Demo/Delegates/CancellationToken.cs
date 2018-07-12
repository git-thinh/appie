class Test
{
    private delegate void MyDelegate(CancellationToken token);
    static MyDelegate obj;
    private static void MyMethod(CancellationToken token)
    {
        for (int i = 0; i < 1000; i++)
        {
            token.ThrowIfCancellationRequested();
            Console.Write(i + " ");
            Thread.Sleep(100);
        }
    }
    public static void Main()
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken token = cts.Token;

        obj = new MyDelegate(MyMethod);
        IAsyncResult result = obj.BeginInvoke(token, null, null);
        new Thread(() => { Thread.Sleep(3000); cts.Cancel(); }).Start();
        try
        {
            obj.EndInvoke(result);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\nOperation was canceled.");
        }
        Console.ReadLine();
    }
}
Output:
0 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18 19 20 21 22 23 24 25 26 27 28 29