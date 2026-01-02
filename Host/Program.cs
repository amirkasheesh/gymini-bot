using System;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Microsoft.Extensions.Configuration;
using Telegram.Bot.Types.ReplyMarkups;
using Entities;

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
            if (update.CallbackQuery != null)
            {
                await HandleCallback(client, update.CallbackQuery, token);
                return;
            }

            if (update.Message != null)
            {
                await HandleMessage(client, update.Message, token);
                return;
            }
        }

        private static async Task HandleMessage(ITelegramBotClient client, Message message, CancellationToken token)
        {
            if (message.Text == null)
            {
                Console.WriteLine("Пришло что-то, но не текст!");
                return;
            }
            Console.WriteLine($"Сообщение: {message.Text}");

            switch(message.Text)
            {
                case "/start":
                    var keyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData("Силовая", "style_power"),
                            InlineKeyboardButton.WithCallbackData("Кардио", "style_cardio"),
                        }
                    });

                    await client.SendMessage(
                        chatId: message.Chat.Id,
                        text: "Привет! Я GYMini. Выбери стиль тренировки:",
                        replyMarkup: keyboard,
                        cancellationToken: token
                    );
                    break;
                case "/help":
                   await client.SendMessage(
                        chatId: message.Chat.Id,
                        text: "Я пока MVPшка', но скоро научусь составлять программы тренировок :)",
                        cancellationToken: token
                    );
                    break;
                default:
                   await client.SendMessage(
                        chatId: message.Chat.Id,
                        text: "Я не понимаю эту команду!",
                        cancellationToken: token
                    );
                    break; 
            }
        }

        private static async Task HandleCallback(ITelegramBotClient client, CallbackQuery callback, CancellationToken token)
        {
            System.Console.WriteLine($"Нажата кнопка: {callback.Data}");

            // Важно! Чтобы убрать ожидание у бота
            await client.AnswerCallbackQuery(callback.Id, cancellationToken: token);
            
            if (callback.Data == "back_to_start")
            {
                var startKeyboard = new InlineKeyboardMarkup(new[]
                {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Силовая", "style_power"),
                        InlineKeyboardButton.WithCallbackData("Кардио", "style_cardio"),
                    }
                });

                await client.EditMessageText(
                    chatId: callback.Message.Chat.Id,
                    messageId: callback.Message.MessageId,
                    text: "Привет! Я GYMini. Выбери стиль тренировки:",
                    replyMarkup: startKeyboard,
                    cancellationToken: token
                );
                return;
            }

            if (callback.Data == "style_power" || callback.Data == "style_cardio")
            {
                string text = "";
                InlineKeyboardMarkup keyboard = null;

                if (callback.Data == "style_power")
                {
                    text = "Силовая тренировка! Выбери упражнение:";
                    keyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[] { InlineKeyboardButton.WithCallbackData("0. Назад", "back_to_start") },
                        new[] { InlineKeyboardButton.WithCallbackData("1. Жим лежа", "ex_bench") },
                        new[] { InlineKeyboardButton.WithCallbackData("2. Тяга блока", "ex_thrust") }
                    });
                }
                else if (callback.Data == "style_cardio")
                {
                    text = "Кардио тренировка! Выбери упражнение:";
                    keyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[] { InlineKeyboardButton.WithCallbackData("0. Назад", "back_to_start") },
                        new[] { InlineKeyboardButton.WithCallbackData("1. Дорожка", "ex_treadmill") },
                        new[] { InlineKeyboardButton.WithCallbackData("2. Пресс", "ex_press") }
                    });
                }

                await client.EditMessageText(
                    chatId: callback.Message.Chat.Id,
                    messageId: callback.Message.MessageId,
                    text: text,
                    replyMarkup: keyboard,
                    cancellationToken: token
                );
            }
        }
    }
}