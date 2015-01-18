#define VERSION "1.8"

#property copyright "Marcin Deptu³a"
#property link "https://github.com/Ravadre"
#property strict
#property icon "\\Images\\logo64.ico"
#property version VERSION

// Global states shared by headers
#include <UltimaConnector.Common.mqh>

DebugState debugState;



#include <stdlib.mqh>
#include <Charts/Chart.mqh>
#include <UltimaConnector.Native.mqh>
#include <UltimaConnector.UI.mqh>
#include <UltimaConnector.mqh>


#include <Common.mqh>

input ScriptExecMode ExecutionMode = Instant;
input string ServerAddress = "127.0.0.1:6300";
input bool DebugMode = false;
extern string SymbolSuffix = "";
extern int SlippageDivider = 1;
extern string SlippageDividers = "";
extern string BrokerAlias = "";

CChart gChart;

bool isRunning;
string registeredInstruments [];
double bids [];
double asks [];

int OnInit()
{
	Print("[Debug] OnInit");
	MathSrand(GetTickCount());
	SetupChart(gChart);

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
 
	debugState.IsDebugging = DebugMode;
	debugState.IsGeneratingTicks = false;
	debugState.GenerateInLock = false;
	debugState.PriceOffset = 0;
	
	CreateUI();
	RefreshDebugState();

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

 	if (!EventSetMillisecondTimer(66))
 	{
 		Alert("Could not start the timer");
		return(INIT_FAILED); 		
 	}
	
	isRunning = true;
	
	return(0);
}

void OnDeinit(const int reason)
{
	Print("[Debug] OnDeinit. Reason: " + GetUninitReasonText(reason));

	DestroyUI();
	
	Print("[Debug] Calling DeInitialize...");
	DeInitialize();
	Print("[Debug] OK");

	EventKillTimer();
	isRunning = false;
	return;
}

void OnTick()
{
}

void OnTimer()
{
	if (isRunning == false)
	{
		Print("[Debug] Timer called, but isRunning = false");
		return;
	}

	if (IsStopped())
	{
		Print("[Debug] " + __FUNCTION__ + ": IsStopped() = true");
		return;
	}
	
	RefreshDebugState();

	if (WaitForCommand(1))
	{
		HandleRegisterCommands();
		HandleCloseOrderCommands();
		HandleCloseOrderByCommands();
		HandleOpenOrderCommands();
		HandleModifyOrderCommands();
		HandleRequestOrderHistoryCommands();
	}

	RefreshRates();

	
	if (!debugState.IsGeneratingTicks ||
		!debugState.IsDebugging)
	{
		UpdateTicks();
	}
	else
	{
		if (debugState.GenerateInLock)
			GenerateDebugLockedTicks();
		else
			GenerateDebugTicks();
	}

	UpdateOrdersImpl();

	return;
}

void OnChartEvent(const int id, const long& lparam, const double& dparam, const string& sparam)
{
	if (id == CHARTEVENT_OBJECT_CLICK)
	{
		if (sparam == "dp_genticks_btn") 
		{
			debugState.IsGeneratingTicks = !debugState.IsGeneratingTicks;     
			Print("[Debug] Generating ticks options = ", debugState.IsGeneratingTicks);  
		}
		else if (sparam == "dp_lockticks_btn")
		{
			debugState.GenerateInLock = !debugState.GenerateInLock;
			Print("[Debug] Generate ticks in lock = ", debugState.GenerateInLock);
		}
		else if (sparam == "dp_priceoffset_up_btn")
		{
			debugState.PriceOffset += 1;
		}
		else if (sparam == "dp_priceoffset_down_btn")
		{
			debugState.PriceOffset -= 1;
		}
		
		UnlockButtons();
	}
}

uint lastDebugTicksUpdate = 0;
void GenerateDebugTicks()
{
	uint tc = GetTickCount();
	if (tc - lastDebugTicksUpdate < 250)
		return;
	lastDebugTicksUpdate = tc;

	int arLen = ArraySize(registeredInstruments);
	for (int i = 0; i < arLen; i++)
	{
		string symbol = registeredInstruments[i];
		string symbolws = SymbolWS(symbol);
   
		Tick t;
	  
		if (bids[i] == 0 || asks[i] == 0)
		{
			t.Bid = MarketInfo(symbolws, MODE_BID);
			t.Ask = MarketInfo(symbolws, MODE_ASK);	
		}
		else
		{
			t.Bid = bids[i];
			t.Ask = asks[i];
		}
	  
		StringToCharArray(symbol, t.Symbol);
		t.Bid += ((MathRand() % 10) - 4) * MarketInfo(symbolws, MODE_POINT);
		t.Ask += ((MathRand() % 10) - 4) * MarketInfo(symbolws, MODE_POINT);
	  
		if (t.Ask < t.Bid)
			t.Ask = t.Bid;
	  
		bids[i] = t.Bid;
		asks[i] = t.Ask;
		UpdatePrice(t);
	} 
}

void GenerateDebugLockedTicks()
{
	uint tc = GetTickCount();
	if (tc - lastDebugTicksUpdate < 250)
		return;
	lastDebugTicksUpdate = tc;
	
	int arLen = ArraySize(registeredInstruments);
	for (int i = 0; i < arLen; i++)
	{
		string symbol = registeredInstruments[i];
		string symbolws = SymbolWS(symbol);
   
		Tick t;
		
		t.Bid = MarketInfo(symbolws, MODE_BID) + debugState.PriceOffset * MarketInfo(symbolws, MODE_POINT);
		t.Ask = MarketInfo(symbolws, MODE_ASK) + debugState.PriceOffset * MarketInfo(symbolws, MODE_POINT);
	  
		if (t.Bid == bids[i])
		{
			t.Bid = t.Bid + MarketInfo(symbolws, MODE_POINT);
			t.Ask = t.Ask + MarketInfo(symbolws, MODE_POINT);
		}
			  	
		StringToCharArray(symbol, t.Symbol);
		bids[i] = t.Bid;
		asks[i] = t.Ask;
		UpdatePrice(t);
	} 
	
}

void UpdateTicks()
{
	int arLen = ArraySize(registeredInstruments);
	for (int i = 0; i < arLen; i++)
	{
		string symbol = registeredInstruments[i];
		string symbolws = SymbolWS(symbol);
   
		Tick t;
		t.Bid = MarketInfo(symbolws, MODE_BID);
		t.Ask = MarketInfo(symbolws, MODE_ASK);
   
   		if (debugState.IsDebugging)
   		{
   			t.Bid += debugState.PriceOffset * MarketInfo(symbolws, MODE_POINT);
	  		t.Ask += debugState.PriceOffset * MarketInfo(symbolws, MODE_POINT);
   		}
   
		if (t.Bid != bids[i] ||
			t.Ask != asks[i])
		{
			StringToCharArray(symbol, t.Symbol);
			bids[i] = t.Bid;
			asks[i] = t.Ask;
			UpdatePrice(t);
		}
	}
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