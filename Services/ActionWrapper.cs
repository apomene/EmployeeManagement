using EmployeeManagement.Models;
using Microsoft.AspNetCore.Mvc;

public class BadRequestException : Exception
{
    public BadRequestException(string message) : base(message) { }
}

public static class ActionWrapper
{
    /// <summary>
    /// Executes an async action with centralized try/catch and logging.
    /// Returns 200 OK with result on success,
    /// 400 Bad Request if validation/business exception occurs,
    /// 404 Not Found for missing resources,
    /// or 500 Internal Server Error on unexpected exception.
    /// </summary>
    public static async Task<ActionResult<T>> ExecuteAsync<T>(
        ILogger logger,
        Func<Task<T>> action,
        string? successMessage = null,
        params object[] successParams)
    {
        try
        {
            var result = await action();

            if (result is IActionResult actionResult)
            {
                LogResult(logger, actionResult, successMessage, successParams);
            }
            else if (!string.IsNullOrEmpty(successMessage))
            {
                logger.LogInformation(successMessage, successParams);
            }

            return result;
        }
        catch (BadRequestException brex)
        {
            logger.LogWarning(brex, "Bad request: {Message}", brex.Message);
            return new BadRequestObjectResult(new { error = brex.Message });
        }
        catch (KeyNotFoundException knf)
        {
            logger.LogWarning(knf, "Resource not found: {Message}", knf.Message);
            return new NotFoundResult();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, StringConstants.ERROR_500, ex.Message);
            return new StatusCodeResult(500);
        }
    }

    /// <summary>
    /// Variant for actions that return only IActionResult.
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
            LogResult(logger, result, successMessage, successParams);
            return result;
        }
        catch (KeyNotFoundException knf)
        {
            logger.LogWarning(knf, "Resource not found: {Message}", knf.Message);
            return new NotFoundResult();
        }
        catch (BadRequestException brex)
        {
            logger.LogWarning(brex, "Bad request: {Message}", brex.Message);
            return new BadRequestObjectResult(new { error = brex.Message });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, StringConstants.ERROR_500, ex.Message);
            return new StatusCodeResult(500);
        }
    }

    /// <summary>
    /// Shared logic for logging based on IActionResult status codes.
    /// </summary>
    private static void LogResult(
        ILogger logger,
        IActionResult result,
        string? successMessage,
        params object[] successParams)
    {
        if (result is ObjectResult objResult)
        {
            if (objResult.StatusCode is >= 200 and < 300)
            {
                if (!string.IsNullOrEmpty(successMessage))
                    logger.LogInformation(successMessage, successParams);
            }
            else if (objResult.StatusCode is >= 400 and < 500)
            {
                logger.LogWarning("Bad request: {Message}", objResult.Value);
            }
        }
        else if (!string.IsNullOrEmpty(successMessage))
        {
            logger.LogInformation(successMessage, successParams);
        }
    }
}
