using System;
using System.Collections.Generic;
using System.Text;
using Shared.Contracts.Events;

namespace Shared.Contracts.Queue.Consumer
{
    public interface IRabbitMqMessageConsumer<T> : IMessageConsumer<T>
    where T : IIntegrationEvent;
}
