using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CourseDesign
{
    public partial class Form1 : Form
    {
        string memoryAddress = "";//内存地址（不包括寄存器地址）
        List<string> instructMemory = new List<string> { };//用于存放所有机器指令相当于指令存储器
        List<string> dataMemory = new List<string> { };//用于存放所有数据相当于数据存储器
        string machineCode = "";//机器指令二进制字符串
        string nextmachineCode = "";//下一指令二进制字符串
        int instructAddrStart = 0;//指令地址开始

        bool microInstructEnd = false;//微指令序列是否结束
        bool isPaused = false;//控制暂停

        int memoryNumber = 0000;
        string instructNumber = "1000";//指令序号
        int microinstructNumber = 0;//微指令序号

        int currentTimeth = 1;//当前正在执行的机器周期
        int nextTimeth = 0;//下一个执行的机器周期
        int enter = 0;//判断是否重新检测周期，初次进入

        string[] substring = new string[] { };//定义用于转化分割字符串
        string[] instructTemp = new string[] { };//临时存放所有机器指令
        string[] dataTemp = new string[] { };//临时存放所有数据
        string exeinstructTemp = "";//执行时存放指令中操作数部分

        public Form1()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            for (int m = 0; m < 4144; m++)
            {
                dataMemory.Add("0000000000000000");
            }
            dataTemp = dataMemory.ToArray();//拥有32位内存单元
        }

        void mateAddressingMode(string x)//匹配寻址方式
        {

            switch (x[0])//译码取值方式
            {
                case 'X':
                    //X不需要动机器码
                    break;
                case 'R':
                    getRegister(x);
                    break;
                default://只有立即数或相对地址，不需要寻址
                    //x = Convert.ToString(x[0]) + Convert.ToString(x[1]);
                    nextmachineCode = Convert.ToString(Convert.ToInt32(x,16),2).PadLeft(8, '0');//x转2进制，再凑8位，左边补0
                    machineCode = machineCode + nextmachineCode;
                    break;
            }
        }

        void getRegister(string y)//选择通用寄存器
        {
            for (int j = 0; j < y.Length - 1; j++)
            {
                if (y[j] == 'R')
                {
                    switch (y[j + 1])
                    {
                        case '0':
                            machineCode = machineCode + "0000";//R0
                            break;
                        case '1':
                            if(y=="R1")
                            {
                                machineCode = machineCode + "0001";//R1
                            }
                            else
                            {
                                switch (y[j + 2])
                                {
                                    case '0':
                                        machineCode = machineCode + "1010";//R10
                                        break;
                                    case '1':
                                        machineCode = machineCode + "1011";//R11
                                        break;
                                    case '2':
                                        machineCode = machineCode + "1100";//R12
                                        break;
                                    case '3':
                                        machineCode = machineCode + "1101";//R13
                                        break;
                                    case '4':
                                        machineCode = machineCode + "1110";//R14
                                        break;
                                    case '5':
                                        machineCode = machineCode + "1111";//R15
                                        break;
                                }
                            }
                            break;
                        case '2':
                            machineCode = machineCode + "0010";//R2
                            break;
                        case '3':
                            machineCode = machineCode + "0011";//R3
                            break;
                        case '4':
                            machineCode = machineCode + "0100";//R4
                            break;
                        case '5':
                            machineCode = machineCode + "0101";//R5
                            break;
                        case '6':
                            machineCode = machineCode + "0110";//R6
                            break;
                        case '7':
                            machineCode = machineCode + "0111";//R7
                            break;
                        case '8':
                            machineCode = machineCode + "1000";//R8
                            break;
                        case '9':
                            machineCode = machineCode + "1001";//R9
                            break;
                    }
                }
            }
        }

        void ExeAddressingmode(string registerNumber)//执行选择寻址方式，传入寄存器号
        {
            switch (nextTimeth)
            {
                case 13:
                    listBoxMicroinstruct.Items.Add(++microinstructNumber + ":R" + Convert.ToInt32(registerNumber, 2) + "->BUS");//算通用寄存器号
                    memoryAddress = registerNumber.PadLeft(16, '0');//取寄存器地址，补齐16位，送内存地址
                    tbBUS.Text = dataTemp[Convert.ToInt32(registerNumber, 2)].PadLeft(16, '0');//取指令给BUS
                    laBUS.Text = tbBUS.Text;
                    tbBUS.BackColor = Color.LightBlue;
                    nextTimeth = 14;
                    break;
                case 14:
                    switch (currentTimeth)
                    {
                        case 2://如果是取源操作数周期
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":BUS->SR");
                            tbSR.Text = tbBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                            laSR.Text = tbSR.Text;
                            tbSR.BackColor = Color.LightBlue;
                            break;
                        case 3://如果时取目的操作数周期
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":BUS->LA");
                            tbLA.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                            tbLA.BackColor = Color.LightBlue;
                            break;
                    }
                    nextTimeth = 15;
                    break;
                case 15:
                    if (currentTimeth == 2)//如果是取源操作数周期
                    {
                        listBoxMicroinstruct.Items.Add(++microinstructNumber + ":1->DT");
                    }
                    else if(currentTimeth == 3)//如果是取源操作数周期
                    {
                        listBoxMicroinstruct.Items.Add(++microinstructNumber + ":1->ET");
                    }
                    tbBUS.BackColor = Color.White;
                    tbSR.BackColor = Color.White;
                    tbDR.BackColor = Color.White;
                    tbLA.BackColor = Color.White;
                    nextTimeth = 16;
                    enter = 0;
                    currentTimeth++;
                    break;
            }
        }

        void Exeimmediate(string imm, bool immok, string aimR)//执行选择立即数，（立即数，是否已经取完立即数，目的寄存器号）
        {
            switch (immok)
            {
                case true://如果已经取完立即数
                    memoryAddress = aimR.PadLeft(16, '0');//将目的寄存器号给主存地址，以便之后找
                    nextTimeth = 16;//下址字段设为16
                    enter = 0;
                    currentTimeth++;
                    break;
                case false://如果没有取完立即数
                    switch (nextTimeth)
                    {
                        case 13:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber+":"+ Convert.ToInt32(imm, 2).ToString("X4") + "->BUS");
                            tbBUS.Text = imm.PadLeft(16, '0');
                            laBUS.Text = tbBUS.Text;
                            tbBUS.BackColor = Color.LightBlue;
                            nextTimeth = 14;
                            break;
                        case 14:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":BUS->MAR");
                            memoryAddress = tbBUS.Text;
                            tbMAR.BackColor = Color.LightBlue;
                            tbMAR.Text = tbBUS.Text;
                            laMAR.Text = tbMAR.Text;
                            nextTimeth = 15;
                            break;
                        case 15:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":READ");
                            nextTimeth = 16;
                            break;
                        case 16:
                            tbMAR.BackColor = Color.White;
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":WAIT");
                            nextTimeth = 17;
                            break;
                        case 17:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":MDR->BUS");
                            tbMDR.Text = dataTemp[Convert.ToInt32(memoryAddress, 2)].PadLeft(16, '0');
                            laMDR.Text = tbMDR.Text;
                            tbBUS.Text = tbMDR.Text;
                            laBUS.Text = tbBUS.Text;
                            tbMDR.BackColor = Color.LightBlue;
                            nextTimeth = 18;
                            break;
                        case 18:
                            tbMDR.BackColor = Color.White;
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":BUS->DR");
                            tbDR.Text = tbBUS.Text.Substring(tbBUS.Text.Length - 8, 8);//后低8位
                            laDR.Text = tbDR.Text;
                            tbDR.BackColor = Color.LightBlue;
                            nextTimeth = 19;
                            break;
                        case 19:
                            tbDR.BackColor = Color.White;
                            tbBUS.BackColor = Color.White;
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":1->DT");
                            nextTimeth = 20;
                            enter = 0;
                            currentTimeth++;
                            break;
                    }
                    break;
            }
        }

        void exeOperation()
        {
            switch (tbIR.Text.Substring(0, 4))//解析IR 0~4位，查看指令前几位，判断执行什么操作
            {
                case "0000":
                    if(Convert.ToString(tbIR.Text).Substring(4, 4)=="1100")//双操作数加
                    {
                        switch (nextTimeth)
                        {
                            case 40:
                                listBoxMicroinstruct.Items.Add(++microinstructNumber + ":SR->BUS");
                                tbSR.BackColor = Color.LightBlue;
                                tbBUS.BackColor = Color.LightBlue;
                                tbBUS.Text = tbSR.Text.PadLeft(16, '0');
                                laBUS.Text = tbBUS.Text;
                                nextTimeth = 41;
                                break;
                            case 41:
                                listBoxMicroinstruct.Items.Add(++microinstructNumber + ":ADD");
                                tbBUS.BackColor = Color.White;
                                tbSR.BackColor = Color.White;
                                nextTimeth = 42;
                                break;
                            case 42:
                                listBoxMicroinstruct.Items.Add(++microinstructNumber + ":ALU->LT");
                                tbLT.BackColor = Color.LightBlue;
                                tbLT.Text = ADD(tbSR.Text.PadLeft(16, '0'), tbLA.Text.PadLeft(16, '0'));
                                nextTimeth = 43;
                                break;
                            case 43:
                                listBoxMicroinstruct.Items.Add(++microinstructNumber + ":LT->BUS");
                                tbBUS.BackColor = Color.LightBlue;
                                tbBUS.Text = tbLT.Text;
                                laBUS.Text = tbBUS.Text;
                                nextTimeth = 44;
                                break;
                            case 44:
                                tbLT.BackColor = Color.White;
                                listBoxMicroinstruct.Items.Add(++microinstructNumber + ":BUS->MDR");
                                tbMDR.BackColor = Color.LightBlue;
                                tbMDR.Text = tbBUS.Text;
                                laMDR.Text = tbMDR.Text;
                                nextTimeth = 45;
                                break;
                            case 45:
                                listBoxMicroinstruct.Items.Add(++microinstructNumber + ":WRITE");
                                nextTimeth = 46;
                                break;
                            case 46:
                                listBoxMicroinstruct.Items.Add(++microinstructNumber + ":WAIT");
                                dataTemp[Convert.ToInt32(memoryAddress, 2)] = tbBUS.Text;
                                switch (Convert.ToInt32(memoryAddress, 2))//解析内存地址，去找相对应的R通用寄存器
                                {
                                    case 0:
                                        labelR0.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);//将BUS内容最后8个字符截出来
                                        break;
                                    case 1:
                                        labelR1.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 2:
                                        labelR2.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 3:
                                        labelR3.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 4:
                                        labelR4.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 5:
                                        labelR5.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 6:
                                        labelR6.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 7:
                                        labelR7.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 8:
                                        labelR8.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 9:
                                        labelR9.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 10:
                                        labelR10.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 11:
                                        labelR11.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 12:
                                        labelR12.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 13:
                                        labelR13.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 14:
                                        labelR14.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 15:
                                        labelR15.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                }
                                nextTimeth = 47;
                                break;
                            case 47:
                                listBoxMicroinstruct.Items.Add(++microinstructNumber + ":END");
                                enter = 0;
                                currentTimeth = 1;
                                nextTimeth = 0;
                                break;
                        }
                    }
                    else if (Convert.ToString(tbIR.Text).Substring(4, 4) == "1000")//双操作数减
                    {
                        switch (nextTimeth)
                        {
                            case 40:
                                listBoxMicroinstruct.Items.Add(++microinstructNumber + ":SR->BUS");
                                tbBUS.BackColor = Color.LightBlue;
                                tbSR.BackColor = Color.LightBlue;
                                tbBUS.Text = tbSR.Text.PadLeft(16, '0');
                                laBUS.Text = tbBUS.Text;
                                nextTimeth = 41;
                                break;
                            case 41:
                                listBoxMicroinstruct.Items.Add(++microinstructNumber + ":SUB");
                                tbBUS.BackColor = Color.White;
                                tbSR.BackColor = Color.White;
                                nextTimeth = 42;
                                break;
                            case 42:
                                listBoxMicroinstruct.Items.Add(++microinstructNumber + ":ALU->LT");
                                tbLT.Text= SUB(tbLA.Text.PadLeft(16, '0'), laBUS.Text);
                                tbLT.BackColor = Color.LightBlue;
                                nextTimeth = 43;
                                break;
                            case 43:
                                listBoxMicroinstruct.Items.Add(++microinstructNumber + ":LT->BUS");
                                tbBUS.BackColor = Color.LightBlue;
                                tbBUS.Text = tbLT.Text;
                                laBUS.Text = tbBUS.Text;
                                nextTimeth = 44;
                                break;
                            case 44:
                                tbLT.BackColor = Color.White;
                                listBoxMicroinstruct.Items.Add(++microinstructNumber + ":BUS->MDR");
                                tbMDR.BackColor = Color.LightBlue;
                                tbMDR.Text = tbBUS.Text;
                                laMDR.Text = tbMDR.Text;
                                nextTimeth = 45;
                                break;
                            case 45:
                                listBoxMicroinstruct.Items.Add(++microinstructNumber + ":WRITE");
                                nextTimeth = 46;
                                break;
                            case 46:
                                listBoxMicroinstruct.Items.Add(++microinstructNumber + ":WAIT");
                                dataTemp[Convert.ToInt32(memoryAddress, 2)] = tbBUS.Text;
                                switch (Convert.ToInt32(memoryAddress, 2))
                                {
                                    case 0:
                                        labelR0.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 1:
                                        labelR1.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 2:
                                        labelR2.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 3:
                                        labelR3.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 4:
                                        labelR4.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 5:
                                        labelR5.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 6:
                                        labelR6.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 7:
                                        labelR7.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 8:
                                        labelR8.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 9:
                                        labelR9.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 10:
                                        labelR10.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 11:
                                        labelR11.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 12:
                                        labelR12.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 13:
                                        labelR13.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 14:
                                        labelR14.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 15:
                                        labelR15.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                }
                                nextTimeth = 47;
                                break;
                            case 47:
                                listBoxMicroinstruct.Items.Add(++microinstructNumber + ":END");
                                enter = 0;
                                currentTimeth = 1;
                                nextTimeth = 0;
                                
                                break;
                        }

                    }
                    else if (Convert.ToString(tbIR.Text).Substring(4, 4) == "0000")//空指令
                    {
                        listBoxMicroinstruct.Items.Add(++microinstructNumber + ":END");
                        enter = 0;
                        currentTimeth = 1;
                        nextTimeth = 0;
                    }

                    break;
                case "0010"://双操作数传送
                    switch (nextTimeth)
                    {
                        case 40:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":SR->BUS");
                            tbBUS.BackColor = Color.LightBlue;
                            tbSR.BackColor = Color.LightBlue;
                            tbBUS.Text = tbSR.Text.PadLeft(16, '0');
                            laBUS.Text = tbBUS.Text;
                            nextTimeth = 44;
                            break;
                        case 44:
                            tbSR.BackColor = Color.White;
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":BUS->MDR");
                            tbMDR.BackColor = Color.LightBlue;
                            tbMDR.Text = tbBUS.Text;
                            laMDR.Text = tbMDR.Text;
                            nextTimeth = 45;
                            break;
                        case 45:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":WRITE");
                            nextTimeth = 46;
                            break;
                        case 46:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":WAIT");
                            dataTemp[Convert.ToInt32(memoryAddress, 2)] = laBUS.Text;
                            switch (Convert.ToInt32(memoryAddress, 2))
                            {
                                case 0:
                                    labelR0.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                                case 1:
                                    labelR1.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                                case 2:
                                    labelR2.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                                case 3:
                                    labelR3.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                                case 4:
                                    labelR4.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                                case 5:
                                    labelR5.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                                case 6:
                                    labelR6.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                                case 7:
                                    labelR7.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                                case 8:
                                    labelR8.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                                case 9:
                                    labelR9.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                                case 10:
                                    labelR10.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                                case 11:
                                    labelR11.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                                case 12:
                                    labelR12.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                                case 13:
                                    labelR13.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                                case 14:
                                    labelR14.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                                case 15:
                                    labelR15.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                            }
                            nextTimeth = 47;
                            break;
                        case 47:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":END");
                            enter = 0;
                            currentTimeth = 1;
                            nextTimeth = 0;
                            break;
                    }
                    break;
                case "1100"://无条件相对跳转
                    switch (nextTimeth)
                    {
                        case 40:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":DR->BUS");
                            tbDR.BackColor = Color.LightBlue;
                            tbBUS.BackColor = Color.LightBlue;
                            tbBUS.Text = tbDR.Text.PadLeft(16, '0');
                            nextTimeth = 41;
                            break;
                        case 41:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":CLEAR LA");
                            tbBUS.BackColor = Color.White;
                            tbDR.BackColor = Color.White;
                            tbLA.Text = "00000000";
                            nextTimeth = 42;
                            break;
                        case 42:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":1->C0");
                            nextTimeth = 43;
                            break;
                        case 43:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":ADD");
                            nextTimeth = 44;
                            break;
                        case 44:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":ALU->LT");
                            tbLT.BackColor = Color.LightBlue;
                            tbLT.Text= ADD(tbBUS.Text, "0000000000000001");
                            nextTimeth = 45;
                            break;
                        case 45:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":LT->BUS");
                            tbBUS.BackColor = Color.LightBlue;
                            tbBUS.Text = tbLT.Text;
                            tbBUS.Text = ADD(tbBUS.Text, tbDR.Text);
                            laBUS.Text = tbBUS.Text;
                            nextTimeth = 46;
                            break;
                        case 46:
                            tbLT.BackColor = Color.White;
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":BUS->MDR");
                            tbMDR.BackColor = Color.LightBlue;
                            tbMDR.Text = tbBUS.Text;
                            laMDR.Text = tbMDR.Text;
                            nextTimeth = 47;
                            break;
                        case 47:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":WRITE");
                            dataTemp[Convert.ToInt32(memoryAddress, 2)] = tbBUS.Text;
                            nextTimeth = 48;
                            break;
                        case 48:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":WAIT");
                            dataTemp[Convert.ToInt32(memoryAddress, 2)] = tbBUS.Text;
                            nextTimeth = 49;
                            break;
                        case 49:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":END");
                            enter = 0;
                            currentTimeth = 1;
                            nextTimeth = 0;
                            if (microinstructNumber == 355)
                            {
                                microInstructEnd = true;
                            }
                            break;
                    }
                    break;
                case "1111"://有条件相对跳转
                    switch (nextTimeth)
                    {
                        case 40:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":DR->BUS");
                            tbDR.BackColor = Color.LightBlue;
                            tbBUS.BackColor = Color.LightBlue;
                            tbBUS.Text = tbDR.Text.PadLeft(16, '0');
                            nextTimeth = 41;
                            break;
                        case 41:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":CLEAR LA");
                            tbBUS.BackColor = Color.White;
                            tbDR.BackColor = Color.White;
                            tbLA.Text = "00000000";
                            nextTimeth = 42;
                            break;
                        case 42:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":1->C0");
                            nextTimeth = 43;
                            break;
                        case 43:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":ADD");
                            nextTimeth = 44;
                            break;
                        case 44:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":ALU->LT");
                            tbLT.BackColor = Color.LightBlue;
                            tbLT.Text = ADD(tbBUS.Text, "0000000000000001");
                            nextTimeth = 45;
                            break;
                        case 45:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":LT->BUS");
                            tbBUS.BackColor = Color.LightBlue;
                            tbBUS.Text = tbLT.Text;
                            if(labelN.Text=="1")//有条件跳转
                            {
                                tbBUS.Text = ADD(tbBUS.Text, tbDR.Text);
                            }     
                            laBUS.Text = tbBUS.Text;
                            nextTimeth = 46;
                            break;
                        case 46:
                            tbLT.BackColor = Color.White;
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":BUS->MDR");
                            tbMDR.BackColor = Color.LightBlue;
                            tbMDR.Text = tbBUS.Text;
                            laMDR.Text = tbMDR.Text;
                            nextTimeth = 47;
                            break;
                        case 47:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":WRITE");
                            dataTemp[Convert.ToInt32(memoryAddress, 2)] = tbBUS.Text;
                            nextTimeth = 48;
                            break;
                        case 48:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":WAIT");
                            dataTemp[Convert.ToInt32(memoryAddress, 2)] = tbBUS.Text;
                            nextTimeth = 49;
                            break;
                        case 49:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":END");
                            enter = 0;
                            currentTimeth = 1;
                            nextTimeth = 0;
                            break;
                    }
                    break;
                case "1110"://载入立即数
                    switch (nextTimeth)
                    {
                        case 40:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":SR->BUS");
                            tbBUS.BackColor = Color.LightBlue;
                            tbSR.BackColor = Color.LightBlue;
                            tbBUS.Text = tbSR.Text.PadLeft(16, '0');
                            laBUS.Text = tbBUS.Text;
                            nextTimeth = 44;
                            break;
                        case 44:
                            tbSR.BackColor = Color.White;
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":BUS->MDR");
                            tbMDR.BackColor = Color.LightBlue;
                            tbMDR.Text = tbBUS.Text;
                            laMDR.Text = tbMDR.Text;
                            nextTimeth = 45;
                            break;
                        case 45:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":WRITE");
                            nextTimeth = 46;
                            break;
                        case 46:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":WAIT");
                            dataTemp[Convert.ToInt32(memoryAddress, 2)] = laBUS.Text;
                            switch (Convert.ToInt32(memoryAddress, 2))
                            {
                                case 0:
                                    labelR0.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                                case 1:
                                    labelR1.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                                case 2:
                                    labelR2.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                                case 3:
                                    labelR3.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                                case 4:
                                    labelR4.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                                case 5:
                                    labelR5.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                                case 6:
                                    labelR6.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                                case 7:
                                    labelR7.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                                case 8:
                                    labelR8.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                                case 9:
                                    labelR9.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                                case 10:
                                    labelR10.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                                case 11:
                                    labelR11.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                                case 12:
                                    labelR12.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                                case 13:
                                    labelR13.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                                case 14:
                                    labelR14.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                                case 15:
                                    labelR15.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                    break;
                            }
                            nextTimeth = 47;
                            break;
                        case 47:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":END");

                            enter = 0;
                            currentTimeth = 1;
                            nextTimeth = 0;
                            break;
                    }
                    break;
                case "1001":
                    if (Convert.ToString(tbIR.Text).Substring(4, 4) == "0000")//装载
                    {
                        switch (nextTimeth)
                        {
                            case 40:
                                listBoxMicroinstruct.Items.Add(++microinstructNumber + ":SR->BUS");
                                tbBUS.BackColor = Color.LightBlue;
                                tbSR.BackColor = Color.LightBlue;
                                tbBUS.Text = tbSR.Text.PadLeft(16, '0');
                                laBUS.Text = tbBUS.Text;
                                nextTimeth = 44;
                                break;
                            case 44:
                                tbSR.BackColor = Color.White;
                                listBoxMicroinstruct.Items.Add(++microinstructNumber + ":BUS->MDR");
                                tbMDR.BackColor = Color.LightBlue;
                                tbMDR.Text = tbBUS.Text;
                                laMDR.Text = tbMDR.Text;
                                nextTimeth = 45;
                                break;
                            case 45:
                                listBoxMicroinstruct.Items.Add(++microinstructNumber + ":WRITE");
                                nextTimeth = 46;
                                break;
                            case 46:
                                listBoxMicroinstruct.Items.Add(++microinstructNumber + ":WAIT");
                                dataTemp[Convert.ToInt32(memoryAddress, 2)] = laBUS.Text;
                                switch (Convert.ToInt32(memoryAddress, 2))
                                {
                                    case 0:
                                        labelR0.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 1:
                                        labelR1.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 2:
                                        labelR2.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 3:
                                        labelR3.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 4:
                                        labelR4.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 5:
                                        labelR5.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 6:
                                        labelR6.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 7:
                                        labelR7.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 8:
                                        labelR8.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 9:
                                        labelR9.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 10:
                                        labelR10.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 11:
                                        labelR11.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 12:
                                        labelR12.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 13:
                                        labelR13.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 14:
                                        labelR14.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 15:
                                        labelR15.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                }
                                nextTimeth = 47;
                                break;
                            case 47:
                                listBoxMicroinstruct.Items.Add(++microinstructNumber + ":END");
                                enter = 0;
                                currentTimeth = 1;
                                nextTimeth = 0;
                                break;
                        }
                    }
                    else if (Convert.ToString(tbIR.Text).Substring(4, 4) == "0010")//存储
                    {
                        switch (nextTimeth)
                        {
                            case 40:
                                listBoxMicroinstruct.Items.Add(++microinstructNumber + ":SR->BUS");
                                tbBUS.BackColor = Color.LightBlue;
                                tbSR.BackColor = Color.LightBlue;
                                tbBUS.Text = tbSR.Text.PadLeft(16, '0');
                                laBUS.Text = tbBUS.Text;
                                nextTimeth = 44;
                                break;
                            case 44:
                                tbSR.BackColor = Color.White;
                                listBoxMicroinstruct.Items.Add(++microinstructNumber + ":BUS->MDR");
                                tbMDR.BackColor = Color.LightBlue;
                                tbMDR.Text = tbBUS.Text;
                                laMDR.Text = tbMDR.Text;
                                nextTimeth = 45;
                                break;
                            case 45:
                                listBoxMicroinstruct.Items.Add(++microinstructNumber + ":WRITE");
                                nextTimeth = 46;
                                break;
                            case 46:
                                listBoxMicroinstruct.Items.Add(++microinstructNumber + ":WAIT");
                                dataTemp[Convert.ToInt32(memoryAddress, 2)] = laBUS.Text;
                                switch (Convert.ToInt32(memoryAddress, 2))
                                {
                                    case 0:
                                        labelR0.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 1:
                                        labelR1.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 2:
                                        labelR2.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 3:
                                        labelR3.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 4:
                                        labelR4.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 5:
                                        labelR5.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 6:
                                        labelR6.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 7:
                                        labelR7.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 8:
                                        labelR8.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 9:
                                        labelR9.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 10:
                                        labelR10.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 11:
                                        labelR11.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 12:
                                        labelR12.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 13:
                                        labelR13.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 14:
                                        labelR14.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                    case 15:
                                        labelR15.Text = laBUS.Text.Substring(laBUS.Text.Length - 8, 8);
                                        break;
                                }
                                nextTimeth = 47;
                                break;
                            case 47:
                                listBoxMicroinstruct.Items.Add(++microinstructNumber + ":END");
                                enter = 0;
                                currentTimeth = 1;
                                nextTimeth = 0;
                                
                                break;
                        }
                    }
                    break;
            }
            tbBUS.BackColor = Color.White;
            tbMDR.BackColor = Color.White;
        }

        void singlestep()//单步执行简单操作 （已设定的微程序序列）
        {
            listBoxTime.SelectedIndex = currentTimeth - 1;//设置当前周期状态
            switch (currentTimeth)
            {
                case 1://FT 取指周期
                    switch (nextTimeth)//判断下一个要执行的机器周期
                    {
                        case 0:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":PC->BUS");      
                            tbBUS.Text = tbPC.Text;//textBox:PC的值赋给BUS
                            laBUS.Text = tbBUS.Text;//label同步
                            tbPC.BackColor = Color.LightBlue;
                            tbBUS.BackColor = Color.LightBlue;//textBox BUS高亮显示
                            nextTimeth = 1;//下址字段设为1，以便执行下一阶段
                            break;
                        case 1:
                            tbPC.BackColor = Color.White;//textBox PC显示灭
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":BUS->MAR");
                            memoryAddress = laBUS.Text;
                            tbMAR.Text = tbBUS.Text;
                            laMAR.Text = tbMAR.Text;
                            tbMAR.BackColor = Color.LightBlue;
                            nextTimeth = 2;
                            break;
                        case 2:
                            tbBUS.BackColor = Color.White;
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":READ");
                            nextTimeth = 3;
                            break;
                        case 3:
                            tbMAR.BackColor = Color.White;
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":CLEAR LA");
                            tbLA.BackColor = Color.LightBlue;
                            nextTimeth = 4;
                            break;
                        case 4:
                            tbLA.BackColor = Color.White;
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":1->C0");
                            nextTimeth = 5;
                            break;
                        case 5:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":ADD");
                            if (Convert.ToInt32(memoryAddress, 2) < 0 + (instructTemp.Length) * 2)//判断是否还有空间可以存指令
                            {
                                instructAddrStart = Convert.ToInt32(memoryAddress, 2);//赋给指令地址起始
                                instructAddrStart = instructAddrStart + 2;//遍历下一条指令(双字节)
                                nextmachineCode = Convert.ToString(instructAddrStart, 2).PadLeft(16, '0');//赋给下一指令二进制字符串
                            }
                            nextTimeth = 6;
                            break;
                        case 6:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":ALU->LT");
                            tbLT.Text = nextmachineCode;
                            tbLT.BackColor = Color.LightBlue;
                            nextTimeth = 7;
                            break;
                        case 7:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":LT->BUS");
                            tbBUS.BackColor = Color.LightBlue;
                            tbBUS.Text = tbLT.Text;
                            laBUS.Text = tbBUS.Text;
                            nextTimeth = 8;
                            break;
                        case 8:
                            tbLT.BackColor = Color.White;
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":BUS->PC");
                            tbPC.BackColor = Color.LightBlue;
                            tbPC.Text = tbBUS.Text;
                            laPC.Text = tbPC.Text;       
                            nextTimeth = 9;
                            break;
                        case 9:
                            tbPC.BackColor = Color.White;
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":WAIT");
                            int index = Convert.ToInt32(memoryAddress, 2) / 2;
                            //得到相对于起始地址的偏移量，再除以 2，即可得到在instructTemp中对应的指令索引
                            if (index >= 0 && index < instructTemp.Length)
                            {
                                tbMDR.Text = instructTemp[index];
                            }
                            else// 处理索引超出范围的情况
                            {
                                tbMDR.Text = instructTemp[index - 1];
                            }
                            //tbMDR.Text = instructTemp[(Convert.ToInt32(memoryAddress, 2) - 0) / 2];   
                            laMDR.Text = tbMDR.Text;
                            nextTimeth = 10;
                            break;
                        case 10:
                            tbMDR.BackColor = Color.LightBlue;
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":MDR->BUS");
                            tbBUS.Text = tbMDR.Text;
                            laBUS.Text = tbBUS.Text;
                            nextTimeth = 11;
                            break;
                        case 11:
                            tbMDR.BackColor = Color.White;
                            tbIR.BackColor = Color.LightBlue;
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":BUS->IR");
                            tbIR.Text = tbBUS.Text;
                            laIMDR.Text = tbIR.Text;//将获得的IR内容赋给IMDR
                            laIMAR.Text = Convert.ToString(Convert.ToInt32(laIMAR.Text, 2) + 1, 2).PadLeft(16, '0'); ;//每次执行时将原来值加一
                            nextTimeth = 12;
                            break;
                        case 12:
                            listBoxMicroinstruct.Items.Add(++microinstructNumber + ":1->ST");
                            tbBUS.BackColor = Color.White;
                            tbIR.BackColor = Color.White;
                            currentTimeth++;//下一个周期
                            break;
                    }
                    break;
                case 2://ST 取源操作数周期
                    if (enter == 0)//如果初次进入此周期，需检测，若不是则可以已规定的微程序继续执行
                    {
                        nextTimeth = 13;
                        switch (tbIR.Text.Substring(0, 4))//取IR 0~4
                        {
                            case "0000":
                                if (Convert.ToString(tbIR.Text).Substring(4, 4) == "1100")//双操作数相加
                                {
                                    exeinstructTemp = Convert.ToString(tbIR.Text).Substring(12, 4);//传入源操作数
                                    ExeAddressingmode(exeinstructTemp);
                                    enter = 1;
                                }
                                else if (Convert.ToString(tbIR.Text).Substring(4, 4) == "1000")//双操作数相减
                                {
                                    exeinstructTemp = Convert.ToString(tbIR.Text).Substring(12, 4);
                                    ExeAddressingmode(exeinstructTemp);
                                    enter = 1;
                                }
                                else if(tbIR.Text.Substring(4, 4)=="0000")//空指令
                                {
                                    enter = 1;
                                    currentTimeth++;
                                }
                                break;
                            case "0010"://双操作数传送
                                exeinstructTemp = Convert.ToString(tbIR.Text).Substring(12, 4);//传入源操作数
                                ExeAddressingmode(exeinstructTemp);
                                enter = 1;
                                break;
                            case "1110"://载入立即数
                                exeinstructTemp = Convert.ToString(tbIR.Text).Substring(4, 8);
                                Exeimmediate(exeinstructTemp, false, Convert.ToString(tbIR.Text).Substring(12, 4));
                                tbSR.Text = exeinstructTemp;
                                laSR.Text = tbSR.Text;
                                enter = 1;
                                break;
                            case "1001":
                                if (Convert.ToString(tbIR.Text).Substring(4, 4) == "0010")//存储
                                {
                                    ExeAddressingmode("1110");
                                }
                                else if(Convert.ToString(tbIR.Text).Substring(4, 4) == "0000")//装载
                                {                      
                                    exeinstructTemp = Convert.ToString(tbIR.Text).Substring(12, 4);
                                    ExeAddressingmode(exeinstructTemp);
                                }
                                enter = 1;
                                break;
                            case "1100"://无条件相对跳转
                                enter = 1;
                                exeinstructTemp = Convert.ToString(tbIR.Text).Substring(8, 8);//传入唯一操作数
                                Exeimmediate(exeinstructTemp,false,"0000");//无目的寄存器，故传任意值即可
                                break;
                            case "1111"://有条件相对跳转
                                enter = 1;
                                exeinstructTemp = Convert.ToString(tbIR.Text).Substring(8, 8);//传入唯一操作数
                                Exeimmediate(exeinstructTemp, false, "0000");//无目的寄存器，故传任意值即可
                                break;
                            default://其他没有源操作数指令
                                enter = 0;
                                currentTimeth++;
                                break;
                            }
                        }
                        else if(Convert.ToString(tbIR.Text).Substring(0, 2) == "11")
                        {
                            Exeimmediate(exeinstructTemp, false, "0000");//无目的寄存器，故传任意值即可                          
                        }else
                        {
                            ExeAddressingmode(exeinstructTemp);
                        }
                            
                    break;
                    case 3://DT 取目的操作数周期
                        if (enter == 0)//如果初次进入此周期，需检测，若不是则可以已规定的微程序继续执行
                        {
                            nextTimeth = 13;
                            switch (Convert.ToString(tbIR.Text).Substring(0, 4))
                            {
                                case "0000":
                                    if(Convert.ToString(tbIR.Text).Substring(4, 4) == "1100")//双操作数相加
                                    {
                                        exeinstructTemp = Convert.ToString(tbIR.Text).Substring(8, 4);
                                        ExeAddressingmode(exeinstructTemp);
                                        enter = 1;
                                    }else if(Convert.ToString(tbIR.Text).Substring(4, 4) == "1000")//双操作数相减
                                    {
                                        exeinstructTemp = Convert.ToString(tbIR.Text).Substring(8, 4);
                                        ExeAddressingmode(exeinstructTemp);
                                        enter = 1;
                                    } 
                                    else if(Convert.ToString(tbIR.Text).Substring(4,4) == "0000")//空指令
                                    {
                                        enter = 1;
                                        currentTimeth++;
                                    } 
                                    break;
                                case "0010"://双操作数传送
                                    exeinstructTemp = Convert.ToString(tbIR.Text).Substring(8, 4);
                                    ExeAddressingmode(exeinstructTemp);
                                    enter = 1;
                                    break;
                                case "1110"://载入立即数
                                    enter = 1;
                                    Exeimmediate(exeinstructTemp, true, Convert.ToString(tbIR.Text).Substring(12, 4));
                                    break;
                                case "1001"://装载
                                    if (Convert.ToString(tbIR.Text).Substring(4, 4) == "0010")//存储
                                    {
                                        exeinstructTemp = Convert.ToString(tbIR.Text).Substring(12, 4);
                                        ExeAddressingmode(exeinstructTemp);
                                    }
                                    else if (Convert.ToString(tbIR.Text).Substring(4, 4) == "0000")//装载
                                    {
                                        ExeAddressingmode("1110");
                                    }
                                    enter = 1;
                                    break;
                                default://其他没有目的操作数指令
                                    enter = 0;
                                    currentTimeth++;
                                    break;
                            }
                        }
                        else if (Convert.ToString(tbIR.Text).Substring(0, 2) == "11")
                        {
                            Exeimmediate(exeinstructTemp, true, Convert.ToString(tbIR.Text).Substring(12, 4));                          
                        }
                        else
                        {
                            ExeAddressingmode(exeinstructTemp);
                        }
                    break;
                    case 4://ET 执行周期
                        if (enter == 0)//如果初次进入此周期，需检测，若不是则可以已规定的微程序继续执行
                        {
                            nextTimeth = 40;
                            enter = 1;
                            exeOperation();
                        }
                        else
                        {
                            exeOperation();
                        }
                        break;
            }
        }

        private void openFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = @"C: \Users\Administrator\Documents\Visual Studio 2015\Projects\CourseDesign\CourseDesign\bin\Debug";//设置初始目录
            openFileDialog.Filter = "Data Files (*.data)|*.data|All Files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                richTextCode.Text = "";//机器码框清空
                instructMemory.Clear();//指令存储器（列表）
                int tempNumber = Convert.ToInt32(instructNumber, 16);
                StreamReader sReader = new StreamReader(openFileDialog.FileName, Encoding.Default);
                richTextInstruct.Text = sReader.ReadToEnd();//设置汇编指令框——为文件内容
                for (int i = 0; i < richTextInstruct.Lines.Length && richTextInstruct.Lines[i] != ""; i++)//汇编指令框中内容一条一条循环
                {
                    //指令格式（操作码，源操作数，目的操作数）
                    //源操作数->目的操作数
                    substring = richTextInstruct.Lines[i].ToUpper().Replace("  ", " ").Split(' ');//以空格分割（码，数）
                    switch (substring[0])//译码算数指令
                    {
                        case "ADD"://双操作数相加
                            machineCode = machineCode + "00001100";//固定的（看机器指令组成），机器指令二进制字符串赋值
                            substring = substring[1].Split(new char[] { ',', '，' });//以逗号分割（数1，数2）
                            mateAddressingMode(substring[1]);//寻址
                            mateAddressingMode(substring[0]);
                            break;
                        case "SUB"://双操作数相减
                            machineCode = machineCode + "00001000";
                            substring = substring[1].Split(new char[] { ',', '，' });
                            mateAddressingMode(substring[1]);
                            mateAddressingMode(substring[0]);
                            break;
                        case "RJMP"://无条件跳转
                            machineCode = machineCode + "11000000";
                            mateAddressingMode(substring[1]);
                            break;
                        case "BRMI"://有条件跳转
                            machineCode = machineCode + "11110001";
                            mateAddressingMode(substring[1]);
                            break;
                        case "MOV"://数据传送
                            machineCode = machineCode + "00101100";
                            substring = substring[1].Split(new char[] { ',', '，' });
                            mateAddressingMode(substring[1]);
                            mateAddressingMode(substring[0]);
                            break;
                        case "LDI"://载入立即数
                            machineCode = machineCode + "1110";
                            substring = substring[1].Split(new char[] { ',', '，' });
                            mateAddressingMode(substring[1]);
                            mateAddressingMode(substring[0]);
                            break;
                        case "LD"://装载指令
                            machineCode = machineCode + "100100001100";
                            substring = substring[1].Split(new char[] { ',', '，' });
                            mateAddressingMode(substring[1]);
                            mateAddressingMode(substring[0]);
                            break;
                        case "ST"://存储
                            machineCode = machineCode + "100100101100";
                            substring = substring[1].Split(new char[] { ',', '，' });
                            mateAddressingMode(substring[1]);
                            mateAddressingMode(substring[0]);
                            break;
                        case "NOP"://空操作指令
                            machineCode = machineCode + "0000";
                            machineCode = machineCode + "000000000000";
                            break;
                        default:
                            machineCode = machineCode + "0000";
                            break;
                    }
                    //机器码框写内容
                    richTextCode.AppendText(machineCode.Substring(0, 4) + ' ' +
                        machineCode.Substring(4, 4) + ' ' + machineCode.Substring(8, 4) + ' ' +
                        machineCode.Substring(12, 4) + "\n");//分割成四组，每组4位，4组后换行                   
                    instructMemory.Add(machineCode);//添加指令到内存中

                    //指令存储单元框写内容
                    ListViewItem item1 = new ListViewItem(tempNumber.ToString("X").PadLeft(4, '0'));  // 第一列数据
                    item1.SubItems.Add(machineCode);         // 第二列数据
                    listViewInstruct.Items.Add(item1);
                    tempNumber++;

                    machineCode = "";//重置机器码

                    //主存单元框写内容
                    ListViewItem item2 = new ListViewItem(Convert.ToString(memoryNumber,16).ToUpper().PadLeft(4, '0'));  // 第一列数据
                    item2.SubItems.Add("0000");         // 第二列数据
                    listViewMemory.Items.Add(item2);
                    memoryNumber++;
                }
                instructTemp = instructMemory.ToArray();//打开之后遍历转换为数组形式
            }
            buSaveFile.Enabled = true; // 解开其他按钮禁用
            buSingleStep.Enabled = true;
            buFullStep.Enabled = true;
            buPause.Enabled = true;
            buClear.Enabled = true;
        }

        private void buttonSingleStep_Click(object sender, EventArgs e)
        {
            singlestep();
            listBoxMicroinstruct.SelectedIndex = microinstructNumber - 1;
        }

        private async void buFullStep_Click(object sender, EventArgs e)
        {
            buFullStep.Enabled = false;
            //int i = 0;//辅助变量 判断是否遍历结束
            while (instructAddrStart != 0 + (richTextInstruct.Lines.Length -1) * 2 || nextTimeth != 0)
            {
                if (isPaused)
                {
                    // 暂停状态，等待继续命令
                    await Task.Delay(100); // 异步延时100毫秒钟
                    continue;
                }

                singlestep();
                listBoxMicroinstruct.SelectedIndex = microinstructNumber - 1;
                if (microInstructEnd)//微指令序列结束
                {
                    listBoxMicroinstruct.Items.Add("该结束了");
                    break;
                }
                //i++;
                await Task.Delay(100); // 异步延时100毫秒钟
            }
        }

        private void buClear_Click(object sender, EventArgs e)
        {
            richTextCode.Clear();
            richTextInstruct.Clear();
            listBoxMicroinstruct.Items.Clear();
            listViewMemory.Items.Clear();
            listViewInstruct.Items.Clear();
            listBoxTime.ClearSelected();
            buSaveFile.Enabled = false;
            buSingleStep.Enabled = false;
            buFullStep.Enabled = false;
            buPause.Enabled = false;

            //所有label，textbox置0
            labelR0.Text = "00000000"; labelR1.Text = "00000000";
            labelR2.Text = "00000000"; labelR3.Text = "00000000";
            labelR4.Text = "00000000"; labelR5.Text = "00000000";
            labelR6.Text = "00000000"; labelR7.Text = "00000000";
            labelR8.Text = "00000000"; labelR9.Text = "00000000";
            labelR10.Text = "00000000"; labelR11.Text = "00000000";
            labelR12.Text = "00000000"; labelR13.Text = "00000000";
            labelR14.Text = "00000000"; labelR15.Text = "00000000";
            laPC.Text = "0000000000000000";
            laBUS.Text = "0000000000000000";
            laMAR.Text = "0000000000000000";
            laMDR.Text = "00000000";
            laIMAR.Text = "0000000000000000";
            laIMDR.Text = "0000000000000000";
            laSR.Text = "00000000";
            laDR.Text = "00000000";
            tbBUS.Text = "0000000000000000";
            tbDR.Text = "00000000";
            tbSR.Text = "00000000";
            tbPC.Text= "0000000000000000";
            tbMAR.Text = "0000000000000000";
            tbMDR.Text = "00000000";
            tbLA.Text= "00000000";
            tbLT.Text = "00000000";
            tbIR.Text = "00000000";

            //关闭高亮
            tbBUS.BackColor=Color.White;
            tbDR.BackColor = Color.White;
            tbSR.BackColor = Color.White;
            tbPC.BackColor = Color.White;
            tbMAR.BackColor = Color.White;
            tbMDR.BackColor = Color.White;
            tbLA.BackColor = Color.White;
            tbLT.BackColor = Color.White;
            tbIR.BackColor = Color.White;
        }

        private void buPause_Click(object sender, EventArgs e)
        {
            isPaused = !isPaused;
            buFullStep.Enabled = false;
            // 根据暂停状态修改按钮文字
            if (isPaused)
            {
                buPause.Text = "继续执行";
            }
            else
            {
                buPause.Text = "暂停执行";
            }
        } 

        private void buSaveFile_Click(object sender, EventArgs e)
        {
            string filename = "数据.txt"; // 文件名
            try
            {
                // 使用 StreamWriter 打开或创建文件
                using (StreamWriter writer = new StreamWriter(filename))
                {
                    writer.WriteLine("通用寄存器R0~R15情况：");// 将各寄存器情况写入文件
                    writer.WriteLine("  R0：" + labelR0.Text);
                    writer.WriteLine("  R1：" + labelR1.Text);
                    writer.WriteLine("  R2：" + labelR2.Text);
                    writer.WriteLine("  R3：" + labelR3.Text);
                    writer.WriteLine("  R4：" + labelR4.Text);
                    writer.WriteLine("  R5：" + labelR5.Text);
                    writer.WriteLine("  R6：" + labelR6.Text);
                    writer.WriteLine("  R7：" + labelR7.Text);
                    writer.WriteLine("  R8：" + labelR8.Text);
                    writer.WriteLine("  R9：" + labelR9.Text);
                    writer.WriteLine("R10：" + labelR10.Text);
                    writer.WriteLine("R11：" + labelR11.Text);
                    writer.WriteLine("R12：" + labelR12.Text);
                    writer.WriteLine("R13：" + labelR13.Text);
                    writer.WriteLine("R14：" + labelR14.Text);
                    writer.WriteLine("R15：" + labelR15.Text);

                    writer.WriteLine("程序计数器情况：");//将其他寄存器情况写入文件
                    writer.WriteLine("  PC：" + tbPC.Text);
                }
            MessageBox.Show("文件导出成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("文件导出失败：" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        string ADD(string aa, string bb) //加法器
        {

            string[] a = new string[] { "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0" };
            for (int i = 0; i < aa.Length; i++)
            {
                a[i] = Convert.ToString(aa[i]);
            }
            string[] b = new string[] { "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0" };
            for (int i = 0; i < bb.Length; i++)
            {
                b[i] = Convert.ToString(bb[i]);
            }
            int[] d = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };//结果
            int c = 0;//进位
            for (int i = 15; i > -1; i--)
            {
                if (Convert.ToInt32(a[i]) + Convert.ToInt32(b[i]) + c == 2)//有进位，没有来自上一位的进位
                {
                    d[i] = 0;
                    c = 1;
                }
                else if (Convert.ToInt32(a[i]) + Convert.ToInt32(b[i]) + c == 3)//有进位，有来自上一位的进位
                {
                    d[i] = 1;
                    c = 1;
                }
                else//一般得数
                {
                    d[i] = Convert.ToInt32(a[i]) + Convert.ToInt32(b[i]) + c;
                    c = 0;
                }
            }
            string x = "";
            for (int i = 0; i < d.Length; i++)
            {
                x = x + Convert.ToString(d[i]);
            }
            if (x == "0000000000000000")
            {
                labelZ.Text = "1";
            }
            return x;
        }

        string SUB(string aa, string bb)//减法器
        {
            string[] a = new string[] { "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0" };
            string n1 = "";
            for (int i = 0; i < aa.Length; i++)
            {
                a[i] = Convert.ToString(aa[i]);//原码
                n1 = n1 + a[i];
            }
            string[] b = new string[] { "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0", "0" };
            string n2 = "";
            for (int i = 0; i < bb.Length; i++)
            {
                if (Convert.ToString(bb[i]) == "1")
                {
                    b[i] = "0";
                }
                else
                {
                    b[i] = "1";
                }
                n2 = n2 + b[i];
            }
            n2 = ADD(n2.PadLeft(16, '0'), "0000000000000001");//反码加1
            n1 = ADD(n2, n1.PadLeft(16, '0'));
            if (n1 == "0000000000000000")
            {
                labelZ.Text = "1";
            }
            return n1;
        }
    }
}
