using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NetStoneMCP.Services;
using NUnit.Framework;

namespace NetStoneMCP.Tests.Services
{
    public class NoteServiceTests
    {
        private const string FileName = "user_notes.json";

        [SetUp]
        public void Setup()
        {
            if (File.Exists(FileName))
                File.Delete(FileName);
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(FileName))
                File.Delete(FileName);
        }

        [Test]
        public async Task AddNoteAsync_WritesNoteToFile()
        {
            var service = new NoteService();
            await service.AddNoteAsync("test note");

            var notes = await service.GetAllNotesAsync();
            Assert.IsTrue(notes.Any(n => n.Content == "test note"));
        }

        [Test]
        public async Task SearchNotesAsync_FindsExistingNote()
        {
            var service = new NoteService();
            await service.AddNoteAsync("hello world");

            var result = await service.SearchNotesAsync("hello");
            Assert.AreEqual(1, result.Count);
        }
    }
}
