using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

namespace TelegramBot
{
    struct BotUpdate
    {
        public string text;
        public long id;
        public string? username;
    }

    class Program
    {
        static readonly TelegramBotClient bot = new("6431958679:AAEhfEKmMGETwfaGU7gB9zJLk89zVYPCMBs");

        static readonly string fileName = "updates.json";
        static List<BotUpdate> botUpdates = new();
        static void Main(string[] args)
        {
            try
            {
                var botUpdatesString = System.IO.File.ReadAllText(fileName);

                botUpdates = JsonConvert.DeserializeObject<List<BotUpdate>>(botUpdatesString) ??
                    botUpdates;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading or deserializing {ex}");
            }
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new Telegram.Bot.Types.Enums.UpdateType[]
                {
                    Telegram.Bot.Types.Enums.UpdateType.Message,
                    Telegram.Bot.Types.Enums.UpdateType.EditedMessage,
                }
            };

            bot.StartReceiving(UpdateHandler, ErrorHandler, receiverOptions);

            Console.ReadLine();
        }

        private static Task ErrorHandler(ITelegramBotClient client, Exception exception, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        private static async Task UpdateHandler(ITelegramBotClient client, Telegram.Bot.Types.Update update, CancellationToken token)
        {
            if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
            {
                if (update.Message.Type == MessageType.Text)
                {
                    var _botUpdate = new BotUpdate
                    {
                        text = update.Message.Text,
                        id = update.Message.Chat.Id,
                        username = update.Message.Chat.Username
                    };

                    botUpdates.Add(_botUpdate);

                    var botUpdatesString = JsonConvert.SerializeObject(botUpdates);

                    System.IO.File.WriteAllText(fileName, botUpdatesString);
                }

                if (update.Message.Document != null)
                {
                    var fileId = update.Message.Document.FileId;
                    var fileInfo = await client.GetFileAsync(fileId, cancellationToken: token);
                    var filePath = fileInfo.FilePath;

                    string destinationFilePath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\{update.Message.Document.FileName}";
                    await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                    await client.DownloadFileAsync(filePath, fileStream, token);
                    fileStream.Close();
                }
            }
        }
    }
}