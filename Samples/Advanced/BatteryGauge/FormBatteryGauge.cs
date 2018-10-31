using BatteryGauge.I2C;
using RaspberrySharp.IO.GeneralPurpose;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BatteryGauge
{
    public partial class FormBatteryGauge : Form
    {
        private LTC2943Service _LTC2943Service;
        private GpioConnection _gpioConnectionGlobalPin;
        ConnectedPin _modemResetPin;

        public FormBatteryGauge()
        {
            //ushort d = 0x7fff;
            //byte[] byteArray = BitConverter.GetBytes(d).Reverse().ToArray();

            //var p = 0.34E-3;

            InitializeComponent();
            _LTC2943Service = new LTC2943Service();
            _LTC2943Service.OnGaugeChanged += _LTC2943Service_OnGaugeChanged;
            _LTC2943Service.OnUnderCharge += _LTC2943Service_OnUnderCharge;

            _gpioConnectionGlobalPin = new GpioConnection();
            OutputPinConfiguration opc = ProcessorPin.Gpio06.Output();
            _gpioConnectionGlobalPin.Add(opc);
            _modemResetPin = _gpioConnectionGlobalPin.Pins[ProcessorPin.Gpio06];
        }


        private void btnStartStop_Click(object sender, EventArgs e)
        {
            //_LTC2943Service.SetPrescaler = radioButton1.Checked ? 1024 : 4096;
            if (this.btnStartStop.Text == "START")
            {
                _LTC2943Service.Start();
                this.btnStartStop.Text = "STOP";
            }
            else
            {
                this.btnStartStop.Text = "START";
                _LTC2943Service.Stop();
            }
        }

        private void _LTC2943Service_OnUnderCharge(UnderChargeEventArgs args)
        {
            if (args.Charging)
                imgLed.Invoke(new Action(() => imgLed.Image =  Properties.Resources.led_red_control_hi));
            else
                imgLed.Invoke(new Action(() => imgLed.Image = Properties.Resources.led_off_control_hi));
        }

        private void _LTC2943Service_OnGaugeChanged(GaugeEventArgs args)
        {
            if (args.LedOn)
                imgLed.Invoke(new Action(() => imgLed.Image = Properties.Resources.led_red_control_hi));
            else
                imgLed.Invoke(new Action(() => imgLed.Image = Properties.Resources.led_off_control_hi));

            pbCharge.Invoke(new Action(() => pbCharge.Value = (int)args.ChargePercentage));
            lblPercentage.Invoke(new Action(() => lblPercentage.Text = String.Format("{0} %", args.ChargePercentage)));
            lblRemTime.Invoke(new Action(() => lblRemTime.Text = args.RemainingTime.ToString(@"hh\:mm")));
            pbCurrent.Invoke(new Action(() => pbCurrent.Value = (int)Math.Abs(args.Current.Milliamperes)));
            pbVoltage.Invoke(new Action(() => pbVoltage.Value = (int)args.Voltage.VoltsDc));
            //pbTemperature.Invoke(new Action(() => pbTemperature.Value = (int)Math.Abs(args.Temperature.DegreesCelsius)));

            double A = args.Current.Amperes;
            lblAmpere.Invoke(new Action(() => lblAmpere.Text = String.Format("{0:0.00} A", A)));
            double V = args.Voltage.VoltsDc;
            lblVolt.Invoke(new Action(() => lblVolt.Text = String.Format("{0:0.00} V", V)));
            double T = args.Temperature.DegreesCelsius;
            lblTemp.Invoke(new Action(() => lblTemp.Text = String.Format("{0:0.0}° C", T)));

            checkBox1.Invoke(new Action(() => checkBox1.Checked = (args.Status & 1) == 1));
            checkBox2.Invoke(new Action(() => checkBox2.Checked = (args.Status & 2) == 2));
            checkBox3.Invoke(new Action(() => checkBox3.Checked = (args.Status & 4) == 4));
            checkBox4.Invoke(new Action(() => checkBox4.Checked = (args.Status & 8) == 8));
            checkBox5.Invoke(new Action(() => checkBox5.Checked = (args.Status & 16) == 16));
            checkBox6.Invoke(new Action(() => checkBox6.Checked = (args.Status & 32) == 32));
            checkBox7.Invoke(new Action(() => checkBox7.Checked = (args.Status & 64) == 64));
            checkBox8.Invoke(new Action(() => checkBox8.Checked = (args.Status & 128) == 128));          
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            _LTC2943Service.Restart();
        }

        private void btnFull_Click(object sender, EventArgs e)
        {
            _LTC2943Service.SetChargingComplete();
        }

        private void FormBatteryGauge_FormClosing(object sender, FormClosingEventArgs e)
        {
            _LTC2943Service.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _modemResetPin.Enabled = !_modemResetPin.Enabled;
        }
    }
}
