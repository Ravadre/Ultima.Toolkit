#pragma once

#include <sql.h>

struct DbStatement
{
	SQLHSTMT stmt;

	DbStatement(SQLHDBC dbc);
	~DbStatement();

	SQLHSTMT operator*();
};