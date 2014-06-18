#include "stdafx.h"

_INITIALIZE_EASYLOGGINGPP

BOOL APIENTRY DllMain( HMODULE hModule,
					   DWORD  ul_reason_for_call,
					   LPVOID lpReserved
					 )
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		_START_EASYLOGGINGPP(0, (const char**)nullptr);
		el::Loggers::reconfigureAllLoggers(el::ConfigurationType::Format, "%datetime{%H:%m:%s.%g} [%thread] %level  %msg");
		break;
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

