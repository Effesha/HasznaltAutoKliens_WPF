using System.Windows.Controls;
using System.Windows.Media;

namespace HasznaltAuto.Desktop.Helpers;

public static class MessageHelper
{
    public static void Error(ref Label resultMessage, string message)
    {
        resultMessage.Foreground = Brushes.Red;
        resultMessage.Content = message;
    }

    public static void Success(ref Label resultMessage, string message)
    {
        resultMessage.Foreground = Brushes.Green;
        resultMessage.Content = message;
    }

    public static void Default(ref Label resultMessage)
    {
        resultMessage.Foreground = Brushes.Gray;
        resultMessage.Content = "Messages regarding the outcome of above operations will be displayed here.";
    }
}
