#pragma once

#include "stdafx.h"

class DbConnection
{
	SQLHENV env;
	SQLHDBC dbc;

public:
	DbConnection();
	~DbConnection();

	int Connect(const wchar_t* server, const wchar_t* database, const wchar_t* user, const wchar_t* password);
	int Connect(const std::wstring& server, const std::wstring& database, const std::wstring& user, const std::wstring& password);

	int IsDatabaseSetupCorrectly();

	void SyncOpenedOrders(const wchar_t* tag, const std::vector<MT4Order>& v);
};