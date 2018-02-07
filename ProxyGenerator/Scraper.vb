Imports System.IO
Imports System.Net
Imports System.Net.Cache
Imports System.Net.Security
Imports System.Runtime.Serialization
Imports System.Security.Cryptography.X509Certificates
Imports System.Security.Principal
Imports System.Text.RegularExpressions
Imports System.Threading
Imports ProxyGenerator.My.Resources

Public Class Scraper
    Dim _sources As List(Of String) = New List(Of String)
    Public Shared Scraped As List(Of String) = New List(Of String)
    Dim _thrdCnt As Integer = 0
    ReadOnly _thrdMax As Integer = 50
    ReadOnly _listLock As Object = New Object
    ReadOnly _d As New Dictionary(Of String, Thread)()
    Private _sourceTotal As Integer = 0
    Private _sourceScraped As Integer = 0
    Private _fileIndex = 0
    Private _fileLock As Object = New Object
    Private ReadOnly _screenLock As Object = New Object
    Dim _lineIndex As Integer = 0
    Dim _fileManager As Thread

    ''' <summary>
    '''     constructor
    ''' </summary>
    Public Sub New()
    End Sub

    ''' <summary>
    '''     Main function called to scrape sources
    ''' </summary>
    ''' <returns></returns>
    Function ScrapeHerder() As Boolean
        LoadSrc()
        Console.SetCursorPosition(left:=0, top:=0)
        Console.WriteLine(ScrapeHeader)
        'Console.WriteLine("/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\ PROXY GENERATOR v2.5 /\/\/\/\/\/\/\/\/\/\/\/\/\/\")
        'Console.WriteLine(" /\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\ BY ERIC904P /\/\/\/\/\/\/\/\/\/\/\/\/\/\/\/\")
        'Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++")
        'Console.WriteLine("- - - - - - - - - - - - - - - -SCRAPING PLEASE WAIT- - - - - - - - - - - - - - -")
        'Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++")
        Console.SetCursorPosition(0, 7)
        Console.CursorVisible = False
        _sourceTotal = _sources.Count
        Dim thrdIndex = 1
        _fileManager = New Thread(AddressOf ScrapeBuffer)
        _fileManager.IsBackground = True
        _fileManager.Start()
        While _sources.Count > 0
            If _thrdCnt <= _thrdMax Then
                _d(thrdIndex.ToString) = New Thread(AddressOf ScrapeTask)
                _d(thrdIndex.ToString).IsBackground = True
                _d(thrdIndex.ToString).Start()
                _thrdCnt = _thrdCnt + 1
                thrdIndex = thrdIndex + 1
            End If
        End While
        Thread.Sleep(20000)
        Return True
    End Function

    ''' <summary>
    '''     Task for scraper thread
    ''' </summary>
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

    ''' <summary>
    '''     Scrapes URL 'link' for proxies and returns found proxies as a List(Of String)
    ''' </summary>
    ''' <param name="link"></param>
    ''' <returns></returns>
    Private Function ScrapeLink(link As String) As List(Of String)
        Dim proxies = New List(Of String)()
        Try 
            proxies = ExtractProx(New StreamReader(HttpWebRequestWrapper.CreateNew(link, 15000, UserAgent).GetResponse().GetResponseStream()).ReadToEnd())
        Catch E As Exception
            writeLog(E.ToString())
        End Try
        
            If proxies.Count > 0 Then
                SyncLock _screenLock
                    Console.SetCursorPosition(0, 7)
                    Console.WriteLine(
                        Math.Abs(Math.Round((1 - (_sources.Count / _sourceTotal)) * 100, 2)) &
                        "%                                   ")
                End SyncLock
                _sourceScraped = _sourceScraped + proxies.Count
            End If

            Return proxies

        
    End Function

    ''' <summary>
    '''     Finds all proxies in a given string, returns them as a List(Of String)
    ''' </summary>
    ''' <param name="http"></param>
    ''' <returns></returns>
    Private Function ExtractProx(http As String) As List(Of String)
        Dim output = New List(Of String)

        For Each proxy As Match In Regex.Matches(http, "[0-9]+\.[0-9]+\.[0-9]+\.[0-9]+:[0-9]+")
            output.Add(proxy.ToString())
        Next
        Return output
    End Function

    ''' <summary>
    '''     loads all source links from local resources
    ''' </summary>
    Private Sub LoadSrc()
        Dim psrc As String = My.Resources.psrc
        _sources = psrc.Split("$").ToList()
        If Not _sources.Count > 0 Then
            LoadSrcWeb()
        End If
        _sources.RemoveAt(0)
    End Sub

    ''' <summary>
    '''     fallback source method
    ''' </summary>
    Private Sub LoadSrcWeb()
        Dim client = New WebClient()
        Dim reader = New StreamReader(client.OpenRead(WebSrc))
        Dim temp As String = reader.ReadToEnd
        Dim tmpSrc As String() = temp.Split("$")
        _sources = tmpSrc.ToList()
        _sources.RemoveAt(0)
    End Sub

    ''' <summary>
    ''' Stores scraped proxies to local files to save on memory usage
    ''' </summary>
    Private Sub ScrapeBuffer()
        Dim tmpPath As String = Path.GetTempPath() + PGS_ + _fileIndex.toString()
        While _thrdCnt > 0
            If Scraped.Count > 10000 Then
                File.AppendAllLines(tmpPath, Scraped.GetRange(0, 10000))
                _fileIndex = _fileIndex + 1
                tmpPath = Path.GetTempPath() + PGS_ + _fileIndex.ToString()
                Scraped.RemoveRange(0, 10000)
            End If
        End While
        If Scraped.Count > 0 And Scraped.Count < 10000 Then
            File.AppendAllLines(tmpPath, Scraped)
            _fileIndex = _fileIndex + 1
            tmpPath = Path.GetTempPath() + PGS_ + _fileIndex.ToString()
            scraped.Clear()
        End If
    End Sub

    ''' <summary>
    ''' Rejoins the split local files, if "bool" then they will be deleted
    ''' </summary>
    ''' <param name="bool"></param>
    ''' <returns></returns>
    Public Shared Function LoadScraped(bool as Boolean) As List(Of String)
        Dim scrape = New List(Of String)
        For Each f As String In Directory.GetFiles(Path.GetTempPath())
            If f.Contains(PGS_) then
                scrape.AddRange(File.ReadAllLines(f).ToList())
                If bool Then
                    File.Delete(f)
                End If
            End If
        Next
        Return scrape
    End Function

    Private Sub writeLog(log As string)
        SyncLock _fileLock
        File.AppendAllText(Path.GetTempPath() + "PG.LOGFILE", log + vbNewLine) 
        End SyncLock      
    End Sub
End Class