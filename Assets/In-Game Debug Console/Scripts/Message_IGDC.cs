using System;
using UnityEngine;

[Serializable]
public class Message_IGDC
{
	public string condition;
	public string stackTrace;
	public LogType logType;

	public Message_IGDC(string condition, string stackTrace, LogType logType)
	{
		this.condition = condition;
		this.stackTrace = stackTrace;
		this.logType = logType;
	}
}