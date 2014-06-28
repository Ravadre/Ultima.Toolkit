#include "stdafx.h"
#include "UltimaClient.hpp"

using namespace std;
using namespace boost::asio;
using namespace boost::asio::ip;
using namespace UltimaConnector;

UltimaClient::UltimaClient()
	: io(), thread(), socket(io), timer(io), connecting(false)
{
	this->buffer.reserve(2048);

	work = make_shared<io_service::work>(io);
	this->thread = std::thread([this]() { 
		LOG(INFO) << "io service running";
		this->io.run();
		LOG(INFO) << "io service stopped";
	});

	registerHandlers();
}

UltimaClient::~UltimaClient()
{
	this->work.reset();
	this->io.stop();
	if (this->thread.joinable())
		this->thread.join();
}

void UltimaClient::connect(const std::string& address)
{
	this->address = address;
	this->reconnect();
}

void UltimaClient::reconnect()
{
	stringstream ss(address);
	string item;
	vector<string> v;

	while (std::getline(ss, item, ':'))
	{
		if (item.size() > 0)
			v.push_back(item);
	}

	if (v.size() == 0) throw exception("Invalid address");
	if (v.size() < 2) v.push_back("6300");

	io.dispatch([this, v]()
	{
		if (connecting)
			return;
		connecting = true;

		tcp::resolver res(io);
		auto resolved = res.resolve(tcp::resolver::query(v[0], v[1]));

		async_connect(socket, resolved, [this](boost::system::error_code ec, tcp::resolver::iterator)
		{
			this->connecting = false;

			if (!ec)
			{
				LOG(INFO) << "Connected to " << this->address;
				this->socket.set_option(tcp::no_delay(true));

				this->sendBuffers.clear();

				doRead();
			}
			else
			{
				LOG(WARNING) << "Could not connect to " << this->address;
				postReconnect();
			}
		});

	});
}

void UltimaClient::doWrite()
{
	const auto& b = sendBuffers.front();
	LOG(INFO) << "Sending " << b.size() << " B";
	socket.async_send(boost::asio::buffer(b, b.size()),
		[this](const boost::system::error_code& ec, size_t sent)
	{
		LOG(INFO) << "handle write " << ec.message() << " " << sent;
		this->handleWrite(ec, sent);
	});
}

void UltimaClient::handleWrite(const boost::system::error_code& ec, size_t sent)
{
	if (ec)
	{
		this->reconnect();
		return;
	}

	sendBuffers.pop_front();
	if (sendBuffers.size() > 0)
	{
		doWrite();
	}
}

void UltimaClient::postReconnect()
{
	timer.expires_from_now(boost::posix_time::seconds(15));

	timer.async_wait([this](const boost::system::error_code& ec)
	{
		if (!ec)
		{
			this->reconnect();
		}
	});
}


void UltimaClient::doRead()
{
	this->socket.async_read_some(boost::asio::buffer(this->buffer, this->buffer.capacity()),
		[this](const boost::system::error_code& ec, size_t received)
	{
		if (ec)
		{
			this->reconnect();
			return;
		}

		recvBuffer.insert(recvBuffer.end(), buffer.begin(), buffer.begin() + received);

		handlePackets();

		doRead();
	});
}

void UltimaClient::handlePackets()
{
	while (recvBuffer.size() >= 8)
	{
		const int* d = reinterpret_cast<const int*>(recvBuffer.data());
		size_t length = *(size_t*)d;
		int msgType = *(d + 1);

		if (recvBuffer.size() >= length)
		{
			auto handler = handlers.find(msgType);

			if (handler == handlers.end())
			{
				LOG(WARNING) << "Invalid msg type packet received: " << msgType;
			}
			else
			{
				handler->second((const char*) (d+2), length - 8);
			}

			recvBuffer.erase(recvBuffer.begin(), recvBuffer.begin() + length);
		}
	}
}

void UltimaClient::registerHandlers()
{
	this->handlers[(int) MessageId::SymbolRegister] = [this](const char* buffer, size_t length)
	{
		pushToQueue(buffer, length, symbolRegQ);
	};
	this->handlers[(int) MessageId::CloseOrder] = [this](const char* buffer, size_t length)
	{
		pushToQueue(buffer, length, closeOrderQ);
	};
	this->handlers[(int) MessageId::OpenOrder] = [this](const char* buffer, size_t length)
	{
		pushToQueue(buffer, length, openOrderQ);
	};
	this->handlers[(int) MessageId::ModifyOrder] = [this](const char* buffer, size_t length)
	{
		pushToQueue(buffer, length, modifyOrderQ);
	};
	this->handlers[(int) MessageId::CloseOrderBy] = [this](const char* buffer, size_t length)
	{
		pushToQueue(buffer, length, closeOrderByQ);
	};
	this->handlers[(int) MessageId::HistoryOrdersRequest] = [this](const char* buffer, size_t length)
	{
		pushToQueue(buffer, length, requestHistoryQ);
	};
}
