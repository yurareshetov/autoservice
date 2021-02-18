using System;
using System.Collections.Generic;
using System.ComponentModel;
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


namespace autoreshetov
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            ServiceList = Core.DB.Service.ToList();
        }
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private List<Service> _ServiceList;
        public List<Service> ServiceList
        {
            get
            {

                var FilteredServiceList = _ServiceList.FindAll(item =>
                item.DiscountFloat >= CurrentDiscountFilter.Item1 &&
                item.DiscountFloat < CurrentDiscountFilter.Item2);

                if (SearchFilter != "")
                    FilteredServiceList = FilteredServiceList.Where(item =>
                        item.Title.IndexOf(SearchFilter, StringComparison.OrdinalIgnoreCase) != -1 ||
                        item.DescriptionString.IndexOf(SearchFilter, StringComparison.OrdinalIgnoreCase) != -1).ToList();


                if (SortPriceAscending)
                    return FilteredServiceList.OrderBy(item => Double.Parse(item.CostWithDiscount)).ToList();

                else
                    return FilteredServiceList.OrderByDescending(item => Double.Parse(item.CostWithDiscount)).ToList();
            }
            set
            {
                _ServiceList = value;
            }
        }
        private Boolean _IsAdminMode = false;

        public event PropertyChangedEventHandler PropertyChanged;


        // публичный геттер, который меняет текущий режим (Админ/не Админ)
        public Boolean IsAdminMode
        {
            get { return _IsAdminMode; }
            set
            {
                _IsAdminMode = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("AdminModeCaption"));
                    PropertyChanged(this, new PropertyChangedEventArgs("AdminVisibility"));
                }

            }
        }
        // этот геттер возвращает текст для кнопки в зависимости от текущего режима
        public string AdminModeCaption
        {
            get
            {
                if (IsAdminMode) return "Выйти из режима\nАдминистратора";
                return "Войти в режим\nАдминистратора";
            }
        }
        private void AdminButton_Click(object sender, RoutedEventArgs e)
        {
            // если мы уже в режиме Администратора, то выходим из него 
            if (IsAdminMode) IsAdminMode = false;
            else
            {
                // создаем окно для ввода пароля
                var InputBox = new InputBoxWindow("Введите пароль Администратора");
                // и показываем его как диалог (модально)
                if ((bool)InputBox.ShowDialog())
                {
                    // если нажали кнопку "Ok", то включаем режим, если пароль введен верно
                    IsAdminMode = InputBox.InputText == "0000";
                }
            }
        }
        public string AdminVisibility
        {
            get
            {
                if (IsAdminMode) return "Visible";
                return "Collapsed";
            }
        }
        private Boolean _SortPriceAscending = true;
        public Boolean SortPriceAscending
        {
            get
            {
                return _SortPriceAscending;
            }
            set
            {
                _SortPriceAscending = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("ServiceList"));
                    PropertyChanged(this, new PropertyChangedEventArgs("ServicesCount"));
                    PropertyChanged(this, new PropertyChangedEventArgs("FilteredServicesCount"));

                }
            }
        }
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SortPriceAscending = (sender as RadioButton).Tag.ToString() == "1";
        }
        public List<string> FilterByDiscountNamesList
        {
            get
            {
                return FilterByDiscountValuesList
                    .Select(item => item.Item1)
                    .ToList();
            }

        }
        private List<Tuple<string, double, double>> FilterByDiscountValuesList =
            new List<Tuple<string, double, double>>()
            {
                Tuple.Create("Все записи", 0d, 1d),
                Tuple.Create("от 0% до 5%", 0d, 0.05d),
                Tuple.Create("от 5% до 15%", 0.05d, 0.15d),
                Tuple.Create("от 15% до 30%", 0.15d, 0.3d),
                Tuple.Create("от 30% до 70%", 0.3d, 0.7d),
                Tuple.Create("от 70% до 100%", 0.7d, 1d)
            };
        private Tuple<double, double> _CurrentDiscountFilter = Tuple.Create(double.MinValue, double.MaxValue);

        public Tuple<double, double> CurrentDiscountFilter
        {
            get
            {
                return _CurrentDiscountFilter;
            }
            set
            {
                _CurrentDiscountFilter = value;
                if (PropertyChanged != null)
                {
                    // при изменении фильтра список перерисовывается
                    PropertyChanged(this, new PropertyChangedEventArgs("ServiceList"));
                    PropertyChanged(this, new PropertyChangedEventArgs("ServicesCount"));
                    PropertyChanged(this, new PropertyChangedEventArgs("FilteredServicesCount"));
                }
            }
        }

        private string _SearchFilter = "";
        public string SearchFilter
        {
            get { return _SearchFilter; }
            set
            {
                _SearchFilter = value;
                if (PropertyChanged != null)
                {
                    // при изменении фильтра список перерисовывается
                    PropertyChanged(this, new PropertyChangedEventArgs("ServiceList"));
                    PropertyChanged(this, new PropertyChangedEventArgs("ServicesCount"));
                    PropertyChanged(this, new PropertyChangedEventArgs("FilteredServicesCount"));
                }
            }
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            SearchFilter = SearchFilterTextBox.Text;
        }

        private void DiscountFilterComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (DiscountFilterComboBox.SelectedIndex >= 0)
                CurrentDiscountFilter = Tuple.Create(
                    FilterByDiscountValuesList[DiscountFilterComboBox.SelectedIndex].Item2,
                    FilterByDiscountValuesList[DiscountFilterComboBox.SelectedIndex].Item3

                );
        }
        public int ServicesCount
        {
            get
            {
                return _ServiceList.Count;
            }

        }
        public int FilteredServicesCount
        {
            get
            {
                return ServiceList.Count;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var item = MainDataGrid.SelectedItem as Service;

            // по условиям задачи нельзя удалять только те услуги, которые уже оказаны
            // свойство ClientService ссылается на таблицу оказанных услуг
            if (item.ClientService.Count > 0)
            {
                MessageBox.Show("Нельзя удалять услугу, она уже оказана");
                return;
            }

            // метод Remove нужно завернуть в конструкцию try..catch, на случай, если 
            // база спроектирована криво и нет каскадного удаления - это сделайте сами
            Core.DB.Service.Remove(item);

            // сохраняем изменения
            Core.DB.SaveChanges();

            // перечитываем изменившийся список, не забывая в сеттере вызвать PropertyChanged
            ServiceList = Core.DB.Service.ToList();
        }
    }
}
