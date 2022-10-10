using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SeaPortsGenetic_WPF
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {

        public SettingsWindow()
        {
            InitializeComponent();
            MainWindow mainW = (MainWindow)Owner;

            txtBox_percentMutations.AppendText(Constants.percent_mutations.ToString("0.0"));
            txtBox_vesselVelocity.AppendText(Constants.vessel_velocity.ToString("0.0"));
            txtBox_vesselLoad.AppendText(Constants.vessel_load.ToString("0"));
            txtBox_vesselAdditionalTime.AppendText(Constants.vessel_additionalTime.ToString("0.0"));
        }

        private void btnClicked_settingsOK(object sender, RoutedEventArgs e)
        {
            try
            {
                MainWindow mainW = (MainWindow)Owner;
                double percMutations;
                double vesselVelocity;
                int vesselLoad;
                double vesselAdditionalTime;

                bool isPercentMutations_parsed = tryParsePercent(txtBox_percentMutations.Text, out percMutations);
                bool isVesselVelocity_parsed = tryParseVesselVelocity(txtBox_vesselVelocity.Text, out vesselVelocity);
                bool isVesselLoad_parsed = tryParseVesselLoad(txtBox_vesselLoad.Text, out vesselLoad);
                bool isVesselAdditionalTime_parsed = tryParseVesselAdditionalTime(txtBox_vesselAdditionalTime.Text, out vesselAdditionalTime);

                if (isPercentMutations_parsed && isVesselVelocity_parsed && isVesselLoad_parsed && isVesselAdditionalTime_parsed)
                {
                    //Установка мутаций
                    Constants.percent_mutations = percMutations;
                    mainW.txtBox_systemOut.AppendText("Установлен процент мутаций: " + (Constants.percent_mutations) + "%;\n");

                    //Установка скорости судна
                    Constants.vessel_velocity = vesselVelocity;
                    mainW.txtBox_systemOut.AppendText("Установлена скорость судна: " + (Constants.vessel_velocity) + " узлов;\n");

                    //Установка загрузки судна в конт.
                    Constants.vessel_load = vesselLoad;
                    mainW.txtBox_systemOut.AppendText("Установлена загрузка расчетного судна: " + (Constants.vessel_load) + " конт.;\n");

                    //Установка времени доп стоянок судна, ч
                    Constants.vessel_additionalTime = vesselAdditionalTime;
                    mainW.txtBox_systemOut.AppendText("Установлено время всопомгательных операций на одно судно: " + (Constants.vessel_additionalTime) + " ч;\n");

                    //Обновление параметров модели в MainWindow
                    mainW.refresh();
                }
                else
                {
                    if (isPercentMutations_parsed == false) MessageBox.Show("Процент мутаций должен быть в диапазоне от 0 до 100.", "Ошибка");
                    if (isVesselVelocity_parsed == false) MessageBox.Show("Скорость судна должна быть в диапазоне от 1 до 50 узлов.", "Ошибка");
                    if (isVesselLoad_parsed == false) MessageBox.Show("Средняя загрузка судна должна быть в диапазоне от 1 до 25000 контейнеров.", "Ошибка");
                    if (isVesselAdditionalTime_parsed == false) MessageBox.Show("Время вспомогательных стоянок судна должно быть в диапазоне от 1 до 24 часов.", "Ошибка");
                }
            }
            catch (Exception exc)
            {
                string error = exc.Source + "\n" + exc.Message + "\n" + exc.StackTrace;
                MessageBox.Show(error, "Ошибка");
            }

        }

        private void btnClicked_settingsCancel(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Усовершенствованный метод Double.TryParse. Позволяет проходить парсинг только числам в промежутке от 0 до 100.
        /// </summary>
        /// <param name="toParse">Параметр типа string. Строка, которую нужно парсить.</param>
        /// <param name="result">Параметр типа out double. Ссылка на существующую переменную, в которую будет записан результат парсинга строки.</param>
        private bool tryParsePercent(string toParse, out double result)
        {
            bool answer = false;
            answer = Double.TryParse(toParse, out result);
            if (result < 0 || result > 100)
            {
                answer = false;
            }
            return answer;
        }

        private bool tryParseVesselVelocity(string toParse, out double result)
        {
            bool answer = false;
            answer = Double.TryParse(toParse, out result);
            if (result < 1 || result > 50)
            {
                answer = false;
            }
            return answer;
        }

        private bool tryParseVesselLoad(string toParse, out int result)
        {
            bool answer = false;
            answer = Int32.TryParse(toParse, out result);
            if (result < 1 || result > 25000)
            {
                answer = false;
            }
            return answer;
        }

        private bool tryParseVesselAdditionalTime(string toParse, out double result)
        {
            bool answer = false;
            answer = Double.TryParse(toParse, out result);
            if (result < 1 || result > 24)
            {
                answer = false;
            }
            return answer;
        }
    }
}
