using System;
using System.Linq;
using NetStoneDiscordBot.Services;
using NUnit.Framework;

namespace NetStoneMCP.Tests.Services
{
    public class RoleplayServiceTests
    {
        [Test]
        public void RoleplayService_LoadsFFXIVCharacters()
        {
            var service = new RoleplayService();

            var available = service.AvailableCharacters;
            Assert.IsNotNull(available);
            Assert.Contains("雅修特拉", available.ToList());
            Assert.Contains("愛梅特賽爾克", available.ToList());
            Assert.Contains("古拉哈提亞", available.ToList());
            Assert.Contains("塔塔露", available.ToList());
            Assert.Contains("阿莉塞", available.ToList());
            Assert.Contains("艾斯蒂尼安", available.ToList());
        }

        [Test]
        public void SwitchCharacter_WithExactName_SwitchesSuccessfully()
        {
            var service = new RoleplayService();

            var success = service.SwitchCharacter("塔塔露", out var msg);

            Assert.IsTrue(success);
            Assert.AreEqual("塔塔露", service.CurrentCharacter);
            Assert.IsTrue(service.IsEnabled);
            StringAssert.Contains("塔塔露", msg);

            var prompt = service.GetSystemPrompt();
            Assert.IsNotNull(prompt);
            StringAssert.Contains("是也", prompt);
        }

        [Test]
        public void SwitchCharacter_WithAlias_ResolvesToCanonicalCharacter()
        {
            var service = new RoleplayService();

            // "貓娘" -> "雅修特拉"
            var s1 = service.SwitchCharacter("貓娘", out var msg1);
            Assert.IsTrue(s1);
            Assert.AreEqual("雅修特拉", service.CurrentCharacter);

            // "愛梅" -> "愛梅特賽爾克"
            var s2 = service.SwitchCharacter("愛梅", out var msg2);
            Assert.IsTrue(s2);
            Assert.AreEqual("愛梅特賽爾克", service.CurrentCharacter);

            // "水晶公" -> "古拉哈提亞"
            var s3 = service.SwitchCharacter("水晶公", out var msg3);
            Assert.IsTrue(s3);
            Assert.AreEqual("古拉哈提亞", service.CurrentCharacter);

            // "大師兄" -> "艾斯蒂尼安"
            var s4 = service.SwitchCharacter("大師兄", out var msg4);
            Assert.IsTrue(s4);
            Assert.AreEqual("艾斯蒂尼安", service.CurrentCharacter);
        }

        [Test]
        public void BuildForgetRemind_ReturnsCharacterSpecificRemind()
        {
            var service = new RoleplayService();
            service.Enable("愛梅特賽爾克");

            var deadline = DateTime.UtcNow.AddMinutes(5);
            var remind = service.BuildForgetRemind(deadline);

            StringAssert.Contains("曾經活過", remind);
        }

        [Test]
        public void BuildMemoryExtendedMessage_ReturnsCharacterSpecificMessage()
        {
            var service = new RoleplayService();
            service.Enable("塔塔露");

            var msg = service.BuildMemoryExtendedMessage(TimeSpan.FromMinutes(10));

            StringAssert.Contains("是也", msg);
            StringAssert.Contains("10", msg);
        }
    }
}
