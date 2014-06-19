//+------------------------------------------------------------------+
//|                                       UltimaConnector.Common.mq4 |
//|                                                   Marcin Deptu³a |
//|                                                                  |
//+------------------------------------------------------------------+
#property library
#property copyright "Marcin Deptu³a"
#property link      ""
#property version   "1.00"
#property strict

string GetUninitReasonText(int reasonCode) export
{
	string text = "";
	switch (reasonCode)
	{
	case REASON_ACCOUNT:
		text = "Account was changed";
		break;
	case REASON_CHARTCHANGE:
		text = "Symbol or timeframe was changed";
		break;
	case REASON_CHARTCLOSE:
		text = "Chart was closed";
		break;
	case REASON_PARAMETERS:
		text = "Input-parameter was changed";
		break;
	case REASON_RECOMPILE:
		text = "Program " + __FILE__ + " was recompiled";
		break;
	case REASON_REMOVE:
		text = "Program " + __FILE__ + " was removed from chart";
		break;
	case REASON_TEMPLATE:
		text = "New template was applied to chart";
		break;
	default:
		text = "Another reason";
	}

	return text;
}

bool IsPending(int type) export
{
	return (type == OP_BUYLIMIT || type == OP_BUYSTOP ||
		type == OP_SELLLIMIT || type == OP_SELLSTOP);
}

bool IsMarket(int type) export
{
	return (type == OP_BUY || type == OP_SELL);
}

string FillStringBuffer100() export
{
	return ("12345678901234567890123456789012345678901234567890" +
		"12345678901234567890123456789012345678901234567890" +
		"12345678901234567890123456789012345678901234567890" +
		"12345678901234567890123456789012345678901234567890" +
		"12345678901234567890123456789012345678901234567890" +
		"12345");
}

void LogStrArray(string& arr []) export
{
	int arLen = ArraySize(arr);

	string s = "";

	s = s + "[ ";
	for (int i = 0; i < arLen; i++)
	{
		s = s + arr[i];
		if (i + 1 < arLen)
		{
			s = s + ", ";
		}
	}
	s = s + " ]";

	Print(s);
}

bool ArrayContains(string& arr [], string element) export
{
	int arLen = ArraySize(arr);

	for (int i = 0; i < arLen; i++)
	{
		if (arr[i] == element)
			return (true);
	}

	return (false);
}