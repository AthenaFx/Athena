using System;
using Athena.Binding;

namespace Athena.Web.ModelBinding.ValueConverters
{
    public interface ValueConverter
    {
        bool Matches(Type destinationType);
        DataBinderResult Convert(Type destinationType, object value);
    }
}