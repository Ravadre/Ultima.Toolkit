#pragma once

#include "easylogging++.h"
#include <boost/asio.hpp>
#include <thread>

class UltimaClient
{
private:
	boost::asio::io_service io;
	boost::asio::ip::tcp::socket socket;
	std::thread thread;
	std::string address;
	std::shared_ptr<boost::asio::io_service::work> work;
	boost::asio::deadline_timer timer;
public:
	UltimaClient();
	~UltimaClient();

	void connect(const std::string& address);
};