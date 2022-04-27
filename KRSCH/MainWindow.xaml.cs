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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace KRSCH
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new Programm.ViewModel();
        }

        private void OpenInstruction(object sender, RoutedEventArgs e) // окно инструкция
        {
            MessageBox.Show("Привет! Это инструкция. Пользоваться программой очень просто.\n1. Выберите нужный режим работы, последовательный, или парралельный.\n" +
                "2. Введите нужные значения напряжения и сопротивления. Учтите, что реостат меняет значение в пределах от 1 до 1000 единиц." +
                "\nНе допускайте расчета слишком больших токов, иначе наша схема превратится в уголь;(.", "Справка");
        }

        private void OpenSi(object sender, RoutedEventArgs e) // окно с приставками в СИ
        {
            MessageBox.Show("T = 10¹²\n" +
                "Г = 10⁹\n" +
                "М = 10⁶\n" +
                "к = 10³\n" +
                "м = 10⁻³\n" +
                "мк = 10⁻⁶\n" +
                "н = 10⁻⁹\n" +
                "п = 10⁻¹²\n", "Приставки СИ");
        }
    }
}

namespace Programm
{

    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Model : BaseViewModel
    {
        // значение сопротивления и строка со значением
        protected double resist = 1; 
        protected string resistString = "1";

        // значение реостата
        protected double reostat = 1;

        // значение напряжение и строка со значением
        protected double voltage = 1;
        protected string voltageString = "1";

        // значение тока
        protected double current;

        // значение тока для второго резистора (в случае парралельного включения резисторов) и строка значение
        protected double secondCurrent;
        protected string secondCurrentString;

        // приставки СИ для двух токов
        protected string curUnits;
        protected string secondCurUnits;

        // булевое значение, становит True если превышен ток
        protected bool SchemeIsBurned = false;

        // картинка в том случае если схема "сгорает"
        protected string ImgBurned = @"~\..\scheme_burned.png";
        protected string ImgLine = @"~\..\scheme_polsed.png";
        protected string ImgParr = @"~\..\scheme_parrarel.png";


        // приставки в СИ для сопротивления, напряжения и реостата
        public string resSelectionUnit = "";
        public string volSelectionUnit = "";
        public string reoSelectionUnit = "";

        // режими работы схемы
        public enum EnConnMode 
        {
            parallel,
            linearly
        }

        // единицы СИ для резистора и сопротивления
        public static Dictionary<string, double> DicUnits = new Dictionary<string, double>
        {
            {"Г", Math.Pow(10,9) },
            {"М", Math.Pow(10,6) },
            {"к", Math.Pow(10,3) },
            {"", 1 },
            {"м", Math.Pow(10,-3) },
            {"мк", Math.Pow(10,-6)}
        };

        // единицы СИ для полученных значений тока
        public static Dictionary<string, double> AmpereDicUnits = new Dictionary<string, double>
        {
            {"ТA", Math.Pow(10,12) },
            {"ГA", Math.Pow(10,9) },
            {"МA", Math.Pow(10,6) },
            {"кA", Math.Pow(10,3) },
            {"A", 1 },
            {"мA", Math.Pow(10,-3) },
            {"мкA", Math.Pow(10,-6) },
            {"нA", Math.Pow(10,-9) },
            {"пA", Math.Pow(10,-12) }
        };

        public List<string> StrUnits { get; set; } = new List<string>(DicUnits.Keys); // 

    }


    public class ViewModel : Model
    {
        EnConnMode connMode = EnConnMode.linearly;

        public EnConnMode ConnectionMode // изменение режима работы 
        {
            get => connMode; 
            set
            {
                if (connMode == value) return;
                connMode = value;
                OnPropertyChanged(nameof(ParrConnect));
                OnPropertyChanged(nameof(LineConnect));
                OnPropertyChanged(nameof(ImgMode));
            }
        }

        public bool ParrConnect // параллельное соединение сопротивление 
        {
            get => (ConnectionMode == EnConnMode.linearly);
            set => ConnectionMode = value ? EnConnMode.linearly : ConnectionMode;
        }

        public bool LineConnect // последовательное соединение сопротивления
        {
            get => (ConnectionMode == EnConnMode.parallel);
            set => ConnectionMode = value ? EnConnMode.parallel : ConnectionMode;
        }

        public string ImgMode // изменение картинки при изменении режима работы
        {
            get
            {
                if (SchemeIsBurned) // если схема сгорела
                {
                    MessageBox.Show("Упс, кажется мы получили слишком большой ток.\n" +
                        "Просто перезапустите программу, и не расчитывайте такие большие токи.");
                    return ImgBurned;
                }
                if (ParrConnect) // параллельное соединение
                {
                    OnPropertyChanged(nameof(Current));
                    return ImgParr;
                }
                else // последовательное соединение
                {
                    OnPropertyChanged(nameof(Current));
                    return ImgLine;
                }
            }
        }

        public string ResSelectionUnit // выбор десятичных единиц для сопротивления
        {
            get => resSelectionUnit;
            set
            {
                resSelectionUnit = value;
                OnPropertyChanged(nameof(Current));
            }
        }

        public string ReoSelectionUnit // выбор десятичных единиц для реостата
        {
            get => reoSelectionUnit;
            set
            {
                reoSelectionUnit = value;
                OnPropertyChanged(nameof(Current));
            }
        }

        public string VolSelectionUnit // выбор десятичных единиц для напряжения
        {
            get => volSelectionUnit;
            set
            {
                volSelectionUnit = value;
                OnPropertyChanged(nameof(Current));
            }
        }

        public double Reostat // получение значения реостата 
        {
            get => reostat;
            set
            {
                reostat = value;
                OnPropertyChanged(nameof(Current));
            }
        }

        public double Resist // получение значения сопротивления 
        {
            get => resist;
            set
            {
                resist = value;

                if (value == 0)
                {
                    OnPropertyChanged(nameof(Current));
                    return;
                }

                if (!double.TryParse(ResistString, out double _resist) || _resist != value)
                {
                    ResistString = value.ToString();
                }
                OnPropertyChanged(nameof(Current));
            }
        }

        public string ResistString // получение значения сопротивления 
        {
            get => resistString;
            set
            {
                if (value.Contains(" ")) return; // пропускаем пробел 
                if (value == "") // если пустая строка
                {
                    resistString = value;
                    Resist = 0;
                    OnPropertyChanged(nameof(Current));
                    return;
                }
                if (double.TryParse(value, out double resist) && resist < 1000) // проверка на наличие символов и значение
                {
                    resistString = value;
                    if (Resist != resist)
                    {
                        Resist = resist;
                    }
                }
                else OnPropertyChanged(nameof(ResistString));
            }
        }

        public double Voltage // получение значения напряжения
        {
            get => voltage;
            set
            {
                voltage = value;

                if(value == 0) 
                {
                    OnPropertyChanged(nameof(Current));
                    return;
                }

                if (!double.TryParse(voltageString, out double temp) || temp != value) 
                {
                    voltageString = value.ToString();
                }
                OnPropertyChanged(nameof(Current));
            }
        }

        public string VoltageString // получение значения напряжения
        {
            get => voltageString;
            set
            {
                if (value.Contains(" ")) return; // пропускаем пробел
                if (value == "") // если пустая строка
                {
                    voltageString = value;
                    Voltage = 0;
                    OnPropertyChanged(nameof(Current));
                    return;
                }
                if (double.TryParse(value, out double voltage) && voltage < 1000) // проверка на наличие символов и значение
                {
                    voltageString = value;
                    if (Voltage != voltage)
                    {
                        Voltage = voltage;
                    }
                }
                else OnPropertyChanged(nameof(VoltageString));
            }
        }

        public string CurUnits // передача единиц тока
        {
            get => curUnits;
            set => curUnits = value;
        }

        public string Current // передача значений тока
        {
            get
            {
                double resUnit = DicUnits[ResSelectionUnit];
                double _resist = Resist * resUnit;

                double reoUnit = DicUnits[ReoSelectionUnit];
                double _reostat = Reostat * reoUnit;

                double volUnit = DicUnits[VolSelectionUnit];
                double _voltage = Voltage * volUnit;

                if (LineConnect) // последовательное соединение
                {
                    SecondCurrent = "Не используется";
                    SecondCurUnits = "";
                  
                    double _current = _voltage / (_resist + _reostat);
                    current = _current;
                }
                else // параллельное соединение
                {
                    double _current1 = _voltage / _reostat;
                    current = _current1;

                    double _current2 = _voltage / _resist;

                    if (_current2 > Math.Pow(10, 15))
                    {
                        SecondCurUnits = "ПА";
                        SecondCurrent = "Больше 1";
                        if (!double.IsInfinity(_current2))
                        {
                            SchemeIsBurned = true;
                            OnPropertyChanged(nameof(ImgMode));
                        }
                    }

                    if (_current2 < Math.Pow(10, -12))
                    {
                        SecondCurUnits = "пА";
                        SecondCurrent = "Меньше 1";
                    }

                    foreach (KeyValuePair<string, double> val in AmpereDicUnits)
                    {
                        if (_current2 / val.Value >= 1 && _current2 / val.Value < 1000)
                        {
                            SecondCurUnits = val.Key;
                            SecondCurrent = Convert.ToString(Math.Round(_current2 / val.Value, 3));
                        }
                    }
                }

                OnPropertyChanged(nameof(SecondCurrent));
                OnPropertyChanged(nameof(SecondCurUnits));

                if (current > Math.Pow(10, 15))
                {
                    CurUnits = "ПА";
                    OnPropertyChanged(nameof(CurUnits));
                    if (!double.IsInfinity(current))
                    {
                        SchemeIsBurned = true;
                        OnPropertyChanged(nameof(ImgMode));
                    }
                    return "Больше 1";
                }
                if (current < Math.Pow(10, -12))
                {
                    CurUnits = "пА";
                    OnPropertyChanged(nameof(CurUnits));
                    return "Меньше 1";
                }

                foreach (KeyValuePair<string, double> val in AmpereDicUnits)
                {
                    if (current / val.Value >= 1 && current / val.Value < 1000)
                    {
                        CurUnits = val.Key;
                        OnPropertyChanged(nameof(CurUnits));
                        return Convert.ToString(Math.Round(current / val.Value, 3));
                    }
                }

                CurUnits = "";
                OnPropertyChanged(nameof(CurUnits));
                return "";
            }
        }

        public string SecondCurUnits // передача единиц тока
        {
            get => secondCurUnits;
            set => secondCurUnits = value;
        }

        public string SecondCurrent // передача единиц тока
        {
            get => secondCurrentString;
            set => secondCurrentString = value;
        }
    }
}
