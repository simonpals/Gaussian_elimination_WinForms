using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using GaussianElimination;

namespace GaussMethodApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            // Define the border style of the form to a dialog box.
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            // Set the MaximizeBox to false to remove the maximize box.
            this.MaximizeBox = false;

            // Set the MinimizeBox to false to remove the minimize box.
            this.MinimizeBox = false;

            // Set the start position of the form to the center of the screen.
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //N - кількість рівнянь
            int N = Convert.ToInt32(textBox1.Text); // rows
            //M - кількість невідомих
            int M = Convert.ToInt32(textBox2.Text); // columns

            //Очистити попередньо згенеровані матриці
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
            dataGridView3.Rows.Clear();
            dataGridView3.Columns.Clear();
            listBox1.Clear();

            //Створити загаловок вектора X
            var colX1 = new DataGridViewColumn();
            colX1.HeaderText = "XGauss";
            colX1.CellTemplate = new DataGridViewTextBoxCell();
            colX1.Width = 70;
            dataGridView3.Columns.Add(colX1);

            // Створити загаловки для таблиці вхідної матриці
            for (int i = 0; i < M + 1; ++i)
            {
                var column = new DataGridViewColumn();
                column.HeaderText = (i == M ? "B" : "X" + (i + 1).ToString());
                column.CellTemplate = new DataGridViewTextBoxCell();
                column.Width = 40;
                dataGridView1.Columns.Add(column);

                if (i != M)
                {
                    dataGridView3.Rows.Add();
                }
            }

            for (int j = 0; j < N; ++j)
            {
                dataGridView1.Rows.Add();
            }

            int max = 10;
            int min = -10;
            Random rand = new Random();

            //Заповнення матриці та вектора вільних 
            //членів випадковими значеннями
            if (checkBox1.Checked)
            {
                for (int i = 0; i < N; ++i)
                {
                    for (int j = 0; j < M + 1; ++j)
                    {
                        int val = rand.Next(min, max);
                        while (val == 0)
                        {
                            val = rand.Next(min, max);
                        }
                        dataGridView1[j, i].Value = Convert.ToString(val);
                    }
                }
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            //N - кількість рівнянь
            //M - кількість невідомих
            int N = Convert.ToInt32(textBox1.Text); // rows
            int M = Convert.ToInt32(textBox2.Text); // columns

            //listBox1.Sorted = false;
            listBox1.Clear();

            //Заповнюємо розвязувач СЛАР рівняннями
            var linearEquationsSystem = new LinearEquationsSystem();
            List<LinearEquation> equations = new List<LinearEquation>();
            for (int i = 0; i < N; ++i)
            {
                List<double> equation = new List<double>();

                for (int j = 0; j < M; ++j)
                {
                    equation.Add(Convert.ToDouble(dataGridView1[j, i].Value));
                }

                equations.Add(new LinearEquation(equation, Convert.ToDouble(dataGridView1[M, i].Value)));
            }

            //Видаляємо попередні рівняння
            linearEquationsSystem.DeleteEquations();
            //Додаємо нові
            linearEquationsSystem.AddEquations(equations);

            //Розвязуємо СЛАР методом Гауса
            linearEquationsSystem.Solve();

            if (!linearEquationsSystem.IsCompatible)
                MessageBox.Show("Система несумісна, коренів немає");
            else if (!linearEquationsSystem.IsDefinite)
                MessageBox.Show("Система невизначена, нескінченна кількість коренів");
            else
            {
                //Виводимо знайдених вектор X
                for (int i = 0; i < M; ++i)
                {
                    dataGridView3[0, i].Value = linearEquationsSystem.Roots[i].ToString();
                }

                //Виводимо результуючу матрицю після
                //перетворень методом Гауса
                string[] lines = null;
                lines = File.ReadAllLines(LinearEquationsSystem.fileName);
                Array.Resize(ref lines, lines.Length + 1);
                lines[lines.Length - 1] = "\nРанг матриці: "+ linearEquationsSystem.MatrixRang;
                listBox1.Lines = lines; 
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Виходимо з програми
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Автор програми: П.І.Б.");
        }
    }
}
