#include "stdafx.h"

#include "DbConnection.hpp"

using namespace std;

DbConnection::DbConnection() : env(nullptr), dbc(nullptr)
{

}

DbConnection::~DbConnection()
{
	if (this->dbc != nullptr)
	{
		SQLFreeHandle(SQL_HANDLE_DBC, this->dbc);
	}

	if (this->env != nullptr) 
	{
		SQLFreeHandle(SQL_HANDLE_ENV, this->env);
	}
}

static void ThrowOnError(const string& name, function<int()> action)
{
	int rc = 0;
	
	if (!SQL_SUCCEEDED(rc = action()))
	{
		LOG(ERROR) << string("Error while invoking: ") + name << ", ec = " << rc;
		throw string("Error while invoking: ") + name;
	}
		
}

static void ThrowOnError(function<int()> action)
{
	ThrowOnError("", action);
}


int DbConnection::Connect(const wchar_t* server, const wchar_t* database, const wchar_t* user, const wchar_t* password)
{
	return Connect(wstring(server), wstring(database), 
				   user == nullptr ? wstring() : wstring(user),
				   password == nullptr ? wstring() : wstring(password));
}

int DbConnection::Connect(const std::wstring& server, const std::wstring& database, const std::wstring& user, const std::wstring& password)
{
	try
	{
		ThrowOnError("Alloc env", [&]()
		{
			return SQLAllocHandle(SQL_HANDLE_ENV, SQL_NULL_HANDLE, &this->env);
		});
		ThrowOnError("Setting odbc version", [&]()
		{
			return SQLSetEnvAttr(env, SQL_ATTR_ODBC_VERSION, (void*) SQL_OV_ODBC3, 0);
		});
		ThrowOnError("Allocating dbc", [&]()
		{
			return SQLAllocHandle(SQL_HANDLE_DBC, env, &dbc);
		});
		ThrowOnError("Setting connection timeout", [&]()
		{
			return SQLSetConnectAttr(dbc, SQL_ATTR_CONNECTION_TIMEOUT, (SQLPOINTER) 3, SQL_IS_UINTEGER);
		});
		ThrowOnError("Setting login timeout", [&]()
		{
			return SQLSetConnectAttr(dbc, SQL_ATTR_LOGIN_TIMEOUT, (SQLPOINTER) 3, SQL_IS_UINTEGER);
		});
		ThrowOnError("Connecting to database", [&]()
		{
			auto ss = wstringstream();
			if (user.length() > 0 &&
				password.length() > 0)
			{
				ss << L"Driver={SQL Server Native Client 11.0}; Server=" << server
					<< L"; Database=" << database << L"; UID=" << user << L"; PWD=" << password << L";";
			}
			else
			{
				ss << L"Driver={SQL Server Native Client 11.0}; Server=" << server
					<< L"; Database=" << database << L";Trusted_Connection=yes;";
			}
			auto str = ss.str();
			auto connString = (wchar_t*) (str.c_str());

			return SQLDriverConnect(dbc, NULL, connString, SQL_NTS, nullptr, 0, nullptr, SQL_DRIVER_COMPLETE);
		});

		return true;
	}
	catch (...)
	{
		return false;
	}
}

int DbConnection::IsDatabaseSetupCorrectly()
{
	DbStatement stmt(dbc);

	int rc = SQLTables(*stmt, nullptr, 0, L"dbo", SQL_NTS, nullptr, 0, L"TABLE", SQL_NTS);

	bool hasOrders = false;
	bool hasHistory = false;

	wchar_t buf[1024];

	while (true)
	{
		rc = SQLFetch(*stmt);

		if (rc == SQL_NO_DATA)
			break;

		SQLGetData(*stmt, 3, SQL_C_WCHAR, buf, sizeof(buf), nullptr);

		if (wstring(buf) == L"Orders") hasOrders = true;
		else if (wstring(buf) == L"History") hasHistory = true;
	}

	return hasOrders && hasHistory;
}

static void UpdateOrdersToDb(SQLHDBC dbc, const wchar_t* tag, const vector<MT4Order>& orders)
{
	DbStatement stmt(dbc);

	wstringstream query;
	query << L"BEGIN TRANSACTION;\n";
	query << L"DELETE FROM [Orders] WHERE [PlatformTag] = '"
		  << tag << L"';\n";
	if (orders.size() > 0)
	{
		query << L"INSERT INTO [Orders] ([PlatformTag], [Order], [Type], [Volume], "
			<< L"[StopLoss], [TakeProfit], [OpenPrice], [OpenTime], [CurrentPrice], "
			<< L"[Profit], [Commission], [Swap], [Comment], [MagicNumber])\n"
			<< "VALUES";
		size_t i = 0;
		for (const auto& o : orders)
		{
			query << "('" << tag << "', " << o.Order << ", " << o.Type << ", " << o.Volume
				<< ", " << o.StopLoss << ", " << o.TakeProfit << ", " << o.OpenPrice
				<< ", " << "DATEADD(second, " << o.OpenTime << ", {d '1970-01-01'})"
				<< ", " << o.ClosePrice << ", " << o.Profit << ", " << o.Commission
				<< ", " << o.Swap << ", '" << o.Comment << "', " << o.MagicNumber
				<< ")";
			if (++i >= orders.size())
				query << ";\n";
			else
				query << ",\n";
		}
	}
	query << L"COMMIT TRANSACTION;\n";

	auto qss = query.str();
	auto qs = qss.c_str();

	int rc;
	if (!SQL_SUCCEEDED(rc = SQLExecDirect(*stmt, (wchar_t*) qs, SQL_NTS)))
	{
		if (rc == SQL_NO_DATA)
			return;

		LOG(ERROR) << "Could not update orders to database. Ec: " << rc;
		throw exception("Could not update orders to database");
	}
}

void DbConnection::SyncOpenedOrders(const wchar_t* tag, const vector<MT4Order>& orders)
{
	UpdateOrdersToDb(dbc, tag, orders);
}