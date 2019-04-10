using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LifeStream
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Размер ячейки пустыни
        private const int CELL_SIZE = 20;

        private bool clickFlag = true;

        //Создаем список живых клеток
        private static List<AliveCell> cellList = new List<AliveCell>();
        private static List<sbyte[,]> massList = new List<sbyte[,]>();
        //Матрица дублирующая поле, для удобства и вычислений
        private static sbyte[,] matrix;

        public MainWindow()
        {
            InitializeComponent();

            //Начальная область пустыни
            canvas1.Width = 620;
            canvas1.Height = 620;

            GridDraw(canvas1);

            //Иничиируем матрицу
            matrix = MatrixFirstState((int)canvas1.Width/20, (int)canvas1.Height/20);
        }

        //Создание живой клетки левой кнопкой мыши
        private void canvas1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (clickFlag)
            {
                Point position = e.GetPosition(canvas1);

                foreach (AliveCell c in cellList)
                {
                    //Клетка не создается, если место в пустыне уже занято
                    if (c.i == CoordinateResizing((int)position.X) && c.j == CoordinateResizing((int)position.Y))
                    {
                        return;
                    }
                }

                //Создание живой клетки с координатами на canvas и в матрице, а так же начальное состояние
                //0 - пустая клетка пустыни, а так же ячейка для отцентровки
                //1 - клетка зарождается
                //2 - клетка умирает
                //3 - клетка живет
                AliveCell cell = new AliveCell(CoordinateResizing((int)position.X), CoordinateResizing((int)position.Y), (int)position.X / CELL_SIZE, (int)position.Y / CELL_SIZE, 3);

                //добавление ячейки в список, матрицу и отрисовка на пустыне
                cellList.Add(cell);
                matrix[cell.massI, cell.massJ] = cell.state;
                AliveCellAdd(cell);
            }         
        }

        //Удаление живой клетки правой кнопкой мыши
        private void canvas1_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (clickFlag)
            {
                Point position = e.GetPosition(canvas1);

                List<AliveCell> tempList = new List<AliveCell>(cellList);

                foreach (AliveCell cell in tempList)
                {
                    if (cell.i == CoordinateResizing((int)position.X) && cell.j == CoordinateResizing((int)position.Y))
                    {
                        canvas1.Children.Remove(cell.rect);
                        cellList.Remove(cell);
                        break;
                    }
                }
            }          
        }

        //Отрисовка пустыни (добавление линий)
        private static void GridDraw(Canvas canvas1)
        {
            for (int i = 0; i <= canvas1.Width; i++)
            {
                if (i % CELL_SIZE == 0)
                {
                    Line razmetka = new Line();
                    razmetka.Stroke = Brushes.Orange;
                    razmetka.StrokeThickness = 1;

                    razmetka.X1 = i;
                    razmetka.X2 = i;
                    razmetka.Y1 = 0;
                    razmetka.Y2 = canvas1.Height;

                    canvas1.Children.Add(razmetka);
                }
            }
            for (int i = 0; i <= canvas1.Height; i++)
            {
                if (i % CELL_SIZE == 0)
                {
                    Line razmetka = new Line();
                    razmetka.Stroke = Brushes.Orange;
                    razmetka.StrokeThickness = 1;

                    razmetka.X1 = 0;
                    razmetka.X2 = canvas1.Width;
                    razmetka.Y1 = i;
                    razmetka.Y2 = i;

                    canvas1.Children.Add(razmetka);
                }
            }
        }

        //Пересоздание матрицы с нулями
        private static sbyte[,] MatrixFirstState(int n, int m)
        {
            matrix = new sbyte[n,m];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    matrix[i, j] = 0;
                }
            }

            return matrix;
        }

        //Кнопка отцентровки колонии
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Centralize();
            massList.Add(matrix);
            button1.IsEnabled = false; 
            button2.IsEnabled = true;
            button3.IsEnabled = true;
            button4.IsEnabled = true;
        }

        //Кнопка перехода поколений
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            //Отцентровка
            Centralize();

            //Поиск умирающих клеток (умирает если рядом меньше 2 соседей или больше 3)
            foreach (AliveCell cell in cellList)
            {
                if (NeighbourCount(cell.massI,cell.massJ) < 2 || NeighbourCount(cell.massI, cell.massJ) > 3)
                {
                    cell.state = 2;
                    matrix[cell.massI, cell.massJ] = cell.state;
                }           
            }

            //Временный (замороженный) список ячеек
            List<AliveCell> tempList = new List<AliveCell>(cellList);

            foreach (AliveCell cell in tempList)
            {
                for (int i = cell.massI - 1; i <= cell.massI + 1; i++)
                {
                    for (int j = cell.massJ - 1; j <= cell.massJ + 1; j++)
                    {
                        if (matrix[i, j] == 0)
                        {
                            //Принятие решения о зарождении клетки (рождается, если рядом ровно 3 соседа, включая умирающих)
                            CellBirth(i, j);
                        }
                    }
                }
            }

            tempList = new List<AliveCell>(cellList);

            //Умирание и взросление клеток
            foreach (AliveCell cell in tempList)
            {
                if (cell.state == 1)
                {
                    cell.state = 3;
                    matrix[cell.massI, cell.massJ] = cell.state;
                }

                if (cell.state == 2)
                {
                    matrix[cell.massI, cell.massJ] = 0;
                    canvas1.Children.Remove(cell.rect);
                    cellList.Remove(cell);
                }
            }

            //Отцентровка колонии
            Centralize();
            massList.Add(matrix);
        }

        //Подсчет соседних живых и умирающих клеток
        private int NeighbourCount(int massI, int massJ)
        {
            int count = 0;

            for (int i = massI-1; i <= massI+1; i++)
            {
                for (int j = massJ-1; j <= massJ+1; j++)
                {
                    if (i == massI && j == massJ)
                    {
                    }
                    else
                    {
                        if (matrix[i, j] == 2 || matrix[i, j] == 3)
                        {
                            count++;
                        }
                    }    
                }
            }

            return count;
        }

        //Зарождение живой клетки
        private void CellBirth(int massI, int massJ)
        {
            if (NeighbourCount(massI, massJ) == 3)
            {
                AliveCell cell = new AliveCell(massI*CELL_SIZE, massJ*CELL_SIZE, massI, massJ, 1);
                cellList.Add(cell);
                matrix[cell.massI, cell.massJ] = cell.state;
                AliveCellAdd(cell);
            }
        }

        //Метод отцентровки
        private void Centralize()
        {
            //Если нет ни одной клетки, то ничего не делаем
            if (cellList.Count == 0)
            {
                return;
            }

            int minX = cellList[0].i;
            int maxX = cellList[0].i;
            int minY = cellList[0].j;
            int maxY = cellList[0].j;

            //Ищем крайние клетки
            foreach (AliveCell cell in cellList)
            {
                if (cell.i < minX)
                {
                    minX = cell.i;
                }
                if (cell.i > maxX)
                {
                    maxX = cell.i;
                }
                if (cell.j < minY)
                {
                    minY = cell.j;
                }
                if (cell.j > maxY)
                {
                    maxY = cell.j;
                }
            }

            //Вычисляем расстояние между крайними клетками
            int distanceOX = maxX - minX;
            int distanceOY = maxY - minY;

            //Расширение canvas
            CanvasResizing(distanceOX, distanceOY);

            //Отрисовка пустыни
            GridDraw(canvas1);

            //Ищем центр пустыни
            int centerX = FindCenter((int)canvas1.Width);
            int centerY = FindCenter((int)canvas1.Height);

            //Ищем северо-западную клектку
            int topLeftCellnotX = centerX - distanceOX/2;
            int topLeftCellnotY = centerY - distanceOY/2;

            //Создание северо-западной живой клетки
            AliveCell topLeftCell = new AliveCell(CoordinateResizing(topLeftCellnotX), CoordinateResizing(topLeftCellnotY), 0, 0, 0);

            foreach (AliveCell cell in cellList)
            {
                //Переопределение координат живой клетки в пустыне относительно северо-западной живой клетки
                cell.i = CoordinateResizing(cell.i + (topLeftCell.i - minX));
                cell.j = CoordinateResizing(cell.j + (topLeftCell.j - minY));

                //matrix[cell.massI, cell.massJ] = 0;

                //Переопределение координат живой клетки в матрице
                cell.massI = cell.i/CELL_SIZE;
                cell.massJ = cell.j/CELL_SIZE;

                //matrix[cell.massI, cell.massJ] = cell.state;

                //Перемещение живой клетки
                AliveCellReplace(cell);
            }

            //Переопределение начального состояния матрицы
            matrix = MatrixFirstState((int)canvas1.Width / 20, (int)canvas1.Height / 20);

            //Живые клетки в матрице
            foreach (AliveCell cell in cellList)
            {
                matrix[cell.massI, cell.massJ] = cell.state;
            }
        }

        //Переопределение размера пустыни
        private void CanvasResizing(int OX, int OY)
        {
            //Есть ли необходимость в расширении пространства пустыни
            //Ширина и высота должны быть не меньше расстояния между крайними живыми клеткаи + 4 размера ячейки
            //Необходимо чтобы не допустить переполнения массива
            if (OX + 4*CELL_SIZE >= canvas1.Width)
            {
                canvas1.Width += 4*CELL_SIZE;
            }
            if (OY + 4*CELL_SIZE >= canvas1.Height)
            {
                canvas1.Height += 4*CELL_SIZE;
            }
        }

        //Отрисовка живой клетки
        private void AliveCellAdd(AliveCell cell)
        {
            canvas1.Children.Add(cell.rect);
            AliveCellReplace(cell);
        }
        //Перемещение тела клетки относительно координат
        private void AliveCellReplace(AliveCell cell)
        {
            try
            {
                Canvas.SetLeft(cell.rect, cell.i);
                Canvas.SetTop(cell.rect, cell.j);
            }
            catch (Exception)
            {
                MessageBox.Show("Фигура не добавлена");
                throw;
            }
        }

        //Координаты без остатка 
        private int CoordinateResizing(int coordinate)
        {
            return coordinate - coordinate % CELL_SIZE;
        }

        //Поиск центра пустыни
        private int FindCenter(int size)
        {
            if(size % (CELL_SIZE*2) == 0)
            {
                return size/2 - CELL_SIZE;
            }
            return size/2;
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            clickFlag = false;

            button2.IsEnabled = false;

            int width = massList[0].GetLength(0);
            int height = 0;

            int nullHeight = 0;

            foreach (sbyte[,] mass in massList)
            {
                if(mass.GetLength(0) > width)
                {
                    width = mass.GetLength(0);
                }

                height += mass.GetLength(1);
            }

            cellList.Clear();
            canvas1.Children.Clear();

            canvas1.Background = Brushes.White;
            canvas1.Width = width*CELL_SIZE;
            canvas1.Height = height*CELL_SIZE + (massList.Count-1)*4*CELL_SIZE;

            foreach (sbyte[,] mass in massList)
            {
                GenerationsOutputDrawing(mass, ref nullHeight);
            }
        }

        private void GenerationsOutputDrawing(sbyte[,] mass, ref int nullHeight)
        {
            Rectangle back = new Rectangle();
            back.Width = canvas1.Width;
            back.Height = mass.GetLength(1)*CELL_SIZE;
            back.Fill = new SolidColorBrush(Colors.Moccasin);

            canvas1.Children.Add(back);
            Canvas.SetLeft(back, 0);
            Canvas.SetTop(back, nullHeight);

            for (int i = 0; i < mass.GetLength(0); i++)
            {
                for (int j = 0; j < mass.GetLength(1); j++)
                {
                    if (mass[i, j] == 3)
                    {
                        Rectangle rect = new Rectangle();
                        rect.Width = CELL_SIZE;
                        rect.Height = CELL_SIZE;
                        rect.Fill = new SolidColorBrush(Colors.LimeGreen);

                        canvas1.Children.Add(rect);
                        Canvas.SetLeft(rect, i * CELL_SIZE + (canvas1.Width - mass.GetLength(0)*CELL_SIZE)/2);
                        Canvas.SetTop(rect, j * CELL_SIZE + nullHeight);
                    }
                }
            }

            for (int i = 0; i <= canvas1.Width; i++)
            {
                if (i % CELL_SIZE == 0)
                {
                    Line razmetka = new Line();
                    razmetka.Stroke = Brushes.Orange;
                    razmetka.StrokeThickness = 1;

                    razmetka.X1 = i;
                    razmetka.X2 = i;
                    razmetka.Y1 = nullHeight;
                    razmetka.Y2 = mass.GetLength(1)*CELL_SIZE + nullHeight;

                    canvas1.Children.Add(razmetka);
                }
            }
            for (int i = nullHeight; i <= mass.GetLength(1)*CELL_SIZE + nullHeight; i++)
            {
                if (i % CELL_SIZE == 0)
                {
                    Line razmetka = new Line();
                    razmetka.Stroke = Brushes.Orange;
                    razmetka.StrokeThickness = 1;

                    razmetka.X1 = 0;
                    razmetka.X2 = canvas1.Width;
                    razmetka.Y1 = i;
                    razmetka.Y2 = i;

                    canvas1.Children.Add(razmetka);
                }
            }

            nullHeight += mass.GetLength(1)*CELL_SIZE + 4*CELL_SIZE;
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            cellList.Clear();
            massList.Clear();
            canvas1.Children.Clear();

            //Начальная область пустыни
            canvas1.Width = 620;
            canvas1.Height = 620;
            canvas1.Background = Brushes.Moccasin;

            GridDraw(canvas1);

            //Иничиируем матрицу
            matrix = MatrixFirstState((int)canvas1.Width / 20, (int)canvas1.Height / 20);

            button1.IsEnabled = true;
            button2.IsEnabled = false;
            button3.IsEnabled = false;
            button4.IsEnabled = false;

            clickFlag = true;
        }
    }
}
