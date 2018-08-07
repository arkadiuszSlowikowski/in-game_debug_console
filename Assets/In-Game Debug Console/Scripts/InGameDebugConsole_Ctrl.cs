using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InGameDebugConsole_Ctrl : MonoBehaviour
{
	public static InGameDebugConsole_Ctrl ciop;

	public enum WINDOW_SIZE { BIG = 2, SMALL = 1, NONE = 0 }
	public WINDOW_SIZE windowSize = WINDOW_SIZE.NONE;
	public enum SMALL_WINDOW_POSITION { UP_CENTER, UP_RIGHT, CENTER_RIGHT, DOWN_RIGHT, DOWN_CENTER, DOWN_LEFT, CENTER_LEFT, UP_LEFT }
	public SMALL_WINDOW_POSITION smallWindowPosition = SMALL_WINDOW_POSITION.DOWN_CENTER;
	public enum STACKING_MODE { EVERY_MESSAGE, CONSECUTIVE_MESSAGES, NONE }
	public STACKING_MODE stackingMode = STACKING_MODE.CONSECUTIVE_MESSAGES;

	private Vector2 smallWindowScale = new Vector2(0.8f, 0.3f);
	public Vector2 SmallWindowScale
	{
		get
		{
			return smallWindowScale;
		}

		set
		{
			for (int i = 0; i < 2; i++)
			{
				if (value[i] > 1)
				{
					value[i] = 1;
				}
				else if (value[i] < 0)
				{
					value[i] = 0;
				}
			}

			smallWindowScale = value;
		}
	}

	[SerializeField]
	private Tray_IGDC_Ctrl tray_ctrl;
	[SerializeField]
	private Notification_IGDC not_prefab;
	[SerializeField]
	private RectTransform nots_parent;
	[SerializeField]
	private ShowingStackTrace_IGDC stackTraceShower;

	[Header("-----------")]
	public bool showOnError;
	[Header("Size of the screen at the beggining of game. If 0 x 0, size will be automaticly set")]
	[SerializeField]
	private Vector2 originalSize = Vector2.zero;
	[Header("Check if you want to show notification when any log message is received")]
	[SerializeField]
	private bool showWhenCreated = false;
	[Header("How much time got to past until notification disappears")]
	[SerializeField]
	private float timeOfExposition = 5;
	[Header("----------")]
	[Header("Number of notifications to be kept in the in-game debug console")]
	public int keep_number = -1;
	[Header("----------")]
	[SerializeField]
	private CanvasScaler canvasScaler;
	[SerializeField]
	private RectTransform stuffContainer;


	public List<Notification_IGDC> nots_alive = new List<Notification_IGDC>();
	[SerializeField]
	private List<Message_IGDC> messages = new List<Message_IGDC>();
	private Vector2 previousSize;
	private CursorLockMode prev_cursorLockMode;
	private bool prev_cursorVisibility;

	private void Awake()
	{
		if (!ciop)
		{
			ciop = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void Start()
	{
		if (originalSize == Vector2.zero)
		{
			originalSize = new Vector2(Screen.width, Screen.height);
		}

		prev_cursorLockMode = Cursor.lockState;
		prev_cursorVisibility = Cursor.visible;

		Application.logMessageReceived += Application_logMessageReceived;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Z))
		{
			Debug.Log("It's a log");
		}
		if (Input.GetKeyDown(KeyCode.X))
		{
			Debug.LogWarning("It's a warning");
		}
		if (Input.GetKeyDown(KeyCode.C))
		{
			Debug.LogError("It's an error");
		}

		TurnOnOrOff();
		Resize(false);
	}

	private void Application_logMessageReceived(string condition, string stackTrace, LogType type)
	{
		if (showOnError && !stuffContainer.gameObject.activeInHierarchy && (type == LogType.Error || type == LogType.Exception))
		{
			OpenWindow(true);
		}

		Message_IGDC message = new Message_IGDC(condition, stackTrace, type);
		messages.Add(message);

		if (keep_number > -1)
		{
			float difference_msg = messages.Count - keep_number;

			if (messages.Count > keep_number)
			{
				for (int i = 0; i < difference_msg; i++)
				{
					messages.RemoveAt(i);
				}
			}

			float difference_nots = nots_parent.childCount - messages.Count;

			if (nots_parent.childCount > messages.Count -1 )
			{
				for (int i = 0; i < difference_nots; i++)
				{
					Destroy(nots_alive[i].gameObject);
					nots_alive.RemoveAt(i);
				}
			}
		}

		switch (stackingMode)
		{
			case STACKING_MODE.CONSECUTIVE_MESSAGES:
				if (nots_alive.Count > 0 && nots_alive[nots_alive.Count - 1].message.condition == condition &&
					nots_alive[nots_alive.Count - 1].message.stackTrace == stackTrace)
				{
					nots_alive[nots_alive.Count - 1].InStack++;
					return;
				}
				break;

			case STACKING_MODE.EVERY_MESSAGE:
				foreach (Notification_IGDC _not in nots_alive)
				{
					if (_not.message.condition == condition &&
					_not.message.stackTrace == stackTrace)
					{
						_not.InStack++;
						return;
					}
				}
				break;

			case STACKING_MODE.NONE:

				break;
		}

		Notification_IGDC not = Instantiate(not_prefab, nots_parent);
		not.SetupNotification(message, delegate { ShowNotificationsStackTrace(not); });

		MoveNotifications();
		if (!stuffContainer.gameObject.activeInHierarchy)
		{
			tray_ctrl.NewUnreadMessage();
		}

		Resize(true);

		//TODO start coroutine showing the notification and then hiding it
	}

	private void TurnOnOrOff()
	{
		bool areModifiers = Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt);

		if (areModifiers && Input.GetKeyDown(KeyCode.B) || areModifiers && Input.GetKeyDown(KeyCode.S))                                 //BIG
		{
			OpenWindow(false);
		}
	}

	public void Resize(bool must)
	{
		Vector2 size = new Vector2(Screen.width, Screen.height);
		Vector2 windowSize = size * (this.windowSize == WINDOW_SIZE.SMALL ? SmallWindowScale : Vector2.one);

		if (previousSize != size || must)
		{
			float scale = windowSize.y / originalSize.y;
			tray_ctrl.Resize(size.y / originalSize.y);
			canvasScaler.referenceResolution = size;
			stuffContainer.sizeDelta = windowSize;
			nots_parent.localPosition = -(windowSize.y / 2f) * Vector2.up;

			//TODO zrobic tak zeby stackTraceShower sie nie zmneijszal przy zmianie skali malego okna

			float sumOfSizes_y = 0;

			for (int i = 0; i < nots_alive.Count; i++)
			{
				Image not_image = nots_parent.GetChild(i).GetComponent<Image>();
				not_image.rectTransform.sizeDelta = new Vector2(not_image.rectTransform.sizeDelta.x, scale * Notification_IGDC.originalHeight);

				sumOfSizes_y += scale * Notification_IGDC.originalHeight;
			}

			nots_parent.sizeDelta = new Vector2(nots_parent.sizeDelta.x, sumOfSizes_y);

			MoveNotifications();

			previousSize = windowSize;
		}
	}

	private void MoveNotifications()
	{
		for (int i = 0; i < nots_alive.Count; i++)
		{
			nots_alive[i].transform.localPosition = new Vector2(
				0,
				(nots_alive.Count - i - 1) * nots_alive[i > 0 ? i - 1 : 0].NotificationHeight
				);
		}
	}

	public void MoveSmallWindow()
	{
		Vector2 pivot = Vector2.zero;
		Vector2 position = Vector2.zero;

		switch (smallWindowPosition)
		{
			case SMALL_WINDOW_POSITION.UP_CENTER:
				pivot = new Vector2(0.5f, 1f);
				position = new Vector2(0, Screen.height / 2f);
				break;

			case SMALL_WINDOW_POSITION.UP_RIGHT:
				pivot = new Vector2(1f, 1f);
				position = new Vector2(Screen.width / 2f, Screen.height / 2f);
				break;

			case SMALL_WINDOW_POSITION.CENTER_RIGHT:
				pivot = new Vector2(1f, 0.5f);
				position = new Vector2(Screen.width / 2f, 0);
				break;

			case SMALL_WINDOW_POSITION.DOWN_RIGHT:
				pivot = new Vector2(1f, 0f);
				position = new Vector2(Screen.width / 2f, -Screen.height / 2f);
				break;

			case SMALL_WINDOW_POSITION.DOWN_CENTER:
				pivot = new Vector2(0.5f, 0f);
				position = new Vector2(0, -Screen.height / 2f);
				break;

			case SMALL_WINDOW_POSITION.DOWN_LEFT:
				pivot = new Vector2(0f, 0f);
				position = new Vector2(-Screen.width / 2f, -Screen.height / 2f);
				break;

			case SMALL_WINDOW_POSITION.CENTER_LEFT:
				pivot = new Vector2(0f, 0.5f);
				position = new Vector2(-Screen.width / 2f, 0);
				break;

			case SMALL_WINDOW_POSITION.UP_LEFT:
				pivot = new Vector2(0f, 1f);
				position = new Vector2(-Screen.width / 2f, Screen.height / 2f);
				break;
		}

		stuffContainer.pivot = pivot;
		stuffContainer.localPosition = position;
	}

	public void ShowNotificationsStackTrace(Notification_IGDC not)
	{
		stackTraceShower.Show(not.message.stackTrace);
	}

	public void OpenWindow(bool must)
	{
		if (Input.GetKeyDown(KeyCode.B))
		{
			windowSize = WINDOW_SIZE.BIG;
		}
		else if (Input.GetKeyDown(KeyCode.S))
		{
			windowSize = WINDOW_SIZE.SMALL;
			MoveSmallWindow();
		}

		if (must)
		{
			MoveSmallWindow();
		}

		stuffContainer.gameObject.SetActive(!stuffContainer.gameObject.activeInHierarchy);

		if (stuffContainer.gameObject.activeInHierarchy)
		{
			prev_cursorLockMode = Cursor.lockState;
			prev_cursorVisibility = Cursor.visible;

			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		else
		{
			Cursor.lockState = prev_cursorLockMode;
			Cursor.visible = prev_cursorVisibility;
		}

		Resize(true);
		if (stuffContainer.gameObject.activeInHierarchy)
		{
			tray_ctrl.WindowOpened();
		}
	}

	public void ChangeStackingMode()
	{
		for (int i = 0; i < nots_alive.Count; i++)
		{
			Destroy(nots_alive[i].gameObject);
		}

		nots_alive.Clear();

		Notification_IGDC curr_not = null;

		switch (stackingMode)
		{
			case STACKING_MODE.CONSECUTIVE_MESSAGES:
				for (int i = 0; i < messages.Count; i++)
				{
					if (i > 0 &&
						messages[i].condition == messages[i - 1].condition &&
						messages[i].stackTrace == messages[i - 1].stackTrace)
					{
						curr_not.InStack++;
					}
					else
					{
						curr_not = Instantiate(not_prefab, nots_parent);
						curr_not.SetupNotification(messages[i], delegate { ShowNotificationsStackTrace(curr_not); });
						tray_ctrl.NewUnreadMessage();
					}
				}
				break;

			case STACKING_MODE.EVERY_MESSAGE:
				bool found;

				for (int m = 0; m < messages.Count; m++)
				{
					found = false;

					for (int n = 0; n < nots_alive.Count; n++)
					{
						if (messages[m].stackTrace == nots_alive[n].message.stackTrace &&
							messages[m].condition == nots_alive[n].message.condition)
						{
							found = true;
							nots_alive[n].InStack++;
						}
					}

					if (!found)
					{
						curr_not = Instantiate(not_prefab, nots_parent);
						curr_not.SetupNotification(messages[m], delegate { ShowNotificationsStackTrace(curr_not); });
						tray_ctrl.NewUnreadMessage();
					}
				}
				break;

			case STACKING_MODE.NONE:
				foreach (Message_IGDC message in messages)
				{
					curr_not = Instantiate(not_prefab, nots_parent);
					curr_not.SetupNotification(message, delegate { ShowNotificationsStackTrace(curr_not); });
					tray_ctrl.NewUnreadMessage();
				}
				break;

		}

		MoveNotifications();
	}

	public void Clear()
	{
		messages.Clear();
		for (int i = 0; i < nots_alive.Count; i++)
		{
			Destroy(nots_alive[i].gameObject);
		}
		nots_alive.Clear();
	}
}