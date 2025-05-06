using NetStoneMCP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStoneMCP.Dict
{
    public static class FFXIVRacesDict
    {
        public static readonly Dictionary<string, RaceInfoDto> Races = new()
        {
            ["Hyur"] = new RaceInfoDto
            {
                ChineseName = "人族",
                Clans = new Dictionary<string, string>
                {
                    ["Midlander"] = "中原之民",
                    ["Highlander"] = "高地之民"
                }
            },
            ["Elezen"] = new RaceInfoDto
            {
                ChineseName = "精靈族",
                Clans = new Dictionary<string, string>
                {
                    ["Wildwood"] = "森林之民",
                    ["Duskwight"] = "黑影之民"
                }
            },
            ["Lalafell"] = new RaceInfoDto
            {
                ChineseName = "拉拉菲爾族",
                Clans = new Dictionary<string, string>
                {
                    ["Plainsfolk"] = "平原之民",
                    ["Dunesfolk"] = "沙漠之民"
                }
            },
            ["Miqo'te"] = new RaceInfoDto
            {
                ChineseName = "貓魅族",
                Clans = new Dictionary<string, string>
                {
                    ["Seekers of the Sun"] = "逐日之民",
                    ["Keepers of the Moon"] = "護月之民"
                }
            },
            ["Roegadyn"] = new RaceInfoDto
            {
                ChineseName = "魯加族",
                Clans = new Dictionary<string, string>
                {
                    ["Sea Wolves"] = "北洋之民",
                    ["Hellsguard"] = "紅焰之民"
                }
            },
            ["Au Ra"] = new RaceInfoDto
            {
                ChineseName = "敖龍族",
                Clans = new Dictionary<string, string>
                {
                    ["Raen"] = "晨曦之民",
                    ["Xaela"] = "暮輝之民"
                }
            },
            ["Viera"] = new RaceInfoDto
            {
                ChineseName = "維埃拉族",
                Clans = new Dictionary<string, string>
                {
                    ["Rava"] = "密林之民",
                    ["Veena"] = "山林之民"
                }
            },
            ["Hrothgar"] = new RaceInfoDto
            {
                ChineseName = "硌獅族",
                Clans = new Dictionary<string, string>
                {
                    ["Helions"] = "日光之民",
                    ["The Lost"] = "遺失之民"
                }
            }
        };
    }
}
