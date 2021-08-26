using AutoClick.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Input;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;

namespace AutoClick
{
    public class HandleAutoClick : ViewModelBase
    {
        private System.Windows.Forms.NotifyIcon MyNotifyIcon;

        private const int MOUSEEVENTF_LEFTDOWN = 0x0002; /* left button down */
        private const int MOUSEEVENTF_LEFTUP = 0x0004; /* left button up */
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008; /* right button down */
        private const int MOUSEEVENTF_RIGHTUP = 0x0010; /* right button up */
        private System.Windows.Threading.DispatcherTimer dt;


        //Local Variable
        private string reportLocation;

        public string ReportLocation
        {
            get => reportLocation;
            set
            {
                reportLocation = value;
                OnPropertyChanged();
            }
        }

        private bool isSettingApplication = true;
        public bool IsSettingApplication
        {
            get => isSettingApplication;
            set
            {
                isSettingApplication = value;
                OnPropertyChanged();
            }
        }
        private string userId;
        public string UserId
        {
            get => userId;
            set
            {
                userId = value;
                OnPropertyChanged();
            }
        }
        private string xToken;
        public string XToken
        {
            get => xToken;
            set
            {
                xToken = value;
                OnPropertyChanged();
            }
        }

        private int startBetLevel = 1;
        public int StartBetLevel
        {
            get => startBetLevel;
            set
            {
                startBetLevel = value;
                OnPropertyChanged();
            }
        }

        public int BetLevel { get; set; } = 1;
        private int maxBetCount;
        public int MaxBetCount
        {
            get => maxBetCount;
            set
            {
                maxBetCount = value;
                if (IsSettingApplication)
                {
                    InitBetCount = maxBetCount;
                }
                OnPropertyChanged();
            }
        }
        private BetType betType = BetType.Tai;
        public BetType BetType
        {
            get => betType;
            set
            {
                betType = value;
                OnPropertyChanged();
            }
        }
        public bool IsStop { get; set; } = false;

        private int betLevelTemp;
        public int BetLevelTemp
        {
            get => betLevelTemp;
            set
            {
                betLevelTemp = value;
                OnPropertyChanged();
            }
        }

        private int initMoneyPerRound = 0;

        public int InitMoneyPerRound
        {
            get => initMoneyPerRound;
            set
            {
                initMoneyPerRound = value;
                OnPropertyChanged();
            }
        }

        private int countDownTime = 5;
        public int CountDownTime
        {
            get => countDownTime;
            set
            {
                countDownTime = value;
                OnPropertyChanged();
            }
        }

        private string resultBet;
        public string ResultBet
        {
            get => resultBet;
            set
            {
                resultBet = value;
                OnPropertyChanged();
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [DllImport("user32")]
        public static extern int SetCursorPos(int x, int y);
        public ObservableCollection<ActionEntry> ActionEntriesCollection { get; set; } = new ObservableCollection<ActionEntry>();

        private ActionEntry selectedButton;
        private List<ActionEntry> sortedList;
        public ActionEntry SelectedButton
        {
            get => selectedButton;
            set
            {
                selectedButton = value;
                OnPropertyChanged();
            }
        }

        public ICommand SetBetTypeTai
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        BetType = BetType.Tai;
                    }
                };
            }
        }

        public ICommand SetBetTypeXiu
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        BetType = BetType.Xiu;
                    }
                };
            }
        }

        public ICommand DeleteButton
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        if (IsSettingApplication)
                        {
                            RemoveButtonFromList();
                        }
                    }
                };
            }
        }

        public ICommand AddTaiPosition
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        if (IsSettingApplication)
                        {
                            var xxxx = System.Windows.Forms.Cursor.Position.X;
                            var yyyy = System.Windows.Forms.Cursor.Position.Y;
                            AddTai(xxxx, yyyy);
                            OnPropertyChanged();
                        }
                    }
                };
            }
        }

        public ICommand AddXiuPosition
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        if (IsSettingApplication)
                        {
                            var xxxx = System.Windows.Forms.Cursor.Position.X;
                            var yyyy = System.Windows.Forms.Cursor.Position.Y;
                            AddXiu(xxxx, yyyy);
                            OnPropertyChanged();
                        }
                    }
                };
            }
        }

        public ICommand AddCuocPosition
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        if (IsSettingApplication)
                        {
                            var xxxx = System.Windows.Forms.Cursor.Position.X;
                            var yyyy = System.Windows.Forms.Cursor.Position.Y;
                            AddCuoc(xxxx, yyyy);
                            OnPropertyChanged();
                        }
                    }
                };
            }
        }

        public ICommand AddValuePosition
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        var xxxx = System.Windows.Forms.Cursor.Position.X;
                        var yyyy = System.Windows.Forms.Cursor.Position.Y;
                        AddValue(xxxx, yyyy);
                        OnPropertyChanged();
                    }
                };
            }
        }

        public ICommand Start
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        if (!IsSettingApplication)
                        {
                            dt.Stop();
                            IsSettingApplication = true;
                            IsStop = false;
                            CountDownTime = 5;
                        }
                        else
                        {
                            if (!ActionEntriesCollection.Any(c => c.ButtonType == ButtonType.Tai)
                        || !ActionEntriesCollection.Any(c => c.ButtonType == ButtonType.Xiu)
                        || !ActionEntriesCollection.Any(c => c.ButtonType == ButtonType.Cuoc)
                        || !ActionEntriesCollection.Any(c => c.ButtonType == ButtonType.Other)
                        || string.IsNullOrEmpty(XToken)
                        || string.IsNullOrEmpty(UserId))
                                return;
                            IsSettingApplication = false;
                            StartWork();
                        }
                    }
                };
            }
        }

        private void StartWork()
        {
            Countdown(TimeSpan.FromSeconds(1), cur => CountDownTime = cur);
        }

        private async Task Countdown(TimeSpan interval, Action<int> ts)
        {
            if (sortedList == null || sortedList.Count != (ActionEntriesCollection.Count - 3))
            {
                sortedList = ActionEntriesCollection.Where(x => x.ButtonType == ButtonType.Other).OrderByDescending(o => o.Value).ToList();
            }

            if (sortedList == null || sortedList.Count == 0) return;
            var tai = ActionEntriesCollection.FirstOrDefault(x => x.ButtonType == ButtonType.Tai);
            if (tai == null) return;
            var xiu = ActionEntriesCollection.FirstOrDefault(x => x.ButtonType == ButtonType.Xiu);
            if (xiu == null) return;
            var cuoc = ActionEntriesCollection.FirstOrDefault(x => x.ButtonType == ButtonType.Cuoc);
            if (cuoc == null) return;

            if (InitBetCount == MaxBetCount)
            {
                BetLevel = StartBetLevel;
            }

            dt = new System.Windows.Threading.DispatcherTimer();
            dt.Interval = interval;
            dt.Tick += (_, a) =>
            {
                if (CountDownTime-- == 0 )
                {
                    CountDownTime = 5;
                    if (IsTurnToNextRound)
                    {
                        Task.Run(() => AutoClick(tai, xiu, cuoc));
                    }
                }
                else
                    ts(CountDownTime);
            };
            ts(CountDownTime);
            dt.Start();

            if (!IsStop)
            {
                _ = Task.Run(async () =>
                {
                    await AcctionBet(tai, xiu, cuoc);

                    string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Log";
                    if (!Directory.Exists(path))
                    {
                        Common.CreateDirectoryAndGrantFullControlPermission(path);
                    }
                    using (StreamWriter w = File.AppendText(path + @"\Reportlog.txt"))
                    {
                        Log(w, 0);
                        w.WriteLine($"\r\nLog Entry : {DateTime.Now.ToLongTimeString()}");
                        w.WriteLine($"Result this game: Lượt đầu");
                        w.WriteLine($"Bet countdown: {MaxBetCount}");
                        w.WriteLine($"Bet level: {BetLevel}000");
                        w.WriteLine("-------------------------------");
                    }

                    IsStop = true;
                });

            }
        }

        private int InitBetCount = 0;

        private string statusMinMaxLastRound;

        private long idLastRound = 0;

        private int luot = 0;

        private async Task AcctionBet(ActionEntry tai, ActionEntry xiu, ActionEntry cuoc)
        {
            if (BetType == BetType.Tai)
            {
                await FireClick(xiu);
                await Task.Delay(200);
                await FireClick(tai);
                await Task.Delay(200);
            }
            else
            {
                await FireClick(tai);
                await Task.Delay(200);
                await FireClick(xiu);
                await Task.Delay(200);
            }
            await CalculateBetMoney(sortedList);
            InitMoneyPerRound = 0;
            await FireClick(cuoc);
        }

        private void CheckResultBetAPI(ref string resultStatus, ref string statusMinMax, ref long idThisRound, ref long betMoney)
        {
            var baseAddress = "https://api-gateway.8afipa.com/gwms/v1/lsc.aspx";
            var webRequest = System.Net.WebRequest.Create(baseAddress);

            if (webRequest != null)
            {
                webRequest.Method = "POST";
                webRequest.Headers.Add("20", "*/*");
                webRequest.ContentType = "application/json";
                webRequest.ContentLength = 121;
                webRequest.Headers.Add("Origin", "https://play.go88vn.top");
                webRequest.Headers.Add("36", "https://play.go88vn.top/");
                webRequest.Headers.Add("x-token", XToken);
                webRequest.Headers.Add("sec-fetch-site", "cross-site");

                using (var streamWriter = new StreamWriter(webRequest.GetRequestStream()))
                {
                    string json = $"{{\"sort\":[{{\"created_time\":{{\"order\":\"desc\"}}}}],\"from\":0,\"size\":5,\"query\":{{\"bool\":{{\"must\":[{{\"match\":{{\"uid\":\"{UserId}\"}}}}]}}}}}}";

                    try
                    {
                        streamWriter.Write(json);
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }
            }
            var httpResponse = (HttpWebResponse)webRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(result);
                resultStatus = json?.data[0]?._source?.game_ticket_status;
                statusMinMax = json?.data[0]?._source?.game_your_bet;
                statusMinMax = statusMinMax.Substring(statusMinMax.IndexOf('.') + 10, 3);
                idThisRound = json?.data[0]?._source?.id;
                betMoney = json?.data[0]?._source?.game_stake;
            }
        }

        private async Task AutoClick(ActionEntry tai, ActionEntry xiu, ActionEntry cuoc)
        {
            #region Get result last round

            string resultStatus = null;
            string statusMinMax = null;
            long idThisRound = 0;
            long betMoney = 0;

            CheckResultBetAPI(ref resultStatus, ref statusMinMax, ref idThisRound, ref betMoney);

            #endregion
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Log\Reportlog.txt";
            if(idLastRound == 0)
            {
                using (StreamWriter w = File.AppendText(path))
                {
                    w.WriteLine($"\r\nLog Entry : {DateTime.Now.ToLongTimeString()} - Round {luot}");
                    w.WriteLine($"Id lượt đầu : {idThisRound}");
                    w.WriteLine("-------------------------------");
                }
                idLastRound = idThisRound;
            }
            if (idLastRound == idThisRound && resultStatus == ResultStatus.Running.ToString())
            {
                //Ghi log ở đây sau
                using (StreamWriter w = File.AppendText(path))
                {
                    w.WriteLine($"\r\nLog Entry : {DateTime.Now.ToLongTimeString()} - Running");
                    w.WriteLine("-------------------------------");
                }
                return;
            }
            IsTurnToNextRound = false;

            luot++;
            #region Demo new code 21/08/2021
            string statusLastRound = resultStatus;
            if (MaxBetCount == 0)
            {
                resultStatus = ResultStatus.Lose.ToString();
            }

            if (resultStatus == ResultStatus.Win.ToString())
            {
                MaxBetCount--;
                BetLevel *= 2;
                double commission = (double)BetLevel * 2 / 100;
                if (commission >= StartBetLevel)
                {
                    BetLevel += StartBetLevel;
                }

                string startTime = DateTime.Now.ToLongTimeString();
                
                #region Bet Then Check
                await AcctionBet(tai, xiu, cuoc);

                //Check does the game is bet
                await Task.Delay(6500);
                CheckResultBetAPI(ref resultStatus, ref statusMinMax, ref idThisRound, ref betMoney);

                int i = 0;
                while (idThisRound == idLastRound)
                {
                    await AcctionBet(tai, xiu, cuoc);

                    //Check does the game is bet
                    await Task.Delay(15000);
                    CheckResultBetAPI(ref resultStatus, ref statusMinMax, ref idThisRound, ref betMoney);
                    i++;
                    if (i == 3)
                    {
                        using (StreamWriter w = File.AppendText(path))
                        {
                            w.WriteLine($"\r\nLog Entry : {DateTime.Now.ToLongTimeString()}");
                            w.WriteLine($"Bet 3 lần không trả về api");
                            w.WriteLine("-------------------------------");
                        }
                        StopApplication();
                        return;
                    }
                }

                idLastRound = idThisRound;
                using (StreamWriter w = File.AppendText(path))
                {
                    w.WriteLine($"\r\nLog Entry : {startTime} - Round {luot}");
                    w.WriteLine($"Kết quả ván trước : {statusLastRound}");
                    w.WriteLine($"Id ván này : {idThisRound}");
                    w.WriteLine($"Commission : {commission}");
                    w.WriteLine($"Số lượt: {MaxBetCount}");
                    w.WriteLine($"Số tiền cược: {BetLevel}000");
                    w.WriteLine("-------------------------------");
                }
                #endregion
            }
            else if (resultStatus == ResultStatus.Lose.ToString())
            {
                MaxBetCount = InitBetCount;
                BetLevel = StartBetLevel;
                string startTime = DateTime.Now.ToLongTimeString();
                
                #region Bet Then Check
                await AcctionBet(tai, xiu, cuoc);

                //Check does the game is bet
                await Task.Delay(6500);
                CheckResultBetAPI(ref resultStatus, ref statusMinMax, ref idThisRound, ref betMoney);

                int i = 0;
                while (idThisRound == idLastRound)
                {
                    await AcctionBet(tai, xiu, cuoc);

                    //Check does the game is bet
                    await Task.Delay(15000);
                    CheckResultBetAPI(ref resultStatus, ref statusMinMax, ref idThisRound, ref betMoney);
                    i++;
                    if (i == 3)
                    {
                        using (StreamWriter w = File.AppendText(path))
                        {
                            w.WriteLine($"\r\nLog Entry : {DateTime.Now.ToLongTimeString()}");
                            w.WriteLine($"Bet 3 lần không trả về api");
                            w.WriteLine("-------------------------------");
                        }
                        StopApplication();
                        return;
                    }
                }

                idLastRound = idThisRound;
                using (StreamWriter w = File.AppendText(path))
                {
                    w.WriteLine($"\r\nLog Entry : {startTime} - Round {luot}");
                    w.WriteLine($"Kết quả ván trước : {statusLastRound}");
                    w.WriteLine($"Id ván này : {idThisRound}");
                    w.WriteLine($"Commission : default");
                    w.WriteLine($"Số lượt: {MaxBetCount}");
                    w.WriteLine($"Số tiền cược: {BetLevel}000");
                    w.WriteLine("-------------------------------");
                }
                #endregion

            }
            else if (resultStatus == ResultStatus.Draw.ToString())
            {
                string startTime = DateTime.Now.ToLongTimeString();

                #region Bet Then Check
                await AcctionBet(tai, xiu, cuoc);

                //Check does the game is bet
                await Task.Delay(6500);
                CheckResultBetAPI(ref resultStatus, ref statusMinMax, ref idThisRound, ref betMoney);

                int i = 0;
                while (idThisRound == idLastRound)
                {
                    await AcctionBet(tai, xiu, cuoc);

                    //Check does the game is bet
                    await Task.Delay(15000);
                    CheckResultBetAPI(ref resultStatus, ref statusMinMax, ref idThisRound, ref betMoney);
                    i++;
                    if (i == 3)
                    {
                        using (StreamWriter w = File.AppendText(path))
                        {
                            w.WriteLine($"\r\nLog Entry : {DateTime.Now.ToLongTimeString()}");
                            w.WriteLine($"Bet 3 lần không trả về api");
                            w.WriteLine("-------------------------------");
                        }
                        StopApplication();
                        return;
                    }
                }

                idLastRound = idThisRound;
                using (StreamWriter w = File.AppendText(path))
                {
                    w.WriteLine($"\r\nLog Entry : {startTime} - Round {luot}");
                    w.WriteLine($"Kết quả ván trước : {statusLastRound}");
                    w.WriteLine($"Id ván này : {idThisRound}");
                    w.WriteLine($"Commission : ");
                    w.WriteLine($"Số lượt: {MaxBetCount}");
                    w.WriteLine($"Số tiền cược: {BetLevel}000");
                    w.WriteLine("-------------------------------");
                }
                #endregion
            }
            else
            {
                string startTime = DateTime.Now.ToLongTimeString();
                using (StreamWriter w = File.AppendText(path))
                {
                    w.WriteLine($"\r\nLog Entry : {startTime} - Round {luot}");
                    w.WriteLine($"Kết quả ván trước : {statusLastRound}");
                    w.WriteLine($"Id ván này : {idThisRound}");
                    w.WriteLine($"Commission : null");
                    w.WriteLine($"Số lượt: {MaxBetCount}");
                    w.WriteLine($"Số tiền cược: {BetLevel}000");
                    w.WriteLine("-------------------------------");
                }
                StopApplication();
                
            }

            IsTurnToNextRound = true;
            #endregion

            #region Code cũ
            //if (idLastRound == 0)
            //{
            //    using (StreamWriter w = File.AppendText(path))
            //    {
            //        Log(w, 1, idThisRound, statusMinMax);
            //    }
            //    idLastRound = idThisRound;
            //    return;
            //}

            //if (idLastRound == idThisRound)
            //{
            //    if (resultStatus == ResultStatus.Running.ToString())
            //    {
            //        using (StreamWriter w = File.AppendText(path))
            //        {
            //            Log(w, 2, idThisRound, statusMinMax);
            //        }
            //        return;
            //    }
            //    else
            //    {
            //        using (StreamWriter w = File.AppendText(path))
            //        {
            //            Log(w, 3, idThisRound, statusMinMax);
            //        }
            //        await Task.Delay(13000);
            //        ResultBet = resultStatus;
            //        ThongKe(statusMinMax);

            //        if (resultStatus == ResultStatus.Lose.ToString() && MaxBetCount == 0)
            //        {
            //            resultStatus = ResultStatus.Win.ToString();
            //        }

            //        if (resultStatus == ResultStatus.Lose.ToString())
            //        {
            //            MaxBetCount--;
            //            BetLevel *= 2;
            //            double commission = (double)BetLevel * 2 / 100;
            //            if (commission >= StartBetLevel)
            //            {
            //                BetLevel += StartBetLevel;
            //            }
            //            using (StreamWriter w = File.AppendText(path))
            //            {
            //                w.WriteLine($"\r\nLog Entry : {DateTime.Now.ToLongTimeString()}");
            //                w.WriteLine($"Commission : {commission}");
            //                w.WriteLine($"Result this game: {resultStatus}");
            //                w.WriteLine($"Bet countdown: {MaxBetCount}");
            //                w.WriteLine($"Bet level: {BetLevel}");
            //                w.WriteLine("-------------------------------");
            //            }

            //            if (BetType == BetType.Tai)
            //            {
            //                await FireClick(xiu);
            //                await Task.Delay(200);
            //                await FireClick(tai);
            //                await Task.Delay(200);
            //            }
            //            else
            //            {
            //                await FireClick(tai);
            //                await Task.Delay(200);
            //                await FireClick(xiu);
            //                await Task.Delay(200);
            //            }
            //            await CalculateBetMoney(sortedList);
            //            InitMoneyPerRound = 0;
            //            await FireClick(cuoc);
            //        }
            //        else if (resultStatus == ResultStatus.Win.ToString())
            //        {
            //            MaxBetCount = InitBetCount;
            //            BetLevel = StartBetLevel;
            //            using (StreamWriter w = File.AppendText(path))
            //            {
            //                w.WriteLine($"\r\nLog Entry : {DateTime.Now.ToLongTimeString()}");
            //                w.WriteLine($"Commission : ");
            //                w.WriteLine($"Result this game: {resultStatus}");
            //                w.WriteLine($"Bet countdown: {MaxBetCount}");
            //                w.WriteLine($"Bet level: {BetLevel}");
            //                w.WriteLine("-------------------------------");
            //            }
            //            if (BetType == BetType.Tai)
            //            {
            //                await FireClick(xiu);
            //                await Task.Delay(200);
            //                await FireClick(tai);
            //                await Task.Delay(200);
            //            }
            //            else
            //            {

            //                await FireClick(tai);
            //                await Task.Delay(200);
            //                await FireClick(xiu);
            //                await Task.Delay(200);
            //            }
            //            await CalculateBetMoney(sortedList);
            //            InitMoneyPerRound = 0;
            //            await FireClick(cuoc);
            //        }
            //        else if (resultStatus == ResultStatus.Draw.ToString())
            //        {
            //            using (StreamWriter w = File.AppendText(path))
            //            {
            //                w.WriteLine($"\r\nLog Entry : {DateTime.Now.ToLongTimeString()}");
            //                w.WriteLine($"Commission : ");
            //                w.WriteLine($"Result this game: {resultStatus}");
            //                w.WriteLine($"Bet countdown: {MaxBetCount}");
            //                w.WriteLine($"Bet level: {BetLevel}");
            //                w.WriteLine("-------------------------------");
            //            }
            //            if (BetType == BetType.Tai)
            //            {
            //                await FireClick(tai);
            //                Thread.Sleep(200);
            //            }
            //            else
            //            {
            //                await FireClick(xiu);
            //                Thread.Sleep(200);
            //            }
            //            await CalculateBetMoney(sortedList);
            //            InitMoneyPerRound = 0;
            //            await FireClick(cuoc);
            //        }
            //        else
            //        {
            //            StopApplication();
            //        }
            //    }
            //}
            //else
            //{
            //    using (StreamWriter w = File.AppendText(path))
            //    {
            //        Log(w, 4, idThisRound, statusMinMax);
            //    }
            //    idLastRound = idThisRound;
            //    return;
            //}
            #endregion
        }

        public bool IsTurnToNextRound { get; set; } = true;
        private void Log(TextWriter writer, int th, long? idThisRound = null, string kqThisRound = null)
        {
            writer.Write($"\r\nLog Entry : {DateTime.Now.ToLongTimeString()}\n");
            writer.WriteLine($"Số lần: {luot}");
            writer.WriteLine("Trường hợp: " + (th == 0 ? "1> Lần đầu" : (th == 1 ? "2> Lần đầu IdLast = 0" : (th == 2 ? "3> Id bằng nhau đang running" : (th == 3 ? "4> Id bằng nhau nhưng có kết quả" : "5> Id khác nhau")))));
            writer.WriteLine($"Id round trước: {idLastRound}");
            writer.WriteLine($"Id round này: {idThisRound}");
            writer.WriteLine($"Kết quả round trước: {ResultBet}");
            writer.WriteLine($"Kết quả round này: {kqThisRound}");
            writer.WriteLine("-------------------------------");
        }

        private void StopApplication()
        {
            if (Start.CanExecute(null))
            {
                Start.Execute(null);
            }
        }

        private async Task FireClick(ActionEntry button)
        {
            SetCursorPos(button.X, button.Y);
            if (button.ClickType.Equals(ClickType.click))
            {
                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            }
            else if (button.ClickType.Equals(ClickType.doubleClick))
            {
                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                await Task.Delay(100);
                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            }
            else //if (action.Type.Equals(ClickType.rightClick))
            {
                mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
            }
        }

        public void RemoveButtonFromList()
        {
            if (selectedButton == null) return;

            ActionEntriesCollection.Remove(selectedButton);
        }

        public void AddTai(int xPos, int yPos)
        {
            var taiPosition = new ActionEntry() { X = xPos, Y = yPos, ButtonType = ButtonType.Tai, ClickType = ClickType.click };
            var preTai = ActionEntriesCollection.FirstOrDefault(x => x.ButtonType == ButtonType.Tai);
            if (preTai != null)
            {
                ActionEntriesCollection[ActionEntriesCollection.IndexOf(preTai)] = taiPosition;
            }
            else
            {
                ActionEntriesCollection.Add(taiPosition);
            }
        }

        public void AddXiu(int xPos, int yPos)
        {
            var xiuPosition = new ActionEntry() { X = xPos, Y = yPos, ButtonType = ButtonType.Xiu, ClickType = ClickType.click };
            var preXiu = ActionEntriesCollection.FirstOrDefault(x => x.ButtonType == ButtonType.Xiu);
            if (preXiu != null)
            {
                ActionEntriesCollection[ActionEntriesCollection.IndexOf(preXiu)] = xiuPosition;
            }
            else
            {
                ActionEntriesCollection.Add(xiuPosition);
            }
        }

        public void AddCuoc(int xPos, int yPos)
        {
            var cuocPosition = new ActionEntry() { X = xPos, Y = yPos, ButtonType = ButtonType.Cuoc, ClickType = ClickType.click };
            var preCuoc = ActionEntriesCollection.FirstOrDefault(x => x.ButtonType == ButtonType.Cuoc);
            if (preCuoc != null)
            {
                ActionEntriesCollection[ActionEntriesCollection.IndexOf(preCuoc)] = cuocPosition;
            }
            else
            {
                ActionEntriesCollection.Add(cuocPosition);
            }
        }

        public void AddValue(int xPos, int yPos)
        {
            var otherPosition = new ActionEntry() { X = xPos, Y = yPos, ButtonType = ButtonType.Other, ClickType = ClickType.click, Value = BetLevelTemp };
            var preCuoc = ActionEntriesCollection.FirstOrDefault(x => x.Value == BetLevelTemp);
            if (preCuoc == null)
            {
                ActionEntriesCollection.Add(otherPosition);
            }
            else
            {
                if (IsSettingApplication)
                {
                    ActionEntriesCollection[ActionEntriesCollection.IndexOf(preCuoc)] = otherPosition;
                }
                else
                {
                    return;
                }
            }
        }

        private async Task CalculateBetMoney(List<ActionEntry> sortedList)
        {
            var remainBetMoney = BetLevel - InitMoneyPerRound;
            if (remainBetMoney == 0) return;
            var button = sortedList.First(x => x.Value <= remainBetMoney);
            await FireClick(button);
            InitMoneyPerRound += button.Value ?? 0;
            Thread.Sleep(500);
            if (remainBetMoney != 0)
            {
                await CalculateBetMoney(sortedList);
            }
        }

        private int increase = 1;
        private int soLan2 = 0;
        private int soLan5 = 0;
        private int soLan6 = 0;
        private int soLan7 = 0;
        private int soLan8 = 0;
        private int soLan9 = 0;
        private int soLan10 = 0;
        private int soLan11 = 0;

        private void ThongKe(string statusMinMax)
        {
            if (statusMinMax == statusMinMaxLastRound)
            {
                increase++;
            }
            else
            {
                increase = 1;
            }
            switch (increase)
            {
                case 2:
                    soLan2++;
                    break;
                case 5:
                    soLan5++;
                    break;
                case 6:
                    soLan6++;
                    break;
                case 7:
                    soLan7++;
                    break;
                case 8:
                    soLan8++;
                    break;
                case 9:
                    soLan9++;
                    break;
                case 10:
                    soLan10++;
                    break;
                case 11:
                    soLan11++;
                    break;
            }
            statusMinMaxLastRound = statusMinMax;
        }

        private void InThongKe()
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Report";

            SaveFileDialog saveFileDialog = new SaveFileDialog();

            string Msg = "5 times: " + soLan5 + Environment.NewLine
                       + "2 times: " + soLan2 + Environment.NewLine
                       + "6 times: " + soLan6 + Environment.NewLine
                       + "7 times: " + soLan7 + Environment.NewLine
                       + "8 times: " + soLan8 + Environment.NewLine
                       + "9 times: " + soLan9 + Environment.NewLine
                       + "10 times: " + soLan10 + Environment.NewLine
                       + "11 times: " + soLan11 + Environment.NewLine;


            if (!Directory.Exists(path))
            {
                Common.CreateDirectoryAndGrantFullControlPermission(path);
            }
            var today = DateTime.Now;
            string fileName = today.Day + "-" + today.Month + "-" + today.Year + "_" + today.Ticks + ".txt";
            File.WriteAllText(path + @"\" + fileName, Msg);
            ReportLocation = path;
        }

        public ICommand PrintReport
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        Task.Run(() => InThongKe());
                    }
                };
            }
        }

        public ICommand ResetBetCountAndBetLevel
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = x =>
                    {
                        BetLevel = StartBetLevel;
                        MaxBetCount = InitBetCount;
                    }
                };
            }
        }
    }
}
