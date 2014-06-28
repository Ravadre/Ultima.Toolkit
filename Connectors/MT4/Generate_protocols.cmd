@echo off
echo Generating protocols for C++... 
..\..\Tools\protoc.exe --cpp_out=.\ Protocol.proto
move /Y Protocol.pb.h UltimaConnector\include
move /Y Protocol.pb.cc UltimaConnector\src
echo Done


echo Generating protocols for C#... 
..\..\Tools\ProtoGen\protogen.exe -i:"Protocol.proto" -o:"Ultima.MT4.Packets\Packets.cs"
echo Done