using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaPortsGenetic_WPF
{
    class SeaPort : Port
    {
        private List<LandPoint> connectedLandPoints = new List<LandPoint>();
        private double timeOfHandling; //время обработки судна в порту
        public double productivityRate { get; set; } = Constants.productivityStandart;

        //Стандартный конструктор
        public SeaPort(double x, double y) : base(x, y) { }
        //Пустой конструктор
        public SeaPort() : base() { }
        
        //getters
        public List<LandPoint> getConnectedLandPoints()
        {
            return connectedLandPoints;
        }
        //работа со списком наземных пунктов
        public void addConnectedLandPoint(LandPoint landPoint)
        {
            connectedLandPoints.Add(landPoint);
        }

        public void removeConnectedLandPoint(LandPoint landPoint)
        {
            connectedLandPoints.Remove(landPoint);
        }
        public void removeConnectedLandPoint(int index)
        {
            connectedLandPoints.RemoveAt(index);
        }

        //Others
        //Создание массива для хранения подключенных контейнеров
        public void connectAllContainers()
        {
            //подсчет количества контейнеров для инициализации массива контейнеров правильной длины
            int countContainers = 0;
            foreach (LandPoint lp in connectedLandPoints)
            {
                countContainers += lp.getContainersArray().Length;
            }
            //инициализация массива контейнеров
            containers = new Container[countContainers];
            //заполнение массива контейнерами из подключенных портов
            int lpCounter = 0;
            int j = 0;
            LandPoint landpoint = connectedLandPoints.ElementAt(lpCounter);
            for (int i = 0; i < containers.Length; i++)
            {
                if (j == landpoint.getContainersArray().Length)
                {
                    lpCounter++;
                    landpoint = connectedLandPoints.ElementAt(lpCounter);
                    j = 0;
                }
                containers[i] = landpoint.getContainersArray()[j];
                containers[i].setOrigin(this);
                j++;
            }
        }

        //удаление массива подключенных контейнеров
        public void removeAllContainers()
        {
            containers = new Container[0];
        }
        
    }

}
