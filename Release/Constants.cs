using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaPortsGenetic_WPF
{
    //Класс для описания констант модели//
    static class Constants
    {
        public static double vessel_velocity { get; set; } = 10; //скорость судна

        public static double percent_mutations { get; set; } = 10; //процент мутаций

        public static double productivityStandart = 30; //стандартная производительность причального перегружателя STS
        public static int vessel_load = 1500; //средняя загрузка судна контейнерами (в конт)
        public static double vessel_additionalTime = 4; //дов время обработки судна (швартовка / отшвартовка / доки и т.д.)
    }
}
