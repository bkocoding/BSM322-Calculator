using BSM322_Calculator.Services;

namespace BSM322_Calculator.Pages;

public partial class ScientificPage : ContentPage
{
    private readonly CalculatorService calculator;

    private bool isSecondMode = false;

    public ScientificPage()
	{
		InitializeComponent();
        calculator = new CalculatorService(OperationEntry, ResultEntry);
    }

    private void NumberClickEvent(object sender, EventArgs e)
    {
        calculator.NumberClickEvent(sender, e);
    }

    private void OperatorClickEvent(object sender, EventArgs e)
    {
        calculator.OperatorClickEvent(sender, e);
    }

    private void MemoryClickEvent(object sender, EventArgs e)
    {
        calculator.MemoryClickEvent(sender, e);
    }

    private void DegClickEvent(object sender, EventArgs e)
    {
        var btn = (Button)sender;
        var mode = calculator.ToggleAngleMode();
        btn.Text = mode;
    }

    private void FEClickEvent(object sender, EventArgs e)
    {
        var btn = (Button)sender;
    }

    private void SecondButtonClickEvent(object sender, EventArgs e)
    {
        isSecondMode = !isSecondMode;

        foreach (var child in MainMatrixGrid.Children)
        {
            if (child is Button btn)
            {
                int column = Grid.GetColumn(btn);
                if (column != 0) continue;

                if (btn.Text == "2nd") continue;

                if (isSecondMode)
                {
                    switch (btn.Text)
                    {
                        case "x²":
                            btn.Text = "x³";
                            break;
                        case "√":
                            btn.Text = "³√x";
                            break;
                        case "xʸ":
                            btn.Text = "ʸ√x";
                            break;
                        case "10^x":
                            btn.Text = "2^x";
                            break;
                        case "log":
                            btn.Text = "logy(x)";
                            break;
                        case "ln":
                            btn.Text = "e^x";
                            break;
                    }
                }
                else
                {
                    switch (btn.Text)
                    {
                        case "x³":
                            btn.Text = "x²";
                            break;
                        case "³√x":
                            btn.Text = "√";
                            break;
                        case "ʸ√x":
                            btn.Text = "xʸ";
                            break;
                        case "2^x":
                            btn.Text = "10^x";
                            break;
                        case "logy(x)":
                            btn.Text = "log";
                            break;
                        case "e^x":
                            btn.Text = "ln";
                            break;
                    }
                }
            }
        }

        if (sender is Button btn2nd)
        {
            var resources = Application.Current!.Resources;

            if (isSecondMode)
            {
                btn2nd.BackgroundColor = (Color)resources["Secondary"];
                btn2nd.TextColor = (Color)resources["SecondaryDarkText"];
            }
            else
            {
                btn2nd.BackgroundColor = (Color)resources["PrimaryDark"];
                btn2nd.TextColor = (Color)resources["PrimaryDarkText"];
            }
        }
    }

    private void ResultEntryTextChanged(object sender, TextChangedEventArgs e)
    {
        var entry = (Entry)sender;
        if (entry.Text == "0" || string.IsNullOrEmpty(entry.Text)) {
            ClearButton.Text = "C";
        }
        else {
            ClearButton.Text = "CE";
        }
    }

    private void TrigPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (sender is not Picker p) return;
        if (p.SelectedItem == null) return;

        string selected = p.SelectedItem.ToString() ?? "";
        if (!string.IsNullOrWhiteSpace(selected))
        {
            calculator.PerformTrigFunction(selected);
        }

        p.SelectedItem = null;

    }

    private void FuncPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (sender is not Picker p) return;
        if (p.SelectedItem == null) return;

        string selected = p.SelectedItem.ToString() ?? "";
        if (!string.IsNullOrWhiteSpace(selected))
        {
            calculator.PerformFunction(selected);
        }

        p.SelectedItem = null;
    }
}