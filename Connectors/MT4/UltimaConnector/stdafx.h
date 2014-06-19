#pragma once

#include "targetver.h"

#define WIN32_LEAN_AND_MEAN            
#include <windows.h>


#define _ELPP_THREAD_SAFE
#include "easylogging++.h"

#include <cstring>
#include <string>
#include <chrono>

#include "Data.hpp"

std::string ToString(const wchar_t* str);
