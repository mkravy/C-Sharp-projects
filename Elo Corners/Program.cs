using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.IO;
using System.Runtime.InteropServices;

namespace Elo_Corners
{
    class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE flags);

        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_SYSTEM_REQUIRED = 0x00000001,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_CONTINUOUS = 0x80000000
        }

        static string league(IWebDriver driver)
        {
            var country = driver.FindElement(By.XPath("//*[@class='event__titleBox']/span[1]"));
            var division = driver.FindElement(By.XPath("//*[@class='event__titleBox']/span[2]"));
            string league = country.Text + " " + division.Text;
            return league;
        }
        static void parser(IWebDriver driver, string del)
        {
            int countseason = 3;
            string[] tags = new string[1000];
            string[] array = new string[1000];
            string round;
            string hc, ac, date, home, away;

            // Создали новую вкладку
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript("window.open();");
            js.ExecuteScript("window.open();");
            driver.SwitchTo().Window(driver.WindowHandles[0]);

            Console.WriteLine("Loading site!");
            driver.Url = "https://www.soccerstand.com/soccer/kazakhstan/premier-league/";
            Thread.Sleep(1000);
            Console.WriteLine("Finish!");

            try
            {
                driver.FindElement(By.XPath("//*[@id='onetrust-banner-sdk']//button[@id='onetrust-accept-btn-handler']")).Click();
                Console.WriteLine("I Accept");
            }
            catch { }

            //Выполняем вход в аккаунт
            driver.SwitchTo().Window(driver.WindowHandles[0]);
            //driver.Manage().Window.Maximize();
            driver.FindElement(By.XPath("//*[@id='signIn']")).Click();
            Thread.Sleep(1000);
            driver.FindElement(By.XPath("//*[@id='email']")).SendKeys("mk-1996@ya.ru");
            driver.FindElement(By.XPath("//*[@id='passwd']")).SendKeys("wsc2016");
            driver.FindElement(By.XPath("//*[@id='login']")).Click();
            Console.WriteLine("Вход выполнен");
            Thread.Sleep(5000);

            var myleagues = driver.FindElements(By.XPath("//*[@id='my-leagues-list']/li[@title]"));

            int i = 1;
            foreach (IWebElement league in myleagues)
            {
                Console.WriteLine(i + ". " + league.GetAttribute("title"));
                ++i;
            }
            //string countleague = Console.ReadLine();
            string countleague = "1";

            try
            {
                driver.FindElement(By.XPath("//*[@id='my-leagues-list']/li[" + countleague + "]")).Click();
                Thread.Sleep(500);
            }
            catch
            {
                driver.FindElement(By.XPath("//*[@id='my-leagues-list']/li[" + countleague + "]")).Click();
                Thread.Sleep(500);
            }

            driver.FindElement(By.XPath("//*[@id='li4']")).Click();
            Thread.Sleep(500);

            var seasons = driver.FindElements(By.XPath("//*[@class='leagueTable__season']/div/a"));
            int k = 0;

            foreach (IWebElement season in seasons)
            {
                driver.SwitchTo().Window(driver.WindowHandles[0]);
                if (k++ == countseason)
                    break;
                Console.WriteLine("     " + season.Text);
                var url = season.GetAttribute("href");
                driver.SwitchTo().Window(driver.WindowHandles[1]);
                driver.Url = url;
                Thread.Sleep(500);

                var year = driver.FindElement(By.XPath("//*[@class='teamHeader__text']")).Text;
                var country = driver.FindElement(By.XPath("//*[@class='event__title--type']")).Text;
                var liga = driver.FindElement(By.XPath("//*[@class='event__title--name']")).Text;
                string pathtotal = "C:\\Users\\micha\\Desktop\\Elo corners\\DB\\" + year + "\\" + country + " " + liga + ".csv";

                driver.FindElement(By.XPath("//*[@id='li1']")).Click();
                Thread.Sleep(500);

                while (true)
                {
                    try
                    {
                        js.ExecuteScript("window.scrollBy(0,100)", "");
                        driver.FindElement(By.XPath("*//a[contains(@class,'event__more')]")).Click();
                        Console.WriteLine("Event more Clicked!");
                    }
                    catch (OpenQA.Selenium.NoSuchElementException)
                    {
                        break;
                    }
                    catch (OpenQA.Selenium.ElementClickInterceptedException)
                    { }
                    catch (OpenQA.Selenium.StaleElementReferenceException)
                    { }
                }

                var games = driver.FindElements(By.XPath("*//div[contains(@id,'g_1_')]"));
                int l = 0;

                foreach (IWebElement game in games)
                {
                    var tag = game.GetAttribute("id").Substring(4);
                    tags[i] = tag;
                    var link = "https://soccerstand.com/match/" + tags[i] + "/#match-summary";
                    array[i] = link;
                    ++i;
                }

                for (int m = i - 1; m > 0; m--)
                {
                    driver.SwitchTo().Window(driver.WindowHandles[2]);

                    driver.Url = "https://soccerstand.com/match/" + tags[m] + "/#match-summary";
                    Thread.Sleep(500);

                    int repeat = 0;
                    try
                    {
                        var xpround = driver.FindElement(By.XPath("*//a[contains(.,'Round')]"));
                        round = xpround.Text.Substring(xpround.Text.IndexOf("- ") + 2);
                    }
                    catch
                    {
                        continue;
                    }

                    Thread.Sleep(1000);
                    try
                    {
                        driver.FindElement(By.XPath("*//li[contains(@id,'statistics')]")).Click();
                        Thread.Sleep(1000);
                    }
                    catch
                    {
                        ++m;
                        ++repeat;
                        Console.WriteLine(repeat);
                        if (repeat == 3)
                        {
                            --m;
                            continue;
                        }
                        Console.WriteLine("Error!");
                        continue;
                    }

                    try
                    {
                        var xpdate = driver.FindElement(By.XPath("*//div[@id='utime']"));
                        date = xpdate.Text;
                    }
                    catch (OpenQA.Selenium.NoSuchElementException)
                    {
                        ++m;
                        ++repeat;
                        if (repeat == 3)
                        {
                            --m;
                            continue;
                        }
                        Console.WriteLine("Error!");
                        continue;
                    }

                    try
                    {
                        var xphome = driver.FindElement(By.XPath("*//div[contains(@class,'tname-home')]//a"));
                        home = xphome.Text;
                    }
                    catch (OpenQA.Selenium.NoSuchElementException)
                    {
                        ++m;
                        ++repeat;
                        if (repeat == 3)
                        {
                            --m;
                            continue;
                        }
                        Console.WriteLine("Error!");
                        continue;
                    }

                    try
                    {
                        var xpaway = driver.FindElement(By.XPath("*//div[contains(@class,'tname-away')]//a"));
                        away = xpaway.Text;
                    }
                    catch (OpenQA.Selenium.NoSuchElementException)
                    {
                        ++m;
                        ++repeat;
                        if (repeat == 3)
                        {
                            --m;
                            continue;
                        }
                        Console.WriteLine("Error!");
                        continue;
                    }

                    try
                    {
                        var xphc = driver.FindElement(By.XPath("*//div[@class='statRow']/div[contains(.,'Corner Kicks')]//preceding-sibling::div"));
                        hc = xphc.Text;
                    }
                    catch (OpenQA.Selenium.NoSuchElementException)
                    {
                        ++m;
                        ++repeat;
                        if (repeat == 3)
                        {
                            --m;
                            continue;
                        }
                        Console.WriteLine("Error!");
                        continue;
                    }

                    try
                    {
                        var xpac = driver.FindElement(By.XPath("*//div[@class='statRow']/div[contains(.,'Corner Kicks')]//following-sibling::div[2]"));
                        ac = xpac.Text;
                    }
                    catch (OpenQA.Selenium.NoSuchElementException)
                    {
                        ++m;
                        ++repeat;
                        if (repeat == 3)
                        {
                            --m;
                            continue;
                        }
                        Console.WriteLine("Error!");
                        continue;
                    }
                    int total = Convert.ToInt32(hc) + Convert.ToInt32(ac);
                    Console.WriteLine(round + " / " + tags[m] + " / " + date + " / " + home + " / " + hc + " / " + ac + " / " + away + " / " + total);

                    driver.SwitchTo().Window(driver.WindowHandles[1]);

                    // This text is added only once to the file.
                    if (!File.Exists(pathtotal))
                    {
                        System.IO.Directory.CreateDirectory("C:\\Users\\micha\\Desktop\\Elo corners\\DB\\" + year);
                        //Создание файла для записи в него
                        string createText = "round" + del + "tag" + del + "date" + del + "Home Team" + del + "Guest Team" + del + "Home Corners" + del + "Guest Corner" + del + "Total" + Environment.NewLine;
                        File.WriteAllText(pathtotal, createText);
                        Console.WriteLine("File created!");
                    }

                    string line = round + del + tags[m] + del + date + del + home + del + away + del + " " + hc + del + " " + ac + del + " " + total + Environment.NewLine;
                    File.AppendAllText(pathtotal, line);
                    Console.WriteLine("Text appended!");
                }
            }
        }

        static void totalparser(IWebDriver driver, string del, int countseason)
            {
                string hc, ac;

                //Создали новую вкладку
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("window.open();");
                js.ExecuteScript("window.open();");
                driver.SwitchTo().Window(driver.WindowHandles[0]);

                Console.WriteLine("Loading site!");
                driver.Url = "https://www.soccerstand.com/";
                Thread.Sleep(1000);
                Console.WriteLine("Finish!");

                try
                {
                    driver.FindElement(By.XPath("//*[@id='onetrust-banner-sdk']//button[@id='onetrust-accept-btn-handler']")).Click();
                    Console.WriteLine("I Accept");
                }
                catch { }

                //Выполняем вход в аккаунт
                driver.SwitchTo().Window(driver.WindowHandles[0]);
                //driver.Manage().Window.Maximize();
                driver.FindElement(By.XPath("//*[@id='signIn']")).Click();
                Thread.Sleep(1000);
                driver.FindElement(By.XPath("//*[@id='email']")).SendKeys("mk-1996@ya.ru");
                driver.FindElement(By.XPath("//*[@id='passwd']")).SendKeys("wsc2016");
                driver.FindElement(By.XPath("//*[@id='login']")).Click();
                Console.WriteLine("Вход выполнен");
                Thread.Sleep(5000);

                var myleagues = driver.FindElements(By.XPath("//*[@id='my-leagues-list']/li"));

                int i = 1;
                foreach (IWebElement league in myleagues)
                {
                    Console.WriteLine(i + ". " + league.GetAttribute("title"));
                    ++i;
                }
                //string countleague = Console.ReadLine();
                string countleague = "9";

                try
                {
                    driver.FindElement(By.XPath("//*[@id='my-leagues-list']/li[" + countleague + "]")).Click();
                    Thread.Sleep(500);
                }
                catch
                {
                    driver.FindElement(By.XPath("//*[@id='my-leagues-list']/li[" + countleague + "]")).Click();
                    Thread.Sleep(500);
                }


                var filename = driver.FindElement(By.XPath("//*[@id='my-leagues-list']/li[" + countleague + "]")).Text;
                string pathtotal = "C:\\Users\\micha\\Desktop\\Elo corners\\total\\" + filename + ".csv";

                driver.FindElement(By.XPath("//*[@id='li4']")).Click();
                Thread.Sleep(500);

                var seasons = driver.FindElements(By.XPath("//*[@class='leagueTable__season']/div/a"));
                int k = 0;

                foreach (IWebElement season in seasons)
                {
                    driver.SwitchTo().Window(driver.WindowHandles[0]);
                    if (k++ == countseason)
                        break;
                    Console.WriteLine("     " + season.Text);
                    var url = season.GetAttribute("href");
                    driver.SwitchTo().Window(driver.WindowHandles[1]);
                    driver.Url = url;
                    Thread.Sleep(500);

                    driver.FindElement(By.XPath("//*[@id='li1']")).Click();
                    Thread.Sleep(500);

                    while (true)
                    {
                        try
                        {
                            js.ExecuteScript("window.scrollBy(0,100)", "");
                            driver.FindElement(By.XPath("*//a[contains(@class,'event__more')]")).Click();
                            Console.WriteLine("Event more Clicked!");
                        }
                        catch (OpenQA.Selenium.StaleElementReferenceException)
                        { }
                        catch (OpenQA.Selenium.NoSuchElementException)
                        {
                            break;
                        }
                        catch (OpenQA.Selenium.ElementClickInterceptedException)
                        {
                            try
                            {
                                js.ExecuteScript("window.scrollBy(0,100)", "");
                                driver.FindElement(By.XPath("*//a[contains(@class,'event__more')]")).Click();
                                Console.WriteLine("Event more Clicked!");
                            }
                            catch (OpenQA.Selenium.ElementClickInterceptedException)
                            { }
                        }
                    }

                    var games = driver.FindElements(By.XPath("*//div[contains(@id,'g_1_')]"));

                    foreach (IWebElement game in games)
                    {
                        var tag = game.GetAttribute("id").Substring(4);
                        driver.SwitchTo().Window(driver.WindowHandles[2]);

                        driver.Url = "https://soccerstand.com/match/" + tag + "/#match-summary";
                        Thread.Sleep(500);

                        try
                        {
                            driver.FindElement(By.XPath("//*[contains(@id,'statistics')]")).Click();
                            Thread.Sleep(500);
                        }
                        catch (OpenQA.Selenium.ElementNotInteractableException)
                        {
                            try
                            {
                                driver.Navigate().Refresh();
                                Thread.Sleep(1000);
                                driver.FindElement(By.XPath("//*[contains(@id,'statistics')]")).Click();
                                Thread.Sleep(500);
                            }
                            catch { }
                        }

                        try
                        {
                            var xphc = driver.FindElement(By.XPath("*//div[@class='statRow']/div[contains(.,'Corner Kicks')]//preceding-sibling::div"));
                            hc = xphc.Text;
                        }
                        catch (OpenQA.Selenium.NoSuchElementException)
                        {
                            try
                            {
                                driver.Navigate().Refresh();
                                Thread.Sleep(1000);
                                var xphc = driver.FindElement(By.XPath("*//div[@class='statRow']/div[contains(.,'Corner Kicks')]//preceding-sibling::div"));
                                hc = xphc.Text;
                                Console.WriteLine("catch home corners'!");
                            }
                            catch
                            {
                                hc = "0";
                                Console.WriteLine("false!");
                            }
                        }

                        try
                        {
                            var xpac = driver.FindElement(By.XPath("*//div[@class='statRow']/div[contains(.,'Corner Kicks')]//following-sibling::div[2]"));
                            ac = xpac.Text;
                        }
                        catch (OpenQA.Selenium.NoSuchElementException)
                        {
                            try
                            {
                                driver.Navigate().Refresh();
                                Thread.Sleep(1000);
                                var xpac = driver.FindElement(By.XPath("*//div[@class='statRow']/div[contains(.,'Corner Kicks')]//following-sibling::div[2]"));
                                ac = xpac.Text;
                                Console.WriteLine("catch away corners'!");
                            }
                            catch
                            {
                                ac = "0";
                                Console.WriteLine("false!");
                            }
                        }

                        int total = Convert.ToInt32(hc) + Convert.ToInt32(ac);

                        // This text is added only once to the file.
                        if (!File.Exists(pathtotal))
                        {
                            //Создание файла для записи в него
                            string createText = "total" + Environment.NewLine;
                            File.WriteAllText(pathtotal, createText);
                            //Console.WriteLine("File created!");
                        }

                        string line = total + Environment.NewLine;
                        File.AppendAllText(pathtotal, line);
                        //Console.WriteLine("Text appended!");

                        driver.SwitchTo().Window(driver.WindowHandles[1]);
                    }
                }
                driver.Quit();
            }


            static void Main(string[] args)
            {
                SetThreadExecutionState(EXECUTION_STATE.ES_SYSTEM_REQUIRED | EXECUTION_STATE.ES_DISPLAY_REQUIRED | EXECUTION_STATE.ES_CONTINUOUS);

                string temp = Environment.CurrentDirectory;
                // Set the path and filename variable "path", filename being MyTest.csv in this example.
                //string pathDB = temp + "\\Russia.csv";
                //string pathDB = "C:\\Users\\micha\\Desktop\\Elo corners\\DB\\" + liga + ".csv";

                // Set the variable "delimiter" to ", ".
                string del = "; ";

                Console.WriteLine("Configure!");
                var options = new ChromeOptions();

                //options.AddArgument("headless");
                options.AddArgument("--silent");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--log-level=3");
                ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                service.SuppressInitialDiagnosticInformation = true;
                service.HideCommandPromptWindow = true;
                IWebDriver driver = new ChromeDriver(service, options);

                parser(driver, del);
                //totalparser(driver, del, countseason);
            }
        }
}

