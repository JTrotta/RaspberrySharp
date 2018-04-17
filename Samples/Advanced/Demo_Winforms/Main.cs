using Demo.Modules;
using Demo.Utilities;

using RaspberrySharp.IO.GeneralPurpose;
using System;
using System.Windows.Forms;

namespace Tests.Demo
{
    using RaspberrySharp.System;

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
            //ShowSystem();
            InitGPIO();
        }

        private void ShowSystem()
        {
            lbSystem.Items.Add($"Model : {Board.Current.Model}");
            lbSystem.Items.Add($"Is Raspberry : {Board.Current.IsRaspberryPi}");
            lbSystem.Items.Add($"ConnectorPinout : {Board.Current.ConnectorPinout}");
            lbSystem.Items.Add($"Firmware : {Board.Current.Firmware}");
            lbSystem.Items.Add($"Is Overclocked : {Board.Current.IsOverclocked}");
            lbSystem.Items.Add($"Processor : {Board.Current.Processor}");
            lbSystem.Items.Add($"ProcessorName : {Board.Current.ProcessorName}");
            lbSystem.Items.Add($"SerialNumber : {Board.Current.SerialNumber}");
            lbSystem.Update();
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

        private void BtnStartOW_Click(object sender, EventArgs e)
        {
            DS18B20Service ow = new DS18B20Service();
            ow.OnTemperatureChanged += Ow_OnTemperatureChanged;
            ow.Start();
        }

        private void Ow_OnTemperatureChanged(DS18B20EventArgs args)
        {
            lblTemperature.Invoke(new Action(() => lblTemperature.Text = args.T.DegreesCelsius.ToString()));
        }
    }
}
