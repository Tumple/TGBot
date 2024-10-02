using Telegram;
using System.Drawing.Imaging;
using System.Drawing;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TumpleTestTGBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using static System.Net.WebRequestMethods;
using System.Threading;
using Telegram.Bot.Types.ReplyMarkups;
using System.IO.Pipes;
using Microsoft.Extensions.DependencyInjection;

internal class Program //test commit
{
    private static readonly Dictionary<long, UserState> UserStates = new Dictionary<long, UserState>();
    private static async Task Main(string[] args)
    {
        DrawwingFile file = new DrawwingFile();
        using var cts = new CancellationTokenSource();


        Console.WriteLine("Введите токен бота");
        var bot = new TelegramBotClient(Console.ReadLine(), cancellationToken: cts.Token);
        Console.Clear();


        var me = await bot.GetMeAsync();
        bot.OnMessage += OnMessage;
        bot.OnUpdate += OnUpdate;
        


        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine($"@{me.Username} начал работу в {DateTime.Now}\n");
        Console.ResetColor();
        Console.ReadLine();
        cts.Cancel(); // stop the bot

        async Task OnMessage(Message msg, UpdateType type)
        {
            Update update = new Update();
            if (!UserStates.ContainsKey(msg.Chat.Id))
            {
                UserStates[msg.Chat.Id] = new UserState { UserId = msg.Chat.Id, CurrentStep = 0 };
            }

            var userState = UserStates[msg.Chat.Id];

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"({msg.Chat.Id}) @{msg.Chat.Username.ToString()}\nСообщение '{msg.Text}'\n{DateTime.Now}\n");
            Console.ResetColor();
        

        switch (msg.Type.ToString())
            {
                case "Text":
                {

                        switch (userState.CurrentStep)
                    {
                        case 0:
                        {
                            if (msg.Text == "/start")
                            {
                                await bot.SendTextMessageAsync(msg.Chat, "Привет!\nЯ напишу тебе заявление.\nДля начала отправь мне фото твоей росписи\nПример:", replyMarkup: new ReplyKeyboardRemove());
                                FileStream fileStream = System.IO.File.Open((@"роспись.png"), FileMode.Open);
                                await bot.SendPhotoAsync(
                                        chatId: msg.Chat.Id,
                                        photo: InputFile.FromStream(fileStream),
                                        cancellationToken: cts.Token);
                            }
                            else if (msg.Text == "/edit")
                            {
                                userState.CurrentStep = 1;
                                await bot.SendTextMessageAsync(msg.Chat, "Отправь свое ФИО", replyMarkup: new ReplyKeyboardRemove());
                                break;
                            } 
                            else if(msg.Text.ToLower() == "создать заявление")
                            { 
                                try
                                {
                                    userState.Date = msg.Text;
                                    userState.CurrentStep = 0;

                                    await bot.SendPhotoAsync(
                                    chatId: msg.Chat.Id,
                                    photo: InputFile.FromStream(System.IO.File.Open(DrawwingFile.DrawwingFile1(msg.Chat.Username.ToString()), FileMode.Open)),
                                    cancellationToken: cts.Token);

                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine($"Заявление отправлено для @{msg.Chat.Username.ToString()}\n");
                                    Console.ResetColor();

                                    await bot.SendTextMessageAsync(msg.Chat, "Готово!\nТеперь отправляй своему куратору и кайфуй");
                                    await bot.SendTextMessageAsync(msg.Chat, "Чтобы начать работать с ботом отправьте /start",
                                        replyMarkup: new ReplyKeyboardMarkup(true).AddButtons("/start"));
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex);
                                }
                                break;
                            }
                            else
                            {
                                await bot.SendTextMessageAsync(msg.Chat.Id, "Неизвестная команда. Используйте /start для начала.");
                            }
                            break;
                        }
                        case 1:
                        {
                            userState.FullName = msg.Text;
                            userState.CurrentStep = 2;
                            await bot.SendTextMessageAsync(msg.Chat.Id, "Пожалуйста, введите номер группы:");
                            break;
                        }


                        case 2:
                        {
                            userState.GroupNumber = msg.Text;
                            userState.CurrentStep = 3;
                            await bot.SendTextMessageAsync(msg.Chat.Id, "Пожалуйста, введите причину:");
                            break;
                        }


                        case 3:
                        {
                            userState.Reason = msg.Text;
                            userState.CurrentStep = 4;
                            await bot.SendTextMessageAsync(msg.Chat.Id, "Пожалуйста, введите дату:");
                            break;
                        }


                        case 4:
                        {
                                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
{                                   
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData("ФИО", "ФИО"),
                                            InlineKeyboardButton.WithCallbackData("Группа", "Группа"),

                                            InlineKeyboardButton.WithCallbackData("Причина", "Причина"),
                                            InlineKeyboardButton.WithCallbackData("Дата", "Дата"),

                                            InlineKeyboardButton.WithCallbackData("создать заявление", "Заявление")
                                        }
                                    });
                            using (StreamWriter sr = new StreamWriter($"Данные/{msg.Chat.Username.ToString()}.txt", false))
                            {
                                await sr.WriteLineAsync(userState.FullName);
                                await sr.WriteLineAsync(userState.GroupNumber);
                                await sr.WriteLineAsync(userState.Reason);
                                await sr.WriteLineAsync(userState.Date);
                            }
                            userState.Date = msg.Text;
                            await bot.SendTextMessageAsync(msg.Chat.Id, $"Проверь свои данные:\nФИО: {userState.FullName}\nНомер группы: {userState.GroupNumber}\nПричина: {userState.Reason}\nДата: {userState.Date}\nЕсли нужно исправить нажми /edit",
                                replyMarkup: inlineKeyboard);
                            userState.CurrentStep = 0;
                            break;
                        }
                    }
                break;
                }

                case "Photo":
                    {
                        userState.CurrentStep = 1;
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine($"Получено фото\nОт ({msg.Chat.Id}) @{msg.Chat.Username.ToString()}\nВремя отправки: {DateTime.Now}\n");
                        Console.ResetColor();

                        var fileInfo = await bot.GetFileAsync(msg.Photo.Last().FileId);
                        var filePath = fileInfo.FilePath;
                        await using Stream fileStream = System.IO.File.Create(@$"Росписи/{msg.Chat.Username.ToString()}.png");
                        await bot.DownloadFileAsync(filePath, fileStream, cancellationToken: cts.Token);
                        fileStream.Close();

                        await bot.SendTextMessageAsync(msg.Chat, "Фотку получил!\nТеперь отправь свое ФИО");

                        break;
                    }
                default:
                {
                    await bot.SendTextMessageAsync(msg.Chat, "Я понимаю только текст и фото");
                    return;
                }
            }
        }

        async Task OnUpdate(Update update)
        {
            if(update.CallbackQuery != null)
            {
                switch (update.CallbackQuery.Data)
                {
                    case "ФИО":
                        Console.WriteLine("ФИО");
                        break;
                    case "Группа":
                        Console.WriteLine("Группа");
                        break;
                    case "Причина":
                        Console.WriteLine("Причина");
                        break;
                    case "Дата":
                        Console.WriteLine("Дата");
                        break;     
                    case "Заявление":
                        Console.WriteLine("Заявление");
                        break;
                }
            }
            else
            {
                Console.WriteLine("Анлак");
            }

        }
        async Task CreateFile()
        {

        }
    }
}