using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace GaussianElimination
{
    //Клас для розвязування СЛАР методом Гауса
    public class LinearEquationsSystem
    {
        public static string fileName = "output.txt";
        public readonly List<LinearEquation> equations;
        private readonly List<double> eVector = new List<double>();
        private List<double> multipliers;
        private int stepNumber;

        //Методи і властивості для виведення:
        //коренів рівняння, сумісності системи
        //визначеності системи, ненульових елементів
        //та рангу матриці
        public List<double> Roots { get; } = new List<double>();
        public bool IsCompatible => equations.Count == MatrixRang;
        public bool IsDefinite => MatrixRang >= equations[0].AMembers.Count;
        public bool IsEchelon => equations.Exists(equation => equation.AMembers.Count(a => a != 0.0) == 1);
        public int MatrixRang => equations.Count(equation => equation.AMembers.Any(a => a != 0));

        public LinearEquationsSystem()
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            //Заповнення випадковими рівняннями
            equations = new List<LinearEquation>
            {
                new LinearEquation(new List<double> {  -3.0, 4.0,  1.0,  4.0, -2.0}, -1.0),
                new LinearEquation(new List<double> {   0.0, 1.0,  3.0,  2.0, 1.0}, -1.0),
                new LinearEquation(new List<double> {   4.0, 0.0, -2.0, -3.0, 1.0},  4.0),
                new LinearEquation(new List<double> {10.0, 1.0,  1.0, 1.0, 1.0}, -2.0),
                new LinearEquation(new List<double> {1000.0, 3.0,  1.0, -5.0, -1.0}, 1.0)
            };
        }

        //Додавання нових рівнянь
        public void AddEquations(List<LinearEquation> newEquations) { equations.AddRange(newEquations); }

        //Видалення попередніх рівнянь
        public void DeleteEquations() { equations.Clear(); }

        //Розвязання СЛАР методом Гауса
        public void Solve()
        {
            Console.WriteLine(" Initial Data");

            DisplaySystem();

            if (IsCompatible)
            {
                if (IsDefinite)
                {
                    ForwardElimination();
                    if (!IsCompatible) return;

                    BackSubstitution();
                    // FindEVector();
                    DisplayResults();
                }
                else Console.WriteLine(" System is undefined.\r\n Infinite number of solutions!");
            }
            else Console.WriteLine(" System is incompatible.\r\n No solutions!");
        }

        //Відображення системи рівнянь
        private void DisplaySystem()
        {
            Console.WriteLine();
            foreach (var equation in equations)
            {
                var line = equation.AMembers.Aggregate(" ", (current, a) => current + $"{a,8:F3}");
                var b = equation.BMember;
                line += $"   ={b,8:F3}";

                string res = "";
                for (int j = 0; j < equation.AMembers.Count; ++j)
                {
                    res += string.Format("{0:0.00}", Math.Abs(equation.AMembers[j])) + "*X" + Convert.ToString(j + 1) + (j < equation.AMembers.Count - 1 ? (equation.AMembers[j + 1] >= 0 ? " + " : " - ") : " = ");
                }
                res += Math.Round(equation.BMember, 3).ToString();

                Console.WriteLine(line);
                File.AppendAllText(fileName, res + "\n");
            }
            Console.WriteLine(" " + new string('-', 44));
            File.AppendAllText(fileName, " " + new string('-', 44) + "\n\n");
        }

        //Визначення ведучого елементу
        private int FindLeadingElement()
        {
            var elementsToCheck = new List<double>();

            for (var i = 0; i < equations.Count; i++)
            {
                if (stepNumber > 0 && i < stepNumber || stepNumber >= equations[i].AMembers.Count)
                    continue;
                var aMember = Math.Abs(equations[i].AMembers[stepNumber]);
                elementsToCheck.Add(aMember);
            }

            if (!elementsToCheck.Any())
                return -1;

            var leadingElement = elementsToCheck.Max();
            return elementsToCheck.IndexOf(leadingElement) + stepNumber;
        }

        //Заміна рядків
        private void SwapLines()
        {
            Console.WriteLine(" Swap Equations");
            var tempEquation = equations[stepNumber];
            var leadingElementIndex = FindLeadingElement();

            if (leadingElementIndex > -1)
            {
                equations[stepNumber] = equations[leadingElementIndex];
                equations[leadingElementIndex] = tempEquation;
            }
        }

        //Визначення множників
        private void FindMultipliers()
        {
            Console.WriteLine("\r\n Finding multipliers...");
            multipliers = new List<double>();
            for (var i = 1; i < equations.Count; i++)
            {
                if (stepNumber < equations[i].AMembers.Count)
                    multipliers.Add(-equations[i].AMembers[stepNumber] / equations[stepNumber].AMembers[stepNumber]);
            }
        }

        //Занулення елементів використовуючи множники
        private void EliminateRows()
        {
            Console.WriteLine(" Eliminate rows using multipliers");
            for (var i = stepNumber + 1; i <= equations.Count - 1; i++)
            {
                for (var j = 0; j < equations[i].AMembers.Count; j++)
                    equations[i].AMembers[j] += equations[stepNumber].AMembers[j] * multipliers[i - 1];

                equations[i].BMember += equations[stepNumber].BMember * multipliers[i - 1];
            }
        }

        //Пряма ітерація для занулення елементів
        private void ForwardElimination()
        {
            stepNumber = 0;

            while (!IsEchelon || stepNumber < equations.Count)
            {
                Console.WriteLine("\r\n Step #" + (stepNumber + 1));

                FindLeadingElement();
                SwapLines();

                DisplaySystem();

                FindMultipliers();
                EliminateRows();

                DisplaySystem();

                stepNumber++;

                if (IsCompatible) continue;
                Console.WriteLine(" System is incompatible.\r\n No solutions!");
                break;
            }
        }

        //Зворотня ітерація
        private void BackSubstitution()
        {
            Console.WriteLine("\r\n Finding roots using back substitution...");

            for (var i = 0; i < equations[0].AMembers.Count; i++)
                Roots.Add(0.0);

            stepNumber = 0;

            for (var currentEquationIndex = equations.Count - 1; currentEquationIndex >= 0; currentEquationIndex--)
            {
                double sumAnXn = 0;
                var lastEquationIndex = equations.Count - 1;

                for (var aMemberIndex = 0; aMemberIndex < stepNumber; aMemberIndex++)
                {
                    var currentAMemberIndex = lastEquationIndex - aMemberIndex;
                    if (currentAMemberIndex < equations[currentEquationIndex].AMembers.Count)
                        sumAnXn += (equations[currentEquationIndex].AMembers[currentAMemberIndex] * Roots[currentAMemberIndex]);
                }

                if (currentEquationIndex < equations[currentEquationIndex].AMembers.Count)
                    Roots[currentEquationIndex] = (equations[currentEquationIndex].BMember - sumAnXn) / equations[currentEquationIndex].AMembers[currentEquationIndex];

                stepNumber++;
            }
        }

        //Виведення коренів рівняння
        private void DisplayResults()
        {
            Console.WriteLine("\r\n Roots:");

            for (var i = 0; i < Roots.Count; i++)
                Console.WriteLine($" x{i + 1} = {Roots[i],8:F5} ");
        }
    }
}