using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaPortsGenetic_WPF
{
    class Container
    {
        private SeaPort destination;
        private SeaPort origin;
        private string imoType;
        private int index;

        //Конструктор с индексом контейнера
        public Container(int index)
        {
            this.index = index;
        }

        //Пустой конструктор
        public Container()
        {

        }

        //Методы
        //Getters
        public SeaPort getDestination()
        {
            return destination;
        }

        public SeaPort getOrigin()
        {
            return origin;
        }

        public string getIMOType()
        {
            return imoType;
        }

        public int getIndex()
        {
            return index;
        }

        //Setters
        public void setDestination(SeaPort destination)
        {
            this.destination = destination;
        }

        public void setOrigin(SeaPort origin)
        {
            this.origin = origin;
        }

        public void setIMOType(string imoType)
        {
            this.imoType = imoType;
        }

        public void setIndex(int index)
        {
            this.index = index;
        }
        
    }
}
