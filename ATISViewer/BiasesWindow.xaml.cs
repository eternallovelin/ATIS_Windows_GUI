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
using System.Windows.Shapes;
using OpalKelly.FrontPanel;

namespace ATISViewer
{
    /// <summary>
    /// Interaction logic for BiasesWindow.xaml
    /// </summary>
    public partial class BiasesWindow : Window
    {
        //private MainWindow mainWidow;
        public BiasesWindow()
        {

            
            InitializeComponent();
            BiasesInterface.BiasName bname = 0;
            //foreach (BiasesInterface.BiasName bname in BiasesInterface.BiasName)
            for (byte i = 0; i < 26; i++)
            {
                var newPanel = new DockPanel();
                newPanel.Margin = new Thickness(5);
                var newSlider = new Slider()
                {
                    Maximum = BiasesInterface.BiasMaxVoltage[bname],
                    Value = BiasesInterface.BiasVoltage[bname],
                    SmallChange = 1,
                    LargeChange = 10,
                    Width = 100,
                    Name = BiasesInterface.stringNames[bname]
                };
                var newLabel = new Label();
                var valueLabel = new Label();

                newLabel.Width = 100;
                newLabel.SetValue(DockPanel.DockProperty, Dock.Right);
                newLabel.Content = BiasesInterface.stringNames[bname];

                valueLabel.SetValue(DockPanel.DockProperty, Dock.Left);
                valueLabel.Width = 40;
                valueLabel.Tag = "value";
                valueLabel.Content = BiasesInterface.BiasVoltage[bname].ToString();


                newSlider.ValueChanged += newSlider_ValueChanged;
                newSlider.Tag = i;

                newPanel.Children.Add(newSlider);
                newPanel.Children.Add(newLabel);
                newPanel.Children.Add(valueLabel);

                sliderPanel.Children.Add(newPanel);
                bname++;
            }
        }

        void newSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = (Slider)sender;
            var panel = (DockPanel)slider.Parent;

            foreach (var cntrl in panel.Children)
            {
                if ((cntrl.GetType() == typeof(Label)))
                {
                    var label = (Label)cntrl;
                    if ((string)label.Tag == "value")
                    {
                        label.Content = Math.Round(e.NewValue).ToString();
                    }
                }
            }
            BiasesInterface.ModifyBias((BiasesInterface.BiasName)(Enum.Parse(typeof(BiasesInterface.BiasName), slider.Name)), (int)Math.Floor(e.NewValue));
            BiasesInterface.programDACs();
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ControlWindow.close_Biases();
        }
    }
}
