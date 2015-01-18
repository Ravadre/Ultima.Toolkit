#property copyright "Marcin Deptu³a"
#property link      ""
#property strict

#include <Charts/Chart.mqh>

enum ScriptExecMode
{
	Instant = 0,
	Market = 1
};

struct DebugState
{
	bool IsDebugging;
	bool IsGeneratingTicks;
	bool GenerateInLock;
	int PriceOffset;
	int PriceSpread;
};

void SetupChart(CChart& chart)
{
	chart.Attach();
	chart.ColorBackground(Black);
	chart.ColorBarDown(Lime);
	chart.ColorBarUp(Lime);
	chart.ColorCandleBear(White);
	chart.ColorCandleBull(Black);
	chart.ColorChartLine(Lime);
	chart.ColorForeground(White);
	chart.ColorLineAsk(Red);
	chart.ColorLineBid(LightSlateGray);
	chart.ColorStopLevels(Red);
	chart.ColorGrid(LightSlateGray);
	chart.Detach();
}

void WaitForTradeContext()
{
	int try = 0;

	while (try < 500)
	{
		try++;
		if (IsTradeContextBusy())
		{
			Sleep(10);
		}
		else
		{
			break;
		}
	}
}


void HandleRegisterSymbol(string symbol, bool register)
{
	int arLen;
	int i;

	if (register)
	{
		if (ArrayContains(registeredInstruments, symbol))
		{
			Print("Symbol ", symbol, " already registered");
			return;
		}

		ArrayResize(registeredInstruments, ArraySize(registeredInstruments) + 1);
		registeredInstruments[ArraySize(registeredInstruments) - 1] = symbol;

		ArrayResize(bids, ArraySize(bids) + 1);
		ArrayResize(asks, ArraySize(asks) + 1);
		ArrayInitialize(bids, 0);
		ArrayInitialize(asks, 0);

		Print("Registered symbol " + symbol);
		LogStrArray(registeredInstruments);
	}
	else
	{
		arLen = ArraySize(registeredInstruments);
		for (i = 0; i < arLen; i++)
		{
			if (registeredInstruments[i] == symbol)
			{
				registeredInstruments[i] = registeredInstruments[arLen - 1];
				ArrayResize(registeredInstruments, arLen - 1);
				break;
			}
		}

		ArrayResize(bids, ArraySize(bids) - 1);
		ArrayResize(asks, ArraySize(asks) - 1);
		ArrayInitialize(bids, 0);
		ArrayInitialize(asks, 0);

		Print("Unregistered symbol " + symbol);
		LogStrArray(registeredInstruments);
	}
}
