using System;
using System.Collections.Generic;
using System.Text;

namespace Course.Assessment.Order.Domain.Shared
{
    public sealed record Address(
       string Country,
       string State,
       string ZipCode,
       string City,
       string Street);
}
