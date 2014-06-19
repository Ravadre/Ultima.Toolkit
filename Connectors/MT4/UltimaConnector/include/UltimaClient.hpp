#pragma once

#include "easylogging++.h"
#include "asio.hpp"
#include <thread>

class UltimaClient
{
private:
	asio::io_service io;
	asio::ip::tcp::socket socket;
	std::thread thread;
	std::string address;
	std::shared_ptr<asio::io_service::work> work;
	asio::deadline_timer timer;
public:
	UltimaClient();
	~UltimaClient();

	void connect(const std::string& address);
};