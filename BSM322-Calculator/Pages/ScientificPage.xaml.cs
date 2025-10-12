using BSM322_Calculator.Services;

namespace BSM322_Calculator.Pages;

public partial class ScientificPage : ContentPage
{
    private readonly CalculatorService calculator;

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
        var button = (Button)sender;
        if (button.Text == "DEG")
        {
            button.Text = "RAD";

        }
        else if (button.Text == "RAD")
        {
            button.Text = "GRAD";
        }
        else if (button.Text == "GRAD")
        {
            button.Text = "DEG";
        }
    }

    private void FEClickEvent(object sender, EventArgs e)
    {

    }
}