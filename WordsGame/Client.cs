using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WordsGame
{
    public partial class Client : Form
    {
        public Client()
        {
            InitializeComponent();
        }
        public string ipAddress;
        Int32 port = 13000;
        string veri;
        TcpClient tcpClient;
        NetworkStream netStream;
        HashSet<string> words = new HashSet<string>();
        string connectionstatus;
        int closetime;
        int time = 60;
        Boolean sendstatus;
        string winstatus;
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
                            sendstatus = true;

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
                        sendstatus = true;


                    }
                }
            }
            else
            {
                MessageBox.Show("Lütfen Bir Kelime Giriniz");
            }


        }
        void streamwrite(bool user, bool timestat)
        {
            try
            {
                if (user == false && timestat == true && winstatus == "You Lose")
                {


                    if (netStream.CanWrite)
                    {
                        Byte[] sendBytes = Encoding.ASCII.GetBytes("Win Status:" + "You Win");
                        netStream.Write(sendBytes, 0, sendBytes.Length);
                        MessageBox.Show("You Lose" + "-> " + lblotheruser.Text + " Win");
                        winstatus = "";
                    }
                    else
                    {
                        MessageBox.Show("You cannot write data to this stream.");
                        tcpClient.Close();
                        netStream.Close();
                        return;
                    }


                }

                if (user == false && timestat == false)
                {
                    if (netStream.CanWrite)
                    {
                        Byte[] sendBytes = Encoding.ASCII.GetBytes(textBox1.Text);
                        netStream.Write(sendBytes, 0, sendBytes.Length);
                        if (lblyword.InvokeRequired)
                        {
                            lblyword.Invoke(new MethodInvoker(delegate { lblyword.Text = textBox1.Text.ToUpper(); }));
                        }
                        else
                        {
                            lblyword.Text = textBox1.Text.ToUpper();
                        }
                    }
                    else
                    {
                        MessageBox.Show("You cannot write data to this stream.");
                        tcpClient.Close();
                        netStream.Close();
                        return;
                    }


                }

                else if (user == false && timestat == true)
                {
                    if (sendstatus == true)
                    {
                        if (netStream.CanWrite)
                        {
                            Byte[] sendBytes = Encoding.ASCII.GetBytes("Time:");
                            netStream.Write(sendBytes, 0, sendBytes.Length);

                        }
                        else
                        {
                            MessageBox.Show("You cannot write data to this stream.");
                            tcpClient.Close();
                            netStream.Close();
                            return;
                        }
                    }
                }

                else if (user == true)
                {
                    if (netStream != null)
                    {
                        if (netStream.CanWrite)
                        {
                            Byte[] sendBytes = Encoding.ASCII.GetBytes("User Name:" + lblname.Text);
                            netStream.Write(sendBytes, 0, sendBytes.Length);
                        }
                        else
                        {
                            MessageBox.Show("You cannot write data to this stream.");
                            tcpClient.Close();

                            // Closing the tcpClient instance does not close the network stream.
                            netStream.Close();
                            return;
                        }
                    }
                }

            }
            catch (Exception)
            {

            }
        }
        void streamread()
        {

            while (true)
            {
                if (netStream.CanRead)
                {
                    byte[] bytes = new byte[tcpClient.ReceiveBufferSize];
                    netStream.Read(bytes, 0, (int)tcpClient.ReceiveBufferSize);
                    veri = Encoding.ASCII.GetString(bytes);
                    if (veri.Contains("User Name:"))
                    {
                        if (lblotheruser.InvokeRequired)
                        {
                            lblotheruser.Invoke(new MethodInvoker(delegate { lblotheruser.Text = Encoding.ASCII.GetString(bytes).Substring(10); }));
                        }
                        else
                        {
                            lblotheruser.Text = Encoding.ASCII.GetString(bytes).Substring(10);

                        }
                        if (label4.InvokeRequired)
                        {
                            label4.Invoke(new MethodInvoker(delegate { label4.Text = lblotheruser.Text + "'s Turn"; }));
                        }
                        else
                        {
                            label4.Text = lblotheruser.Text + "'s Turn";

                        }

                    }
                    else if (veri.Contains("Host terminated."))
                    {
                        connectionstatus = "Host terminated.";
                    }
                    else if (veri.Contains("You Win"))
                    {
                        winstatus = "You Win";
                        MessageBox.Show("You Win");
                    }
                    else if (veri.Contains("Time:"))
                    {
                        try
                        {
                            time = Convert.ToInt32(veri.Substring(5));
                        }
                        catch (Exception e)
                        {

                        }
                    }
                    else
                    {
                        if (lbloword.InvokeRequired)
                        {
                            lbloword.Invoke(new MethodInvoker(delegate { lbloword.Text = Encoding.ASCII.GetString(bytes).ToUpper(); }));
                        }
                        else
                        {
                            lbloword.Text = Encoding.ASCII.GetString(bytes).ToUpper();
                        }
                        words.Add(lbloword.Text.ToLower());

                    }
                }


            }
        }
        void client()
        {
            try
            {
                tcpClient = new TcpClient("127.0.0.1", port);
                netStream = tcpClient.GetStream();
                if (netStream != null)
                    connectionstatus = "Connected!";
                while (true)
                {
                    streamread();
                }
            }
            catch (System.Net.Sockets.SocketException e)
            {
                connectionstatus = "Host didn't start";
            }
        }

        private void Client_Load(object sender, EventArgs e)
        {

            var taskListener = Task.Factory.StartNew(() =>
                               client());
            toolStripTextBox1.Text = Properties.Settings.Default.UserName;
            lblname.Text = Properties.Settings.Default.UserName;
            timer2.Start();
            timer1.Start();
            timer4.Start();
            timer5.Start();
        }


        private void toolStripTextBox1_TextChanged(object sender, EventArgs e)
        {

            lblname.Text = toolStripTextBox1.Text;
            Properties.Settings.Default.UserName = lblname.Text;
            Properties.Settings.Default.Save();
            var taskListener = Task.Factory.StartNew(() =>
                                           streamwrite(true, false));

        }

        private void timer1_Tick(object sender, EventArgs e)
        {



            if (label4.Text == "Your Turn" && sendstatus == false && time == 0)
            {
                winstatus = "You Lose";
                var taskListener = Task.Factory.StartNew(() =>
                                        streamwrite(false, true));
                timer1.Stop();
            }


        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (connectionstatus == "Connected!")
            {
                var task = Task.Factory.StartNew(() =>
                                              streamwrite(true, false));
                timer2.Stop();
            }
        }


        private void lbloword_TextChanged(object sender, EventArgs e)
        {
            label4.Text = "Your Turn";
            sendstatus = false;
        }
       
        private void timer4_Tick(object sender, EventArgs e)
        {
            lbltime.Text = time.ToString();
            lblserverstatus.Text = connectionstatus;
            if (label4.Text == "Your Turn")
            {
                textBox1.Enabled = true;
                button1.Enabled = true;
            }
            else
            {
                textBox1.Enabled = false;
                button1.Enabled = false;
            }
            
        }

        private void timer5_Tick(object sender, EventArgs e)
        {
            if (connectionstatus == "Host terminated." || connectionstatus == "Host didn't start")
            {
                timer4.Stop();
                lblserverstatus.Text = "Host Closed Client is Closing.-->" + closetime + "sn";
                closetime += 1;
                if (closetime == 5)
                {
                    lblserverstatus.Text = "Host Closed Client is Closing.-->" + closetime + "sn";
                    this.Close();
                }
            }
        }

     
    }
}
