//Copyright (C) Microsoft Corporation.  All rights reserved.
//Anonymous Delegates Sample
//This sample demonstrates using an anonymous delegate to calculate a salary bonus for an employee. Using an anonymous delegate simplifies the program because there is no need to define a separate method.

//The data for each employee is stored in an object containing personal details as well as a delegate that references the algorithm required to calculate the bonus.By defining the algorithm by means of a delegate, the same method can be used to perform the bonus calculation, irrespective of how it is actually calculated. Also of note, a local variable, multiplier, becomes a captured outer variable because it is referenced in a delegated calculation.

//Security Note
//This sample code is provided to illustrate a concept and should not be used in applications or Web sites, as it may not illustrate the safest coding practices. Microsoft assumes no liability for incidental or consequential damages should the sample code be used for purposes other than as intended.


using System;
using System.Collections.Generic;
using System.Text;

namespace AnonymousDelegate_Sample
{

    // Define the delegate method.
    delegate decimal CalculateBonus(decimal sales);

    // Define an Employee type.
    class Employee
    {
        public string name;
        public decimal sales;
        public decimal bonus;
        public CalculateBonus calculation_algorithm;
    }

    class Program
    {

        // This class will define two delegates that perform a calculation.
        // The first will be a named method, the second an anonymous delegate.

        // This is the named method.
        // It defines one possible implementation of the Bonus Calculation algorithm.

        static decimal CalculateStandardBonus(decimal sales)
        {
            return sales / 10;
        }

        static void Main(string[] args)
        {

            // A value used in the calculation of the bonus.
            // Note: This local variable will become a "captured outer variable".
            decimal multiplier = 2;

            // This delegate is defined as a named method.
            CalculateBonus standard_bonus = new CalculateBonus(CalculateStandardBonus);

            // This delegate is anonymous - there is no named method.
            // It defines an alternative bonus calculation algorithm.
            CalculateBonus enhanced_bonus = delegate(decimal sales) { return multiplier * sales / 10; };

            // Declare some Employee objects.
            Employee[] staff = new Employee[5];

            // Populate the array of Employees.
            for (int i = 0; i < 5; i++)
                staff[i] = new Employee();

            // Assign initial values to Employees.
            staff[0].name = "Mr Apple";
            staff[0].sales = 100;
            staff[0].calculation_algorithm = standard_bonus;

            staff[1].name = "Ms Banana";
            staff[1].sales = 200;
            staff[1].calculation_algorithm = standard_bonus;

            staff[2].name = "Mr Cherry";
            staff[2].sales = 300;
            staff[2].calculation_algorithm = standard_bonus;

            staff[3].name = "Mr Date";
            staff[3].sales = 100;
            staff[3].calculation_algorithm = enhanced_bonus;

            staff[4].name = "Ms Elderberry";
            staff[4].sales = 250;
            staff[4].calculation_algorithm = enhanced_bonus;

            // Calculate bonus for all Employees
            foreach (Employee person in staff)
                PerformBonusCalculation(person);

            // Display the details of all Employees
            foreach (Employee person in staff)
                DisplayPersonDetails(person);


        }

        public static void PerformBonusCalculation(Employee person)
        {

            // This method uses the delegate stored in the person object
            // to perform the calculation.
            // Note: This method knows about the multiplier local variable, even though
            // that variable is outside the scope of this method.
            // The multipler varaible is a "captured outer variable".
            person.bonus = person.calculation_algorithm(person.sales);
        }

        public static void DisplayPersonDetails(Employee person)
        {
            Console.WriteLine(person.name);
            Console.WriteLine(person.bonus);
            Console.WriteLine("---------------");
        }
    }
}


