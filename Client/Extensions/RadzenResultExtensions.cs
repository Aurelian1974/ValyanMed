using Radzen;
using Shared.Common;
using System.Net.Http.Json;

namespace Client.Extensions;

/// <summary>
/// Extensions pentru Result Pattern în Radzen Blazor Components
/// Conform planului de refactoring - CRITICAL PRIORITY
/// </summary>
public static class RadzenResultExtensions
{
    /// <summary>
    /// Extension method pentru afi?area notific?rilor în Radzen
    /// </summary>
    public static void ShowNotification(this Result result, NotificationService notificationService)
    {
        if (result.IsSuccess && !string.IsNullOrEmpty(result.SuccessMessage))
        {
            notificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Succes",
                Detail = result.SuccessMessage,
                Duration = 3000
            });
        }
        else if (!result.IsSuccess && result.Errors.Any())
        {
            notificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Eroare",
                Detail = string.Join(", ", result.Errors),
                Duration = 5000
            });
        }
    }

    /// <summary>
    /// Extension method pentru afi?area notific?rilor pentru Result<T>
    /// </summary>
    public static void ShowNotification<T>(this Result<T> result, NotificationService notificationService)
    {
        if (result.IsSuccess && !string.IsNullOrEmpty(result.SuccessMessage))
        {
            notificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Success,
                Summary = "Succes",
                Detail = result.SuccessMessage,
                Duration = 3000
            });
        }
        else if (!result.IsSuccess && result.Errors.Any())
        {
            notificationService.Notify(new NotificationMessage
            {
                Severity = NotificationSeverity.Error,
                Summary = "Eroare",
                Detail = string.Join(", ", result.Errors),
                Duration = 5000
            });
        }
    }

    /// <summary>
    /// Afi?are notificare de informare
    /// </summary>
    public static void ShowInfo(this NotificationService notificationService, string message, string title = "Informare")
    {
        notificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Info,
            Summary = title,
            Detail = message,
            Duration = 4000
        });
    }

    /// <summary>
    /// Afi?are notificare de avertizare
    /// </summary>
    public static void ShowWarning(this NotificationService notificationService, string message, string title = "Atentie")
    {
        notificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Warning,
            Summary = title,
            Detail = message,
            Duration = 4000
        });
    }

    /// <summary>
    /// Afi?are notificare de succes
    /// </summary>
    public static void ShowSuccess(this NotificationService notificationService, string message, string title = "Succes")
    {
        notificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = title,
            Detail = message,
            Duration = 3000
        });
    }

    /// <summary>
    /// Afi?are notificare de eroare
    /// </summary>
    public static void ShowError(this NotificationService notificationService, string message, string title = "Eroare")
    {
        notificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = title,
            Detail = message,
            Duration = 6000
        });
    }

    /// <summary>
    /// Handle API Response cu Result Pattern
    /// </summary>
    public static async Task<T?> HandleApiResponse<T>(this HttpResponseMessage response, NotificationService notificationService, string? successMessage = null)
    {
        if (response.IsSuccessStatusCode)
        {
            try
            {
                var result = await response.Content.ReadFromJsonAsync<T>();
                if (!string.IsNullOrEmpty(successMessage))
                {
                    notificationService.ShowSuccess(successMessage);
                }
                return result;
            }
            catch (Exception ex)
            {
                notificationService.ShowError($"Eroare la procesarea raspunsului: {ex.Message}");
                return default(T);
            }
        }
        else
        {
            try
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMessage = string.IsNullOrEmpty(errorContent) ? 
                    $"Eroare HTTP: {response.StatusCode}" : 
                    errorContent;
                notificationService.ShowError(errorMessage);
            }
            catch
            {
                notificationService.ShowError($"Eroare HTTP: {response.StatusCode}");
            }
            return default(T);
        }
    }

    /// <summary>
    /// Handle API Response cu Result Pattern
    /// </summary>
    public static async Task<bool> HandleApiResponse(this HttpResponseMessage response, NotificationService notificationService, string? successMessage = null)
    {
        if (response.IsSuccessStatusCode)
        {
            if (!string.IsNullOrEmpty(successMessage))
            {
                notificationService.ShowSuccess(successMessage);
            }
            return true;
        }
        else
        {
            try
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMessage = string.IsNullOrEmpty(errorContent) ? 
                    $"Eroare HTTP: {response.StatusCode}" : 
                    errorContent;
                notificationService.ShowError(errorMessage);
            }
            catch
            {
                notificationService.ShowError($"Eroare HTTP: {response.StatusCode}");
            }
            return false;
        }
    }
}