using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;

namespace SeaPortsGenetic_WPF
{
    class IOWorker
    {
        private string myDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private string SPGfolder = "\\Sea Ports Genetic";
        private string createdFilePath;
        
        //Конструктор
        public IOWorker()
        {
            DirectoryInfo dirInfo = new DirectoryInfo(myDocumentsPath + SPGfolder);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }
        }

        //Запись констант в файл .csv
        public void saveConstants()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(myDocumentsPath + SPGfolder + "\\Результаты.csv", true, Encoding.Default))
                {
                    writer.WriteLine("Прогон " + DateTime.Now.ToString() + ".");
                    writer.WriteLine("Эксплуатационная скорость судна (узлов): " + ";" + Constants.vessel_velocity);
                    writer.WriteLine("Средняя загрузка судна (TEU): " + ";" + Constants.vessel_load);
                    writer.WriteLine("Время вспомогательных операций судна для 1-го судозахода (ч): " + ";" + Constants.vessel_additionalTime);
                    writer.WriteLine("Производительность причального перегружателя STS (в TEU в ч): " + ";" + Constants.productivityStandart);
                    writer.WriteLine("Процент мутаций: " + ";" + Constants.percent_mutations);
                    writer.WriteLine();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка сохрнанения результатов ГА: " + e.Message);
            }
        }     
        
        //запись журнала в файл
        public void saveLogToFile(string toSave, out string result)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(myDocumentsPath + SPGfolder + "\\SeaPortsGenetic Log.txt"))
                {
                    writer.Write(toSave);
                }
                result = "Сохранение журнала успешно выполнено!";
            }
            catch (Exception e)
            {
                result = "Ошибка: " + e.Message;
            }
        }

        //запись в .csv файл. Шапка
        public void saveCSVfileTitle(double[,] distanceMatrix, double[,] timeMatrix, int[,] matrixQ)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(myDocumentsPath + SPGfolder + "\\Результаты.csv", true, Encoding.Default))
                {
                    writer.WriteLine("Матрица расстояний:");
                    for (int i = 0; i < distanceMatrix.GetLength(0); i++)
                    {
                        for (int j = 0; j < distanceMatrix.GetLength(1); j++)
                        {
                            writer.Write(distanceMatrix[i, j] + ";");
                        }
                        writer.WriteLine();
                    }
                    writer.WriteLine("Матрица времен перехода между портами:");
                    for (int i = 0; i < timeMatrix.GetLength(0); i++)
                    {
                        for (int j = 0; j < timeMatrix.GetLength(1); j++)
                        {
                            writer.Write(timeMatrix[i, j] + ";");
                        }
                        writer.WriteLine();
                    }
                    writer.WriteLine("Матрица грузооборотов (в TEU) между портами:");
                    for (int i = 0; i < matrixQ.GetLength(0); i++)
                    {
                        for (int j = 0; j < matrixQ.GetLength(1); j++)
                        {
                            writer.Write(matrixQ[i, j] + ";");
                        }
                        writer.WriteLine();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка сохрнанения результатов ГА: " + e.Message);
            }
        }

        //запись в .csv файл
        public void saveCSVfile(int stepNumber, Solution[] solPopulation)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(myDocumentsPath + SPGfolder + "\\Результаты.csv", true, Encoding.Default))
                {
                    writer.WriteLine("Шаг " + stepNumber);
                    for (int i = 0; i < solPopulation.Length; i++)
                    {
                        SeaPort[] solution = solPopulation[i].getSolution();
                        for (int j = 0; j < solution.Length; j++)
                        {
                            writer.Write(solution[j].getName() + ";");
                        }
                        writer.Write(solPopulation[i].getFitnessD() + ";");
                        writer.Write(solPopulation[i].getFitness() + ";");
                        writer.WriteLine();
                    }
                    writer.WriteLine();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка сохрнанения результатов ГА: " + e.Message);
            }
        }


        //Сохранение настроек модели
        public void saveSettings()
        {
            try
            {
                using (BinaryWriter writer = new BinaryWriter(File.Open(myDocumentsPath + SPGfolder + "\\settings.dat", FileMode.OpenOrCreate)))
                {
                    writer.Write(Constants.vessel_velocity);
                    writer.Write(Constants.vessel_load);
                    writer.Write(Constants.vessel_additionalTime);
                    writer.Write(Constants.productivityStandart);
                    writer.Write(Constants.percent_mutations);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка сохрнанения настроек модели: " + e.Message);
            }
        }

        //Чтение настроек модели
        public void readSettings()
        {
            try
            {
                using (BinaryReader reader = new BinaryReader(File.Open(myDocumentsPath + SPGfolder + "\\settings.dat", FileMode.Open)))
                {
                    while(reader.PeekChar() > -1)
                    {
                        Constants.vessel_velocity = reader.ReadDouble();
                        Constants.vessel_load = reader.ReadInt32();
                        Constants.vessel_additionalTime = reader.ReadDouble();
                        Constants.productivityStandart = reader.ReadDouble();
                        Constants.percent_mutations = reader.ReadDouble();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка чтения настроек модели: " + e.Message);
            }
        }
    }
}
