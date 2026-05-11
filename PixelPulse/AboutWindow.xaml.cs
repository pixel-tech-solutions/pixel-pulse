using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace PixelPulse;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Email_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Email functionality disabled for offline operation
    }

    private void Website_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Website functionality disabled for offline operation
    }
}
