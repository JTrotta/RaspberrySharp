using BatteryGauge.GPIO;
using RaspberrySharp.IO.GeneralPurpose;
using SampleCommon.Buses;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnitsNet;

namespace BatteryGauge.I2C
{
    public enum LTC2943_Constants : int
    {
        ADDRESS = 0x64,
        STATUS = 0x00,
        CONTROL = 0x01,
        ACC_CHARGE_MSB = 0x02,
        ACC_CHARGE_LSB = 0x03,
        CHARGE_THRESH_HIGH_MSB = 0x04,
        CHARGE_THRESH_HIGH_LSB = 0x05,
        CHARGE_THRESH_LOW_MSB = 0x06,
        CHARGE_THRESH_LOW_LSB = 0x07,
        VOLTAGE_MSB = 0x08,
        VOLTAGE_LSB = 0x09,
        VOLTAGE_THRESH_HIGH_MSB = 0x0A,
        VOLTAGE_THRESH_HIGH_LSB = 0x0B,
        VOLTAGE_THRESH_LOW_MSB = 0x0C,
        VOLTAGE_THRESH_LOW_LSB = 0x0D,
        CURRENT_MSB = 0x0E,
        CURRENT_LSB = 0x0F,
        CURRENT_THRESH_HIGH_MSB = 0x10,
        CURRENT_THRESH_HIGH_LSB = 0x11,
        CURRENT_THRESH_LOW_MSB = 0x12,
        CURRENT_THRESH_LOW_LSB = 0x13,
        TEMPERATURE_MSB = 0x14,
        TEMPERATURE_LSB = 0x15,
        TEMPERATURE_THRESH_HIGH = 0x16,
        TEMPERATURE_THRESH_LOW = 0x17,
    }

    public class GaugeEventArgs : EventArgs
    {
        public ElectricCurrent Current { get; set; }

        public ElectricPotentialDc Voltage { get; set; }

        public Temperature Temperature { get; set; }

        public ElectricCharge Charge { get; set; }

        public double ChargePercentage { get; set; }

        public byte Status { get; set; }

        public TimeSpan RemainingTime { get; set; }

        public bool LedOn { get; set; }

    }

    public class UnderChargeEventArgs : EventArgs
    {
        public bool Charging { get; set; }
    }

    /// <summary>
    /// Battery Gauge
    /// </summary>
    public class LTC2943Service : I2CBus<LTC2943_Constants>
    {
        #region Const
        private const float CHARGE_LEAST = 0.34f; // 0.34 mAh
        //const float VOLTAGE_LEAST = 1.44f; // 1.44 mV
        //const float CURRENT_LEAST = 0.0293f; // 0.0293 mA
        const float TEMPERATURE_LEAST = 0.25f; // 0.25 C
        const float FULLSCALE_VOLTAGE = 23.6f; // 23.6 V
        const float FULLSCALE_CURRENT = 60f; // 60 mV
        const float FULLSCALE_TEMPERATURE = 510f; // 510 K
        const float SENSE_RESISTOR_VALUE = 50f; // 50 mohm

        const int ALERT_RESPONSE_ADDRESS = 0x0C;
        const int AUTOMATIC_MODE = 0xC0;
        const int SCAN_MODE = 0x80;
        const int MANUAL_MODE = 0x40;
        const int SLEEP_MODE = 0x00;
        const int PRESCALER_1 = 0x00;
        const int PRESCALER_4 = 0x08;
        const int PRESCALER_16 = 0x10;
        const int PRESCALER_64 = 0x18;
        const int PRESCALER_256 = 0x20;
        const int PRESCALER_1024 = 0x28;
        const int PRESCALER_4096 = 0x30;
        const int PRESCALER_4096_2 = 0x31;
        const int ALERT_MODE = 0x04;
        const int CHARGE_COMPLETE_MODE = 0x02;
        const int DISABLE_ALCC_PIN = 0x00;
        const int SHUTDOWN_MODE = 0x01;

        private readonly double BATTERY_CAPACITY_MAH = 1800;//mAh 
        private readonly double BATTERY_NOMINAL_VOLTAGE;//Volt 
        private readonly double BATTERY_MAX_VOLTAGE;//Volt 
        private double QLSB; //mAh
        private double QBATMAX;//mAH
        private const float COULOMBS_TO_MILLIAMPEREHOUR = 0.27777777778f;
        private ChargeStatusType _chargeStatus;
        private int _prescalerSelected;
        private bool _batteryUnderChargeStatus;
        #endregion

        #region Fields
        public delegate void DelegateBatteryGauge(GaugeEventArgs args);
        public delegate void DelegateUnderCharge(UnderChargeEventArgs args);
        public event DelegateBatteryGauge OnGaugeChanged;
        public event DelegateUnderCharge OnUnderCharge;
        private int _prescalerValue;
        private readonly ProcessorPin _batteryUnderChargeGpio;
        #endregion

        private enum ChargeStatusType
        {
            Charging,
            Discharging,
            Completed,
            NotCharging
        }


        #region Constructors
        public LTC2943Service(): base((int)LTC2943_Constants.ADDRESS)
        {
            this.Enabled = this.InitSensor();
            _batteryUnderChargeGpio = ProcessorPin.Gpio33;//GPIO33, led di carica
            GpioGlobalConnection.Add(_batteryUnderChargeGpio, false);
            GpioGlobalConnection.GlobalConnectionPin.PinStatusChanged += GlobalConnectionPin_PinStatusChanged;
            _chargeStatus = ChargeStatusType.NotCharging;
            _prescalerSelected = PRESCALER_1024;
            _batteryUnderChargeStatus = true;
            BATTERY_NOMINAL_VOLTAGE = 7.2;
            BATTERY_MAX_VOLTAGE = 8.38;
        }
        #endregion

        #region Utils

        private float CalculateAccumulatedChargeFromADCCode(int accChargeCode)
        {
            float mAh_charge = 0;

            //mAh_charge = (float)(accChargeCode * CHARGE_LEAST * this._prescalerValue * 50) / (SENSE_RESISTOR_VALUE * 4096f);

            mAh_charge = (float)(accChargeCode * QLSB);

            return mAh_charge;
        }

        private float CalculateVoltageFromADCCode(int voltageCode)
        {
            float calculatedVoltageValue = 0;

            calculatedVoltageValue = ((float)voltageCode / 65535) * FULLSCALE_VOLTAGE;

            return calculatedVoltageValue;
        }

        private float CalculateCurrentFromADCCode(int currentCode)
        {
            float calculatedCurrentValue = 0;

            calculatedCurrentValue = (((float)currentCode - 32767) / 32767) * ((float)(FULLSCALE_CURRENT) / SENSE_RESISTOR_VALUE);

            return calculatedCurrentValue;
        }

        private float CalculateTemperatureFromADCCode(int tempCode)
        {
            float calculatedTempValue = 0;

            calculatedTempValue = tempCode * ((float)(FULLSCALE_TEMPERATURE) / 65535);// - 273.15f;

            return calculatedTempValue;
        }


        /// <summary>
        /// Convert a charge in mAh to a register value.
        /// Note: no overflow checking is done here, such effects must be checked by the caller.
        /// </summary>
        /// <param name="chargeMAH"></param>
        /// <returns></returns>
        private void SetChargeMAHToRegister(double chargeMAH)
        {
            // Rearranging from the above 
            //x = 1000 * (float)(accChargeCode * CHARGE_LEAST * this._prescalerValue * 0.05) / (SENSE_RESISTOR_VALUE * 4096);
            //double qLSB = (SENSE_RESISTOR_VALUE * 4096) / (1000 * CHARGE_LEAST * _prescalerValue * 0.05);
            //double qLSB = (double)((CHARGE_LEAST * 50 *  _prescalerValue) / (SENSE_RESISTOR_VALUE * 4096));
            //QLSB = 65536*qLSB;
            ushort registerValue = (ushort)(chargeMAH / QLSB);
            Console.WriteLine("db{0}, r{1}, qLSB{2}", chargeMAH, registerValue, QLSB);
            if (registerValue > 0xffff)
            {
                registerValue = 0xffff;
            }
            if (registerValue < 0)
            {
                registerValue = 0;
            }

            ////shutdown 
            //SetControlRegister(SHUTDOWN_MODE, _prescalerValue, ALERT_MODE);
            //write
            byte[] data = BitConverter.GetBytes(registerValue).Reverse().ToArray();
            this.WriteRegisterByte(LTC2943_Constants.ACC_CHARGE_MSB, data);
            Console.WriteLine("w{0}, w{1}", data[0], data[1]);
        }
        #endregion

        #region Methods
        private void SetControlRegister(int _mode, int _prescalerCode, int _alccConfiguration)
        {
            byte configuration = 0x00;

            configuration = (byte)(_mode | _prescalerCode | _alccConfiguration);

            this.WriteRegisterByte(LTC2943_Constants.CONTROL, configuration);
            Console.WriteLine(string.Format("Setting Control register: {0}", configuration));

            switch (_prescalerCode)
            {
                case 0x00:
                    this._prescalerValue = 1;
                    break;
                case 0x08:
                    this._prescalerValue = 4;
                    break;
                case 0x10:
                    this._prescalerValue = 16;
                    break;
                case 0x18:
                    this._prescalerValue = 64;
                    break;
                case 0x20:
                    this._prescalerValue = 256;
                    break;
                case 0x28:
                    this._prescalerValue = 1024;
                    break;
                case 0x30:
                    this._prescalerValue = 4096;
                    break;
                case 0x31:
                    this._prescalerValue = 4096;
                    break;
                default:
                    this._prescalerValue = 4096;
                    break;
            }
        }

        private ElectricPotentialDc ReadVoltage()
        {
            float f_voltage = 0;
            int voltageADCCode;

            byte[] result = this.ReadRegisterByte(LTC2943_Constants.VOLTAGE_MSB, 2, true);

            if (result == null)
            {
                Console.WriteLine("Read from voltage register failed.");
                return ElectricPotentialDc.MinValue;
            }

            voltageADCCode = result[0] * 256 + result[1];

            f_voltage = CalculateVoltageFromADCCode(voltageADCCode);
            Console.WriteLine("Read from voltage register: {0}-{1}", result[0], result[1]);
            return ElectricPotentialDc.From(f_voltage, UnitsNet.Units.ElectricPotentialDcUnit.VoltDc);
        }

        private ElectricCurrent ReadCurrent()
        {
            float f_current = 0;
            int currentADCCode;

            byte[] result = this.ReadRegisterByte(LTC2943_Constants.CURRENT_MSB, 2, true);

            if (result == null)
            {
                Console.WriteLine("Read from current register failed.");
                return ElectricCurrent.MinValue;
            }

            currentADCCode = result[0] * 256 + result[1];
            Console.WriteLine(string.Format("Read from current register: {0}-{1}", result[0], result[1]));
            f_current = CalculateCurrentFromADCCode(currentADCCode);

            return ElectricCurrent.From(f_current, UnitsNet.Units.ElectricCurrentUnit.Ampere);
        }

        private Temperature ReadTemperature()
        {
            float f_temperature = 0;
            int temperatureADCCode;

            byte[] result = this.ReadRegisterByte(LTC2943_Constants.TEMPERATURE_MSB, 2, true);

            if (result == null)
            {
                Console.WriteLine("Read from temperature register failed.");
                return Temperature.MinValue;
            }

            temperatureADCCode = result[0] * 256 + result[1];
            Console.WriteLine(string.Format("Read from temperature register: {0}-{1}", result[0], result[1]));
            f_temperature = CalculateTemperatureFromADCCode(temperatureADCCode);

            return Temperature.From(f_temperature, UnitsNet.Units.TemperatureUnit.Kelvin);
        }

        private ElectricCharge ReadAccumulatedCharge()
        {
            float f_accumulatedCharge = 0.0f;
            int accumulatedChargeADCCode;

            byte[] result = this.ReadRegisterByte(LTC2943_Constants.ACC_CHARGE_MSB, 2, true);

            if (result == null)
            {
                Console.WriteLine("Read from accumulated register failed.");
                return ElectricCharge.Zero;
            }

            accumulatedChargeADCCode = result[0] * 256 + result[1];
            Console.WriteLine(string.Format("Read from accumulated register: {0}-{1}", result[0], result[1]));
            f_accumulatedCharge = CalculateAccumulatedChargeFromADCCode(accumulatedChargeADCCode);

            return ElectricCharge.FromCoulombs(f_accumulatedCharge * 3.6f);
        }

        private byte ReadStatusRegister()
        {
            byte result = this.ReadRegisterByte(LTC2943_Constants.STATUS, true);

            return result;
        }

        private void ReadALL()
        {

            byte[] result = this.ReadRegisterByte(LTC2943_Constants.STATUS, 22, true);
            //if (result != null)
            //    CommonHelper.Logger.ConditionalDebug(string.Format("Read ALL: {0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}-{8}-{9}-{10}-{11}",
            //        result[0], result[1], result[2], result[3], result[4], result[5],
            //        result[6], result[7], result[8], result[9], result[10], result[11]));

        }

        private void SetVoltageThresholdLow(float minVoltage)
        {
            short minVoltageCode = (short)(minVoltage * 65535 / FULLSCALE_VOLTAGE);

            byte[] data = BitConverter.GetBytes(minVoltageCode).Reverse().ToArray();

            this.WriteRegisterByte(LTC2943_Constants.VOLTAGE_THRESH_LOW_MSB, data);
        }

        private void SetVoltageThresholdHigh(float _maxVoltage)
        {
            ushort maxVoltageCode = (ushort)(_maxVoltage * 65535 / FULLSCALE_VOLTAGE);

            byte[] data = BitConverter.GetBytes(maxVoltageCode).Reverse().ToArray();

            this.WriteRegisterByte(LTC2943_Constants.VOLTAGE_THRESH_HIGH_MSB, data);
        }

        private void SetChargeThresholdLow(short minCharge)
        {
            byte[] data = BitConverter.GetBytes(minCharge).Reverse().ToArray();

            this.WriteRegisterByte(LTC2943_Constants.CHARGE_THRESH_LOW_MSB, data);
        }

        private void SetChargeThresholdHigh(short maxCharge)
        {
            byte[] data = BitConverter.GetBytes(maxCharge).Reverse().ToArray();

            this.WriteRegisterByte(LTC2943_Constants.CHARGE_THRESH_HIGH_MSB, data);
        }

        private void SetCurrentThresholdLow(int minCurrent)
        {
            short minCurrentCode = (short)(SENSE_RESISTOR_VALUE * minCurrent * 0x7FFF / FULLSCALE_CURRENT + 0x7FFF);

            byte[] data = BitConverter.GetBytes(minCurrentCode).Reverse().ToArray();

            this.WriteRegisterByte(LTC2943_Constants.CURRENT_THRESH_LOW_MSB, data);
        }

        private void SetCurrentThresholdHigh(int maxCurrent)
        {
            short maxCurrentCode = (short)(SENSE_RESISTOR_VALUE * maxCurrent * 0x7FFF / FULLSCALE_CURRENT + 0x7FFF);

            byte[] data = BitConverter.GetBytes(maxCurrentCode).Reverse().ToArray();

            this.WriteRegisterByte(LTC2943_Constants.CURRENT_THRESH_HIGH_MSB, data);
        }

        private void SetTemperatureThresholdLow(int minTemp)
        {
            short minTemperature = (short)((minTemp + 273.15f) * (0xFFFF) / FULLSCALE_TEMPERATURE);

            byte[] data = BitConverter.GetBytes(minTemperature).Reverse().ToArray();

            this.WriteRegisterByte(LTC2943_Constants.TEMPERATURE_THRESH_LOW, data);
        }

        private void SetTemperatureThresholdHigh(int maxTemp)
        {
            short maxTemperature = (short)((maxTemp + 273.15f) * (0xFFFF) / FULLSCALE_TEMPERATURE);

            byte[] data = BitConverter.GetBytes(maxTemperature).Reverse().ToArray();

            this.WriteRegisterByte(LTC2943_Constants.TEMPERATURE_THRESH_HIGH, data);

        }

        public void SetChargingComplete()
        {
            // First read the control register as we have to power
            // down the analogue parts when setting the charge registers
            // If the power down bit is not set, set it
            SetControlRegister(AUTOMATIC_MODE, _prescalerSelected, ALERT_MODE | SHUTDOWN_MODE);
            Thread.Sleep(500);
            // Set the charge counter to max
            this.WriteRegisterByte(LTC2943_Constants.ACC_CHARGE_MSB, 0xFFFF);

            // If the ADC mode, in bits 6 and 7, is not zero then
            // we must switch on the analogue power again.
            SetControlRegister(AUTOMATIC_MODE, _prescalerSelected, ALERT_MODE);
        }

        private void ResetAlertStatus()
        {
            this.WriteRegisterByte(ALERT_RESPONSE_ADDRESS);
        }

        #endregion

        #region Loop
        /// <summary>
        /// Read battery status continously 
        /// </summary>
        public override void Start()
        {
            //shutdown
            SetControlRegister(SHUTDOWN_MODE, _prescalerSelected, ALERT_MODE);

            //configure params
            QLSB = CHARGE_LEAST * (50 / SENSE_RESISTOR_VALUE) * (this._prescalerValue / 4096d);//maH
            QBATMAX = QLSB * 65536d; //mAH
            double QbatUsed = BATTERY_CAPACITY_MAH / QLSB; //number
            double LastCharge = QBATMAX - 0.5 * BATTERY_CAPACITY_MAH; // last charge default mAh
            _batteryUnderChargeStatus = GpioGlobalConnection.Read(_batteryUnderChargeGpio);

            // Set the charge counter to db value
            if (File.Exists("lastbatterychargestatus.txt"))
            {
                string bcs = File.ReadAllText("lastbatterychargestatus.txt");
                LastCharge = Convert.ToDouble(bcs);
            }

            SetChargeMAHToRegister(LastCharge);

            SetControlRegister(AUTOMATIC_MODE, _prescalerSelected, ALERT_MODE);
            SetVoltageThresholdHigh(8.3f);
            SetVoltageThresholdLow(6.5f);

            base.Start();
            Task.Factory.StartNew(() => ReadContinuosly(_cancellationTokenSource.Token), _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default).ConfigureAwait(false);
            Console.WriteLine("LTC2943: Started");
        }

        public override void Stop()
        {
            //GpioGlobalConnection.Remove(_batteryUnderChargeGpio);
            base.Stop();
            System.Console.WriteLine("LTC2943: Stopped");
        }

        public void Close()
        {
            GpioGlobalConnection.Remove(_batteryUnderChargeGpio);
            GpioGlobalConnection.Stop();
            base.Stop();
        }

        public void Restart()
        {
            //this.Stop();
            //ResetAlertStatus();
            SetControlRegister(SLEEP_MODE, _prescalerValue, ALERT_MODE | SHUTDOWN_MODE);
            Thread.Sleep(500);
            // If the ADC mode, in bits 6 and 7, is not zero then
            // we must switch on the analogue power again.
            SetControlRegister(AUTOMATIC_MODE, _prescalerValue, ALERT_MODE);
            //this.Start();
        }

        private void SaveLastCharge(Double c)
        {
            File.WriteAllText("lastbatterychargestatus.txt", Convert.ToString(c / 3.6));// Convert.ToString((ushort)(c.Coulombs / 3.6f)));
        }

        private async Task ReadContinuosly(CancellationToken cancellationToken)
        { 
            GaugeEventArgs eventArgs = new GaugeEventArgs();
            ChargeStatusType previousStatus = ChargeStatusType.NotCharging;
            bool chargeCompleted = false;
            while (!cancellationToken.IsCancellationRequested)
            {
                try 
                {
                    //read values from module
                    ElectricCurrent a = ReadCurrent();
                    ElectricPotentialDc v = ReadVoltage();
                    Temperature t = ReadTemperature();
                    byte s = ReadStatusRegister();
                    ElectricCharge c = ReadAccumulatedCharge();

                    //set status
                    if (_batteryUnderChargeStatus)
                    {
                        if (a.Milliamperes < 0)
                            _chargeStatus = ChargeStatusType.Discharging;
                        else if (a.Milliamperes > 0 && a.Milliamperes <= 1 && v.Value > BATTERY_MAX_VOLTAGE)
                            _chargeStatus = ChargeStatusType.NotCharging;
                        else if (a.Milliamperes > 0 && a.Milliamperes <= 1 && v.Value >= BATTERY_NOMINAL_VOLTAGE)
                            _chargeStatus = ChargeStatusType.Completed;
                    }
                    else
                    {
                        if (v.VoltsDc >= BATTERY_NOMINAL_VOLTAGE && a.Milliamperes > 10)
                        {
                            _chargeStatus = ChargeStatusType.Charging;
                        }
                    }

                    //calculate remaining charge
                    double remainingCharge = BATTERY_CAPACITY_MAH + (c.Coulombs * COULOMBS_TO_MILLIAMPEREHOUR) - QBATMAX; //mAh                    
                    double remainingTime = Math.Abs(remainingCharge / a.Milliamperes);// hours
                    double maxTime = Math.Abs(BATTERY_CAPACITY_MAH / a.Milliamperes); //hours
                    double remainigPercentage = Math.Round(100 * remainingTime / maxTime, 2);

                    switch (_chargeStatus)
                    {
                        case ChargeStatusType.Charging:
                            remainingTime = maxTime - remainingTime;
                            chargeCompleted = false;
                            if ((s & 32) == 32 || eventArgs.Charge.Coulombs >= c.Coulombs) //during charging got overflow, set percentage to 99 and not save anything on db                        
                                remainigPercentage = 100;
                            break;
                        case ChargeStatusType.Discharging:
                            // if charge is too low, close app
                            chargeCompleted = false;
                            if (remainigPercentage <= 20)
                                Console.WriteLine("Close APP {0}", remainigPercentage);
                            if ((s & 32) == 32)  //during discharging got overflow, set percentage to 99 and not save anything on db                        
                                remainigPercentage = 0;
                            break;
                        case ChargeStatusType.Completed:
                            if (!chargeCompleted)
                            {
                                SetChargingComplete();
                                chargeCompleted = true;
                                SaveLastCharge(QBATMAX);
                            }
                            remainigPercentage = 100;
                            break;
                        case ChargeStatusType.NotCharging:
                            chargeCompleted = false;
                            break;
                    }

                    Console.WriteLine("{0} {1} {2} {3} {4} {5}", remainingCharge, remainingTime, maxTime, remainigPercentage, s, _chargeStatus);

                    //aggiorna e scatena eventi
                    if ((Math.Abs(remainigPercentage - eventArgs.ChargePercentage) >= 1) || previousStatus != _chargeStatus)
                    {
                        eventArgs.Charge = c;
                        eventArgs.Current = a;
                        eventArgs.Temperature = t;
                        eventArgs.Voltage = v;
                        eventArgs.ChargePercentage = remainigPercentage > 100 ? 100 : remainigPercentage;
                        eventArgs.Status = s;
                        eventArgs.RemainingTime = TimeSpan.FromHours(remainingTime);
                        eventArgs.LedOn = !_batteryUnderChargeStatus;
                        Console.WriteLine(string.Format("LTC: charge= {0}, status= {1}, %={2}, v={3}, a={4}, t={5}, cs={6}", c, s, remainigPercentage, v.Value, a.Milliamperes, t, _chargeStatus));
                        //save last charge to db. to be used at startup, next switch off/switch on
                        SaveLastCharge(c.Value);
                        previousStatus = _chargeStatus;

                        OnGaugeChanged?.Invoke(eventArgs);
                    }
                    await Task.Delay(10000, cancellationToken);
                }
                catch(Exception e)
                {
                    Console.WriteLine("Error: {0}, {1}", e.Message, e.InnerException.Message);
                }
            }

            Console.WriteLine("Stopping LTC2943 reading task.");
        }

        private void GlobalConnectionPin_PinStatusChanged(object sender, PinStatusEventArgs e)
        {
            if (e.Configuration.Pin == _batteryUnderChargeGpio)
            {               
                Console.WriteLine("Battery charge changed in: {0}", e.Enabled ? "Full, Discharging or Removed" : "Charging");
                _batteryUnderChargeStatus = e.Enabled;

                OnUnderCharge?.Invoke(new UnderChargeEventArgs { Charging = e.Enabled });
            }
        }
        #endregion
    }
}
