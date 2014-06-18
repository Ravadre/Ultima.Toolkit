#property copyright "Marcin Deptu³a"
#property link      ""
#property version   "1.00"
#property strict

struct MT4Order
{
	int Order;
	int Type;
	double Volume;
	
	double StopLoss;
	double TakeProfit;
	
	double OpenPrice;
	datetime OpenTime;
	
	double ClosePrice;
	datetime CloseTime;
	
	double Profit;
	double Commission;
	double Swap;
	
	char Comment[128];
	int MagicNumber;
};


input string PlatformTag = "";
input string Server = "";
input string Database = "";
input string User = "";
input string Password = "";

string _PlatformTag;

#import "MT4Sync.dll"
bool SyncInitialize(string path);
bool SyncConnect(string server, string database, string user, string pwd);
bool SyncConnectIntegrated(string server, string database);
bool SyncIsDatabaseSetupCorrectly();
void SyncClose();
void SyncOpenedOrders(string tag, MT4Order& orders[], int size);
#import

string terminalDataPath;


int OnInit()
{
	if (StringLen(PlatformTag) == 0) 
		_PlatformTag = AccountCompany();
	else
		_PlatformTag = PlatformTag;
		
	Print("Platform tag: " + _PlatformTag);
	
	terminalDataPath = TerminalInfoString(TERMINAL_DATA_PATH);
	SyncInitialize(terminalDataPath);
	
	bool connected = false;
	
	if (User == "" || Password == "")
		connected = SyncConnectIntegrated(Server, Database);
	else
		connected = SyncConnect(Server, Database, User, Password);
	
	if (!connected) 
	{
		Alert("Could not connect");
		Print("Could not connect");
		return(INIT_FAILED);
	}
	
	if (!SyncIsDatabaseSetupCorrectly())
	{
		Alert("Database has invalid schema");
		Print("Database has invalid schema");
		return(INIT_FAILED);
	}
		
	EventSetMillisecondTimer(10000);
	 
	return(INIT_SUCCEEDED);
}

void OnDeinit(const int reason)
{
	EventKillTimer();     
	SyncClose();
}

void OnTick()
{

   
}

void OnTimer()
{
	int total = OrdersTotal();
	MT4Order orders[];
	ArrayResize(orders, total);
	
	for (int i = 0; i < total; ++i) 
	{
		if (!OrderSelect(i, SELECT_BY_POS, MODE_TRADES))
			continue;
		
		orders[i].Order = OrderTicket();
		orders[i].ClosePrice = OrderClosePrice();
		orders[i].CloseTime = OrderCloseTime();
		StringToCharArray(OrderComment(), orders[i].Comment);
		orders[i].Commission = OrderCommission();
		orders[i].MagicNumber = OrderMagicNumber();
		orders[i].OpenPrice = OrderOpenPrice();
		orders[i].OpenTime = OrderOpenTime();
		orders[i].Profit = OrderProfit();
		orders[i].StopLoss = OrderStopLoss();
		orders[i].Swap = OrderSwap();
		orders[i].TakeProfit = OrderTakeProfit();
		orders[i].Type = OrderType();
		orders[i].Volume = OrderLots();
	}
	
	SyncOpenedOrders(_PlatformTag, orders, total);
	Print("Orders synced...");
}
