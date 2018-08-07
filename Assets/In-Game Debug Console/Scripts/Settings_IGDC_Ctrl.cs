using System;
using UnityEngine;
using UnityEngine.UI;

public class Settings_IGDC_Ctrl : MonoBehaviour
{
	[SerializeField]
	private RectTransform background;
	[SerializeField]
	private Dropdown windowsSizeDropdown;
	[SerializeField]
	private Dropdown windowPositionDropdown;
	[SerializeField]
	private Dropdown stackingModeDropdown;
	[SerializeField]
	private InputField messagesToKeepInputField;
	[SerializeField]
	private InputField[] scaleCordInputFields;
	[SerializeField]
	private Toggle showOnErrorToggle;

	public void Switch(bool doTurnOff)
	{
		if (doTurnOff)
		{
			InGameDebugConsole_Ctrl.ciop.ChangeStackingMode();
			InGameDebugConsole_Ctrl.ciop.MoveSmallWindow();
			InGameDebugConsole_Ctrl.ciop.Resize(true);
		}

		background.gameObject.SetActive(doTurnOff ? false : !background.gameObject.activeInHierarchy);

		if (background.gameObject.activeInHierarchy)
		{
			windowsSizeDropdown.value = (int)InGameDebugConsole_Ctrl.ciop.windowSize;
			windowPositionDropdown.value = (int)InGameDebugConsole_Ctrl.ciop.smallWindowPosition;
			stackingModeDropdown.value = (int)InGameDebugConsole_Ctrl.ciop.stackingMode;
			messagesToKeepInputField.text = InGameDebugConsole_Ctrl.ciop.keep_number.ToString();
			scaleCordInputFields[0].text = InGameDebugConsole_Ctrl.ciop.SmallWindowScale.x.ToString();
			scaleCordInputFields[1].text = InGameDebugConsole_Ctrl.ciop.SmallWindowScale.y.ToString();
		}
	}

	public void WindowSizeDropdown_OnValueChanged()
	{
		InGameDebugConsole_Ctrl.ciop.windowSize = (InGameDebugConsole_Ctrl.WINDOW_SIZE)windowsSizeDropdown.value;
	}

	public void WindowPositionDropdown_OnValueChanged()
	{
		InGameDebugConsole_Ctrl.ciop.smallWindowPosition = (InGameDebugConsole_Ctrl.SMALL_WINDOW_POSITION)windowPositionDropdown.value;
	}

	public void StackingModeDropdown_OnValueChanged()
	{
		InGameDebugConsole_Ctrl.ciop.stackingMode = (InGameDebugConsole_Ctrl.STACKING_MODE)stackingModeDropdown.value;
	}

	public void MessagesToKeepInputField_OnEndEdit()
	{
		InGameDebugConsole_Ctrl.ciop.keep_number = Convert.ToInt32(messagesToKeepInputField.text) - 1;
	}

	public void SmallWindowScaleDropdown_OnValueChanged(string axis)
	{
		Vector2 scale = InGameDebugConsole_Ctrl.ciop.SmallWindowScale;

		if (axis == "x")
		{
			scale.x = float.Parse(scaleCordInputFields[0].text);
		}
		else if (axis == "y")
		{
			scale.y = float.Parse(scaleCordInputFields[1].text);
		}

		InGameDebugConsole_Ctrl.ciop.SmallWindowScale = scale;
	}

	public void ShowOnErrorToggle_OnValueChanged()
	{
		InGameDebugConsole_Ctrl.ciop.showOnError = showOnErrorToggle.isOn;
	}
}