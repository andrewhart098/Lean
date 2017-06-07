using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using QuantConnect.AlgorithmFactory;
using QuantConnect.AlgorithmFactory.Analyze;

namespace QuantConnect.Tests.AlgorithmFactory
{
    [TestFixture, Ignore]
    public class AlgorithmAnalyzeTests
    {
        [Test]
        public void AnalyzeAlgorithmInitialize_CanAnalyzePython_Successfully()
        {
            var originalFile = "../../../Algorithm.Python/BasicTemplateAlgorithm.py";
            var assemblyNamePath = Path.Combine(Directory.GetCurrentDirectory(), "main.py");
            var originalLines = File.ReadAllLines(originalFile).Where(x => !x.Contains("np")).ToArray();
            File.WriteAllLines(assemblyNamePath, originalLines);

            var loader = new Loader(Language.Python, TimeSpan.FromMinutes(2), names => names.SingleOrDefault());

            var result = loader.Analyze(assemblyNamePath);
            Assert.AreEqual(AnalysisResult.Success, result.Result);
        }

        [Test]
        public void AnalyzeAlgorithmInitialize_CanAnalyzeCSharp_Successfully()
        {
            var loader = new Loader(Language.CSharp, TimeSpan.FromMinutes(2), names => names.FirstOrDefault(x => x == "QuantConnect.Algorithm.CSharp.BasicTemplateAlgorithm"));

            var result = loader.Analyze("QuantConnect.Algorithm.CSharp.dll");
            Assert.AreEqual(AnalysisResult.Success, result.Result);
        }

        [Test]
        public void AnalyzeAlgorithmInitialize_CanAnalyzeFSharp_Successfully()
        {
            var loader = new Loader(Language.FSharp, TimeSpan.FromMinutes(2), names => names.FirstOrDefault(x => x == "QuantConnect.Algorithm.FSharp.BasicTemplateAlgorithm"));

            var result = loader.Analyze("QuantConnect.Algorithm.FSharp.dll");
            Assert.AreEqual(AnalysisResult.Success, result.Result);
        }
    }
}
