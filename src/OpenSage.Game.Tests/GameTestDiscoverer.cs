using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Data;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace OpenSage.Game.Tests
{
    public sealed class GameTestDiscoverer : IXunitTestCaseDiscoverer
    {
        private readonly IMessageSink _diagnosticMessageSink;

        static readonly ISet<SageGame> InstalledGames;

        public GameTestDiscoverer(IMessageSink diagnosticMessageSink)
        {
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            var arguments = factAttribute.GetConstructorArguments();
            var game = (SageGame) arguments.Single();

            if (!InstalledGames.Contains(game))
            {
                _diagnosticMessageSink.OnMessage(
                    new DiagnosticMessage($"Skipped test {testMethod.TestClass.Class.Name}.{testMethod.Method.Name}, because it requires {game} to be installed.")
                );
                yield break;
            }

            yield return new XunitTestCase(_diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod);
        }

        static GameTestDiscoverer()
        {
            var locator = new RegistryInstallationLocator();
            InstalledGames = new HashSet<SageGame>(SageGames.GetAll().Where(game => locator.FindInstallations(game).Any()));
        }
    }

    [XunitTestCaseDiscoverer("OpenSage.Game.Tests.GameTestDiscoverer", "OpenSage.Game.Tests")]
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class GameFact : FactAttribute
    {
        public readonly SageGame Game;

        public GameFact(SageGame game)
        {
            Game = game;
        }
    }
}
