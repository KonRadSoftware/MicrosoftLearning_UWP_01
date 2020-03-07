using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace MicrosoftLearning_UWP_APOD_Nasa
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // The objective of the NASA API portal is to make NASA data, including imagery, eminently accessible to application developers. 
        const string EndpointURL = "https://api.nasa.gov/planetary/apod";

        // June 16, 1995: the APOD launch date.
        DateTime launchDate = new DateTime(1995, 6, 16);

        // A count of images downloaded today.
        int imageCountToday;

        // Settings name strings, used to preserve UI values between sessions.
        const string SettingDateToday = "date today";
        const string SettingShowOnStartup = "show on startup";
        const string SettingImageCountToday = "image count today";
        const string SettingLimitRange = "limit range";

        // Declare a container for the local settings.
        Windows.Storage.ApplicationDataContainer localSettings;

        public MainPage()
        {
            this.InitializeComponent();

            // Set the maximum date to today, and the minimum date to the APOD launch date.
            dateMonthCalender.MinDate = launchDate;
            dateMonthCalender.MaxDate = DateTime.Today;

            // Create the container for the local settings.
            localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            ReadSettings();
        }

        private void ReadSettings()
        {
            // If the app is being started the same day that it was run previously, the count of images downloaded today
            // must be set to the stored setting. Otherwise, it should be zero.
            bool isToday = false;
            Object todayObject = localSettings.Values[SettingDateToday];

            if (todayObject != null)
            {
                // First check to see if this is the same day as the previous run of the app.
                DateTime dt = DateTime.Parse((string)todayObject);
                if (dt.Equals(DateTime.Today))
                {
                    isToday = true;
                }
            }

            // Set the default for images downloaded today.
            imageCountToday = 0;

            if (isToday)
            {
                Object value = localSettings.Values[SettingImageCountToday];
                if (value != null)
                {
                    imageCountToday = int.Parse((string)value);
                }
            }
            txtAnzahlBilder.Text = imageCountToday.ToString();

            // Set the UI check boxes, depending on the stored settings or defaults if there are no settings.
            Object showTodayObject = localSettings.Values[SettingShowOnStartup];
            if (showTodayObject != null)
            {
                chkBildToday.IsChecked = bool.Parse((string)showTodayObject);
            }
            else
            {
                // Set the default.
                chkBildToday.IsChecked = true;
            }

            Object limitRangeObject = localSettings.Values[SettingLimitRange];
            if (limitRangeObject != null)
            {
                chkAuswahlJahr.IsChecked = bool.Parse((string)limitRangeObject);
            }
            else
            {
                // Set the default.
                chkAuswahlJahr.IsChecked = false;
            }

            // Show today's image if the check box requires it.
            if (chkBildToday.IsChecked == true)
            {
                dateMonthCalender.Date = DateTime.Today;
            }
        }

        private void cmdLaunchButton_Click(object sender, RoutedEventArgs e)
        {
            // Make sure the full range of dates is available.
            chkBildToday.IsChecked = false;

            // This will not load up the image, just sets the calendar to the APOD launch date.
            dateMonthCalender.Date = launchDate;
        }

        private void chkAuswahlJahr_Checked(object sender, RoutedEventArgs e)
        {
            // Set the calendar minimum date to the first of the current year.
            var firstDayOfThisYear = new DateTime(DateTime.Today.Year, 1, 1);
            dateMonthCalender.MinDate = firstDayOfThisYear;
        }

        private void chkAuswahlJahr_Unchecked(object sender, RoutedEventArgs e)
        {
            // Set the calendar minimum date to the launch of the APOD program.
            dateMonthCalender.MinDate = launchDate;
        }

        private async void dateMonthCalender_DateChangedAsync(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            await RetrievePhoto();
        }

        private async Task RetrievePhoto()
        {
            var client = new HttpClient();
            JObject jResult = null;
            string responseContent = null;
            string description = null;
            string photoUrl = null;
            string copyright = null;

            // Set the UI elements to defaults
            txtCopyright.Text = "NASA";
            txtBemerkung.Text = "";

            // Build the date parameter string for the date selected, or the last date if a range is specified.
            DateTimeOffset dt = (DateTimeOffset)dateMonthCalender.Date;

            string dateSelected = $"{dt.Year.ToString()}-{dt.Month.ToString("00")}-{dt.Day.ToString("00")}";
            string URLParams = $"?date={dateSelected}&api_key=DEMO_KEY";

            // Populate the HTTP client appropriately.
            client.BaseAddress = new Uri(EndpointURL);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // The critical call: send a GET request with the appropriate parameters.
            HttpResponseMessage response = client.GetAsync(URLParams).Result;

            if (response.IsSuccessStatusCode)
            {
                // Be ready to catch any data or server errors.
                try
                {
                    // Parse the response by using the Newtonsoft APIs.
                    responseContent = await response.Content.ReadAsStringAsync();

                    // Parse the response string for the details we need.
                    jResult = JObject.Parse(responseContent);

                    // Now get the image.
                    photoUrl = (string)jResult["url"];
                    var photoURI = new Uri(photoUrl);
                    var bmi = new BitmapImage(photoURI);

                    imagePictureBox.Source = bmi;

                    if (IsSupportedFormat(photoUrl))
                    {
                        // Get the copyright message, but fill with "NASA" if no name is provided.
                        copyright = (string)jResult["copyright"];
                        if (copyright != null && copyright.Length > 0)
                        {
                            txtCopyright.Text = copyright;
                        }

                        // Populate the description text box.
                        description = (string)jResult["explanation"];
                        txtBemerkung.Text = description;
                    }
                    else
                    {
                        txtBemerkung.Text = $"Image type is not supported. URL is {photoUrl}";
                    }
                }
                catch (Exception ex)
                {
                    txtBemerkung.Text = $"Image data is not supported. {ex.Message}";
                }

                // Keep track of our downloads in case we reach the limit.
                ++imageCountToday;
                txtAnzahlBilder.Text = imageCountToday.ToString();
            }
            else
            {
                txtBemerkung.Text = "We were unable to retrieve the NASA picture for that day: " +
                    $"{response.StatusCode.ToString()} {response.ReasonPhrase}";
            }
        }

        private bool IsSupportedFormat(string photoURL)
        {
            // Extract the extension and force to lower case for comparison purposes.
            string ext = Path.GetExtension(photoURL).ToLower();

            // Check the extension against supported UWP formats.
            return (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif" ||
                    ext == ".tif" || ext == ".bmp" || ext == ".ico" || ext == ".svg");
        }

        private void Grid_LostFocus(object sender, RoutedEventArgs e)
        {
            WriteSettings();
        }

        private void WriteSettings()
        {
            // Preserve the required UI settings in the local storage container.
            localSettings.Values[SettingDateToday] = DateTime.Today.ToString();
            localSettings.Values[SettingShowOnStartup] = chkBildToday.IsChecked.ToString();
            localSettings.Values[SettingLimitRange] = chkAuswahlJahr.IsChecked.ToString();
            localSettings.Values[SettingImageCountToday] = imageCountToday.ToString();
        }
    }
}
