//Copyright (C) Microsoft Corporation.  All rights reserved.
//Conditional Methods Sample
//This sample demonstrates conditional methods, which provide a powerful mechanism by which calls to methods can be included or omitted depending on whether a symbol is defined.

//Security Note
//This sample code is provided to illustrate a concept and should not be used in applications or Web sites, as it may not illustrate the safest coding practices.Microsoft assumes no liability for incidental or consequential damages should the sample code be used for purposes other than as intended.


// TraceTest.cs
// compile with: /reference:CondMethod.dll
// arguments: A B C
using System; 
using TraceFunctions; 

public class TraceClient 
{
   public static void Main(string[] args) 
   { 
      Trace.Message("Main Starting"); 
   
      if (args.Length == 0) 
      { 
          Console.WriteLine("No arguments have been passed"); 
      } 
      else 
      { 
          for( int i=0; i < args.Length; i++)    
          { 
              Console.WriteLine("Arg[{0}] is [{1}]",i,args[i]); 
          } 
      } 

       Trace.Message("Main Ending"); 
   } 
}

