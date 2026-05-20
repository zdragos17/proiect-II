using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace Proiect
{
    public static class WindowNavigationHelper
    {
        public static void NavigateWithFade(Window currentWindow, Window nextWindow)
        {
            nextWindow.WindowState = WindowState.Maximized;
            nextWindow.Opacity = 0;

            nextWindow.Show();

            DoubleAnimation fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(150)
            };

            nextWindow.BeginAnimation(Window.OpacityProperty, fadeIn);

            currentWindow.Close();
        }
    }
}