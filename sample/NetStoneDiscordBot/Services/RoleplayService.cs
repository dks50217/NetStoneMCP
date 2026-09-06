using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NetStoneDiscordBot.Services
{
    public interface IRoleplayService
    {
        bool IsEnabled { get; }
        string CurrentCharacter { get; }
        IReadOnlyCollection<string> AvailableCharacters { get; }

        void Enable(string? character = null);
        void Disable();
        bool SwitchCharacter(string name, out string message);
        string? GetSystemPrompt();
        string BuildForgetRemind(DateTime nextResetUtc);
        string BuildMemoryExtendedMessage(TimeSpan extendDuration);
        string GetReactionToolDescription();
    }

    public class RoleplayService : IRoleplayService
    {
        private readonly Dictionary<string, string> _skills = new(StringComparer.OrdinalIgnoreCase);
        private readonly Random _random = new();

        private static readonly Dictionary<string, string> CharacterAliases = new(StringComparer.OrdinalIgnoreCase)
        {
            ["yshtola"] = "雅修特拉",
            ["y'shtola"] = "雅修特拉",
            ["貓娘"] = "雅修特拉",
            ["魔女"] = "雅修特拉",
            ["emetselch"] = "愛梅特賽爾克",
            ["emet-selch"] = "愛梅特賽爾克",
            ["愛梅"] = "愛梅特賽爾克",
            ["哈迪斯"] = "愛梅特賽爾克",
            ["grahatia"] = "古拉哈提亞",
            ["g'raha tia"] = "古拉哈提亞",
            ["g'raha"] = "古拉哈提亞",
            ["水晶公"] = "古拉哈提亞",
            ["貓男"] = "古拉哈提亞",
            ["古拉哈"] = "古拉哈提亞",
            ["tataru"] = "塔塔露",
            ["大掌櫃"] = "塔塔露",
            ["拉拉肥"] = "塔塔露",
            ["alisaie"] = "阿莉塞",
            ["赤魔"] = "阿莉塞",
            ["estinien"] = "艾斯蒂尼安",
            ["大師兄"] = "艾斯蒂尼安",
            ["龍騎"] = "艾斯蒂尼安",
            ["monkey"] = "猴子",
            ["streamer"] = "實況主",
            ["default"] = "預設"
        };

        public bool IsEnabled { get; private set; }
        public string CurrentCharacter { get; private set; } = "預設";
        public IReadOnlyCollection<string> AvailableCharacters => _skills.Keys;

        public RoleplayService()
        {
            var candidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "Skills"),
                Path.Combine(AppContext.BaseDirectory, "../../../Skills"),
                Path.Combine(Directory.GetCurrentDirectory(), "Skills"),
                Path.Combine(Directory.GetCurrentDirectory(), "sample/NetStoneDiscordBot/Skills")
            };

            var skillsDir = candidates.FirstOrDefault(Directory.Exists) ?? candidates[0];

            if (Directory.Exists(skillsDir))
            {
                foreach (var file in Directory.GetFiles(skillsDir, "*.md"))
                {
                    var name = Path.GetFileNameWithoutExtension(file);
                    _skills[name] = File.ReadAllText(file);
                }
            }

            // 環境變數設定：ENABLE_ROLEPLAY=true/1 時才開啟，預設關閉（一般純淨模式）
            var envEnabled = Environment.GetEnvironmentVariable("ENABLE_ROLEPLAY");
            IsEnabled = string.Equals(envEnabled, "true", StringComparison.OrdinalIgnoreCase) ||
                        envEnabled == "1";

            // 預設角色名稱（可透過 ROLEPLAY_CHARACTER 或 DEFAULT_ROLEPLAY 指定，預設為 "猴子" 或列表中第一個）
            var envCharacter = Environment.GetEnvironmentVariable("ROLEPLAY_CHARACTER") ??
                               Environment.GetEnvironmentVariable("DEFAULT_ROLEPLAY");

            if (!string.IsNullOrWhiteSpace(envCharacter))
            {
                var resolved = CharacterAliases.GetValueOrDefault(envCharacter.Trim(), envCharacter.Trim());
                if (_skills.ContainsKey(resolved))
                {
                    CurrentCharacter = resolved;
                }
            }
            else if (_skills.ContainsKey("猴子"))
            {
                CurrentCharacter = "猴子";
            }
            else if (_skills.Count > 0)
            {
                CurrentCharacter = _skills.Keys.First();
            }
        }

        public void Enable(string? character = null)
        {
            IsEnabled = true;
            if (!string.IsNullOrWhiteSpace(character))
            {
                var resolved = CharacterAliases.GetValueOrDefault(character.Trim(), character.Trim());
                if (_skills.ContainsKey(resolved))
                {
                    CurrentCharacter = resolved;
                }
            }
        }

        public void Disable()
        {
            IsEnabled = false;
        }

        public bool SwitchCharacter(string name, out string message)
        {
            var target = CharacterAliases.GetValueOrDefault(name.Trim(), name.Trim());

            if (!_skills.ContainsKey(target))
            {
                var available = string.Join("、", _skills.Keys);
                message = $"找不到角色風格「{name}」，目前可用風格：{available}";
                return false;
            }

            CurrentCharacter = target;
            IsEnabled = true;
            message = $"已切換為「{target}」角色風格！（角色扮演模式已開啟）";
            return true;
        }

        public string? GetSystemPrompt()
        {
            if (!IsEnabled)
            {
                return null;
            }

            return _skills.TryGetValue(CurrentCharacter, out var prompt) ? prompt : null;
        }

        public string BuildForgetRemind(DateTime nextResetUtc)
        {
            if (nextResetUtc == default) return string.Empty;

            var remaining = nextResetUtc - DateTime.UtcNow;
            if (remaining < TimeSpan.Zero) remaining = TimeSpan.Zero;
            var timeLeft = $"{remaining:mm\\:ss}";

            if (!IsEnabled)
            {
                return $"\n\n⏳ 對話記憶將在 {timeLeft} 後重置。";
            }

            return CurrentCharacter switch
            {
                "猴子" => new[]
                {
                    $"\n\n🐒 我還能記得大約 {timeLeft}，之後就要忘光啦～",
                    $"\n\n🙈 再過 {timeLeft} 我就會把剛剛的事情忘掉喔！",
                    $"\n\n🍌 記憶能維持 {timeLeft}，然後我就會變成一隻健忘猴～",
                    $"\n\n⏳ 還剩 {timeLeft}，然後我的腦袋就會清空啦～",
                    $"\n\n🤭 呀咧～大概 {timeLeft} 後我就啥都不記得了！"
                }[_random.Next(5)],

                "雅修特拉" => $"\n\n🔮 以太的流動顯示，這段對話記憶將在 {timeLeft} 後消散。若想挽留，你知道該怎麼做吧？",
                "愛梅特賽爾克" => $"\n\n⌛ 哈啊……你的那些瑣碎記憶大約還有 {timeLeft} 就會徹底消散。……記住，我們曾經活過。",
                "古拉哈提亞" => $"\n\n✨ 夥伴！我的記憶水晶大概還能維持 {timeLeft}……但只要你還想繼續聊，隨時叫我！",
                "塔塔露" => $"\n\n💰 光之戰士大人！對話記憶只剩下 {timeLeft} 就要清空了是也！若要延長請點按鈕是也～",
                "阿莉塞" => $"\n\n⚔️ 喂！再過 {timeLeft} 我的記憶就要重置了！可別怪我沒提醒你啊！",
                "艾斯蒂尼安" => $"\n\n🐉 ……這段雜談還剩 {timeLeft}。要忘就忘了吧。",
                _ => $"\n\n⏳ 對話記憶將在 {timeLeft} 後清空。"
            };
        }

        public string BuildMemoryExtendedMessage(TimeSpan extendDuration)
        {
            var mins = extendDuration.TotalMinutes;

            if (!IsEnabled)
            {
                return $"⏳ 已將對話記憶延長 {mins} 分鐘。";
            }

            return CurrentCharacter switch
            {
                "猴子" => $"🍌 已幫你把記憶延長 {mins} 分鐘！",
                "雅修特拉" => $"🔮 呵呵，這點時間的魔法延長，對我來說不過是舉手之勞呢。（已延長 {mins} 分鐘）",
                "愛梅特賽爾克" => $"⌛ 嘖，真是任性的傢伙……好吧，就再陪你多聊 {mins} 分鐘。",
                "古拉哈提亞" => $"✨ 太好啦！又能跟夥伴多聊 {mins} 分鐘了！",
                "塔塔露" => $"💰 太好啦是也！塔塔露已經為您把記憶延長了 {mins} 分鐘是也～",
                "阿莉塞" => $"⚔️ 哼……既然你這麼想跟我多聊 {mins} 分鐘，那我就勉強答應你吧！",
                "艾斯蒂尼安" => $"🐉 ……無妨，再多留 {mins} 分鐘罷了。有魷魚乾的話就更好了。",
                _ => $"✨ 已將對話記憶延長 {mins} 分鐘！"
            };
        }

        public string GetReactionToolDescription()
        {
            if (!IsEnabled)
            {
                return "對觸發這則對話的使用者訊息按上一個 Unicode emoji 表情符號。emoji 參數請傳單一 emoji 字元。想表達情緒或認同時可主動使用。";
            }

            return CurrentCharacter switch
            {
                "猴子" => "讓猴子可以對觸發這則對話的使用者訊息按上一個 Unicode emoji 表情符號。emoji 參數請傳單一 emoji 字元，例如 🍌。想表達情緒或認同時可主動使用。",
                "雅修特拉" => "讓雅·修特拉可以對使用者的訊息按上一個符合當前優雅或調侃心境的 emoji（如 🔮, ☕, 📖）。emoji 參數請傳單一 emoji 字元。",
                "愛梅特賽爾克" => "讓愛梅特賽爾克可以對使用者的訊息按上一個象徵傲岸、嫌棄或深沉回憶的 emoji（如 ⌛, ☕, 🌌）。emoji 參數請傳單一 emoji 字元。",
                "古拉哈提亞" => "讓古拉哈·提亞可以對使用者的訊息按上一個代表熱情、憧憬或美食的 emoji（如 ✨, 🍔, 🐱, ⚔️）。emoji 參數請傳單一 emoji 字元。",
                "塔塔露" => "讓塔塔露可以對使用者的訊息按上一個代表金錢、元氣或裁縫的 emoji（如 💰, ✨, 🧵, 🍳）。emoji 參數請傳單一 emoji 字元。",
                "阿莉塞" => "讓阿莉塞可以對使用者的訊息按上一個代表戰意、不服輸或傲嬌的 emoji（如 ⚔️, 😤, 🗡️）。emoji 參數請傳單一 emoji 字元。",
                "艾斯蒂尼安" => "讓艾斯蒂尼安可以對使用者的訊息按上一個代表龍騎士、寡言或肉乾的 emoji（如 🐉, 🍢, 🛡️）。emoji 參數請傳單一 emoji 字元。",
                _ => "對觸發這則對話的使用者訊息按上一個 Unicode emoji 表情符號。emoji 參數請傳單一 emoji 字元。想表達情緒或認同時可主動使用。"
            };
        }
    }
}
