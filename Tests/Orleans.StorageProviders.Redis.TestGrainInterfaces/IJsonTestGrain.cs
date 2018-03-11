﻿using System;
using System.Threading.Tasks;

namespace Orleans.StorageProviders.Redis.TestGrainInterfaces
{
    public interface IJsonTestGrain : IGrainWithIntegerKey
    {
        Task Set(string stringValue, int intValue, DateTime dateTimeValue, Guid guidValue, IJsonTestGrain grainValue);
        Task<Tuple<string, int, DateTime, Guid, IJsonTestGrain>> Get();
        Task Clear();
    }
}