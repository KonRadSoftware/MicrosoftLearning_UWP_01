using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace MicrosoftLearning_UWP_03
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // Gets or sets the value of the first number that the arithmetic will be performed on.
        private double? FirstNumber { get; set; }

        // Gets or sets the value of the second number that the arithmetic will be performed on.
        private double? SecondNumber { get; set; }

        // Gets or sets the selected arithmetic operation.
        private Func<double, double, double> SelectedMathFunction { get; set; }

        public MainPage()
        {
            this.InitializeComponent();
        }

        private double Add(double a, double b)
        {
            double result = a + b;
            return result;
        }

        private double Subtract(double a, double b)
        {
            double result = a - b;
            return result;
        }

        private double Multiply(double a, double b)
        {
            double result = a * b;
            return result;
        }

        private double Divide(double a, double b)
        {
            double result = a / b;
            return result;
        }

        private void FirstNumberBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Check if the text in FirstNumberBox.Text is empty.
            if (string.IsNullOrEmpty(FirstNumberBox?.Text))
            {
                FirstNumber = null;
                return;
            }

            // If text was entered, check to see if it can be parsed into an number.
            if (double.TryParse(FirstNumberBox.Text, out double parsedNumber))
            {
                // If the text is an integer, then set the value of the FirstNumber property.
                FirstNumber = parsedNumber;
            }
            else
            {
                // If it not a number, remove the last entered character with Trim() method.
                FirstNumberBox.Text = FirstNumberBox.Text.TrimEnd(FirstNumberBox.Text.LastOrDefault());
            }
        }

        private void RadioButton_OnChecked(object sender, RoutedEventArgs e)
        {
            var radio = sender as RadioButton;
            string inhalt = radio?.Name.ToString();
            switch (inhalt)
            {
                case "rdoPlus":
                    SelectedMathFunction = Add;
                    break;
                case "rdoMinus":
                    SelectedMathFunction = Subtract;
                    break;
                case "rdoDivi":
                    SelectedMathFunction = Divide;
                    break;
                case "rdoMulti":
                    SelectedMathFunction = Multiply;
                    break;
                default:
                    SelectedMathFunction = null;
                    break;
            }

        }

        private void SecondNumber_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(SecondNumberBox?.Text))
            {
                SecondNumber = null;
                return;
            }
            if (double.TryParse(SecondNumberBox.Text, out double parsedNumber))
            {
                SecondNumber = parsedNumber;
            }
            else
            {
                SecondNumberBox.Text = SecondNumberBox.Text.TrimEnd(SecondNumberBox.Text.LastOrDefault());
            }
        }

        private void SecondNumberSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            SecondNumberBox.Text = e.NewValue.ToString("N");
        }

        private void IncludeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CalculationDatePicker.Visibility = Visibility.Visible;
            CalculationDatePicker.Date = DateTimeOffset.Now;
        }

        private void IncludeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CalculationDatePicker.Visibility = Visibility.Collapsed;
        }

        private async void cmdRechnen_Click(object sender, RoutedEventArgs e)
        {
            // Before doing any calculations, confirm the user entered both numbers.
            if (FirstNumber == null || SecondNumber == null)
            {
                await new MessageDialog("You need to set both numbers to calculate a result.").ShowAsync();
                return;
            }

            // Now is a good time to do some validation on the numbers and prevent any serious problems.
            // For example, here we make sure the user isn't trying to divide from zero, this can crash your app!
            if (SecondNumber == 0 && SelectedMathFunction == Divide)
            {
                await new MessageDialog("You cannot divide from zero, please pick a different 2nd number.").ShowAsync();
                return;
            }

            // Next, it's time to do the actual math. We only need to pass the two numbers into the SelectedMathFunction.
            double result = SelectedMathFunction((double)FirstNumber, (double)SecondNumber);

            // Finally, show the result to the user!
            if (IncludeCheckBox.IsChecked == true)
            {
                // If the CheckBox was checked, show the number with the "N2" string format (a number to 2 decimal points),
                // but also include the Date in the output with the "d" string format (a short date format).
                txtErgebnis.Text = $"Result: {result:N2}, Date: {CalculationDatePicker.Date:d}";
            }
            else
            {
                // If the CheckBox was not checked, show the number with the "N2" string format (a number to 2 decimal points).
                txtErgebnis.Text = $"Result: {result:N2}";
            }
        }
    }
}
