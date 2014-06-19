#include "stdafx.h"
#include "UltimaClient.hpp"

using namespace std;
using namespace asio;
using namespace asio::ip;

UltimaClient::UltimaClient()
	: io(), thread(), socket(io), timer(io)
{
	work = make_shared<io_service::work>(io);
	this->thread = std::thread([this]() { 
		LOG(INFO) << "io service running";
		this->io.run();
		LOG(INFO) << "io service stopped";
	});
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

	io.dispatch([this,v]() 
	{
		tcp::resolver res(io);
		auto resolved = res.resolve(tcp::resolver::query(v[0], v[1]));

		async_connect(socket, resolved, [this](std::error_code ec, tcp::resolver::iterator) 
		{
			if (!ec)
			{
				LOG(INFO) << "Connected to " << this->address;
				this->socket.set_option(tcp::no_delay(true));
			}
			else
			{
				LOG(WARNING) << "Could not connect to " << this->address;
				timer.expires_from_now(boost::posix_time::seconds(15));

				timer.async_wait([this](const std::error_code& ec)
				{
					if (!ec)
					{
						this->connect(this->address);
					}
				});
			}
		});

	});
	//res.resolve()


}