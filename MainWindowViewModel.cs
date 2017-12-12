using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using UnhandledExceptionsWpf.Annotations;

namespace UnhandledExceptionsWpf
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _title;
        private string _errorText;
        private RelayCommand _errorThrCmd;
        private SynchronizationContext _synchronizationContext;
        private RelayCommand m_errorBckgThrCmd;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainWindowViewModel()
        {
            _synchronizationContext = SynchronizationContext.Current;

            // chyba ve vlákně
            ErrorCmd = new RelayCommand(ThreadFactoryException);

            // chyba ošetřena ve vlákně
            ErrorThrCmd = new RelayCommand(ThreadHandledException);

            // chyba v background vlákně
            ErrorBckgThrCmd = new RelayCommand(ThreadBackgroundHandledException);

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

        public RelayCommand ErrorBckgThrCmd
        {
            get => m_errorBckgThrCmd;
            set
            {
                if (Equals(value, m_errorBckgThrCmd)) return;
                m_errorBckgThrCmd = value;
                OnPropertyChanged();
            }
        }

        private void ThreadBackgroundHandledException()
        {
            Exception err = null;
            var thr = new BackgroundWorker();
            ErrorText = "Start background...";
            thr.DoWork += delegate(object sender, DoWorkEventArgs e)
            {
                try
                {
                    Thread.Sleep(200);
                    throw new ApplicationException("Background error");
                }
                catch (Exception ex)
                {
                    err = ex;
                }
            };
            thr.RunWorkerCompleted += delegate(object sender, RunWorkerCompletedEventArgs e)
            {
                ErrorText = "Backgroundworker finnish.";
                if (err != null)
                {
                    MessageBox.Show(err.Message, "Backgroundworker", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            };
            thr.RunWorkerAsync();
        }

        private void ThreadHandledException()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    Thread.Sleep(300);
                    throw new ApplicationException("Chyba vlakno: " + DateTime.Now.ToString("HH:mm:ss"));
                }
                catch (Exception e)
                {
                    _synchronizationContext.Post(o =>
                    {
                        var err = (Exception)o;
                        MessageBox.Show(err.Message, "Thread", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }, e);
                }
            });
            Title = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        }

        private void ThreadFactoryException()
        {
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(300);
                throw new ApplicationException("Chyba vlakno: " + DateTime.Now.ToString("HH:mm:ss"));
            });
            Title = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        }

        public RelayCommand ErrorThrCmd
        {
            get => _errorThrCmd;
            set
            {
                if (Equals(value, _errorThrCmd)) return;
                _errorThrCmd = value;
                OnPropertyChanged();
            }
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