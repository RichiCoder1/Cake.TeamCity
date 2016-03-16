using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.IO;
using Kajabity.Tools.Java;

namespace Cake.TeamCity
{
    /// <summary>
    /// Contains functionality related to running AliaSql.
    /// </summary>
    [CakeAliasCategory("TeamCity")]
    public static class TeamCityExtensions
    {
        /// <summary>
        /// Retrieves all the teamcity properties for the current build.
        /// </summary>
        /// <param name="context">The context.</param>
        [CakeMethodAlias]
        public static TeamCityProperties GetTeamCityProperties(this ICakeContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var propertiesFilePath = context.Environment.GetEnvironmentVariable("TEAMCITY_BUILD_PROPERTIES_FILE");
            if (string.IsNullOrWhiteSpace(propertiesFilePath))
            {
                throw new CakeException("Failed to find teamcity properties file environment variable. Ensure you're running in TeamCity.");
            }

            var propertiesFile = context.FileSystem.GetFile(propertiesFilePath);
            if (!propertiesFile.Exists)
            {
                throw new CakeException("Failed to find teamcity properties file. Ensure you're running in TeamCity.");
            }

            var propertiesDictionary = new Dictionary<string, string>();
            using (var fileStream = propertiesFile.OpenRead())
            {
                var properties = new JavaProperties();
                properties.Load(fileStream);
                foreach (var property in properties.Cast<DictionaryEntry>())
                {
                    propertiesDictionary.Add((string) property.Key, (string) property.Value);
                }
            }

            return new TeamCityProperties(propertiesDictionary);
        }
    }
}
