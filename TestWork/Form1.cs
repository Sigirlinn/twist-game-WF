using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace TestWork
{
    public partial class Form1 : Form
    {
        //Игровое поле
        Field F;
        //Строка результата
        string win;
        //флаг 
        bool victory = true;
        //размер поля
        int countHand = 4;
        //ширина ячейки 
        public int PixW;
        //высота ячейки
        public int PixH;
        //горизонтальная ручка
        Image horizontal = Properties.Resources.left;
        //вертикальная ручка
        Image vertical = Properties.Resources.top;
        //количество шагов которые сделал пользователь
        public int step;

        public Form1()
        {
            InitializeComponent();
        }

        //при загрузке формы запускаем игру
        private void Form1_Load(object sender, EventArgs e)
        {
            Init();
        }

        //запускает новую игру
        private void PlayButton_Click(object sender, EventArgs e)
        {
            Init();
        }

        private void Init()
        {
            //проверяем, не запущен ли таймер
            if (timer1.Enabled)
            {
                timer1.Stop();
            }
            //делаем инициализацию
            victory = false;
            win = ".. ^_^ ..";
            int N = countHand;
            PixW = pictureBox.Width / N;
            PixH = pictureBox.Height / N;
            //создаем объект поля
            F = new Field(N);
            //запускаем таймер, чтобы в нем можно было рисовать повороты ручек
            timer1.Start();
            //pictureBox.Invalidate();
            pictureStep.Invalidate();
            pictureWin.Invalidate();
            pictureCountHand.Invalidate();
        }

        //поворачиваем ручку на поле
        private void pictureBox_MouseClick(object sender, MouseEventArgs e)
        {
            //проверяем если объект существует, и мы только что не выиграли
            if (F != null && !victory)
            {
                //делаем поворот ручки, индекс считаем через округление координат
                bool rez = F.Rotation(e.Location.X / PixW, e.Location.Y / PixH);
                //увеличиваем ход
                step++;
                //перерисовываем счетчик и поле
                pictureBox.Invalidate();
                pictureStep.Invalidate();
                //если мы выиграли
                if (rez)
                {
                    //помечаем
                    win = "Победа!";
                    victory = true;
                }
                //если еще не выиграли
                else
                {
                    //тоже помечам
                    win = "Мимо!";
                }
                //потом рисуем эту информацию
                pictureWin.Invalidate();
            }
            
        }

        //если форма изменит размер, пересчитываем ячейки и делаем перерисовку
        private void Form1_Resize(object sender, EventArgs e)
        {
            if (F != null)
            {
                PixH = pictureBox.Height / F.count;
                PixW = pictureBox.Width / F.count;
                pictureBox.Invalidate();
            }
        }

        //вызываем перерисовку поля по таймеру
        private void timer1_Tick(object sender, EventArgs e)
        {
            pictureBox.Invalidate();
        }

        //рисуем поле
        private void pictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (F != null) DrawField(e);
        }

        //поле
        public void DrawField(PaintEventArgs e)
        {
            for (int i = 0; i < F.count; i++)
            {
                for (int j = 0; j < F.count; j++)
                {
                    //если ручку не трогали
                    if (F.state[i][j] == 0)
                    {
                        //в зависимости от положения рисуем соответствующую картинку
                        if (F.mas[i][j] == 0)
                        {
                            e.Graphics.DrawImage(horizontal, j * PixW, i * PixH + PixH / 4, PixW, PixH / 2);
                        }
                        else
                        {
                            e.Graphics.DrawImage(vertical, j * PixW + PixW / 4, i * PixH, PixW / 2, PixH);
                        }
                    }
                    //если ручку трогали
                    else
                    {
                        //сохраняем состояние Graphics
                        var save = e.Graphics.Save();
                        //смотрим в какую сторону надо поворачивать
                        if (F.state[i][j] > 0)
                        {
                            //сдвигаем Graphics на середину подкартинки 
                            e.Graphics.TranslateTransform(PixW * j + PixW / 2, PixH * i + PixH / 2);
                            //Поворачиваем его на угол, зависящий от состояния
                            e.Graphics.RotateTransform(9 * F.state[i][j]);
                            //рисуем на нем картинку
                            e.Graphics.DrawImage(horizontal, -PixW / 2, -PixH / 2 + PixH / 4, PixW, PixH / 2);
                            //возращаемся к изначальному состоянию
                            e.Graphics.Restore(save);
                            //изменяем состояние
                            F.state[i][j] -= 1f;
                        }
                        else
                        {
                            //аналогично, но поворот в другую сторону
                            e.Graphics.TranslateTransform(PixW * j + PixW / 2, PixH * i + PixH / 2);
                            e.Graphics.RotateTransform(-9 * F.state[i][j]);
                            e.Graphics.DrawImage(vertical, -PixW / 2 + PixW / 4, -PixH / 2, PixW / 2, PixH);
                            e.Graphics.Restore(save);
                            F.state[i][j] += 1f;
                        }
                    }
                }
            }
        }

        //рисуем количество ходов
        private void pictureStep_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(Color.Beige), 1, 1, 224, 58);
            e.Graphics.DrawRectangle(new Pen(Color.DarkGreen, 2), 1, 1, 224, 58);
            e.Graphics.DrawString("Количество ходов - " + step.ToString(),
                new Font("Segoe Print", 12),
                new SolidBrush(Color.DarkGreen),
                10,
                14);
        }

        //рисуем сообщение о выигрыше или промахе
        private void pictureWin_Paint(object sender, PaintEventArgs e)
        {
            SolidBrush sb;
            if (F != null)
            {
                if (step % 2 == 0)
                {
                    sb = new SolidBrush(Color.DarkKhaki);
                }
                else
                {
                    sb = new SolidBrush(Color.DarkGreen);
                }
                e.Graphics.RotateTransform(30);
                e.Graphics.DrawRectangle(new Pen(Color.DarkKhaki, 3), 20, -50, 120, 75);
                e.Graphics.FillRectangle(new SolidBrush(Color.Beige), 20, -50, 120, 75);
                e.Graphics.DrawString(win,
                    new Font("Segoe Print", 14),
                    sb,
                    45,
                    -25);
            }
        }

        //рисуем размер матрицы
        private void pictureCountHand_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(Color.Beige), 1, 1, 49, 39);
            e.Graphics.DrawRectangle(new Pen(Color.DarkGreen, 2), 1, 1, 49, 39);
            e.Graphics.DrawString(countHand.ToString(),
                new Font("Segoe Print", 12),
                new SolidBrush(Color.DarkGreen),
                10,
                5);
        }

        //увеличиваем размер матрицы (countHand)
        private void buttonUp_Click(object sender, EventArgs e)
        {
            if (countHand < 12)
            {
                countHand++;
                pictureCountHand.Invalidate();
            }
        }

        //уменьшаем размер матрицы (countHand)
        private void buttonDown_Click(object sender, EventArgs e)
        {
            if (countHand > 2)
            {
                countHand--;
                pictureCountHand.Invalidate();
            }
        }
    }
}
