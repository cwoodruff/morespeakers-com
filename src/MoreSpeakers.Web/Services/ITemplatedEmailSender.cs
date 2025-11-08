using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Services;

public interface ITemplatedEmailSender
{
    /// <summary>
    /// Generates and sends templated emails based on the object
    /// </summary>
    /// <param name="emailTemplate"></param>
    /// <param name="telemetryEventName"></param>
    /// <param name="subject"></param>
    /// <param name="toUser"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    public Task<bool> SendTemplatedEmail(string emailTemplate, string telemetryEventName, string subject, User toUser, object? model); 
    
}