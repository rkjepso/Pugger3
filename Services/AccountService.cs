using Pugger3.Shared;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;


namespace Pugger3.Services;

public interface IAccountService
{
    public Task<string> GetErrorString();
    public List<TWord> GetDictionary();

    public Task<int> Load(LangT lantT);
    //public Task<UserRecord> DoLogin(Login login);
    //public Task<UserRecord> Register(AddUser login);
    public bool IsServerDown { get; set; }
    //public WordSource WordSource { get; }
    //public string GetInfoString { get; }
}



class AccountService : IAccountService
{

    HttpClient? Http { get => Program._Http; }
    private string BaseUrl
    {
        get => Http?.BaseAddress?.ToString() + @"WeatherForecast/";
    }

    public bool IsServerDown { get; set; } = false;
    //public WordSource WordSource { get; set; } = WordSource.Default;

    public async Task<string> GetErrorString()
    {
        try
        {
            if (Http==null)
                return "";
            string uri = BaseUrl + @"GetErrorString";
            var s = await Http.GetStringAsync(uri);
            return s;
        }
        catch (Exception)
        {
            return "";
        }

    }

    private static   Dictionary<LangT, List<TWord>>  LstLang = new();

    public List<TWord> GetDictionary()
    {
        // New stuff
        List<TWord> lst = new ();
        LangT langT = Program.BaseDataObj.LangT;
        if (!LstLang.ContainsKey(langT))
            return lst;
        
        int max = Program.Storage?._GetData().TotalWords ?? 1000;
        lst = LstLang[langT].CopyMany(0, max);   
        return lst;
    }

    public async Task<int> Load(LangT langT)
    {
        if (LstLang.ContainsKey(langT) && LstLang[langT].Count > 0)
            return 0;
DateTime dt = DateTime.Now;
        LstLang[langT] = await GetDefaultDictionary(langT);
int ms = (DateTime.Now - dt).Milliseconds;
        return ms;
    }


    private static async Task<List<TWord>> GetDefaultDictionary(LangT langT)
    {
        var  lst = await ReadData.Read(langT);
        lst = lst.Where(w => !string.IsNullOrWhiteSpace(w.FromWord) &&
                             !string.IsNullOrWhiteSpace(w.ToWord)).ToList();
        lst = lst.Distinct(new TWordItemComparer()).ToList();
        NewID();

        return lst;

        void NewID()
        { 
            int _ID = 1;
            for (int idx = 0; idx < lst.Count; idx++)
                lst[idx] = lst[idx] with { ID = _ID++ };
        }
    }

    //public async Task<UserRecord> DoLogin(Login login)
    //{
    //    UserRecord user;
    //    string url = BaseUrl + @"Login";
    //    user = await Put<UserRecord>(url, login);
    //    return user;
    //}

    //public async Task<UserRecord> Register(AddUser login)
    //{
    //    UserRecord user;
    //    string url = BaseUrl + @"Register";
    //    user = await Put<UserRecord>(url, login);
    //    return user;
    //}

    //private async Task<T> Put<T>(string uri, object value)
    //{
    //    var request = createRequest(HttpMethod.Put, uri, value);
    //    return await sendRequest<T>(request);
    //}
    //private HttpRequestMessage createRequest(HttpMethod method, string uri, object value = null)
    //{
    //    var request = new HttpRequestMessage(method, uri);
    //    if (value != null)
    //    {
    //        request.Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");
    //    }

    //    return request;
    //}
    //private async Task<T> sendRequest<T>(HttpRequestMessage request)
    //{
    //    // send request
    //    using var response = await Http.SendAsync(request);
    //    if (!response.IsSuccessStatusCode)
    //    {
    //        throw new Exception("Something went wrong on the server");
    //    }

    //    var options = new JsonSerializerOptions();
    //    options.PropertyNameCaseInsensitive = true;
    //    // FARLIG options.Converters.Add(new StringConverter());
    //    return await response.Content.ReadFromJsonAsync<T>(options);
    //}

    //public string GetInfoString
    //{
    //    get
    //    {
    //        string StrMsg;
    //        if (IsServerDown)
    //        {
    //            StrMsg = " (innebygd)";
    //        }
    //        else
    //        {
    //            StrMsg = " ok ";
    //        }

    //        if (WordSource == WordSource.LocalStore)
    //        {
    //            StrMsg += " (cache)";
    //        }

    //        if (WordSource == WordSource.Server)
    //        {
    //            StrMsg += " (server)";
    //        }

    //        return StrMsg;
    //    }
    //}
}

//public async Task<List<TWord>> GetLocalCache()
//{
//    ILocalStorageService lss = LocalStorageService.lss;
//    string strVer = "";
//    try
//    {
//        // First try local storage..
//        strVer = await lss.GetItem<string>(Defs.keyCache);
//        if (strVer == Program.GetVersionGloser)
//        {
//            WordLists ??= await lss.GetItem<List<TWord>>(strDict);
//            WordSource = WordSource.LocalStore;
//            return WordLists;
//        }
//    }
//    catch
//    {
//        return null;// Not serious - just continue .....
//    }
//    return null;
//}
//public async void SetLocalCache()
//{
//    ILocalStorageService lss = LocalStorageService.lss;
//    await lss.SetItem(Defs.keyCache, Program.GetVersionGloser);
//    await lss.SetItem(strDict, WordLists);
//}

//private string __cw(string s)
//{
//    s = s.ToLower();
//    s = s.Trim('"');
//    return s;

//}
//IsServerDown = true;
//try
//{
//    WordLists = new List<TWord>();
//    var atb = await Http.GetFromJsonAsync<TWordBase[]>(@"WeatherForecast/GetDict");
//    foreach (var b in atb)
//        WordLists.Add(new TWord()
//        {
//            ID = b.ID,
//            Spanish = __cw(b.Spanish),
//            English = __cw(b.English),
//            Norwegian = __cw(b.Norwegian)
//        });

//    IsServerDown = false;
//    WordSource = WordSource.Server;

//    WordLists.ForEach(o => (o.Norwegian, o.Spanish, o.English)
//                      = (__cw(o.Norwegian), __cw(o.Spanish), __cw(o.English)));

//    SetLocalCache();


//}
//catch (Exception e)
//{         
//    List<TWord> lst = new();
//    WordLists = FillWordsLang(lst, Program.BaseDataObj.LangT);
//    WordSource = WordSource.Default;
//    ErrorInfo = e.Message;
//}   
//return WordLists;