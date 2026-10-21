 
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
 

namespace Seedunk.Net
{
    public abstract class IViewModel: INotifyPropertyChanged
    {

        public  abstract void SetView(object viewObject);
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected virtual bool SetProperty<T>(ref T member, T value, [CallerMemberName] string propertyName = null) {

            if (EqualityComparer<T>.Default.Equals(member, value)) {
                return false;
            }
            member = value;
            OnPropertyChanged(propertyName);
            return true;
            
        }
    }
}
