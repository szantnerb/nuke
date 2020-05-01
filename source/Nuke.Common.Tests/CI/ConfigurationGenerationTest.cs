// Copyright 2020 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nuke.Common.CI;
using Nuke.Common.CI.TeamCity;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities;
using Xunit;

namespace Nuke.Common.Tests.CI
{
    public class ConfigurationGenerationTest
    {
        [Theory]
        [MemberData(nameof(GetAttributes))]
        public void Test(string testName, ITestConfigurationGenerator attribute)
        {
            var build = new TestBuild();
            var executableTargets = ExecutableTargetFactory.CreateAll(build, x => x.A);

            var stream = new MemoryStream();
            attribute.Stream = stream;
            attribute.Generate(build, executableTargets);
        }

        public static IEnumerable<object[]> GetAttributes()
        {
            return TestBuild.GetAttributes().Select(x => new object[] { x.TestName, x.Generator });
        }
    }

    public class TestBuild : NukeBuild
    {
        public static IEnumerable<(string TestName, IConfigurationGenerator Generator)> GetAttributes()
        {
            yield return
            (
                "TeamCity",
                new TestTeamCityAttribute(TeamCityAgentPlatform.Unix)
                {
                    NonEntryTargets = new[] { nameof(Clean) },
                    VcsTriggeredTargets = new[] { nameof(Test), nameof(Pack) },
                    ManuallyTriggeredTargets = new[] { nameof(Publish) },
                    NightlyTriggeredTargets = new[] { nameof(Publish) }
                }
            );
        }

        public Target Clean => _ => _
            .Before(Restore);

        [Parameter] public readonly bool IgnoreFailedSources;

        public Target Restore => _ => _;

        [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
        public readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

        public AbsolutePath OutputDirectory => RootDirectory / "output";

        public Target Compile => _ => _
            .DependsOn(Restore);

        public AbsolutePath PackageDirectory => OutputDirectory / "packages";

        public Target Pack => _ => _
            .DependsOn(Compile)
            .Produces(PackageDirectory / "*.nupkg");

        [Partition(2)] public readonly Partition TestPartition;
        public AbsolutePath TestResultDirectory => OutputDirectory / "test-results";

        public Target Test => _ => _
            .DependsOn(Compile)
            .Produces(TestResultDirectory / "*.trx")
            .Produces(TestResultDirectory / "*.xml")
            .Partition(() => TestPartition);

        public string CoverageReportArchive => OutputDirectory / "coverage-report.zip";

        public Target Coverage => _ => _
            .DependsOn(Test)
            .TriggeredBy(Test)
            .Consumes(Test)
            .Produces(CoverageReportArchive);

        [Parameter("NuGet Api Key")] public readonly string ApiKey;

        [Parameter("NuGet Source for Packages")]
        public readonly string Source = "https://api.nuget.org/v3/index.json";

        public Target Publish => _ => _
            .DependsOn(Clean, Test, Pack)
            .Consumes(Pack)
            .Requires(() => ApiKey);

        public Target Announce => _ => _
            .TriggeredBy(Publish)
            .AssuredAfterFailure();
    }
}
