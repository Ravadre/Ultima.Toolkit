#pragma once

#include "targetver.h"

#pragma warning(push)
#pragma warning(disable: 4996)
#include <boost/asio.hpp>
#pragma warning(pop)

#define WIN32_LEAN_AND_MEAN            
#include <windows.h>


#define _ELPP_THREAD_SAFE
#include "easylogging++.h"

#include <cstring>
#include <string>
#include <chrono>
#include <vector>

#include "Data.hpp"
#include "MessageId.hpp"

std::string ToString(const wchar_t* str);
