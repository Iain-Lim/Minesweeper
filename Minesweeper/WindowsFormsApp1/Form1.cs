using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

//Lim Song Hao, Iain
//ES22

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        //general diamensions
        int leftspace = 100, abovespace = 100;  //distance displaced from side
        int btnsize = 20;                       //size of button

        //top bar
        int top_bar_upspace = 40;       // displacement from the top
        Button play_btn = new Button(); //play button
        Timer timer1 = new Timer();     //timer
        Label Labeltime = new Label();  //label for time
        int time = 0;                   //timer time
        Label Labelflag = new Label();  //display number of mines left
        int minesleft=0;

        //Arrays for buttons
        int[,] btn_value = new int[200, 200];   //value of button
        PictureBox[,] pic = new PictureBox[200, 200];   //picture behind button

        //outside game variable (0=false, 1=true)
        int first_game = 1; //check if first game round of the program
        int game_start = 1; //check if player has open any box

        //in game variables
        int xbutton = 25, ybutton = 15;     //number of box
        int mines = 40;     //number of mines

        //RNG
        Random rand = new Random();  //random number generator

        //surrounding of box
        int[] s8x = { 1, 0, -1, 0, 1, -1, -1, 1 };     //surounding for x
        int[] s8y = { 0, 1, 0, -1, 1, -1, 1, -1 };     //surounding for y

        //state of box
        int[,] open = new int[200, 200];    //weather box is revealed (if value is 0: false, 1:true)
        int[,] flag = new int[200, 200];    //weather there is flag on box (if value is 0: noting on the box, 1: flag, 2: question mark)

        public Form1()
        {
            InitializeComponent();

            //top bar

            //number of mines left label
            Labelflag = new Label();
            Labelflag.SetBounds(50, top_bar_upspace, 120, 20);
            Labelflag.Font = new Font("", 10, FontStyle.Bold);
            Labelflag.BackColor = System.Drawing.Color.LightGray;
            Labelflag.ForeColor = System.Drawing.Color.Red;
            Controls.Add(Labelflag);
            Labelflag.Text = "Flags left: 0";

            //start button
            play_btn = new Button();
            play_btn.SetBounds(250, top_bar_upspace, 60, 30);
            Controls.Add(play_btn);
            play_btn.MouseUp += new MouseEventHandler(Play);
            play_btn.Text = "Start";

            //timer
            timer1 = new Timer();
            timer1.Interval = 1000;
            timer1.Tick += new EventHandler(Ticks);

            //label for time
            Labeltime = new Label();
            Labeltime.SetBounds(450, top_bar_upspace, 60, 30);
            Labeltime.Font = new Font("", 20, FontStyle.Bold);
            Labeltime.BackColor=System.Drawing.Color.LightGray;
            Labeltime.ForeColor = System.Drawing.Color.Red;
            Labeltime.Text = "000";
            Controls.Add(Labeltime);



            //end of public form
        }

        private void Play(object sender, MouseEventArgs e)
        {
            minesleft = mines;
            Labelflag.Text = "Flags left: " + Convert.ToString(minesleft);
            timer1.Stop();
            Labeltime.Text = "000";
            if(first_game==1)
            {
                //creating buttons
                create_buttons();
            }
            else if(game_start==1)
            {
                //resetting button value
                reseting_btn_value();
                for (int i = 1; i <= xbutton; i++)
                {
                    for (int j = 1; j <= ybutton; j++)
                    {
                        pic[i, j].Image = Properties.Resources.facingDown;
                        pic[i, j].MouseDown += new MouseEventHandler(FirstClick);
                        //pic[i, j].MouseDown -= new MouseEventHandler(OnClick);
                    }
                }

                if(first_game==0)
                {
                    for (int i = 1; i <= xbutton; i++)
                    {
                        for (int j = 1; j <= ybutton; j++)
                        {
                            pic[i, j].MouseDown -= new MouseEventHandler(OnClick);
                        }
                    }
                }

                game_start = 0;
            }


            first_game = 0;
        }

        private void Ticks(object sender, EventArgs e)
        {
            time++;
            if(time<100)
            {
                if(time<10)
                {
                    Labeltime.Text = "00" + Convert.ToString(time);
                }
                else
                {
                    Labeltime.Text = "0" + Convert.ToString(time);
                }
            }
            else
            {
                Labeltime.Text = Convert.ToString(time);
            }
        }

        private void FirstClick(object sender, MouseEventArgs e)
        {
            int x, y;
            Point coord;
            coord = ((PictureBox)sender).Location;
            x = (coord.X - leftspace) / btnsize;
            y = (coord.Y - abovespace) / btnsize;

            
            game_start = 1;
            

            if (e.Button == MouseButtons.Right)
            {   //right click
                flagging(x, y);
            }
            else if (flag[x, y] == 0)
            {   //left click

                for (int i = 1; i <= xbutton; i++)
                {
                    for (int j = 1; j <= ybutton; j++)
                    {
                        pic[i, j].MouseDown -= new MouseEventHandler(FirstClick);
                        pic[i, j].MouseDown += new MouseEventHandler(OnClick);
                    }
                }

                //mines setting
                setting_mines(x, y);

                //assigning number
                assigning_number();

                //starting timer
                timer1.Start();
                time = 1;
                Labeltime.Text = "001";

                //revealing tile opened
                show_num(x, y);
            }
        }



        private void OnClick(object sender, MouseEventArgs e)
        {
            int x, y;
            Point coord;
            coord = ((PictureBox)sender).Location;
            x = (coord.X - leftspace) / btnsize;
            y = (coord.Y - abovespace) / btnsize;

            if (e.Button == MouseButtons.Right)
            {   //right click
                flagging(x, y);
            }
            else if(flag[x, y] == 0)
            {   //left click

                show_num(x, y);
            }
            //btn[x, y].ForeColor = System.Drawing.Color.Red;
        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }



        //-------------------------------------------------------
        
        //functions

        //creating buttons
        void create_buttons()
        {
            for (int i = 1; i <= xbutton; i++)
            {
                for (int j = 1; j <= ybutton; j++)
                {
                    pic[i, j] = new PictureBox();
                    pic[i, j].SetBounds(i * btnsize + leftspace, j * btnsize + abovespace, btnsize, btnsize);
                    Controls.Add(pic[i, j]);
                    pic[i, j].Image = Properties.Resources.facingDown;
                    pic[i, j].MouseDown += new MouseEventHandler(FirstClick);
                    pic[i, j].SizeMode = PictureBoxSizeMode.StretchImage;
                }
            }
        }

        //setting number
        void assigning_number()
        {
            for (int x = 1; x <= xbutton; x++)
            {
                for (int y = 1; y <= ybutton; y++)
                {
                    if (btn_value[x, y] != -1)
                    {
                        int num = 0;

                        for (int i = 0; i < 8; i++)
                        {
                            if (btn_value[s8x[i] + x, s8y[i] + y] == -1)
                            {
                                num++;
                            }
                        }
                        btn_value[x, y] = num;

                        //btn[x, y].Text = Convert.ToString(btn_value[x, y]);
                    }
                }
            }
        }

        //resetting btn_value
        void reseting_btn_value()
        {
            for (int x = 1; x <= xbutton; x++)
            {
                for (int y = 1; y <= ybutton; y++)
                {
                    btn_value[x, y] = 0;
                    open[x, y] = 0;
                    flag[x, y] = 0;
                }
            }
        }

        //mines setting
        void setting_mines(int x, int y)
        {
            int temp_mines = mines;
            //ensure that no mines would be set on or around players first move by placing mines aound
            for (int i = 0; i < 8; i++)
            {
                btn_value[s8x[i] + x, s8y[i] + y] = -1;
            }
            btn_value[x, y] = -1;

            while (temp_mines > 0)
            {
                int xrand = rand.Next(1, xbutton + 1);
                int yrand = rand.Next(1, ybutton + 1);
                if (btn_value[xrand, yrand] != -1)
                {
                    btn_value[xrand, yrand] = -1;
                    //btn[xrand, yrand].Text = "-";   //prove if mine here
                    temp_mines--;
                }
            }

            // remove mines that were set earlier in this function
            for (int i = 0; i < 8; i++)
            {
                btn_value[s8x[i] + x, s8y[i] + y] = 0;
            }
            btn_value[x, y] = 0;
        }

//show number of the button clicked
        void show_num(int x, int y)
        {
            if (x >= 1 && x <= xbutton && y >= 1 && y <= ybutton)
            {
                if (open[x, y] == 0 && flag[x, y]==0)
                {
                    open[x, y] = 1;

                    switch (btn_value[x, y])
                    {
                        case 0:
                            pic[x, y].Image = Properties.Resources._0;
                            for (int i = 0; i < 8; i++)
                            {
                                if (btn_value[s8x[i] + x, s8y[i] + y] != -1)
                                {
                                    show_num(s8x[i] + x, s8y[i] + y);
                                }
                            }
                            break;

                        case 1:

                            pic[x, y].Image = Properties.Resources._1;
                            break;

                        case 2:
                            pic[x, y].Image = Properties.Resources._2;
                            break;

                        case 3:
                            pic[x, y].Image = Properties.Resources._3;
                            break;

                        case 4:
                            pic[x, y].Image = Properties.Resources._4;
                            break;

                        case 5:
                            pic[x, y].Image = Properties.Resources._5;
                            break;

                        case 6:
                            pic[x, y].Image = Properties.Resources._6;
                            break;

                        case 7:
                            pic[x, y].Image = Properties.Resources._7;
                            break;

                        case 8:
                            pic[x, y].Image = Properties.Resources._8;
                            break;

                        case -1:
                            game_over(x, y);
                            break;
                    }
                }
            }
            check_win();
        }

        void flagging(int x, int y)
        {
            if(open[x, y]==0)
            {
                switch(flag[x, y])
                {
                    case 0:
                        pic[x, y].Image = Properties.Resources.flagged;
                        flag[x, y] = 1;
                        minesleft--;
                        break;

                    case 1:
                        pic[x, y].Image = Properties.Resources._question;
                        flag[x, y] = 2;
                        break;

                    case 2:
                        pic[x, y].Image = Properties.Resources.facingDown;
                        flag[x, y] = 0; ;
                        minesleft++;
                        break;
                }
            }
            Labelflag.Text = "Flags left: " + Convert.ToString(minesleft);
        }

        //game over
        void game_over(int x, int y)
        {
            review_mines(x, y);
            timer1.Stop();
            for (int i = 1; i <= xbutton; i++)
            {
                for (int j = 1; j <= ybutton; j++)
                {
                    pic[i, j].MouseDown -= new MouseEventHandler(OnClick);
                }
            }
            switch (rand.Next(1,5))
            {
                case 1:
                    MessageBox.Show("you tried your best... ,\nBUT \nyour best is NOT ENOUGH!!! ", "YOU LOSE");
                    break;
                case 2:
                    MessageBox.Show("MINE YOUR STEP!!! \nYOU BLEW IT UP", "YOU LOSE");
                    break;
                case 3:
                    MessageBox.Show("I BELIVE I CAN FLY (using a bomb???)", "YOU LOSE");
                    break;
                case 4:
                    MessageBox.Show("Codes are full of bugs, \nBUT \nTHIS is not one of them.. ", "YOU LOSE");
                    break;
            }
        }
        //review mines
        void review_mines(int x, int y)
        {
            for (int i = 1; i <= xbutton; i++)
            {
                for (int j = 1; j <= ybutton; j++)
                {
                    if(btn_value[i, j]==-1 && flag[i, j]==0)
                    {
                        pic[i, j].Image = Properties.Resources.bomb;
                    }
                    else if (btn_value[i, j] != -1 && flag[i, j] == 1)
                    {
                        pic[i, j].Image = Properties.Resources.wrong_flag;
                        pic[i, j].BackColor = System.Drawing.Color.LightGray;
                    }
                }
            }
            pic[x, y].Image = Properties.Resources.open_bomb;
        }


        void check_win()
        {
            int current_num_open = 0;

            for (int i = 1; i <= xbutton; i++)
            {
                for (int j = 1; j <= ybutton; j++)
                {
                    if(open[i, j]==1 && btn_value[i, j]!=-1)
                    {
                        current_num_open++;
                    }
                }
            }

            if(current_num_open==xbutton*ybutton-mines)
            {
                game_win();
            }

        }

        void game_win()
        {
            timer1.Stop();
            for (int i = 1; i <= xbutton; i++)
            {
                for (int j = 1; j <= ybutton; j++)
                {
                    pic[i, j].MouseDown -= new MouseEventHandler(OnClick);
                }
            }

            MessageBox.Show("Your time is: " + Convert.ToString(time), "SUCCESS!!!");
        }
    }
}
