// Copyright 2020 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using System.IO;
using Nuke.Common.CI.TeamCity;
using Nuke.Common.Utilities;
using Xunit;

namespace Nuke.Common.Tests.CI
{
    public class ConfigurationGenerationTest
    {
        [Fact]
        public void Test()
        {

        }

        private class TeamCityTestAttribute : TeamCityAttribute
        {
            public TeamCityTestAttribute(TeamCityAgentPlatform platform, Stream stream)
                : base(platform)
            {
            }

            public override CustomFileWriter CreateWriter()
            {
                return new
            }
        }

        public class TestBuild : NukeBuild
        {
            Target A => _ => _
                .Executes(() =>
                {

                });
        }
    }
}
