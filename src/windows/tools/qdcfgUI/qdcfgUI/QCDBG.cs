using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;

namespace QCUtility
{
    class QCDBG
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern void OutputDebugString(string message);

        public static void Printf(String format, params Object[] args)
        {
            string formatedOutput = String.Format(format, args);
            OutputDebugString(formatedOutput);
        }
    }

    public static class ListExtension
    {
        public static void Sort<TSource, TKey>(this ObservableCollection<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (source == null) return;

            Comparer<TKey> comparer = Comparer<TKey>.Default;
            for (int i = source.Count - 1; i >= 0; i--)
            {
                for (int j = 1; j <= i; j++)
                {
                    TSource o1 = source[j - 1];
                    TSource o2 = source[j];
                    if (comparer.Compare(keySelector(o1), keySelector(o2)) > 0)
                    {
                        source.Remove(o1);
                        source.Insert(j, o1);
                    }
                }
            }
        }
    }

    public class RelayCommand : ICommand
    {
        private Action<object> execute;
        private Func<object, bool> canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            this.execute(parameter);
        }
    }

    public class DebugFlagsAndLevelRule : ValidationRule
    {
        public int MaxLength { get; set; }        

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if(value != null && !string.IsNullOrEmpty(value.ToString()))
            {
                string str = value.ToString();
                if (str.Length > MaxLength)
                {
                    return new ValidationResult(false, "Please enter maximum of " + MaxLength + " characters ");
                }

                Regex regex = new Regex("[^0-9a-fA-F]");
                if (regex.IsMatch(str))
                {
                    return new ValidationResult(false, "Please enter only hex characters");
                }
            }
            

            return new ValidationResult(true, null);
        }
    }

    public class FileSizeValidationRule : ValidationRule
    {      

        public int MaxValue { get; set; }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            
            if (value != null && !string.IsNullOrEmpty(value.ToString()))
            {                
                int num;
                bool result = int.TryParse(value.ToString(), out num);                
                if(result)
                {
                    if (num < 0 || num > 10000)
                    {
                        return new ValidationResult(false, "Please enter in range [0-" + MaxValue + "]");
                    }
                }
                else
                {
                    return new ValidationResult(false, "Please enter only numeric digits [0-9]");
                }
                                
            }

            return new ValidationResult(true, null);
        }
    }

    public class CNotifyPropertyChanged: INotifyPropertyChanged
    {
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }

    public class ConsoleController
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
