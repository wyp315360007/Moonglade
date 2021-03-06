﻿using System;
using System.Collections.Generic;

namespace Moonglade.DateTimeOps
{
    public interface IDateTimeResolver
    {
        DateTime NowOfTimeZone { get; }

        DateTime ToTimeZone(DateTime utcDateTime);
        DateTime ToUtc(DateTime userDateTime);
        IEnumerable<TimeZoneInfo> ListTimeZones();
        TimeSpan GetTimeSpanByZoneId(string timeZoneId);
    }
}