using Microsoft.Azure.Devices.Client;
using Microsoft.IoT.AdcMcp3008;
using System;
using System.Diagnostics;
using uPLibrary.Networking.M2Mqtt;
using Windows.Devices.Adc;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MainIoTApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            setup();
        }

        DispatcherTimer m_t;
        GpioPin m_blink;
        GpioPinValue m_blinkValue;
        BMP280 m_bmp280;
        AdcController m_adc;
        AdcChannel[] m_adcChannel;
        TCS34725 m_tcs;
        DeviceClient m_clt;
        MqttClient m_mqtt;

        private async void setup()
        {
            try {
                //0. IoTHub
                m_clt = DeviceClient.CreateFromConnectionString(TKConnectionString, TransportType.Http1);
                await m_clt.SendEventAsync(new Message(new byte[] { 1, 2, 3 }));
                //0. MQQT for IoT Hub
                m_mqtt = new MqttClient(TKConnectionMqtt, 8883,true,MqttSslProtocols.TLSv1_2);
                m_mqtt.Connect(TKMqttDeviceId, TKConnectionMqttUsername, TKConnectionMqttPassword);
                m_mqtt.Publish(TKMqttTopic, new byte[] { 64, 65, 66 });
                m_mqtt.Publish("ABC", new byte[] { 67, 68, 69 });

                //1. LED
                var gpio = GpioController.GetDefault();
                if (gpio != null)
                {
                    m_blink = gpio.OpenPin(26); //See board, connected to pin 19, 
                    m_blinkValue = GpioPinValue.High;
                    m_blink.Write(m_blinkValue);
                    m_blink.SetDriveMode(GpioPinDriveMode.Output);
                }
                //2. BMP280
                m_bmp280 = new BMP280();
                await m_bmp280.Initialize();
                //3. ADC
                m_adc = (await AdcController.GetControllersAsync(AdcMcp3008Provider.GetAdcProvider()))[0];
                m_adcChannel = new AdcChannel[m_adc.ChannelCount];
                for (int i = 0; i < m_adc.ChannelCount; i++)
                {
                    m_adcChannel[i] = m_adc.OpenChannel(i);
                }
                //4. TCS34725
                m_tcs = new TCS34725();
                await m_tcs.Initialize();



                m_t = new DispatcherTimer();
                m_t.Interval = TimeSpan.FromSeconds(1);
                m_t.Tick += M_t_Tick;
                m_t.Start();
            } catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }


        }

        private void M_t_Tick(object sender, object e)
        {
            //throw new NotImplementedException();
        }
    }
}
