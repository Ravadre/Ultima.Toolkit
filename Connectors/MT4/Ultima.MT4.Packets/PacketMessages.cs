using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stacks;

namespace Ultima.MT4.Packets
{
    [StacksMessage(1)]
    public partial class LoginDTO { }

    [StacksMessage(2)]
    public partial class PriceDTO { }

    [StacksMessage(3)]
    public partial class CommandResultDTO { }

    [StacksMessage(4)]
    public partial class UpdateOrdersDTO { }

    [StacksMessage(5)]
    public partial class HistoryOrderInfoDTO { }

    [StacksMessage(6)]
    public partial class SymbolRegistrationDTO { }

    [StacksMessage(7)]
    public partial class CloseOrderCommandDTO { }

    [StacksMessage(8)]
    public partial class OpenOrderCommandDTO { }

    [StacksMessage(9)]
    public partial class ModifyOrderCommandDTO { }

    [StacksMessage(10)]
    public partial class CloseOrderByCommandDTO { }

    [StacksMessage(11)]
    public partial class OrdersHistoryDTO { }
}
