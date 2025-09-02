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
    /// or 500 Internal Server Error on unexpected exception.
    /// </summary>
    /// <typeparam name="T">Type of the result returned by the action.</typeparam>
    public static async Task<ActionResult<T>> ExecuteAsync<T>(
        ILogger logger,
        Func<Task<T>> action,
        string? successMessage = null,
        params object[] successParams)
    {
        try
        {
            T result = await action();
            
            if ( result is ObjectResult)
            {
                var objResult = result as ObjectResult;
                if (objResult.StatusCode >= 200 && objResult.StatusCode < 300)
                {
                    if (!string.IsNullOrEmpty(successMessage))
                    {
                        logger.LogInformation(successMessage, successParams);
                    }
                }
                else if (objResult.StatusCode >= 400 && objResult.StatusCode < 500)
                {

                    logger.LogWarning("Bad request: {Message}", objResult.Value);
                }
                return result;
            }
           

            if (!string.IsNullOrEmpty(successMessage))
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
        catch (Exception ex)
        {
            logger.LogError(ex, StringConstants.ERROR_500,ex.Message);
            return new StatusCodeResult(500);
        }
    }

    /// <summary>
    /// Variant for actions that return only IActionResult
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

            if (result is ObjectResult)
            {
                var objResult = result as ObjectResult;
                if (objResult.StatusCode >= 200 && objResult.StatusCode < 300)
                {
                    if (!string.IsNullOrEmpty(successMessage))
                    {
                        logger.LogInformation(successMessage, successParams);
                    }
                }
                else if (objResult.StatusCode >= 400 && objResult.StatusCode < 500)
                {

                    logger.LogWarning("Bad request: {Message}", objResult.Value);
                }
                return result;
            }

            if (!string.IsNullOrEmpty(successMessage))
            {
                logger.LogInformation(successMessage, successParams);
            }
            return result;
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex.Message);
            return new StatusCodeResult(404);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, StringConstants.ERROR_500, ex.Message);
            return new StatusCodeResult(500);
        }
    }



}
