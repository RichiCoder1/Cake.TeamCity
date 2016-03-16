using System.Collections.Generic;

namespace Cake.TeamCity
{
    public class TeamCityProperties : Dictionary<string, string>
    {
        internal TeamCityProperties(IDictionary<string, string> properties)
            :base(properties) { }

        // TODO: Maybe add helpers for common TeamCity Properties?
        // https://confluence.jetbrains.com/display/TCD9/Predefined+Build+Parameters
    }
}
