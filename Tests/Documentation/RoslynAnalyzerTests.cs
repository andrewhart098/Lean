using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using QuantConnect.Api;
using QuantConnect.CloudManagement.Packets;
using QuantConnect.CodeSuggestion;
using QuantConnect.Packets;

namespace QuantConnect.Tests.Documentation
{
    [TestFixture]
    public class RoslynAnalyzerTests
    {
        private int _rowNumber = 35;
        private string _sourceCode;
        private RoslynAnalyzer _roslynAnalyzer;
        

        [TestFixtureSetUp]
        public void RoslynAnalyzerTestsSetup()
        {
            _sourceCode = File.ReadAllText(@"..\..\..\Algorithm.CSharp\BasicTemplateAlgorithm.cs");
            _roslynAnalyzer = new RoslynAnalyzer();
        }

        [Test]
        public void SymbolProjectSuggestions_CanBeGenerated_Successfully()
        {
            var packet = CreateDocumentationPacket("var sym = new Symbol");
            var suggestions = _roslynAnalyzer.GetSuggestions(packet);

            var identifierSuggestion = suggestions.First() as IdentifierSuggestion;

            Assert.AreEqual(suggestions.Count, 1);
            Assert.IsNotNull(identifierSuggestion);
            Assert.AreEqual(identifierSuggestion.Methods.Count(), 12);
            Assert.AreEqual(identifierSuggestion.Properties.Count(), 2);
            Assert.AreEqual(identifierSuggestion.Constructors.Count(), 1);
        }

        [Test]
        public void HistoryMethodSuggestions_CanBeGenerated_Successfully()
        {
            var packet = CreateDocumentationPacket("History(");
            var suggestions = _roslynAnalyzer.GetSuggestions(packet);
            Assert.IsTrue(suggestions.Count == 17);
        }

        [Test]
        public void IndicatorSuggestions_CanBeCreatedSuccessfully()
        {
            var packet = CreateDocumentationPacket(" var rsi = RSI(");
            var suggestions = _roslynAnalyzer.GetSuggestions(packet);

            Assert.AreEqual(suggestions.Count(), 1);
        }

        private DocumentationPacket CreateDocumentationPacket(string code)
        {
            // Insert code to line 35
            var lines = _sourceCode.Split(new[] { "\r\n" }, StringSplitOptions.None);

            var codeLinesList = lines.ToList();
            codeLinesList.Insert(_rowNumber, code);

            var insertedCode = string.Join("\r\n", codeLinesList.ToArray());

            // Create documentation packet
            return new DocumentationPacket()
            {
                Files = new List<ProjectFile>()
                {
                    new ProjectFile()
                    {
                        Code = insertedCode,
                        Name = "BasicTemplateAlgo.cs"
                    }
                },
                Row = _rowNumber,
                Column = 0
            };
        }
    }
}
