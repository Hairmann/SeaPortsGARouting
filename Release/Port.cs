using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaPortsGenetic_WPF
{
    class Port
    {
        
        protected int indexInSeaPortsArray;
        protected int shore;

        protected double x, y;
        protected Container[] containers;
        protected string name;

        //Стандартный конструктор
        public Port(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        //Пустой конструктор
        public Port() { }

        //Методы
        //Getters
        public double getX()
        {
            return this.x;
        }

        public double getY()
        {
            return this.y;
        }

        public string getName()
        {
            return this.name;
        }

        public Container[] getContainersArray()
        {
            return this.containers;
        }

        public int getIndex()
        {
            return this.indexInSeaPortsArray;
        }

        public int getShore()
        {
            return this.shore;
        }
        //Setters
        public void setX(double x)
        {
            this.x = x;
        }

        public void setY(double y)
        {
            this.y = y;
        }

        public void setName(string name)
        {
            this.name = name;
        }

        public void setIndex(int index)
        {
            this.indexInSeaPortsArray = index;
        }

        public void setShore(int shoreIndex)
        {
            this.shore = shoreIndex;
        }
    }
}