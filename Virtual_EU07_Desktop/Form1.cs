using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;

namespace Virtual_EU07_Desktop
{
    public partial class Form1 : Form
    {
        private TcpClient tcp_client;
        private readonly TrainConfiguration train_configuration;
        private bool handle_connection;
        private Thread connectionThread;

        public Form1()
        {
            InitializeComponent();

            tcp_client = new TcpClient();
            tcp_client.ReceiveTimeout = 5;

            train_configuration = new TrainConfiguration();
            handle_connection = false;

            connectionThread = new Thread(new ThreadStart(ConnectThread));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox_wycieraczki.SelectedIndex = 0;
            comboBox_rodzaj_hamulca.SelectedIndex = 1;
            comboBox_nacisk.SelectedIndex = 0;
            comboBox_zakres_pradu.SelectedIndex = 0;
            trackBar_nastawnik_jazdy.Value = 0;
            textBox_nastawnik_jazdy.Text = "0";
            trackBar_nastawnik_bocznikowania.Value = 0;
            textBox_nastawnik_bocznikowania.Text = "0";
            textBox_hamulec_glowny.Text = "0";
            textBox_hamulec_pomocniczy.Text = "0";

            textBox_log.AppendText("Wirtualny Pulpit EU07");
            textBox_log.AppendText(Environment.NewLine);
            textBox_log.AppendText("Wersja 1.0.0");
            textBox_log.AppendText(Environment.NewLine);
        }

        private void trackBar_nastawnik_jazdy_Scroll(object sender, EventArgs e)
        {
            textBox_nastawnik_jazdy.Text = trackBar_nastawnik_jazdy.Value.ToString();
        }

        private void trackBar_nastawnik_bocznikowania_Scroll(object sender, EventArgs e)
        {
            textBox_nastawnik_bocznikowania.Text = trackBar_nastawnik_bocznikowania.Value.ToString();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Add:
                    {
                        if (trackBar_nastawnik_jazdy.Value < 43)
                        {
                            trackBar_nastawnik_jazdy.Value += 1;
                            textBox_nastawnik_jazdy.Text = trackBar_nastawnik_jazdy.Value.ToString();
                        }
                        break;
                    }
                case Keys.Subtract:
                    {
                        if (trackBar_nastawnik_jazdy.Value > 0)
                        {
                            trackBar_nastawnik_jazdy.Value -= 1;
                            textBox_nastawnik_jazdy.Text = trackBar_nastawnik_jazdy.Value.ToString();
                        }
                        break;
                    }
                case Keys.Divide:
                    {
                        if (trackBar_nastawnik_bocznikowania.Value < 6)
                        {
                            trackBar_nastawnik_bocznikowania.Value += 1;
                            textBox_nastawnik_bocznikowania.Text = trackBar_nastawnik_bocznikowania.Value.ToString();
                        }
                        break;
                    }
                case Keys.Multiply:
                    {
                        if (trackBar_nastawnik_bocznikowania.Value > 0)
                        {
                            trackBar_nastawnik_bocznikowania.Value -= 1;
                            textBox_nastawnik_bocznikowania.Text = trackBar_nastawnik_bocznikowania.Value.ToString();
                        }
                        break;
                    }
                case Keys.Space:
                    {
                        this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_BUTTON_SHP_ALERTER_RESET, 1);
                        break;
                    }
                
                default:
                    {
                        break;
                    }
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                    {
                        this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_BUTTON_SHP_ALERTER_RESET, 0);
                        break;
                    };
                default:
                    {
                        break;
                    }
            }
        }

        private void UpdateControls()
        {
            light_pns.BackColor =
                (this.train_configuration.GetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_INDICATOR_COMPRESSOR_OVERLOAD) == 1 ? Color.Maroon : Color.RosyBrown);
            light_pnw.BackColor =
                (this.train_configuration.GetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_INDICATOR_VENTILATOR_OVERLOAD) == 1 ? Color.Maroon : Color.RosyBrown);
            light_wylacznik_szybki.BackColor =
                (this.train_configuration.GetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_INDICATOR_LINE_BREAKER) == 1 ? Color.DarkGreen : Color.DarkSeaGreen);
            light_pnst.BackColor =
                (this.train_configuration.GetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_INDICATOR_TRACTION_ENGINE_OVERLOAD) == 1 ? Color.Maroon : Color.RosyBrown);
            light_przek_roznicowy.BackColor =
                (this.train_configuration.GetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_INDICATOR_MAIN_CIRCUIT_DIFFERENTIAL) == 1 ? Color.Maroon : Color.RosyBrown);
            light_pnpiop.BackColor =
                (this.train_configuration.GetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_INDICATOR_CONVERTER_OVERLOAD) == 1 ? Color.Maroon : Color.RosyBrown);
            light_styczniki_liniowe.BackColor =
                (this.train_configuration.GetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_INDICATOR_LINE_CONTACTORS) == 1 ? Color.RoyalBlue : Color.LightSteelBlue);
            light_poslizg.BackColor =
                (this.train_configuration.GetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_INDICATOR_WHEELSLIP) == 1 ? Color.WhiteSmoke : Color.Gray);
            light_wysoki_rozruch.BackColor =
                (this.train_configuration.GetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_INDICATOR_HIGH_START) == 1 ? Color.Maroon : Color.RosyBrown);
            light_jazda_na_oporach.BackColor =
                (this.train_configuration.GetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_INDICATOR_RESTISTOR_RIDE) == 1 ? Color.Gold : Color.Wheat);
            light_ogrzewanie_pociagu.BackColor =
                (this.train_configuration.GetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_INDICATOR_TRAIN_HEATING) == 1 ? Color.RoyalBlue : Color.LightSteelBlue);
            light_czuwak.BackColor =
                (this.train_configuration.GetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_INDICATOR_ALERTER) == 1 ? Color.Maroon : Color.RosyBrown);
            light_shp.BackColor =
                (this.train_configuration.GetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_INDICATOR_SHP) == 1 ? Color.Maroon : Color.RosyBrown);

            textBox_predkosc.Text = this.train_configuration.GetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_HASLER_VELOCITY).ToString() + " km/h";
            textBox_amperomierz_nn.Text = this.train_configuration.GetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_AMMETER_LOW_VOLTAGE).ToString() + " A";
            textBox_woltomierz_nn.Text = this.train_configuration.GetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_VOLTMETER_LOW_VOLTAGE).ToString() + " V";
            textBox_amperomierz_wn1.Text = this.train_configuration.GetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_AMMETER_HIGH_VOLTAGE1).ToString() + " A";
            textBox_amperomierz_wn2.Text = this.train_configuration.GetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_AMMETER_HIGH_VOLTAGE2).ToString() + " A";
            textBox_woltomierz_wn.Text = this.train_configuration.GetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_VOLTMETER_HIGH_VOLTAGE).ToString() + " V";
            textBox_cylinder_hamulcowy.Text = this.train_configuration.GetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_BREAK_PRESSURE).ToString() + " kPa";
            textBox_przewod_glowny.Text = this.train_configuration.GetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_PIPE_PRESSURE).ToString() + "  kPa";
            textBox_zbiornik_glowny.Text = this.train_configuration.GetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_TANK_PRESSURE).ToString() + " kPa"; ;
        }

        private void updateControllsOnConnect()
        {
            button_polacz.Enabled = true;
            button_polacz.BackColor = Color.Crimson;
            button_polacz.Text = "roz³¹cz";

            textBox_status.Text = "po³¹czony";

            textBox_log.AppendText("Po³¹czeno z " + textBox_IP.Text + ":" + textBox_port.Text);
            textBox_log.AppendText(Environment.NewLine);
        }

        private void updateControllsOnException(Exception e)
        {
            textBox_log.AppendText("B³¹d po³¹czenia : " + e.Message);
            textBox_log.AppendText(Environment.NewLine);

            button_polacz.Enabled = true;
            textBox_IP.Enabled = true;
            textBox_port.Enabled = true;
        }

        private int NetworkRead(ref NetworkStream stream, byte[] buffer, int size)
        {
            int total_read = 0;

            try
            {
                while (total_read < size)
                {
                    total_read += stream.Read(buffer, total_read, size - total_read);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("Exception on reading network data ({0} bytes) : {1}", size, e.Message));
            }

            return total_read;
        }
        private void NetworkWrite(ref NetworkStream stream, byte[] buffer, int size)
        {
            try
            {
                stream.Write(buffer, 0, size);
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("Exception on writing network data ({0} bytes) : {1}", size, e.Message));
            }
        }
        private void SendRequestData(ref NetworkStream stream)
        {
            DataMessage data_message = new DataMessage(DataMessage.MessageType.MESSAGE_TYPE_RESPONSE_DATA);

            for (TrainConfiguration.TrainConfigurationIds id = TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_BUTTON_RESERVE;
                id < TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_MAX; id++)
            {
                if (this.train_configuration.IsUpdated(id))
                    data_message.AddMessageItem(new DataMessage.MessageItem((UInt32)id, this.train_configuration.GetConfigValue(id)));
            }

            NetworkWrite(ref stream, data_message.getRawData(), data_message.getRawDataSize());

            this.train_configuration.CleanUpdates();
        }

        private void HandleMessageData(ref NetworkStream stream, ref DataMessage.MessageHeader header)
        {
            /* TODO */
            if (header.GetNumberOfItems() > 0)
            {
                int bytes_to_read = header.GetNumberOfItems() * DataMessage.MessageItem.GetMessageItemSize();
                byte[] buffer = new byte[bytes_to_read];

                if (NetworkRead(ref stream, buffer, bytes_to_read) == bytes_to_read)
                {
                    for (int i = 0; i < header.GetNumberOfItems(); i++)
                    {
                        DataMessage.MessageItem item = new DataMessage.MessageItem(buffer.Skip(i * DataMessage.MessageItem.GetMessageItemSize()).ToArray());
                        this.train_configuration.SetConfigValue((TrainConfiguration.TrainConfigurationIds)item.GetId(), item.GetValue());
                    }

                    Invoke(new Action(UpdateControls));
                }
            }
        }

        private void SendConfirmationData(ref NetworkStream stream)
        {
            DataMessage data_message = new DataMessage(DataMessage.MessageType.MESSAGE_TYPE_CONFIRM_DATA);
            NetworkWrite(ref stream, data_message.getRawData(), data_message.getRawDataSize());
        }

        private void ReceiveData(ref NetworkStream stream)
        {
            try
            {
                byte[] header_bytes = new byte[DataMessage.MessageHeader.GetMessageHeaderSize()];

                if (NetworkRead(ref stream, header_bytes, DataMessage.MessageHeader.GetMessageHeaderSize()) ==
                    DataMessage.MessageHeader.GetMessageHeaderSize())
                {
                    DataMessage.MessageHeader header = new DataMessage.MessageHeader(header_bytes);

                    if (header.GetMessageType() == DataMessage.MessageType.MESSAGE_TYPE_REQUEST_DATA)
                    {
                        SendRequestData(ref stream);
                    }
                    else if (header.GetMessageType() == DataMessage.MessageType.MESSAGE_TYPE_SEND_DATA)
                    {
                        HandleMessageData(ref stream, ref header);
                        SendConfirmationData(ref stream);
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(String.Format("Exception in ReceiveData : {0}", e.Message));
            }
            catch (Exception e)
            {
                Console.WriteLine(String.Format("Exception in ReceiveData : {0}", e.Message));
            }
        }

        private void ConnectThread()
        {
            try
            {
                string ipAddress = textBox_IP.Text;
                Int32 ipPort = Int32.Parse(textBox_port.Text);

                tcp_client.Connect(ipAddress, ipPort);
                NetworkStream stream = tcp_client.GetStream();
                stream.ReadTimeout = 1000;

                Invoke(new Action(updateControllsOnConnect));

                while (handle_connection)
                {
                    ReceiveData(ref stream);

                }
            }
            catch (Exception)
            {
                MessageBox.Show(String.Format("Nie mo¿na ustanowiæ po³¹czenia z {0}:{1}", textBox_IP.Text, textBox_port.Text));
                Invoke(new Action(EnableConnectControls));
            }
        }

        private void EnableConnectControls()
        {
            button_polacz.Text = "po³¹cz";
            button_polacz.Enabled = true;
            button_polacz.BackColor = Color.Teal;
            textBox_status.Text = "nie po³¹czony";
            textBox_IP.Enabled = true;
            textBox_port.Enabled = true;
        }

        private void DisableConnectControls()
        {
            button_polacz.Enabled = false;

            textBox_IP.Enabled = false;
            textBox_port.Enabled = false;
        }

        private void button_polacz_Click(object sender, EventArgs e)
        {
            if (textBox_status.Text == "po³¹czony")
            {
                handle_connection = false;
                //this.connectionThread.Join();

                tcp_client.Close();

                textBox_log.AppendText("Roz³¹czeno z " + textBox_IP.Text + ":" + textBox_port.Text);
                textBox_log.AppendText(Environment.NewLine);

                EnableConnectControls();
            }
            else
            {
                handle_connection = true;
                tcp_client = new TcpClient();
                tcp_client.ReceiveTimeout = 5;

                connectionThread = new Thread(new ThreadStart(ConnectThread));
                connectionThread.Start();
            }
        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void button_rezerwa_MouseDown(object sender, MouseEventArgs e)
        {
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_BUTTON_RESERVE, 1);
        }

        private void button_rezerwa_MouseUp(object sender, MouseEventArgs e)
        {
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_BUTTON_RESERVE, 0);
        }

        private void button_opns_MouseDown(object sender, MouseEventArgs e)
        {
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_BUTTON_COMPRESSOR_OVERLOAD_UNLOCK, 1);
        }

        private void button_opns_MouseUp(object sender, MouseEventArgs e)
        {
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_BUTTON_COMPRESSOR_OVERLOAD_UNLOCK, 0);
        }

        private void button_wsw_MouseDown(object sender, MouseEventArgs e)
        {
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_BUTTON_LINE_BREAKER_DISABLE, 1);
        }

        private void button_wsw_MouseUp(object sender, MouseEventArgs e)
        {
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_BUTTON_LINE_BREAKER_DISABLE, 0);
        }

        private void button_wsz_MouseDown(object sender, MouseEventArgs e)
        {
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_BUTTON_LINE_BREAKER_ENABLE, 1);
        }

        private void button_wsz_MouseUp(object sender, MouseEventArgs e)
        {
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_BUTTON_LINE_BREAKER_ENABLE, 0);
        }

        private void button_opnst_MouseDown(object sender, MouseEventArgs e)
        {
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_BUTTON_TRACTION_ENGINE_OVERLOAD_UNLOCK, 1);
        }

        private void button_opnst_MouseUp(object sender, MouseEventArgs e)
        {
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_BUTTON_TRACTION_ENGINE_OVERLOAD_UNLOCK, 0);
        }

        private void button_opnpiop_MouseDown(object sender, MouseEventArgs e)
        {
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_BUTTON_CONVERTER_OVERLOAD_UNLOCK, 1);
        }

        private void button_opnpiop_MouseUp(object sender, MouseEventArgs e)
        {
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_BUTTON_CONVERTER_OVERLOAD_UNLOCK, 0);
        }

        private void button_sl_wyl_MouseDown(object sender, MouseEventArgs e)
        {
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_BUTTON_LINE_CONTACTORS_DISABLE, 1);
        }

        private void button_sl_wyl_MouseUp(object sender, MouseEventArgs e)
        {
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_BUTTON_LINE_CONTACTORS_DISABLE, 0);
        }

        private void button_poslizg_MouseDown(object sender, MouseEventArgs e)
        {
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_BUTTON_WHEELSLIP_COUNTER_ACTION, 1);
        }

        private void button_poslizg_MouseUp(object sender, MouseEventArgs e)
        {
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_BUTTON_WHEELSLIP_COUNTER_ACTION, 0);
        }

        private void button_luzowanie_hamulca_MouseDown(object sender, MouseEventArgs e)
        {
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_BUTTON_RELAXER, 1);
        }

        private void button_luzowanie_hamulca_MouseUp(object sender, MouseEventArgs e)
        {
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_BUTTON_RELAXER, 0);
        }

        private void button_czuwak_MouseDown(object sender, MouseEventArgs e)
        {
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_BUTTON_SHP_ALERTER_RESET, 1);
        }

        private void button_czuwak_MouseUp(object sender, MouseEventArgs e)
        {
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_BUTTON_SHP_ALERTER_RESET, 0);
        }

        private void button_stn_MouseDown(object sender, MouseEventArgs e)
        {
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_BUTTON_SIREN_LOW, 1);
        }

        private void button_stn_MouseUp(object sender, MouseEventArgs e)
        {
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_BUTTON_SIREN_LOW, 0);
        }

        private void button_stw_MouseDown(object sender, MouseEventArgs e)
        {
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_BUTTON_SIREN_HIGH, 1);
        }

        private void button_stw_MouseUp(object sender, MouseEventArgs e)
        {
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_BUTTON_SIREN_HIGH, 0);
        }

        private void checkBox_aktywacja_kabiny_CheckedChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(checkBox_aktywacja_kabiny.Checked ? 1 : 0);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_SWITCH_CABIN_ACTIVATION, value);
        }

        private void checkBox_ogrzewanie_nog_CheckedChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(checkBox_ogrzewanie_nog.Checked ? 1 : 0);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_SWITCH_LEGS_HEATING, value);
        }

        private void checkBox_przyciemnienie_kabiny_CheckedChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(checkBox_przyciemnienie_kabiny.Checked ? 1 : 0);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_SWITCH_CABIN_LIGHT_DIMM, value);
        }

        private void checkBox_przyciemnienie_opp_CheckedChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(checkBox_przyciemnienie_opp.Checked ? 1 : 0);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_SWITCH_MEASURE_INSTRUMENT_LIGHT_DIMM, value);
        }

        private void checkBox_przyciemnienie_ls1_CheckedChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(checkBox_przyciemnienie_ls1.Checked ? 1 : 0);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_SWITCH_SIGNAL_LAMP1_DIMM, value);
        }

        private void checkBox_przyciemnienie_ls2_CheckedChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(checkBox_przyciemnienie_ls2.Checked ? 1 : 0);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_SWITCH_SIGNAL_LAMP2_DIMM, value);
        }

        private void checkBox_sygnal_czerw_lewy_CheckedChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(checkBox_sygnal_czerw_lewy.Checked ? 1 : 0);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_SWITCH_SIGNAL_LAMP_RED_LEFT, value);
        }

        private void checkBox_rezerwa_CheckedChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(checkBox_rezerwa.Checked ? 1 : 0);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_SWITCH_RESERVE, value);
        }

        private void checkBox_sygnal_czerw_prawy_CheckedChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(checkBox_sygnal_czerw_prawy.Checked ? 1 : 0);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_SWITCH_SIGNAL_LAMP_RED_RIGHT, value);
        }

        private void checkBox_oswietlenie_ogolne_CheckedChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(checkBox_oswietlenie_ogolne.Checked ? 1 : 0);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_SWITCH_MAIN_LIGHT, value);
        }

        private void checkBox_oswietlenie_kabiny_CheckedChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(checkBox_oswietlenie_kabiny.Checked ? 1 : 0);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_SWITCH_CABIN_LIGHT, value);
        }

        private void checkBox_oswietlenie_pp_CheckedChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(checkBox_oswietlenie_pp.Checked ? 1 : 0);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_SWITCH_MEASURE_INSTRUMENT_LIGHT, value);
        }

        private void checkBox_osiwetlenie_szafy_wn_CheckedChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(checkBox_osiwetlenie_szafy_wn.Checked ? 1 : 0);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_SWITCH_HIGH_VOLTAGE_BOX_LIGHT, value);
        }

        private void checkBox_reflektor_lewy_CheckedChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(checkBox_reflektor_lewy.Checked ? 1 : 0);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_SWITCH_SIGNAL_LIGHT_LEFT, value);
        }

        private void checkBox_reflektor_gorny_CheckedChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(checkBox_reflektor_gorny.Checked ? 1 : 0);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_SWITCH_SIGNAL_LIGHT_TOP, value);
        }

        private void checkBox_reflektor_prawy_CheckedChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(checkBox_reflektor_prawy.Checked ? 1 : 0);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_SWITCH_SIGNAL_LIGHT_RIGHT, value);
        }

        private void checkBox_bateria_CheckedChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(checkBox_bateria.Checked ? 1 : 0);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_SWITCH_BATTERY, value);
        }

        private void checkBox_przyciemnienie_shp_CheckedChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(checkBox_przyciemnienie_shp.Checked ? 1 : 0);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_SWITCH_SHP_INDICATOR_DIMM, value);
        }

        private void checkBox_pantograf_przod_CheckedChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(checkBox_pantograf_przod.Checked ? 1 : 0);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_SWITCH_PANTHOGRAPH_A, value);
        }

        private void checkBox_sprezarka_CheckedChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(checkBox_sprezarka.Checked ? 1 : 0);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_SWITCH_COMPRESSOR, value);
        }

        private void checkBox_przetwornica_CheckedChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(checkBox_przetwornica.Checked ? 1 : 0);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_SWITCH_CONVERTER, value);
        }

        private void checkBox_przyciemnienie_czuwaka_CheckedChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(checkBox_przyciemnienie_czuwaka.Checked ? 1 : 0);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_SWITCH_ALERTER_INDICATOR_DIMM, value);
        }

        private void checkBox_pantograf_tyl_CheckedChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(checkBox_pantograf_tyl.Checked ? 1 : 0);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_SWITCH_PANTHOGRAPH_B, value);
        }

        private void checkBox_ogrzewanie_pociagu_CheckedChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(checkBox_ogrzewanie_pociagu.Checked ? 1 : 0);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_SWITCH_TRAIN_HEATING, value);
        }

        private void comboBox_rodzaj_hamulca_SelectedValueChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(comboBox_rodzaj_hamulca.SelectedIndex);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_SWITCH_BREAK_MODE, value);
        }

        private void comboBox_nacisk_SelectedValueChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(comboBox_nacisk.SelectedIndex);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_SWITCH_WHEEL_PUSH_MODE, value);
        }

        private void comboBox_zakres_pradu_SelectedValueChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(comboBox_zakres_pradu.SelectedIndex);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_SWITCH_VOLTAGE_RANGE_MODE, value);
        }

        private void checkBox_przyciemnienie_refl1_CheckedChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(checkBox_przyciemnienie_refl1.Checked ? 1 : 0);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_SWITCH_SIGNAL_LIGHT1_DIMM, value);
        }

        private void checkBox_przyciemnienie_refl2_CheckedChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(checkBox_przyciemnienie_refl2.Checked ? 1 : 0);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_SWITCH_SIGNAL_LIGHT2_DIMM, value);
        }

        private void trackBar_nastawnik_kierunku_ValueChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(trackBar_nastawnik_kierunku.Value);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_CONTROLLER_TRAIN_DIRECTION, value);
        }

        private void trackBar_nastawnik_jazdy_ValueChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(trackBar_nastawnik_jazdy.Value);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_CONTROLLER_ADJUSTER_WHEEL_POSITION, value);
        }

        private void trackBar_nastawnik_bocznikowania_ValueChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(trackBar_nastawnik_bocznikowania.Value);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_CONTROLLER_SHUNT_POSITION, value);
        }

        private void trackBar_hamulec_glowny_ValueChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(trackBar_hamulec_glowny.Value);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_MAIN_BREAK_VALUE, value);

            textBox_hamulec_glowny.Text = trackBar_hamulec_glowny.Value.ToString();
        }

        private void trackBar_hamulec_pomocniczy_ValueChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(trackBar_hamulec_pomocniczy.Value);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_LOC_BREAK_VALUE, value);

            textBox_hamulec_pomocniczy.Text = trackBar_hamulec_pomocniczy.Value.ToString();
        }

        private void comboBox_wycieraczki_SelectedValueChanged(object sender, EventArgs e)
        {
            UInt32 value = (UInt32)(comboBox_wycieraczki.SelectedIndex);
            this.train_configuration.SetConfigValue(TrainConfiguration.TrainConfigurationIds.CONFIGURATION_ID_SWITCH_WIPERS_MODE, value);
        }

       
    }
}
