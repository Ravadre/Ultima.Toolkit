#include "stdafx.h"


std::string ToString(const wchar_t* str)
{
	if (str == nullptr)
		return std::string("null");

	std::string s;
	s.resize(wcslen(str));
	WideCharToMultiByte(CP_ACP, 0, str, -1, (char*)s.data(), s.capacity(), nullptr, nullptr);

	return s;
}

std::wstring ToWString(const char* str)
{
	if (str == nullptr)
		return std::wstring(L"null");

	std::wstring s;
	s.resize(strlen(str));
	MultiByteToWideChar(CP_ACP, 0, str, -1, (wchar_t*) s.data(), s.capacity());

	return s;
}