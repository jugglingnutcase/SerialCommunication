using ReactiveUI;
using ReactiveUI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
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
using System.IO.Ports;

namespace SerialCommunication {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IViewFor<MainWindowViewModel> {
        private SerialPort _serialPort;

        public MainWindow() {
            ViewModel = new MainWindowViewModel();
            InitializeComponent();

            _serialPort = new SerialPort("COM4", 9600);
            _serialPort.Open();

            this.Bind(ViewModel, x => x.Red);
            this.Bind(ViewModel, x => x.Green);
            this.Bind(ViewModel, x => x.Blue);

            this.Bind(ViewModel, x => x.SaveText, x => x.Save.Content);

            this.BindCommand(ViewModel, x => x.SaveColor, x => x.Save);

            this.OneWayBind(ViewModel, x => x.FinalColor, x => x.FinalColor.Background);

            // And output some stuff to the serial port
            this.WhenAny(x => x.ViewModel.FinalColor, x => x.Value)
                .Where(x => x != null)
                .Throttle(TimeSpan.FromSeconds(0.3), RxApp.DeferredScheduler)
                .Subscribe(x => _serialPort.WriteLine(String.Format("33,{0},{1},{2}", x.Color.R, x.Color.G, x.Color.B)));
            
            var currentLED = 0;
            ViewModel.SaveText = String.Format("Save to LED {0}", currentLED);

            ViewModel.SaveColor.Subscribe(x =>
            {
                Debug.Print("{0},{1},{2},{3}", currentLED, ViewModel.FinalColor.Color.R, ViewModel.FinalColor.Color.G, ViewModel.FinalColor.Color.B);
                _serialPort.WriteLine(String.Format("{0},{1},{2},{3}", currentLED, ViewModel.FinalColor.Color.R, ViewModel.FinalColor.Color.G, ViewModel.FinalColor.Color.B));
                
                currentLED += 1;
                if (currentLED >= 32)
                {
                    currentLED = 0;
                }

                // Update the text to show the next Save
                ViewModel.SaveText = String.Format("Save to LED {0}", currentLED);
            });
        }

        public MainWindowViewModel ViewModel {
            get { return (MainWindowViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(MainWindowViewModel), typeof(MainWindow), new PropertyMetadata(null));

        object IViewFor.ViewModel {
            get { return ViewModel; }
            set { ViewModel = (MainWindowViewModel)value; }
        }
    }
}
