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

        [McpServerTool(Name = "search_latest_note", Title = "Search latest note by keyword")]
        [Description("根據關鍵字搜尋最接近現在的一筆筆記")]
        public async Task<NoteDto?> SearchLatestNote([FromBody] string keyword)
        {
            var notes = await _noteService.SearchNotesAsync(keyword);
            return notes.OrderByDescending(n => n.Timestamp).FirstOrDefault();
        }

        [McpServerTool(Name = "delete_note", Title = "delete note by Id")]
        [Description("根據Id刪除指定的筆記")]
        public async Task<bool> DeleteNote([FromBody] string Id)
        {
            return await _noteService.DeleteNoteAsync(Id);
        }
    }
}
