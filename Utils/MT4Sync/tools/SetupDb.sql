create table Orders
(
	Id bigint IDENTITY(1, 1) NOT NULL,
	PlatformTag nvarchar(max),
	[Order] int,
	Type int,
	Volume float,

	StopLoss float,
	TakeProfit float,

	OpenPrice float,
	OpenTime datetime,

	CurrentPrice float,

	Profit float,
	Commission float,
	Swap float,

	Comment nvarchar(max),
	MagicNumber float
);

--create table History
--(
--	Id bigint IDENTITY(1, 1) NOT NULL,
--	PlatformTag nvarchar(max),
--	[Order] int,
--	Type int,
--	Volume float,

--	StopLoss float,
--	TakeProfit float,

--	OpenPrice float,
--	OpenTime datetime,

--	ClosePrice float,
--	CloseTime datetime,

--	Profit float,
--	Commission float,
--	Swap float,

--	Comment nvarchar(max),
--	MagicNumber float
--);