using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnhandledExceptionsWpf.Annotations;

namespace UnhandledExceptionsWpf
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _title;
        private string _errorText;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindowViewModel()
        {
            var synchronizationContext = SynchronizationContext.Current;

            // chyba ve vlákně
            ErrorCmd = new RelayCommand(() =>
            {
                Task.Factory.StartNew(() =>
                {                    
                    synchronizationContext.Post(o =>
                    {
                        ErrorText = "Volam chybu vlakna...";
                    }, null);
                    Thread.Sleep(1000);
                    throw new ApplicationException("Chyba nahoda");
                });
                Title = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            });

            // chyba, která se "nehandluje"
            ErrorMainCmd = new RelayCommand(() =>
            {
                ErrorText = "Volam hlavni chybu";                
                throw new Exception("Toto je hlavni chyba");
            });

            // aplikační "handlovaná" chyba
            ErrorAppMainCmd = new RelayCommand(() =>
            {
                ErrorText = "Volam hlavni aplikační chybu";                
                throw new ApplicationException("Toto je hlavni aplikační chyba");
            });
        }

        public string Title
        {
            get => _title;
            set
            {
                if (value == _title) return;
                _title = value;
                OnPropertyChanged();
            }
        }

        public string ErrorText
        {
            get => _errorText;
            set
            {
                if (value == _errorText) return;
                _errorText = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand ErrorCmd { get; set; }

        public RelayCommand ErrorMainCmd { get; set; }

        public RelayCommand ErrorAppMainCmd { get; set; }

    }
}