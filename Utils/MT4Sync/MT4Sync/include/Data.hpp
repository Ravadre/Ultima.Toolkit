#pragma once

#include <cstdint>

#pragma pack(push, 1)
struct MT4Order
{
	int Order;
	int Type;
	double Volume;

	double StopLoss;
	double TakeProfit;

	double OpenPrice;
	int64_t OpenTime;

	double ClosePrice;
	int64_t CloseTime;

	double Profit;
	double Commission;
	double Swap;

	char Comment[128];
	int MagicNumber;
};
#pragma pack(pop)