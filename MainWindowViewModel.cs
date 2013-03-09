using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ReactiveUI;
using ReactiveUI.Xaml;

namespace SerialCommunication {
    public class MainWindowViewModel : ReactiveObject {
        int _Red;
        public int Red {
            get { return _Red; }
            set { this.RaiseAndSetIfChanged(value); }
        }

        int _Green;
        public int Green {
            get { return _Green; }
            set { this.RaiseAndSetIfChanged(value); }
        }

        int _Blue;
        public int Blue {
            get { return _Blue; }
            set { this.RaiseAndSetIfChanged(value); }
        }

        String _SaveText;
        public String SaveText
        {
            get { return _SaveText; }
            set { this.RaiseAndSetIfChanged(value); }
        }

        ObservableAsPropertyHelper<SolidColorBrush> _FinalColor;
        public SolidColorBrush FinalColor {
            get { return _FinalColor.Value; }
        }

        public ReactiveCommand SaveToLED;
        public ReactiveCommand SaveAll;

        public MainWindowViewModel() {
            var whenAnyColorChanges = this.WhenAny(x => x.Red, x => x.Green, x => x.Blue,
                    (r, g, b) => Tuple.Create(r.Value, g.Value, b.Value))
                .Select(intsToColor);

            whenAnyColorChanges
                .Where(x => x != null)
                .Select(x => new SolidColorBrush(x.Value))
                .Throttle(TimeSpan.FromSeconds(0.3), RxApp.DeferredScheduler)
                .ToProperty(this, x => x.FinalColor);

            SaveToLED = new ReactiveCommand();
            SaveAll = new ReactiveCommand();
        }

        Color? intsToColor(Tuple<int, int, int> colorsAsInts) {
            byte? r = inRange(colorsAsInts.Item1), g = inRange(colorsAsInts.Item2), b = inRange(colorsAsInts.Item3);

            if (r == null || g == null || b == null) return null;
            return Color.FromRgb(r.Value, g.Value, b.Value);
        }

        static byte? inRange(int value) {
            if (value < 0 || value > 255) {
                return null;
            }

            return (byte)value;
        }
    }
}