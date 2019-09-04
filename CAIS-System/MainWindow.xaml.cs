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
        public MainWindow()
        {
            InitializeComponent();
        }
        ~MainWindow()
        {
            if(smev != null) smev.CloseChannel();
        }
        private void MainWindow_Load (object sender, EventArgs e)
        {
            smev.OpenChannel();
        }
        private void MainWindow_Dispatch (object sender, EventArgs e)
        {

        }
        
        private async void SendRequestButton_Click(object sender, RoutedEventArgs e)
        {
            
            SmevExchange.SendRequestResponse response = new SmevExchange.SendRequestResponse();
            try
            {
                response = await smev.SendMessage();
            }
            catch(Exception exception)
            {
                ErrorHandler.ErrorHandling(exception);
            }
            finally
            {
                if (response != null)
                {
                    response.ToString();
                }
            }
            
        }
    }
}
