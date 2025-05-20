# FCM Test Tool

**Author:** Nathan
**Date:** May 20, 2025

## Description

FCM Test Tool is a simple .NET Framework console application designed to test sending Firebase Cloud Messaging (FCM) messages to a specific device token. It's primarily useful for:

* Diagnosing FCM sending issues from a server environment.
* Verifying network connectivity to FCM services.
* Confirming the validity and configuration of your Firebase service account credentials.
* Testing the responsiveness of individual device tokens.

The application takes the device token, message title, and message body as command-line arguments, reads a Firebase service account key from a local JSON file, and attempts to send a push notification. It provides detailed output, including any error messages from the Firebase Admin SDK.

## Prerequisites

* **.NET Framework:** Version 4.6.2 or higher.
* **Firebase Project:** An active Firebase project with Firebase Cloud Messaging API enabled.
* **Service Account Key:** A JSON private key file for a service account associated with your Firebase project. This service account must have permissions to send FCM messages (e.g., the "Firebase Cloud Messaging API Admin" role or a role with `cloudmessaging.messages.create` permission).
* **FCM Device Token:** A valid FCM registration token for a target client device (Android, iOS, or Web).
* **Firebase Admin SDK:** The program relies on the `FirebaseAdmin` NuGet package.

## Setup

1.  **Get the Code:**
    * If you have the source code as a Visual Studio project, open it.
    * If you only have the `FCMTest.exe` (or similarly named executable), proceed to step 4.

2.  **Install Dependencies (if building from source):**
    * Open the project in Visual Studio.
    * Ensure the `FirebaseAdmin` NuGet package is installed. If not, right-click on the project in Solution Explorer -> "Manage NuGet Packages..." -> Search for `FirebaseAdmin` and install the latest stable version.

3.  **Build the Project (if building from source):**
    * In Visual Studio, build the solution (Build -> Build Solution). This will generate an executable file (e.g., `FCMTest.exe`) typically located in the `bin\Debug` or `bin\Release` subfolder of your project directory.

4.  **Prepare Service Account Key:**
    * Navigate to your [Firebase Console](https://console.firebase.google.com/).
    * Select your project.
    * Go to "Project settings" (click the gear icon near "Project Overview").
    * Select the "Service accounts" tab.
    * Click the "Generate new private key" button and confirm. A JSON file will be downloaded.
    * **Rename this downloaded JSON file to `service-account.json`**.

5.  **Place Files:**
    * Copy the compiled executable (e.g., `FCMTest.exe`) to a directory on your server or testing machine.
    * Copy the `service-account.json` file into the **same directory** as the executable.

## Usage

1.  Open a Command Prompt (cmd.exe) or PowerShell.
2.  Navigate (`cd`) to the directory where you placed `FCMTest.exe` and `service-account.json`.
3.  Run the program using the following command structure:

    ```bash
    FCMTest.exe <deviceToken> <messageTitle> <messageBody> [sound]
    ```

    **Command-Line Arguments:**

    * `<deviceToken>`: (Required) The FCM registration token of the target device. If the token contains special characters, it's a good practice to enclose it in double quotes.
    * `<messageTitle>`: (Required) The title for the push notification. Enclose in double quotes if it contains spaces (e.g., "My Test Title").
    * `<messageBody>`: (Required) The main content of the push notification. Enclose in double quotes if it contains spaces (e.g., "This is the message body.").
    * `[sound]`: (Optional) The sound to be played when the notification is received.
        * Defaults to `"default"` if not specified.
        * For Android, this is the name of a sound resource in your app.
        * For iOS (APNS), this can be `"default"` or the name of a custom sound file included in your app's bundle (e.g., `"my_alert.caf"`).

### Examples

**1. Basic Test:**

```bash
FCMTest.exe "YOUR_DEVICE_REGISTRATION_TOKEN_HERE" "Test From Server" "This is a test message to check FCM setup."
