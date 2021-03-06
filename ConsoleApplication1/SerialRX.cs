﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Threading;
using SharpDX.DirectInput; 

namespace SerialRX
{
    class SerialControl
    {
        static SerialPort serialPort = new SerialPort();
        static int __xVal = 0;
        static int __zVal = 0;
        static bool __hasChanged = false;

        static private int GetAxisValuePercentage(double maxValue, double currentValue)
        {
            return (int)Math.Round(currentValue * 100 / maxValue);
        }

        static private void ProcessJoystickUpdate(JoystickUpdate state)
        {
            if (state.Offset == JoystickOffset.X)
            {
                int xAxisPercent = GetAxisValuePercentage(65535, state.Value);
                __hasChanged = true;
            }
            if (state.Offset == JoystickOffset.Y)
            {
                int yAxisPercent = GetAxisValuePercentage(65535, state.Value);
                SetXVal(yAxisPercent);
                __hasChanged = true;
            }
            else if (state.Offset == JoystickOffset.RotationZ)
            {
                int zAxisPercent = GetAxisValuePercentage(65535, state.Value);
                SetZVal(zAxisPercent);
                __hasChanged = true;
            }
            if (__hasChanged)
            {
                SendCommand(GetFinalValues()+".");
            }
        }

        static void SetupSerial()
        {
            serialPort.BaudRate = 9600;
            serialPort.PortName = "COM9";
            serialPort.Parity = Parity.None;
            serialPort.StopBits = StopBits.One;
            serialPort.Handshake = Handshake.None;

            //**uncomment for live :)
            serialPort.Open();
            serialPort.WriteLine("I");                                     //initialise device
        }

        static void SendCommand(string command)
        {
            //**uncomment for live :)
            serialPort.Write(command);
            if (command == "0|0.") {
                serialPort.Write(command);
            }
            Console.WriteLine(command);

            string msg = serialPort.ReadLine();
            Console.WriteLine(msg);
        }

        static void SetXVal(int xVal)
        {
            __xVal = (xVal - 50)*-1;
        }

        static void SetZVal(int zVal)
        {
//            if (zVal < 0) { zVal = zVal + 50; }
            __zVal = zVal;
        }

        static int GetXVal()
        {
            return __xVal;
        }

        static int GetZVal()
        {
            return __zVal;
        }

        static string GetFinalValues()
        {
            int left = GetXVal()*2;
            int right = GetXVal()*2;
            if (GetZVal() > 50)
            {
                left = left + (GetZVal()-50);
            }
            if (GetZVal() < 50)
            {
                right = right + (50-GetZVal());
            }
            return left.ToString() + "|" + right.ToString();
        }

        static void Main(string[] args)
        {
            SetupSerial();
            CancellationTokenSource _joystickMonitorCancellation = new CancellationTokenSource();
            var joystickMonitor = new JoystickMonitor("USB Gamepad");
            var progress = new Progress<JoystickUpdate>(s => ProcessJoystickUpdate(s));
            while (1==1) 
            {
                joystickMonitor.PollJoystick(progress, _joystickMonitorCancellation.Token);
            }
            
        }
    }
}

