using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class Account_Menu : MonoBehaviour
{
	public bool ShowDebugLogs = true;

	public GameObject UsernameField;
	public GameObject EmailField;
	public GameObject PasswordField;
	public GameObject RetypePasswordField;
	public GameObject SaveButton;
	public GameObject ChangePasswordButton;
	public GameObject BackButton;

	private void OnEnable()
	{
		// Establish listeners for all applicable events
		SessionManager.Instance.OnSMDisconnected += Disconnected_Event;

		UsernameField.GetComponent<InputField>().text = PlayerManager.Instance.Username;
		EmailField.GetComponent<InputField>().text = PlayerManager.Instance.Email;
	}
	
	private void OnDisable()
	{
		// Remove listeners for all applicable events
		SessionManager.Instance.OnSMDisconnected -= Disconnected_Event;
	}

	#region OnClick
	/// <summary>
	/// Event called when Save button is clicked
	/// </summary>
	public void Save_Click()
	{
		string email = EmailField.GetComponent<InputField>().text;
		string username = UsernameField.GetComponent<InputField>().text;
		var regexItem = new Regex("^[a-zA-Z0-9 ]+$");

		// Test to see if a proper username and password have been entered
		if(((email.Contains("@") && email.Contains(".")) || email == "") && regexItem.IsMatch(username))
		{
			// Attempt to save email and username to server
			StartCoroutine(UpdateAccount(EmailField.GetComponent<InputField>().text, UsernameField.GetComponent<InputField>().text));
		}
		else
		{
			// TO DO: Tell user the email or username are incorrect
		}
	}

	/// <summary>
	/// Event called when Change Password button is clicked
	/// </summary>
	public void ChangePassword_Click()
	{
		if(PasswordField.GetComponent<InputField>().text == RetypePasswordField.GetComponent<InputField>().text)
		{
			// Attempt to reset the user's password
			StartCoroutine(ChangePassword(PasswordField.GetComponent<InputField>().text));

			PasswordField.GetComponent<InputField>().text = "";
			RetypePasswordField.GetComponent<InputField>().text = "";
		}
		else
		{
			// TO DO: Tell user the two passwords need to match
		}
	}

	/// <summary>
	/// Event called when the Back button is clicked
	/// </summary>
	public void Back_Click()
	{
		MenuManager.Instance.ShowMainMenu();
	}
	#endregion

	#region SERVER METHODS
	/// <summary>
	/// IEnumerator that changes the password on the server for the current player and waits for a response from the server
	/// </summary>
	/// <returns></returns>
	/// <param name="newPassword">New password.</param>
	private IEnumerator ChangePassword(string newPassword)
	{
		Log ("Resetting Password");
		WWW www = new WWW("http://www.diademstudios.com/accounts/DBResetPassword.php?playerID=" + PlayerManager.Instance.PlayerID.ToString() + "&password=" + newPassword);

		while(!www.isDone)
		{
			yield return 0;
		}

		// Test for a failure to update the password and tell the user
		if(www.text == "Passed")
		{
			NotificationManager.Instance.DisplayNotification(new NotificationData("Success!", "Saved Password", "Success", 0, new Vector3(0, 0, 0)));
			Log ("Password Reset");
		}
		else
		{
			NotificationManager.Instance.DisplayNotification(new NotificationData("Error", "An error occurred when trying to save your new password.", "Error", 0, new Vector3(0, 0, 0)));
			LogError("Error Resetting Password");
		}
	}

	/// <summary>
	/// IEnumerator that changes the player's account details on the server for the current
	/// player and waits for a response from the server
	/// </summary>
	/// <returns></returns>
	/// <param name="email">Email</param>
	/// <param name="username">Username</param>
	private IEnumerator UpdateAccount(string email, string username)
	{
		Log ("Updating Account Details");
		WWW www = new WWW("http://www.diademstudios.com/accounts/DBUpdateAccount.php?playerID=" + PlayerManager.Instance.PlayerID.ToString() + "&email=" + email + "&username=" + username);
		
		while(!www.isDone)
		{
			yield return 0;
		}
		
		// Test for a failure to update the password and tell the user
		if(www.text == "Passed")
		{
			NotificationManager.Instance.DisplayNotification(new NotificationData("Success!", "Saved Password", "Success", 0, new Vector3(0, 0, 0)));
			Log ("Account Details Updated");
		}
		else
		{
			NotificationManager.Instance.DisplayNotification(new NotificationData("Error", "An error occurred when trying to save your new password.", "Error", 0, new Vector3(0, 0, 0)));
			LogError("Error Updating Account Details");
		}
	}
	#endregion

	#region Events
	private void Disconnected_Event()
	{
		// Transition to the start menu if disconnected from the network
		MenuManager.Instance.ShowStartMenu();
	}
	#endregion
	
	#region MessageHandling
	protected void Log(string message)
	{
		if(ShowDebugLogs)
			Debug.Log("[Account_Menu] " + message);
	}
	
	protected void LogError(string message)
	{
		Debug.LogError("[Account_Menu] " + message);
	}
	#endregion
}
