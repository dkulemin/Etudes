using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Media;

namespace LifeStream
{
    //Класс живых клеток
    public class AliveCell
    {
        private const int RECT_SIZE = 20;

        public Rectangle rect;
        public int i;
        public int j;
        public int massI;
        public int massJ;
        public sbyte state;

        public AliveCell(int i, int j, int massI, int massJ, sbyte state)
        {
            this.i = i;
            this.j = j;
            this.massI = massI;
            this.massJ = massJ;
            this.state = state;

            //Тело живой клетки
            rect = new Rectangle();
            rect.Width = RECT_SIZE;
            rect.Height = RECT_SIZE;
            rect.Fill = new SolidColorBrush(Colors.LimeGreen);
        }
    }
}
