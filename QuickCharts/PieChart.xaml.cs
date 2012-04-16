using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.ComponentModel;

namespace QuickCharts
{
    /// <summary>
    /// Interaction logic for PieChart.xaml
    /// </summary>
    public partial class PieChart
    {
        public PieChart()
        {
            InitializeComponent();
        }

        readonly ObservableCollection<PieDataItem> _data = new ObservableCollection<PieDataItem>
        {
                new PieDataItem { Title = "s1", Value = 10 },
                //new PieDataItem() { Title = "s2", Value = 30 },
                //new PieDataItem() { Title = "s3", Value = 20 },
                //new PieDataItem() { Title = "s4", Value = 80 }
            };

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            pie1.DataSource = this._data;
        }

        private void AddDataButton_Click(object sender, RoutedEventArgs e)
        {
            this._data.Add(new PieDataItem { Title = "s5", Value = 12.56 });
            this._data.Add(new PieDataItem { Title = "s6", Value = 25 });
        }

        private void RealTimeDataChanges_Click(object sender, RoutedEventArgs e)
        {
            DispatcherTimer timer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(2)};
            timer.Tick += TimerTick;
            timer.Start();
        }

        readonly Random rnd = new Random();
        int _sliceCounter = 10;
        void TimerTick(object sender, EventArgs e)
        {
            double action = rnd.NextDouble();
            if (action > 0.85 && this._data.Count > 0)
            {
                this._data.RemoveAt(rnd.Next(this._data.Count));
            }
            else if (action > 0.8)
            {
                this._data.Add(new PieDataItem { Title = "slice " + this._sliceCounter, Value = rnd.NextDouble() * 50 });
                this._sliceCounter++;
            }
            else
            {
                foreach (PieDataItem di in this._data)
                {
                    di.Value = rnd.NextDouble() * 50;
                }
            }
        }
    }

    public class PieDataItem : INotifyPropertyChanged
    {
        public string Title { internal get; set; }


        private double _value;
        public double Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;

                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Value"));
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
