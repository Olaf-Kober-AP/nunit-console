﻿// ***********************************************************************
// Copyright (c) 2018 Charlie Poole, Rob Prouse
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System.Collections.Generic;
using NUnit.Engine.Runners;
using NUnit.Framework;

namespace NUnit.Engine.Tests.Services.TestRunnerFactoryTests.TestCases
{
#if NETCOREAPP1_1 || NETCOREAPP2_0
    internal static class NetStandardTestCases
    {
        public static IEnumerable<TestCaseData> TestCases
        {
            get
            {
                var testName = "Single assembly";
                var package = TestPackageFactory.OneAssembly();
                var expected = new RunnerResult { TestRunner = typeof(LocalTestRunner) };
                yield return new TestCaseData(package, expected).SetName($"{{m}}({testName})");

                testName = "Two assemblies";
                package = TestPackageFactory.TwoAssemblies();
                expected = new RunnerResult
                {
                    TestRunner = typeof(AggregatingTestRunner),
                    SubRunners = new[]
                    {
                        new RunnerResult { TestRunner = typeof(LocalTestRunner) },
                        new RunnerResult { TestRunner = typeof(LocalTestRunner) }
                    }
                };
                yield return new TestCaseData(package, expected).SetName($"{{m}}({testName})");

#if NETCOREAPP2_0
                testName = "Single project";
                package = TestPackageFactory.OneProject();
                expected = new RunnerResult
                {
                    TestRunner = typeof(AggregatingTestRunner),
                    SubRunners = new[]
                    {
                        new RunnerResult { TestRunner = typeof(LocalTestRunner) }
                    }
                };
                yield return new TestCaseData(package, expected).SetName($"{{m}}({testName})");
#endif
            }
        }
    }
#endif
}