using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WordsGame
{
    public partial class Server : Form
    {
        public Server()
        {
            InitializeComponent();
        }
        string serverstatus;
        Int32 port = 13000;
        IPAddress localAddr = IPAddress.Parse("127.0.0.1");
        TcpListener server;
        NetworkStream stream;
        TcpClient client;
        HashSet<string> words = new HashSet<string>();
        bool sendstatus = false;
        bool startstatus = false;

        string winstatus;
        int time = 60;


        void streamread()
        {

            Byte[] bytes = new Byte[256];
            int i;
            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                // Translate data bytes to a ASCII string.
                if (Encoding.ASCII.GetString(bytes, 0, i).Contains("User Name:"))
                {

                    if (lblotheruser.InvokeRequired)
                    {
                        lblotheruser.Invoke(new MethodInvoker(delegate { lblotheruser.Text = System.Text.Encoding.ASCII.GetString(bytes, 0, i).Substring(10); ; }));
                    }
                    else
                    {
                        lblotheruser.Text = System.Text.Encoding.ASCII.GetString(bytes, 0, i).Substring(10); ;
                    }
                }
                else if (Encoding.ASCII.GetString(bytes, 0, i).Contains("Time"))
                {
                    time = 60;

                }
                else if (Encoding.ASCII.GetString(bytes, 0, i).Contains("You Win"))
                {
                    winstatus = "You Win";
                    MessageBox.Show("You Win");
                }
                else
                {
                    if (lbloword.InvokeRequired)
                    {
                        lbloword.Invoke(new MethodInvoker(delegate { lbloword.Text = System.Text.Encoding.ASCII.GetString(bytes, 0, i).ToUpper(); }));
                    }
                    else
                    {
                        lbloword.Text = System.Text.Encoding.ASCII.GetString(bytes, 0, i).ToUpper();
                    }

                    words.Add(lbloword.Text.ToLower());
                }
            }
        }
        void streamwrite(bool user, bool timestat)
        {
            try
            {
                if (user == false && timestat == false && winstatus == "You Lose")
                {
                    if (stream != null)
                    {
                        byte[] msg = System.Text.Encoding.ASCII.GetBytes("Win Status:" + "You Win");
                        stream.Write(msg, 0, msg.Length);
                        MessageBox.Show("You Lose" + "-> " + lblotheruser.Text + " Win");
                        winstatus = "";
                    }
                }

                if (timestat == true)
                {
                    if (stream != null)
                    {
                        if (time == 0)
                        {
                            byte[] msg = System.Text.Encoding.ASCII.GetBytes("Time:" + time.ToString());
                            stream.Write(msg, 0, msg.Length);
                            time = 60;
                            timer3.Stop();
                        }
                        else
                        {
                            byte[] msg = System.Text.Encoding.ASCII.GetBytes("Time:" + time.ToString());
                            stream.Write(msg, 0, msg.Length);
                            time--;
                        }
                    }
                }



                if (user == false && timestat==false)
                {
                    if (stream != null)
                    {
                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(textBox1.Text);
                        stream.Write(msg, 0, msg.Length);
                        if (lblyword.InvokeRequired)
                        {
                            lblyword.Invoke(new MethodInvoker(delegate { lblyword.Text = textBox1.Text.ToUpper(); }));
                        }
                        else
                        {
                            lblyword.Text = textBox1.Text.ToUpper();
                        }
                    }
                }

                else if (user == true)
                {
                    if (stream != null)
                    {
                        byte[] msg = System.Text.Encoding.ASCII.GetBytes("User Name:" + Properties.Settings.Default.UserName);
                        stream.Write(msg, 0, msg.Length);

                    }
                }
            }
            catch
            {

            }
        }
        void listen(TcpListener listener)
        {
            try
            {
                client = listener.AcceptTcpClient();
                serverstatus = "Connected!";
                stream = client.GetStream();

                while (true)
                {
                    streamread();
                }
            }
            catch (Exception e)
            {
                serverstatus = e.ToString();
            }
        }
        void listener()
        {
            server = new TcpListener(localAddr, port);
            server.Start();
            serverstatus = "Waiting for a connection... ";
            while (true)
            {
                listen(server);
            }


            // Buffer for reading data

        }
        private void Server_Load(object sender, EventArgs e)
        {
            startstatus = false;
            var taskListener = Task.Factory.StartNew(() =>
                               listener());
            toolStripTextBox1.Text = Properties.Settings.Default.UserName;
            lblname.Text = Properties.Settings.Default.UserName;
            timer2.Start();
            timer1.Start();
            timer4.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string txt = textBox1.Text.ToLower();
            if (textBox1.Text != "")
            {
                if (words.Contains(textBox1.Text))
                {
                    MessageBox.Show("Bu kelime daha önce kullanıldı.");
                }
                else
                {
                    if (lbloword.Text != "")
                    {

                        if (txt.StartsWith(lbloword.Text.Substring(lbloword.Text.Length - 2).ToLower()))
                        {
                            var taskListener = Task.Factory.StartNew(() =>
                                          streamwrite(false, false));
                            words.Add(txt);
                            label4.Text = lblotheruser.Text + "'s Turn";
                            time = 60;
                        }
                        else
                        {
                            MessageBox.Show("Kelime Son İki harf ile başlamıyor");
                        }
                    }
                    else
                    {
                        var taskListener = Task.Factory.StartNew(() =>
                                            streamwrite(false, false));
                        words.Add(txt);
                        label4.Text = lblotheruser.Text + "'s Turn";
                        time = 60;
                    }
                }
            }
            else
            {
                MessageBox.Show("Lütfen Bir Kelime Giriniz");
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (winstatus == "You Win") {
                textBox1.Enabled = true;
                button1.Enabled = true;
            }

            if (label4.Text == "Your Turn" && startstatus == true)
            {
                textBox1.Enabled = true;
                button1.Enabled = true;
                if (sendstatus == false && time == 0)
                {
                    winstatus = "You Lose";
                    textBox1.Enabled = false;
                    button1.Enabled = false;
                    button2.Enabled = true;
                    var taskListener = Task.Factory.StartNew(() =>
                                            streamwrite(false, false));
                    timer1.Stop();
                    
                }
            }
            else
            {
                textBox1.Enabled = false;
                button1.Enabled = false;
            }
        }



        private void Server_FormClosed(object sender, FormClosedEventArgs e)
        {

            server.Stop();
        }

        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {

        }

        private void toolStripTextBox1_TextChanged(object sender, EventArgs e)
        {

            lblname.Text = toolStripTextBox1.Text;
            Properties.Settings.Default.UserName = lblname.Text;
            Properties.Settings.Default.Save();
            var taskListener = Task.Factory.StartNew(() =>
                                           streamwrite(true, false));


        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (serverstatus == "Connected!")
            {
                var task = Task.Factory.StartNew(() =>
                                              streamwrite(true, false));
                timer2.Stop();
            }
        }

        private void Server_FormClosing(object sender, FormClosingEventArgs e)
        {

            byte[] msg = System.Text.Encoding.ASCII.GetBytes("Host terminated.");
            if (stream != null)
                stream.Write(msg, 0, msg.Length);
            server.Stop();
        }

        private void lbloword_TextChanged(object sender, EventArgs e)
        {
            label4.Text = "Your Turn";

            time = 60;
            timer3.Start();
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            var taskListener = Task.Factory.StartNew(() =>
                                        streamwrite(false, true));


        }

        private void timer4_Tick(object sender, EventArgs e)
        {
            lbltime.Text = time.ToString();
            lblserverstatus.Text = serverstatus;
            if (startstatus == true)
            {
                button1.Enabled = true;
                textBox1.Enabled = true;
            }
            else
            {
                button1.Enabled = false;
                textBox1.Enabled = false;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Start();
            startstatus = true;
            var taskListener = Task.Factory.StartNew(() =>
                                        streamwrite(false, false));
            timer3.Start();
            button2.Enabled = false;

        }


    }
}
