using System;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Microsoft.Extensions.Configuration;

namespace Host
{
    internal class Program
    {
        static void Main()
        {
            IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
            var token = config["BotToken"];
            var botClient = new TelegramBotClient(token ?? "None");
            using var cts = new CancellationTokenSource();

            botClient.StartReceiving(
                updateHandler: FuncUpdate,
                errorHandler: FuncError,
                receiverOptions: new ReceiverOptions(),
                cancellationToken: cts.Token
            );

            Console.ReadLine();
            cts.Cancel();
        }

        private static async Task FuncError(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
        {
            Console.WriteLine($"Ошибка: {exception.Message}");
        }

        private static async Task FuncUpdate(ITelegramBotClient client, Update update, CancellationToken token)
        {
            if (update.Message?.Text == null)
            {
                Console.WriteLine("Пришло что-то, но не текст!");
                return;
            }
            Console.WriteLine($"Сообщение: {update.Message?.Text}");

            await client.SendMessage(
                chatId: update.Message.Chat.Id,
                text: "Я тебя услышал: " + update.Message.Text,
                cancellationToken: token
            );
        }
    }
}