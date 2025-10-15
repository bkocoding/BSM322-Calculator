using BSM322_Calculator.Enums;

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

        private AngleMode angleMode = AngleMode.Deg;

        /// <summary>
        /// Toggles the current angle mode to the next mode in the sequence.
        /// </summary>
        /// <remarks>The angle modes cycle in the following order: Degrees (Deg), Radians (Rad), and
        /// Gradians (Grad). Calling this method updates the current angle mode and returns its new value as an
        /// uppercase string.</remarks>
        /// <returns>A string representing the new angle mode after the toggle. Possible values are "DEG", "RAD", or "GRAD".</returns>
        public string ToggleAngleMode()
        {
            angleMode = angleMode switch
            {
                AngleMode.Deg => AngleMode.Rad,
                AngleMode.Rad => AngleMode.Grad,
                _ => AngleMode.Deg
            };
            return angleMode.ToString().ToUpper();
        }

        /// <summary>
        /// Converts the specified angle value to radians based on the current angle mode.
        /// </summary>
        /// <remarks>The conversion is determined by the current angle mode, which can be degrees,
        /// gradians, or radians. Ensure the angle mode is set appropriately before calling this method.</remarks>
        /// <param name="value">The angle value to convert. The interpretation of this value depends on the current angle mode.</param>
        /// <returns>The angle value converted to radians. If the angle mode is degrees, the value is converted from degrees to
        /// radians.  If the angle mode is gradians, the value is converted from gradians to radians. If the angle mode
        /// is already in radians, the value is returned unchanged.</returns>
        private double ToRadians(double value)
        {
            return angleMode switch
            {
                AngleMode.Deg => value * Math.PI / 180.0,
                AngleMode.Grad => value * Math.PI / 200.0,
                _ => value
            };
        }

        /// <summary>
        /// Converts an angle from radians to the current angle mode.
        /// </summary>
        /// <remarks>The conversion is based on the current angle mode, which determines the output unit.
        /// Ensure  that the angle mode is set appropriately before calling this method.</remarks>
        /// <param name="radians">The angle in radians to be converted.</param>
        /// <returns>The equivalent angle in the current angle mode. The result is in degrees if the angle mode  is <see
        /// cref="AngleMode.Deg"/>, in gradians if the angle mode is <see cref="AngleMode.Grad"/>,  or unchanged if the
        /// angle mode is radians.</returns>
        private double FromRadians(double radians)
        {
            return angleMode switch
            {
                AngleMode.Deg => radians * 180.0 / Math.PI,
                AngleMode.Grad => radians * 200.0 / Math.PI,
                _ => radians
            };
        }

        /// <summary>
        /// Handles the event triggered when a numeric button is clicked.
        /// </summary>
        /// <remarks>Updates the display to reflect the numeric value of the clicked button. If the button
        /// represents a decimal separator, ensures it is added only if the current number does not already
        /// contain one. Resets the display if a new number is being entered or if a calculation was just
        /// performed.</remarks>
        /// <param name="sender">The source of the event, typically the button that was clicked.</param>
        /// <param name="e">An <see cref="EventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Handles click events for operator buttons in a calculator application.
        /// </summary>
        /// <remarks>This method processes various operator inputs, such as arithmetic operations, special functions, and
        /// control commands. The behavior depends on the text of the clicked button, which determines the operation to perform.
        /// Examples of supported operations include addition, subtraction, square root, factorial, and logarithmic
        /// functions.</remarks>
        /// <param name="sender">The source of the event, typically a <see cref="Button"/> representing the operator.</param>
        /// <param name="e">The event data associated with the click event.</param>
        /// <exception cref="ArgumentException">Thrown when an invalid input is provided for certain operations, such as factorial, which requires a non-negative
        /// integer.</exception>
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

                case "π":
                    ApplySingleOperation(_ => Math.PI, "π", prefix: false);
                    break;

                case "e":
                    ApplySingleOperation(_ => Math.E, "e", prefix: true);
                    break;

                case "n!":
                    ApplySingleOperation(x =>
                    {
                        if (x < 0 || x != Math.Floor(x))
                            throw new ArgumentException("Invalid input");
                        double fact = 1;
                        for (int i = 1; i <= (int)x; i++)
                            fact *= i;
                        return fact;
                    }, "fact", prefix: true);
                    break;

                case "xʸ":
                    storedValue = double.Parse(resultEntry.Text.Replace(",", "."),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture);
                    currentOperator = "xʸ";
                    operationEntry.Text = $"{storedValue} ^";
                    isNewNumber = true;
                    break;

                case "10^x":
                    ApplySingleOperation(x => Math.Pow(10, x), "10^", prefix: true);
                    break;

                case "exp":
                    ApplySingleOperation(Math.Exp, "exp", prefix: true);
                    break;

                case "mod":
                    PerformPendingOperation();
                    currentOperator = "mod";
                    storedValue = currentValue;
                    operationEntry.Text = $"{storedValue} mod";
                    isNewNumber = true;
                    break;

                case "log":
                    ApplySingleOperation(Math.Log10, "log", prefix: true);
                    break;

                case "ln":
                    ApplySingleOperation(Math.Log, "ln", prefix: true);
                    break;

                case "|x|":
                    ApplySingleOperation(Math.Abs, "|x|", prefix: true);
                    break;

                case "x³":
                    ApplySingleOperation(x => Math.Pow(x, 3), "³", prefix: false);
                    break;

                case "³√x":
                    ApplySingleOperation(x =>
                    {
                        if (x < 0)
                            return -Math.Pow(Math.Abs(x), 1.0 / 3.0);
                        return Math.Pow(x, 1.0 / 3.0);
                    }, "³√", prefix: true);
                    break;

                case "ʸ√x":
                    storedValue = double.Parse(resultEntry.Text.Replace(",", "."),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture);
                    currentOperator = "ʸ√x";
                    operationEntry.Text = $"{storedValue}√";
                    isNewNumber = true;
                    break;

                case "2^x":
                    ApplySingleOperation(x => Math.Pow(2, x), "2^", prefix: true);
                    break;

                case "logy(x)":
                    storedValue = double.Parse(resultEntry.Text.Replace(",", "."),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture);
                    currentOperator = "logy(x)";
                    operationEntry.Text = $"log{storedValue}(x)";
                    isNewNumber = true;
                    break;

                case "e^x":
                    ApplySingleOperation(x => Math.Exp(x), "e^", prefix: true);
                    break;

                default:
                    break;

            }
        }

        /// <summary>
        /// Handles memory-related operations triggered by button clicks in a calculator application.
        /// </summary>
        /// <remarks>This method processes memory operations based on the text of the clicked button:
        /// <list type="bullet"> <item><term>"MC"</term> - Clears the memory.</item> <item><term>"MR"</term> - Recalls
        /// the stored memory value and displays it, or shows an alert if memory is empty.</item>
        /// <item><term>"MS"</term> - Stores the current value in memory.</item> <item><term>"M+"</term> - Adds the
        /// current value to the stored memory value, or initializes memory if empty.</item> <item><term>"M-"</term> -
        /// Subtracts the current value from the stored memory value, or initializes memory if empty.</item>
        /// <item><term>"M↓"</term> - Displays the stored memory value, or shows an alert if memory is empty.</item>
        /// </list> If the input value is invalid or empty, it defaults to 0.</remarks>
        /// <param name="sender">The source of the event, expected to be a <see cref="Button"/>.</param>
        /// <param name="e">The event data associated with the click event.</param>
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

            }
        }

        /// <summary>
        /// Performs a pending mathematical operation based on the current operator and input value.
        /// </summary>
        /// <remarks>This method evaluates the operation specified by the current operator and updates the
        /// result accordingly. Supported operations include addition, subtraction, multiplication,  division,
        /// exponentiation, modulus, root calculations, and logarithms. If the operation  is invalid (e.g., division by
        /// zero or invalid logarithmic inputs), an error message is  displayed in the result entry.</remarks>
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
                case "xʸ":
                    currentValue = Math.Pow(storedValue, newValue);
                    break;

                case "mod":
                    currentValue = storedValue % newValue;
                    break;

                case "ʸ√x":
                    currentValue = Math.Pow(newValue, 1.0 / storedValue);
                    break;

                case "logy(x)":
                    if (storedValue <= 0 || storedValue == 1 || newValue <= 0)
                    {
                        resultEntry.Text = "Invalid input";
                        return;
                    }
                    currentValue = Math.Log(newValue) / Math.Log(storedValue);
                    break;

                default:
                    break;

            }

            resultEntry.Text = currentValue.ToString();
        }

        /// <summary>
        /// Applies a mathematical operation to the current value and updates the result display.
        /// </summary>
        /// <remarks>This method validates the input value based on the operation symbol and handles
        /// specific cases such as division by zero,  invalid inputs for square root of negative numbers, logarithms of
        /// non-positive numbers, and trigonometric inverse functions  with out-of-range values. If the input is
        /// invalid, an appropriate error message is displayed, and the operation is not performed.</remarks>
        /// <param name="operation">A function that defines the mathematical operation to apply. The function takes a single <see
        /// cref="double"/> input and returns a <see cref="double"/> result.</param>
        /// <param name="symbol">A string representing the symbol of the operation (e.g., "√" for square root, "1/" for reciprocal).</param>
        /// <param name="prefix">A boolean value indicating whether the operation symbol should be displayed as a prefix (e.g., "√(value)") 
        /// or a suffix (e.g., "value√") in the operation display.</param>
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

            else if (symbol == "√" && value < 0)
            {
                resultEntry.Text = "Invalid input";
                isNewNumber = true;
                hasJustCalculated = true;
                return;
            }

            else if (symbol == "fact" && (value < 0 || value != Math.Floor(value)))
            {
                resultEntry.Text = "Invalid input";
                isNewNumber = true;
                hasJustCalculated = true;
                return;
            }

            else if (symbol == "ln" && value <= 0)
            {
                resultEntry.Text = "Invalid input";
                isNewNumber = true;
                hasJustCalculated = true;
                return;
            }

            else if ((symbol == "log" || symbol == "log10") && value <= 0)
            {
                resultEntry.Text = "Invalid input";
                isNewNumber = true;
                hasJustCalculated = true;
                return;
            }

            else if ((symbol == "sin^-1" || symbol == "cos^-1" || symbol == "csc^-1" || symbol == "sec^-1") && (value < -1 || value > 1))
            {
                resultEntry.Text = "Invalid input";
                isNewNumber = true;
                hasJustCalculated = true;
                return;
            }

            double newValue = operation(value);

            currentValue = newValue;
            resultEntry.Text = currentValue.ToString();

            if (symbol == "π" || symbol == "e")
                operationEntry.Text = symbol;
            else

            if (prefix)
                operationEntry.Text = $"{symbol}({value})";
            else
                operationEntry.Text = $"{value}{symbol}";

            isNewNumber = true;
            hasJustCalculated = true;
        }

        /// <summary>
        /// Applies a percentage operation based on the current operator and updates the result.
        /// </summary>
        /// <remarks>This method interprets the value in the result entry as a percentage and performs a
        /// calculation  using the stored value and the current operator. Supported operators include addition,
        /// subtraction,  multiplication, and division. If no operator is specified, the method treats the value as a
        /// percentage  of 100 and updates the result accordingly.</remarks>
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

        /// <summary>
        /// Toggles the sign of the numeric value displayed in the result entry field.
        /// </summary>
        /// <remarks>This method updates the text in the result entry field to reflect the negated value.  If the current
        /// text is "0", "0," or empty, the method does nothing.  The method ensures proper parsing and formatting of the
        /// numeric value, including handling culture-specific decimal separators.</remarks>
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

        /// <summary>
        /// Performs the specified trigonometric or hyperbolic operation on the current input value.
        /// </summary>
        /// <remarks>The input value is derived from the current text in the result entry. If the input is
        /// invalid or empty, it defaults to <c>0</c>. The method applies the specified function to the input value and
        /// updates the result accordingly. <para> For trigonometric functions, the input is interpreted as degrees. For
        /// hyperbolic functions, the input is interpreted as-is. </para> <para> If the operation results in an invalid
        /// state (e.g., division by zero), an error message is displayed to the user. </para></remarks>
        /// <param name="func">The name of the trigonometric or hyperbolic function to apply. Supported values include: <list
        /// type="bullet"> <item><description>Trigonometric functions: <c>"sin"</c>, <c>"cos"</c>, <c>"tan"</c>,
        /// <c>"cot"</c>, <c>"csc"</c>, <c>"sec"</c>.</description></item> <item><description>Inverse trigonometric
        /// functions: <c>"sin^-1"</c>, <c>"cos^-1"</c>, <c>"tan^-1"</c>, <c>"cot^-1"</c>, <c>"csc^-1"</c>,
        /// <c>"sec^-1"</c>.</description></item> <item><description>Hyperbolic functions: <c>"sinh"</c>, <c>"cosh"</c>,
        /// <c>"tanh"</c>, <c>"coth"</c>, <c>"csch"</c>, <c>"sech"</c>.</description></item> <item><description>Inverse
        /// hyperbolic functions: <c>"sinh^-1"</c>, <c>"cosh^-1"</c>, <c>"tanh^-1"</c>, <c>"coth^-1"</c>,
        /// <c>"csch^-1"</c>, <c>"sech^-1"</c>.</description></item> </list></param>
        /// <exception cref="ArgumentException">Thrown if the specified function results in an invalid operation, such as division by zero.</exception>
        public void PerformTrigFunction(string func)
        {
            string text = resultEntry?.Text?.Trim();
            if (string.IsNullOrEmpty(text) || text == "," || text == "-")
                text = "0";

            if (!double.TryParse(text.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double value))
            {
                value = 0;
            }

            var page = Application.Current?.Windows[0].Page;

            try
            {
                switch (func)
                {
                    case "sin":
                        ApplySingleOperation(x => Math.Sin(ToRadians(x)), "sin", prefix: true);
                        break;
                    case "cos":
                        ApplySingleOperation(x => Math.Cos(ToRadians(x)), "cos", prefix: true);
                        break;
                    case "tan":
                        ApplySingleOperation(x => Math.Tan(ToRadians(x)), "tan", prefix: true);
                        break;
                    case "cot":
                        ApplySingleOperation(x =>
                        {
                            double t = Math.Tan(ToRadians(x));
                            if (t == 0) throw new ArgumentException("Invalid Argument");
                            return 1.0 / t;
                        }, "cot", prefix: true);
                        break;
                    case "csc":
                        ApplySingleOperation(x =>
                        {
                            double s = Math.Sin(ToRadians(x));
                            if (s == 0) throw new ArgumentException("Invalid Argument");
                            return 1.0 / s;
                        }, "csc", prefix: true);
                        break;
                    case "sec":
                        ApplySingleOperation(x =>
                        {
                            double c = Math.Cos(ToRadians(x));
                            if (c == 0) throw new ArgumentException("Invalid Argument");
                            return 1.0 / c;
                        }, "sec", prefix: true);
                        break;

                    case "sin^-1":
                        ApplySingleOperation(x => FromRadians(Math.Asin(x)), "sin^-1", prefix: true);
                        break;
                    case "cos^-1":
                        ApplySingleOperation(x => FromRadians(Math.Acos(x)), "cos^-1", prefix: true);
                        break;
                    case "tan^-1":
                        ApplySingleOperation(x => FromRadians(Math.Atan(x)), "tan^-1", prefix: true);
                        break;
                    case "cot^-1":
                        ApplySingleOperation(x => FromRadians(Math.Atan(1.0 / x)), "cot^-1", prefix: true);
                        break;
                    case "csc^-1":
                        ApplySingleOperation(x => FromRadians(Math.Asin(1.0 / x)), "csc^-1", prefix: true);
                        break;
                    case "sec^-1":
                        ApplySingleOperation(x => FromRadians(Math.Acos(1.0 / x)), "sec^-1", prefix: true);
                        break;

                    case "sinh":
                        ApplySingleOperation(Math.Sinh, "sinh", prefix: true);
                        break;
                    case "cosh":
                        ApplySingleOperation(Math.Cosh, "cosh", prefix: true);
                        break;
                    case "tanh":
                        ApplySingleOperation(Math.Tanh, "tanh", prefix: true);
                        break;
                    case "coth":
                        ApplySingleOperation(x =>
                        {
                            double t = Math.Tanh(x);
                            if (t == 0) throw new ArgumentException("Invalid Argument");
                            return 1.0 / t;
                        }, "coth", prefix: true);
                        break;
                    case "csch":
                        ApplySingleOperation(x =>
                        {
                            double s = Math.Sinh(x);
                            if (s == 0) throw new ArgumentException("Invalid Argument");
                            return 1.0 / s;
                        }, "csch", prefix: true);
                        break;
                    case "sech":
                        ApplySingleOperation(x =>
                        {
                            double c = Math.Cosh(x);
                            if (c == 0) throw new ArgumentException("Invalid Argument");
                            return 1.0 / c;
                        }, "sech", prefix: true);
                        break;

                    case "sinh^-1":
                        ApplySingleOperation(Math.Asinh, "asinh", prefix: true);
                        break;
                    case "cosh^-1":
                        ApplySingleOperation(Math.Acosh, "acosh", prefix: true);
                        break;
                    case "tanh^-1":
                        ApplySingleOperation(Math.Atanh, "atanh", prefix: true);
                        break;
                    case "coth^-1":
                        ApplySingleOperation(x => 0.5 * Math.Log((1 + 1.0 / x) / (1 - 1.0 / x)), "acoth", prefix: true);
                        break;
                    case "csch^-1":
                        ApplySingleOperation(x => Math.Log(1.0 / x + Math.Sqrt(1.0 / (x * x) + 1)), "acsch", prefix: true);
                        break;
                    case "sech^-1":
                        ApplySingleOperation(x => Math.Log((1.0 / x) + Math.Sqrt(1.0 / (x * x) - 1)), "asech", prefix: true);
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                page?.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        /// <summary>
        /// Performs a mathematical or conversion operation based on the specified function identifier.
        /// </summary>
        /// <remarks>If the input value is invalid or cannot be parsed, it defaults to 0. The result of
        /// the operation is displayed in the appropriate UI elements, and certain operations may update the result in a
        /// specific format (e.g., DMS).</remarks>
        /// <param name="func">A string representing the operation to perform. Supported values include: <list type="bullet">
        /// <item><description><c>"|x|"</c>: Calculates the absolute value of the input.</description></item>
        /// <item><description><c>"⌊x⌋"</c>: Rounds the input down to the nearest integer (floor).</description></item>
        /// <item><description><c>"⌈x⌉"</c>: Rounds the input up to the nearest integer (ceiling).</description></item>
        /// <item><description><c>"rand"</c>: Generates a random number between 0 and 1.</description></item>
        /// <item><description><c>"→dms"</c>: Converts a decimal degree value to degrees, minutes, and seconds (DMS)
        /// format.</description></item> <item><description><c>"→deg"</c>: Converts a DMS value back to decimal
        /// degrees.</description></item> </list></param>
        public void PerformFunction(string func)
        {
            string text = resultEntry?.Text?.Trim();
            if (string.IsNullOrEmpty(text) || text == "," || text == "-")
                text = "0";

            if (!double.TryParse(text.Replace(",", "."),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double value))
            {
                value = 0;
            }


            double result = value;
            string displaySymbol = func;

            var page = Application.Current?.Windows[0].Page;

            try
            {
                switch (func)
                {
                    case "|x|":
                        result = Math.Abs(value);
                        operationEntry.Text = $"|{value}|";
                        break;

                    case "⌊x⌋":
                        result = Math.Floor(value);
                        operationEntry.Text = $"⌊{value}⌋";
                        break;

                    case "⌈x⌉":
                        result = Math.Ceiling(value);
                        operationEntry.Text = $"⌈{value}⌉";
                        break;

                    case "rand":
                        Random rnd = new();
                        result = rnd.NextDouble();
                        operationEntry.Text = "rand()";
                        break;

                    case "→dms":
                        int degrees = (int)value;
                        double minutesFull = (Math.Abs(value - degrees)) * 60;
                        int minutes = (int)minutesFull;
                        double seconds = (minutesFull - minutes) * 60;
                        operationEntry.Text = $"{value}° → DMS";
                        resultEntry!.Text = $"{degrees}° {minutes}' {seconds:0.##}\"";
                        return;

                    case "→deg":

                        double deg = Math.Truncate(value);
                        double min = (value - deg) * 100;
                        double dVal = deg + (min / 60.0);
                        result = dVal;
                        operationEntry.Text = $"{value} → °";
                        break;

                    default:
                        break;
                }

                resultEntry.Text = result.ToString(System.Globalization.CultureInfo.InvariantCulture).Replace(".", ",");
                currentValue = result;
                storedValue = result;
                isNewNumber = true;
                hasJustCalculated = true;
            }
            catch (Exception ex)
            {
                page?.DisplayAlert("Function Error", ex.Message, "OK");
            }
        }

        /// <summary>
        /// Resets all calculator state to its initial values.
        /// </summary>
        /// <remarks>This method clears the current and stored values, resets the operator, and prepares
        /// the calculator  for a new operation. It also resets the display entries to their default states.</remarks>
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
