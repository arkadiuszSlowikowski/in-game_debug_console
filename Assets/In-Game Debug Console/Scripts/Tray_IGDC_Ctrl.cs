using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Tray_IGDC_Ctrl : MonoBehaviour
{
	[SerializeField]
	private Image messageButton_image;
	[SerializeField]
	private Image autohideButton_image;
	[SerializeField]
	private Image settingsButton_image;
	[SerializeField]
	private Image closeButton_image;

	[SerializeField]
	private Settings_IGDC_Ctrl settings_ctrl;
	[SerializeField]
	private Image background;
	[SerializeField]
	private Text newMessages_text;
	[SerializeField]
	private Image autohide_image;
	[Header("----------")]
	[SerializeField]
	private float timeToWaitBeforeShowUp;

	private int unreadMessages;
	private int UnreadMessages
	{
		get
		{
			return unreadMessages;
		}

		set
		{
			unreadMessages = value;

			if (unreadMessages > 9999)
			{
				unreadMessages = 9999;
			}

			newMessages_text.text = unreadMessages.ToString();
		}
	}

	private Coroutine slidingCoroutine;
	private Vector3 originalSize;
	private float timeIn;
	private bool doAutohide = true;
	private bool isPointerDown;

	private void Start()
	{
		originalSize = background.rectTransform.sizeDelta;
	}

	private void Update()
	{
		if (isPointerDown)
		{
			timeIn += Time.deltaTime;

			if (timeIn >= timeToWaitBeforeShowUp)
			{
				isPointerDown = false;

				if (slidingCoroutine != null)
				{
					StopCoroutine(slidingCoroutine);
				}

				slidingCoroutine = StartCoroutine(HideOrShow(false));
			}
		}
		else
		{
			timeIn = 0;
		}
	}

	public void PointerEnter()
	{
		isPointerDown = true;
	}

	public void PointerExit()
	{
		isPointerDown = false;

		if (slidingCoroutine != null)
		{
			StopCoroutine(slidingCoroutine);
		}

		slidingCoroutine = StartCoroutine(HideOrShow(true));
	}

	public void NewUnreadMessage()
	{
		UnreadMessages++;
	}

	public void WindowOpened()
	{
		UnreadMessages = 0;
	}

	public void OpenWindowButton_Clicked()
	{
		InGameDebugConsole_Ctrl.ciop.OpenWindow(true);
	}

	public void AutoHideButton_Clicked()
	{
		doAutohide = !doAutohide;

		autohide_image.color = doAutohide ? Color.green : Color.red;
	}

	public void SettingsButton_Clicked()
	{
		settings_ctrl.Switch(false);
	}

	public void ClearButton_Clicked()
	{
		InGameDebugConsole_Ctrl.ciop.Clear();
	}

	public void Resize(float scaleY)
	{
		background.rectTransform.sizeDelta = originalSize * scaleY;
		messageButton_image.rectTransform.sizeDelta = new Vector2(90, 90) * scaleY;
		autohideButton_image.rectTransform.sizeDelta = new Vector2(45, 45) * scaleY;
		closeButton_image.rectTransform.sizeDelta = new Vector2(45, 45) * scaleY;
		settingsButton_image.rectTransform.sizeDelta = new Vector2(90, 90) * scaleY;
	}

	[SerializeField]
	private float timeOfSliding;

	private IEnumerator HideOrShow(bool hide)
	{
		if (doAutohide)
		{
			float timer = 0;
			Vector2 pos0, pos1;
			pos0 = Vector2.zero;
			pos1 = Vector2.zero;

			if (hide)
			{
				pos0 = background.transform.localPosition;
				pos1 = new Vector2(0, -84);

				if (pos0 != Vector2.zero)
				{
					timer = background.transform.localPosition.y / 84f;
				}
			}
			else
			{
				pos0 = background.transform.localPosition;
				pos1 = Vector2.zero;

				if (pos0 != new Vector2(0, -84))
				{
					timer = background.transform.localPosition.y / 84f;
				}
			}

			while (timer < timeOfSliding)
			{
				timer += Time.deltaTime;

				background.transform.localPosition = Vector2.Lerp(pos0, pos1, timer / timeOfSliding);
				yield return 0;
			}

			slidingCoroutine = null;
		}
	}
}