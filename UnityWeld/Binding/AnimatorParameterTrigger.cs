using System.ComponentModel;

namespace UnityWeld.Binding
{
    public class AnimatorParameterTrigger : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool TriggerSetOrReset { private set; get; }

        public AnimatorParameterTrigger(bool defaultTriggerValue)
        {
            TriggerSetOrReset = defaultTriggerValue;
        }

        public void Set()
        {
            TriggerSetOrReset = true;
            OnPropertyChanged("TriggerSetOrReset");
        }

        public void Reset()
        {
            TriggerSetOrReset = false;
            OnPropertyChanged("TriggerSetOrReset");
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
