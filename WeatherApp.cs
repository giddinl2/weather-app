using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;

namespace WeatherApp
{
    internal class WeatherApp
    {
        private static NotifyIcon weatherIcon;
        private static NotifyIcon temperatureIcon;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        extern static bool DestroyIcon(IntPtr handle);

        public WeatherApp()
        {
            ContextMenuStrip menu = new ContextMenuStrip();

            ToolStripMenuItem optionPromptLocation = new ToolStripMenuItem();
            optionPromptLocation.Text = "Input Location";
            optionPromptLocation.Click += new EventHandler(PromptLocation);
            menu.Items.Add(optionPromptLocation);

            ToolStripMenuItem optionAutoLocation = new ToolStripMenuItem();
            optionAutoLocation.Text = "Auto-Location";
            optionAutoLocation.Click += new EventHandler(AutoGetLocation);
            menu.Items.Add(optionAutoLocation);

            ToolStripMenuItem optionExit = new ToolStripMenuItem();
            optionExit.Text = "Exit";
            optionExit.Click += new EventHandler(Exit);
            menu.Items.Add(optionExit);

            temperatureIcon = new NotifyIcon
            {
                ContextMenuStrip = menu,
                Text = "Right-click for location options",
                Visible = true
            };

            weatherIcon = new NotifyIcon
            {
                ContextMenuStrip = menu,
                Icon = Resources.mostlyclear,
                Text = "Right-click for location options",
                Visible = true
            };
        }

        private static void PromptLocation(Object sender, EventArgs e)
        {
            Form prompt = new Form()
            {
                Width = 160,
                Height = 100,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterScreen,
                ControlBox = false
            };
            Label label = new Label() { Left = 5, Top = 10, Text = "Input Location:" };
            prompt.Controls.Add(label);
            TextBox textBox = new TextBox() { Left = 5, Top = 35, Width = 140 };
            prompt.Controls.Add(textBox);
            Button buttonOk = new Button() { Left = 80, Top = 65, Width = 65, Text = "OK", DialogResult = DialogResult.OK };
            buttonOk.Click += (s, e) => { prompt.Close(); };
            prompt.Controls.Add(buttonOk);
            Button buttonCancel = new Button() { Left = 5, Top = 65, Width = 65, Text = "Cancel", DialogResult = DialogResult.Cancel };
            buttonCancel.Click += (s, e) => { prompt.Close(); };
            prompt.Controls.Add(buttonCancel);
            prompt.AcceptButton = buttonOk;
            prompt.CancelButton = buttonCancel;
            if (prompt.ShowDialog() == DialogResult.OK)
            {
                GetLatLong(textBox.Text);
            }
        }

        private static async void GetLatLong(String location)
        {
            HttpClient http = new HttpClient();
            ProductInfoHeaderValue userAgentHeader = new ProductInfoHeaderValue("WeatherApp", "1.0");
            http.DefaultRequestHeaders.UserAgent.Add(userAgentHeader);
            String url = "https://geocode.maps.co/search?q=" + location;
            String response = await http.GetStringAsync(url);
            JToken root1 = JToken.Parse(response);
            JToken first = root1.First;
            if (first != null && first.HasValues)
            {
                String latitude = first.Value<String>("lat");
                String longitude = first.Value<String>("lon");
                GetWeather(latitude, longitude);
            }
        }

        private static async void AutoGetLocation(Object sender, EventArgs e)
        {
            HttpClient http = new HttpClient();
            ProductInfoHeaderValue userAgentHeader = new ProductInfoHeaderValue("WeatherApp", "1.0");
            http.DefaultRequestHeaders.UserAgent.Add(userAgentHeader);
            String url = "https://ipapi.co/json/";
            String response = await http.GetStringAsync(url);
            JToken root = JToken.Parse(response);
            if (root.HasValues)
            {
                String latitude = root.Value<String>("latitude");
                String longitude = root.Value<String>("longitude");
                GetWeather(latitude, longitude);
            }
        }

        private static async void GetWeather(String latitude, String longitude)
        {
            HttpClient http = new HttpClient();
            ProductInfoHeaderValue userAgentHeader = new ProductInfoHeaderValue("WeatherApp", "1.0");
            http.DefaultRequestHeaders.UserAgent.Add(userAgentHeader);
            String url = "https://api.open-meteo.com/v1/forecast?latitude=" + latitude + "&longitude=" + longitude + "&current_weather=true&temperature_unit=fahrenheit";
            String response = await http.GetStringAsync(url);
            JToken root = JToken.Parse(response);
            JToken weather = root.Value<JToken>("current_weather");
            if (weather != null && weather.HasValues)
            {
                int weatherCode = weather.Value<int>("weathercode");
                String forecast = getForecast(weatherCode);
                weatherIcon.Icon = getIcon(weatherCode);
                int temperatureF = weather.Value<int>("temperature");
                int temperatureC = ((temperatureF - 32) * 5) / 9;
                weatherIcon.Text = forecast;
                SetTemperatureIcon('f', temperatureF);
                temperatureIcon.Text = temperatureF + " °F" + Environment.NewLine + temperatureC + " °C";
            }
        }
        private static void PasteDigitImage(Bitmap bitmap, Bitmap digitmap, int xOffset)
        {
            for (int x = 0; x < 6; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    bitmap.SetPixel(x + xOffset, y, digitmap.GetPixel(x, y));
                }
            }
        }

        private static void SetTemperatureIcon(char measurement, int temperature)
        {
            Bitmap bitmap;
            if (measurement == 'c') bitmap = new Bitmap(Resources.degreeC);
            else if (measurement == 'f') bitmap = new Bitmap(Resources.degreeF);
            else return;
            if (temperature < 0)
            {
                temperature *= -1;
                int digit1 = temperature % 10;
                int digit2 = temperature / 10;
                if (digit2 > 0)
                {
                    Bitmap digitmap1 = getDigitImage(digit1);
                    PasteDigitImage(bitmap, digitmap1, 6);
                    Bitmap digitmap2 = getDigitImage(-1);
                    PasteDigitImage(bitmap, digitmap2, 0);
                }
                else
                {
                    Bitmap digitmap1 = getDigitImage(digit1);
                    PasteDigitImage(bitmap, digitmap1, 6);
                    Bitmap digitmap2 = getDigitImage(-2);
                    PasteDigitImage(bitmap, digitmap2, 0);
                }
            }
            else
            {
                int digit1 = temperature % 10;
                int digit2 = temperature / 10;
                Bitmap digitmap1 = getDigitImage(digit1);
                PasteDigitImage(bitmap, digitmap1, 6);
                if (digit2 > 0)
                {
                    Bitmap digitmap2 = getDigitImage(digit2);
                    PasteDigitImage(bitmap, digitmap2, 0);
                }
            }
            IntPtr ptr = bitmap.GetHicon();
            Icon icon = Icon.FromHandle(ptr);
            temperatureIcon.Icon = (Icon)icon.Clone();
            DestroyIcon(icon.Handle);
        }

        private void Exit(Object sender, EventArgs e)
        {
            weatherIcon.Visible = false;
            weatherIcon.Dispose();
            Application.Exit();
        }
        private static Bitmap getDigitImage(int digit)
        {
            switch (digit)
            {
                case -2: return Resources.tempminus;
                case -1: return Resources.tempminus1;
                case 0: return Resources.temp0;
                case 1: return Resources.temp1;
                case 2: return Resources.temp2;
                case 3: return Resources.temp3;
                case 4: return Resources.temp4;
                case 5: return Resources.temp5;
                case 6: return Resources.temp6;
                case 7: return Resources.temp7;
                case 8: return Resources.temp8;
                case 9: return Resources.temp9;
                default: return Resources.temp0;
            }
        }

        private static Icon getIcon(int weatherCode)
        {
            switch (weatherCode)
            {
                case 0: return Resources.sunny;
                case 1: return Resources.mostlyclear;
                case 2: return Resources.partlycloudy;
                case 3: return Resources.overcast;
                case 45: return Resources.fog;
                case 48: return Resources.fog;
                case 51: return Resources.drizzle;
                case 53: return Resources.drizzle;
                case 55: return Resources.drizzle;
                case 56: return Resources.drizzle;
                case 57: return Resources.drizzle;
                case 61: return Resources.rain;
                case 63: return Resources.rain;
                case 65: return Resources.rain;
                case 66: return Resources.rain;
                case 67: return Resources.rain;
                case 71: return Resources.snow;
                case 73: return Resources.snow;
                case 75: return Resources.snow;
                case 77: return Resources.snow;
                case 80: return Resources.rain;
                case 81: return Resources.rain;
                case 82: return Resources.rain;
                case 85: return Resources.snow;
                case 86: return Resources.snow;
                case 95: return Resources.thunderstorm;
                case 96: return Resources.thunderstorm;
                case 99: return Resources.thunderstorm;
                default: return Resources.mostlyclear;
            }
        }

        private static String getForecast(int weatherCode)
        {
            switch (weatherCode)
            {
                case 0: return "Clear Sky";
                case 1: return "Mostly Clear";
                case 2: return "Partly Cloudy";
                case 3: return "Overcast";
                case 45: return "Fog";
                case 48: return "Rime Fog";
                case 51: return "Light Drizzle";
                case 53: return "Moderate Drizzle";
                case 55: return "Heavy Drizzle";
                case 56: return "Light Freezing Drizzle";
                case 57: return "Heavy Freezing Drizzle";
                case 61: return "Light Rain";
                case 63: return "Moderate Rain";
                case 65: return "Heavy Rain";
                case 66: return "Light Freezing Rain";
                case 67: return "Heavy Freezing Rain";
                case 71: return "Light Snowfall";
                case 73: return "Moderate Snowfall";
                case 75: return "Heavy Snowfall";
                case 77: return "Snow Grains";
                case 80: return "Light Rain Showers";
                case 81: return "Moderate Rain Showers";
                case 82: return "Heavy Rain Showers";
                case 85: return "Light Snow Showers";
                case 86: return "Heavy Snow Showers";
                case 95: return "Thunderstorm";
                case 96: return "Thunderstorm with Light Hail";
                case 99: return "Thunderstorm with Heavy Hail";
                default: return "";
            }
        }
    }
}
