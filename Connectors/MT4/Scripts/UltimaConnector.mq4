#define VERSION "1.6"

#property copyright "Marcin Deptu³a"
#property link ""
#property strict
#property icon "\\Images\\logo64.ico"
#property version VERSION

#include <stdlib.mqh>
#include <UltimaConnector.Native.mqh>
#include <UltimaConnector.Common.mqh>
#include <UltimaConnector.mqh>


#include <Common.mqh>

input ScriptExecMode ExecutionMode = Instant;
input string ServerAddress = "127.0.0.1:6300";
extern string SymbolSuffix = "";
extern int SlippageDivider = 1;
extern string SlippageDividers = "";
extern string BrokerAlias = "";


bool isRunning;
string registeredInstruments [];
double bids [];
double asks [];

int OnInit()
{
	Print("[Debug] OnInit");
	isRunning = false;

	if (!IsDllsAllowed())
	{
		Alert("DLL Import disabled");
		return(INIT_FAILED);
	}

	if (!IsExpertEnabled())
	{
		Alert("Expert Advisors disabled");
		return(INIT_FAILED);
	}

	CreateVersionLabel(VERSION);

	string company = AccountCompany();
	if (StringLen(BrokerAlias) > 0)
		company = BrokerAlias;
	string dataPath = TerminalInfoString(TERMINAL_DATA_PATH);

	Print("[Debug] Loading Ultima.Meta4.dll...");
	int res = Initialize(dataPath, company, ServerAddress);
	if (res == false)
	{
		Alert("Initialize Error");
		return(INIT_FAILED);
	}
	Print("[Debug] OK");

	ArrayResize(registeredInstruments, 0);
	ArrayResize(bids, 0);
	ArrayResize(asks, 0);

	isRunning = true;

	Run();
	return(0);
}

void OnDeinit(const int reason)
{
	Print("[Debug] OnDeinit. Reason: " + GetUninitReasonText(reason));

	DeleteVersionLabel();

	Print("[Debug] Calling DeInitialize...");
	DeInitialize();
	Print("[Debug] OK");

	isRunning = false;
	return;
}

void OnTick()
{
	return;
}

int Run()
{
	Print("[Debug] Run called");

	string symbol;
	string symbolws;
	int arLen;
	int i;

	if (isRunning == false)
	{
		Print("[Debug] Run called, but isRunning = false");
		return(0);
	}

	while (true)
	{
		if (IsStopped())
		{
			Print("[Debug] " + __FUNCTION__ + ": IsStopped() = true");
			return(0);
		}

		if (WaitForCommand(50))
		{
			HandleRegisterCommands();
			HandleCloseOrderCommands();
			HandleCloseOrderByCommands();
			HandleOpenOrderCommands();
			HandleModifyOrderCommands();
			HandleRequestOrderHistoryCommands();
		}

		RefreshRates();

		arLen = ArraySize(registeredInstruments);
		for (i = 0; i < arLen; i++)
		{
			symbol = registeredInstruments[i];
			symbolws = SymbolWS(symbol);

			Tick t;
			StringToCharArray(symbol, t.Symbol);
			t.Bid = MarketInfo(symbolws, MODE_BID);
			t.Ask = MarketInfo(symbolws, MODE_ASK);

			if (t.Bid != bids[i] ||
				t.Ask != asks[i])
			{
				bids[i] = t.Bid;
				asks[i] = t.Ask;
				UpdatePrice(t);
			}
		}

		UpdateOrdersImpl();
	}

	return(0);
}

void HandleRegisterCommands()
{
	SymbolRegistrationCommand cmd;
	while (GetSymbolRegCommand(cmd))
	{
		string symbol = CharArrayToString(cmd.Symbol);
		bool reg = (bool) cmd.Register;
		Print("Registering symbol ", symbol, ", registering: ", reg);

		HandleRegisterSymbol(symbol, reg);
	}
}

void HandleCloseOrderCommands()
{
	CloseOrderCommand cmd;
	while (GetCloseOrderCommand(cmd))
	{
		CloseOrder(cmd.Command, cmd.Order, cmd.Retries, cmd.RetrySpanMs);
	}
}

void HandleCloseOrderByCommands()
{
	CloseOrderByCommand cmd;
	while (GetCloseOrderByCommand(cmd))
	{
		CloseOrderBy(cmd.Command, cmd.Order, cmd.OrderBy, cmd.Retries, cmd.RetrySpanMs);
	}
}

void HandleOpenOrderCommands()
{
	OpenOrderCommand cmd;

	while (GetOpenOrderCommand(cmd))
	{
		Print("[Debug] Received open order command.");
		OpenPosition(cmd.Command, CharArrayToString(cmd.Symbol), cmd.TradeCommand,
			cmd.Volume, cmd.OpenPrice, cmd.Slippage, cmd.StopLoss, cmd.TakeProfit,
			CharArrayToString(cmd.Comment), cmd.MagicNumber,
			cmd.Retries, cmd.RetrySpanMs, cmd.LastChanceRetrySpanMs);
	}
}

void HandleModifyOrderCommands()
{
	ModifyOrderCommand cmd;
	while (GetModifyOrderCommand(cmd))
	{
		ModifyOrder(cmd.Command, cmd.Order, cmd.OpenPrice, cmd.StopLoss, cmd.TakeProfit,
			cmd.Retries, cmd.RetrySpanMs);
	}
}

void HandleRequestOrderHistoryCommands()
{
	OrderHistoryCommand cmd;
	while (GetRequestOrderHistory(cmd))
	{
		RequestOrderHistory(cmd.Command);
	}
}