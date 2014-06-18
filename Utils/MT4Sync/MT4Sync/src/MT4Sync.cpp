#include "stdafx.h"
#include "MT4Sync.h"

#include "DbConnection.hpp"

using namespace std;

shared_ptr<DbConnection> dbConnection;

extern "C" 
{
	API void SyncClose()
	{
		LOG(INFO) << "Called close";

		dbConnection.reset();
	}

	API void SyncInitialize(wchar_t* _path)
	{
		char buffer[1024] = { 0 };
		WideCharToMultiByte(CP_ACP, 0, _path, -1, buffer, sizeof(buffer), nullptr, nullptr);
		string path(buffer);
		if (path.back() != '\\')
		{
			path += '\\';
		}

		path.append("MQL4\\Logs\\MT4Sync.log");


		el::Loggers::reconfigureAllLoggers(el::ConfigurationType::Filename, path.c_str());

		LOG(INFO) << "Initialize called";
	}

	API void SyncConnectIntegrated(wchar_t* server, wchar_t* database)
	{
		if (dbConnection != nullptr)
			SyncClose();

		dbConnection = make_shared<DbConnection>();
		dbConnection->Connect(server, database, nullptr, nullptr);
	}

	API void SyncConnect(wchar_t* server, wchar_t* database, wchar_t* user, wchar_t* password)
	{
		LOG(INFO) << "Called Connect. Server: " << ToString(server) 
			<< ", Database: " << ToString(database) 
			<< ", user: " << ToString(user) 
			<< ", password: *******";

		if (dbConnection != nullptr)
			SyncClose();

		dbConnection = make_shared<DbConnection>();
		dbConnection->Connect(server, database, user, password);
	}

	API int SyncIsDatabaseSetupCorrectly()
	{
		return dbConnection->IsDatabaseSetupCorrectly();
	}

	API void SyncOpenedOrders(wchar_t* tag, MT4Order* orders, int ordersSize)
	{
		LOG(INFO) << "Opened orders # " << ordersSize;
		
		vector<MT4Order> v;
		
		if (ordersSize > 0)
			v = vector<MT4Order>(orders, orders + ordersSize);

		dbConnection->SyncOpenedOrders(tag, v);
	}

}