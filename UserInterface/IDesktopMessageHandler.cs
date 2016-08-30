using QuantConnect.Packets;

namespace QuantConnect.Views
{
    public interface IDesktopMessageHandler
    {
        void Initialize(AlgorithmNodePacket job);
        void DisplayHandledErrorPacket(HandledErrorPacket packet);

        void DisplayRuntimeErrorPacket(RuntimeErrorPacket packet);

        void DisplayLogPacket(LogPacket packet);

        void DisplayDebugPacket(DebugPacket packet);

        void DisplayBacktestResultsPacket(BacktestResultPacket packet);
    }
}