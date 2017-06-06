/*
* QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
* Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
* 
* Licensed under the Apache License, Version 2.0 (the "License"); 
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
* 
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using NUnit.Framework;
using QuantConnect.AlgorithmFactory;
using QuantConnect.Interfaces;
using System;
using System.IO;
using System.Linq;

namespace QuantConnect.Tests.AlgorithmFactory
{
    [TestFixture, Ignore]
    public class LoaderTests
    {
        [Test]
        public void LoadsSamePythonAlgorithmTwice()
        {
            var assemblyPath = "../../../Algorithm.Python/BasicTemplateAlgorithm.py";

            string error1;
            IAlgorithm algorithm1;
            var one = new Loader(Language.Python, TimeSpan.FromMinutes(1), names => names.SingleOrDefault())
                .TryCreateAlgorithmInstanceWithIsolator(assemblyPath, 512, out algorithm1, out error1);

            string error2;
            IAlgorithm algorithm2;
            var two = new Loader(Language.Python, TimeSpan.FromMinutes(1), names => names.SingleOrDefault())
                .TryCreateAlgorithmInstanceWithIsolator(assemblyPath, 512, out algorithm2, out error2);

            Assert.AreNotEqual(algorithm1.ToString(), algorithm2.ToString());
        }

        [Test]
        public void LoadsTwoDifferentPythonAlgorithm()
        {
            var assemblyPath1 = "../../../Algorithm.Python/BasicTemplateAlgorithm.py";
            var assemblyPath2 = "../../../Algorithm.Python/AddRemoveSecurityRegressionAlgorithm.py";

            string error1;
            IAlgorithm algorithm1;
            var one = new Loader(Language.Python, TimeSpan.FromMinutes(1), names => names.SingleOrDefault())
                .TryCreateAlgorithmInstanceWithIsolator(assemblyPath1, 512, out algorithm1, out error1);

            string error2;
            IAlgorithm algorithm2;
            var two = new Loader(Language.Python, TimeSpan.FromMinutes(1), names => names.SingleOrDefault())
                .TryCreateAlgorithmInstanceWithIsolator(assemblyPath2, 512, out algorithm2, out error2);

            Assert.AreNotEqual(algorithm1.ToString(), algorithm2.ToString());
        }

        [Test]
        public void PythonModules_WithTheSameName_AreLoadedAsTheSameModule()
        {
            var originalFile = "../../../Algorithm.Python/BasicTemplateAlgorithm.py";

            var assemblyFileName = "main.py";
            var compiledAssembly = "main.pyc";

            var guid1 = Guid.NewGuid().ToString();
            var guid2 = Guid.NewGuid().ToString();

            var assembly1Directory = Path.Combine(Directory.GetCurrentDirectory(), guid1);
            var assembly2Directory = Path.Combine(Directory.GetCurrentDirectory(), guid2);
            Directory.CreateDirectory(assembly1Directory);
            Directory.CreateDirectory(assembly2Directory);

            var assemblyName1Path = Path.Combine(assembly1Directory, assemblyFileName);
            var assemblyName2Path = Path.Combine(assembly2Directory, assemblyFileName);
            var compiledAssemblyPath = Path.Combine(Directory.GetCurrentDirectory(), compiledAssembly);

            var originalLines = File.ReadAllLines(originalFile).Where(x => !x.Contains("np")).ToArray();
            var newLines = originalLines.Select(x => x.Replace("SPY", "BAC")).ToArray();

            File.WriteAllLines(assemblyName1Path, originalLines);
            File.WriteAllLines(assemblyName2Path, newLines);

            string error1;
            IAlgorithm algorithm1;
            var one = new Loader(Language.Python, TimeSpan.FromMinutes(5), names => names.SingleOrDefault())
                .TryCreateAlgorithmInstanceWithIsolator(assemblyName1Path, 512, out algorithm1, out error1);

            algorithm1.Initialize();
            var keys1 = algorithm1.Securities.Keys;

            Directory.Delete(assembly1Directory, true);

            string error2;
            IAlgorithm algorithm2;
            var two = new Loader(Language.Python, TimeSpan.FromMinutes(5), names => names.SingleOrDefault())
                .TryCreateAlgorithmInstanceWithIsolator(assemblyName2Path, 512, out algorithm2, out error2);

            algorithm2.Initialize();
            var keys2 = algorithm2.Securities.Keys;

            Assert.AreEqual(keys1, keys2);

            Directory.Delete(assembly2Directory, true);
            File.Delete(compiledAssemblyPath);
        }


        [Test]
        public void PythonModules_WithDifferentNames_AreLoadedAsDifferentModules()
        {
            var originalFile = "../../../Algorithm.Python/BasicTemplateAlgorithm.py";

            var assemblyFileName = "main.py";
            var assemblyFileName2 = "main2.py";

            var assembly1Directory = Directory.GetCurrentDirectory();
            var assembly2Directory = Directory.GetCurrentDirectory();

            var assemblyName1Path = Path.Combine(assembly1Directory, assemblyFileName);
            var assemblyName2Path = Path.Combine(assembly2Directory, assemblyFileName2);

            var originalLines = File.ReadAllLines(originalFile).Where(x => !x.Contains("np")).ToArray();
            var newLines = originalLines.Select(x => x.Replace("SPY", "BAC")).ToArray();

            File.WriteAllLines(assemblyName1Path, originalLines);
            File.WriteAllLines(assemblyName2Path, newLines);

            string error1;
            IAlgorithm algorithm1;
            var one = new Loader(Language.Python, TimeSpan.FromMinutes(5), names => names.SingleOrDefault())
                .TryCreateAlgorithmInstanceWithIsolator(assemblyName1Path, 512, out algorithm1, out error1);

            algorithm1.Initialize();
            var keys1 = algorithm1.Securities.Keys;

            string error2;
            IAlgorithm algorithm2;
            var two = new Loader(Language.Python, TimeSpan.FromMinutes(5), names => names.SingleOrDefault())
                .TryCreateAlgorithmInstanceWithIsolator(assemblyName2Path, 512, out algorithm2, out error2);

            algorithm2.Initialize();
            var keys2 = algorithm2.Securities.Keys;

            Assert.AreNotEqual(keys1, keys2);

            File.Delete(assemblyName1Path);
            File.Delete(assemblyName2Path);

            File.Delete(assemblyName1Path + "c");
            File.Delete(assemblyName2Path + "c");
        }
    }
}