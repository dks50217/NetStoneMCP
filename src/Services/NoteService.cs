using NetStone.Model.Parseables.Character.ClassJob;
using NetStone.Model.Parseables.Character.Collectable;
using NetStone.Model.Parseables.Character;
using NetStone.Model.Parseables.CWLS;
using NetStone.Model.Parseables.FreeCompany.Members;
using NetStone.Model.Parseables.Linkshell;
using NetStone.Model.Parseables.Search.Character;
using NetStone.Model.Parseables.Search.CWLS;
using NetStone.Model.Parseables.Search.FreeCompany;
using NetStone.Model.Parseables.Search.Linkshell;
using NetStoneMCP.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NetStoneMCP.Services
{
    public interface INoteService
    {
        public Task AddNoteAsync(string userInput);
        public Task<List<NoteDto>> GetAllNotesAsync();
        public Task<List<NoteDto>> SearchNotesAsync(string keyword);
    }


    public class NoteService : INoteService
    {
        private readonly string _filePath = "user_notes.json";
        private const int MaxNoteLength = 512;
        private readonly object _lock = new();

        public NoteService()
        {
            if (!File.Exists(_filePath))
                File.WriteAllText(_filePath, "[]");
        }

        public async Task AddNoteAsync(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                throw new ArgumentException("Content must not be empty.");

            if (userInput.Length > MaxNoteLength)
                throw new ArgumentException($"Note content must not exceed {MaxNoteLength} characters.");

            var notes = await ReadNotesAsync();

            notes.Add(new NoteDto
            {
                Id = Guid.NewGuid().ToString(),
                Timestamp = DateTime.UtcNow,
                Content = userInput
            });

            await SaveNotesAsync(notes);
        }

        public async Task<List<NoteDto>> GetAllNotesAsync()
        {
            return await ReadNotesAsync();
        }

        public async Task<List<NoteDto>> SearchNotesAsync(string keyword)
        {
            var notes = await ReadNotesAsync();
            return notes.Where(n => n.Content.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        private async Task<List<NoteDto>> ReadNotesAsync()
        {
            lock (_lock)
            {
                var json = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<NoteDto>>(json) ?? new();
            }
        }

        private async Task SaveNotesAsync(List<NoteDto> notes)
        {
            lock (_lock)
            {
                var json = JsonSerializer.Serialize(notes, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
        }
    }
}
