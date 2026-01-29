using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Domain.QueueMessageEventModels.v1.Order
{
    public sealed record OrderCanceledEvent(Guid OrderId);
}
