using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using YachtDice.Server.Logic.Configuration;

namespace YachtDice.Server.Tests.Configuration
{
    /// <summary>
    /// Reads the same file of rules that the server uses, so the tests check the real achievements and levels.
    /// </summary>
    internal static class GameRulesFile
    {
        private const string RelativePath = "Resources\\GameRules.xml";

        /// <summary>
        /// Reads the rules of the game of the server.
        /// </summary>
        /// <returns>The rules of the game.</returns>
        public static GameRules Load()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, RelativePath);

            using (XmlReader reader = XmlReader.Create(path))
            {
                return GameRulesReader.Read(XDocument.Load(reader));
            }
        }
    }
}
