using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FCMTest
{
    class PushNotificationMessage
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Sound { get; set; }
    }

    class Program
    {
        private const string ServiceAccountJsonFileName = "service-account.json";

        // Traditional entry point for older .NET Framework/C# versions
        public static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        // Your asynchronous logic is now in this method
        static async Task MainAsync(string[] args)
        {
            Console.WriteLine("Firebase FCM Test Tool");
            Console.WriteLine("----------------------");

            if (args.Length < 3)
            {
                PrintUsage();
                return;
            }

            string deviceToken = args[0];
            string messageTitle = args[1];
            string messageBody = args[2];
            string sound = (args.Length > 3) ? args[3] : "default";

            if (string.IsNullOrWhiteSpace(deviceToken))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Device token cannot be empty.");
                Console.ResetColor();
                PrintUsage();
                return;
            }
            if (string.IsNullOrWhiteSpace(messageTitle))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Message title cannot be empty.");
                Console.ResetColor();
                PrintUsage();
                return;
            }
            if (string.IsNullOrWhiteSpace(messageBody))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: Message body cannot be empty.");
                Console.ResetColor();
                PrintUsage();
                return;
            }

            Console.WriteLine($"Device Token: {deviceToken}");
            Console.WriteLine($"Title: {messageTitle}");
            Console.WriteLine($"Body: {messageBody}");
            Console.WriteLine($"Sound: {sound}");
            Console.WriteLine();

            string serviceAccountPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ServiceAccountJsonFileName);
            Console.WriteLine($"Looking for service account file at: {serviceAccountPath}");

            if (!File.Exists(serviceAccountPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: Service account file '{ServiceAccountJsonFileName}' not found in the application directory.");
                Console.ResetColor();
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                return;
            }

            FirebaseApp defaultApp = null;
            try
            {
                Console.WriteLine("Initializing Firebase App...");
                defaultApp = FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(serviceAccountPath)
                });
                Console.WriteLine($"Firebase App '{defaultApp.Name}' initialized successfully.");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error initializing Firebase App:");
                PrintExceptionDetails(ex);
                Console.ResetColor();
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine();
            var pushMessage = new PushNotificationMessage
            {
                Title = messageTitle,
                Body = messageBody,
                Sound = sound
            };

            await SendFirebasePushNotificationWithPayload(defaultApp, new List<string> { deviceToken }, pushMessage);

            Console.WriteLine();
            Console.WriteLine("Processing complete. Press any key to exit.");
            Console.ReadKey();
        }

        static void PrintUsage()
        {
            Console.WriteLine("\nUsage: FCMTest.exe <deviceToken> <messageTitle> <messageBody> [sound]");
            Console.WriteLine("Example: FCMTest.exe \"your_device_token\" \"Test Title\" \"Test Body\" \"my_sound.caf\"");
            Console.WriteLine($"\nMake sure '{ServiceAccountJsonFileName}' is in the same directory as the executable.");
        }

        static async Task SendFirebasePushNotificationWithPayload(FirebaseApp app, IList<string> tokens, PushNotificationMessage message)
        {
            Console.WriteLine("\nAttempting to send FCM message...");

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
                        Priority = Priority.High,
                        Notification = new AndroidNotification()
                        {
                            Sound = message.Sound
                        },
                    },
                    Apns = new ApnsConfig()
                    {
                        Aps = new Aps()
                        {
                            Sound = message.Sound,
                        },
                    },
                    Tokens = readOnlyTokens
                };

                var fcm = FirebaseMessaging.GetMessaging(app);
                Console.WriteLine($"Sending to {tokens.Count} token(s)...");
                BatchResponse fcmResult = await fcm.SendEachForMulticastAsync(multicastMessage).ConfigureAwait(false);

                Console.WriteLine($"FCM Send Result: SuccessCount: {fcmResult.SuccessCount}, FailureCount: {fcmResult.FailureCount}");

                if (fcmResult.FailureCount > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Failures occurred:");
                    for (var i = 0; i < fcmResult.Responses.Count; i++)
                    {
                        var response = fcmResult.Responses[i];
                        if (!response.IsSuccess)
                        {
                            Console.WriteLine($"  Token [{i}]: {tokens[i]} FAILED");
                            if (response.Exception is FirebaseMessagingException fcmEx)
                            {
                                Console.WriteLine($"    Error Code: {fcmEx.MessagingErrorCode}");
                                Console.WriteLine($"    Message: {fcmEx.Message}");
                                if (fcmEx.InnerException != null)
                                {
                                    Console.WriteLine($"    Inner Exception: {fcmEx.InnerException.Message}");
                                }
                            }
                            else if (response.Exception != null)
                            {
                                Console.WriteLine($"    General Exception: {response.Exception.Message}");
                                PrintExceptionDetails(response.Exception.InnerException, "    ");
                            }
                            else
                            {
                                Console.WriteLine("    An unknown error occurred (Exception object was null).");
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"  Token [{i}]: {tokens[i]} SUCCEEDED. Message ID: {response.MessageId}");
                            Console.ResetColor();
                        }
                    }
                    Console.ResetColor();
                }
                else if (fcmResult.SuccessCount > 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("All messages sent successfully!");
                    for (var i = 0; i < fcmResult.Responses.Count; i++)
                    {
                        var response = fcmResult.Responses[i];
                        Console.WriteLine($"  Token [{i}]: {tokens[i]} SUCCEEDED. Message ID: {response.MessageId}");
                    }
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine("No messages were processed (Success and Failure counts are zero). This might indicate an issue with the token list or an unexpected SDK state.");
                }
            }
            catch (FirebaseMessagingException fcmEx)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("A FirebaseMessagingException occurred during FCM operation:");
                Console.WriteLine($"  Error Code: {fcmEx.MessagingErrorCode}");
                Console.WriteLine($"  Message: {fcmEx.Message}");
                PrintExceptionDetails(fcmEx.InnerException, "  ");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An unexpected error occurred while sending FCM message:");
                PrintExceptionDetails(ex);
                Console.ResetColor();
            }
        }

        static void PrintExceptionDetails(Exception ex, string indent = "")
        {
            if (ex == null) return;
            Console.WriteLine($"{indent}Exception Type: {ex.GetType().FullName}");
            Console.WriteLine($"{indent}Message: {ex.Message}");
            Console.WriteLine($"{indent}Stack Trace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"{indent}Inner Exception:");
                PrintExceptionDetails(ex.InnerException, indent + "  ");
            }
        }
    }
}