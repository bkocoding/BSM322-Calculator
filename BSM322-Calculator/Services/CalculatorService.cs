namespace BSM322_Calculator.Services
{
    public class CalculatorService(Entry operationEntry, Entry resultEntry)
    {
        private readonly Entry operationEntry = operationEntry;
        private readonly Entry resultEntry = resultEntry;

        private double currentValue = 0;
        private double storedValue = 0;
        private double? memoryValue = null;
        private string currentOperator = "";
        private bool isNewNumber = true;
        private bool hasJustCalculated = false;

        public void NumberClickEvent(object sender, EventArgs e)
        {
            if (sender is not Button button)
                return;

            string value = button.Text;

            if (isNewNumber || hasJustCalculated)
            {
                resultEntry.Text = "";
                isNewNumber = false;
                hasJustCalculated = false;
            }

            if (value == "," && resultEntry.Text.Contains(','))
                return;

            if (resultEntry.Text == "0" && value != ",")
                resultEntry.Text = value;
            else
                resultEntry.Text += value;
        }

        public void OperatorClickEvent(object sender, EventArgs e)
        {
            if (sender is not Button button)
                return;

            string op = button.Text;

            switch (op)
            {
                case "C":
                    ResetAll();
                    break;

                case "CE":
                    resultEntry.Text = "0";
                    isNewNumber = true;
                    break;

                case "⌫":
                    if (!string.IsNullOrEmpty(resultEntry.Text) && resultEntry.Text.Length > 0)
                        resultEntry.Text = resultEntry.Text[..^1];
                    if (string.IsNullOrEmpty(resultEntry.Text))
                        resultEntry.Text = "0";
                    break;

                case "+":
                case "−":
                case "×":
                case "÷":
                    PerformPendingOperation();
                    currentOperator = op;
                    storedValue = currentValue;
                    operationEntry.Text = $"{storedValue} {currentOperator}";
                    isNewNumber = true;
                    break;

                case "=":
                    PerformPendingOperation();
                    operationEntry.Text = currentValue.ToString();
                    currentOperator = "";
                    isNewNumber = true;
                    hasJustCalculated = true;
                    break;

                case "%":
                    ApplyPercentage();
                    break;

                case "√":
                    ApplySingleOperation(Math.Sqrt, "√", prefix: true);
                    break;
                case "x²":
                    ApplySingleOperation(x => x * x, "²", prefix: false);
                    break;
                case "1/x":
                    ApplySingleOperation(x => 1 / x, "1/", prefix: true);
                    break;

                case "±":
                    ToggleSign();
                    break;
            }
        }

        public async void MemoryClickEvent(object sender, EventArgs e)
        {
            if (sender is not Button button)
                return;

            string op = button.Text;

            if (string.IsNullOrEmpty(resultEntry.Text))
            {
                resultEntry.Text = "0";
            }

            string textValue = resultEntry.Text.Trim();

            if (string.IsNullOrEmpty(textValue) || textValue == "," || textValue == "-")
                textValue = "0";

            if (!double.TryParse(textValue.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double currentValue))
            {
                currentValue = 0;
            }

            switch (op)
            {
                case "MC":
                    memoryValue = null;
                    await Application.Current!.Windows[0].Page!.DisplayAlert("Memory", "Memory cleared.", "OK");
                    break;

                case "MR":
                    if (memoryValue.HasValue)
                    {
                        resultEntry.Text = memoryValue.Value.ToString(System.Globalization.CultureInfo.InvariantCulture).Replace(".", ",");
                    }
                    else
                    {
                        await Application.Current!.Windows[0].Page!.DisplayAlert("Memory", "Memory is empty.", "OK");
                    }
                    break;

                case "MS":
                    memoryValue = currentValue;
                    await Application.Current!.Windows[0].Page!.DisplayAlert("Memory", $"Saved {memoryValue.Value}", "OK");
                    break;

                case "M+":
                    if (memoryValue.HasValue)
                    {
                        memoryValue += currentValue;
                        await Application.Current!.Windows[0].Page!.DisplayAlert("Memory", $"Memory updated to {memoryValue.Value}", "OK");
                    }
                    else
                    {
                        memoryValue = currentValue;
                        await Application.Current!.Windows[0].Page!.DisplayAlert("Memory", $"Memory initialized to {memoryValue.Value}", "OK");
                    }
                    break;

                case "M-":
                    if (memoryValue.HasValue)
                    {
                        memoryValue -= currentValue;
                        await Application.Current!.Windows[0].Page!.DisplayAlert("Memory", $"Memory updated to {memoryValue.Value}", "OK");
                    }
                    else
                    {
                        memoryValue = -currentValue;
                        await Application.Current!.Windows[0].Page!.DisplayAlert("Memory", $"Memory initialized to {memoryValue.Value}", "OK");
                    }
                    break;

                case "M↓":
                    if (memoryValue.HasValue)
                    {
                        await Application.Current!.Windows[0].Page!.DisplayAlert("Memory", $"Stored value: {memoryValue.Value}", "OK");
                    }
                    else
                    {
                        await Application.Current!.Windows[0].Page!.DisplayAlert("Memory", "Memory is empty.", "OK");
                    }
                    break;

                default:
                    break;
            }
        }


        private void PerformPendingOperation()
        {
            if (!double.TryParse(resultEntry.Text.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double newValue))
                return;

            if (string.IsNullOrEmpty(currentOperator))
            {
                currentValue = newValue;
                return;
            }

            switch (currentOperator)
            {
                case "+": currentValue += newValue; break;
                case "−": currentValue -= newValue; break;
                case "×": currentValue *= newValue; break;
                case "÷":
                    if (newValue == 0)
                    {
                        resultEntry.Text = "Division by zero";
                        return;
                    }
                    currentValue /= newValue;
                    break;
            }

            resultEntry.Text = currentValue.ToString();
        }

        private void ApplySingleOperation(Func<double, double> operation, string symbol, bool prefix)
        {
            if (!double.TryParse(resultEntry.Text.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double value))
                return;

            if (symbol == "1/" && value == 0)
            {
                resultEntry.Text = "Division by zero";
                isNewNumber = true;
                hasJustCalculated = true;
                return;
            }

            double newValue = operation(value);

            currentValue = newValue;
            resultEntry.Text = currentValue.ToString();

            if (prefix)
                operationEntry.Text = $"{symbol}({value})";
            else
                operationEntry.Text = $"{value}{symbol}";

            isNewNumber = true;
            hasJustCalculated = true;
        }

        private void ApplyPercentage()
        {
            if (!double.TryParse(resultEntry.Text.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double percentValue))
                return;

            double baseValue = storedValue;
            double result;
            switch (currentOperator)
            {
                case "+":
                    result = baseValue + (baseValue * percentValue / 100);
                    break;

                case "−":
                    result = baseValue - (baseValue * percentValue / 100);
                    break;

                case "×":
                    result = baseValue * (percentValue / 100);
                    break;

                case "÷":
                    result = baseValue / (percentValue / 100);
                    break;

                default:
                    result = percentValue / 100;
                    operationEntry.Text = result.ToString();
                    resultEntry.Text = result.ToString();

                    currentValue = result;
                    storedValue = result;
                    currentOperator = "";
                    isNewNumber = true;
                    hasJustCalculated = true;
                    return;
            }

            operationEntry.Text = result.ToString();
            resultEntry.Text = result.ToString();

            currentValue = result;
            storedValue = result;
            currentOperator = "";
            isNewNumber = true;
            hasJustCalculated = true;
        }



        private void ToggleSign()
        {
            if (resultEntry.Text == "0" || resultEntry.Text == "0," || string.IsNullOrEmpty(resultEntry.Text))
                return;

            if (!double.TryParse(resultEntry.Text.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double value))
                return;

            value = -value;

            resultEntry.Text = value.ToString(System.Globalization.CultureInfo.InvariantCulture).Replace(".", ",");
        }

        private void ResetAll()
        {
            resultEntry.Text = "0";
            operationEntry.Text = "";
            currentValue = 0;
            storedValue = 0;
            currentOperator = "";
            isNewNumber = true;
            hasJustCalculated = false;
        }
    }
}
