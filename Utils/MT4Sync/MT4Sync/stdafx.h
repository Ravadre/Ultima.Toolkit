#pragma once

#include "targetver.h"

#define WIN32_LEAN_AND_MEAN   
// Windows Header Files:
#include <windows.h>

#include <sql.h>
#include <sqlext.h>

#define _ELPP_THREAD_SAFE
//#define _ELPP_UNICODE
#include "easylogging++.h"

#include <string>
#include <vector>
#include <memory>

#include "Data.hpp"

#include "DbStatement.hpp"

std::string ToString(const wchar_t* str);
std::wstring ToWString(const char* str);