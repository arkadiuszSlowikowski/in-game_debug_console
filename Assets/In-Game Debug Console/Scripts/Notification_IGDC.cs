using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Notification_IGDC : MonoBehaviour
{
	public static float originalHeight;

	public float NotificationHeight
	{
		get
		{
			return _image.rectTransform.rect.height;
		}
	}

	private int inStack;
	public int InStack
	{
		get
		{
			return inStack;
		}

		set
		{
			inStack = value;

			_text.text = inStack > 1 ? "[" + inStack + "] " + message.condition : message.condition;
		}
	}

	public Message_IGDC message;
	public Button _button;
	public Image _image;
	public Text _text;
	public RectTransform _rectTransform;

	[Header("----------")]
	[SerializeField]
	private Color log_color;
	[SerializeField]
	private Color warning_color;
	[SerializeField]
	private Color error_color;

	public void SetupNotification(Message_IGDC message, UnityAction call)
	{
		originalHeight = _image.rectTransform.rect.height;
		this.message = message;

		_text.text = message.condition;
		_button.onClick.AddListener(call);

		switch (message.logType)
		{
			case LogType.Assert:
			case LogType.Log:

				_image.color = log_color;

				break;
			case LogType.Warning:

				_image.color = warning_color;

				break;
			case LogType.Error:
			case LogType.Exception:

				_image.color = error_color;

				break;
		}

		inStack++;

		InGameDebugConsole_Ctrl.ciop.nots_alive.Add(this);
	}
}