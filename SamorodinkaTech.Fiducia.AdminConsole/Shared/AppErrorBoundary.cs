using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using SamorodinkaTech.Fiducia.Infrastructure.Common.Exceptions;

namespace SamorodinkaTech.Fiducia.AdminConsole.Shared
{
    /// <summary>
    /// ErrorBoundary с логированием через ILogger (Serilog).
    /// Наследуется от стандартного ErrorBoundary и переопределяет OnErrorAsync.
    /// </summary>
    public class AppErrorBoundary : ErrorBoundary
    {
        [Inject] private NavigationManager Nav { get; set; } = default!;
        [Inject] private ILogger<AppErrorBoundary> Logger { get; set; } = default!;

        protected override Task OnErrorAsync(Exception exception)
        {
            try
            {
                var uri = Nav.Uri;
                var infos = ExceptionFlattener.Flatten(exception);
                foreach (var i in infos)
                {
                    Logger.LogError("Blazor UI {ExceptionType}: {Message} uri={Uri}",
                        i.Type, i.Message, uri);
                    if (!string.IsNullOrEmpty(i.StackTrace))
                    {
                        Logger.LogError("  {StackTrace}", i.StackTrace);
                    }
                }
            }
            catch
            {
                // избегаем вторичных сбоев в обработчике ошибок
            }

            return Task.CompletedTask;
        }
    }
}
