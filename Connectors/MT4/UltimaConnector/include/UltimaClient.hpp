#pragma once

#include "easylogging++.h"
#include <boost/asio.hpp>
#include <thread>
#include <deque>
#include <queue>
#include <map>

class UltimaClient
{
private:
	boost::asio::io_service io;
	boost::asio::ip::tcp::socket socket;
	std::thread thread;
	std::string address;
	std::shared_ptr<boost::asio::io_service::work> work;
	boost::asio::deadline_timer timer;

	std::vector<char> buffer;
	std::vector<char> recvBuffer;
	std::queue<std::vector<char>> sendBuffers;

	std::map<int, std::function< void(const char*, int)>> handlers;


	void postReconnect();
	void handlePackets();
	void handleRead(const boost::system::error_code& ec, size_t received);
public:
	UltimaClient();
	~UltimaClient();

	void connect(const std::string& address);
	
};