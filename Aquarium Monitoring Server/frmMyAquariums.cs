using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Collections.Specialized;

namespace Aquarium_Monitoring_Server
{
    public partial class frmMyAquariums : Form
    {
        #region Constructor frmMyAquariums()
        public frmMyAquariums()
        {
            InitializeComponent();
            trkLight.Minimum = 0;
            trkLight.Maximum = 255;
            //if (lstAquariums.Items.Count != 0)
            //{
            //    lstAquariums.SelectedIndex = 0;
            //}
            //else
            //{
            //    lblAddAquarium.Focus();
            //}
            timerTemperature.Start();
            timerFeedWarning.Start();
            cmbTemperature.SelectedIndex = 0;
            cmbLightInterval.SelectedIndex = 0;
            cmbFoodInterval.SelectedIndex = 0;
            hour = minute = second = 0;
            lightStrength = trkLight.Value.ToString();
            lblLightStrength.Text = (100 * (Convert.ToInt32(lightStrength) + 1) / 11) + "%";
            temperatureScale = "Celsius";
            labelChart = "#VALºC";
            //
            feedStatus = "NotFed";
            lastTimeFed = DateTime.Now;
            lblLastTimeFed.Text = lastTimeFed.ToString();
            lightInterval = defaultLightInterval;
            txtLightInterval.Text = lightInterval.ToString();
            feedingInterval = defaultFeedingInterval;
            txtFoodInterval.Text = feedingInterval.ToString();
            heaterTemperature = defaultHeaterTemperature;
            txtHeaterTemperature.Text = heaterTemperature.ToString();
        }
        #endregion

        #region Global Variables
        string heaterStatus, feedStatus, lightStatus, post, url, lightStrength,temperatureScale, labelChart;
        double lightInterval, defaultLightInterval = 10, feedingInterval, defaultFeedingInterval = 24, heaterTemperature, heaterTemperatureFarenheit, defaultHeaterTemperature = 27;
        double temperatureNow;
        string ledStrength = "0", waterLevel = "0";
        DateTime lastTimeFed;
        int hour, minute, second;
        #endregion

        #region Button '+Aquarium'
        private void lblAddAquarium_MouseEnter(object sender, EventArgs e)
        {
            //lblAddAquarium.ForeColor = Color.Green;
            //this.Cursor = Cursors.Hand;
        }

        private void lblAddAquarium_MouseLeave(object sender, EventArgs e)
        {
            //lblAddAquarium.ForeColor = Color.Black;
            //this.Cursor = Cursors.Arrow;
        }
        #endregion

        #region Temperature

        #region Button heater
        private void btnHeater_Click(object sender, EventArgs e)
        {
            url = "https://ams-aquarium.herokuapp.com/my-aquariums";
            turnHeaterOnOff(heaterStatus);
        }

        #region Method Turn heater ON/OFF
        private void turnHeaterOnOff(string _heaterStatus)
        {
            if (_heaterStatus == "ON")
            {
                post = "heateroff";
                heaterStatus = "OFF"; // comando get receberá o novo status do heater após o post, e o messagebox será baseado nele.
            }
            else
            {
                post = "heateron";
                heaterStatus = "ON";
            }

            Post(post, post, "http://localhost:3030/manual-heater-controll");

            
        }
        #endregion

        #endregion

        #region Chart

        #region Timer
        private void timer1_Tick(object sender, EventArgs e)
        {
            //url = "http://localhost:3030/rotateste";

            temperatureNow = Convert.ToDouble(Get("http://localhost:3030/get-temp").Replace(".",",")); // Celsius
            string hourMinute = DateTime.Now.Hour.ToString() + ":" + DateTime.Now.Minute.ToString();

            /*if ((chTemperature.Series[0].Points.Count == 3) || (chTemperature.Series[1].Points.Count == 3))
            {
                chTemperature.Series[0].Points.RemoveAt(0);
                chTemperature.Series[1].Points.RemoveAt(0);
                chTemperature.Update();
            }*/
            lblCelsius.Text = temperatureNow.ToString() + "ºC";
            lblFarenheit.Text = Convert.ToDouble((temperatureNow * 1.8) + 32).ToString() + "ºF";
            chTemperature.Series[0].Points.AddXY(hourMinute, temperatureNow);
            chTemperature.Series[1].Points.AddXY(hourMinute, Convert.ToDouble((temperatureNow * 1.8) + 32));
        }
        #endregion

        #endregion

        #region Settings

        #region Troca de Índice na ComboBox Temperature
        private void cmbTemperature_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtHeaterTemperature.Clear();
            txtHeaterTemperature.Focus();
        }
        #endregion

        #region Button Update Heater Temperature
        private void btnHeaterTemperature_Click(object sender, EventArgs e)
        {
            updateHeaterTemperature();
        }

        #region Method Update Heater Temperature
        private void updateHeaterTemperature()
        {
            heaterTemperature = Math.Round(double.Parse(txtHeaterTemperature.Text),2);    
            switch (cmbTemperature.Text)
            {
                case "°Celsius":
                    temperatureScale = "Celsius";
                    break;
                case "°Farenheit":
                    temperatureScale = "Farenheit";
                    heaterTemperatureFarenheit = heaterTemperature;
                    heaterTemperature = Math.Round(((heaterTemperature - 32)/ 1.8),2);
                    lblHeaterTemperature.Text = heaterTemperatureFarenheit.ToString() + "°" + temperatureScale;
                    break;
            }
            post = heaterTemperature.ToString();
            url = "";

            Post("targettemp",post, "http://localhost:3030/manual-heater-controll");
        }
        #endregion

        #endregion

        #region Default

        #region Method Default Temperature
        private void defaultTemperatureSettings()
        {
            cmbTemperature.SelectedIndex = 0;
            cmbTemperature.Enabled = false;
            txtHeaterTemperature.Text = defaultHeaterTemperature.ToString();
            txtHeaterTemperature.Enabled = false;
        }
        #endregion

        #region Checkbutton Default Temperature
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (chkDefaultTemperature.Checked == true)
            {
                defaultTemperatureSettings();
            }
            else
            {
                txtHeaterTemperature.Enabled = true;
                txtHeaterTemperature.Text = "";
                cmbTemperature.Enabled = true;
                txtHeaterTemperature.Focus();
            }
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            string[] status = Get("http://localhost:3030/aquarium-status").Split();
        }
        #endregion

        #endregion

        #endregion

        #endregion

        #region Light

        #region Method Turn Light ON/OFF
        private void turnLightOnOff(string _lightStatus)
        {
            if (_lightStatus == "ON")
            {
                post = "ledoff";
                lightStatus = "OFF"; // comando get receberá o novo status do heater após o post, e o messagebox será baseado nele.
            }
            else
            {
                post = "ledon";
                lightStatus = "ON";
            }

            Post(post,post, url);
        }
        #endregion

        #region Button Light
        private void btnLight_Click(object sender, EventArgs e)
        {
            url = "http://localhost:3030/manual-led-controll";
            turnLightOnOff(lightStatus);
        }
        #endregion

        #region Light Interval

        #region CheckButton Default Light
        private void chkDefaultLight_CheckedChanged(object sender, EventArgs e)
        {
            if (chkDefaultLight.Checked == true)
            {
                cmbLightInterval.SelectedIndex = 0;
                cmbLightInterval.Enabled = false;
                lightInterval = 10;
                txtLightInterval.Text = lightInterval.ToString();
                txtLightInterval.Enabled = false;
                trkLight.Value = 0;
                trkLight.Enabled = false;
                Post("ledmode", "auto", "http://localhost:3030/manual-led-controll");
            }
            else
            {
                txtLightInterval.Enabled = true;
                txtLightInterval.Text = "";
                cmbLightInterval.Enabled = true;
                txtLightInterval.Focus();
                trkLight.Enabled = true;
            }
        }
        #endregion

        #region Troca de Índice na ComboBox Light Interval
        private void cmbLightInterval_SelectedIndexChanged(object sender, EventArgs e)
        {
            //switch (cmbLightInterval.Text)
            //{
            //    case "Hour":
            //        txtLightInterval.Mask = "00 hours,";
            //        break;
            //    case "Minute":
            //        txtLightInterval.Mask = "000 minutes,";
            //        break;
            //    case "Second":
            //        txtLightInterval.Mask = "00 seconds";
            //        break;
            //}
            txtLightInterval.Clear();
            txtLightInterval.Focus();
        }

        #region Button Update Light Interval
        private void btnLightInterval_Click(object sender, EventArgs e)
        {
            if(cmbLightInterval.Text == "Hour")
            {
                using (var client = new WebClient())
                {
                    var values = new NameValueCollection();
                    values["ledcicle"] = Convert.ToString(txtLightInterval.Text).Replace(",", ".");
                    var response = client.UploadValues("http://localhost:3030/manual-led-controll", values);
                    var responseString = Encoding.Default.GetString(response);
                }
            }
            else if (cmbLightInterval.Text == "Minute")
            {
                using (var client = new WebClient())
                {
                    var values = new NameValueCollection();
                    values["ledcicle"] = (Convert.ToDouble(txtLightInterval.Text) / 60).ToString().Replace(",", ".");
                    var response = client.UploadValues("http://localhost:3030/manual-led-controll", values);
                    var responseString = Encoding.Default.GetString(response);
                }
            }
            else if (cmbLightInterval.Text == "Second")
            {
                using (var client = new WebClient())
                {
                    var values = new NameValueCollection();
                    values["ledcicle"] = (Convert.ToDouble(txtLightInterval.Text) / 3600).ToString().Replace(",", ".");
                    var response = client.UploadValues("http://localhost:3030/manual-led-controll", values);
                    var responseString = Encoding.Default.GetString(response);
                }
            }
        }
        #endregion

        #endregion

        #region Timer Light Clock
        private void timerLightTime_Tick(object sender, EventArgs e)
        {
            //lblLightClock.Text = hour.ToString("00") + ":" + minute.ToString("00") + ":" + second.ToString("00");
            //if (second < 60)
            //{
            //    second++;
            //}
            //else
            //{
            //    if (minute < 60)
            //    {
            //        second = 0;
            //        minute++;
            //    }
            //    else
            //    {
            //        hour++;
            //        second = 0;
            //        minute = 0;
            //    }
            //}
        }
        #endregion

        #region Light Strength Bar
        private void trkLight_ValueChanged(object sender, EventArgs e)
        {
            lightStrength = trkLight.Value.ToString();
            lblLightStrength.Text = (Convert.ToInt32(lightStrength) * 100 / 255).ToString() + "%";
            using (var client = new WebClient())
            {
                var values = new NameValueCollection();
                values["ledstrh"] = lightStrength;
                var response = client.UploadValues("http://localhost:3030/manual-led-controll", values);
                var responseString = Encoding.Default.GetString(response);
            }
        }
        #endregion

        #endregion

        #endregion

        #region Food

        #region Timer Feed Warning
        private void timerFoodWarning_Tick(object sender, EventArgs e)
        {
            if (lblLastTimeFed.Text == "None")
            {
                #region Change Colors
                if (lblFeedWarning.ForeColor == Color.Red)
                {
                    lblFeedWarning.ForeColor = Color.Blue;
                    btnFeed.ForeColor = Color.Blue;
                    lblFoodStatus.ForeColor = Color.Blue;
                }
                else if (lblFeedWarning.ForeColor == Color.Blue)
                {
                    lblFeedWarning.ForeColor = Color.Red;
                    btnFeed.ForeColor = Color.Red;
                    lblFoodStatus.ForeColor = Color.Red;
                }
                #endregion
            }
        }
        
        #endregion

        #region Method feed
        private void feed(string _feedStatus)
        {
            post = "feed";
            //Post(post, url);
            feedStatus = "Fed";
            MessageBox.Show("Feeding successful!", "Feeding Pet", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            lblLastTimeFed.Text = lastTimeFed.ToString();
            lblFoodStatus.Text = "Fed";
            lblFoodStatus.ForeColor = Color.Green;
            lblFeedWarning.ForeColor = Color.Green;
            lblFeedWarning.Text = "Your fish is fed!";
            btnFeed.ForeColor = Color.Green;
        }
        #endregion

        #region Button feed
        private void btnFeed_Click(object sender, EventArgs e)
        {
            if (lblLastTimeFed.Text != "None")
            {
                if (MessageBox.Show("Feeding Time not reached yet. Do you want to feed it anyway?",
                    "Feeding Fish", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    Post("manualfeed", "manualfeed", "http://localhost:3030/manual-food-controll");
                }
            }
            else
            {
                Post("manualfeed", "manualfeed", "http://localhost:3030/manual-food-controll");
            }
        }
        #endregion

        #region Feeding Interval

        #region Checkbutton Default
        private void chkDefaultFood_CheckedChanged(object sender, EventArgs e)
        {
            if (chkDefaultFood.Checked == true)
            {
                cmbFoodInterval.SelectedIndex = 0;
                cmbFoodInterval.Enabled = false;
                feedingInterval = 24;
                txtFoodInterval.Text = lightInterval.ToString();
                txtFoodInterval.Enabled = false;
            }
            else
            {
                txtFoodInterval.Enabled = true;
                txtFoodInterval.Text = "";
                cmbFoodInterval.Enabled = true;
                txtFoodInterval.Focus();
            }

        }

        #region Troca de índice na combobox Food Interval
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //switch (cmbFoodInterval.Text)
            //{
            //    case "Hour":
            //        txtFoodInterval.Mask = "00 hours,";
            //        break;
            //    case "Minute":
            //        txtFoodInterval.Mask = "000 minutes,";
            //        break;
            //}
            txtFoodInterval.Clear();
            txtFoodInterval.Focus();
        }
        #endregion

        #endregion

        #region Button Update Feeding Interval
        private void btnFoodInterval_Click(object sender, EventArgs e)
        {
            if (cmbFoodInterval.Text == "Hour")
            {
                using (var client = new WebClient())
                {
                    var values = new NameValueCollection();
                    values["targetfoodtime"] = Convert.ToString(txtFoodInterval.Text).Replace(",", ".");
                    var response = client.UploadValues("http://localhost:3030/manual-food-controll", values);
                    var responseString = Encoding.Default.GetString(response);
                }
            }
            else if (cmbFoodInterval.Text == "Minute")
            {
                using (var client = new WebClient())
                {
                    var values = new NameValueCollection();
                    values["targetfoodtime"] = (Convert.ToDouble(txtFoodInterval.Text) / 60).ToString().Replace(",", ".");
                    var response = client.UploadValues("http://localhost:3030/manual-food-controll", values);
                    var responseString = Encoding.Default.GetString(response);
                }
            }
            else if (cmbFoodInterval.Text == "Second")
            {
                using (var client = new WebClient())
                {
                    var values = new NameValueCollection();
                    values["targetfoodtime"] = (Convert.ToDouble(txtFoodInterval.Text) / 3600).ToString().Replace(",", ".");
                    var response = client.UploadValues("http://localhost:3030/manual-food-controll", values);
                    var responseString = Encoding.Default.GetString(response);
                }
            }
        }
        #endregion

        #endregion

        #endregion

        #region Method POST
        private void Post(string _post,string _value, string _url)
        {
            using (var client = new WebClient())
            {
                var values = new NameValueCollection();
                values[_post] = _value;
                var response = client.UploadValues(_url, values);
                var responseString = Encoding.Default.GetString(response);
            }
        }
        #endregion

        #region Method Get
        public string Get(string _url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_url);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
        #endregion

        private void timer2_Tick(object sender, EventArgs e)
        {
            string[] getStr = Get("http://localhost:3030/get-info").Split(Convert.ToChar(" "));
            heaterStatus = getStr[0];
            lightStatus = getStr[1];
            lblLightStrength.Text = getStr[2] + "%";
            lblLastTimeFed.Text = getStr[3].Replace("-", " ");
            lblLightClock.Text = getStr[4].Substring(11);
            lblLightInterval.Text = Convert.ToDouble((getStr[5].Replace(".", ","))).ToString("00.####") + " hours";
            lblFoodInterval.Text = Convert.ToDouble((getStr[6].Replace(".", ","))).ToString("00.####") + " hours";
            lblHeaterTemperature.Text = Convert.ToDouble((getStr[7].Replace(".", ","))).ToString("00.####") + "°C";
            lblFoodClock.Text = getStr[8].Substring(11);
            if(lightStatus == "OFF")
            {
                lblLightStrength.Text = "0%";
            }
            if(getStr[9] == "0")
            {
                lblWaterLevel.Text = "Full";
                pctLevel.Image = Image.FromFile(Directory.GetCurrentDirectory() + "/0.jpeg");
            }
            else if(getStr[9] == "1"){
                lblWaterLevel.Text = "Border Line";
                pctLevel.Image = Image.FromFile(Directory.GetCurrentDirectory() + "/1.jpeg");

            }
            else if (getStr[9] == "2")
            {
                lblWaterLevel.Text = "Bellow Border Line";
                pctLevel.Image = Image.FromFile(Directory.GetCurrentDirectory() + "/2.jpeg");
            }
            else if (getStr[9] == "3")
            {
                lblWaterLevel.Text = "Change Level";
                pctLevel.Image = Image.FromFile(Directory.GetCurrentDirectory() + "/3.jpeg");
            }
            if (lblLastTimeFed.Text == "null")
            {
                lblLastTimeFed.Text = "None";
            }
            else
            {
                lblLastTimeFed.Text = getStr[3].Replace("-", " ");
            }
            if (heaterStatus == "ON")
            {
                lblHeaterON.BackColor = Color.LightGreen;
                lblHeaterOFF.BackColor = Color.Gainsboro;
            }
            else
            {
                lblHeaterON.BackColor = Color.Gainsboro;
                lblHeaterOFF.BackColor = Color.LightCoral;
            }

            if (lightStatus == "ON")
            {
                lblLightOn.BackColor = Color.LightGreen;
                lblLightOff.BackColor = Color.Gainsboro;
            }
            else
            {
                lblLightOn.BackColor = Color.Gainsboro;
                lblLightOff.BackColor = Color.LightCoral;
            }
            if(lblLastTimeFed.Text != "None")
            {
                feedStatus = "Fed";
                lblFoodStatus.Text = "Fed";
                lblFoodStatus.ForeColor = Color.Green;
                lblFeedWarning.ForeColor = Color.Green;
                lblFeedWarning.Text = "Your fish is fed!";
                btnFeed.ForeColor = Color.Green;
                pctFed.Image = Image.FromFile(Directory.GetCurrentDirectory() + "/h.png");
            }
            else
            {
                feedStatus = "Not Fed";
                lblFoodStatus.Text = "Not Fed";
                lblFoodStatus.ForeColor = Color.Red;
                lblFeedWarning.ForeColor = Color.Red;
                lblFeedWarning.Text = "You should feed your fish now!";
                btnFeed.ForeColor = Color.Red;
                pctFed.Image = Image.FromFile(Directory.GetCurrentDirectory() + "/s.png");
            }
        }
    }
}
