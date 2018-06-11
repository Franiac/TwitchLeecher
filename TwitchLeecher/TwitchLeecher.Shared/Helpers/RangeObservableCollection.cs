using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace TwitchLeecher.Shared.Helpers
{
    public class RangeObservableCollection<T> : ObservableCollection<T>
    {
        public RangeObservableCollection()
            : base()
        {
        }

        public RangeObservableCollection(IEnumerable<T> collection)
            : base(collection)
        {
        }

        public RangeObservableCollection(List<T> list)
            : base(list)
        {
        }

        public void AddRange(IEnumerable<T> range)
        {
            foreach (var item in range)
            {
                Items.Add(item);
            }

            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void Reset(IEnumerable<T> range)
        {
            Items.Clear();
            AddRange(range);
        }
    }
}