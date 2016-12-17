using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

namespace SerialPortConnection
{
    public enum GPRS_Res
    {
        OK = 0,
        FALSE = 1
    };
    public class GPRS
    {
        public char[] RxBuf = new char[1000];
        public int RxLen = 0;
        public string[] ParseResult = new string[100];
        public int ParseResultNum = 0;
        public bool RxFinished = false;
        System.Timers.Timer t = new System.Timers.Timer(100);
        string CPASx = "";
        public SerialPort sp1 = new SerialPort();
        /* 对收到的命令进行解析 */
        public void Parse()
        {
            string RxString = new string(RxBuf);
            //string RxString = "\r\n+CPAS:5\r\n";
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
        private GPRS_Res SendCommand(string Command,int Index,string Match)
        {
            int timeout = 0, Retry = 0,NotMatch = 0;
            Restart:
            timeout = 0;
            sp1.WriteLine(Command);
            while (RxFinished == false)
            {
                Thread.Sleep(200);
                if (timeout++ > 5)
                {
                    if (Retry++ > 4) return GPRS_Res.FALSE;
                    goto Restart;
                }
            }
            RxFinished = false;
            /* 收到串口数据 */
            if (Index < ParseResultNum && ParseResult[Index] == Match) return GPRS_Res.OK;
            MessageBox.Show("发送命令："+ Command + "结果不对");
            if (++NotMatch < 2) goto Restart;
            return GPRS_Res.FALSE;
        }

        public GPRS_Res Init()
        {
            ATE0();
            ATI();
            AT_CCID();
            AT_CPAS();
            AT_CSQ();
            AT_CGREG_0();
            AT_CGREG_1();
            AT_MYNETACT();
            AT_PING();
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
            if (SendCommand("ATE0",0,"OK") == GPRS_Res.OK)
            {

            }
            return GPRS_Res.OK;
        }
        public GPRS_Res AT_CCID()
        {
            if (SendCommand("AT+CCID", 1, "OK") == GPRS_Res.OK)
            {

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
            if (SendCommand("AT$MYNETACT=0,1", 1, "OK") == GPRS_Res.OK)
            {

            }
            return GPRS_Res.OK;
        }
        public GPRS_Res AT_PING()
        {
            if (SendCommand("AT+PING=119.75.217.109", 0, "OK") == GPRS_Res.OK)
            {

            }
            return GPRS_Res.OK;
        }
    }
}
