using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;//подключение потоков
using System.Diagnostics;//это для того чтобы все потоки на одном ядре были

namespace Лаба_6__rez
{
  
    public partial class Form1 : Form
    {
        private Random rnd = new Random();
        private object locker = new object();
        int count = -1;//счетчик созданных потоков
        const int kolvo = 100;//максимальное количество потоков
        Thread[] potok = new Thread[kolvo];//массив на 100 потоков
        String[] prioritet = new String[5];//массив имен приоритетов
        SolidBrush[] kist = new SolidBrush[kolvo];//100 разных кистей для каждого потока
        int[] radius = new int[kolvo];//радиусы
        String[] names = new String[kolvo];//массив имен потоков для текстового поля
        Graphics[] g = new Graphics[kolvo];//массив для рисования
       // static Mutex mutexObj = new Mutex();//создаем новый мьютекс
        public Form1()
        {
            InitializeComponent();//Метод, который инициализирует все компоненты, расположенные на форме: поля, кнопки, меню, переключатели и т. п.
            for (int i = 0; i < kolvo; i++)
                g[i] = pictureBox1.CreateGraphics();//задаем поверхность для рисования
            prioritet[0] = "Lowest"; //имена для полей комбобоксов
            prioritet[1] = "BelowNormal";
            prioritet[2] = "Normal";
            prioritet[3] = "AboveNormal";
            prioritet[4] = "Highest";
            for(int i=0;i<5;i++)
            {
                comboBox1.Items.Add(prioritet[i]);
                comboBox2.Items.Add(prioritet[i]);
            }
            numericUpDown1.Value = (int)(10);
            for (int i = 0; i < kolvo; i++)
                radius[i] = new int();
            comboBox1.Text = Convert.ToString(Thread.CurrentThread.Priority);

        }
        public static void SetThreadAffinity(int mask)
        {
            int threadId = GetCurrentThreadId();//чтобы выполнялись на одном ядре
            var process = Process.GetCurrentProcess();
            process.Refresh();

            var currentThread = process.Threads
                .OfType<ProcessThread>()
                .Where(t => t.Id == threadId)
                .Single();
            currentThread.ProcessorAffinity = new IntPtr(mask);
        }
        [System.Runtime.InteropServices.DllImport("kernel32.dll")] static extern int GetCurrentThreadId();

        private void button2_Click(object sender, EventArgs e)
        {
            count++;
            Random col = new Random();
            kist[count] = new SolidBrush(Color.FromArgb(col.Next(0, 255), col.Next(0, 255), col.Next(0, 255)));//создаю кисть 
            names[count] = Convert.ToString("Potok " + (count));
            list.Items.Add(names[count]);//посылаю значение в листбокс
            radius[count] = Convert.ToInt32(20);//значение радиуса из поля
            
            potok[count] = new Thread(drow);//создание потока
            potok[count].Start();//запуск потока
            
        }
        private void drow()
        {
           
            int i = count;
            SetThreadAffinity(1);//чтобы было на одном ядре
           
            g[i] = pictureBox1.CreateGraphics();//чтобы рисовало только в области(picturebox1)
            int x, y;
            
            while(true)
            {
                    lock(locker)
                { 
                //mutexObj.WaitOne();//ожидает до тех пор, пока не будет получен мьютекс
                    x = rnd.Next(0, 470);
                    y = rnd.Next(0, 410);
                
                    g[i].FillEllipse(kist[i], x, y, radius[i], radius[i]);//интервал между потоками
                }
                //mutexObj.ReleaseMutex();//освобождаем мьютекс
            }
             
        }

        private void button3_Click(object sender, EventArgs e)
        {
            for (int i = 0; i <= count; i++)
                if (list.Text == names[i]) 
                {
                    list.SelectedItem = names[0];
                    list.Items.Remove(names[i]);
                    potok[i].Abort(); //остановка потока
                }
                   
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i <= count; i++)
                    if (list.Text == names[i])
                        potok[i].Suspend();//останавливаю поток
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                button1.BackColor = colorDialog1.Color;//задаю цвет кнопки с помошью которой открывается менюшка цветов
                for (int i = 0; i <= count; i++)
                    if (list.Text == names[i])
                    {
                        kist[i].Color = colorDialog1.Color;//меняю цвет кисти потока
                        potok[i].Resume();//продолжаю работу потока
                    }
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            for (int i = 0; i <= count; i++)
                if (list.Text == names[i])
                    radius[i] = Convert.ToInt32(numericUpDown1.Value);
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int prior = -1;
            if (Convert.ToString(list.SelectedItem) != "")
            {
                for (int i = 0; i <= count; i++)
                    if (list.Text == names[i])
                    {
                        prior = i;
                        button1.BackColor = kist[prior].Color;
                        numericUpDown1.Value = radius[prior];
                    }
                comboBox2.Text = Convert.ToString(potok[prior].Priority);//присваиваю полю комбобокс приоритет выбранного потока
            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int i = 0; i <= count; i++)
                if (list.Text == names[i])
                {
                    switch (comboBox2.Text)
                    {
                        case "Lowest": potok[i].Priority = ThreadPriority.Lowest; break;
                        case "BelowNormal": potok[i].Priority = ThreadPriority.BelowNormal; break;
                        case "Normal": potok[i].Priority = ThreadPriority.Normal; break;
                        case "AboveNormal": potok[i].Priority = ThreadPriority.AboveNormal; break;
                        case "Highest": potok[i].Priority = ThreadPriority.Highest; break;
                        default: ; break;
                    }
                }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            for (int i = 0; i <= count; i++)
                if (list.Text == names[i])
                    potok[i].Suspend();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            for (int i = 0; i <= count; i++)
                if (list.Text == names[i])
                    potok[i].Resume();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.Text)
            {
                case "Lowest": Thread.CurrentThread.Priority = ThreadPriority.Lowest; break;
                case "BelowNormal": Thread.CurrentThread.Priority = ThreadPriority.BelowNormal; break;
                case "Normal": Thread.CurrentThread.Priority = ThreadPriority.Normal; break;
                case "AboveNormal": Thread.CurrentThread.Priority = ThreadPriority.AboveNormal; break;
                case "Highest": Thread.CurrentThread.Priority = ThreadPriority.Highest; break;
                default: ; break;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            for (int i = 0; i <= count; i++)
                potok[i].Abort();
        }
        
    }
}
