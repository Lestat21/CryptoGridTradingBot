namespace CryptoGridTradingBot.Services
{
    public class RiskManager
    {
        private readonly double _maxDrawdown;

        public RiskManager(double maxDrawdown)
        {
            _maxDrawdown = maxDrawdown;
        }

        public bool CheckRisk(double currentBalance, double initialBalance)
        {
            double drawdown = (initialBalance - currentBalance) / initialBalance;
            return drawdown <= _maxDrawdown;
        }
    }
}