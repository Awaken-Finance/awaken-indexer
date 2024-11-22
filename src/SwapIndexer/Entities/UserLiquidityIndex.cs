using System;

using AeFinder.Sdk.Entities;
using Nest;

namespace SwapIndexer.Entities
{
    public class UserLiquidityIndex : AeFinderEntity, IAeFinderEntity
    {
        [Keyword]
        public string Pair { get; set; }
        [Keyword]
        public string Address { get; set; }
        public long LpTokenAmount { get; set; }
        public long Timestamp { get; set; }
    }
}