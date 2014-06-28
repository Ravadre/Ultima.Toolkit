#pragma once

enum class MessageId
{
	Login = 1,
	Tick = 2,
	Command = 3,
	OpenedOrders = 4,
	OrdersHistory = 5,
	SymbolRegister = 6,
	CloseOrder = 7,
	OpenOrder = 8,
	ModifyOrder = 9,
	CloseOrderBy = 10,
	HistoryOrdersRequest = 11,
};