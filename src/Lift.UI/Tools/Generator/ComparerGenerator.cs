using System;
using System.Collections.Generic;
using Lift.UI.Data;

namespace Lift.UI.Tools;

public class ComparerGenerator
{
    private static readonly Dictionary<Type, ComparerTypeCode> TypeCodeDic = new()
    {
        [typeof(DateTimeRange)] = ComparerTypeCode.DateTimeRange,
    };

    public static IComparer<T> GetComparer<T>()
    {
        if (TypeCodeDic.TryGetValue(typeof(T), out var comparerType))
        {
            if (comparerType == ComparerTypeCode.DateTimeRange)
            {
                return (IComparer<T>) new DateTimeRangeComparer();
            }

            return null;
        }

        return null;
    }

    private enum ComparerTypeCode
    {
        DateTimeRange
    }
}
