using System;

namespace TimeTravelApi.Models
{
    public interface ITimeTravelClock
    {
        DateTime Now { get; }
    }
}