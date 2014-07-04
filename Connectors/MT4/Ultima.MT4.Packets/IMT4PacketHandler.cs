using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ultima.MT4.Packets
{
    public interface IMT4PacketHandler
    {
        IObservable<LoginDTO> Login { get; }
        IObservable<PriceDTO> Price { get; }
        IObservable<CommandResultDTO> CommandResult { get; }
        IObservable<UpdateOrdersDTO> UpdateOrders { get; }
        IObservable<HistoryOrderInfoDTO> HistoryOrderInfo { get; }
    }
}
