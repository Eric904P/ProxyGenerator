Imports System.IO
Imports System.Net
Imports System.Text.RegularExpressions
Imports System.Threading
Imports ProxyGenerator.My.Resources

Public Class Scraper
    Public Sub New()
    End Sub

    Dim _sources As List(Of String) = New List(Of String)
    Public Shared Scraped As List(Of String) = New List(Of String)
    Dim _thrdCnt As Integer = 0
    ReadOnly _thrdMax As Integer = 50
    ReadOnly _listLock As Object = New Object
    ReadOnly _d As New Dictionary(Of String, Thread)()
    Private _sourceTotal As Integer = 0

    Function ScrapeHerder() As Boolean
        _sourceTotal = _sources.Count
        Dim thrdIndex = 1
        While _sources.Count > 0
            If _thrdCnt <= _thrdMax Then
                _d(thrdIndex.ToString) = New Thread(AddressOf ScrapeTask)
                _d(thrdIndex.ToString).IsBackground = True
                _d(thrdIndex.ToString).Start()
                _thrdCnt = _thrdCnt + 1
                thrdIndex = thrdIndex + 1
            End If
        End While
        Return True
    End Function

    Private Sub ScrapeTask()
        If _sources.Count > 0 Then
            Dim toScrape As String
            SyncLock _listLock
                toScrape = _sources.Item(0)
                _sources.RemoveAt(0)
            End SyncLock
            Scraped.AddRange(ScrapeLink(toScrape).Distinct().ToList())
            _thrdCnt = _thrdCnt - 1
        End If
    End Sub

    'scrapes a given link for proxies
    Private Function ScrapeLink(link As String) As List(Of String)
        Dim proxies = New List(Of String)
        Dim temp As String
        Try 'gets the entire web page as a string
            Dim r As HttpWebRequest = HttpWebRequest.Create(link)
            r.UserAgent =
                "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/30.0.1599.69 Safari/537.36"
            r.Timeout = 15000
            Dim re As HttpWebResponse = r.GetResponse()
            Dim rs As Stream = re.GetResponseStream
            Using sr As New StreamReader(rs)
                temp = sr.ReadToEnd()
            End Using
            Dim src = temp
            rs.Dispose()
            rs.Close()
            r.Abort()
            'uses extractProx method to find all proxies on the webpage
            proxies = ExtractProx(src)

            If proxies.Count > 0 Then
                Console.WriteLine(proxies.Count & v2_Threading_ScrapeLink__proxies_found_at_ & link)
            End If
        Catch ex As Exception

        End Try
        'returns scraped result
        Return proxies
    End Function

    'finds all proxies in a given string, returns them as a List(Of String)
    Private Function ExtractProx(http As String) As List(Of String)
        Dim output = New List(Of String)

        For Each proxy As Match In Regex.Matches(http, "[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+:[0-9]+")
            output.Add(proxy.ToString())
        Next
        Return output
    End Function

    'loads all source links from internal resources
    Private Sub LoadSrc()
        Dim psrc As String = My.Resources.psrc
        _sources = psrc.Split("$").ToList()
        If Not _sources.Count > 0 Then
            LoadSrcWeb()
        End If
        _sources.RemoveAt(0)
    End Sub

    'fallback method, will remove to keep my sources safe
    Private Sub LoadSrcWeb()
        Dim client = New WebClient()
        Dim reader = New StreamReader(client.OpenRead(WebSrc))
        Dim temp As String = reader.ReadToEnd
        Dim tmpSrc As String() = temp.Split("$")
        _sources = tmpSrc.ToList()
        _sources.RemoveAt(0)
    End Sub

    Public Sub Load()
        LoadSrc()
    End Sub

    Public Function ReturnThreadCount() As Integer
        Return _thrdCnt
    End Function

    Public Function ReturnSourceCount() As Integer
        Return _sourceTotal
    End Function

    Public Function ReturnScrapedCount() As Integer
        Return (_sourceTotal - _sources.Count)
    End Function

    Public Function ReturnPercent() As Double
        Return ((_sourceTotal - _sources.Count) / _sourceTotal)
    End Function
End Class