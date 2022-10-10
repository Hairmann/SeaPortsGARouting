using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SeaPortsGenetic_WPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int RADIUS = 200;
        private const int N_PORTS = 10;
        private Brush lineColor = Brushes.Red;
        private double[] europePortsX = { 510, 900, 800, 610, 555, 660 };
        private double[] europePortsY = { 630, 220, 350, 400, 430, 550 };

        //Переменные, касающиеся воркера
        BackgroundWorker worker_geneticAlgorithm = new BackgroundWorker();
        private bool isWorkerRunning = false;
        private string messageFromWorker = "";

        private int nSeaPorts = N_PORTS;
        private int nLandPoints;
        private readonly int nConnectedStandartly;
        private int nSteps = 1000;
        private SeaPort[] seaPorts = new SeaPort[N_PORTS]; //набор морских портов
        private LandPoint[] landPoints = new LandPoint[18]; //набор наземных пунктов 
        private static Solution[] solPopulation = new Solution[0]; //Популяция решений
        private Random random = new Random();
        private Ellipse[] ellipses;
        private Ellipse[] ellipsesLP;
        //Матрицы расстояний и грузооборотов
        private static double[,] distanceMatrix = new double[0, 0]; //Матрица L между морскими портами
        private static double[,] timeMatrix = new double[0, 0]; //матрица времен переходов между портами
        private static int[,] matrixQ = new int[0, 0]; //Матрица Q между морскими портами
        //Процент мутаций
        private double percentMutations = Constants.percent_mutations;

        //Вспомогательные (доп) переменные
        public double vesselVelocity { get; set; } = Constants.vessel_velocity;  //скорость судна по умолчанию 10 узлов

        //переменные, отвечающие за радио-переключатели
        private bool isSeaPortsRandom = true;
        private bool isSeaPortsContainerRandom = true;
        private bool isDorQ = true;

        //Переменная, отвечающая за установку портов на холсте
        private const int ELLIPSE_THICKNESS = 5;
        private double xMargin;
        private int nSetSeaPorts = 0;
        private bool allPortsAreSet = false;

        //Переменная для подсчета процента выполненого алгоритмом
        private int nStepsCompleted = 0;

        //ввод/вывод
        private IOWorker ioWorker = new IOWorker();

        //Переменные для графики



        public MainWindow()
        {
            InitializeComponent();
            //this.DataContext = this;

            txtBox_systemOut.AppendText(DateTime.Now.ToString() + ". Добро пожаловать в приложение SeaPortsGenetic!\nВерсия .03 от 13.02.2020\n" + 
                "Порядок работы с приложением:\n" + "1. Введите исходные данные в поля;\n" + "2. Нажмите кнопку <Сгенерировать>;\n" + "3. Нажмите кнопку Начать!.\n");
            string nSP = String.Format("{0}", nSeaPorts);
            string nStp = String.Format("{0}", nSteps);
            string pMutations = String.Format("{0}", percentMutations);
            txtBox_nSeaPortsInput.AppendText(nSP);
            txtBox_nSteps.AppendText(nStp);
            //initializePorts();

            //Инициализация количества наземных пунктов
            nConnectedStandartly = 3;
            nLandPoints = nSeaPorts * nConnectedStandartly;
        }

        //*****Привязка радио-кнопок к булевым переменным модели*******
        private void radio_randomSeaPorts(object sender, RoutedEventArgs e)
        {
            isSeaPortsRandom = true;
        }

        private void radio_graphicalSeaPorts(object sender, RoutedEventArgs e)
        {
            isSeaPortsRandom = false;
        }

        private void radio_randomContainer(object sender, RoutedEventArgs e)
        {
            isSeaPortsContainerRandom = true;
        }

        private void radio_byD(object sender, RoutedEventArgs e)
        {
            isDorQ = true;
            lineColor = Brushes.Red;
        }

        private void radio_byQ(object sender, RoutedEventArgs e)
        {
            isDorQ = false;
            lineColor = Brushes.DodgerBlue;
        }
        //*********************

        //Кнопка генерации портов
        private void btnClicked_generate(object sender, RoutedEventArgs e)
        {
            //Валидация ввода в поле портов
            bool parsing = tryParsePositiveInt(txtBox_nSeaPortsInput.Text, out nSeaPorts);
            //Число портов валидно
            if (parsing)
            {
                //1. ЗАГРУЗКА ПОРТОВ +++++ создание заданного числа морских портов
                //CSVreader csv = new CSVreader();
                //csv.run();

                //seaPorts = csv.results;
                //nSeaPorts = seaPorts.Length;
                //for (int i = 0; i < seaPorts.Length; i++)
                //{
                //    seaPorts[i].setIndex(i);
                //}
                //Генерация портов внутри модели******************
                seaPorts = new SeaPort[nSeaPorts];
                for (int i = 0; i < seaPorts.Length; i++)
                {
                    seaPorts[i] = new SeaPort();
                    seaPorts[i].setIndex(i);
                    seaPorts[i].setName("Port " + (i+1));
                }
                //******************************************
                //2. создание случайного числа подключенных к каждому порту наземных пунктов
                nLandPoints = nSeaPorts * nConnectedStandartly;
                landPoints = new LandPoint[nLandPoints];
                for (int i = 0; i < landPoints.Length; i++)
                {
                    landPoints[i] = new LandPoint();
                    landPoints[i].setIndex(i);
                }
                //3. соединение наземных пунктов с морскими портами
                for (int i = 0; i < seaPorts.Length; i++)
                {
                    seaPorts[i].addConnectedLandPoint(landPoints[i * nConnectedStandartly]);
                    landPoints[i * nConnectedStandartly].setSeaPortConnectedTo(seaPorts[i]);

                    seaPorts[i].addConnectedLandPoint(landPoints[i * nConnectedStandartly + 1]);
                    landPoints[i * nConnectedStandartly + 1].setSeaPortConnectedTo(seaPorts[i]);

                    seaPorts[i].addConnectedLandPoint(landPoints[i * nConnectedStandartly + 2]);
                    landPoints[i * nConnectedStandartly + 2].setSeaPortConnectedTo(seaPorts[i]);
                }
                //4. генерация случайного набора контейенров в каждом наземном пункте
                foreach (LandPoint lp in landPoints)
                {
                    lp.generateRandomContainers(seaPorts, random);
                }
                //5. подключение контейнеров к массивам контейнеров в портах
                foreach (SeaPort sp in seaPorts)
                {
                    sp.connectAllContainers();
                }

                //*****************TEST****************************
                //для круговой расстановки: setRoundPosition(seaPorts, canvas.ActualWidth / 2, canvas.ActualHeight / 2, RADIUS);
                setRoundPosition(seaPorts, canvas.ActualWidth / 2, canvas.ActualHeight / 2, RADIUS);

                //для расстановки по портам Европы
                //setEuropePosition(seaPorts);

                //для случайно расстановки:
                //setRandomPosition(seaPorts);

                //*0.3 установка позиций пунктов по кругу от портов
                foreach (SeaPort sp in seaPorts)
                {
                    LandPoint[] landPoints = sp.getConnectedLandPoints().ToArray();
                    setRoundPosition(landPoints, sp.getX(), sp.getY(), 10);
                }

                double radius = RADIUS;
                double perimeter = seaPorts.Length * Math.Sin(Math.PI / seaPorts.Length) * 2 * radius;
                txtBox_systemOut.AppendText(DateTime.Now.ToString() + ". Длина искомого решения равна: " + String.Format("{0:0.00}", perimeter) + "\n");
                //************************************************

                //Генерация имен портов
                //Имена с номерами:
                //for (int i = 0; i < seaPorts.Length; i++)
                //{
                //    seaPorts[i].setName("Порт" + (i + 1));
                //}

                //Имена портов Европы:
                //seaPorts[0].setName("Валенсия");
                //seaPorts[1].setName("Санкт-Петербург");
                //seaPorts[2].setName("Гданьск");
                //seaPorts[3].setName("Роттердам");
                //seaPorts[4].setName("Гавр");
                //seaPorts[5].setName("Генуя");

                //+++++++++++++++БЕЗ НАЗЕМНЫХ ПУНКТОВ (НАЗЕМНЫЕ ПУНКТЫ НЕ ГЕНЕРИРУЮТСЯ)
                //Генерация имен наземных пунктов
                for (int i = 0; i < landPoints.Length; i++)
                {
                    landPoints[i].setName("Пункт " + (i + 1));
                }


                //Создание матрицы грузооборотов между наземными
                matrixQ = new int[nSeaPorts, nSeaPorts];
                for (int i = 0; i < nSeaPorts; i++)
                {
                    foreach (Container cont in seaPorts[i].getContainersArray())
                    {
                        matrixQ[i, cont.getDestination().getIndex()]++;
                    }
                }
                //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

                //Создание матрицы расстояний между портами
                distanceMatrix = new double[nSeaPorts, nSeaPorts];
                for (int i = 0; i < nSeaPorts; i++)
                {
                    for (int j = 0; j < nSeaPorts; j++)
                    {
                        if (i != j)
                        {
                            distanceMatrix[i, j] = calculateDistance(seaPorts[i], seaPorts[j]);
                        }
                        else
                        {
                            distanceMatrix[i, j] = 0;
                        }
                    }
                }

                //Создание матрицы времен переходов между портами
                timeMatrix = new double[nSeaPorts, nSeaPorts];
                for (int i = 0; i < nSeaPorts; i++)
                {
                    for (int j = 0; j < nSeaPorts; j++)
                    {
                        if (i != j)
                        {
                            timeMatrix[i, j] = distanceMatrix[i, j] / Constants.vessel_velocity;
                        }
                        else
                        {
                            timeMatrix[i, j] = 0;
                        }
                    }
                }


                //Вывод информации о портах
                foreach (SeaPort sp in seaPorts)
                {
                    txtBox_systemOut.AppendText(sp.getName() + ". X порта: " + String.Format("{0:0.00}", sp.getX()) + ", Y порта: " + String.Format("{0:0.00}", sp.getY()) + ", Кол-во контейнеров:  " + sp.getContainersArray().Length + "\n");
                }
                txtBox_systemOut.AppendText("Созданные наземные пункты:\n");
                //Вывод информации о наземных пунктах
                foreach (LandPoint lp in landPoints)
                {
                    txtBox_systemOut.AppendText(lp.getName() + ". Подключен к " + lp.getSeaPortConnectedTo().getName() + ". Кол-во контейнеров:  " + lp.getContainersArray().Length + "\n");
                }
                
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!вывод матрицы Q в консоль
                printMatrixQ();
                printMatrixT();
                /*
                foreach (Container cont in seaPorts[1].getContainersArray())
                {
                    txtBox_systemOut.AppendText("Контейнер №" + cont.getIndex() + ". Порт назначения: " + cont.getDestination().getName() + "\n");
                }
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                */

                ellipses = new Ellipse[nSeaPorts];

                for (int i = 0; i < ellipses.Length; i++)
                {
                    ellipses[i] = new Ellipse();
                    setEllipsePosition(ellipses[i], seaPorts[i].getX(), seaPorts[i].getY());
                    setPortNameLabel(seaPorts[i].getName(), seaPorts[i].getX(), seaPorts[i].getY());
                }

                //*0.3 расстановка лэнд пунктов
                //ellipsesLP = new Ellipse[nLandPoints];
                //for (int i = 0; i < ellipsesLP.Length; i++)
                //{
                //    ellipsesLP[i] = new Ellipse();
                //    setEllipsesForLandPoints(ellipsesLP[i], landPoints[i].getX(), landPoints[i].getY());
                //    //setPortNameLabel(landPoints[i].getName(), landPoints[i].getX(), landPoints[i].getY());
                //}
            }
            //число портов не валидно
            else
            {
                MessageBox.Show("Необходимо ввести целое положительное число морских портов.", "Ошибка");
                txtBox_systemOut.AppendText(DateTime.Now.ToString() + ". Не удалось создать набор морских портов.\n");
            }
        }

        private void btnClicked_clear(object sender, RoutedEventArgs e)
        {
            canvas.Children.Clear();
            canvas.Children.Add(rectCanvas);
            //canvas.Children.Add(img_BackgroundMap);
        }

        private void btnClicked_start(object sender, RoutedEventArgs e)
        {
            //Проверка - worker запущен или нет. Если да - то это кнопка отмены.
            if (isWorkerRunning)
            {
                worker_geneticAlgorithm.CancelAsync();
                isWorkerRunning = false;
                btn_Start.Content = "Начать!";
            }
            else
            {
                //Проверка валидности исходных данных
                bool parsingSteps = tryParsePositiveInt(txtBox_nSteps.Text, out nSteps);
                double percentMutations;
                bool parsingMutations = true; //   tryParsePercent(txtBox_percentMutations.Text, out percentMutations);
                if (parsingMutations && parsingSteps)
                {
                    //*******Данные валидны*********
                    nStepsCompleted = 0;
                    pgBar_geneticAlgorithm.Visibility = Visibility.Visible;

                    //********Очистка сообщения от воркера*********
                    messageFromWorker = "";

                    //Запуск ГА из бэкграунд воркера
                    worker_geneticAlgorithm = new BackgroundWorker();
                    worker_geneticAlgorithm.DoWork += worker_geneticAlgorithmStart;
                    worker_geneticAlgorithm.WorkerReportsProgress = true;
                    worker_geneticAlgorithm.WorkerSupportsCancellation = true;
                    worker_geneticAlgorithm.RunWorkerCompleted += worker_geneticAlgorithmCompleted;
                    worker_geneticAlgorithm.ProgressChanged += worker_geneticAlgorithmProgress;
                    worker_geneticAlgorithm.RunWorkerAsync();

                    //Замена кнопки "Старт" на кнопку "Отмена"
                    isWorkerRunning = true;
                     btn_Start.Content = "Остановить!";
                }
                else
                {
                    //*******Данные не валидны*******
                    string errorMessage = "";
                    if (parsingSteps == false)
                    {
                        errorMessage += "Необходимо ввести целое положительное число шагов генетического алгоритма.\n";
                    }
                    if (parsingMutations == false)
                    {
                        errorMessage += "Необходимо ввести процент мутаций от 0 до 100.";
                    }
                    MessageBox.Show(errorMessage, "Ошибка");
                    txtBox_systemOut.AppendText(DateTime.Now.ToString() + ". Не удалось запустить генетический алгоритм.\n");
                }
            }
        }
        
        private void worker_geneticAlgorithmStart(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            
            /*
            txtBox_systemOut.AppendText(DateTime.Now.ToString() + ". Запуск генетического алгоритма.\n");
            */
            initializeSolutions();

            //сохранение результатов (шапка)
            ioWorker.saveConstants();
            ioWorker.saveCSVfileTitle(distanceMatrix, timeMatrix, matrixQ);

            for (int i = 0; i < nSteps; i++)
            {
                //Проверка нажатия кнопки отмены
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    return;
                }
                
                sortPopulation(solPopulation, isDorQ);  //сортировка популяции решений от лучшего к худшему

                int nBest = (int)(solPopulation.Length / 2);
                if (nBest % 2 != 0)
                {
                    nBest = ((int)(solPopulation.Length / 2) + 1);  //определение количества лучших решений (так чтобы их всегда было четное кол-во)
                }



                Solution[] virtualPopulation = new Solution[nBest]; //создание виртуальной популяции решений для заполнения потомками
                for (int j = 0; j < virtualPopulation.Length; j++)
                {
                    virtualPopulation[j] = new Solution(seaPorts, random);    //инициализация популяции решений
                    virtualPopulation[j].setMatrixQ(matrixQ);
                    virtualPopulation[j].setMatrixL(distanceMatrix);
                }

                //Размножение особей по 2 - из каждой пары по 4 решения
                for (int j = 0, count = 0; j < (nBest / 2); j++, count += 2)
                {
                    breed(solPopulation[j], solPopulation[j + 1], isDorQ, ref virtualPopulation[count], ref virtualPopulation[count + 1]);
                }

                int counter = 0;
                for (int j = nBest; j < solPopulation.Length; j++)
                {
                    solPopulation[j] = solPopulation[counter];
                    counter++;
                }
                for (int j = 0; j < nBest; j++)
                {
                    solPopulation[j] = virtualPopulation[j];
                }

                //*********Мутации********
                
                if (randomTrue(Convert.ToInt32(percentMutations))) //лень было менять всё на double, поэтому ввод в double, a расчет randomTrue - в int
                {
                    int mutantIndex = random.Next(0, solPopulation.Length); //Выбрать случайного мутанта из популяции
                    solPopulation[mutantIndex].swap2ports(random.Next(0, solPopulation[mutantIndex].getSolution().Length - 2), random.Next(0, solPopulation[mutantIndex].getSolution().Length - 2));    //переставить местами два случайных порта в мутанте
                }
                
                //************************

                /*
                if (solPopulation.Length > virtualPopulation.Length)
                {
                    for (int j = 0; j < solPopulation.Length; j++)
                    {
                        solPopulation[j] = j < virtualPopulation.Length ? virtualPopulation[j] : solPopulation[0];  //ОШИБКА, НАМЕРЕННАЯ - в последнее решение из популяции не переносится первое старое решение, а вместо этого просто копируется первое решение-потомок
                    }
                }
                else
                {
                    for (int j = 0; j < solPopulation.Length; j++)
                    {
                        solPopulation[j] = virtualPopulation[j];
                    }
                }
                */


                /*
                txtBox_systemOut.AppendText("Шаг " + (i + 1) + ":\n");
                */
                if (i % (nSteps / 10) == 0)
                {
                    messageFromWorker += ("Шаг " + (i + 1) + ":\n");
                    messageWorker_appendSolutions(solPopulation);

                    //сохранение результатов
                    ioWorker.saveCSVfile(i, solPopulation);
                }


                sortPopulation(solPopulation, isDorQ);


                double percentageCompleted = (i * 100) / nSteps;
                int percentageCompletedInt = Convert.ToInt32(percentageCompleted);
                worker.ReportProgress(percentageCompletedInt);
            }
        }

        private void worker_geneticAlgorithmCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                pgBar_geneticAlgorithm.Value = 0;
                pgBar_geneticAlgorithm.Visibility = Visibility.Hidden;
                txtBox_systemOut.AppendText(DateTime.Now.ToString() + ". Работа генетического алгоритма прервана пользователем!\n");
                //printSolutions(solPopulation);
                txtBox_systemOut.AppendText(messageFromWorker);

                drawLines(solPopulation[0]);
            }
            else
            {
                pgBar_geneticAlgorithm.Value = 0;
                pgBar_geneticAlgorithm.Visibility = Visibility.Hidden;
                txtBox_systemOut.AppendText(DateTime.Now.ToString() + ". Работа генетического алгоритма успешно завершена.\n");
                //printSolutions(solPopulation);
                txtBox_systemOut.AppendText(messageFromWorker);

                btn_Start.Content = "Начать!";

                drawLines(solPopulation[0]);
            }
            
        }

        private void worker_geneticAlgorithmProgress(object sender, ProgressChangedEventArgs e)
        {
            //Обновление погрессбара
            pgBar_geneticAlgorithm.Value = e.ProgressPercentage;

            //Отрисвока линий
            if (checkBox_drawLines.IsChecked == true)
            {
                drawLines(solPopulation[0]);
            }
            //Вывод решений на экран
            //printSolutions(sol_Population);
        }

        //Генерация круговой расстановки портов (для теста)
        private void setRoundPosition(Port[] ports, double centerX, double centerY, double radius)
        {
            double kX = 1;
            double kY = 1;
            for (int i = 0; i < ports.Length; i++)
            {
                if (ports[i] is LandPoint)
                {
                    kX = random.NextDouble() * 5;
                    kY = random.NextDouble() * 5;
                }
                ports[i].setX(Math.Cos((2 * Math.PI) / ports.Length * i) * radius * kX + centerX);
                ports[i].setY(Math.Sin((2 * Math.PI) / ports.Length * i) * radius * kY + centerY);
            }
        }

        //*0.3 Генерация случайной расстановки портов (для теста)
        private void setRandomPosition(Port[] ports)
        {
            int upperBound_X = (int)(canvas.ActualWidth);
            int upperBound_Y = (int)(canvas.ActualHeight);
            for (int i = 0; i < ports.Length; i++)
            {
                ports[i].setX(random.Next(150, upperBound_X - 150));
                ports[i].setY(random.Next(150, upperBound_Y - 150));
            }
        }

        //генерация заданной расстановки - порты Европы
        private void setEuropePosition(Port[] ports)
        {
            try
            {
                for (int i = 0; i < ports.Length; i++)
                {
                    ports[i].setX(europePortsX[i]);
                    ports[i].setY(europePortsY[i]);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка: " + e.Message);
            }
        }

        //Вызов окна настроек
        private void btnClicked_settings(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;

            settingsWindow.Show();
        }

        //Сохранение журнала событий
        private void btnClicked_saveLog(object sender, RoutedEventArgs e)
        {
            string toSave = txtBox_systemOut.Text;
            string result;
            
            ioWorker.saveLogToFile(toSave, out result);

            txtBox_systemOut.AppendText(result + "\n");
        }

        //Сохранение состояния модели
        private void btnClicked_saveSettings(object sender, RoutedEventArgs e)
        {
            ioWorker.saveSettings();
        }

        //Сохранение состояния модели
        private void btnClicked_openSettings(object sender, RoutedEventArgs e)
        {
            ioWorker.readSettings();
        }

        //Улавливание нажатия кнопки мыши на холсте
        private void canvas_LMB(object sender, MouseButtonEventArgs e)
        {
            if (isSeaPortsRandom)
            {
                MessageBox.Show("Нельзя задавать положение портов графически. Включите графический способ задания положения портов.", "Ошибка");
                txtBox_systemOut.AppendText(DateTime.Now.ToString() + ". Запрещено задавать положение портов графически.\n");
            }
            else
            {
                xMargin = MainGrid.ColumnDefinitions.ElementAt(0).ActualWidth + MainGrid.ColumnDefinitions.ElementAt(1).ActualWidth;
                Point p = e.GetPosition(this);
                setEllipsePosition(new Ellipse(), p.X - xMargin, p.Y);
                txtBox_systemOut.AppendText(DateTime.Now.ToString() + ". X: " + String.Format("{0}", p.X) + ", Y: " + String.Format("{0}", p.Y) + "\n");
            }
            
            /****** не работает
            if (isSeaPortsRandom == false)
            {
                if (allPortsAreSet == true)
                {
                    MessageBox.Show("Координаты всех портов стерты.");
                    canvas.Children.Clear();
                    nSetSeaPorts = 0;
                    allPortsAreSet = false;
                }
                Point p = e.GetPosition(this);
                //MessageBox.Show("Координата x=" +p.X.ToString()+ " y="+p.Y.ToString());
                seaPorts[nSetSeaPorts].setX(p.X);
                seaPorts[nSetSeaPorts].setY(p.Y);
                if (ellipses[nSetSeaPorts] == null)
                {
                    ellipses[nSetSeaPorts] = new Ellipse();
                }
                setEllipsePosition(ellipses[nSetSeaPorts], seaPorts[nSetSeaPorts].getX(), seaPorts[nSetSeaPorts].getY());
                setPortNameLabel(seaPorts[nSetSeaPorts].getName(), seaPorts[nSetSeaPorts].getX(), seaPorts[nSetSeaPorts].getY());

                //Проверка, что устанавливается последний порт
                if (nSetSeaPorts == seaPorts.Length - 1)
                {
                    allPortsAreSet = true;
                }
                else
                {
                    nSetSeaPorts++;
                }
            }
            else
            {
                MessageBox.Show("Графический способ задания координат портов запрещен. Поменяйте способ задания расположения портов.");
            }
            */
        }

        private void initializeSolutions()
        {
            //Временно
            int nSolutions = Convert.ToInt32(nSeaPorts * 1.5);
            //parsePositiveInt(out nSolutions);
            solPopulation = new Solution[nSolutions]; //инициализация популяции решений
            for (int i = 0; i < nSolutions; i++)
            {
                solPopulation[i] = new Solution(seaPorts, random); //каждое решение в популяции = массив seaPorts
                solPopulation[i].shuffle(); //рандомные перестановки решений

                solPopulation[i].setMatrixQ(matrixQ); //передача матрицы Q в решение
                solPopulation[i].setMatrixL(distanceMatrix); //передача матрицы L в решение

                //Запись в месседж от воркера
                messageFromWorker += ("Решение " + (i + 1) + ":\t");
                int count = 0;
                foreach (SeaPort sp in solPopulation[i].getSolution())
                {
                    messageFromWorker += (sp.getName());

                    if (count < solPopulation[i].getSolution().Length - 1)
                    {
                        messageFromWorker += (" -> ");
                    }
                    else
                    {
                        messageFromWorker += ("\tФФ грузооборот в конт. = " + solPopulation[i].getFitness());
                        messageFromWorker += ("\tФФ время в ч. = " + String.Format("{0:0.0}", solPopulation[i].getFitnessD()));
                    }
                    count++;
                }
                messageFromWorker += ("\n");
            }
        }

        private void breed(Solution sol1, Solution sol2, bool DorQ, ref Solution child1, ref Solution child2)       //функция размножения двух особей на две другие особи
        {
            //*Для предварительного отбора лучших
            Solution childCandidate1 = new Solution(seaPorts, random);
            Solution childCandidate2 = new Solution(seaPorts, random);
            Solution childCandidate3 = new Solution(seaPorts, random);
            Solution childCandidate4 = new Solution(seaPorts, random);
            Solution[] childCandidates = new Solution[] { childCandidate1, childCandidate2, childCandidate3, childCandidate4 };
            foreach (Solution chCandidate in childCandidates)
            {
                chCandidate.setMatrixQ(matrixQ);
                chCandidate.setMatrixL(distanceMatrix);
            }
            //*


            bool[] mask_Child1 = new bool[nSeaPorts];
            bool[] mask_Child2 = new bool[nSeaPorts];
            bool[] mask_Child3 = new bool[nSeaPorts];
            bool[] mask_Child4 = new bool[nSeaPorts];

            for (int j = 0; j < mask_Child1.Length; j++)
            {
                mask_Child1[j] = false;
                mask_Child2[j] = false;
                mask_Child3[j] = false;
                mask_Child4[j] = false;
            }

            int[] childIndex1 = new int[sol1.getSolution().Length];
            int[] childIndex2 = new int[sol1.getSolution().Length];
            int[] childIndex3 = new int[sol1.getSolution().Length];
            int[] childIndex4 = new int[sol1.getSolution().Length];

            double halfDouble = sol1.getSolution().Length / 2;
            int half = (int)(Math.Round(halfDouble));
            int geneStartIndex = random.Next(0, half);
            int geneEndIndex = geneStartIndex + half;


            //int nParents = half % 2 == 0 ? half : half + 1;

            for (int j = 0; j < sol1.getSolution().Length - 1; j++)
            {
                if (j > geneStartIndex && j <= geneEndIndex)
                {
                    childIndex2[j] = sol1.getSolution()[j].getIndex();
                    mask_Child2[childIndex2[j]] = true;
                    childIndex4[j] = sol2.getSolution()[j].getIndex();
                    mask_Child4[childIndex4[j]] = true;
                }
                else
                {
                    //Ребенок 1 наследует полную последовательность генов от родителя 1; ребенок 3 наследует эти же номера генов от Родителя 2
                    childIndex1[j] = sol1.getSolution()[j].getIndex();
                    mask_Child1[childIndex1[j]] = true;
                    childIndex3[j] = sol2.getSolution()[j].getIndex();
                    mask_Child3[childIndex3[j]] = true;
                }
            }

            /*****
            Console.WriteLine("Маски:\t");
            Console.Write("Маска 1:\t");
            for (int k = 0; k < mask_Child1.Length; k++)
            {
                Console.Write(mask_Child1[k] + "\t");
                if (k == mask_Child1.Length - 1) Console.WriteLine();
            }

            Console.Write("Маска 2:\t");
            for (int k = 0; k < mask_Child1.Length; k++)
            {
                Console.Write(mask_Child2[k] + "\t");
                if (k == mask_Child2.Length - 1) Console.WriteLine();
            }

            Console.Write("Маска 3:\t");
            for (int k = 0; k < mask_Child1.Length; k++)
            {
                Console.Write(mask_Child3[k] + "\t");
                if (k == mask_Child3.Length - 1) Console.WriteLine();
            }

            Console.Write("Маска 4:\t");
            for (int k = 0; k < mask_Child4.Length; k++)
            {
                Console.Write(mask_Child4[k] + "\t");
                if (k == mask_Child4.Length - 1) Console.WriteLine();
            }
            */

            //***********TEMPORARY*************************
            //Временное решение: нехватающие гены в каждом из 4-х решений генерируются случайно, как и в генетических химерах
            int num = random.Next(0, nSeaPorts);

            for (int j = 0; j < sol1.getSolution().Length - 1; j++)
            {
                if (j > geneStartIndex && j <= geneEndIndex)
                {

                    while (mask_Child1[num])
                    {
                        num = random.Next(0, nSeaPorts);
                    }
                    childIndex1[j] = num;
                    mask_Child1[num] = true;

                    while (mask_Child3[num])
                    {
                        num = random.Next(0, nSeaPorts);
                    }
                    childIndex3[j] = num;
                    mask_Child3[num] = true;
                }
                else
                {
                    while (mask_Child2[num])
                    {
                        num = random.Next(0, nSeaPorts);
                    }
                    childIndex2[j] = num;
                    mask_Child2[num] = true;


                    while (mask_Child4[num])
                    {
                        num = random.Next(0, nSeaPorts);
                    }
                    childIndex4[j] = num;
                    mask_Child4[num] = true;
                }
            }

            childCandidate1.assignPorts(childIndex1);
            childCandidate2.assignPorts(childIndex2);
            childCandidate3.assignPorts(childIndex3);
            childCandidate4.assignPorts(childIndex4);

            sortPopulation(childCandidates, DorQ);

            //*********************************************
            child1 = childCandidates[0];
            child2 = childCandidates[1];
        }

        private static void sortPopulation(Solution[] solPopulation, bool DorQ)     //Тупая сортировка пузырьком. Если будет работать медленно - сделать поумнее: quicksort, например
        {
            //Неудачное разделение на вызов двух разных функций от Solution.
            if (DorQ == false)
            {
                Solution solTemp;
                for (int i = 0; i < solPopulation.Length - 1; i++)
                {
                    for (int j = 0; j < solPopulation.Length - i - 1; j++)
                    {
                        if (solPopulation[j + 1].getFitness() > solPopulation[j].getFitness())
                        {
                            solTemp = solPopulation[j + 1];
                            solPopulation[j + 1] = solPopulation[j];
                            solPopulation[j] = solTemp;
                        }
                    }
                }
            }
            else
            {
                Solution solTemp;
                for (int i = 0; i < solPopulation.Length - 1; i++)
                {
                    for (int j = 0; j < solPopulation.Length - i - 1; j++)
                    {
                        if (solPopulation[j + 1].getFitnessD() < solPopulation[j].getFitnessD())
                        {
                            solTemp = solPopulation[j + 1];
                            solPopulation[j + 1] = solPopulation[j];
                            solPopulation[j] = solTemp;
                        }
                    }
                }
            }
        }

        //Графика
        private void setEllipsePosition(Ellipse ell, double x, double y)
        {
            ell.Width = ELLIPSE_THICKNESS * 2;
            ell.Height = ELLIPSE_THICKNESS * 2;
            ell.Fill = lineColor;
            double left = x - ell.Width / 2;
            double top = y - ell.Height / 2;
            ell.Margin = new Thickness(left, top, 0, 0);

            canvas.Children.Add(ell);
        }

        private void setEllipsesForLandPoints(Ellipse ell, double x, double y)
        {
            ell.Width = ELLIPSE_THICKNESS;
            ell.Height = ELLIPSE_THICKNESS;
            ell.Fill = Brushes.DodgerBlue;
            double left = x - ell.Width / 2;
            double top = y - ell.Height / 2;
            ell.Margin = new Thickness(left, top, 0, 0);

            canvas.Children.Add(ell);
        }

        private void setPortNameLabel(string title, double x, double y)
        {
            Label portName = new Label();
            portName.Content = title;
            double left = x - 25;
            double top = y - 35;
            portName.Margin = new Thickness(left, top, 0, 0);
            portName.FontSize = 16;

            canvas.Children.Add(portName);
        }

        private void drawLines(Solution solution)
        {
            IEnumerable<Line> lines = canvas.Children.OfType<Line>();
            
            /*Старые линии удаляются*/
            int n_Lines = lines.Count();
            for (int i = 0; i < n_Lines; i++)
            {
                canvas.Children.Remove(lines.ElementAt(0));
            }

            /*Старые линии становятся серыми
            foreach (Line existingLine in lines)
            {
                existingLine.Stroke = Brushes.DarkGray;
            }
            */

            SeaPort[] spSolution = solution.getSolution();
            for (int i = 0; i < spSolution.Length - 1; i++)
            {
                int index = spSolution[i].getIndex();
                int indexNext = spSolution[i + 1].getIndex();
                Line lineNew = new Line();
                lineNew.X1 = ellipses[index].Margin.Left + ELLIPSE_THICKNESS / 2;
                lineNew.Y1 = ellipses[index].Margin.Top + ELLIPSE_THICKNESS / 2;
                lineNew.X2 = ellipses[indexNext].Margin.Left + ELLIPSE_THICKNESS / 2;
                lineNew.Y2 = ellipses[indexNext].Margin.Top + ELLIPSE_THICKNESS / 2;
                lineNew.Stroke = lineColor;
                lineNew.StrokeThickness = 1;
                canvas.Children.Add(lineNew);
            }

        }

        
        //Вспомогательное
        private bool randomTrue(int probabilityTrue)
        {
            bool answer;
            if (random.Next(0, 100) < probabilityTrue)
            {
                answer = true;
            }
            else
            {
                answer = false;
            }
            return answer;
        }

        private static double calculateDistance(SeaPort sp1, SeaPort sp2)
        {
            double dist = Math.Sqrt(Math.Pow((sp2.getX() - sp1.getX()), 2) + Math.Pow((sp2.getY() - sp1.getY()), 2));
            return dist;
        }

        /// <summary>
        /// Метод, выводящий в лог приложения информацию о каждом решении в популяции решений. 
        /// В лог выводится следующая информация:
        /// \n- номер решения в популяции;
        /// \n- последовательность портов в решении;
        /// \n- значение фитнесс-функции решения по количеству перемещенных контейнеров;
        /// \n- значение фитнесс-функции решения по расстоянию.
        /// </summary>
        /// <param name="solPopulation">Популяция решений, из которой решения выводятся в лог. Представлена массивом решений типа Solution[].</param>
        private void printSolutions(Solution[] solPopulation)    //вывод на экран информации о всех решениях в популяции
        {
            for (int i = 0; i < solPopulation.Length; i++)
            {
                txtBox_systemOut.AppendText("Решение " + (i + 1) + ":\t");
                int count = 0;
                foreach (SeaPort sp in solPopulation[i].getSolution())
                {
                    txtBox_systemOut.AppendText(sp.getName());

                    if (count < solPopulation[i].getSolution().Length - 1)
                    {
                        txtBox_systemOut.AppendText(" -> ");
                    }
                    else
                    {
                        txtBox_systemOut.AppendText("\tФФ грузооборт в конт. = " + solPopulation[i].getFitness());
                        txtBox_systemOut.AppendText("\tФФ время в ч. = " + String.Format("{0:0.0}", solPopulation[i].getFitnessD()));
                    }
                    count++;
                }
                txtBox_systemOut.AppendText("\n");
            }
        }

        /// <summary>
        /// Метод для добавления к строке сообщения от воркера инфу о решениях.
        /// </summary>
        /// <param name="solPopulation">Популяция решений, из которой решения выводятся в лог. Представлена массивом решений типа Solution[].</param>
        private void messageWorker_appendSolutions(Solution[] solPopulation)    //вывод на экран информации о всех решениях в популяции
        {
            for (int i = 0; i < solPopulation.Length; i++)
            {
               messageFromWorker += ("Решение " + (i + 1) + ":\t");
                int count = 0;
                foreach (SeaPort sp in solPopulation[i].getSolution())
                {
                    messageFromWorker += (sp.getName());

                    if (count < solPopulation[i].getSolution().Length - 1)
                    {
                        messageFromWorker += (" -> ");
                    }
                    else
                    {
                        messageFromWorker += ("\tФФ грузооборт в конт. = " + solPopulation[i].getFitness());
                        messageFromWorker += ("\tФФ время в ч. = " + String.Format("{0:0.0}", solPopulation[i].getFitnessD()));
                    }
                    count++;
                }
                messageFromWorker += ("\n");
            }
        }

        /*
        /// <summary>
        /// Используется для обращения к объекту Dispatcher, чтобы через него обратиться к элементу UI: txtBox. Передает в txtBox сообщение для отображения в логе приложения.
        /// </summary>
        /// <param name="message">Параметр типа string. Сообщение для отображения в логе приложения.</param>
        private void appendTextVIADispatcher(string message)
        {
            this.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        (ThreadStart)delegate ()
                        {
                            txtBox_systemOut.AppendText(message);
                        }
                          );
        }
        */

        /**Валидация ввода данных */

        /// <summary>
        /// Усовершенствованный метод Int32.TryParse. Позволяет проходить парсинг только целым положительным числам.
        /// </summary>
        /// <param name="toParse">Параметр типа string. Строка, которую нужно парсить.</param>
        /// <param name="result">Параметр типа out int. Ссылка на существующую переменную, в которую будет записан результат парсинга строки.</param>
        private bool tryParsePositiveInt(string toParse, out int result)
        {
            bool answer = false;
            answer = Int32.TryParse(toParse, out result);
            if (result <= 0)
            {
                answer = false;
            }
            return answer;
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

        //Вывод матрицы грузооборотов между портами
        private void printMatrixQ()
        {
            //Вывод на экран
            txtBox_systemOut.AppendText("Матрица грузооборотв между морскими портами (в конт.):\n");
            txtBox_systemOut.AppendText("\t");

            for (int i = 0; i < nSeaPorts; i++)
            {
                txtBox_systemOut.AppendText(seaPorts[i].getName() + "\t");
                if (i == nSeaPorts - 1)
                {
                    txtBox_systemOut.AppendText("\n");
                }
            }
            for (int i = 0; i < nSeaPorts; i++)
            {
                txtBox_systemOut.AppendText(seaPorts[i].getName() + ":\t");
                for (int j = 0; j < nSeaPorts; j++)
                {
                    txtBox_systemOut.AppendText(matrixQ[i, j] + "\t");
                }
                txtBox_systemOut.AppendText("\n");
            }
        }

        //*0.3 Вывод матрицы времен переходов между портами
        private void printMatrixT()
        {
            double timeBetweenPorts = 0;
            //Вывод на экран
            txtBox_systemOut.AppendText("Матрица времен переходов между морскими портами (в часах), при скорости судна 10 узлов:\n");
            txtBox_systemOut.AppendText("\t");

            for (int i = 0; i < nSeaPorts; i++)
            {
                txtBox_systemOut.AppendText(seaPorts[i].getName() + "\t");
                if (i == nSeaPorts - 1)
                {
                    txtBox_systemOut.AppendText("\n");
                }
            }
            for (int i = 0; i < nSeaPorts; i++)
            {
                txtBox_systemOut.AppendText(seaPorts[i].getName() + ":\t");
                for (int j = 0; j < nSeaPorts; j++)
                {
                    timeBetweenPorts = distanceMatrix[i, j] / vesselVelocity;
                    txtBox_systemOut.AppendText(String.Format("{0:0.0}", timeBetweenPorts) + "\t");
                }
                txtBox_systemOut.AppendText("\n");
            }
        }


        //**********QuickSort строки типа (double) из двумерного массива MatrixD*********
        //метод для обмена элементов массива
        private void swapPorts(ref double x, ref double y)
		{
			var t = x;
			x = y;
			y = t;
		}

		//метод возвращающий индекс опорного элемента
		private int Partition(double[] array, int minIndex, int maxIndex)
		{
			var pivot = minIndex - 1;
			for (var i = minIndex; i < maxIndex; i++)
			{
				if (array[i] < array[maxIndex])
				{
					pivot++;
					swapPorts(ref array[pivot], ref array[i]);
				}
			}

			pivot++;
			swapPorts(ref array[pivot], ref array[maxIndex]);
			return pivot;
		}

		//быстрая сортировка
		private double[] QuickSort(double[] array, int minIndex, int maxIndex)
		{
			if (minIndex >= maxIndex)
			{
				return array;
			}

			var pivotIndex = Partition(array, minIndex, maxIndex);
			QuickSort(array, minIndex, pivotIndex - 1);
			QuickSort(array, pivotIndex + 1, maxIndex);

			return array;
		}

		private double[] QuickSort(double[] array)
		{
			return QuickSort(array, 0, array.Length - 1);
		}
        //**********QuickSort закончился*********

        //обновление всех вспомогательных параметров
        public void refresh()
        {
            percentMutations = Constants.percent_mutations;
            vesselVelocity = Constants.vessel_velocity;
        }
    }
}
