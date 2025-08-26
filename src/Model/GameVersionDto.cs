using NetStone.Model.Parseables.Character;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStoneMCP.Model
{
    public readonly record struct LatestVersionDto(string Slug, string VersionString);
}
