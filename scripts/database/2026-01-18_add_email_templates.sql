
CREATE TABLE [dbo].[EmailTemplates](
    Location [nvarchar](150) not null primary key,
    Content [nvarchar](max) not null,
    CreatedDate datetime2 default getutcdate() not null,
    IsActive [bit] not null default 1,
    LastModified [datetime2] default getutcdate() not null,
    LastRequested [datetime2] NULL,
);

INSERT INTO [dbo].[EmailTemplates]
    ([Location], [Content])
VALUES (
        '/Pages/Shared/_EmailLayout.cshtml',
        N'<!DOCTYPE html>
        <html lang="en">
        <head>
            <meta charset="utf-8">
            <title></title>
            <style>
                body { font-family: ''Inter'', Arial, sans-serif; line-height: 1.6; color: #333; }
                .container { max-width: 600px; margin: 0 auto; padding: 20px; }
                .header { background: linear-gradient(135deg, #fd7e14 0%, #e55d0e 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }
                .content { background: white; padding: 30px; border-radius: 0 0 8px 8px; box-shadow: 0 4px 8px rgba(0,0,0,0.1); }
                .badge { background: #fd7e14; color: white; padding: 8px 16px; border-radius: 20px; display: inline-block; font-size: 14px; font-weight: bold; }
                .next-steps { background: #f8f9fa; padding: 20px; border-radius: 6px; margin: 20px 0; }
                .step { display: flex; align-items: center; margin: 15px 0; }
                .step-number { background: #fd7e14; color: white; width: 30px; height: 30px; border-radius: 50%; display: flex; align-items: center; justify-content: center; margin-right: 15px; font-weight: bold; }
                .btn { background: #fd7e14; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; display: inline-block; font-weight: bold; }
                .footer { text-align: center; margin-top: 30px; color: #666; font-size: 14px; }
            </style>
        </head>
        <body>
            <div class="container">
                <div class="header">
                    <h1>🎤 Welcome from MoreSpeakers.com!</h1>
                    <p>Your speaking journey starts here</p>
                </div>

                <div class="content">
                    @RenderBody()
                </div>

                <h3>🤝 Community Guidelines</h3>
                <p>We''re building a supportive and inclusive community. Please be respectful, helpful, and authentic in all your interactions.</p>

                <div class="footer">
                    <p>
                        <strong>MoreSpeakers.com</strong><br>
                        Connecting speakers, sharing knowledge, building community<br>
                        <a href="mailto:support@morespeakers.com">support@morespeakers.com</a>
                    </p>
                    <p style="font-size: 12px; color: #999;">
                        You received this email because you registered an account at MoreSpeakers.com.<br>
                        If you have any questions, please contact our support team.
                    </p>
                </div>
            </div>
        </body>
        </html>"'
       );

INSERT INTO [dbo].[EmailTemplates]
([Location], [Content])
VALUES (
           '/EmailTemplates/ConfirmUserEmail.cshtml',
           N'@model MoreSpeakers.Domain.Models.UserConfirmationEmail

@{
    Layout = "~/Pages/Shared/_EmailLayout.cshtml";
}

<h2>Hello @Model.User.FullName!</h2>
<p>Thank you for registering with MoreSpeakers.com. We''re excited to have you on board.</p>

Now that you registered for an account, you can start sharing your knowledge and experiences with the world.
In order to take the next step in your journey, please confirm your email address.  We promise we won''t spam you.

<a href="@Model.ConfirmationUrl" class="btn">Confirm Email</a>
'
       );

INSERT INTO [dbo].[EmailTemplates]
([Location], [Content])
VALUES (
           '/EmailTemplates/MentorshipRequestAcceptedFromMentee.cshtml',
           '@model MoreSpeakers.Domain.Models.Mentorship

@{
    Layout = "~/Pages/Shared/_EmailLayout.cshtml";
}

<h2>Hello @Model.Mentor.FirstName!</h2>

<p>Congratulations your mentoring request was accepted from @Model.Mentor.FullName! </p>

<p>We''re thrilled to have you as a mentee now in our growing network of passionate speakers.</p>

<p>You should contact @Model.Mentor.FirstName to schedule your first meeting based on the @Model.PreferredFrequency frequency.</p>

<p>Thank you again for being a part of the MoreSpeakers community and if you have any questions please contact Chris Woody Woodruff at [cwoodruff@live.com](mailto:cwoodruff@live.com)  or Joe Guadagno at [jguadagno@hotmail.com](mailto:jguadagno@hotmail.com) </p>'
       );

INSERT INTO [dbo].[EmailTemplates]
([Location], [Content])
VALUES (
           '/EmailTemplates/MentorshipRequestAcceptedToMentor.cshtml',
           '@model MoreSpeakers.Domain.Models.Mentorship

@{
    Layout = "~/Pages/Shared/_EmailLayout.cshtml";
}

<h2>Hello @Model.Mentor.FirstName!</h2>

<p>Congratulations on accepting the mentoring request from @Model.Mentee.FullName!</p>

<p>We''re thrilled to have you as a mentor in our growing network of passionate speakers.</p>

<p>You should contact @Model.Mentee.FirstName to schedule your first meeting based on the @Model.PreferredFrequency frequency. Your new mentee contact information is:</p>

<p>
    Email - @Model.Mentee.Email
    Phone Number - @Model.Mentee.PhoneNumber
</p>

<p>Thank you again for being a part of the MoreSpeakers community and if you have any questions please contact Chris Woody Woodruff at [cwoodruff@live.com](mailto:cwoodruff@live.com)  or Joe Guadagno at [jguadagno@hotmail.com](mailto:jguadagno@hotmail.com) </p>'
       );

INSERT INTO [dbo].[EmailTemplates]
([Location], [Content])
VALUES (
           '/EmailTemplates/MentorshipRequestCancelledFromMentee.cshtml',
           '@model MoreSpeakers.Domain.Models.Mentorship

@{
    Layout = "~/Pages/Shared/_EmailLayout.cshtml";
}

<p>Hey @Model.Mentee.FullName, </p>

<p>We wanted to let you know that the mentorship has been canceled.</p>

<p>Thank you again for being a part of the MoreSpeakers community and if you have any questions please contact Chris Woody Woodruff at [cwoodruff@live.com](mailto:cwoodruff@live.com)  or Joe Guadagno at [jguadagno@hotmail.com](mailto:jguadagno@hotmail.com) </p>'
       );

INSERT INTO [dbo].[EmailTemplates]
([Location], [Content])
VALUES (
           '/EmailTemplates/MentorshipRequestCancelledToMentor.cshtml',
           '@model MoreSpeakers.Domain.Models.Mentorship

@{
    Layout = "~/Pages/Shared/_EmailLayout.cshtml";
}

<p>Hey @Model.Mentee.FullName, </p>

<p>We wanted to let you know that the mentorship has been canceled.</p>

<p>Thank you again for being a part of the MoreSpeakers community and if you have any questions please contact Chris Woody Woodruff at [cwoodruff@live.com](mailto:cwoodruff@live.com)  or Joe Guadagno at [jguadagno@hotmail.com](mailto:jguadagno@hotmail.com) </p>'
       );

INSERT INTO [dbo].[EmailTemplates]
([Location], [Content])
VALUES (
           '/EmailTemplates/MentorshipRequestDeclinedFromMentee.cshtml',
           N'@model MoreSpeakers.Domain.Models.Mentorship

@{
    Layout = "~/Pages/Shared/_EmailLayout.cshtml";
}

<p>Hey @Model.Mentor.FullName, </p>

<p>We wanted to let you know that @Model.Mentor.FullName declined the mentorship request from you. We understand that this news may not be the best at this time but just know that there are many great mentors at MoeSpeakers.com.</p>

<p>Please keep looking at the list of possible mentors. We know there is a great match for you coming soon.</p>

<p>Thank you again for being a part of the MoreSpeakers community and if you have any questions please contact Chris Woody Woodruff at [cwoodruff@live.com](mailto:cwoodruff@live.com)  or Joe Guadagno at [jguadagno@hotmail.com](mailto:jguadagno@hotmail.com) </p>'
       );

INSERT INTO [dbo].[EmailTemplates]
([Location], [Content])
VALUES (
           '/EmailTemplates/MentorshipRequestDeclinedToMentor.cshtml',
           N'@model MoreSpeakers.Domain.Models.Mentorship

@{
    Layout = "~/Pages/Shared/_EmailLayout.cshtml";
}

<p>Hey @Model.Mentee.FullName, </p>

<p>We wanted to let you know that the mentorship with @Model.Mentee.FullName has been declined by yourself. We know it was a hard decision and we hope that you find a fulfilling mentoring connection soon with another new speaker in the MoreSpeakers.com community. </p>

<p>Thank you again for being a part of the MoreSpeakers community and if you have any questions please contact Chris Woody Woodruff at [cwoodruff@live.com](mailto:cwoodruff@live.com)  or Joe Guadagno at [jguadagno@hotmail.com](mailto:jguadagno@hotmail.com) </p>'
       );

INSERT INTO [dbo].[EmailTemplates]
([Location], [Content])
VALUES (
           '/EmailTemplates/MentorshipRequestFromMentee.cshtml',
           '@model MoreSpeakers.Domain.Models.Mentorship

@{
    Layout = "~/Pages/Shared/_EmailLayout.cshtml";
}

<p>Hey @Model.Mentee.FullName, </p>

<p>We wanted to let you know that we received your mentorship request. The request has been sent to @Model.Mentor.FullName.</p>

<p>Thank you again for being a part of the MoreSpeakers community and if you have any questions please contact Chris Woody Woodruff at [cwoodruff@live.com](mailto:cwoodruff@live.com)  or Joe Guadagno at [jguadagno@hotmail.com](mailto:jguadagno@hotmail.com) </p>
'
       );

INSERT INTO [dbo].[EmailTemplates]
([Location], [Content])
VALUES (
           '/EmailTemplates/MentorshipRequestToMentor.cshtml',
           '@model MoreSpeakers.Domain.Models.Mentorship

@{
    Layout = "~/Pages/Shared/_EmailLayout.cshtml";
}

<p>Hey @Model.Mentor.FullName, </p>

<p>We wanted to let you know that @Model.Mentee.FullName has sent a request to you for mentorship. Please review the request at MoreSpeakers.com and respond at your earliest convenience.</p>

<p>Thank you again for being a part of the MoreSpeakers community and if you have any questions please contact Chris Woody Woodruff at [cwoodruff@live.com](mailto:cwoodruff@live.com)  or Joe Guadagno at [jguadagno@hotmail.com](mailto:jguadagno@hotmail.com) </p>'
       );

INSERT INTO [dbo].[EmailTemplates]
([Location], [Content])
VALUES (
           '/EmailTemplates/PasswordReset.cshtml',
           '@model MoreSpeakers.Domain.Models.UserPasswordResetEmail;

@{
    Layout = "~/Pages/Shared/_EmailLayout.cshtml";
}

<h2>Reset Your Password</h2>
<p>Hello @Model.User.FullName,</p>

<p>We received a request to reset the password for your MoreSpeakers.com account. If you didn''t make this request, you can safely ignore this email.</p>

<p>To reset your password, please click the button below:</p>

<p>
    <a href="@Model.ResetEmailUrl" class="btn">Reset Password</a>
</p>

<p>For security reasons, this link will expire in 24 hours.</p>

<p>If you''re having trouble clicking the "Reset Password" button, copy and paste the URL below into your web browser:</p>
<p>@Model.ResetEmailUrl</p>

<p>Thanks,<br />
The MoreSpeakers Team</p>
'
       );

INSERT INTO [dbo].[EmailTemplates]
([Location], [Content])
VALUES (
           '/EmailTemplates/WelcomeEmail.cshtml',
           N'@model MoreSpeakers.Domain.Models.User

@{
    Layout = "~/Pages/Shared/_EmailLayout.cshtml";
}

<h2>Hello @Model.FirstName!</h2>

<p>Congratulations on joining the MoreSpeakers.com community! We''re thrilled to have you as a <span class="badge">@Model.SpeakerType.ToFriendlyName()</span> in our growing network of passionate speakers.</p>

<h3>Your Registration Details:</h3>
<ul>
    <li><strong>Name:</strong> @Model.FirstName @Model.LastName</li>
    <li><strong>Email:</strong> @Model.Email</li>
    <li><strong>Speaker Type:</strong> @Model.SpeakerType.ToFriendlyName()</li>
    <li><strong>Registration Date:</strong> @(DateTime.UtcNow.ToString("MMMM dd, yyyy"))</li>
</ul>

<div class="next-steps">
    <h3>🚀 What''s Next?</h3>

    <div class="step">
        <div class="step-number">1</div>
        <div>
            <strong>Complete Your Profile</strong><br>
            <small>Add more details, upload a headshot, and showcase your expertise to help others find and connect with you.</small>
        </div>
    </div>

    <div class="step">
        <div class="step-number">2</div>
        <div>
            <strong>@(Model.SpeakerTypeId == (int) MoreSpeakers.Domain.Models.SpeakerTypeEnum.NewSpeaker ? "Find Mentors" : "Connect with New Speakers")</strong><br>
            <small>@(Model.SpeakerTypeId == (int) MoreSpeakers.Domain.Models.SpeakerTypeEnum.NewSpeaker)
                ? "Browse our community of experienced speakers and find mentors who can guide your speaking journey."
                : "Discover new speakers who could benefit from your experience and mentorship.")</small>
        </div>
    </div>

    <div class="step">
        <div class="step-number">3</div>
        <div>
            <strong>Join the Community</strong><br>
            <small>Participate in discussions, share your experiences, and connect with fellow speakers from around the world.</small>
        </div>
    </div>
</div>

<div style="text-align: center; margin: 30px 0;">
    <a href="https://www.morespeakers.com/Profile/Edit" class="btn">Complete My Profile →</a>
</div>

<h3>📧 Email Confirmation Required</h3>
<p>To activate your account and access all features, please check your email for a confirmation link. If you don''t see it in your inbox, don''t forget to check your spam folder.</p>
'
       );
