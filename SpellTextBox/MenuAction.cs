using System.ComponentModel;
using System.Windows.Input;

namespace SpellTextBox
{
    public class MenuAction : INotifyPropertyChanged
    {
        private string name;
        public string Name { get { return name; } set { name = value; OnPropertyChanged("Name"); } }

        private ICommand command;
        public ICommand Command { get { return command; } set { command = value; OnPropertyChanged("Command"); } }

        public override string ToString()
        {
            return Name;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion
    }
}
