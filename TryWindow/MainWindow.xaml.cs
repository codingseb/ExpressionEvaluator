using CodingSeb.ExpressionEvaluator;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace TryWindow
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string persistFileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "code.cs");

        public MainWindow()
        {
            InitializeComponent();

            if (File.Exists(persistFileName))
                ScriptTextBox.Text = File.ReadAllText(persistFileName);
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.Namespaces.Add("System.Windows");

            evaluator.EvaluateVariable += Evaluator_EvaluateVariable;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            try
            {
                ResultTextBlock.Text = evaluator.ScriptEvaluate(ScriptTextBox.Text).ToString();
            }
            catch(Exception exception)
            {
                ResultTextBlock.Text = exception.Message;
            }

            stopWatch.Stop();
            ExecutionTimeTextBlock.Text = $"Execution time : {stopWatch.Elapsed}";

            evaluator.EvaluateVariable -= Evaluator_EvaluateVariable;
        }

        private void Evaluator_EvaluateVariable(object sender, VariableEvaluationEventArg e)
        {
            if(e.This != null && e.Name.Equals("Json"))
            {
                e.Value = JsonConvert.SerializeObject(e.This); 
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                File.WriteAllText(persistFileName, ScriptTextBox.Text);
            }
            catch { }
        }
    }
}
