#pragma once

#include "easylogging++.h"
#include <boost/asio.hpp>
#include <thread>
#include <deque>
#include <queue>
#include <map>
#include <atomic>
#include <condition_variable>
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
	std::condition_variable cmdCond;
	std::deque<UltimaConnector::SymbolRegistrationDTO> symbolRegQ;
	std::deque<UltimaConnector::CloseOrderCommandDTO> closeOrderQ;
	std::deque<UltimaConnector::CloseOrderByCommandDTO> closeOrderByQ;
	std::deque<UltimaConnector::OpenOrderCommandDTO> openOrderQ;
	std::deque<UltimaConnector::ModifyOrderCommandDTO> modifyOrderQ;
	std::deque<UltimaConnector::RequestHistoryDTO> requestHistoryQ;

	std::vector<char> lastSentOpenedOrdersPacket;

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
	inline int getCommandsCount() { return commandsInQueues; }
	inline bool waitForCommand(int timeoutMs)
	{
		std::unique_lock<std::mutex> l(cmdMutex);
		if (commandsInQueues > 0)
			return true;
		return cmdCond.wait_for(l, std::chrono::milliseconds(timeoutMs)) == std::cv_status::no_timeout;
	}

	template<MessageId id, typename T>
	void send(const T& packet)
	{
		io.dispatch([this, packet]() 
		{
			std::vector<char> buf;
			auto bSize = packet.ByteSize() + 8;

			buf.resize(bSize);

			auto data = buf.data();

			packet.SerializeToArray(data + 8, bSize - 8);
			*(int*)(data) = bSize;
			*(int*)(data + 4) = (int)id;

			if (id == MessageId::OpenedOrders)
			{
				if (lastSentOpenedOrdersPacket.size() == buf.size())
				{
					if (memcmp(lastSentOpenedOrdersPacket.data(), buf.data(), buf.size()) == 0)
					{
						return;
					}
				}

				lastSentOpenedOrdersPacket = buf;
			}

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
		cmdCond.notify_one();
	}
	
	template<typename T>
	bool getFromQueue(T& packet, std::deque<T>& queue)
	{
		lock_guard<mutex> l(cmdMutex);

		if (queue.size() == 0)
			return false;

		packet = queue.front();
		queue.pop_front();
		--commandsInQueues;

		return true;
	}

	inline bool getFromQueue(UltimaConnector::SymbolRegistrationDTO& packet)
	{
		return getFromQueue(packet, symbolRegQ);
	}

	inline bool getFromQueue(UltimaConnector::CloseOrderCommandDTO& packet)
	{
		return getFromQueue(packet, closeOrderQ);
	}

	inline bool getFromQueue(UltimaConnector::CloseOrderByCommandDTO& packet)
	{
		return getFromQueue(packet, closeOrderByQ);
	}

	inline bool getFromQueue(UltimaConnector::OpenOrderCommandDTO& packet)
	{
		return getFromQueue(packet, openOrderQ);
	}

	inline bool getFromQueue(UltimaConnector::ModifyOrderCommandDTO& packet)
	{
		return getFromQueue(packet, modifyOrderQ);
	}

	inline bool getFromQueue(UltimaConnector::RequestHistoryDTO& packet)
	{
		return getFromQueue(packet, requestHistoryQ);
	}
};