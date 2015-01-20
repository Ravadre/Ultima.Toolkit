#property copyright "Marcin Deptu³a"
#property link      "https://github.com/Ravadre"
#property strict

#include <Common.mqh>

string SymbolWS(string symbol)
{
	return (symbol + SymbolSuffix);
}

string SymbolWoS(string symbol)
{
	return (StringSubstr(symbol, 0, StringLen(symbol) - StringLen(SymbolSuffix)));
}

void CloseOrder(int command, int order, int retries, int retrySpanMs)
{
	bool selected = OrderSelect(order, SELECT_BY_TICKET);

	if (selected == false)
	{
		ReportCommand(command, Res_Error, order);
		return;
	}
	
	if (IsMarket(OrderType()))
	{
		CloseSelectedMarketPosition(command, order, retries, retrySpanMs);
	}
	else if (IsPending(OrderType()))
	{
		CloseSelectedPendingPosition(command, order, retries, retrySpanMs);
	}
	else
	{
		ReportCommand(command, Res_Error, order);
		return;
	}
}

void CloseOrderBy(int command, int order, int orderBy, int retries, int retrySpanMs)
{
	int t = 0;
	bool closed = false;
   
	for (t = 0; t < retries; t++)
	{
		WaitForTradeContext();
		closed = OrderCloseBy(order, orderBy);
	  
		if (closed)
			break;
		 
		Sleep(retrySpanMs);    
	}
   
	if (!closed)
	{
		ReportCommand(command, Res_Error, order);
	}
	else
	{
		ReportCommand(command, Res_OK, order);
	}
}

void CloseSelectedPendingPosition(int command, int order, int retries, int retrySpanMs)
{	
	int t = 0;
	bool deleted = false;

	for (t = 0; t < retries; t++)
	{
		WaitForTradeContext();
		deleted = OrderDelete(order);
	  
		if(deleted)
			break;
		 
		Sleep(retrySpanMs);   
	}

	if (deleted)
	{	
		ReportCommand(command, Res_OK, order);
	}
	else
	{
		ReportCommand(command, Res_Error, order);
	}
}

void CloseSelectedMarketPosition(int command, int order, int retries, int retrySpanMs)
{
	int t;
	bool closed = false;
   
	for (t = 0; t < retries; t++)
	{
		WaitForTradeContext();
		closed = OrderClose(order, OrderLots(), OrderClosePrice(), 0);
	  
		if (closed)
			break;
		 
		Sleep(retrySpanMs);   
	}
	
	if (!closed)
	{
		ReportCommand(command, Res_Error, order);
		return;    	
	}
	
	ReportCommand(command, Res_OK, order);
}

void OpenPosition(int command, string symbol, int tradeCmd, double volume, 
					double price, int slippage, double stopLoss, 
					double takeProfit, string comment, int magicNumber, 
					int retries, int retrySpanMs)
{
	Print("[Trade] Opening new position, cmd: ", tradeCmd, ", symbol: ", symbol, " volume: ", volume,
			", stopLoss: ", stopLoss);
	symbol = SymbolWS(symbol);

	if (IsMarket(tradeCmd))
	{
		OpenMarketPosition(command, symbol, tradeCmd, volume, price, slippage,
						   stopLoss, takeProfit, comment, magicNumber, retries, retrySpanMs);
	}
	else if (IsPending(tradeCmd))
	{
		OpenPendingPosition(command, symbol, tradeCmd, volume, price, slippage,
							stopLoss, takeProfit, comment, magicNumber, retries, retrySpanMs);
	}
	else
	{
		ReportCommand(command, Res_Error, 0);
	}
}

double GetPriceIfZero(string symbol, int tradeCmd, double price)
{
	if (MathAbs(price) < 0.0001)
	{
		if (tradeCmd == OP_BUY)
		{
			price = MarketInfo(symbol, MODE_ASK);
		}
		else 
		{
			price = MarketInfo(symbol, MODE_BID);
		}
	}
	
	return (price);
}

void OpenMarketPosition(int command, string symbol, int tradeCmd, double volume, 
						double price, int slippage, double stopLoss, 
						double takeProfit, string comment, int magicNumber, 
						int retries, int retrySpanMs)
{
	int order = -1;
	bool selected = false;
	bool modified = false;
	int t = 0;
	double origPrice = 0.0;
	double realStopLoss = 0.0;
	
   	origPrice = price;
  
	for (t = 0; t < retries; t++) 
	{
		price = GetPriceIfZero(symbol, tradeCmd, origPrice);
	
		WaitForTradeContext();
		order = OrderSend(symbol, tradeCmd, volume, price, slippage, stopLoss, takeProfit, comment, magicNumber);
	 	 
		if (order >= 0)
		{
			double realOpenPrice = 0.0;
			if (OrderSelect(order, SELECT_BY_TICKET))
			{
				realOpenPrice = OrderOpenPrice();
				realStopLoss = OrderStopLoss();
			}
			
			Print("[Trade] Position opened successfully. Order: ", order, ", open price: ", realOpenPrice);
			break;
		}		
		
		Sleep(retrySpanMs);    
	}
	
	if (order < 0)
	{
		Print("[Trade] Could not open new position. Error: ", ErrorDescription(GetLastError()));
		ReportCommand(command, Res_Error, order);
		return;
	}
	
	if (stopLoss != 0.0 && stopLoss != realStopLoss)
	{
		Print("[Trade] Stop loss for order ", order, " is not set");
		ReportCommand(command, Res_OKPartial, order);
		return;
	}
	
	ReportCommand(command, Res_OK, order);
}

void OpenPendingPosition(int command, string symbol, int tradeCmd, double volume, 
						 double openPrice, int slippage, double stopLoss, double takeProfit, 
						 string comment, int magicNumber, int retries, int retrySpanMs)
{
	int t = 0;
	int order = 0;
   
	for (t = 0; t < retries; t++)
	{
		WaitForTradeContext();
		order = OrderSend(symbol, tradeCmd, volume, openPrice, slippage, 
							  stopLoss, takeProfit, comment, magicNumber, 0);
							  
		if (order >= 0)
		{
			Print("[Trade] Limit position opened successfully. Order: ", order);
			break;
		}
	   
		Sleep(retrySpanMs); 
	}
	
	if (order < 0)
	{
		Print("[Trade] Could not open new limit position. Error: ", ErrorDescription(GetLastError()));
		ReportCommand(command, Res_Error, order);			
		return;
	}

	ReportCommand(command, Res_OK, order);	
}

void ModifyOrder(int command, int order, double openPrice,
				 double stopLoss, double takeProfit, 
				 int retries, int retrySpanMs)
{
	int t;
	bool modified;
	bool selected = OrderSelect(order, SELECT_BY_TICKET);
   
	if (!selected)
	{
		Print("[Trade] Could not modify position ", order, ", because it could not be selected.");
		ReportCommand(command, Res_Error, order);
		return;
	}
   
	for (t = 0; t < retries; t++)
	{
		WaitForTradeContext();
		modified = OrderModify(order, openPrice, stopLoss, takeProfit, 0);
	   
		if (modified)
		{
			Print("[Trade] Order ", order, " modified successfully.");
			break;
		}	
		 
		Sleep(retrySpanMs);   
	}
   
	if (modified)
	{
		ReportCommand(command, Res_OK, order);
	}
	else
	{
		Print("[Trade] Could not modify position ", order, ", error: ", 
				ErrorDescription(GetLastError()));
		ReportCommand(command, Res_Error, order);
	}
}
			
void UpdateOrdersImpl()
{
	MT4Order orders[];
	
	RefreshRates();
	
	int totalOrders = OrdersTotal();
	int i = 0;

	int r = ArrayResize(orders, totalOrders);
	if (r < 0)
		return;
	
	for (int o = 0; o < totalOrders; o++)
	{
		bool selected = OrderSelect(o, SELECT_BY_POS);		

		if (!selected)
			continue;

		double point = MarketInfo(OrderSymbol(), MODE_POINT);

		orders[i].Order = OrderTicket();
		orders[i].OpenPrice = OrderOpenPrice();
		orders[i].ClosePrice = OrderClosePrice();
		orders[i].Profit = OrderProfit();
		orders[i].StopLoss = OrderStopLoss();
		orders[i].TakeProfit = OrderTakeProfit();
		orders[i].Lots = OrderLots();
		orders[i].TradeCommand = OrderType();
		StringToCharArray(SymbolWoS(OrderSymbol()), orders[i].Symbol);
		orders[i].OpenTime = OrderOpenTime();
		orders[i].CloseTime = OrderCloseTime();
		orders[i].Swap = OrderSwap();
		orders[i].Commission = OrderCommission();
		
		if (OrderType() == OP_BUY)
		{
			orders[i].PointProfit = (int)((OrderClosePrice() - OrderOpenPrice()) / point);
		}
		else if (OrderType() == OP_SELL)
		{
			orders[i].PointProfit = (int)((OrderOpenPrice() - OrderClosePrice()) / point);
		}
		else
		{
			orders[i].PointProfit = 0;
		}

		i++;

		if (i >= totalOrders)
			break;
	}	

	UpdateOrders(i, orders);
}

void RequestOrderHistory(int command)
{
	MT4Order orders[];
	
	RefreshRates();
	
	int totalOrders = OrdersHistoryTotal();
	int i = 0;

	int r = ArrayResize(orders, totalOrders);
	if (r < 0)
		return;
	
	for (int o = 0; o < totalOrders; o++)
	{
		bool selected = OrderSelect(o, SELECT_BY_POS, MODE_HISTORY);		

		if (!selected)
			continue;

		double point = MarketInfo(OrderSymbol(), MODE_POINT);

		orders[i].Order = OrderTicket();
		orders[i].OpenPrice = OrderOpenPrice();
		orders[i].ClosePrice = OrderClosePrice();
		orders[i].Profit = OrderProfit();
		orders[i].Commission = OrderCommission();
		orders[i].Swap = OrderSwap();
		orders[i].StopLoss = OrderStopLoss();
		orders[i].TakeProfit = OrderTakeProfit();
		orders[i].Lots = OrderLots();
		orders[i].TradeCommand = OrderType();
		StringToCharArray(SymbolWoS(OrderSymbol()), orders[i].Symbol);
		orders[i].OpenTime = OrderOpenTime();
		orders[i].CloseTime = OrderCloseTime();
		
		if (OrderType() == OP_BUY)
		{
			orders[i].PointProfit = (int)((OrderClosePrice() - OrderOpenPrice()) / point);
		}
		else if (OrderType() == OP_SELL)
		{
			orders[i].PointProfit = (int)((OrderOpenPrice() - OrderClosePrice()) / point);
		}
		else
		{
			orders[i].PointProfit = 0;
		}

		i++;

		if (i >= totalOrders)
			break;
	}	

	UpdateHistory(command, i, orders);
}