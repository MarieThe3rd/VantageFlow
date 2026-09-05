using Microsoft.UI.Xaml.Data;
using VantageFlow.Core.Modules.TaskManager.Models;

namespace VantageFlow.Modules.TaskManager.Views.Converters;

/// <summary>Formats an optional Recipient for display — independent of Requester, per CONTEXT.md.</summary>
public sealed class RecipientToSummaryConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is Person person ? $"— owed to {person.Name}" : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
