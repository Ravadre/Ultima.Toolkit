#include "stdafx.h"
#include "UltimaConnector.h"
#include "UltimaClient.hpp"

using namespace std;
using namespace asio;

shared_ptr<UltimaClient> client;

extern "C"
{
	bool API Initialize(const wchar_t* dataPath, const wchar_t* company, const wchar_t* server)
	{
		auto _dataPath = ToString(dataPath);
		auto _company = ToString(company);

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

	}

	void API UpdatePrice(const Tick* tick)
	{

	}

	void API UpdateOrders(int orderCount, MT4Order* orders)
	{

	}

	void API UpdateHistory(int command, int orderCount, MT4Order* orders)
	{

	}
}