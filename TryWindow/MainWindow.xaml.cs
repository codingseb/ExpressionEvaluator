using CodingSeb.ExpressionEvaluator;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
        private string persistCodeFileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "code.cs");
        private string persistIterationFileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "iterations");

        private CancellationTokenSource cancellationTokenSource = null;

        public MainWindow()
        {
            InitializeComponent();

            if (File.Exists(persistCodeFileName))
                ScriptTextBox.Text = File.ReadAllText(persistCodeFileName);
            if (File.Exists(persistIterationFileName))
                IterationsTextBox.Text = File.ReadAllText(persistIterationFileName);
        }

        private async void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            CalculateButton.IsEnabled = false;
            CancelButton.IsEnabled = true;

            ExpressionEvaluator evaluator = new ExpressionEvaluator()
            {
                OptionScriptNeedSemicolonAtTheEndOfLastExpression = NeedSemicolonAtTheEndCheckBox.IsChecked.GetValueOrDefault(),
            };

            if (UseCachesCheckbox.IsChecked ?? false)
                evaluator.CacheTypesResolutions = true;

            evaluator.Namespaces.Add("System.Windows");
            evaluator.Namespaces.Add("System.Diagnostics");

            evaluator.EvaluateVariable += Evaluator_EvaluateVariable;
            evaluator.EvaluateFunction += Evaluator_EvaluateFunction;

            Stopwatch stopWatch = new Stopwatch();

            try
            {
                string script = evaluator.RemoveComments(ScriptTextBox.Text);
                string sIteration = IterationsTextBox.Text;
                Exception exception = null;
                cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.Token.ThrowIfCancellationRequested();
                string result = await Task.Run(() =>
                {
                    if (!int.TryParse(sIteration, out int iterations))
                        iterations = 1;

                    iterations = Math.Max(0, iterations);

                    stopWatch.Start();
                    try
                    {
                        using (cancellationTokenSource.Token.Register(Thread.CurrentThread.Abort))
                        {
                            string innerResult = "null or void";

                            for (int i = 0; i < iterations; i++)
                                innerResult = evaluator.ScriptEvaluate(script)?.ToString() ?? "null or void";

                            return innerResult;
                        }
                    }
                    catch (Exception innerException)
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
            catch (Exception exception)
            {
                ResultTextBlock.Text = exception.Message;
            }

            ExecutionTimeTextBlock.Text = $"Execution time : {stopWatch.Elapsed}";

            evaluator.EvaluateVariable -= Evaluator_EvaluateVariable;

            CalculateButton.IsEnabled = true;
            CancelButton.IsEnabled = false;
        }

        private void Evaluator_EvaluateFunction(object sender, FunctionEvaluationEventArg e)
        {
            
        }

        private void Evaluator_EvaluateVariable(object sender, VariableEvaluationEventArg e)
        {
            if (e.This != null)
            {
                if (e.Name.Equals("Json"))
                {
                    e.Value = JsonConvert.SerializeObject(e.This);
                }
                else if (e.Name.Equals("MethodsNames"))
                {
                    e.Value = JsonConvert.SerializeObject(e.This.GetType().GetMethods().Select(m => m.Name));
                }
                else if (e.Name.Equals("PropertiesNames"))
                {
                    e.Value = JsonConvert.SerializeObject(e.This.GetType().GetProperties().Select(m => m.Name));
                }
                else if (e.Name.Equals("FieldsNames"))
                {
                    e.Value = JsonConvert.SerializeObject(e.This.GetType().GetFields().Select(m => m.Name));
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (cancellationTokenSource != null)
                cancellationTokenSource.Cancel();
        }

        private void ScriptTextBox_TextChanged(object sender, EventArgs e)
        {
            SaveCode();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveCode();
            SaveIterations();
        }

        private void SaveIterations()
        {
            try
            {
                File.WriteAllText(persistIterationFileName, IterationsTextBox.Text);
            }
            catch { }
        }

        private void SaveCode()
        {
            try
            {
                File.WriteAllText(persistCodeFileName, ScriptTextBox.Text);
            }
            catch { }
        }
    }
}
