using Microsoft.UI.Xaml.Data;
using VantageFlow.Core.Modules.TaskManager.Models;

namespace VantageFlow.Modules.TaskManager.Views.Converters;

/// <summary>
/// Formats a Task's Source, including its Ticket Number when the Source is a ticket-type value.
/// Bound to the whole TaskItem (no Path) rather than just Source, since it needs both
/// TaskItem.Source and TaskItem.TicketNumber together — WinUI has no MultiBinding.
/// </summary>
public sealed class TaskSourceSummaryConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not TaskItem { Source: { } source } task)
        {
            return string.Empty;
        }

        return source.IsTicket && !string.IsNullOrEmpty(task.TicketNumber)
            ? $"via {source.Name} (#{task.TicketNumber})"
            : $"via {source.Name}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
