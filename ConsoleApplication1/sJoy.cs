using SharpDX.DirectInput;
using System;
using System.Linq;
using System.Threading;

namespace SerialRX
{
    public class JoystickMonitor
    {
        private string _joystickName;

        public JoystickMonitor(string joystickName)
        {
            _joystickName = joystickName;
        }

        public void PollJoystick(IProgress<JoystickUpdate> progress, CancellationToken cancellationToken)
        {
            var directInput = new DirectInput();

            DeviceInstance device = directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AttachedOnly).Single();
            device = directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AttachedOnly)
                                          .SingleOrDefault(x => x.ProductName == device.ProductName);

            if (device == null)
            {
                throw new Exception(string.Format("Can't find joystick:", _joystickName));
            }

            var joystick = new Joystick(directInput, device.InstanceGuid);

            joystick.Properties.BufferSize = 128;
            joystick.Acquire();

            while (!cancellationToken.IsCancellationRequested)
            {
                joystick.Poll();
                JoystickUpdate[] states = joystick.GetBufferedData();

                foreach (var state in states)
                {
                    progress.Report(state);
                }
            }
        }
    }
}