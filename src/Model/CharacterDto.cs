using NetStone.Model.Parseables.Character;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStoneMCP.Model
{
    public class CharacterDto
    {
        public LodestoneCharacter? Character { get; set; }
        public required string Race { get; set; }
    }
}
