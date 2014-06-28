#include "stdafx.h"
#include "UltimaConnector.h"
#include "UltimaClient.hpp"

using namespace std;
using namespace boost::asio;
using namespace UltimaConnector;

shared_ptr<UltimaClient> client;
string companyName;

extern "C"
{
	bool API Initialize(const wchar_t* dataPath, const wchar_t* company, const wchar_t* server)
	{
		auto _dataPath = ToString(dataPath);

		companyName = ToString(company);

		if (_dataPath.back() != '\\')
			_dataPath += '\\';

		_dataPath.append("MQL4\\Logs\\UltimaConnector.log");

		el::Loggers::reconfigureAllLoggers(el::ConfigurationType::Filename, _dataPath.c_str());

		LOG(INFO) << "Initialize called";
		string address = ToString(server);
		client = make_shared<UltimaClient>();
		client->connect(address);
		
		return true;
	}

	void API DeInitialize()
	{
		LOG(INFO) << "DeInitialize called";
		client.reset();
		LOG(INFO) << "DeInitialize done";
	}

	bool API HasCommands()
	{
		return false;
	}

	bool API WaitForCommand(int timeoutMs)
	{
		this_thread::sleep_for(chrono::milliseconds(timeoutMs));
		return false;
	}

	bool API GetSymbolRegCommand(SymbolRegistrationCommand* cmd)
	{
		return false;
	}

	bool API GetCloseOrderCommand(CloseOrderCommand* cmd)
	{
		return false;
	}

	bool API GetCloseOrderByCommand(CloseOrderByCommand* cmd)
	{
		return false;
	}

	bool API GetOpenOrderCommand(OpenOrderCommand* cmd)
	{
		return false;
	}

	bool API GetModifyOrderCommand(ModifyOrderCommand* cmd)
	{
		return false;
	}

	bool API GetRequestOrderHistory(OrderHistoryCommand* cmd)
	{
		return false;
	}

	void API ReportCommand(int command, CommandResult result, int order)
	{
		CommandResultDTO packet;
		packet.set_command(command);
		packet.set_order(order);
		packet.set_result((int)result);

		client->send<MessageId::Command>(packet);
	}

	void API UpdatePrice(const Tick* tick)
	{
		PriceDTO packet;
		packet.set_symbol(tick->Symbol);
		packet.set_bid(tick->Bid);
		packet.set_ask(tick->Ask);

		client->send<MessageId::Tick>(packet);
	}

	void API UpdateOrders(int orderCount, MT4Order* orders)
	{
		UpdateOrdersDTO d;

		for (int i = 0; i < orderCount; ++i)
		{
			auto o = d.add_orders();
			auto& so = orders[i];

			o->set_symbol(so.Symbol);
			o->set_order(so.Order);
			o->set_tradecommand(so.TradeCommand);
			o->set_volume(so.Lots);
			o->set_openprice(so.OpenPrice);
			o->set_stoploss(so.StopLoss);
			o->set_takeprofit(so.TakeProfit);
			o->set_closeprice(so.ClosePrice);
			o->set_profit(so.Profit);
			o->set_pointprofit(so.PointProfit);
			o->set_opentime((int)(so.OpenTime));
		}

		client->send<MessageId::OpenedOrders>(d);
	}

	void API UpdateHistory(int command, int orderCount, MT4Order* orders)
	{
		OrdersHistoryDTO d;
		d.set_command(command);

		for (int i = 0; i < orderCount; ++i)
		{
			auto o = d.add_orders();
			auto& so = orders[i];

			o->set_symbol(so.Symbol);
			o->set_order(so.Order);
			o->set_tradecommand(so.TradeCommand);
			o->set_volume(so.Lots);
			o->set_openprice(so.OpenPrice);
			o->set_stoploss(so.StopLoss);
			o->set_takeprofit(so.TakeProfit);
			o->set_closeprice(so.ClosePrice);
			o->set_profit(so.Profit);
			o->set_pointprofit(so.PointProfit);
			o->set_opentime((int) (so.OpenTime));
			o->set_closetime((int) (so.CloseTime));
		}

		client->send<MessageId::OrdersHistory>(d);
	}
}