using System;
using System.Collections.Generic;
using NLog;

namespace OpenSage.Diagnostics.Util
{
    /// <summary>
    /// This wrapper of NLog.Logger can be used to avoid duplicate warnings
    /// </summary>
    class DistinctLogger
    {
        private readonly object _lock = new();
        private readonly Dictionary<object, int> _duplicates = new();
        private readonly Logger _logger;

        public DistinctLogger(Logger logger)
        {
            _logger = logger;
        }

        public void Info(string message) => Info(message, message);
        public void Info(object key, string message) => Log(_logger.Info, key, message);
        public void Warn(string message) => Warn(message, message);
        public void Warn(object key, string message) => Log(_logger.Warn, key, message);

        private void Log(Action<string> log, object key, string message)
        {
            lock (_lock)
            {
                if (_duplicates.TryGetValue(key, out var count))
                {
                    _duplicates[key] = count + 1;
                    return;
                }
                log(message);
                _duplicates[key] = 0;
            }
        }
    }
}
