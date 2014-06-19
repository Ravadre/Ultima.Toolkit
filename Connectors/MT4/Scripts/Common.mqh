//+------------------------------------------------------------------+
//|                                                       Common.mqh |
//|                                                   Marcin Deptu�a |
//|                                                                  |
//+------------------------------------------------------------------+
#property copyright "Marcin Deptu�a"
#property link      ""
#property strict

#import "Common.ex4"
	string GetUninitReasonText(int reasonCode);
	bool IsPending(int type);
	bool IsMarket(int type);
	string FillStringBuffer100();
	void LogStrArray(string& arr[]);
	bool ArrayContains(string& arr[], string element);
#import
