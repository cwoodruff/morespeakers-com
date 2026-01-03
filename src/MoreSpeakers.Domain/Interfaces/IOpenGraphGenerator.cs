namespace MoreSpeakers.Domain.Interfaces;

public interface IOpenGraphGenerator
{
    /// <summary>
    /// Queues a message to create an OpenGraph profile image for a speaker.
    /// </summary>
    /// <param name="id">The unique identifier of the speaker.</param>
    /// <param name="headshotUrl">The URL of the speaker's headshot image.</param>
    public Task QueueSpeakerOpenGraphProfileImageCreation(Guid id, string? headshotUrl = null);
}