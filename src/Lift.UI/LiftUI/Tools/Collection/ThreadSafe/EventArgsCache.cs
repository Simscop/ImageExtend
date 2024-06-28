// https://github.com/meziantou/Meziantou.Framework

#if NETCOREAPP
using System.Collections.Specialized;
using System.ComponentModel;

namespace Lift.UI.Tools;

internal static class EventArgsCache
{
    internal static readonly PropertyChangedEventArgs CountPropertyChanged = new PropertyChangedEventArgs("Count");
    internal static readonly PropertyChangedEventArgs IndexerPropertyChanged = new PropertyChangedEventArgs("Item[]");

    internal static readonly NotifyCollectionChangedEventArgs ResetCollectionChanged =
        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
}
#endif
