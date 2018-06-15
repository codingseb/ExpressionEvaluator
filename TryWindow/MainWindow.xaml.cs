using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CodingSeb.ExpressionEvaluator;
using Newtonsoft.Json;

namespace TryWindow
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ExpressionEvaluator evaluator = new ExpressionEvaluator();

        public MainWindow()
        {
            InitializeComponent();
            evaluator.ScriptModeActive = true;
            evaluator.EvaluateVariable += Evaluator_EvaluateVariable;
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ResultTextBlock.Text = evaluator.Evaluate(ScriptTextBox.Text).ToString();
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
