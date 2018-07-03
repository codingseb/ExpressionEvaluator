using CodingSeb.ExpressionEvaluator;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace TryWindow
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string persistFileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "code.cs");

        private CancellationTokenSource cancellationTokenSource = null;

        public MainWindow()
        {
            InitializeComponent();

            if (File.Exists(persistFileName))
                ScriptTextBox.Text = File.ReadAllText(persistFileName);
        }

        private async void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            CalculateButton.IsEnabled = false;
            CancelButton.IsEnabled = true;

            ExpressionEvaluator evaluator = new ExpressionEvaluator();

            evaluator.Namespaces.Add("System.Windows");

            evaluator.EvaluateVariable += Evaluator_EvaluateVariable;

            Stopwatch stopWatch = new Stopwatch();
            

            try
            {
                string script = evaluator.RemoveComments(ScriptTextBox.Text);
                Exception exception = null;
                cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.Token.ThrowIfCancellationRequested();
                string result = await Task.Run<string>(() => 
                {
                    stopWatch.Start();
                    try
                    {
                        using (cancellationTokenSource.Token.Register(Thread.CurrentThread.Abort))
                        {
                            return evaluator.ScriptEvaluate(script)?.ToString() ?? "null or void";
                        }
                    }
                    catch(Exception innerException)
                    {
                        exception = innerException;
                        return null;
                    }
                    finally
                    {
                        stopWatch.Stop();
                    }
                }, cancellationTokenSource.Token);

                if (exception == null)
                    ResultTextBlock.Text = result;
                else
                    throw exception;
            }
            catch(Exception exception)
            {
                ResultTextBlock.Text = exception.Message;
            }

            ExecutionTimeTextBlock.Text = $"Execution time : {stopWatch.Elapsed}";

            evaluator.EvaluateVariable -= Evaluator_EvaluateVariable;

            CalculateButton.IsEnabled = true;
            CancelButton.IsEnabled = false;
        }

        private void Evaluator_EvaluateVariable(object sender, VariableEvaluationEventArg e)
        {
            if(e.This != null && e.Name.Equals("Json"))
            {
                e.Value = JsonConvert.SerializeObject(e.This); 
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (cancellationTokenSource != null)
                cancellationTokenSource.Cancel();
        }

        private void ScriptTextBox_TextChanged(object sender, EventArgs e)
        {
            Save();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Save();
        }

        private void Save()
        {
            try
            {
                File.WriteAllText(persistFileName, ScriptTextBox.Text);
            }
            catch { }
        }
    }
}
