using Microsoft.AspNetCore.Mvc;
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
    public class NoteTool(INoteService noteService,
                          ILogger<CommonTool> logger)
    {
        private readonly INoteService _noteService = noteService;
        private readonly ILogger<CommonTool> _logger = logger;

        [McpServerTool(Name = "add_note", Title = "Add note")]
        [Description("紀錄任何一句話，幫你回憶重要的事情、教學、網址等等")]
        public async Task AddNote([FromBody] string content)
        {
            await _noteService.AddNoteAsync(content);
        }

        [McpServerTool(Name = "search_notes", Title = "Search notes by keyword")]
        [Description("根據輸入的關鍵字搜尋筆記內容")]
        public async Task<IEnumerable<NoteDto>> SearchNotes([FromBody] string keyword)
        {
            return await _noteService.SearchNotesAsync(keyword);
        }
    }
}
