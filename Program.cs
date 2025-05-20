using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FCMTest
{

    class PushNotificationMessage
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Sound { get; set; }
        public int? Badges { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {


            var defautlApp = FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(@"C:\Users\pooik\Downloads\flutter-doctorsapp-prod-firebase-adminsdk-3dcer-64a9aa57b2.json")
            });

            SendFirebasePushNotificationWithPayload(defautlApp, new List<string> { "eX7tsKUPTxy6IR5HCGtlbR:APA91bFsRIGoxfxRxyVwpg6MyvQzu9elxBmfKYe-0oen11_bOfr20xjtrjA7X9Kh41thHXfWgKS7kYTD6JOCR9lSTHuq6DjU7s1WLP2-u3U7gFsNiO5BPrIz_WBq-cbLMGkJMYRKF25H" },

                new PushNotificationMessage
                {
                    Title = "test",
                    Body = "body"
                }

                );

            Console.ReadLine();

        }

        static async Task<IList<string>> SendFirebasePushNotificationWithPayload(FirebaseApp app, IList<string> tokens, PushNotificationMessage message)
        {

            var failedTokens = new List<string>();

            try
            {
                var readOnlyTokens = new System.Collections.ObjectModel.ReadOnlyCollection<string>(tokens);

                Notification notification = null;
                if (!string.IsNullOrEmpty(message.Title) || !string.IsNullOrEmpty(message.Body))
                {
                    notification = new Notification()
                    {
                        Title = message.Title,
                        Body = message.Body,
                    };
                }
                MulticastMessage multicastMessage = new MulticastMessage()
                {
                    Notification = notification,
                    Android = new AndroidConfig()
                    {
                        Notification = new AndroidNotification()
                        {
                            Sound = message.Sound
                        },
                        TimeToLive = TimeSpan.FromHours(20),
                        Priority = Priority.High,
                    },
                    Apns = new ApnsConfig()
                    {
                        Aps = new Aps()
                        {
                            Sound = message.Sound,
                            ContentAvailable = notification == null,
                        },
                     
                    },
                    Tokens = readOnlyTokens
                };


                var fcm = FirebaseMessaging.GetMessaging(app);
                var fcmResult = await fcm.SendEachForMulticastAsync(multicastMessage).ConfigureAwait(false);

                if (fcmResult.FailureCount > 0)
                {
                    for (var i = 0; i < fcmResult.Responses.Count; i++)
                    {

                        if (!fcmResult.Responses[i].IsSuccess)
                        {
                            failedTokens.Add(tokens[i]);
                            var response = fcmResult.Responses[i];
                            string errorMessage = "Unknown error";
                            string errorCode = "N/A";

                            if (response.Exception is FirebaseMessagingException fcmEx)
                            {
                                errorMessage = fcmEx.Message;
                                errorCode = fcmEx.MessagingErrorCode?.ToString() ?? "N/A";
                            }
                            else if (response.Exception != null)
                            {
                                errorMessage = response.Exception.Message;
                            }

                        }
                    }


                    return failedTokens;

                }

            }
            catch (Exception ex)
            {
            }
            return failedTokens;
        }


    }
}
