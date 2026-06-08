using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Notify.Models;
using Notify.Client;
using Notify.Exceptions;
using Notify.Models;
using Notify.Models.Responses;
using NUnit.Framework;

namespace Notify.Tests.IntegrationTests
{
    [TestFixture]
    public class NotificationClientAsyncIntegrationTests
    {
        private NotificationClient client;

        private readonly string NOTIFY_API_URL = Environment.GetEnvironmentVariable("NOTIFY_API_URL");
        private readonly string API_KEY = Environment.GetEnvironmentVariable("API_KEY");
        private readonly string API_SENDING_KEY = Environment.GetEnvironmentVariable("API_SENDING_KEY");

        private readonly string FUNCTIONAL_TEST_NUMBER = Environment.GetEnvironmentVariable("FUNCTIONAL_TEST_NUMBER");
        private readonly string FUNCTIONAL_TEST_EMAIL = Environment.GetEnvironmentVariable("FUNCTIONAL_TEST_EMAIL");

        private readonly string EMAIL_TEMPLATE_ID = Environment.GetEnvironmentVariable("EMAIL_TEMPLATE_ID");
        private readonly string SMS_TEMPLATE_ID = Environment.GetEnvironmentVariable("SMS_TEMPLATE_ID");
        private readonly string LETTER_TEMPLATE_ID = Environment.GetEnvironmentVariable("LETTER_TEMPLATE_ID");
        private readonly string EMAIL_REPLY_TO_ID = Environment.GetEnvironmentVariable("EMAIL_REPLY_TO_ID");
        private readonly string SMS_SENDER_ID = Environment.GetEnvironmentVariable("SMS_SENDER_ID");
        private readonly string INBOUND_SMS_QUERY_KEY = Environment.GetEnvironmentVariable("INBOUND_SMS_QUERY_KEY");

        private string smsNotificationId;
        private string emailNotificationId;
        private string letterNotificationId;

        const string TEST_TEMPLATE_SMS_BODY = "Hello ((name))\r\n\r\nFunctional Tests make our world a better place";
        const string TEST_SMS_BODY = "Hello someone\n\nFunctional Tests make our world a better place";

        const string TEST_TEMPLATE_EMAIL_BODY = "Hello ((name))\r\n\r\nFunctional test help make our world a better place";
        const string TEST_EMAIL_BODY = "Hello someone\r\n\r\nFunctional test help make our world a better place";
        const string TEST_EMAIL_SUBJECT = "Functional Tests are good";

        const string TEST_LETTER_BODY = "Hello Foo";
        const string TEST_LETTER_SUBJECT = "Main heading";

        [SetUp]
        public void SetUp()
        {
            var notifyApiUrl = Environment.GetEnvironmentVariable("NOTIFY_API_URL");
            var apiKey = Environment.GetEnvironmentVariable("API_KEY");

            if (string.IsNullOrEmpty(notifyApiUrl) || string.IsNullOrEmpty(apiKey))
            {
                Assert.Ignore("Integration tests require NOTIFY_API_URL and API_KEY environment variables to be set.");
            }

            this.client = new NotificationClient(notifyApiUrl, apiKey);
        }

        [Test, Category("Integration"), Category("Integration/NotificationClientAsync")]
        public async Task SendSmsTestWithPersonalisation()
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                { "name", "someone" }
            };

            SmsNotificationResponse response =
                await this.client.SendSmsAsync(FUNCTIONAL_TEST_NUMBER, SMS_TEMPLATE_ID, personalisation, "sample-test-ref");
            this.smsNotificationId = response.id;
            Assert.IsNotNull(response);
            Assert.AreEqual(response.content.body, TEST_SMS_BODY);

            Assert.IsNotNull(response.reference);
            Assert.AreEqual(response.reference, "sample-test-ref");
        }

        [Test, Category("Integration"), Category("Integration/NotificationClientAsync")]
        public async Task GetSMSNotificationWithIdReturnsNotification()
        {
            await SendSmsTestWithPersonalisation();
            Notification notification = await this.client.GetNotificationByIdAsync(this.smsNotificationId);

            Assert.IsNotNull(notification);
            Assert.IsNotNull(notification.id);
            Assert.AreEqual(notification.id, this.smsNotificationId);

            Assert.IsNotNull(notification.body);
            Assert.AreEqual(notification.body, TEST_SMS_BODY);

            Assert.IsNotNull(notification.reference);
            Assert.AreEqual(notification.reference, "sample-test-ref");

            NotifyAssertions.AssertNotification(notification);
        }

        [Test, Category("Integration"), Category("Integration/NotificationClientAsync")]
        public async Task SendEmailTestWithPersonalisation()
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                { "name", "someone" }
            };

            EmailNotificationResponse response =
                await this.client.SendEmailAsync(FUNCTIONAL_TEST_EMAIL, EMAIL_TEMPLATE_ID, personalisation);
            this.emailNotificationId = response.id;

            Assert.IsNotNull(response);
            Assert.AreEqual(response.content.body, TEST_EMAIL_BODY);
            Assert.AreEqual(response.content.subject, TEST_EMAIL_SUBJECT);
        }

        [Test, Category("Integration"), Category("Integration/NotificationClientAsync")]
        public async Task GetEmailNotificationWithIdReturnsNotification()
        {
            await SendEmailTestWithPersonalisation();
            Notification notification = await this.client.GetNotificationByIdAsync(this.emailNotificationId);
            Assert.IsNotNull(notification);
            Assert.IsNotNull(notification.id);
            Assert.AreEqual(notification.id, this.emailNotificationId);

            Assert.IsNotNull(notification.body);
            Assert.AreEqual(notification.body, TEST_EMAIL_BODY);
            Assert.IsNotNull(notification.subject);
            Assert.AreEqual(notification.subject, TEST_EMAIL_SUBJECT);

            NotifyAssertions.AssertNotification(notification);
        }

        [Test, Category("Integration"), Category("Integration/NotificationClientAsync")]
        public async Task SendLetterTestWithPersonalisation()
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                { "address_line_1", "Foo" },
                { "address_line_2", "Bar" },
                { "postcode", "SW1 1AA" }
            };

            LetterNotificationResponse response =
                await this.client.SendLetterAsync(LETTER_TEMPLATE_ID, personalisation);

            this.letterNotificationId = response.id;

            Assert.IsNotNull(response);
            Assert.AreEqual(response.content.body, TEST_LETTER_BODY);
            Assert.AreEqual(response.content.subject, TEST_LETTER_SUBJECT);
        }

        [Test, Category("Integration"), Category("Integration/NotificationClientAsync")]
        public async Task GetLetterNotificationWithIdReturnsNotification()
        {
            await SendLetterTestWithPersonalisation();
            Notification notification = await this.client.GetNotificationByIdAsync(this.letterNotificationId);

            Assert.IsNotNull(notification);
            Assert.IsNotNull(notification.id);
            Assert.AreEqual(notification.id, this.letterNotificationId);

            Assert.IsNotNull(notification.body);
            Assert.AreEqual(notification.body, TEST_LETTER_BODY);

            Assert.IsNotNull(notification.subject);
            Assert.AreEqual(notification.subject, TEST_LETTER_SUBJECT);

            NotifyAssertions.AssertNotification(notification);
        }

        [Test, Category("Integration"), Category("Integration/NotificationClientAsync")]
        public async Task SendPrecompiledLetterTest()
        {
            string reference = Guid.NewGuid().ToString();
            string postage = "first";
            byte[] pdfContents;
            try
            {
                pdfContents = File.ReadAllBytes("../../../IntegrationTests/test_files/one_page_pdf.pdf");
            }
            catch (DirectoryNotFoundException)
            {
                pdfContents = File.ReadAllBytes("IntegrationTests/test_files/one_page_pdf.pdf");
            }

            LetterNotificationResponse response = await this.client.SendPrecompiledLetterAsync(reference, pdfContents, postage);

            Assert.IsNotNull(response.id);
            Assert.AreEqual(response.reference, reference);
            Assert.AreEqual(response.postage, postage);

            Notification notification = await this.client.GetNotificationByIdAsync(response.id);

            Assert.IsNotNull(notification);
            Assert.IsNotNull(notification.id);
            Assert.AreEqual(notification.id, response.id);
            Assert.AreEqual(notification.reference, response.reference);
            Assert.AreEqual(notification.postage, response.postage);

            NotifyAssertions.AssertNotification(notification);
        }

        [Test, Category("Integration"), Category("Integration/NotificationClientAsync")]
        public async Task SendEmailWithDocumentPersonalisationTest()
        {
            byte[] pdfContents;

            try
            {
                pdfContents = File.ReadAllBytes("../../../IntegrationTests/test_files/one_page_pdf.pdf");
            }
            catch (DirectoryNotFoundException)
            {
                pdfContents = File.ReadAllBytes("IntegrationTests/test_files/one_page_pdf.pdf");
            }

            var personalisation = new Dictionary<string, dynamic>
            {
                { "name", NotificationClient.PrepareUpload(pdfContents) }
            };

            EmailNotificationResponse response =
                await this.client.SendEmailAsync(FUNCTIONAL_TEST_EMAIL, EMAIL_TEMPLATE_ID, personalisation);

            Assert.IsNotNull(response.id);
            Assert.IsNotNull(response.template.id);
            Assert.IsNotNull(response.template.uri);
            Assert.IsNotNull(response.template.version);
            Assert.AreEqual(response.content.subject, TEST_EMAIL_SUBJECT);
            Assert.IsTrue(response.content.body.Contains("https://documents."));
        }

        [Test, Category("Integration"), Category("Integration/NotificationClientAsync")]
        public async Task SendEmailWithCSVDocumentPersonalisationTestUsingEmailConfirmationAndRetentionPeriod()
        {
            byte[] pdfContents;

            try
            {
                pdfContents = File.ReadAllBytes("../../../IntegrationTests/test_files/one_page_pdf.pdf");
            }
            catch (DirectoryNotFoundException)
            {
                pdfContents = File.ReadAllBytes("IntegrationTests/test_files/one_page_pdf.pdf");
            }

            var personalisation = new Dictionary<string, dynamic>
            {
                { "name", NotificationClient.PrepareUpload(pdfContents, "report.csv", true, "4 weeks") }
            };

            EmailNotificationResponse response =
                await this.client.SendEmailAsync(FUNCTIONAL_TEST_EMAIL, EMAIL_TEMPLATE_ID, personalisation);

            Assert.IsNotNull(response.id);
            Assert.IsNotNull(response.template.id);
            Assert.IsNotNull(response.template.uri);
            Assert.IsNotNull(response.template.version);
            Assert.AreEqual(response.content.subject, TEST_EMAIL_SUBJECT);
            Assert.IsTrue(response.content.body.Contains("https://documents."));
        }

        [Test, Category("Integration"), Category("Integration/NotificationClientAsync")]
        public async Task GetAllNotifications()
        {
            NotificationList notificationsResponse = await this.client.GetNotificationsAsync();
            Assert.IsNotNull(notificationsResponse);
            Assert.IsNotNull(notificationsResponse.notifications);

            var notifications = notificationsResponse.notifications;

            foreach (var notification in notifications)
            {
                NotifyAssertions.AssertNotification(notification);
            }
        }

        [Test, Category("Integration"), Category("Integration/NotificationClientAsync")]
        public async Task GetReceivedTexts()
        {
            var client_inbound = new NotificationClient(NOTIFY_API_URL, INBOUND_SMS_QUERY_KEY);
            ReceivedTextListResponse receivedTextListResponse = await client_inbound.GetReceivedTextsAsync();
            Assert.IsNotNull(receivedTextListResponse);
            Assert.IsNotNull(receivedTextListResponse.receivedTexts);
            Assert.AreNotEqual(receivedTextListResponse.receivedTexts.Count, 0);

            var receivedTexts = receivedTextListResponse.receivedTexts;

            foreach (var receivedText in receivedTexts)
            {
                NotifyAssertions.AssertReceivedTextResponse(receivedText);
            }
        }

        [Test, Category("Integration"), Category("Integration/NotificationClientAsync")]
        public void GetNotificationWithInvalidIdRaisesClientException()
        {
            var ex = Assert.ThrowsAsync<NotifyClientException>(async () =>
                await this.client.GetNotificationByIdAsync("fa5f0a6e-5293-49f1-b99f-3fade784382f")
            );
            Assert.That(ex.Message, Does.Contain("No result found"));
        }

        [Test, Category("Integration"), Category("Integration/NotificationClientAsync")]
        public void GetTemplateWithInvalidIdRaisesClientException()
        {
            var ex = Assert.ThrowsAsync<NotifyClientException>(async () =>
                await this.client.GetTemplateByIdAsync("invalid_id")
            );
            Assert.That(ex.Message, Does.Contain("id is not a valid UUID"));
        }

        [Test, Category("Integration"), Category("Integration/NotificationClientAsync")]
        public void GetTemplateWithIdWithoutResultRaisesClientException()
        {
            var ex = Assert.ThrowsAsync<NotifyClientException>(async () =>
                await this.client.GetTemplateByIdAsync("fa5f0a6e-5293-49f1-b99f-3fade784382f")
            );
            Assert.That(ex.Message, Does.Contain("No result found"));
        }

        [Test, Category("Integration"), Category("Integration/NotificationClientAsync")]
        public async Task GetAllTemplates()
        {
            TemplateList templateList = await this.client.GetAllTemplatesAsync();
            Assert.IsNotNull(templateList);
            Assert.AreNotEqual(templateList.templates.Count, 0);

            foreach (var template in templateList.templates)
            {
                NotifyAssertions.AssertTemplateResponse(template);
            }
        }

        [Test, Category("Integration"), Category("Integration/NotificationClientAsync")]
        public async Task GetAllSMSTemplates()
        {
            const string type = "sms";
            TemplateList templateList = await this.client.GetAllTemplatesAsync(type);
            Assert.IsNotNull(templateList);
            Assert.AreNotEqual(templateList.templates.Count, 0);

            foreach (var template in templateList.templates)
            {
                NotifyAssertions.AssertTemplateResponse(template, type);
            }
        }

        [Test, Category("Integration"), Category("Integration/NotificationClientAsync")]
        public async Task GetAllEmailTemplates()
        {
            const string type = "email";
            TemplateList templateList = await this.client.GetAllTemplatesAsync(type);
            Assert.IsNotNull(templateList);
            Assert.AreNotEqual(templateList.templates.Count, 0);

            foreach (var template in templateList.templates)
            {
                NotifyAssertions.AssertTemplateResponse(template, type);
            }
        }

        [Test, Category("Integration"), Category("Integration/NotificationClientAsync")]
        public void GetAllInvalidTemplatesRaisesClientException()
        {
            const string type = "invalid";

            var ex = Assert.ThrowsAsync<NotifyClientException>(async () => await this.client.GetAllTemplatesAsync(type));
            Assert.That(ex.Message, Does.Contain("type invalid is not one of [sms, email, letter, broadcast]"));
        }

        [Test, Category("Integration"), Category("Integration/NotificationClientAsync")]
        public async Task GetSMSTemplateWithId()
        {
            TemplateResponse template = await this.client.GetTemplateByIdAsync(SMS_TEMPLATE_ID);
            Assert.AreEqual(template.id, SMS_TEMPLATE_ID);
            Assert.AreEqual(template.body, TEST_TEMPLATE_SMS_BODY);
        }

        [Test, Category("Integration"), Category("Integration/NotificationClientAsync")]
        public async Task GetEmailTemplateWithId()
        {
            TemplateResponse template = await this.client.GetTemplateByIdAsync(EMAIL_TEMPLATE_ID);
            Assert.AreEqual(template.id, EMAIL_TEMPLATE_ID);
            Assert.AreEqual(template.body, TEST_TEMPLATE_EMAIL_BODY);
        }

        [Test, Category("Integration"), Category("Integration/NotificationClientAsync")]
        public async Task GenerateSMSPreviewWithPersonalisation()
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                { "name", "someone" }
            };

            TemplatePreviewResponse response =
                await this.client.GenerateTemplatePreviewAsync(SMS_TEMPLATE_ID, personalisation);

            Assert.IsNotNull(response);
            Assert.AreEqual(response.body, TEST_SMS_BODY);
            Assert.AreEqual(response.subject, null);
        }

        [Test, Category("Integration"), Category("Integration/NotificationClientAsync")]
        public async Task GenerateEmailPreviewWithPersonalisation()
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                { "name", "someone" }
            };

            TemplatePreviewResponse response =
                await this.client.GenerateTemplatePreviewAsync(EMAIL_TEMPLATE_ID, personalisation);

            Assert.IsNotNull(response);
            Assert.AreEqual(response.body, TEST_EMAIL_BODY);
            Assert.AreEqual(response.subject, TEST_EMAIL_SUBJECT);
        }

        [Test, Category("Integration"), Category("Integration/NotificationClientAsync")]
        public void GenerateEmailPreviewWithMissingPersonalisationRaisesClientException()
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                { "invalid", "personalisation" }
            };

            var ex = Assert.ThrowsAsync<NotifyClientException>(async () =>
                await this.client.GenerateTemplatePreviewAsync(EMAIL_TEMPLATE_ID, personalisation)
            );
            Assert.That(ex.Message, Does.Contain("Missing personalisation: name"));
        }

        [Test, Category("Integration"), Category("Integration/NotificationClientAsync")]
        public async Task SendEmailTestServiceDefaultEmailReplyTo()
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                { "name", "someone" }
            };

            EmailNotificationResponse response = await this.client.SendEmailAsync(FUNCTIONAL_TEST_EMAIL, EMAIL_TEMPLATE_ID, personalisation);
            this.emailNotificationId = response.id;
            Assert.IsNotNull(response);
            Assert.AreEqual(response.content.body, TEST_EMAIL_BODY);
            Assert.AreEqual(response.content.subject, TEST_EMAIL_SUBJECT);
        }

        [Test, Category("Integration"), Category("Integration/NotificationClientAsync")]
        public async Task SendEmailTestSpecificEmailReplyTo()
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                { "name", "someone" }
            };

            EmailNotificationResponse response = await this.client.SendEmailAsync(FUNCTIONAL_TEST_EMAIL, EMAIL_TEMPLATE_ID, personalisation, emailReplyToId: EMAIL_REPLY_TO_ID);
            this.emailNotificationId = response.id;
            Assert.IsNotNull(response);
            Assert.AreEqual(response.content.body, TEST_EMAIL_BODY);
            Assert.AreEqual(response.content.subject, TEST_EMAIL_SUBJECT);
        }

        [Test, Category("Integration"), Category("Integration/NotificationClientAsync")]
        public void SendEmailTestEmailReplyToNotPresent()
        {
            string fakeReplayToId = Guid.NewGuid().ToString();
            var personalisation = new Dictionary<string, dynamic>
            {
                { "name", "someone" }
            };

            var ex = Assert.ThrowsAsync<NotifyClientException>(async () => await this.client.SendEmailAsync(FUNCTIONAL_TEST_EMAIL, EMAIL_TEMPLATE_ID, personalisation, emailReplyToId: fakeReplayToId));
            Assert.That(ex.Message, Does.Contain("email_reply_to_id " + fakeReplayToId));
        }

        [Test, Category("Integration"), Category("Integration/NotificationClientAsync")]
        public async Task SendEmailTestAllArguments()
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                { "name", "someone" }
            };

            EmailNotificationResponse response = await this.client.SendEmailAsync(FUNCTIONAL_TEST_EMAIL, EMAIL_TEMPLATE_ID, personalisation, clientReference: "TestReference", emailReplyToId: EMAIL_REPLY_TO_ID);
            this.emailNotificationId = response.id;
            Assert.IsNotNull(response);
            Assert.AreEqual(response.content.body, TEST_EMAIL_BODY);
            Assert.AreEqual(response.content.subject, TEST_EMAIL_SUBJECT);
        }

        [Test, Category("Integration"), Category("Integration/NotificationClientAsync")]
        public async Task SendSmsTestWithPersonalisationAndSmsSenderId()
        {
            var personalisation = new Dictionary<string, dynamic>
            {
                { "name", "someone" }
            };

            var client_sending = new NotificationClient(NOTIFY_API_URL, API_SENDING_KEY);

            SmsNotificationResponse response =
                await client_sending.SendSmsAsync(FUNCTIONAL_TEST_NUMBER, SMS_TEMPLATE_ID, personalisation, "sample-test-ref", SMS_SENDER_ID);
            this.smsNotificationId = response.id;
            Assert.IsNotNull(response);
            Assert.AreEqual(response.content.body, TEST_SMS_BODY);

            Assert.IsNotNull(response.reference);
            Assert.AreEqual(response.reference, "sample-test-ref");
        }

        [Test, Category("Integration"), Category("Integration/NotificationClientAsync")]
        public async Task SendMessageBoxNotificationAsyncTest()
        {
            var attachments = new List<Attachment>
            {
                new Attachment
                {
                    file = Convert.ToBase64String(Encoding.UTF8.GetBytes("Hello world")),
                    filename = "test.txt"
                }
            };

            var response = await client.SendMessageBoxNotificationAsync(
                sender: "12345678901234567890",
                recipient: "123456789",
                message: "Test message content",
                subject: "Test subject",
                attachments: attachments,
                reference: "async-test-ref"
            );

            Assert.IsNotNull(response);
            Assert.IsNotNull(response.Id);
            Assert.AreEqual("async-test-ref", response.Reference);
        }

        [Test, Category("Integration"), Category("Integration/NotificationClientAsync")]
        public async Task GetMessageBoxNotificationByIdAsyncTest()
        {
            var attachments = new List<Attachment>
            {
                new Attachment
                {
                    file = Convert.ToBase64String(Encoding.UTF8.GetBytes("Hello world")),
                    filename = "test.txt"
                }
            };

            var sendResponse = await client.SendMessageBoxNotificationAsync(
                sender: "12345678901234567890",
                recipient: "123456789",
                message: "Test message content",
                attachments: attachments,
                reference: "get-test-ref"
            );

            var notification = await client.GetNotificationByIdAsync(sendResponse.Id);

            Assert.IsNotNull(notification);
            Assert.AreEqual(sendResponse.Id, notification.id);
            Assert.AreEqual("get-test-ref", notification.reference);
            Assert.AreEqual("Test message content", notification.body);
        }
    }
}