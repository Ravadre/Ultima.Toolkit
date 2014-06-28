#pragma once

#include "easylogging++.h"
#include <boost/asio.hpp>
#include <thread>
#include <deque>
#include <queue>
#include <map>
#include <atomic>
#include "Protocol.pb.h"
#include "MessageId.hpp"

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
	std::deque<std::vector<char>> sendBuffers;

	std::map<int, std::function< void(const char*, size_t)>> handlers;
	std::atomic<uint32_t> commandsInQueues;

	std::mutex cmdMutex;
	std::deque<UltimaConnector::SymbolRegistrationDTO> symbolRegQ;
	std::deque<UltimaConnector::CloseOrderCommandDTO> closeOrderQ;
	std::deque<UltimaConnector::CloseOrderByCommandDTO> closeOrderByQ;
	std::deque<UltimaConnector::OpenOrderCommandDTO> openOrderQ;
	std::deque<UltimaConnector::ModifyOrderCommandDTO> modifyOrderQ;
	std::deque<UltimaConnector::RequestHistoryDTO> requestHistoryQ;


	bool connecting;

	void doRead();
	void doWrite();
	void postReconnect();
	void handlePackets();
	void handleRead(const boost::system::error_code& ec, size_t received);
	void handleWrite(const boost::system::error_code& ec, size_t sent);
	void registerHandlers();

	void reconnect();
public:
	UltimaClient();
	~UltimaClient();

	void connect(const std::string& address);
	
	template<MessageId id, typename T>
	void send(const T& packet)
	{
		io.dispatch([this, &packet]() 
		{
			std::vector<char> buf;
			auto bSize = packet.ByteSize() + 8;
			buf.reserve(bSize);

			auto data = buf.data();

			packet.SerializeToArray(data + 8, bSize - 8);
			*(int*)(data) = bSize;
			*(int*)(data + 4) = (int)id;

			bool isSending = sendBuffers.size() > 0;
			sendBuffers.push_back(buf);
			
			if (!isSending)
			{
				doWrite();
			}
		});
	}

	template<typename T>
	void pushToQueue(const void* data, int size, std::deque<T>& queue)
	{
		lock_guard<mutex> l(cmdMutex);

		T packet;
		packet.ParseFromArray(data, size);

		queue.push_back(packet);
		++commandsInQueues;
	}
	
};