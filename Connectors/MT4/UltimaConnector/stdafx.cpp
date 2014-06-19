#include "stdafx.h"


std::string ToString(const wchar_t* str)
{
	if (str == nullptr)
		return std::string("null");

	std::string s;
	s.resize(wcslen(str));
	WideCharToMultiByte(CP_ACP, 0, str, -1, (char*) s.data(), s.capacity(), nullptr, nullptr);

	return s;
}