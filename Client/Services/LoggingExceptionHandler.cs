using Microsoft.Extensions.Logging;
using System;

namespace Client.Services
{
    public interface IExceptionHandler
    {
        void HandleException(Exception ex);
    }

    public class LoggingExceptionHandler : IExceptionHandler
    {
        public void HandleException(Exception ex)
        {
            Console.Error.WriteLine($"Unhandled exception: {ex.Message}");
            Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}