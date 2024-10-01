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


        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine($"@{me.Username} начал работу в {DateTime.Now}\n");
        Console.ResetColor();
        Console.ReadLine();
        cts.Cancel(); // stop the bot

        async Task OnMessage(Message msg, UpdateType type)
        {
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
                            if (msg.Text.ToLower() == "/start")
                            {
                                await bot.SendTextMessageAsync(msg.Chat, "Привет!\nЯ напишу тебе заявление.\nДля начала отправь мне фото твоей росписи\nПример:", replyMarkup: new ReplyKeyboardRemove());
                                FileStream fileStream = System.IO.File.Open((@"роспись.png"), FileMode.Open);
                                await bot.SendPhotoAsync(
                                        chatId: msg.Chat.Id,
                                        photo: InputFile.FromStream(fileStream),
                                        cancellationToken: cts.Token);
    }
                            else
                            {
                                await bot.SendTextMessageAsync(msg.Chat.Id, "Неизвестная команда. Используйте /start для начала.");
                            }
                            break;
                        }


                        case 1:
                        {
                            using (StreamWriter sr = new StreamWriter($"Данные/{msg.Chat.Username.ToString()}.txt", false))
                            {
                                await sr.WriteLineAsync(msg.Text);
                            }
                            userState.FullName = msg.Text;
                            userState.CurrentStep = 2;
                            await bot.SendTextMessageAsync(msg.Chat.Id, "Пожалуйста, введите номер группы:");
                            break;
                        }


                        case 2:
                        {
                            using (StreamWriter sr = new StreamWriter($"Данные/{msg.Chat.Username.ToString()}.txt", true))
                            {
                                await sr.WriteLineAsync(msg.Text);
                            }
                            userState.GroupNumber = msg.Text;
                            userState.CurrentStep = 3;
                            await bot.SendTextMessageAsync(msg.Chat.Id, "Пожалуйста, введите причину:");
                            break;
                        }


                        case 3:
                        {
                            using (StreamWriter sr = new StreamWriter($"Данные/{msg.Chat.Username.ToString()}.txt", true))
                            {
                                await sr.WriteLineAsync(msg.Text);
                            }
                            userState.Reason = msg.Text;
                            userState.CurrentStep = 4;
                            await bot.SendTextMessageAsync(msg.Chat.Id, "Пожалуйста, введите дату:");
                            break;
                        }


                        case 4:
                        {
                            try
                            {
                                userState.Date = msg.Text;
                                userState.CurrentStep = 0;

                                await bot.SendPhotoAsync(
                                chatId: msg.Chat.Id,
                                photo: InputFile.FromStream(System.IO.File.Open(DrawwingFile.DrawwingFile1(msg.Chat.Username.ToString(), msg.Text), FileMode.Open)),
                                cancellationToken: cts.Token);

                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"Заявление отправлено для @{msg.Chat.Username.ToString()}\n");
                                Console.ResetColor();

                                await bot.SendTextMessageAsync(msg.Chat, "Готово!\nТеперь отправляй своему куратору и кайфуй");
                                await bot.SendTextMessageAsync(msg.Chat, "Чтобы начать работать с ботом отправьте /start",
                                    replyMarkup: new ReplyKeyboardMarkup(true).AddButtons("/start"));
                            }
                            catch(Exception ex)
                            {
                                Console.WriteLine(ex);
                            }

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
        /*
        // method that handle messages received by the bot:
        async Task OnMessage(Message msg, UpdateType type)
        {
            if(!System.IO.File.Exists($"Данные/{msg.Chat.Username.ToString()}.txt"))
            {
                System.IO.File.Create($"Данные/{msg.Chat.Username.ToString()}.txt").Close();
            }



            if (msg.Type.ToString() == "Photo")
            {
                try
                {


                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                await bot.SendTextMessageAsync(msg.Chat, "Фотку получил!\nТеперь отправь свое ФИО\nПример:");
                await bot.SendTextMessageAsync(msg.Chat, "1 Булдаков Владислав Александрович");//---------------------------------------------- 1

            }
            else if(msg.Type.ToString() == "Text")
            {
                var msgcheck = msg.Text.Split(' ');
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"Сообщение '{msg.Text}'\nОт ({msg.Chat.Id}) @{msg.Chat.Username.ToString()}\nВремя отправки: {DateTime.Now}\n");
                Console.ResetColor();
                switch (msgcheck[0])
                {
                    case "/start":
                        await bot.SendTextMessageAsync(msg.Chat, "Привет\nЯ напишу тебе заявление.\nЧтобы продолжить нажми на кнопку",
                            replyMarkup: new ReplyKeyboardMarkup(true)
                            .AddButtons("Заявление"));
                        break;
                    case "Заявление":
                        await bot.SendTextMessageAsync(msg.Chat, "Мне нужна будет твоя информация!\nДля начала отправь мне фото твоей росписи!");
                        await bot.SendTextMessageAsync(msg.Chat, "Пример", replyMarkup: new ReplyKeyboardRemove());
                        FileStream fileStream = System.IO.File.Open((@"роспись.png"), FileMode.Open);
                        await bot.SendPhotoAsync(
                            msg.Chat.Id: msg.Chat.Id,
                            photo: InputFile.FromStream(fileStream),
                            cancellationToken: cts.Token);

                        break;
                    case "1":
                        
                        using (StreamWriter sr = new StreamWriter($"Данные/{msg.Chat.Username.ToString()}.txt", false))
                        {
                            await sr.WriteLineAsync(msg.Text.Substring(2));
                        }
                        await bot.SendTextMessageAsync(msg.Chat, "Теперь отправь свою группу\nПример:");
                        await bot.SendTextMessageAsync(msg.Chat, "2 3-ИСП9-34");
                        break;
                    case "2":
                        using (StreamWriter sr = new StreamWriter($"Данные/{msg.Chat.Username.ToString()}.txt", true))
                        {
                            await sr.WriteLineAsync(msg.Text.Substring(2));
                        }
                        await bot.SendTextMessageAsync(msg.Chat, "Теперь отправь причину отсутствия\nПример:");
                        await bot.SendTextMessageAsync(msg.Chat, "3 Семейные обстоятельства");
                        break;
                    case "3":
                        using (StreamWriter sr = new StreamWriter($"Данные/{msg.Chat.Username.ToString()}.txt", true))
                        {
                            await sr.WriteLineAsync(msg.Text.Substring(2));
                        }
                        await bot.SendTextMessageAsync(msg.Chat, "Теперь отправь дату отсутствия\nПример:");
                        await bot.SendTextMessageAsync(msg.Chat, "4 01.05.2013");
                        break;
                    case "4":
                        await bot.SendPhotoAsync(
                        msg.Chat.Id: msg.Chat.Id,
                        photo: InputFile.FromStream(System.IO.File.Open(DrawwingFile.DrawwingFile1(msg.Chat.Username.ToString(), msg.Text.Substring(2)), FileMode.Open)),
                        cancellationToken: cts.Token);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Заявление отправлено для @{msg.Chat.Username.ToString()}\n");
                        Console.ResetColor();
                        await bot.SendTextMessageAsync(msg.Chat, "Готово!\nТеперь отправляй своему куратору и кайфуй");
                        await bot.SendTextMessageAsync(msg.Chat, "Чтобы начать работать с ботом отправьте /start",
                            replyMarkup: new ReplyKeyboardMarkup(true).AddButtons("/start"));
                        break;
                    default:
                            await bot.SendTextMessageAsync(msg.Chat, "Напиши как в примере плиз");
                        
                        break;
                }

            }
            else
            {
                await bot.SendTextMessageAsync(msg.Chat, "Я принимаю только фото и текст");
            }

            // comit
        }
        */
    }
}