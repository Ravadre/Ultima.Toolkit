#property copyright "Marcin Deptu³a"
#property link      ""
#property strict

enum ScriptExecMode
{
	Instant = 0,
	Market = 1
};

void CreateVersionLabel(string version)
{
	ObjectCreate("version_label", OBJ_LABEL, 0, 0, 0);
	ObjectSet("version_label", OBJPROP_XDISTANCE, 5);
	ObjectSet("version_label", OBJPROP_YDISTANCE, 13);
	ObjectSetText("version_label", StringConcatenate("Ultima connector ver. ", version), 8, "Arial", White);
}

void DeleteVersionLabel()
{
	ObjectDelete("version_label");
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
