using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaPortsGenetic_WPF
{
    class Solution
    {
        //Поля
        private SeaPort[] solution;
        private Random random;
        private double[,] matrix_L;
        private int[,] matrix_Q;

        private MainWindow mainW;

        //Конструктор
        public Solution(Random random)
        {
            this.random = random;
        }

        public Solution(SeaPort[] seaPorts, Random random)
        {
            this.random = random;

            this.solution = new SeaPort[seaPorts.Length + 1]; //Сразу присваиваем решению новый массив из МорПортов длиной по количеству портов + 1 (для начальной точки)
            for (int i = 0; i < seaPorts.Length; i++)
            {
                this.solution[i] = seaPorts[i]; //присваиваем каждому эл-ту массива решения эл-т из массива МорПортов.
            }
            this.solution[this.solution.Length - 1] = this.solution[0]; //ставим финальную точку равную первой (замыкаем кругорейс)
        }

        //Методы
        //Getters
        public SeaPort[] getSolution()
        {
            return this.solution;
        }
        //Setters

        public void setMatrixQ(int[,] matrix)
        {
            this.matrix_Q = matrix;
        }

        public void setMatrixL(double[,] matrix)
        {
            this.matrix_L = matrix;
        }
        //Others
        public void shuffle() //Вызов шаффла по Фишеру-Йетсу + установка последнего порта захода равного первому
        {
            for (int i = this.solution.Length - 2; i >= 1; i--) //-2, а не -1 потому что последний элемент массива - это повторение первого элемента
            {
                int j = this.random.Next(i + 1);

                SeaPort spTemp = this.solution[j];
                this.solution[j] = this.solution[i];
                this.solution[i] = spTemp;
            }
            this.solution[this.solution.Length - 1] = this.solution[0];
        }


        public void swap2ports(int index1, int index2) //переставить две указанные точки (два порта)
        {
            SeaPort spTemp = this.solution[index1];
            this.solution[index1] = this.solution[index2];
            this.solution[index2] = spTemp;

            this.solution[this.solution.Length - 1] = this.solution[0]; // на всякий случай, чтобы последняя точка всегда была равна первой
        }

        public void assignPorts(int[] indexArray) //не самое лучшее решение - скорость работы сомнительна. Возможно потребуется оптимизация скорости
        {
            SeaPort[] spTempArray = new SeaPort[this.solution.Length];
            for (int i = 0; i < spTempArray.Length; i++)
            {
                int j = 0;
                while (this.solution[j].getIndex() != indexArray[i])
                {
                    j++;
                }
                spTempArray[i] = this.solution[j];
            }
            spTempArray[spTempArray.Length - 1] = spTempArray[0];

            for (int i = 0; i < this.solution.Length; i++)
            {
                this.solution[i] = spTempArray[i];
            }
        }

        public int getFitness() //фитнесс-функция на грузообороте
        {
            int fitness = 0;
            for (int i = 0; i < this.solution.Length - 1; i++) //-1 потому что до предпоследнего
            {
                fitness += this.matrix_Q[solution[i].getIndex(), solution[i + 1].getIndex()]; //к фитнесс функции добавить Q на переходе между i и i+1 точкой
            }
            return fitness;
        }

        public double getFitnessD() //фитнесс-функция на расстоянии
        {
            double fitnessD = 0;
            for (int i = 0; i < this.solution.Length - 1; i++) //-1 потому что до предпоследнего
            {
                fitnessD += this.matrix_L[solution[i].getIndex(), solution[i + 1].getIndex()]; //к фитнесс функции добавить L на переходе между i и i+1 точкой
            }
            fitnessD = fitnessD / Constants.vessel_velocity; //*0.3 Теперь это не фитнесс функция по расстоянию. Это фитнесс функция по времени.
            return fitnessD;
        }
    }
}
