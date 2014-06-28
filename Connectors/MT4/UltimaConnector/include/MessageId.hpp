#pragma once

enum class MessageId
{
	Tick = 1,
	Command = 2,
	OpenedOrders = 3,
	OrdersHistory = 4,
	SymbolRegister = 5,
	CloseOrder = 6,
	OpenOrder = 7,
	ModifyOrder = 8,
	CloseOrderBy = 9,
	HistoryOrdersRequest = 10,
};