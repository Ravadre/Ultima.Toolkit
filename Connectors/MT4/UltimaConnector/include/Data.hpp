#pragma once

#include <cstdint>

#pragma pack(push, 1)


enum class CommandResult
{
	OK = 1,
	OKPartial = 2,
	Error = 3
};

struct SymbolRegistrationCommand
{
	char Symbol[16];
	int Register;
};

struct CloseOrderCommand
{
	int Command;
	int Order;
	int Retries;
	int RetrySpanMs;
};

struct CloseOrderByCommand
{
	int Command;
	int Order;
	int OrderBy;
	int Retries;
	int RetrySpanMs;
};

struct OpenOrderCommand
{
	int Command;
	char Symbol[16];
	int TradeCommand;
	double Volume;
	double OpenPrice;
	int Slippage;
	double StopLoss;
	double TakeProfit;
	char Comment[64];
	int MagicNumber;
	int Retries;
	int RetrySpanMs;
	int LastChanceRetrySpanMs;
};

struct ModifyOrderCommand
{
	int Command;
	int Order;
	double OpenPrice;
	double StopLoss;
	double TakeProfit;
	int Retries;
	int RetrySpanMs;
};

struct OrderHistoryCommand
{
	int Command;
};

struct Tick
{
	char Symbol[16];
	double Bid;
	double Ask;
};

struct MT4Order
{
	char Symbol[16];
	int Order;
	int TradeCommand;
	double Lots;
	double OpenPrice;
	double StopLoss;
	double TakeProfit;
	double ClosePrice;
	double Profit;
	double Commission;
	double Swap;
	int PointProfit;
	int64_t OpenTime;
	int64_t CloseTime;
};
#pragma pack(pop)