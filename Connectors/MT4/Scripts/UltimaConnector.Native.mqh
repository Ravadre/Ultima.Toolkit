//+------------------------------------------------------------------+
//|                                       UltimaConnector.Native.mqh |
//|                                                   Marcin Deptu³a |
//|                                                                  |
//+------------------------------------------------------------------+
#property copyright "Marcin Deptu³a"
#property link      "https://github.com/Ravadre"
#property strict

enum CommandResult
{
	Res_OK = 1,
	Res_OKPartial = 2,
	Res_Error = 3
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
	datetime OpenTime;
	datetime CloseTime;
};

#import "UltimaConnector.dll"
bool Initialize(string dataPath, string company, string server);
void DeInitialize();

bool HasCommands();
bool WaitForCommand(int timeoutMs);

bool GetSymbolRegCommand(SymbolRegistrationCommand& cmd);

bool GetCloseOrderCommand(CloseOrderCommand& cmd);
bool GetCloseOrderByCommand(CloseOrderByCommand& cmd);
bool GetOpenOrderCommand(OpenOrderCommand& cmd);
bool GetModifyOrderCommand(ModifyOrderCommand& cmd);
bool GetRequestOrderHistory(OrderHistoryCommand& cmd);

void ReportCommand(int command, int result, int order);

void UpdatePrice(Tick& tick);
void UpdateOrders(int orderCount, MT4Order& orders []);
void UpdateHistory(int command, int orderCount, MT4Order& orders []);
#import
