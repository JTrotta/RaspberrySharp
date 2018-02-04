using Demo.Utilities;
using NLog;
using RaspberrySharp.IO.GeneralPurpose;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Tests.Demo
{
    public partial class Main : Form
    {
        //GPIO
        private GpioConnection _connectionGlobalPin;

        public Main()
        {
            InitializeComponent();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            InitGPIO();
        }

        private void InitGPIO()
        {
            cbGPIO.DataSource = Enum.GetValues(typeof(ProcessorPin));
        }

        private void BtnOnOff_Click(object sender, EventArgs e)
        {
            ToogleGPIO((ProcessorPin)cbGPIO.SelectedValue);
        }

        private void ToogleGPIO(ProcessorPin selectedGPIO)
        {
            OutputPinConfiguration _gpio = selectedGPIO.Output();
            try
            {
                if (_connectionGlobalPin == null)
                {
                    _connectionGlobalPin = new GpioConnection(_gpio);
                }

                if (!_connectionGlobalPin.Contains(_gpio))
                    _connectionGlobalPin.Add(_gpio);

                _connectionGlobalPin.Pins[_gpio].Enabled = !_connectionGlobalPin.Pins[_gpio].Enabled;
                CommonHelper.Logger.Info("GPIO {0}: enabled", _connectionGlobalPin.Pins[_gpio].Enabled);
            }
            catch (Exception e)
            {
                CommonHelper.Logger.Error(e, "GPIO Error : {0}", e.Message);
            }
        }
    }
}
