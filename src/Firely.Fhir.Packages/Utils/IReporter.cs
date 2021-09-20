using System;

namespace Firely
{
    public interface IReporter
    {
        void Report(string message);
    }

    public class ConsoleReporter : IReporter
    {
        public void Report(string message)
        {
            Console.WriteLine(message);
        }
    }
}
