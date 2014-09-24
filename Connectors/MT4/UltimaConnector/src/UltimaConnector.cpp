#include "stdafx.h"
#include "UltimaConnector.h"
#include "UltimaClient.hpp"

using namespace std;
using namespace boost::asio;
using namespace Ultima::MT4::Packets;

shared_ptr<UltimaClient> client;

extern "C"
{
	bool API Initialize(const wchar_t* dataPath, const wchar_t* company, const wchar_t* server)
	{
		auto _dataPath = ToString(dataPath);

		if (_dataPath.back() != '\\')
			_dataPath += '\\';

		_dataPath.append("MQL4\\Logs\\UltimaConnector.log");

		el::Loggers::reconfigureAllLoggers(el::ConfigurationType::Filename, _dataPath.c_str());

		LOG(INFO) << "Initialize called";
		client = make_shared<UltimaClient>();
		client->connect(ToString(company), ToString(server));
		
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
		return client->getCommandsCount() > 0;
	}

	bool API WaitForCommand(int timeoutMs)
	{
		return client->waitForCommand(timeoutMs);
	}

	bool API GetSymbolRegCommand(SymbolRegistrationCommand* cmd)
	{
		SymbolRegistrationDTO packet;

		if (!client->getFromQueue(packet))
			return false;

		cmd->Register = packet.register_();
		strcpy_s(cmd->Symbol, packet.symbol().c_str());
		
		return true;
	}

	bool API GetCloseOrderCommand(CloseOrderCommand* cmd)
	{
		CloseOrderCommandDTO packet;

		if (!client->getFromQueue(packet))
			return false;

		cmd->Command = packet.command();
		cmd->Order = packet.order();
		cmd->Retries = packet.retries();
		cmd->RetrySpanMs = packet.retryspanms();

		return true;
	}

	bool API GetCloseOrderByCommand(CloseOrderByCommand* cmd)
	{
		CloseOrderByCommandDTO packet;

		if (!client->getFromQueue(packet))
			return false;

		cmd->Command = packet.command();
		cmd->Order = packet.order();
		cmd->OrderBy = packet.orderby();
		cmd->Retries = packet.retries();
		cmd->RetrySpanMs = packet.retryspanms();

		return true;
	}

	bool API GetOpenOrderCommand(OpenOrderCommand* cmd)
	{
		OpenOrderCommandDTO packet;

		if (!client->getFromQueue(packet))
			return false;

		cmd->Command = packet.command();
		strcpy_s(cmd->Comment, packet.comment().c_str());
		cmd->LastChanceRetrySpanMs = packet.lastchanceretryspanms();
		cmd->MagicNumber = packet.magicnumber();
		cmd->OpenPrice = packet.openprice();
		cmd->Retries = packet.retries();
		cmd->RetrySpanMs = packet.retryspanms();
		cmd->Slippage = packet.slippage();
		cmd->StopLoss = packet.stoploss();
		strcpy_s(cmd->Symbol, packet.symbol().c_str());
		cmd->TakeProfit = packet.takeprofit();
		cmd->TradeCommand = packet.tradecommand();
		cmd->Volume = packet.volume();

		return true;
	}

	bool API GetModifyOrderCommand(ModifyOrderCommand* cmd)
	{
		ModifyOrderCommandDTO packet;

		if (!client->getFromQueue(packet))
			return false;

		cmd->Command = packet.command();
		cmd->OpenPrice = packet.openprice();
		cmd->Order = packet.order();
		cmd->Retries = packet.retries();
		cmd->RetrySpanMs = packet.retryspanms();
		cmd->StopLoss = packet.stoploss();
		cmd->TakeProfit = packet.takeprofit();

		return true;
	}

	bool API GetRequestOrderHistory(OrderHistoryCommand* cmd)
	{
		RequestHistoryDTO packet;

		if (!client->getFromQueue(packet))
			return false;

		cmd->Command = packet.command();

		return true;
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
			o->set_commission(so.Commission);
			o->set_swap(so.Swap);
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