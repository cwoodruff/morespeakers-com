namespace MoreSpeakers.Domain.Models.Messages
{
	/// <summary>
	/// Represents an email that needs to be sent
	/// </summary>
	[Serializable]
	public class Email
	{
		/// <summary>
		/// The recipient of the email
		/// </summary>
		public required string ToMailAddress { get; set; }
		/// <summary>
		/// The display name of the recipient
		/// </summary>
		public required string ToDisplayName { get; set; }
		/// <summary>
		/// Who the email was from
		/// </summary>
		public required string FromMailAddress { get; set; }
		/// <summary>
		/// The display name of the person receiving the email
		/// </summary>
		public required string FromDisplayName { get; set; }
        /// <summary>
        /// The reply to Address
        /// </summary>
        public required string ReplyToMailAddress { get; set; }
	    /// <summary>
	    /// The display name of the person reply to person
	    /// </summary>
	    public required string ReplyToDisplayName { get; set; }
        /// <summary>
        /// The subject of the email
        /// </summary>
        public required string Subject { get; set; }
		/// <summary>
		/// The body of the email
		/// </summary>
		public required string Body { get; set; }
	}
}