using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using NetStoneMCP.Model;
using NetStoneMCP.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetStoneMCP.Tools
{
    [McpServerToolType]
    public class CommonTool(ICommonService commonService, 
                            ILodeStoneNewsService lodeStoneNewsService, 
                            IThaliakService thaliakService, 
                            ICustomService customService,
                            ILogger<CommonTool> logger)
    {
        private readonly ICommonService _commonService = commonService;
        private readonly ILodeStoneNewsService _lodeStoneNewsService = lodeStoneNewsService;
        private readonly IThaliakService _thaliakService = thaliakService;
        private readonly ICustomService _customService = customService;
        private readonly ILogger<CommonTool> _logger = logger;

        [McpServerTool(Name = "get_data_center", Title = "Get data center information")]
        [Description("Get all FFXIV Data Centers (e.g. Elemental, Gaia, Mana, Meteor, Aether, Crystal, Primal, Chaos, Light, Materia).")]
        public async Task<IEnumerable<DataCenterDto>?> GetDataCenter(CancellationToken cancellationToken = default)
        {
            return await _commonService.GetDataCenter(cancellationToken);
        }

        [McpServerTool(Name = "get_world", Title = "Get world information")]
        [Description("Get all FFXIV Worlds (servers) and their associated Data Centers.")]
        public async Task<IEnumerable<WorldDto>?> GetWorlds(CancellationToken cancellationToken = default)
        {
            return await _commonService.GetWorlds(cancellationToken);
        }

        [McpServerTool(
            Name = "ffxiv_get_current_maintenance",
            Title = "FFXIV Current Maintenance Info"
        )]
        [Description("Retrieve current and upcoming Final Fantasy XIV maintenance events (start time, end time, description).")]
        public async Task<LodeStoneNewsMaintenance?> GetCurrentMaintenance(CancellationToken cancellationToken = default)
        {
            return await _lodeStoneNewsService.GetCurrentMaintenances(cancellationToken);
        }

        [McpServerTool(Name = "ffxiv_get_topics", Title = "FFXIV Get Topics")]
        [Description("Retrieves the latest 'Topics' articles and news from the Final Fantasy XIV (FFXIV) Lodestone website.")]
        public async Task<IEnumerable<LodeStoneNewsItem>?> GetTopics(CancellationToken cancellationToken = default)
        {
            return await _lodeStoneNewsService.GetTopics(cancellationToken);
        }

        [McpServerTool(Name = "ffxiv_get_post", Title = "FFXIV Get Posts")]
        [Description("Retrieves detailed information for a specific Final Fantasy XIV (FFXIV) Lodestone news post by its ID.")]
        public async Task<LodeStoneNewsItem?> GetPost(
            [Description("The Lodestone news/post ID (e.g. obtained from ffxiv_get_topics).")] string id,
            CancellationToken cancellationToken = default)
        {
            return await _lodeStoneNewsService.GetPost(id, cancellationToken);
        }

        [McpServerTool(Name = "get_ffxiv_latest_versions", Title = "Get FFXIV Latest Game Versions")]
        [Description("Fetch the latest available version numbers for FFXIV game and expansions (boot, game, ex1~ex5).")]
        public async Task<IEnumerable<LatestVersionDto>> GetLatestVersionsAsync(CancellationToken cancellationToken = default)
        {
            return await _thaliakService.GetLatestVersionsAsync(cancellationToken);
        }

        [McpServerTool(
            Name = "get_ffxiv_latest_chn_text_patch",
            Title = "Get Latest FFXIV Chinese Patch Download Link (獲取最新FFXIV漢化/中文化補丁下載連結)"
        )]
        [Description("取得 Final Fantasy XIV (FFXIV/FF14) 社群繁體/簡體中文化漢化補丁 (Chinese Localization Text Patch) 的最新版本資訊與 GitHub 下載連結。當使用者詢問「最新的漢化」、「FFXIV漢化」、「FF14漢化」、「漢化」、「漢化包」、「中文化補丁」、「繁中補丁」時，必須調用此工具。")]
        public async Task<GithubReleaseDto?> GetFFXIVChnTextPatchLastReleaseAsync(CancellationToken cancellationToken = default)
        {
            return await _customService.GetGithubLastReleaseAsync(cancellationToken);
        }

        [McpServerTool(
            Name = "get_ffxiv_cht_lock_server",
            Title = "Get FFXIV Traditional Chinese servers character creation status"
        )]
        [Description("Retrieves the current character creation availability (open/closed) for each World on the Final Fantasy XIV Traditional Chinese servers, and returns a concise summary of the latest status.")]
        public async Task<string?> GetFFXIVTraditionalChineseLockServerStatusAsync(CancellationToken cancellationToken = default)
        {
            return await _commonService.GetFFXIVTraditionalChineseLockServerStatus(cancellationToken);
        }
    }
}
