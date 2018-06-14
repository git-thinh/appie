//Copyright (C) Microsoft Corporation.  All rights reserved.
// Nullable Sample
// This sample demonstrates the use of nullable types.This feature allows value types to have an uninitialized, or empty, state similar to how reference types can be set to null.

using System;

class NullableBasics
{
    static void DisplayValue(int? num)
    {
        if (num.HasValue == true)
        {
            Console.WriteLine("num = " + num);
        }
        else
        {
            Console.WriteLine("num = null");
        }

        // num.Value throws an InvalidOperationException if num.HasValue is false
        try
        {
            Console.WriteLine("value = {0}", num.Value);
        }
        catch (InvalidOperationException e)
        {
            Console.WriteLine(e.Message);
        }
    }

    static void Main()
    {
        DisplayValue(1);
        DisplayValue(null);
    }
}