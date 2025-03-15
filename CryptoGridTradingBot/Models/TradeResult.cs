using System;

namespace CryptoGridTradingBot.Models
{
    public class TradeResult
    {
        public DateTime Timestamp { get; set; }
        public string Symbol { get; set; }
        public double Price { get; set; }
        public double Volume { get; set; }
        public double Profit { get; set; } // Прибыль или убыток
    }
}