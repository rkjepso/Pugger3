using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using Pugger3.Client;
using Pugger3.Services;
using static Pugger3.Functions;
using static System.Net.WebRequestMethods;

namespace Pugger3
{
    public partial class Program
    {
        public static WebAssemblyHostBuilder? builder;
        public static async Task Main(string[] args)
        {
            builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            MyStuff();
            await AccountService.Load(Program.BaseDataObj.LangT);
            await builder.Build().RunAsync();
        }

        static void MyStuff()
        {
            _Http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
            builder.Services.AddScoped(sp => _Http);

            //// YYZ
            //builder.Services.AddSingleton<ILocalStorageService, LocalStorageService>();
            builder.Services.AddSingleton<IAccountService, AccountService>();
            var Service = builder.Services.AddScoped<BrowserService>(); // scoped service
            builder.Services.AddSingleton<IStorage, Storage>();

            // YYZ
            var host = builder.Build();
            Storage = host.Services.GetRequiredService<IStorage>();
            AccountService = host.Services.GetRequiredService<IAccountService>();
            BrowserService = host.Services.GetRequiredService<BrowserService>();

            _host = host;

            try
            {
                BaseDataObj.Load(Storage);// = await ls.GetItem<ProgramData>("program_data");
            }
            catch (Exception)
            {
                // Data.Default();
            }
        }
    }

    public static partial class Program
    {
        private static WebAssemblyHost? _host;
        static public string GetVersionGloser { get; } = "5.2 ";
        static public string GetDBVersionGloser { get; } = "1.1";
        static public Pugger3.Pages.HelpBox? helpBox = null;

        static private bool showHelp;

        static public bool ShowHelp
        {
            get { return showHelp; }
            set
            {
                showHelp = value;
                try
                {
                }
                catch { }
            }
        }




        public static (int, int) WidthHeight()
        {

            var bs = _host?.Services?.GetRequiredService<BrowserService>();
            var dimension = bs?.GetDimensions();
            if (dimension == null)
                return (0, 0);
            return (dimension.Width, dimension.Height);
        }
        public static int Height { get; set; }

        public static BrowserService? BrowserService { get; set; }

        public static IStorage? Storage { get; set; }
        public static IAccountService? AccountService { get; set; }


        static public bool IsLoggedIn { get; set; } = false;
        static public HttpClient? _Http { get; set; } = null;
        static public BaseData BaseDataObj { get; set; } = new();

        private static Action? a = null;
        static public Action? currentStateHasChanged
        {
            get { return a; }
            set
            {
                if (a != null)
                    a = null;
                a = value;
            }
        }


        static public void InvokeCurrentStateHasChanged()
        {
            currentStateHasChanged?.Invoke();
        }



        public static Data CreateStartupData(int TotalWords=250)
        {
            if (Storage == null)
                return new Data();
            var data = Storage._GetData();
            try
            {
                string strLang = LangInNorwegian(GetFromLangFromLangT(BaseDataObj.LangT));
                data.Default();
                data.IdxStart = 0;
                data.Step = 10;
                data.TotalWords = TotalWords;
                data.Order = Order.Sequental;
                data.Info = string.Format($"{strLang} Ord 1-{data.Step}. {data.TotalWords} viktigste");

                Storage._SetData(data);

                var lst = GetDictionary();
                 SaveSelectedWords(lst.GetRange(0, data.Step));
            }
            catch (Exception)
            {
                return new Data();
            }
            return data;
        }

        public static List<TWord> GetDictionary()
        {
            var lst = AccountService?.GetDictionary();

            LoadListStat(lst);
            return lst ?? new List<TWord> { };
        }

        public static List<TWord> GetSelectedWords()
        {
            var lst = Storage?.GetItem<List<TWord>>(Defs.keySelectedWords);
            return lst ?? new List<TWord> { };
        }

        public static void SaveSelectedWords(List<TWord>? lst)
        {
            if (lst == null)
            {
                return;
            }

            Storage?.SetItem(Defs.keySelectedWords, lst);
        }



        private static void LoadListStat(List<TWord>? lst)
        {
            if (lst == null)
            {
                return;
            }

            var d = Storage?.GetItem<Dictionary<int, int>>(Defs.keyLevel);
            if (d == null)
            {
                return;
            }

            lst.ForEach(w => w.Level = d.ContainsKey(w.ID) ? d[w.ID] : 0);
        }

        private static void UpdateListStat(List<TWord> lst)
        {
            if (Storage == null)
                return;
            var d = Storage.GetItem<Dictionary<int, int>>(Defs.keyLevel);
            d ??= new();
            foreach (var e in lst)
            {
                d[e.ID] = e.Level;
            }

            Storage.SetItem(Defs.keyLevel, d);
        }

        public static void UpdateSelectedWord(TWord tw)
        {
            var lstS = GetSelectedWords();
            if (lstS == null)
            {
                return;
            }

            int i = lstS.FindIndex(w => w.ID == tw.ID);
            if (i == -1)
            {
                return;
            }

            lstS[i] = lstS[i] with { Level = tw.Level };
            SaveSelectedWords(lstS);

            // Very inefficient
            UpdateListStat(lstS);
        }

        // Update Level of selected words ..
        public static void UpdateSelectedWords(List<TWord> lst)
        {
            var lstS = GetSelectedWords();
            lstS ??= new();
            if (lst == null || lst.Count == 0)
                return;

            var d = lst.ToDictionary(keySelector: w => w.ID);
            for (int i = 0; i < lst.Count; i++)
            {
                if (d.Keys.Contains(lst[i].ID))
                    lst[i] = lst[i] with { Level = d[lst[i].ID].Level };
            }
            SaveSelectedWords(lstS);
            UpdateListStat(lstS);
        }
    }
}