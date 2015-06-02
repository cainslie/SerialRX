using System;
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
                SetXVal(xAxisPercent);
                __hasChanged = true;
            }
            if (state.Offset == JoystickOffset.Y)
            {
                int yAxisPercent = GetAxisValuePercentage(65535, state.Value);
                __hasChanged = true;
            }
            else if (state.Offset == JoystickOffset.RotationZ)
            {
                int zAxisPercent = GetAxisValuePercentage(65535, state.Value);
                SetZVal(zAxisPercent);
                __hasChanged = true;
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
            //serialPort.Open();
            //serialPort.Write("I");                                     //initialise device
        }

        static void SendCommand(string command)
        {
            //**uncomment for live :)
            //serialPort.Write(command);
            Console.WriteLine(command);
        }

        static void SetXVal(int xVal)
        {
            __xVal = xVal;
        }

        static void SetZVal(int zVal)
        {
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
            int left = GetXVal();
            int right = GetXVal();
            if (GetZVal() > 50)
            {
                left = left + GetZVal();
            }
            if (GetZVal() < 50)
            {
                right = right - GetZVal();
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
                if (__hasChanged)
                {
                    SendCommand(GetFinalValues());
                }
            }
            
        }
    }
}

