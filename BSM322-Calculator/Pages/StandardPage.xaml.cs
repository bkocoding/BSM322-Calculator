using BSM322_Calculator.Services;

namespace BSM322_Calculator.Pages;

public partial class StandardPage : ContentPage
{
    private readonly CalculatorService calculator;

    public StandardPage()
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

}
