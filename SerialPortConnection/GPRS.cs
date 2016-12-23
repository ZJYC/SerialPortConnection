using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
//using System.Random;

namespace SerialPortConnection
{
    /* 测试结果(序列化用于保存) */
    [Serializable]
    public struct TestResult_t
    {
        public DateTime TestBeginTime;
        public int TestDuring;
        public int TestCSQ;
        public int Result;
        public int PayloadLen;
        public float Match; 
    }
    public enum GPRS_Res
    {
        OK = 0,
        FALSE = 1
    };
    public class GPRS
    {
        /* 用以接收串口数据 */
        public char[] RxBuf = new char[1000];
        public int RxLen = 0;
        /* 用以解析串口命令 */
        public string[] ParseResult = new string[100];
        public int ParseResultNum = 0;
        /* 标明串口接收一帧完成 */
        public bool RxFinished = false;
        System.Timers.Timer t = new System.Timers.Timer(100);
        /* 串口初 */
        public SerialPort sp1 = new SerialPort();
        /* 测试相关 */
        public char[] TestPayload = new char[2000];
        public bool TransferMode = false;
        public int TransferLen = 0;
        public int TestCounter = 0;
        public TestResult_t []TestResult = new TestResult_t[10000];
        /* 随机数生成 */
        Random RD = new Random();
        /* 对收到的命令进行解析 */
        public void Parse()
        {
            string RxString = new string(RxBuf);
            //string RxString = "\r\n+CPAS:5\r\n";
            //string RxString = "+CSQ: 20, 99";
            /* 删除所有空格 */
            RxString = RxString.Replace(" ", "");
            RxString = RxString.Replace("\n\r\n", "\n");
            RxString = RxString.Replace("\r\n", "\n");
            /* 分解字符 */
            List<string> sArray = RxString.Split('\n').ToList();
            ParseResultNum = 0;
            for (int i = 0,j = 0; i < sArray.Count; i++)
            {
                if (sArray[i] == "") continue;
                if (sArray[i][0] != '\0')
                {
                    ParseResult[j++] = sArray[i];
                    ParseResultNum++;
                }
                else
                {
                    sArray.Clear();
                    RxFinished = true;
                    return;
                }
            }
            sArray.Clear();
            RxFinished = true;
        }

        /* 向GPRS模块发送指令 */
        /// <summary>
        /// 向串口发送指令并简单判断应答，结果保存在ParseResult一共调用者进一步分析
        /// </summary>
        /// <param name="Command"></param>发送的命令，自动加换行
        /// <param name="Index"></param>返回字串中带匹配项的索引
        /// <param name="Match"></param>匹配项
        /// <returns></returns>
        private GPRS_Res SendCommand(string Command,int Index,string Match)
        {
            int timeout = 0, Retry = 0,NotMatch = 0;
            /* 释放串口内存 */
            sp1.DiscardInBuffer();
            Restart:
            timeout = 0;
            sp1.WriteLine(Command);
            while (RxFinished == false)
            {
                Thread.Sleep(500);
                if (timeout++ > 8)
                {
                    if (Retry++ > 4) return GPRS_Res.FALSE;
                    goto Restart;
                }
            }
            RxFinished = false;
            /* 到此我们收到串口数据 */
            if (Index < ParseResultNum && ParseResult[Index] == Match) return GPRS_Res.OK;
            /* 自动尝试2次 */
            if (++NotMatch < 2) goto Restart;
            return GPRS_Res.FALSE;
        }
        /// <summary>
        /// GPRS初始化
        /// </summary>
        /// <returns></returns>
        public GPRS_Res Init()
        {
            ATE0();
            AT_CGREG_1();
            AT_MYNETACT();
            AT_NETCREATE();
            GPRS_TestOnce(0);
            
            //AT_CCID();
            //AT_CPAS();
            //AT_CSQ();
            //AT_CGREG_0();
            //AT_CGREG_1();
            //AT_MYNETACT();
            //AT_PING();
            return GPRS_Res.OK;
        }
        public GPRS_Res ATI()
        {
            if(SendCommand("ATI",3,"OK") == GPRS_Res.OK)
            {
                //if(ParseResult[])
            }
            return GPRS_Res.OK;
        }
        public GPRS_Res ATE0()
        {
            if (SendCommand("ATE0",1,"OK") == GPRS_Res.OK)
            {

            }
            return GPRS_Res.OK;
        }
        public GPRS_Res AT_CCID()
        {
            if (SendCommand("AT+CCID", 1, "OK") != GPRS_Res.OK)
            {
                MessageBox.Show("没有SIM卡？？");
            }
            return GPRS_Res.OK;
        }
        public GPRS_Res AT_CPAS()
        {
            if (SendCommand("AT+CPAS", 0, "+CPAS:0") == GPRS_Res.OK)
            {

            }
            return GPRS_Res.OK;
        }
        public GPRS_Res AT_CGREG_1()
        {
            if (SendCommand("AT+CGREG=1", 0, "OK") == GPRS_Res.OK)
            {

            }
            return GPRS_Res.OK;
        }
        public GPRS_Res AT_CGREG_0()
        {
            if (SendCommand("AT+CGREG=0", 0, "OK") == GPRS_Res.OK)
            {

            }
            return GPRS_Res.OK;
        }
        public GPRS_Res AT_CSQ()
        {
            if (SendCommand("AT+CSQ", 1, "OK") == GPRS_Res.OK)
            {

            }
            return GPRS_Res.OK;
        }
        public GPRS_Res AT_MYNETACT()
        {
            if (SendCommand("AT$MYNETACT=0,1",0, "OK") == GPRS_Res.OK)
            {

            }
            return GPRS_Res.OK;
        }
        public GPRS_Res AT_NETCREATE()
        {
            if (SendCommand("AT$MYNETCREATE=0,0,0,\"124.128.34.76\",6666", 0, "CONNECT") != GPRS_Res.OK)
            {
                MessageBox.Show("数据透传开启失败");
            }
            return GPRS_Res.OK;
        }
        /// <summary>
        /// 获取当前的信号量
        /// </summary>
        /// <returns></returns>返回信号量
        public int GetCSQ()
        {
            if(SendCommand("AT+CSQ", 1, "OK") == GPRS_Res.OK)
            {
                char[] ArrayCSQ = new char[2];
                ArrayCSQ[0] = ParseResult[0][5];
                ArrayCSQ[1] = ParseResult[0][6];
                //string ParseResult[0]
                string StrCSQ = new string(ArrayCSQ);
                int IntCSQ = Convert.ToInt16(StrCSQ);
                return IntCSQ;
            }
            return 0;
        }
        //AT$MYIPFILTER=0,1,"124.128.34.76","255.255.255.255"
        //AT$MYNETOPEN=0
        //AT$MYNETCREATE=0,0,0,"124.128.34.76",6666
        public GPRS_Res AT_PING()
        {
            if (SendCommand("AT+PING=119.75.217.109", 0, "OK") == GPRS_Res.OK)
            {
            }
            return GPRS_Res.OK;
        }
        /// <summary>
        /// 产生测试数据<随机长度并且随机数值>
        /// </summary>
        /// <returns></returns>测试数据长度
        public int GPRS_GeneratePayload()
        {
            //int Len = RD.Next(50,1460);
            int Len = 500;
            for(int i = 0;i < Len;i ++)
            {
                //TestPayload[i] = (char)RD.Next(0,255);
                /* 为了自测.... */
                TestPayload[i] = 'A';
            }
            TestPayload[0] = (char)(Len / 256);
            TestPayload[1] = (char)(Len % 256);
            return Len;
        }
        /// <summary>
        /// 测试一次
        /// </summary>
        /// <param name="Number"></param>当前测试编号
        /// <returns></returns>
        public GPRS_Res GPRS_TestOnce(int Number)
        {
            TransferMode = true;
            //TestResult[Number].TestCSQ = GetCSQ();
            TestResult[Number].TestBeginTime = DateTime.Now;
            TransferLen = TestResult[Number].PayloadLen = GPRS_GeneratePayload();
            sp1.DiscardInBuffer();
            sp1.Write(TestPayload, 0, TestResult[Number].PayloadLen);
            RxFinished = false;
            /* 等待返回 */
            int Retry = 10;
            while (--Retry != 0)
            {
                if (RxFinished != true)
                {
                    Thread.Sleep(1000);
                }
                else
                {
                    TransferMode = false;
                    return GPRS_Res.OK;
                }
            }
            TransferMode = false;
            return GPRS_Res.FALSE;
        }
    }
}
