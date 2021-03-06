﻿using System;
using System.IO.Ports;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using INIFILE;
using System.Text.RegularExpressions;
using System.Threading;
using System.Collections;
using SerialPortConnection;

namespace SerialPortConnection
{
    public partial class Form1 : Form
    {
        GPRS GPRS = new GPRS();
        char[] TestPayloadCopy = new char[2000];
        int TestPayloadCopyIndex = 0;
        private List<byte> buffer = new List<byte>(4096);
        //SerialPort sp1 = new SerialPort();
        public Form1()
        {
            //t.Elapsed += new System.Timers.ElapsedEventHandler(theout);
            //t.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；
            //t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；
            InitializeComponent();
        }

        public void theout(object source, System.Timers.ElapsedEventArgs e)
        {

        }


        //加载
        private void Form1_Load(object sender, EventArgs e)
        {
            INIFILE.Profile.LoadProfile();//加载所有
            
           // 预置波特率
            switch (Profile.G_BAUDRATE)
            {
                case "300":
                    cbBaudRate.SelectedIndex = 0;
                    break;
                case "600":
                    cbBaudRate.SelectedIndex = 1;
                    break;
                case "1200":
                    cbBaudRate.SelectedIndex = 2;
                    break; 
                case "2400":
                    cbBaudRate.SelectedIndex = 3;
                    break;
                case "4800":
                    cbBaudRate.SelectedIndex = 4;
                    break;
                case "9600":
                    cbBaudRate.SelectedIndex = 5;
                    break;
                case "19200":
                    cbBaudRate.SelectedIndex = 6;
                    break; 
                case "38400":
                    cbBaudRate.SelectedIndex = 7;
                    break;
                case "115200":
                    cbBaudRate.SelectedIndex = 8;
                    break;
                default:
                    {
                        MessageBox.Show("波特率预置参数错误。");
                        return;
                    }                  
            }

            //预置波特率
            switch (Profile.G_DATABITS)
            {
                case "5":
                    cbDataBits.SelectedIndex = 0;
                    break;
                case "6":
                    cbDataBits.SelectedIndex = 1;
                    break; 
                case "7":
                    cbDataBits.SelectedIndex = 2;
                    break; 
                case  "8":
                    cbDataBits.SelectedIndex = 3;
                    break;
                default:
                    {
                        MessageBox.Show("数据位预置参数错误。");
                        return;
                    }

            }
            //预置停止位
            switch (Profile.G_STOP)
            {
                case "1":
                    cbStop.SelectedIndex = 0;
                        break;
                case "1.5":
                    cbStop.SelectedIndex = 1;
                    break;
                case "2":
                    cbStop.SelectedIndex = 2;
                    break;
                default:
                    {
                        MessageBox.Show("停止位预置参数错误。");
                        return;
                    }
            }

            //预置校验位
            switch(Profile.G_PARITY)
            {
                case "NONE":
                    cbParity.SelectedIndex = 0;
                    break;
                case "ODD":
                    cbParity.SelectedIndex = 1;
                    break;
                case "EVEN":
                    cbParity.SelectedIndex = 2;
                    break;
                default:
                    {
                        MessageBox.Show("校验位预置参数错误。");
                        return;
                    }
            }

            //检查是否含有串口
            string[] str = SerialPort.GetPortNames();
            if (str == null)
            {
                MessageBox.Show("本机没有串口！", "Error");
                return;
            }

            //添加串口项目
            foreach (string s in System.IO.Ports.SerialPort.GetPortNames())
            {//获取有多少个COM口
                //System.Diagnostics.Debug.WriteLine(s);
                cbSerial.Items.Add(s);
            }

            //串口设置默认选择项
            //cbSerial.SelectedIndex = 1;         //note：获得COM9口，但别忘修改
            cbBaudRate.SelectedIndex = 8;
            // cbDataBits.SelectedIndex = 3;
            // cbStop.SelectedIndex = 0;
            //  cbParity.SelectedIndex = 0;

            GPRS.sp1.BaudRate = 115200;

            Control.CheckForIllegalCrossThreadCalls = false;    //这个类中我们不检查跨线程的调用是否合法(因为.net 2.0以后加强了安全机制,，不允许在winform中直接跨线程访问控件的属性)
            GPRS.sp1.DataReceived += new SerialDataReceivedEventHandler(sp1_DataReceived);
            //sp1.ReceivedBytesThreshold = 1;

            //radio1.Checked = true;  //单选按钮默认是选中的
            rbRcvStr.Checked = true;

            //准备就绪
            GPRS.sp1.ReceivedBytesThreshold = 1;
            GPRS.sp1.ReadBufferSize = 20480;
            GPRS.sp1.WriteBufferSize = 20480;
            GPRS.sp1.DtrEnable = false;
            GPRS.sp1.RtsEnable = false;
            //设置数据读取超时为1秒
            GPRS.sp1.ReadTimeout = 2000;

            GPRS.sp1.Close();
        }

        void sp1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (GPRS.sp1.IsOpen)     //此处可能没有必要判断是否打开串口，但为了严谨性，我还是加上了
            {
                //输出当前时间
                DateTime dt = DateTime.Now;
                txtReceive.Text += "----------" + dt.ToString("mm:ss ffff") + "----------" + "\r\n";
                if (GPRS.TransferMode == false) Thread.Sleep(100);
                if (GPRS.TransferMode == true) Thread.Sleep(100);
                
                //char [] byteRead = new char[RecvLen];    //BytesToRead:sp1接收的字符个数
                if (GPRS.TransferMode == false)
                {
                    GPRS.RxLen = GPRS.sp1.BytesToRead;
                    if (rdSendStr.Checked)                          //'发送字符串'单选按钮
                    {
                        for (int i = 0; i < GPRS.RxLen; i++)
                        {
                            GPRS.RxBuf[i] = (char)GPRS.sp1.ReadChar();
                            if (GPRS.RxBuf[i] == '\r') continue;
                            txtReceive.Text += GPRS.RxBuf[i];
                        }
                        //txtReceive.Text += sp1.ReadLine() + "\r\n"; //注意：回车换行必须这样写，单独使用"\r"和"\n"都不会有效果
                        GPRS.sp1.DiscardInBuffer();                      //清空SerialPort控件的Buffer 
                        GPRS.Parse();
                    }
                }
                if (GPRS.TransferMode == true)
                {
                    int Len = GPRS.sp1.BytesToRead;
                    txtReceive.Text += Len.ToString() + "\r\n";
                    byte[] Buff = new byte[Len];
                    GPRS.sp1.Read(Buff, 0, Len);
                    //buffer.AddRange(Buff);
                    //GPRS.sp1.DiscardInBuffer();
                    if (buffer.Count == GPRS.TransferLen)
                    {
                        int MatchCounter = 0;
                        for (int i = 0; i < GPRS.TransferLen; i++)
                        {
                            if (GPRS.TestPayload[i] != buffer[i])
                            {
                                continue;
                            }
                            MatchCounter++;
                        }
                        /* 记录测试用时 */
                        GPRS.TestResult[GPRS.TestCounter].TestDuring = DateTime.Now.Millisecond - GPRS.TestResult[GPRS.TestCounter].TestBeginTime.Millisecond;
                        GPRS.TestResult[GPRS.TestCounter].Result = (MatchCounter == GPRS.TransferLen ? 'T' : 'F');
                        GPRS.TestResult[GPRS.TestCounter].Match = (float)(MatchCounter * 1.0 / GPRS.TransferLen);
                        GPRS.RxFinished = true;

                        GPRS.sp1.DiscardInBuffer();
                    }

                }
            }
        }
        //发送按钮
        private void btnSend_Click(object sender, EventArgs e)
        {
            if (cbTimeSend.Checked)
            {
                tmSend.Enabled = true;
            }
            else
            {
                tmSend.Enabled = false;
            }

            if (!GPRS.sp1.IsOpen) //如果没打开
            {
                MessageBox.Show("请先打开串口！", "Error");
                return;
            }

            String strSend = txtSend.Text;
            if (radio1.Checked == true)	//“HEX发送” 按钮 
            {
                //处理数字转换
                string sendBuf = strSend;
                string sendnoNull = sendBuf.Trim();
                string sendNOComma = sendnoNull.Replace(',', ' ');    //去掉英文逗号
                string sendNOComma1 = sendNOComma.Replace('，', ' '); //去掉中文逗号
                string strSendNoComma2 = sendNOComma1.Replace("0x", "");   //去掉0x
                strSendNoComma2.Replace("0X", "");   //去掉0X
                string[] strArray = strSendNoComma2.Split(' ');

                int byteBufferLength = strArray.Length;
                for (int i = 0; i < strArray.Length; i++ )
                {
                    if (strArray[i]=="")
                    {
                        byteBufferLength--;
                    }
                }               
               // int temp = 0;
                byte[] byteBuffer = new byte[byteBufferLength];
                int ii = 0;
                for (int i = 0; i < strArray.Length; i++)        //对获取的字符做相加运算
                {
                  
                    Byte[] bytesOfStr = Encoding.Default.GetBytes(strArray[i]);
                    
                    int decNum = 0;
                    if (strArray[i] == "")
                    {
                        //ii--;     //加上此句是错误的，下面的continue以延缓了一个ii，不与i同步
                        continue;
                    }
                    else
                    {
                         decNum = Convert.ToInt32(strArray[i], 16); //atrArray[i] == 12时，temp == 18 
                    }
                           
                   try    //防止输错，使其只能输入一个字节的字符
                   {
                       byteBuffer[ii] = Convert.ToByte(decNum);        
                   }
                   catch (System.Exception ex)
                   {
                       MessageBox.Show("字节越界，请逐个字节输入！", "Error");
                       tmSend.Enabled = false;
                       return;
                   }

                   ii++;    
                }
                GPRS.sp1.Write(byteBuffer, 0, byteBuffer.Length);
            }
            else		//以字符串形式发送时 
            {
                GPRS.sp1.WriteLine(txtSend.Text);    //写入数据
            }
        }

        //开关按钮
        private void btnSwitch_Click(object sender, EventArgs e)
        {
            //serialPort1.IsOpen
            if (!GPRS.sp1.IsOpen)
            {
                try
                {
                    //设置串口号
                    string serialName = cbSerial.SelectedItem.ToString();
                    GPRS.sp1.PortName = serialName;

                    //设置各“串口设置”
                    string strBaudRate = cbBaudRate.Text;
                    string strDateBits = cbDataBits.Text;
                    string strStopBits = cbStop.Text;
                    Int32 iBaudRate = Convert.ToInt32(strBaudRate);
                    Int32 iDateBits = Convert.ToInt32(strDateBits);

                    GPRS.sp1.BaudRate = iBaudRate;       //波特率
                    GPRS.sp1.DataBits = iDateBits;       //数据位
                    switch (cbStop.Text)            //停止位
                    {
                        case "1":
                            GPRS.sp1.StopBits = StopBits.One;
                            break;
                        case "1.5":
                            GPRS.sp1.StopBits = StopBits.OnePointFive;
                            break;
                        case "2":
                            GPRS.sp1.StopBits = StopBits.Two;
                            break;
                        default:
                            MessageBox.Show("Error：参数不正确!", "Error");
                            break;
                    }
                    switch (cbParity.Text)             //校验位
                    {
                        case "无":
                            GPRS.sp1.Parity = Parity.None;
                            break;
                        case "奇校验":
                            GPRS.sp1.Parity = Parity.Odd;
                            break;
                        case "偶校验":
                            GPRS.sp1.Parity = Parity.Even;
                            break;
                        default:
                            MessageBox.Show("Error：参数不正确!", "Error");
                            break;
                    }

                    if (GPRS.sp1.IsOpen == true)//如果打开状态，则先关闭一下
                    {
                        GPRS.sp1.Close();
                    }
                    //状态栏设置
                    tsSpNum.Text = "串口号：" + GPRS.sp1.PortName + "|";
                    tsBaudRate.Text = "波特率：" + GPRS.sp1.BaudRate + "|";
                    tsDataBits.Text = "数据位：" + GPRS.sp1.DataBits + "|";
                    tsStopBits.Text = "停止位：" + GPRS.sp1.StopBits + "|";
                    tsParity.Text = "校验位：" + GPRS.sp1.Parity + "|";

                    //设置必要控件不可用
                    cbSerial.Enabled = false;
                    cbBaudRate.Enabled = false;
                    cbDataBits.Enabled = false;
                    cbStop.Enabled = false;
                    cbParity.Enabled = false;

                    GPRS.sp1.Open();     //打开串口
                    btnSwitch.Text = "关闭串口";
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Error:" + ex.Message, "Error");
                    tmSend.Enabled = false;
                    return;
                }
            }
            else
            {
                //状态栏设置
                tsSpNum.Text = "串口号：未指定|";
                tsBaudRate.Text = "波特率：未指定|";
                tsDataBits.Text = "数据位：未指定|";
                tsStopBits.Text = "停止位：未指定|";
                tsParity.Text = "校验位：未指定|";
                //恢复控件功能
                //设置必要控件不可用
                cbSerial.Enabled = true;
                cbBaudRate.Enabled = true;
                cbDataBits.Enabled = true;
                cbStop.Enabled = true;
                cbParity.Enabled = true;

                GPRS.sp1.Close();                    //关闭串口
                btnSwitch.Text = "打开串口";
                tmSend.Enabled = false;         //关闭计时器
            }
        }

        //清空按钮
        private void btnClear_Click(object sender, EventArgs e)
        {
            txtReceive.Text = "";       //清空文本
        }

        //退出按钮
        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        //关闭时事件
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            INIFILE.Profile.SaveProfile();
            GPRS.sp1.Close();
        }

        private void txtSend_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (radio1.Checked== true)
            {
                //正则匹配
                string patten = "[0-9a-fA-F]|\b|0x|0X| "; //“\b”：退格键
                Regex r = new Regex(patten);
                Match m = r.Match(e.KeyChar.ToString());

                if (m.Success )//&&(txtSend.Text.LastIndexOf(" ") != txtSend.Text.Length-1))
                {
                    e.Handled = false;
                }
                else
                {
                    e.Handled = true;
                }
            }//end of radio1
            else
            {
                e.Handled = false;
            }
        }

        private void txtSend_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            
            //设置各“串口设置”
            string strBaudRate = cbBaudRate.Text;
            string strDateBits = cbDataBits.Text;
            string strStopBits = cbStop.Text;
            Int32 iBaudRate = Convert.ToInt32(strBaudRate);
            Int32 iDateBits = Convert.ToInt32(strDateBits);

            Profile.G_BAUDRATE = iBaudRate+"";       //波特率
            Profile.G_DATABITS = iDateBits+"";       //数据位
            switch (cbStop.Text)            //停止位
            {
                case "1":
                    Profile.G_STOP = "1";
                    break;
                case "1.5":
                    Profile.G_STOP = "1.5";
                    break;
                case "2":
                    Profile.G_STOP ="2";
                    break;
                default:
                    MessageBox.Show("Error：参数不正确!", "Error");
                    break;
            }
            switch (cbParity.Text)             //校验位
            {
                case "无":
                    Profile.G_PARITY = "NONE";
                    break;
                case "奇校验":
                    Profile.G_PARITY = "ODD";
                    break;
                case "偶校验":
                    Profile.G_PARITY = "EVEN";
                    break;
                default:
                    MessageBox.Show("Error：参数不正确!", "Error");
                    break;
            }

            //保存设置
            // public static string G_BAUDRATE = "1200";//给ini文件赋新值，并且影响界面下拉框的显示
            //public static string G_DATABITS = "8";
            //public static string G_STOP = "1";
            //public static string G_PARITY = "NONE";
            Profile.SaveProfile();
        }

        //定时器
        private void tmSend_Tick(object sender, EventArgs e)
        {
            //转换时间间隔
            string strSecond = txtSecond.Text;
            try
            {
                int isecond = int.Parse(strSecond) * 1000;//Interval以微秒为单位
                tmSend.Interval = isecond;
                if (tmSend.Enabled == true)
                {
                    btnSend.PerformClick();
                }
            }
            catch (System.Exception ex)
            {
                tmSend.Enabled = false;
                MessageBox.Show("错误的定时输入！", "Error");
            }
            
        }

        private void txtSecond_KeyPress(object sender, KeyPressEventArgs e)
        {
            string patten = "[0-9]|\b"; //“\b”：退格键
            Regex r = new Regex(patten);
            Match m = r.Match(e.KeyChar.ToString());

            if (m.Success)
            {
                e.Handled = false;   //没操作“过”，系统会处理事件    
            }
            else
            {
                e.Handled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread th = new Thread(new ThreadStart(ThreadMethod)); //也可简写为new Thread(ThreadMethod);                
            th.Start(); //启动线程 
        }

        public void ThreadMethod()
        {
            GPRS.Init();
            /*
             * AT$MYNETCON=0,"CFGP",1024
             * AT$MYNETACT=0,1
             * AT$MYNETACT=0,1
             * AT+PING=119.75.217.109
             */
        }
        

    }

}
