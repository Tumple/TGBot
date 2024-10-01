using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Data;
using Telegram.Bot.Types;


namespace TumpleTestTGBot
{
    class DrawwingFile
    {
        public static int Random()
        {
            Random random = new Random();
            int randomNumber = random.Next(1, 14);
            return randomNumber;
        }
        public static string  DrawwingFile1(string username, string date) 
        {
            string fullname, group, reason;
            string[] fullname1;
            string filename = $"Результаты/{username}+{DateTime.Now.ToShortDateString()}.png";
            try
            {
                using (StreamReader sr = new StreamReader($"Данные/{username}.txt"))
                {
                    fullname = sr.ReadLine();
                    group = sr.ReadLine();
                    reason = sr.ReadLine();
                    fullname1 = fullname.Split(' ');
                }


                // Размеры A4 в пикселях при разрешении 300 DPI
                int width = 2480;
                int height = 3000;
                // Загрузить 

                System.Drawing.Text.PrivateFontCollection privateFontCollection = new System.Drawing.Text.PrivateFontCollection();
                try
                {
                    privateFontCollection.AddFontFile(@$"Шрифты/{Random()}.ttf");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                
                // Создаем объект Font, взяв из коллекции 0-й шрифт
                System.Drawing.Font customFont = new System.Drawing.Font(privateFontCollection.Families[0], 65, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
                // Создание пустого Bitmap
                using (Bitmap bitmap = new Bitmap(width, height))
                {
                    // Создание объекта Graphics для рисования на Bitmap
                    using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap))
                    {
                        // Заливка фона белым цветом
                        graphics.Clear(System.Drawing.Color.White);


                        System.Drawing.Brush brush = Brushes.DarkBlue;

                        // Текст для записи
                        string lines =
                                    $"Директору ГБПОУ\nКолледжа связи 54\nПавлюку И. А.\nот студента гр.{group}\n{fullname1[0]} {fullname1[1][0]}. {fullname1[2][0]}.";

                        string lines2 =
                                    $"      Я, {fullname}, студент группы {group} прошу Вас разрешить мне отсутствовать {date} на учебных занятиях по причине: {reason} \nОтветственность за изучение пропущенного материала беру на себя.";

                        graphics.DrawString(lines, customFont, brush, 1400, 100);
                        graphics.DrawString("Заявление", customFont, brush, 1200, 1000);
                        DrawStringWithWordWrap(graphics, lines2, customFont, brush, new RectangleF(100, 1100, width - 200, height - 1200));
                        
                        //graphics.DrawString(lines2, customFont, brush, 100, 1100);
                        using (Image image = Image.FromFile(@$"Росписи/{username}.png"))
                        {
                            graphics.DrawImage(image, 1500, 1800);
                        }
                        graphics.DrawString(DateTime.Today.ToShortDateString(), customFont, brush, 100, 2000);
                        bitmap.Save(filename, ImageFormat.Png);

                        graphics.Dispose();
                    }
                    bitmap.Dispose();
                }
            }
            catch (Exception ex){ Console.WriteLine(ex); }
            return filename;
        }
        public static void DrawStringWithWordWrap(Graphics graphics, string text, Font font, Brush brush, RectangleF layoutRectangle)
        {
            StringFormat format = new StringFormat();
            format.Alignment = StringAlignment.Near;
            format.LineAlignment = StringAlignment.Near;
            format.Trimming = StringTrimming.Word;

            graphics.DrawString(text, font, brush, layoutRectangle, format);
        }
    }

}

