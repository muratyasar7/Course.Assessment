using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Contracts.Options
{
    public class ServiceBusOption
    {
        public required string RabbitMqConnectionString { get; set; }
    }
}
