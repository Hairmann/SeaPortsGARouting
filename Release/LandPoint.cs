using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaPortsGenetic_WPF
{
    class LandPoint : Port
    {
        private const int MIN_CONT = 50, MAX_CONT = 1000;
        private SeaPort spConnectedTo;

        public LandPoint(double x, double y) : base(x, y) { }
        //Пустой конструктор
        public LandPoint() : base() { }

        //get
        public SeaPort getSeaPortConnectedTo()
        {
            return spConnectedTo;
        }

        //set
        public void setSeaPortConnectedTo(SeaPort seaPort)
        {
            spConnectedTo = seaPort;
        }

        //Генерация случайного набора контейнеров
        public void generateRandomContainers(SeaPort[] seaPorts, Random random)
        {
            int nContainers = random.Next(MIN_CONT * getIndex(), MAX_CONT * getIndex());
            this.containers = new Container[nContainers];
            for (int i = 0; i < this.containers.Length; i++)
            {
                this.containers[i] = new Container(i);      //с установкой инедкса контейнера
            }

            //Установка случайного порта назначения
            foreach (Container cont in containers)
            {
                SeaPort i = seaPorts[random.Next(0, seaPorts.Length)];
                while (i == spConnectedTo)
                {
                    i = seaPorts[random.Next(0, seaPorts.Length)];
                }
                cont.setDestination(i);
            }
        }

    }
}
