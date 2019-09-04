using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CAIS_System
{
    static class ErrorHandler
    {
        public static void ErrorHandling(Exception exception)
        {
            // Сообщение об ошибке без логгирования
            MessageBox.Show(
                "Ошибка в " + exception.TargetSite + "\n Описание: " + exception.Message + "\nСтек вызовов: " + exception.StackTrace,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

        }

        public static void ErrorHandling(Exception exception, bool isLogging)
        {
            //сообщение об ошибке и запись в логи (TODO)
            MessageBox.Show(
                "Ошибка в " + exception.TargetSite + "\n Описание: " + exception.Message + "\nСтек вызовов: " + exception.StackTrace,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            if (isLogging)
            {

            }
        }
    }
}
