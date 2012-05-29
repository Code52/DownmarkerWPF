﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MarkPad.XAML.Converters;

namespace MarkPad.Contracts
{
	public enum SpellingLanguages
	{
		[DisplayString("English (Australia)")]
		Australian,
		[DisplayString("English (Canada)")]
		Canadian,
		[DisplayString("English (United States)")]
		UnitedStates,
		[DisplayString("Spanish (Spain)")]
		Spain
	}
}
