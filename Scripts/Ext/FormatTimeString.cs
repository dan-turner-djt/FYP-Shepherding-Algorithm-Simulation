using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormatTimeString 
{
	public static string FormatTextForShowing(float time)
	{
		float t = time;

		float minutes = t / 60;
		string minutesTens = ((int)minutes / 10).ToString();
		string minutesUnits = ((int)minutes % 10).ToString();
		float seconds = t % 60;
		string secondsTens = ((int)seconds / 10).ToString();
		string secondsUnits = ((int)seconds % 10).ToString();
		float decimalSeconds = (t % 1) * 100;
		string decimalSecondsTens = ((int)decimalSeconds / 10).ToString();
		string decimalSecondsUnits = ((int)decimalSeconds % 10).ToString();

		return minutesTens + "" + minutesUnits + ":" + secondsTens + "" + secondsUnits + "." + decimalSecondsTens + "" + decimalSecondsUnits;
	}
}
