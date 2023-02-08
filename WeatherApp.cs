using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using Timer = System.Windows.Forms.Timer;
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable IDE0017 // Object initialization can be simplified

namespace WeatherApp
{
    internal class WeatherRenderer : ToolStripProfessionalRenderer
    {
        private bool lightTheme = false;
        private static WeatherColors weatherColors = new();
        public WeatherRenderer() : base(weatherColors) { }

        public void ChamgeTheme(bool lightTheme)
        {
            this.lightTheme = lightTheme;
            weatherColors.lightTheme = lightTheme;
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = lightTheme ? WeatherColors.DarkGray1 : WeatherColors.LightGray3;
            base.OnRenderArrow(e);
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = lightTheme ? WeatherColors.DarkGray1 : WeatherColors.LightGray3;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            Pen pen = new(lightTheme ? WeatherColors.LightGray5 : WeatherColors.DarkGray5, 1);
            e.Graphics.DrawLine(pen, new Point(0, e.Item.Height / 2), new Point(e.ToolStrip.Width, e.Item.Height / 2));
        }
    }

    internal class WeatherColors : ProfessionalColorTable
    {
        public bool lightTheme = false;
        public static Color DarkGray1 = Color.FromArgb(15, 15, 15);
        public static Color DarkGray2 = Color.FromArgb(31, 31, 31);
        public static Color DarkGray3 = Color.FromArgb(47, 47, 47);
        public static Color DarkGray4 = Color.FromArgb(63, 63, 63);
        public static Color DarkGray5 = Color.FromArgb(79, 79, 79);
        public static Color LightGray1 = Color.FromArgb(239, 239, 239);
        public static Color LightGray2 = Color.FromArgb(223, 223, 223);
        public static Color LightGray3 = Color.FromArgb(207, 207, 207);
        public static Color LightGray4 = Color.FromArgb(191, 191, 191);
        public static Color LightGray5 = Color.FromArgb(175, 175, 175);

        public override Color MenuItemSelected
        {
            get { return lightTheme ? LightGray5 : DarkGray5; }
        }

        public override Color MenuItemSelectedGradientBegin
        {
            get { return lightTheme ? LightGray5 : DarkGray5; }
        }

        public override Color MenuItemSelectedGradientEnd
        {
            get { return lightTheme ? LightGray5 : DarkGray5; }
        }

        public override Color MenuItemBorder
        {
            get { return lightTheme ? LightGray5 : DarkGray5; }
        }

        public override Color SeparatorDark
        {
            get { return lightTheme ? LightGray5 : DarkGray5; }
        }

        public override Color CheckBackground
        {
            get { return lightTheme ? LightGray5 : DarkGray5; }
        }

        public override Color CheckSelectedBackground
        {
            get { return lightTheme ? LightGray5 : DarkGray5; }
        }

        public override Color ButtonSelectedBorder
        {
            get { return lightTheme ? LightGray5 : DarkGray5; }
        }

        public override Color ButtonCheckedHighlightBorder
        {
            get { return lightTheme ? LightGray5 : DarkGray5; }
        }

        public override Color ToolStripPanelGradientBegin
        {
            get { return lightTheme ? LightGray3 : DarkGray3; }
        }

        public override Color ToolStripPanelGradientEnd
        {
            get { return lightTheme ? LightGray3 : DarkGray3; }
        }

        public override Color ToolStripDropDownBackground
        {
            get { return lightTheme ? LightGray3 : DarkGray3; }
        }

        public override Color ImageMarginGradientBegin
        {
            get { return lightTheme ? LightGray3 : DarkGray3; }
        }

        public override Color ImageMarginGradientEnd
        {
            get { return lightTheme ? LightGray3 : DarkGray3; }
        }

        public override Color ImageMarginGradientMiddle
        {
            get { return lightTheme ? LightGray3 : DarkGray3; }
        }
    }

    internal class WeatherApp
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        extern static bool DestroyIcon(IntPtr handle);

        private static WeatherRenderer weatherRenderer = new();
        private static NotifyIcon weatherIcon = new();
        private static NotifyIcon temperatureIcon = new();
        private static ToolStripMenuItem menuHourlyForecast = new();
        private static ContextMenuStrip menuHourlyDropdown = new();
        private static ToolStripMenuItem menuDailyForecast = new();
        private static ContextMenuStrip menuDailyDropdown = new();
        private static ToolStripMenuItem menuRefresh = new();
        private static Dictionary<int, ToolStripMenuItem> menuRefreshItems = new();
        private static ToolStripMenuItem menuTemp = new();
        private static ToolStripMenuItem menuTempF = new();
        private static ToolStripMenuItem menuTempC = new();
        private static ToolStripMenuItem menuTheme = new();
        private static ToolStripMenuItem menuThemeLight = new();
        private static ToolStripMenuItem menuThemeDark = new();
        private static Timer timer = new();

        private static String latitude = "";
        private static String longitude = "";
        private static char measurement = 'F';
        private static int interval = 30;
        private static bool lightTheme = false;
        private static int temperatureF = 0;
        private static int temperatureC = 0;

        public WeatherApp()
        {
            latitude = Properties.Settings.Default.latitude;
            longitude = Properties.Settings.Default.longitude;
            measurement = Properties.Settings.Default.measurement;
            measurement = (measurement == 'C') ? 'C' : 'F';
            interval = Properties.Settings.Default.interval;
            lightTheme = Properties.Settings.Default.lightTheme;
            weatherRenderer.ChamgeTheme(lightTheme);

            ContextMenuStrip menu = new();
            menu.ShowCheckMargin = false;
            menu.ShowImageMargin = false;
            menu.Renderer = weatherRenderer;

            ToolStripLabel menuTitle = new();
            menuTitle.Text = " WeatherApp";
            menuTitle.Image = Resources.mostlyclear.ToBitmap();
            menu.Items.Add(menuTitle);

            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem menuPromptLocation = new();
            menuPromptLocation.Text = "Input Location";
            menuPromptLocation.Click += new EventHandler(PromptLocation);
            menu.Items.Add(menuPromptLocation);

            ToolStripMenuItem menuAutoLocation = new();
            menuAutoLocation.Text = "Auto-Location";
            menuAutoLocation.Click += new EventHandler(AutoGetLocation);
            menu.Items.Add(menuAutoLocation);

            menu.Items.Add(new ToolStripSeparator());

            menuHourlyForecast.Text = "Hourly Forecast";
            menuHourlyForecast.Enabled = false;
            menuHourlyForecast.DropDown = menuHourlyDropdown;
            menuHourlyDropdown.Renderer = weatherRenderer;
            menuHourlyDropdown.ShowImageMargin = false;
            menuHourlyDropdown.ShowCheckMargin = false;
            menu.Items.Add(menuHourlyForecast);

            menuDailyForecast.Text = "Daily Forecast";
            menuDailyForecast.Enabled = false;
            menuDailyForecast.DropDown = menuDailyDropdown;
            menuDailyDropdown.Renderer = weatherRenderer;
            menuDailyDropdown.ShowImageMargin = false;
            menuDailyDropdown.ShowCheckMargin = false;
            menu.Items.Add(menuDailyForecast);

            menu.Items.Add(new ToolStripSeparator());

            menuRefresh.Text = "Auto-Refresh Interval";
            menu.Items.Add(menuRefresh);

            CreateRefreshMenu(0, "No Auto-Refresh");
            menuRefresh.DropDownItems.Add(new ToolStripSeparator());
            CreateRefreshMenu(1, "1 Minute");
            CreateRefreshMenu(5, "5 Minutes");
            CreateRefreshMenu(10, "10 Minutes");
            CreateRefreshMenu(15, "15 Minutes");
            CreateRefreshMenu(20, "20 Minutes");
            CreateRefreshMenu(30, "30 Minutes");
            CreateRefreshMenu(60, "60 Minutes");
            CreateRefreshMenu(480, "8 Hours");
            CreateRefreshMenu(1440, "24 Hours");

            try
            {
                menuRefreshItems[interval].Checked = true;
            }
            catch
            {
                menuRefreshItems[30].Checked = false;
                interval = 30;
            }

            menuTemp.Text = "Temperature (°" + measurement + ")";
            menu.Items.Add(menuTemp);

            menuTempF.Text = "°F";
            menuTempF.Checked = (measurement != 'C');
            menuTempF.Click += (s, e) => { ChangeMeasurement(false); };
            menuTemp.DropDownItems.Add(menuTempF);

            menuTempC.Text = "°C";
            menuTempC.Checked = (measurement == 'C');
            menuTempC.Click += (s, e) => { ChangeMeasurement(true); };
            menuTemp.DropDownItems.Add(menuTempC);

            menuTheme.Text = "Theme";
            menu.Items.Add(menuTheme);

            menuThemeLight.Text = "Light";
            menuThemeLight.Checked = lightTheme;
            menuThemeLight.Click += (s, e) => { ChangeTheme(true); };
            menuTheme.DropDownItems.Add(menuThemeLight);

            menuThemeDark.Text = "Dark";
            menuThemeDark.Checked = !lightTheme;
            menuThemeDark.Click += (s, e) => { ChangeTheme(false); };
            menuTheme.DropDownItems.Add(menuThemeDark);

            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem menuExit = new();
            menuExit.Text = "Exit";
            menuExit.Click += new EventHandler(Exit);
            menu.Items.Add(menuExit);

            temperatureIcon = new NotifyIcon
            {
                ContextMenuStrip = menu,
                Visible = true
            };
            temperatureIcon.Click += (s, e) =>
            {
                MethodInfo? mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                mi?.Invoke(temperatureIcon, null);
            };

            weatherIcon = new NotifyIcon
            {
                ContextMenuStrip = menu,
                Icon = Resources.mostlyclear,
                Text = "Click for location options",
                Visible = true
            };
            weatherIcon.Click += (s, e) =>
            {
                MethodInfo? mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                mi?.Invoke(weatherIcon, null);
            };

            GetWeather();

            timer.Interval = interval * 60000;
            timer.Tick += (s, e) => { GetWeather(); };
            timer.Start();
        }

        private static void CreateRefreshMenu(int minutes, String text)
        {
            ToolStripMenuItem menuRefreshItem = new();
            menuRefreshItem.Text = text;
            menuRefreshItem.Click += (s, e) => { SetAutoUpdateInterval(minutes); };
            menuRefreshItems.Add(minutes, menuRefreshItem);
            menuRefresh.DropDownItems.Add(menuRefreshItem);
        }

        private static void SetAutoUpdateInterval(int minutes)
        {
            foreach (ToolStripMenuItem t in menuRefreshItems.Values)
            {
                t.Checked = false;
            }
            menuRefreshItems[minutes].Checked = true;
            interval = minutes;
            if (minutes == 0)
            {
                timer.Stop();
            }
            else
            {
                timer.Stop();
                timer.Interval = minutes * 60000;
                timer.Start();
            }
            SaveSettings();
        }

        private static void ChangeMeasurement(bool C)
        {
            measurement = C ? 'C' : 'F';
            menuTempC.Checked = C;
            menuTempF.Checked = !C;
            menuTemp.Text = "Temperature (°" + measurement + ")";
            GetWeather();
            SaveSettings();
        }

        private static void ChangeTheme(bool light)
        {
            lightTheme = light;
            weatherRenderer.ChamgeTheme(light);
            menuThemeLight.Checked = light;
            menuThemeDark.Checked = !light;
            SetTemperatureIcon();
            SaveSettings();
        }

        private static void PromptLocation(Object? sender, EventArgs e)
        {
            Form prompt = new()
            {
                Width = 160,
                Height = 100,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterScreen,
                ControlBox = false
            };
            Label label = new() { Left = 5, Top = 10, Text = "Input Location:" };
            prompt.Controls.Add(label);
            TextBox textBox = new() { Left = 5, Top = 35, Width = 140 };
            prompt.Controls.Add(textBox);
            Button buttonOk = new() { Left = 80, Top = 65, Width = 65, Text = "OK", DialogResult = DialogResult.OK };
            buttonOk.Click += (s, e) => { prompt.Close(); };
            prompt.Controls.Add(buttonOk);
            Button buttonCancel = new() { Left = 5, Top = 65, Width = 65, Text = "Cancel", DialogResult = DialogResult.Cancel };
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
            try
            {
                HttpClient http = new();
                ProductInfoHeaderValue userAgentHeader = new("WeatherApp", "1.0");
                http.DefaultRequestHeaders.UserAgent.Add(userAgentHeader);
                String url = "https://geocode.maps.co/search?q=" + location;
                String response = await http.GetStringAsync(url);
                JToken root1 = JToken.Parse(response);
                JToken? first = root1.First;
                if (first != null && first.HasValues)
                {
                    latitude = first.Value<String>("lat") ?? latitude;
                    longitude = first.Value<String>("lon") ?? longitude;
                    GetWeather();
                }
            }
            catch
            {

            }
        }

        private static async void AutoGetLocation(Object? sender, EventArgs e)
        {
            try
            {
                HttpClient http = new();
                ProductInfoHeaderValue userAgentHeader = new("WeatherApp", "1.0");
                http.DefaultRequestHeaders.UserAgent.Add(userAgentHeader);
                String url = "https://ipapi.co/json/";
                String response = await http.GetStringAsync(url);
                JToken root = JToken.Parse(response);
                if (root.HasValues)
                {
                    latitude = root.Value<String>("latitude") ?? latitude;
                    longitude = root.Value<String>("longitude") ?? longitude;
                    GetWeather();
                }
            }
            catch
            {

            }
        }

        private static async void GetWeather()
        {
            if (latitude == "" || longitude == "") return;
            try
            {
                HttpClient http = new();
                ProductInfoHeaderValue userAgentHeader = new("WeatherApp", "1.0");
                http.DefaultRequestHeaders.UserAgent.Add(userAgentHeader);
                String url = "https://api.open-meteo.com/v1/forecast?" +
                    "latitude=" + latitude + "&longitude=" + longitude +
                    "&hourly=temperature_2m,weathercode" +
                    "&daily=weathercode,temperature_2m_max,temperature_2m_min" +
                    "&current_weather=true&temperature_unit=fahrenheit" +
                    "&timeformat=unixtime&timezone=auto";
                String response = await http.GetStringAsync(url);
                JToken root = JToken.Parse(response);
                JToken? weather = root.Value<JToken>("current_weather");
                if (weather != null && weather.HasValues)
                {
                    int weatherCode = weather.Value<int>("weathercode");
                    String forecast = GetForecast(weatherCode);
                    weatherIcon.Icon = GetIcon(weatherCode);
                    temperatureF = weather.Value<int>("temperature");
                    temperatureC = ((temperatureF - 32) * 5) / 9;
                    weatherIcon.Text = forecast;
                    SetTemperatureIcon();
                    temperatureIcon.Text = temperatureF + " °F" + Environment.NewLine + temperatureC + " °C";
                    menuHourlyForecast.Enabled = true;
                    menuDailyForecast.Enabled = true;
                    SaveSettings();
                    
                    int currentTime = weather.Value<int>("time");
                    JToken hourly = root.Value<JToken>("hourly");
                    JToken daily = root.Value<JToken>("daily");
                    DisplayHourlyForecast(hourly, currentTime);
                    DisplayDailyForecast(daily);
                }
            }
            catch
            {

            }
        }

        private static void DisplayHourlyForecast(JToken hourly, int currentTime)
        {
            int[] times = hourly.Value<JArray>("time").ToObject<int[]>();
            int[] hourlyTemps = hourly.Value<JArray>("temperature_2m").ToObject<int[]>();
            int[] hourlyCodes = hourly.Value<JArray>("weathercode").ToObject<int[]>();
            menuHourlyForecast.DropDownItems.Clear();
            ToolStripLabel label = new ToolStripLabel(String.Format("      {0}    {1}", "Temp", "Time"));
            menuHourlyForecast.DropDownItems.Add(label);
            menuHourlyForecast.DropDownItems.Add(new ToolStripSeparator());
            int hours = 0;
            int currentDay = new DateTime(1970, 1, 1).AddSeconds(currentTime).ToLocalTime().Day;
            bool nextDay = false;
            for (int i = 0; i < times.Length && hours < 11; i++)
            {
                if (times[i] > currentTime)
                {
                    if (!nextDay)
                    {
                        int day = new DateTime(1970, 1, 1).AddSeconds(times[i]).ToLocalTime().Day;
                        if (day != currentDay)
                        {
                            if (i != 0) menuHourlyForecast.DropDownItems.Add(new ToolStripSeparator());
                            nextDay = true;
                        }
                    }
                    int hourlyTempF = hourlyTemps.ElementAt<int>(i);
                    int hourlyTempC = ((hourlyTempF - 32) * 5) / 9;
                    String hourlyTemp = measurement == 'C' ? hourlyTempC + "°" : hourlyTempF + "°";
                    int hourlyCode = int.Parse(hourlyCodes[i].ToString());
                    Image image = GetIcon(hourlyCode).ToBitmap();
                    String time = new DateTime(1970, 1, 1).AddSeconds(times[i]).ToLocalTime().ToLongTimeString();
                    String ampm = time.Split(' ')[1];
                    String hour = time.Split(':')[0];
                    String display = String.Format("  {0, 4}     {1, 2} {2}", hourlyTemp, hour, ampm);
                    ToolStripLabel hourlyForecast = new ToolStripLabel(display, image);
                    menuHourlyForecast.DropDownItems.Add(hourlyForecast);
                    hours++;
                }
            }
        }

        private static void DisplayDailyForecast(JToken daily)
        {
            int[] days = daily.Value<JArray>("time").ToObject<int[]>();
            int[] dailyMaxTemps = daily.Value<JArray>("temperature_2m_max").ToObject<int[]>();
            int[] dailyMinTemps = daily.Value<JArray>("temperature_2m_min").ToObject<int[]>();
            int[] dailyCodes = daily.Value<JArray>("weathercode").ToObject<int[]>();
            menuDailyForecast.DropDownItems.Clear();
            ToolStripLabel label = new ToolStripLabel(String.Format("      {0}  {1}    {2}", "High", "Low", "Day"));
            menuDailyForecast.DropDownItems.Add(label);
            menuDailyForecast.DropDownItems.Add(new ToolStripSeparator());
            for (int i = 0; i < 7; i++)
            {
                int dailyMaxTempF = dailyMaxTemps.ElementAt<int>(i);
                int dailyMinTempF = dailyMinTemps.ElementAt<int>(i);
                int dailyMaxTempC = ((dailyMaxTempF - 32) * 5) / 9;
                int dailyMinTempC = ((dailyMinTempF - 32) * 5) / 9;
                String dailyMaxTemp = measurement == 'C' ? dailyMaxTempC + "°" : dailyMaxTempF + "°";
                String dailyMinTemp = measurement == 'C' ? dailyMinTempC + "°" : dailyMinTempF + "°";
                int dailyCode = int.Parse(dailyCodes[i].ToString());
                Image image = GetIcon(dailyCode).ToBitmap();
                String day = new DateTime(1970, 1, 1).AddSeconds(days[i]).ToLocalTime().DayOfWeek.ToString().Substring(0, 3);
                String display = String.Format("  {0, 4}   {1, 4}    {2}", dailyMaxTemp, dailyMinTemp, day);
                ToolStripLabel dailyForecast = new ToolStripLabel(display, image);
                menuDailyForecast.DropDownItems.Add(dailyForecast);
                if (i < 6)
                {
                    menuDailyForecast.DropDownItems.Add(new ToolStripSeparator());
                }
            }
        }

        private static void PasteDigitImage(Bitmap bitmap, Bitmap digitmap, int xOffset)
        {
            for (int x = 0; x < 6; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    if (digitmap.GetPixel(x, y).A == 255)
                    {
                        bitmap.SetPixel(x + xOffset, y, lightTheme ? Color.Black : Color.White);
                    }
                }
            }
        }

        private static void SetTemperatureIcon()
        {
            Bitmap bitmap = new(16, 16);
            PasteDigitImage(bitmap, (measurement == 'C') ? new Bitmap(Resources.degreeC) : new Bitmap(Resources.degreeF), 10);
            int temperature = (measurement == 'C') ? temperatureC : temperatureF;
            if (temperature > 99) temperature = 99;
            if (temperature < -19) temperature = -19;
            if (temperature < 0)
            {
                temperature *= -1;
                int digit1 = temperature % 10;
                int digit2 = temperature / 10;
                if (digit2 > 0)
                {
                    Bitmap digitmap1 = GetDigitImage(digit1);
                    PasteDigitImage(bitmap, digitmap1, 6);
                    Bitmap digitmap2 = GetDigitImage(-1);
                    PasteDigitImage(bitmap, digitmap2, 0);
                }
                else
                {
                    Bitmap digitmap1 = GetDigitImage(digit1);
                    PasteDigitImage(bitmap, digitmap1, 6);
                    Bitmap digitmap2 = GetDigitImage(-2);
                    PasteDigitImage(bitmap, digitmap2, 0);
                }
            }
            else
            {
                int digit1 = temperature % 10;
                int digit2 = temperature / 10;
                Bitmap digitmap1 = GetDigitImage(digit1);
                PasteDigitImage(bitmap, digitmap1, 6);
                if (digit2 > 0)
                {
                    Bitmap digitmap2 = GetDigitImage(digit2);
                    PasteDigitImage(bitmap, digitmap2, 0);
                }
            }
            IntPtr ptr = bitmap.GetHicon();
            Icon icon = Icon.FromHandle(ptr);
            temperatureIcon.Icon = (Icon)icon.Clone();
            DestroyIcon(icon.Handle);
        }

        private static void SaveSettings()
        {
            Properties.Settings.Default.longitude = longitude;
            Properties.Settings.Default.latitude = latitude;
            Properties.Settings.Default.measurement = (measurement == 'F' || measurement == 'C') ? measurement : 'F';
            Properties.Settings.Default.interval = interval;
            Properties.Settings.Default.lightTheme = lightTheme;
            Properties.Settings.Default.Save();
        }

        private void Exit(Object? sender, EventArgs e)
        {
            SaveSettings();
            weatherIcon.Visible = false;
            weatherIcon.Dispose();
            temperatureIcon.Visible = false;
            temperatureIcon.Dispose();
            Application.Exit();
        }

        private static Bitmap GetDigitImage(int digit)
        {
            return digit switch
            {
                -2 => Resources.tempminus,
                -1 => Resources.tempminus1,
                0 => Resources.temp0,
                1 => Resources.temp1,
                2 => Resources.temp2,
                3 => Resources.temp3,
                4 => Resources.temp4,
                5 => Resources.temp5,
                6 => Resources.temp6,
                7 => Resources.temp7,
                8 => Resources.temp8,
                9 => Resources.temp9,
                _ => Resources.temp0,
            };
        }

        private static Icon GetIcon(int weatherCode)
        {
            return weatherCode switch
            {
                0 => Resources.sunny,
                1 => Resources.mostlyclear,
                2 => Resources.partlycloudy,
                3 => Resources.overcast,
                45 => Resources.fog,
                48 => Resources.fog,
                51 => Resources.drizzle,
                53 => Resources.drizzle,
                55 => Resources.drizzle,
                56 => Resources.drizzle,
                57 => Resources.drizzle,
                61 => Resources.rain,
                63 => Resources.rain,
                65 => Resources.rain,
                66 => Resources.rain,
                67 => Resources.rain,
                71 => Resources.snow,
                73 => Resources.snow,
                75 => Resources.snow,
                77 => Resources.snow,
                80 => Resources.rain,
                81 => Resources.rain,
                82 => Resources.rain,
                85 => Resources.snow,
                86 => Resources.snow,
                95 => Resources.thunderstorm,
                96 => Resources.thunderstorm,
                99 => Resources.thunderstorm,
                _ => Resources.mostlyclear,
            };
        }

        private static String GetForecast(int weatherCode)
        {
            return weatherCode switch
            {
                0 => "Clear Sky",
                1 => "Mostly Clear",
                2 => "Partly Cloudy",
                3 => "Overcast",
                45 => "Fog",
                48 => "Rime Fog",
                51 => "Light Drizzle",
                53 => "Moderate Drizzle",
                55 => "Heavy Drizzle",
                56 => "Light Freezing Drizzle",
                57 => "Heavy Freezing Drizzle",
                61 => "Light Rain",
                63 => "Moderate Rain",
                65 => "Heavy Rain",
                66 => "Light Freezing Rain",
                67 => "Heavy Freezing Rain",
                71 => "Light Snowfall",
                73 => "Moderate Snowfall",
                75 => "Heavy Snowfall",
                77 => "Snow Grains",
                80 => "Light Rain Showers",
                81 => "Moderate Rain Showers",
                82 => "Heavy Rain Showers",
                85 => "Light Snow Showers",
                86 => "Heavy Snow Showers",
                95 => "Thunderstorm",
                96 => "Thunderstorm with Light Hail",
                99 => "Thunderstorm with Heavy Hail",
                _ => "",
            };
        }
    }
}
