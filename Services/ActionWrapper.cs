using Microsoft.AspNetCore.Mvc;

public static class ActionWrapper
{
    /// <summary>
    /// Executes an async action with centralized try/catch and logging.
    /// Returns 200 OK with result on success, or 500 Internal Server Error on exception.
    /// </summary>
    /// <typeparam name="T">Type of the result returned by the action.</typeparam>
    /// <param name="logger">Logger for info and error messages.</param>
    /// <param name="action">Async function that returns T.</param>
    /// <param name="successMessage">Optional info message template for logging.</param>
    /// <param name="successParams">Optional parameters for info logging.</param>
    /// <returns>ActionResult wrapping the result or a 500 error.</returns>
    public static async Task<ActionResult<T>> ExecuteAsync<T>(
        ILogger logger,
        Func<Task<T>> action,
        string? successMessage = null,
        params object[] successParams)
    {
        try
        {
            T result = await action();

            if (!string.IsNullOrEmpty(successMessage))
            {
                logger.LogInformation(successMessage, successParams);
            }

            return result; // Automatically wrapped in 200 OK
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while executing API action.");
            return new StatusCodeResult(500);
        }
    }

    /// <summary>
    /// Variant for actions that return only IActionResult (no payload)
    /// </summary>
    public static async Task<IActionResult> ExecuteAsync(
        ILogger logger,
        Func<Task<IActionResult>> action,
        string? successMessage = null,
        params object[] successParams)
    {
        try
        {
            var result = await action();

            if (!string.IsNullOrEmpty(successMessage))
            {
                logger.LogInformation(successMessage, successParams);
            }

            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while executing API action.");
            return new StatusCodeResult(500);
        }
    }
}
