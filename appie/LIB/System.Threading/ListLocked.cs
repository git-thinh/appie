using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace System
{
    // http://www.albahari.com/threading/part3.aspx
    // https://stackoverflow.com/questions/4217398/when-is-readerwriterlockslim-better-than-a-simple-lock
    /*

class Program
{
    static void Main()
    {
        ISynchro[] test = { new Locked(), new RWLocked() };

        Stopwatch sw = new Stopwatch();

        foreach ( var isynchro in test )
        {
            sw.Reset();
            sw.Start();
            Thread w1 = new Thread( new ParameterizedThreadStart( WriteThread ) );
            w1.Start( isynchro );

            Thread w2 = new Thread( new ParameterizedThreadStart( WriteThread ) );
            w2.Start( isynchro );

            Thread r1 = new Thread( new ParameterizedThreadStart( ReadThread ) );
            r1.Start( isynchro );

            Thread r2 = new Thread( new ParameterizedThreadStart( ReadThread ) );
            r2.Start( isynchro );

            w1.Join();
            w2.Join();
            r1.Join();
            r2.Join();
            sw.Stop();

            Console.WriteLine( isynchro.ToString() + ": " + sw.ElapsedMilliseconds.ToString() + "ms." );
        }

        Console.WriteLine( "End" );
        Console.ReadKey( true );
    }

    static void ReadThread(Object o)
    {
        ISynchro synchro = (ISynchro)o;

        for ( int i = 0; i < 500; i++ )
        {
            Int32? value = synchro.Get( i );
            Thread.Sleep( 50 );
        }
    }

    static void WriteThread( Object o )
    {
        ISynchro synchro = (ISynchro)o;

        for ( int i = 0; i < 125; i++ )
        {
            synchro.Add( i );
            Thread.Sleep( 200 );
        }
    }

}     
         
         */

    

    interface ISynchro
    {
        void Add(Int32 value);
        Int32? Get(Int32 index);
    }

    class Locked : List<Int32>, ISynchro
    {
        readonly Object locker = new object();

        #region ISynchro Members

        public new void Add(int value)
        {
            lock (locker)
                base.Add(value);
        }

        public int? Get(int index)
        {
            lock (locker)
            {
                if (this.Count <= index)
                    return null;
                return this[index];
            }
        }

        #endregion
        public override string ToString()
        {
            return "Locked";
        }
    }

    class RWLocked : List<Int32>, ISynchro
    {
        ReaderWriterLockSlim locker = new ReaderWriterLockSlim();

        #region ISynchro Members

        public new void Add(int value)
        {
            try
            {
                locker.EnterWriteLock();
                base.Add(value);
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public int? Get(int index)
        {
            try
            {
                locker.EnterReadLock();
                if (this.Count <= index)
                    return null;
                return this[index];
            }
            finally
            {
                locker.ExitReadLock();
            }
        }

        #endregion

        public override string ToString()
        {
            return "RW Locked";
        }
    }
}
