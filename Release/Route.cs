using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaPortsGenetic_WPF
{
    class Route : IComparable
    {
        private double _distance;
        private SeaPort _sp1, _sp2;

        public double Distance
        {
            get
            {
                return _distance;
            }
            set
            {
                _distance = value;
            }
        }

        public SeaPort SeaPort1
        {
            get
            {
                return _sp1;
            }
            set
            {
                _sp1 = value;
            }
        }

        public SeaPort SeaPort2
        {
            get
            {
                return _sp2;
            }
            set
            {
                _sp2 = value;
            }
        }

        public int CompareTo (object obj)
        {
            Route routeToCompare = obj as Route;
            if (routeToCompare != null)
            {
                if (routeToCompare.Distance >= this.Distance)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                throw new Exception("Невозможно сравнить!");
            }
        }
    }
}
