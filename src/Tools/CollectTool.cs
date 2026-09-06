using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using NetStoneMCP.Model;
using NetStoneMCP.Services;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace NetStoneMCP.Tools
{
    [McpServerToolType]
    public class CollectTool(IFFXIVCollectService collectService, ILogger<CollectTool> logger)
    {
        private readonly IFFXIVCollectService _collectService = collectService;
        private readonly ILogger<CollectTool> _logger = logger;

        [McpServerTool(Name = "get_ffxiv_mount", Title = "Get FFXIV Mount Information")]
        [Description("Search for FFXIV mount information including acquisition sources (how to get it), seats, patch, description, and tradeability. Query can be in English or Japanese.")]
        public async Task<IEnumerable<CollectItemDto>?> GetMount(
            [Description("The mount name (e.g. 'Fatter Cat', 'Regalia Type-G', 'でぶチョコボ').")] string name)
        {
            return await _collectService.SearchMountsAsync(name);
        }

        [McpServerTool(Name = "get_ffxiv_minion", Title = "Get FFXIV Minion Information")]
        [Description("Search for FFXIV minion information including acquisition sources (how and where to obtain it), patch, description, and tradeability. Query can be in English or Japanese.")]
        public async Task<IEnumerable<CollectItemDto>?> GetMinion(
            [Description("The minion name (e.g. 'The Major-General', 'Starbird', '古代人').")] string name)
        {
            return await _collectService.SearchMinionsAsync(name);
        }

        [McpServerTool(Name = "get_ffxiv_emote", Title = "Get FFXIV Emote Information")]
        [Description("Search for FFXIV emote/expression information including unlock command (/emote), acquisition sources, patch, and tradeability. Query can be in English or Japanese.")]
        public async Task<IEnumerable<CollectItemDto>?> GetEmote(
            [Description("The emote name or command (e.g. 'Savor Tea', 'Eat Apple', 'お茶を飲む').")] string name)
        {
            return await _collectService.SearchEmotesAsync(name);
        }

        [McpServerTool(Name = "get_ffxiv_hairstyle", Title = "Get FFXIV Hairstyle Information")]
        [Description("Search for FFXIV hairstyle (Modern Aesthetics) information including acquisition sources (e.g. Island Sanctuary, MGP, PvP, Bozja), patch, and tradeability.")]
        public async Task<IEnumerable<CollectItemDto>?> GetHairstyle(
            [Description("The hairstyle name (e.g. 'Practical Ponytails', 'Early to Rise', '編込').")] string name)
        {
            return await _collectService.SearchHairstylesAsync(name);
        }

        [McpServerTool(Name = "get_ffxiv_orchestrion", Title = "Get FFXIV Orchestrion Roll Information")]
        [Description("Search for FFXIV orchestrion roll (music sheet) information including roll number, category, and acquisition sources (e.g. FATE, Raid, Crafting, Store).")]
        public async Task<IEnumerable<CollectItemDto>?> GetOrchestrion(
            [Description("The orchestrion roll name or track name (e.g. 'Neath Dark Waters', 'Flow', '悠久の風').")] string name)
        {
            return await _collectService.SearchOrchestrionsAsync(name);
        }

        [McpServerTool(Name = "get_ffxiv_blue_mage_spell", Title = "Get FFXIV Blue Mage Spell Information")]
        [Description("Search for FFXIV Blue Mage (BLU) spell information including where/how to learn it (monster, duty, Masked Carnivale), type, aspect, and effect description.")]
        public async Task<IEnumerable<CollectItemDto>?> GetBlueMageSpell(
            [Description("The spell name (e.g. 'Moon Flute', 'Aetherial Spark', '月の笛').")] string name)
        {
            return await _collectService.SearchSpellsAsync(name);
        }

        [McpServerTool(Name = "get_ffxiv_barding", Title = "Get FFXIV Chocobo Barding Information")]
        [Description("Search for FFXIV chocobo barding information including acquisition sources, patch, and tradeability.")]
        public async Task<IEnumerable<CollectItemDto>?> GetBarding(
            [Description("The barding name (e.g. 'Black Mage Barding', 'Far Eastern Barding').")] string name)
        {
            return await _collectService.SearchBardingsAsync(name);
        }

        [McpServerTool(Name = "get_ffxiv_fashion", Title = "Get FFXIV Fashion Accessory Information")]
        [Description("Search for FFXIV fashion accessories (wings, glasses, parasols/umbrellas, backpacks) including acquisition sources, patch, and tradeability.")]
        public async Task<IEnumerable<CollectItemDto>?> GetFashion(
            [Description("The fashion accessory name (e.g. 'Statice\\'s Wings', 'Fallen Angel Wings').")] string name)
        {
            return await _collectService.SearchFashionsAsync(name);
        }

        [McpServerTool(Name = "get_ffxiv_achievement", Title = "Get FFXIV Achievement Information")]
        [Description("Search for FFXIV achievement details, points, descriptions, and unlock rewards (such as titles, mounts, or items).")]
        public async Task<IEnumerable<CollectAchievementDto>?> GetAchievement(
            [Description("The achievement name (e.g. 'Pal-less Palace', 'I Hope Senpai Notices Me').")] string name)
        {
            return await _collectService.SearchAchievementsAsync(name);
        }

        [McpServerTool(Name = "search_ffxiv_collectable", Title = "Search Any FFXIV Collectable")]
        [Description("Search across any collectible category in FFXIV (mounts, minions, emotes, hairstyles, orchestrions, spells, bardings, fashions).")]
        public async Task<IEnumerable<CollectItemDto>?> SearchCollectable(
            [Description("Category: 'mounts', 'minions', 'emotes', 'hairstyles', 'orchestrions', 'spells', 'bardings', 'fashions'.")] string category,
            [Description("The collectible search query.")] string name)
        {
            return await _collectService.SearchCollectablesAsync(category, name);
        }
    }
}
