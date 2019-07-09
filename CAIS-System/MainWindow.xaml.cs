using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

namespace CAIS_System
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NodeSmev smev = new NodeSmev();
        private static StringBuilder cmdOutput = null;
        static string outCmdInfo;
        Process cmdProcess;
        StreamWriter cmdStreamWriter;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void MainWindow_Load (object sender, EventArgs e)
        {
           
        }
        private void MainWindow_Dispatch (object sender, EventArgs e)
        {
            cmdStreamWriter.Close();
            cmdProcess.WaitForExit();
            cmdProcess.Close();
        }
        private static void SortOutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                cmdOutput.Append(Environment.NewLine + outLine.Data);
                outCmdInfo = outLine.Data;
                MessageBox.Show(outCmdInfo);
            }
            
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            cmdOutput = new StringBuilder("");
            cmdProcess = new Process();

            cmdProcess.StartInfo.FileName = "cmd.exe";
            cmdProcess.StartInfo.UseShellExecute = false;
            cmdProcess.StartInfo.CreateNoWindow = true;
            cmdProcess.StartInfo.RedirectStandardOutput = true;

            cmdProcess.OutputDataReceived += new DataReceivedEventHandler(SortOutputHandler);
            cmdProcess.StartInfo.RedirectStandardInput = true;
            cmdProcess.Start();

            cmdStreamWriter = cmdProcess.StandardInput;
            cmdProcess.BeginOutputReadLine();
            cmdStreamWriter.WriteLine("identitytool.bat");
            
            smev.SendMessage(outCmdInfo);

        }
    }
}
