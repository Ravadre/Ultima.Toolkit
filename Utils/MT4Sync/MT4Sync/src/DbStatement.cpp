#include "stdafx.h"
#include "DbStatement.hpp"

DbStatement::DbStatement(SQLHDBC dbc)
{
	SQLAllocHandle(SQL_HANDLE_STMT, dbc, &this->stmt);
}

DbStatement::~DbStatement()
{
	if (this->stmt != nullptr)
		SQLFreeHandle(SQL_HANDLE_STMT, this->stmt);
}

SQLHSTMT DbStatement::operator*() 
{
	return this->stmt;
}

