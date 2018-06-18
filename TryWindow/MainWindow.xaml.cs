using CodingSeb.ExpressionEvaluator;
using Newtonsoft.Json;
using System;
using System.Windows;

namespace TryWindow
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.EvaluateVariable += Evaluator_EvaluateVariable;

            try
            {
                ResultTextBlock.Text = evaluator.ScriptEvaluate(ScriptTextBox.Text).ToString();
            }
            catch(Exception exception)
            {
                ResultTextBlock.Text = exception.Message;
            }

            evaluator.EvaluateVariable -= Evaluator_EvaluateVariable;
        }

        private void Evaluator_EvaluateVariable(object sender, VariableEvaluationEventArg e)
        {
            if(e.This != null && e.Name.Equals("Json"))
            {
                e.Value = JsonConvert.SerializeObject(e.This); 
            }
        }
    }
}
